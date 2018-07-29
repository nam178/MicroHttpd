using log4net;
using System;
using System.IO;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace MicroHttpd.Core
{
	/// <summary>
	/// Designed to be a global singleton, threadsafe.
	/// </summary>
	sealed class TcpSessionInitializer : ITcpSessionInitializer, IDisposable
    {
		readonly ILog _logger = LogManager.GetLogger(typeof(TcpSessionInitializer));
		readonly ISslService _sslService;
		readonly ITcpSessionFactory _tcpSessionFactory;
		readonly IWatchDog _tcpWatchDog;
		readonly TcpSettings _tcpSettings;

		/// <summary>
		/// Number of active TCP sessions, includes those sleeing keep-alive HTTP.
		/// </summary>
		int _concurrentTcpSessionCount;

		public bool IsLimitReached
		{
			get => Interlocked.CompareExchange(
				ref _concurrentTcpSessionCount, 0, 0)  >= _tcpSettings.MaxTcpClients;
		}

		public TcpSessionInitializer(
			ISslService sslService,
			ITcpSessionFactory tcpSessionFactory,
			IWatchDog tcpWatchDog,
			TcpSettings tcpSettings)
		{
			Validation.RequireValidTcpSettings(tcpSettings);
			_sslService = sslService 
				?? throw new ArgumentNullException(nameof(sslService));
			_tcpSessionFactory = tcpSessionFactory 
				?? throw new ArgumentNullException(nameof(tcpSessionFactory));
			_tcpWatchDog = tcpWatchDog
				?? throw new ArgumentNullException(nameof(tcpWatchDog));
			_tcpWatchDog.MaxSessionDuration = tcpSettings.IdleTimeout;
			_tcpSettings = tcpSettings;
		}

		public async void InitializeNewTcpSession(ITcpClient client)
		{
			// Logging
			Interlocked.Increment(ref _concurrentTcpSessionCount);
			_logger.Debug(
				$"Tcp client connected: {client}, total clients: {CountTotalClients()}"
				);
			try
			{
				await Impl(client);
			}
			catch(TcpException ex) {
				_logger.Warn(ex.Message);
			}
			catch(Exception ex) {
				_logger.Error(ex);
			}
			finally {
				Interlocked.Decrement(ref _concurrentTcpSessionCount);
				// Logging
				_logger.Debug(
					$"Tcp client disconnected: {client}, total clients: {CountTotalClients()}"
					);
			}
		}

		async Task Impl(ITcpClient client)
		{
			using(client)
			using(var networkActivityWatchDog = _tcpWatchDog.Watch(client))
			{
				ApplyTcpSettings(client, _tcpSettings);

				// Get the raw tcp stream.
				// For better exception handling and tracking of idle session,
				// we wrap it with our custom TcpStream.
				var stream = (Stream)new TcpExceptionStreamDecorator(
					new WatchDogStreamDecorator(client.GetStream(), networkActivityWatchDog));

				// Wrap again with SSL, if required.
				if(_sslService.IsAvailable)
					stream = await _sslService.AddSslAsync(stream);

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
		}

		int CountTotalClients() => Interlocked.CompareExchange(ref _concurrentTcpSessionCount, 0, 0);

		static void ApplyTcpSettings(ITcpClient client, TcpSettings tcpSettings)
		{
			client.ReceiveTimeout = (int)tcpSettings.IdleTimeout.TotalMilliseconds;
			client.SendTimeout = (int)tcpSettings.IdleTimeout.TotalMilliseconds;
			client.GetStream().ReadTimeout = (int)tcpSettings.IdleTimeout.TotalMilliseconds;
			client.GetStream().WriteTimeout = (int)tcpSettings.IdleTimeout.TotalMilliseconds;
			client.SendBufferSize = tcpSettings.ReadWriteBufferSize;
			client.ReceiveBufferSize = tcpSettings.ReadWriteBufferSize;
		}
		
		int _disposed;
		public void Dispose()
		{
			if(Interlocked.CompareExchange(ref _disposed, 1, 0) == 0)
			{
				_tcpWatchDog.Dispose();
			}
		}
	}
}
