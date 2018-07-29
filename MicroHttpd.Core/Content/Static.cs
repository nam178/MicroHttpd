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
		readonly IReadOnlyList<IVirtualHostConfigReadOnly> _vhosts;
		readonly IContentSettingsReadOnly _contentSettings;
		readonly IReadOnlyDictionary<StringCI, MimeTypeEntry> _mimeTypes;
		readonly TcpSettings _tcpSettings;
		readonly ILog _logger = LogManager.GetLogger(typeof(Static));

		public Static(
			IReadOnlyList<IVirtualHostConfigReadOnly> vhostConfig, 
			IContentSettingsReadOnly contentSettings,
			IReadOnlyDictionary<StringCI, MimeTypeEntry> mimeTypes,
			TcpSettings tcpSettings)
		{
			Validation.RequireValidTcpSettings(tcpSettings);
			_vhosts = vhostConfig 
				?? throw new ArgumentNullException(nameof(vhostConfig));
			_contentSettings = contentSettings 
				?? throw new ArgumentNullException(nameof(contentSettings));
			_mimeTypes = mimeTypes;
			_tcpSettings = tcpSettings;
		}

		public async Task<bool> WriteContentAsync(
			IHttpRequest request,
			IHttpResponse response)
		{
			// Early exit for non GET/HEAD request
			if(request.Header.Method != HttpRequestMethod.GET
				&& request.Header.Method != HttpRequestMethod.HEAD)
			{
				return false;
			}

			// No vhost? Early exit after returning 404 Not Found
			var vhost = GetVirtualHost(request);
			if(null == vhost)
			{
				await Write404NotFoundAsync(response);
				return true;
			}

			// Not a valid path? 
			if(false == IsValidFileUri(vhost.DocumentRoot, 
				request.Header.Uri.TrimStart('/'),
				out string resolvedPath))
			{
				return false;
			}

			// Valid path to a file.
			await WriteFileContentAsync(request, response, 
				resolvedPath, _contentSettings.DefaultCharsetForTextContents);
			return true;
		}

		async Task WriteFileContentAsync(
			IHttpRequest request, 
			IHttpResponse response, 
			string absolutePathToFile,
			string defaultCharSetForTextContent)
		{
			// First, set response content-type header
			var ext = Path.GetExtension(absolutePathToFile);
			response.Header[HttpKeys.ContentType] = 
				(ext != null && _mimeTypes.ContainsKey((StringCI)ext))
				? (
					_mimeTypes[ext].IsText 
						? $"{_mimeTypes[ext].HttpContentType}; charset={defaultCharSetForTextContent}" 
						: _mimeTypes[ext].HttpContentType
					)
				: "application/octet-stream";

			// Open the file in async mode, ready to stream it.
			using(var fs = new FileStream(absolutePathToFile, FileMode.Open,
				FileAccess.Read, FileShare.Read, _tcpSettings.ReadWriteBufferSize, true))
			{
				// Then, set content-length before writing into the body.
				// This prevents the body from sending contents as chunked,
				// though improving performance.
				response.Header[HttpKeys.ContentLength] = fs.Length.ToString(
					CultureInfo.InvariantCulture);

				// Then, write the body, only for GET request
				if(request.Header.Method == HttpRequestMethod.GET)
					await fs.CopyToAsync(response.Body, _tcpSettings.ReadWriteBufferSize);
				// For HEAD request, we are not sending the body.
				else
				{
					// Flush the header.
					await response.SendHeaderAsync();
				}
			}
		}

		static bool IsValidFileUri(
			string documentRoot, 
			string path,
			out string resolvedPath)
		{
			resolvedPath = null;

			// We don't accept absolute path,
			// i.e. GET /my.jpeg is valid, but not GET C:\my.jpeg
			if(Path.IsPathRooted(path))
				return false;

			// We don't accept path outside of the document root folder,
			// i.e. GET /hack/../../my.jpeg
			var fullPath = Path.GetFullPath(Path.Combine(documentRoot, path));
			if(false == fullPath.StartsWith(documentRoot, StringComparison.InvariantCulture))
				return false;

			// We don't accept files that don't exist
			if(false == File.Exists(fullPath))
				return false;

			resolvedPath = fullPath;
			return true;
		}

		Task Write404NotFoundAsync(IHttpResponse response)
		{
			response.Header.StatusCode = 404;
			return response.Body.WriteAsync(
				Encoding.UTF8.GetBytes("Host not found"),
				_tcpSettings.ReadWriteBufferSize
				);
		}

		IVirtualHostConfigReadOnly GetVirtualHost(IHttpRequest request)
		{
			var host = request.Header.ContainsKey(HttpKeys.Host)
				? request.Header[HttpKeys.Host]
				: string.Empty;

			for(var i = 0; i < _vhosts.Count; i++)
			{
				if(_vhosts[i].HostName.IsMatch(host))
				{
					// Found a vHost!!
					return _vhosts[i];
				}
			}

			// No vhost found!
			return null;
		}
	}
}
