using log4net;
using System;
using System.IO;
using System.Threading.Tasks;

namespace MicroHttpd.Core
{
	sealed class HttpConnectionLoop : IAsyncOperation
	{
		readonly Stream _connection;
		readonly IHttpKeepAliveService _keepAliveService;
		readonly IHttpSessionFactory _httpSessionFactory;
		readonly ILog _logger = LogManager.GetLogger(typeof(HttpConnectionLoop));

		public HttpConnectionLoop(
			Stream tcpConnection,
			IHttpKeepAliveService keepAliveService,
			IHttpSessionFactory httpSessionFactory)
		{
			_connection = tcpConnection 
				?? throw new ArgumentNullException(nameof(tcpConnection));
			_keepAliveService = keepAliveService 
				?? throw new ArgumentNullException(nameof(keepAliveService));
			_httpSessionFactory = httpSessionFactory 
				?? throw new ArgumentNullException(nameof(httpSessionFactory));
		}

		public async Task ExecuteAsync()
		{
			var debugKeepAliveCount = 0;

			// HTTP connection loop:
			// We can have multiple HTTP request-responses for each
			// TCP connection, we'll keep accepting HTTP requests
			// out of the underlying connection until it is no longer kept alive.
			try
			{
				do
				{
					if(debugKeepAliveCount > 0)
						_logger.Debug($"Connection kept alive {debugKeepAliveCount} time(s)");

					// Create new http session
					var httpSession = _httpSessionFactory.Create();
					try
					{
						// Execute it
						await httpSession.ExecuteAsync();
					} finally {
						_httpSessionFactory.Destroy(httpSession);
						debugKeepAliveCount++;
					}
				}
				// Continue, as long as the connection remains in
				// the keep-alive service.
				while(_keepAliveService.IsRegistered(_connection));
			}
			finally
			{
				EnsureSessionIsRemovedFromKeepAliveService();
			}
		}

		void EnsureSessionIsRemovedFromKeepAliveService()
		{
			if(_keepAliveService.IsRegistered(_connection))
			{
				_keepAliveService.Deregister(_connection);
			}
		}
	}
}
