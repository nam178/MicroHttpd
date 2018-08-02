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

		Stream IHttpResponse.Body
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
			// GET request contains no body, nothing to do.
			// Notes: in some cases, i.e. malformed request, 
			// accessing the header raises Exception
			try
			{
				if(_request.Header.Method == HttpRequestMethod.GET)
					return;
			}
			catch(InvalidOperationException) { return; };
			
			// Get a reference to request body,
			// also be aware of InvalidOperationException due to
			// accessing of body on a malformed request.
			Stream body;
			try
			{
				body = _request.Body;
			}
			catch(InvalidOperationException) { return; }
			
			// Now read 
			var buff = new byte[_tcpSettings.ReadWriteBufferSize];
			while(true)
			{
				if(0 == await body.ReadAsync(buff, 0, buff.Length))
					break;
			}
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
