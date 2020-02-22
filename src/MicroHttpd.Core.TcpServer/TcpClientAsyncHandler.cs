using MicroHttpd.Core.TcpServer;
using NLog;
using System;
using System.Threading.Tasks;

namespace MicroHttpd.Core
{
	/// <summary>
	/// Asynchronous handles tcp client and responsible for disposing it.
	/// </summary>
	sealed class TcpClientAsyncHandler : ITcpClientHandler
    {
		readonly ILogger _logger = LogManager.GetCurrentClassLogger();
		readonly ITcpClientCounter _tcpClientCounter;
		readonly ISslService _sslService;
		readonly ITcpSessionFactory _tcpSessionFactory;
		readonly IWatchDog _watchDog;
		readonly TcpSettings _tcpSettings;

		public TcpClientAsyncHandler(
			ITcpClientCounter tcpClientCounter,
			ISslService sslService,
			ITcpSessionFactory tcpSessionFactory,
			IWatchDog tcpWatchDog,
			TcpSettings tcpSettings)
		{
			TcpSettings.Validate(tcpSettings);
			_tcpClientCounter = tcpClientCounter 
				?? throw new ArgumentNullException(nameof(tcpClientCounter));
			_sslService = sslService 
				?? throw new ArgumentNullException(nameof(sslService));
			_tcpSessionFactory = tcpSessionFactory 
				?? throw new ArgumentNullException(nameof(tcpSessionFactory));
			_watchDog = tcpWatchDog
				?? throw new ArgumentNullException(nameof(tcpWatchDog));
			_watchDog.MaxSessionDuration = tcpSettings.IdleTimeout;
			_tcpSettings = tcpSettings;
		}

		public async void Handle(ITcpClient client)
		{
			_tcpClientCounter.Increase();
			_logger.Debug($"TCP client connected: {client}, total clients: {_tcpClientCounter.Count}");
			try
			{
				await HandlAsync(client);
			}
			catch(TcpException ex)
			{
				_logger.Warn(ex.Message);
			}
			catch(Exception ex)
			{
				_logger.Error(ex);
			}
			finally
			{
				client.Dispose();
				_tcpClientCounter.Decrease();
				_logger.Debug($"TCP client disconnected: {client}, total clients: {_tcpClientCounter.Count}");
			}
		}

		async Task HandlAsync(ITcpClient client)
		{
			using var watchDogSession = _watchDog.Watch(client);
			ApplyTcpSettings(client, _tcpSettings);

			var stream = client.GetStream();

			// Wrap with SSL, if required.
			var t = await _sslService.WrapSslAsync(client, stream);
			if(t != null)
				stream = t;

			// Get the raw tcp stream.
			// For better exception handling and tracking of idle session,
			// we wrap it with our custom TcpStream.
			stream = new TcpExceptionStreamDecorator(new WatchDogStreamDecorator(stream, watchDogSession));

			// Create a TCP session and execute it
			var session = _tcpSessionFactory.Create(client, stream);
			try
			{
				await session.ExecuteAsync();
			}
			finally
			{
				_tcpSessionFactory.Destroy(session);
			}
		}

		static void ApplyTcpSettings(ITcpClient client, TcpSettings tcpSettings)
		{
			var tcpStream = client.GetStream();
			tcpStream.ReadTimeout = (int)tcpSettings.IdleTimeout.TotalMilliseconds;
			tcpStream.WriteTimeout = (int)tcpSettings.IdleTimeout.TotalMilliseconds;

			client.ReceiveTimeout = (int)tcpSettings.IdleTimeout.TotalMilliseconds;
			client.SendTimeout = (int)tcpSettings.IdleTimeout.TotalMilliseconds;
			client.SendBufferSize = tcpSettings.ReadWriteBufferSize;
			client.ReceiveBufferSize = tcpSettings.ReadWriteBufferSize;
		}
	}
}
