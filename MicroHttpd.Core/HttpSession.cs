using log4net;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MicroHttpd.Core
{
	sealed class HttpSession : IAsyncOperation
	{
		readonly Stream _connection;
		readonly TcpSettings _tcpSettings;
		readonly IContent _content;
		readonly IContentSettingsReadOnly _contentSettings;
		readonly IHttpKeepAliveService _keepAliveService;
		readonly IHttpRequestInternal _request;
		readonly IHttpResponseInternal _response;
		readonly ILog _logger = LogManager.GetLogger(typeof(HttpSession));

		public HttpSession(
			Stream connection,
			TcpSettings tcpSettings,
			IContent content,
			IContentSettingsReadOnly contentSettings,
			IHttpKeepAliveService keepAliveService,
			IHttpRequestInternal request,
			IHttpResponseInternal response)
		{
			Validation.RequireValidTcpSettings(tcpSettings);
			_tcpSettings = tcpSettings;
			_connection = connection
				?? throw new ArgumentNullException(nameof(connection));
			_content = content 
				?? throw new ArgumentNullException(nameof(content));
			_contentSettings = contentSettings 
				?? throw new ArgumentNullException(nameof(contentSettings));
			_keepAliveService = keepAliveService
				?? throw new ArgumentNullException(nameof(keepAliveService));
			_request = request
				?? throw new ArgumentNullException(nameof(request));
			_response = response
				?? throw new ArgumentNullException(nameof(response));
		}

		public async Task ExecuteAsync()
		{
			try
			{
				// Read the request header, minimum required prior processing the request;
				await _request.WaitForHttpHeaderAsync();
				_logger.Debug($"{_request.Header.Method} {_request.Header.Uri}");

				// Got the header.
				// Very likely that we just woke up from a keep-alive sleep,
				// Abort the watch dog to avoid being killed.
				if(_keepAliveService.IsRegistered(_connection))
					_keepAliveService.Deregister(_connection);

				// Process the request-response
				SetDefaultResponseHeaders();
				await ProcessRequestResponseAsync();

				// Before continuing to the next HTTP session in the connection loop,
				// the response header + body of this message must be completely written 
				// (or abort the connection),
				// otherwise it causes unexpected behaviour for the other HTTP client.
				await _response.Body.CompleteAsync();

				// If we promised the client to keep alive, do it.
				if(!StringCI.Compare(
					_response.Header[HttpKeys.Connection], HttpKeys.CloseValue))
				{
					_keepAliveService.Register(_connection);
				}
			}
			catch(HttpBadRequestException ex)
			{
				// Caused by malformed HTTP request message
				// sent by the client;
				// We'll return with an 400 BadRequest
				_logger.Warn(ex.Message);
				await TryRespondErrorAsync(
					400,
					$"<h1>Bad Request</h1><br />{ex.ToString()}");
			}
			catch(TcpException) {
				throw;
			}
			catch(Exception ex) {
				// Unexpected error caused by us,
				// We'll return with an internal error
				_logger.Error(ex);
				await TryRespondErrorAsync(
					500, 
					$"<h1>Internal Server Error</h1><br />{ex.ToString()}");
				// Let the connection loop handle this exception,
				// probably by closing the connection.
				throw;
			}
		}
		
		async Task TryRespondErrorAsync(int statusCode, string message)
		{
			try
			{
				// Header hasn't been sent,
				// we can still change the status code and 
				// write into the body.
				if(false == _response.IsHeaderSent)
				{
					_response.Header.StatusCode = statusCode;
					_response.Header[HttpKeys.Connection] = HttpKeys.CloseValue;
					// notes: no need to use DefaultCharsetForTextContents here
					_response.Header[HttpKeys.ContentType] = "text/plain; charset=utf-8";
					_response.Body.Clear();
					await _response.Body.WriteAsync(
						Encoding.UTF8.GetBytes(message),
						_tcpSettings.ReadWriteBufferSize
						);
					await _response.Body.CompleteAsync();
				}
				// Else,
				// Header already sent, we can't do anything.
			}
			catch(Exception) {
				// It's safe to catch all exceptions in here
			}
		}

		/// <summary>
		/// Read the request, process it, and write to the response.
		/// </summary>
		/// <returns></returns>
		async Task ProcessRequestResponseAsync()
		{
			if(false == await _content.ServeAsync(_request, _response))
			{
				// Content hasn't been served to the client,
				// This indicates a programming error
				throw new HttpSessionException(
					"No content has been served to the client"
					);
			}
		}

		/// <summary>
		/// Set minimum required for the response header.
		/// </summary>
		void SetDefaultResponseHeaders()
		{
			_response.Header.StatusCode = 200;
			_response.Header[HttpKeys.ContentType] 
				= $"text/html; charset={_contentSettings.DefaultCharsetForTextContents}";

			// By default, we would like to keep the connection alive.
			if(false == ShoudKeepAlive())
				_response.Header[HttpKeys.Connection] = HttpKeys.CloseValue;
			else
				_response.Header[HttpKeys.Connection] = HttpKeys.KeepAliveValue;
		}
		
		/// <summary>
		/// Based on the request header and keep-alive service state,
		/// should we keep the current connection alive?
		/// </summary>
		bool ShoudKeepAlive()
		{
			if(false == _keepAliveService.CanRegister(_connection))
				return false;

			// https://tools.ietf.org/html/rfc7230#section-6
			// If the "close" connection option is present, the connection will
			// not persist after the current response; else,
			if(_request.Header.ContainsKey(HttpKeys.Connection)
				&& StringCI.Compare(_request.Header[HttpKeys.Connection],  HttpKeys.CloseValue))
			{
				return false;
			}

			// If the received protocol is HTTP/1.1 (or later), the connection
			// will persist after the current response; else,
			if(_request.Header.Protocol == HttpProtocol.Http11)
			{
				return true;
			}

			// If the received protocol is HTTP / 1.0, the "keep-alive" connection
			// option is present, the recipient is not a proxy, and the recipient
			// wishes to honor the HTTP/ 1.0 "keep-alive" mechanism, the
			// connection will persist after the current response; otherwise,
			if(_request.Header.Protocol == HttpProtocol.Http10
				&& _request.Header.ContainsKey(HttpKeys.Connection)
				&& StringCI.Compare(_request.Header[HttpKeys.Connection], HttpKeys.KeepAliveValue))
			{
				return true;
			}

			// Else, the connection will close after the current response.
			return false;
		}
	}
}
