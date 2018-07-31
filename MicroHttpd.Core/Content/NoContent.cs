using System;
using System.Text;
using System.Threading.Tasks;

namespace MicroHttpd.Core.Content
{
	sealed class NoContent : IContent
	{
		readonly TcpSettings _tcpSettings;
		readonly IContentSettingsReadOnly _contentSettings;

		public NoContent(TcpSettings tcpSettings, 
			IContentSettingsReadOnly contentSettings)
		{
			Validation.RequireValidTcpSettings(tcpSettings);
			_tcpSettings = tcpSettings;
			_contentSettings = contentSettings 
				?? throw new ArgumentNullException(nameof(contentSettings));
		}

		readonly byte[] _notFoundText = Encoding.UTF8.GetBytes(
			@"<h1>404 - Not Found</h1>");

		public async Task<bool> ServeAsync(
			IHttpRequest request, 
			IHttpResponse response)
		{
			response.Header[HttpKeys.ContentType] = $"text/html; charset={_contentSettings}";
			response.Header.StatusCode = 404;
			await response.Body.WriteAsync(_notFoundText, _tcpSettings.ReadWriteBufferSize);
			return true;
		}
	}
}
