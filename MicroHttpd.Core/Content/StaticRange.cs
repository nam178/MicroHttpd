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
				if(request.Header.Method == HttpRequestMethod.GET
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
			var requestedRanges = StaticRangeUtils.GetRequestedRanges(request.Header[HttpKeys.Range]);
			if(requestedRanges.Count == 0 || requestedRanges.Count > MaxRangeCount)
				throw new StaticRangeNotSatisfiableException();

			// Range is good, from this point forward,
			// we will be serving partial content.
			response.Header.StatusCode = 206;

			if(requestedRanges.Count == 1)
				return _staticFileServer.ServeSingleRangeAsync(
					requestedRanges[0], 
					response, 
					pathToContentFile, _tcpSettings.ReadWriteBufferSize
					);
			else
				return _staticFileServer.ServeMultiRangeAsync(
					requestedRanges, 
					response, 
					pathToContentFile, _tcpSettings.ReadWriteBufferSize
					);
		}
	}
}

