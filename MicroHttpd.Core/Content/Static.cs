using log4net;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MicroHttpd.Core.Content
{
	sealed class Static : IContent
	{
		readonly ILog _logger = LogManager.GetLogger(typeof(Static));
		readonly IStaticFileServer _fileServer;
		readonly TcpSettings _tcpSettings;

		public Static(IStaticFileServer fileServer, TcpSettings tcpSettings)
		{
			Validation.RequireValidTcpSettings(tcpSettings);
			_fileServer = fileServer 
				?? throw new ArgumentNullException(nameof(fileServer));
			_tcpSettings = tcpSettings;
		}

		public async Task<bool> ServeAsync(
			IHttpRequest request,
			IHttpResponse response)
		{
			// Early exit for non GET/HEAD request
			if(request.Header.Method != HttpRequestMethod.GET
				&& request.Header.Method != HttpRequestMethod.HEAD)
			{
				return false;
			}

			// Early exit if the request is not for a file
			if(false == _fileServer.TryResolve(request, out string resolvedPathToFile))
				return false;

			// Now write the header and contents
			response.Header[HttpKeys.ContentType] = 
				_fileServer.GetContentTypeHeader(resolvedPathToFile);
			using(var fs = _fileServer.OpenRead(resolvedPathToFile))
			{
				response.Header[HttpKeys.ContentLength] = fs.Length.ToString(CultureInfo.InvariantCulture);

				// Write body (only for GET request)
				if(request.Header.Method == HttpRequestMethod.GET)
				{
					await fs.CopyToAsync(response.Body, _tcpSettings.ReadWriteBufferSize);
				}
				// For HEAD request, we are not sending the body,
				// Immediatly flush the header to prevent Content-length 
				// validation on body upon completion of this request.
				else await response.SendHeaderAsync();
			}

			// Return true, as we have served the content
			return true;
		}


	
	}
}
