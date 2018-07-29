using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MicroHttpd.Core
{
	/// <summary>
	/// Represents the Http request message comes from the client.
	/// </summary>
	/// <remarks>Not thread safe.</remarks>
	sealed class HttpRequest : IHttpRequest, IHttpRequestInternal, IDisposable
	{
		readonly RollbackableStream _requestStream;
		readonly TcpSettings _tcpSettings;
		readonly IHttpRequestBodyFactory _requestBodyFactory;

		HttpRequestHeader _requestHeader;
		public IHttpRequestHeader Header
		{
			get {
				RequireNonDispose();
				RequireNonNullHeader();
				return _requestHeader;
			}
		}

		ReadOnlyStream _requestBody;
		public ReadOnlyStream Body
		{
			get {
				RequireNonDispose();
				RequireNonNullBody();
				return _requestBody;
			}
		}

		public HttpRequest(
			Stream requestStream, 
			TcpSettings tcpSettings, 
			IHttpRequestBodyFactory requestBodyFactory)
		{
			Validation.RequireValidTcpSettings(tcpSettings);
			_requestStream = new RollbackableStream(
				requestStream ?? throw new ArgumentNullException(nameof(requestStream)), 
				tcpSettings);
			_tcpSettings = tcpSettings;
			_requestBodyFactory = requestBodyFactory 
				?? throw new ArgumentNullException(nameof(requestBodyFactory));
		}

		/// <summary>
		/// Before this HttpRequest becomes available to use, 
		/// it must first, receive the header, and then construct the message body;
		/// </summary>
		public async Task WaitForHttpHeaderAsync()
		{
			// Receive the header first
			await ReceiveHeaderAsync();
			Debug.Assert(_requestHeader != null);

			// Now we have the header, which contians
			// the metadata required to construct the body.
			_requestBody = _requestBodyFactory.Create(
				_tcpSettings,
				_requestHeader,
				_requestStream);
			Debug.Assert(_requestBody != null);
		}

		async Task ReceiveHeaderAsync()
		{
			RequireNullHeader();

			var headerBuilder = HttpHeaderBuilderFactory.CreateRequestHeaderBuilder();
			var buffer = new byte[_tcpSettings.ReadWriteBufferSize];

			// Keep reading socket until header received
			while(null == _requestHeader)
			{
				var bytesRead = await _requestStream.ReadAsync(buffer, 0, 
					buffer.Length);
				HttpPrematureFinishException.ThrowIfZero(bytesRead);

				int bodyStartIndex;
				if(headerBuilder.AppendBuffer(buffer, 0, bytesRead, 
					out bodyStartIndex))
				{
					// Done!
					// Set the header
					_requestHeader = headerBuilder.Result;

					// The remaining bytes of the buffer, those belongs to the body.
					_requestStream.TryRollbackFromIndex(buffer, 
						srcLength: bytesRead, startIndex: bodyStartIndex);
				}
			}
		}

		void RequireNullHeader()
		{
			if(_requestHeader != null)
				throw new InvalidOperationException(
					"Header already received");
		}

		void RequireNonNullHeader()
		{
			if(_requestHeader == null)
				throw new InvalidOperationException(
					"Must receive header first"
					);
		}

		void RequireNonNullBody()
		{
			if(_requestBody == null)
				throw new InvalidOperationException(
					"Body has not been built"
					);
		}

		void RequireNonDispose()
		{
			if(Interlocked.CompareExchange(ref _disposed, 0, 0) != 0)
				throw new ObjectDisposedException(GetType().FullName);
		}

		int _disposed;
		void IDisposable.Dispose()
		{
			if(Interlocked.CompareExchange(ref _disposed, 1, 0) == 0)
			{
				_requestBody?.Dispose();
			}
		}
	}
}
