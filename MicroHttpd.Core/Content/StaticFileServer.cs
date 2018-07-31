using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MicroHttpd.Core.Content
{
	sealed class StaticFileServer : IStaticFileServer
    {
		readonly IReadOnlyList<IVirtualHostConfigReadOnly> _vhosts;
		readonly TcpSettings _tcpSettings;
		readonly IReadOnlyDictionary<StringCI, MimeTypeEntry> _mimeTypes;
		readonly IContentSettingsReadOnly _contentSettings;

		public StaticFileServer(
			IReadOnlyList<IVirtualHostConfigReadOnly> vhostConfig,
			TcpSettings tcpSettings, 
			IReadOnlyDictionary<StringCI, MimeTypeEntry> mimeTypes,
			IContentSettingsReadOnly contentSettings)
		{
			Validation.RequireValidTcpSettings(tcpSettings);
			_vhosts = vhostConfig 
				?? throw new ArgumentNullException(nameof(vhostConfig));
			_tcpSettings = tcpSettings;
			_mimeTypes = mimeTypes;
			_contentSettings = contentSettings 
				?? throw new ArgumentNullException(nameof(contentSettings));
		}

		public bool TryResolve(IHttpRequest request, out string resolvedPathToContentFile)
		{
			// No vhost? Early exit
			var vhost = GetVirtualHost(request);
			if(null == vhost)
				throw new HttpBadRequestException("No host matched requested host");

			// We can serve the requested URI if it points to a valid file
			return IsValidFileUri(
				vhost.DocumentRoot,
				request.Header.Uri.TrimStart('/'),
				out resolvedPathToContentFile
				);
		}

		public string GetContentTypeHeader(string pathToContentFile)
		{
			var ext = Path.GetExtension(pathToContentFile);
			return (ext != null && _mimeTypes.ContainsKey(ext))
				? (
					_mimeTypes[ext].IsText
						? $"{_mimeTypes[ext].HttpContentType}; charset={_contentSettings.DefaultCharsetForTextContents}"
						: _mimeTypes[ext].HttpContentType
					)
				: "application/octet-stream";
		}

		public Stream OpenRead(string pathToContentFile)
		{
			return new FileStream(
				pathToContentFile, FileMode.Open, FileAccess.Read, FileShare.Read, 
				_tcpSettings.ReadWriteBufferSize, true
				);
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

		IVirtualHostConfigReadOnly GetVirtualHost(IHttpRequest request)
		{
			var host = request.Header.ContainsKey(HttpKeys.Host)
				? request.Header[HttpKeys.Host]
				: string.Empty;

			if(_vhosts.Count == 0)
				throw new VirtualHostConfigException("No virtual host configured");

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
