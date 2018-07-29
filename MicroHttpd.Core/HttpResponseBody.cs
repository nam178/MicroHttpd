using log4net;
using System;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace MicroHttpd.Core
{
	sealed class HttpResponseBody : WriteOnlyStream
	{
		readonly MemoryStream _buffer;
		readonly Stream _rawResponseStream;
		readonly HttpSettings _httpSettings;
		readonly TcpSettings _tcpSettings;
		readonly IHttpResponse _response;
		readonly ILog _logger;
		readonly static ILog _staticLogger = LogManager.GetLogger(typeof(HttpResponseBody));

		IHttpResponseEncoder _encoder;
		bool _isCompleted;

		public HttpResponseBody(
			Stream rawResponseStream,
			TcpSettings tcpSettings,
			HttpSettings httpSettings,
			IHttpResponse response) 
			: this(rawResponseStream, tcpSettings, httpSettings, response, _staticLogger)
		{

		}

		internal HttpResponseBody(
			Stream rawResponseStream, 
			TcpSettings tcpSettings,
			HttpSettings httpSettings, 
			IHttpResponse response,
			ILog logger)
		{
			Validation.RequireValidHttpSettings(httpSettings);
			Validation.RequireValidTcpSettings(tcpSettings);
			_tcpSettings = tcpSettings;
			_logger = logger
				?? throw new ArgumentNullException(nameof(logger));
			_rawResponseStream = rawResponseStream 
				?? throw new ArgumentNullException(nameof(rawResponseStream));
			_response = response
				?? throw new ArgumentNullException(nameof(response));
			_httpSettings = httpSettings;
			_buffer = new MemoryStream(httpSettings.MaxBodyChunkSize);
			_buffer.Position = 0;
		}

		#region Not Supoprted
		public override void Flush()  => throw new NotSupportedException();

		public override Task FlushAsync(CancellationToken cancellationToken)  => throw new NotSupportedException();
		#endregion

		#region Not Yet Implemented
		public override void Write(byte[] buffer, int offset, int count) => throw new NotImplementedException();

		public override void WriteByte(byte value) => throw new NotImplementedException();
		#endregion

		public override async Task WriteAsync(
			byte[] buffer, 
			int offset, 
			int count, 
			CancellationToken cancellationToken)
		{
			RequireNonCompleted();

			// Header wasn't sent,
			// We suppose to store this write into memory
			if(false == _response.IsHeaderSent)
			{
				// Is the type of body explicitly specified in header?
				// (Content-length or Transfer-Encoding)
				// Or,
				// Will this write overflows the internal buffer?
				// If so, send the header.
				if(WillInternalBufferFull(additionalBytes: count) 
					|| _response.IsBodyTypeExplicitlySpecified())
				{
					await SendHeaderAndCreateEncoderAsync();
					// Dump this write request into it.
					await _encoder.AppendAsync(buffer, offset, count);
				}
				// Otherwise, all writes go to the internal buffer
				else
					_buffer.Write(buffer, offset, count);
			}
			// Header was sent, 
			// We direct all writes to the current encoder.
			else
			{
				// If we don't have an encoder, 
				// which means the header was sent by someone else,
				// we should have no data held in memory
				// (if we do, encoder was created already),
				// so just create an encoder and use it.
				if(null == _encoder)
					_encoder = GetEncoderImpl(_rawResponseStream);

				await _encoder.AppendAsync(buffer, offset, count);
			}
		}

		/// <summary>
		/// User of this instance should call this method to indicate
		/// they have finished sending data.
		/// 
		/// The async version of this method is preferred over the non-async one,
		/// because it uses async IO under the hood.
		/// </summary>
		public async Task CompleteAsync()
		{
			// Already completed? 
			if(_isCompleted)
				return;
			_isCompleted = true;

			// Send the header if it was not sent
			if(false == _response.IsHeaderSent)
				await SendHeaderAndCreateEncoderAsync();

			// Complete encoding
			if(_encoder != null)
				await _encoder.CompleteAsync();
		}

		/// <summary>
		/// If header is not sent, 
		/// this method allows caller to clear contents written into this 
		/// instance to start writting them from scratch.
		/// 
		/// Otherwise, this throws InvalidOperationException.
		/// </summary>
		internal void Clear()
		{
			if(_response.IsHeaderSent)
				throw new InvalidOperationException();
			_buffer.SetLength(0);
		}

		async Task SendHeaderAndCreateEncoderAsync()
		{
			if(_response.IsHeaderSent)
				throw new InvalidOperationException();
			// First, crete the encoder
			_encoder = GetEncoderImpl(_rawResponseStream);

			// Flush headers.
			// This need tobe done before any data written into the encoder,
			// and after the encoder is created.
			await _response.SendHeaderAsync();

			// Transfer bytes held in memory to the encoder
			await _encoder.AppendAsync(
				_buffer.GetBuffer(), 0, (int)_buffer.Length,
				_tcpSettings.ReadWriteBufferSize);
		}

		IHttpResponseEncoder GetEncoderImpl(Stream target)
		{
			IHttpResponseEncoder encoder;

			// If we can't determine the content length, use the chunked encoder;
			if(false == TryGetProposedContentLength(out long proposedContentLength))
			{
				encoder = new HttpChunkedResponseEncoder(_rawResponseStream,
					_tcpSettings, _httpSettings);
				_response.Header.AddChunkedTransferEncodingHeaderIfRequired();
			}
			// Otherwise, use the passthrough encoder 
			// with fixed length
			else
			{
				RequireValidContentLength(proposedContentLength);
				_response.Header.AddContentLengthHeaderWhenRequired(proposedContentLength);
				encoder = new HttpPassthroughResponseEncoder(_rawResponseStream, 
					proposedContentLength);
			}
			return encoder;
		}

		bool WillInternalBufferFull(int additionalBytes)
			=> (_buffer.Position + additionalBytes) >= _httpSettings.MaxBodySizeInMemory;

		void RequireValidContentLength(long proposedContentLength)
		{
			Validation.RequireNonNegative(proposedContentLength);

			// Proposed content length is less than what was writen?
			if(proposedContentLength < _buffer.Length)
			{
				throw new InvalidOperationException(
					"The message body contains more data " +
					"than what specified in the " +
					"Content-Length header field"
					);
			}
		}

		void RequireNonCompleted()
		{
			if(_isCompleted)
				throw new InvalidOperationException(
					"Already finished writing into message body"
					);
		}

		int _disposed;
		protected override void Dispose(bool disposing)
		{
			// Already disposed?
			if(Interlocked.CompareExchange(ref _disposed, 1, 0) == 1)
				return;

			// Ensure the Complete() method always called prior 
			// disposing this instance.
			try
			{
				Complete();
			}
			catch(TcpException) {
				// It's pretty normal to get TcpException 
				// when closing the body, i.e. when the user closes the browser,
				// causing the whole session to drop.
			}
			catch(Exception ex)
			{
				// Disposer should never throw exception,
				// so we just log it at most.
				_logger.Error(ex.Message, ex);
			}
			base.Dispose(disposing);
		}

		void Complete()
		{
			if(_isCompleted)
				return;
			try
			{
				CompleteAsync().Wait();
			}
			catch(AggregateException ex)
			{
				if(ex.InnerException != null)
					ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
				throw;
			}
		}

		bool TryGetProposedContentLength(out long contentLength)
		{
			// If Transfer-Encoding: chunked is specified in header,
			// we will be using the chunked encoder.
			if(_response.Header.ContainsKey(HttpKeys.TransferEncoding))
			{
				var encodings = _response.Header.Get(HttpKeys.TransferEncoding, false);
				if(encodings.Count == 1
					&& string.Compare(
						encodings[0], HttpKeys
						.ChunkedValue, 
						StringComparison.InvariantCultureIgnoreCase) == 0)
				{
					contentLength = default(long);
					return false;
				}
				else throw new NotSupportedException("Only single chunked Transfer-Encoding is supported");
			}

			// Here, no Transfer-Encoding is specified,
			// If Content-Length is also not specified:
			//
			// 1. We will use chunked encoding (returning false) 
			// if we haven't completed sending data, since we don't know when the data ends.
			//
			// 2. We will use fixed length body (pass-through encoding, returning true)
			// if we have completed sending data, since we knew the length.
			if(false == _response.Header.ContainsKey(HttpKeys.ContentLength))
			{
				if(_isCompleted)
				{
					contentLength = _buffer.Length;
					return true;
				} else {
					contentLength = default(long);
					return false;
				}
			}

			// Here, Content-Length is specified.
			// We will use fixed length;
			contentLength = _response.Header.GetContentLength();
			return true;
		}
	}
}
