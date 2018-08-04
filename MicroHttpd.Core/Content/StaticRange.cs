using System;
using System.Threading.Tasks;

namespace MicroHttpd.Core.Content
{
	sealed class StaticRange : IContent
	{
		readonly IStaticFileServer _staticFileServer;
		readonly TcpSettings _tcpSettings;

		// Maximum number of ranges a client can request,
		// should be a reasonable number so the server don't
		// go crazy seeking back and forth.
		const int MaxRangeCount = 32;

		/// <summary>
		/// Maximum length of range header value we accept,
		/// To prevent clients from sending extremerly large 
		/// value causing the regex to crash.
		/// </summary>
		const int MaxRangeHeaderValueLength = 1024;

		public StaticRange(IStaticFileServer staticFileServer, TcpSettings tcpSettings)
		{
			Validation.RequireValidTcpSettings(tcpSettings);
			_staticFileServer = staticFileServer 
				?? throw new ArgumentNullException(nameof(staticFileServer));
			_tcpSettings = tcpSettings;
		}

		public async Task<bool> ServeAsync(IHttpRequest request, IHttpResponse response)
		{
			// Alter the header to indicate that we accept range requests;
			response.Header["Accept-Ranges"] = "bytes";
			try
			{
				// Handle the request if client requested range
				if(IsGetOrHead(request)
					&& request.Header.ContainsKey(HttpKeys.Range)
					&& _staticFileServer.TryResolve(request, out string resolvedFile))
				{
					await ServeRangeContentAsync(request, response, resolvedFile);
					return true;
				}
				// Else, we won't be serving range content for this request,
				// leave it untouched.
				return false;
			}
			catch (StaticRangeNotSatisfiableException) {
				// Bad range requested by the client,
				// We'll handle this
				await response.Return416Async();
				return true;
			}
		}

		Task ServeRangeContentAsync(
			IHttpRequest request,
			IHttpResponse response,
			string pathToContentFile)
		{
			// Get and validate the range
			var rangeHeaderValie = request.Header[HttpKeys.Range];
			if(rangeHeaderValie.Length >= MaxRangeHeaderValueLength)
				throw new HttpPayloadTooLargeException(
					$"Range header value length reached maximum value of {MaxRangeHeaderValueLength}"
					);
			var requestedRanges = StaticRangeValueParserUtils.GetRequestedRanges(rangeHeaderValie);
			if(requestedRanges.Length == 0 || requestedRanges.Length > MaxRangeCount)
				throw new StaticRangeNotSatisfiableException();

			// Range is good, from this point forward,
			// we will be serving partial content.
			response.Header.StatusCode = 206;

			if(requestedRanges.Length == 1)
				return _staticFileServer.WriteAsync(
					request,
					requestedRanges[0], 
					response, 
					pathToContentFile, _tcpSettings.ReadWriteBufferSize
					);
			else
				return _staticFileServer.WriteAsync(
					request,
					requestedRanges, 
					response, 
					pathToContentFile, _tcpSettings.ReadWriteBufferSize
					);
		}

		static bool IsGetOrHead(IHttpRequest request)
		{
			return request.Header.Method == HttpRequestMethod.GET
				|| request.Header.Method == HttpRequestMethod.HEAD;
		}
	}
}

