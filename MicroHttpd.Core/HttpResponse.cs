using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MicroHttpd.Core
{
	/// <summary>
	/// The response message to the client
	/// </summary>
	/// <remarks>Not thread safe</remarks>
	sealed class HttpResponse : IHttpResponseInternal, IDisposable
	{
		readonly IHttpRequest _request;
		readonly Stream _rawResponseStream;
		readonly TcpSettings _tcpSettings;

		readonly HttpResponseHeader _header;
		public IHttpResponseHeader Header
		{ get { return _header; } }

		readonly HttpResponseBody _body;
		public HttpResponseBody Body
		{ get { return _body; } }

		WriteOnlyStream IHttpResponse.Body
		{ get { return _body; } }

		public bool IsHeaderSent
		{ get; set; }
		
		public HttpResponse(
			IHttpRequest request,
			Stream rawResponseStream, 
			TcpSettings tcpSettings, 
			HttpSettings httpSettings)
		{
			Validation.RequireValidTcpSettings(tcpSettings);
			_request = request;
			_rawResponseStream = rawResponseStream 
				?? throw new ArgumentNullException(nameof(rawResponseStream));
			_tcpSettings = tcpSettings;
			_header = new HttpResponseHeader();
			_header.StatusCode = 200;
			_body = new HttpResponseBody(_rawResponseStream, 
				tcpSettings, httpSettings, this);
		}

		public async Task SendHeaderAsync()
		{
			RequireUnsentHeader();
			await EnsureRequestBodyIsReadAsync();
			await _rawResponseStream.WriteAsync(
				Encoding.ASCII.GetBytes(_header.AsPlainText),
				_tcpSettings.ReadWriteBufferSize
				);
			FlagHeaderAsSent();
		}

		async Task EnsureRequestBodyIsReadAsync()
		{
			var buff = new byte[_tcpSettings.ReadWriteBufferSize];

			// In some cases, the request body hasn't been built,
			// for example, when we receive an malformed request,
			// and need to close the connection immediately.
			//
			// If body has not been built, this throws InvalidOperationException.
			// Catch and ignore it.
			ReadOnlyStream body;
			try
			{
				body = _request.Body;
			}
			catch(InvalidOperationException)
			{ return; }
			
			while(true)
			{
				if(0 == await body.ReadAsync(buff, 0, buff.Length))
					break;
			}
		}

		public void SendHeader()
		{
			RequireUnsentHeader();
			_rawResponseStream.Write(
				Encoding.ASCII.GetBytes(_header.AsPlainText),
				_tcpSettings.ReadWriteBufferSize
				);
			FlagHeaderAsSent();
		}

		void FlagHeaderAsSent()
		{
			IsHeaderSent = true;
			_header.IsWritable = false;
		}

		void RequireUnsentHeader()
		{
			if(IsHeaderSent)
				throw new HttpHeaderAlreadySentException();
		}

		int _disposed = 0;
		public void Dispose()
		{
			if(Interlocked.CompareExchange(ref _disposed, 1, 0) == 1)
				return;
			_body.Dispose();
		}
	}
}
