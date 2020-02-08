using NLog;
using System;
using System.Linq;
using System.Threading;

namespace MicroHttpd.Core
{
	/// <summary>
	/// Accepts TCP connections
	/// </summary>
	/// <remarks>Thread safe</remarks>
	sealed class Server : IDisposable
	{
		readonly ILogger _logger = LogManager.GetCurrentClassLogger();
		readonly ITcpListenerFactory _tcpListenerFactory;
		readonly ITcpClientHandler _tcpClientConnectedEventHandler;
		readonly object _syncRoot = new object();

		const string ListenAddress = "0.0.0.0";

		public Server(
			ITcpListenerFactory tcpListenerFactory,
			ITcpClientHandler tcpClientConnectedEventHandler)
		{
			_tcpListenerFactory = tcpListenerFactory 
				?? throw new ArgumentNullException(nameof(tcpListenerFactory));
			_tcpClientConnectedEventHandler = tcpClientConnectedEventHandler 
				?? throw new ArgumentNullException(nameof(tcpClientConnectedEventHandler));
		}

		ITcpListener[] _listeners;

		public void Start(int[] ports)
		{
			if(ports == null)
				throw new ArgumentNullException(nameof(ports));
			if(ports.Length == 0)
				throw new ArgumentException("Must specify at least one port");

			lock(_syncRoot)
			{
				// State validation
				ThrowIfDisposed();
				if(_listeners != null)
					throw new InvalidOperationException(
						"Already started"
						);

				// Start the listeners
				_listeners = ports.Select(port => AcceptConnections(port)).ToArray();
			}
		}

		ITcpListener AcceptConnections(int port)
		{
			var listener = _tcpListenerFactory.Create(ListenAddress, port);
			listener.Start();
			_logger.Debug($"TCP Server started {ListenAddress}:{port}");

			AcceptConnections(
				listener,
				_tcpClientConnectedEventHandler,
				_logger);
			return listener;
		}

		async void AcceptConnections(
			ITcpListener tcpListener, 
			ITcpClientHandler eventHandler, 
			ILogger logger)
		{
			if(tcpListener == null)
				throw new ArgumentNullException(nameof(tcpListener));

			while(false == IsDisposed())
			{
				try
				{
					eventHandler.Handle(
						await tcpListener.AcceptTcpClientAsync()
						);
				}
				catch(ObjectDisposedException ex)
				{
					if(ex.ObjectName != "System.Net.Sockets.Socket")
						logger.Error(ex);
				}
				catch(Exception ex)
				{
					logger.Error(ex);
				}
			}
		}

		bool IsDisposed()
			=> Interlocked.CompareExchange(ref _dispose, 0, 0) == 1;

		void ThrowIfDisposed()
		{
			if(IsDisposed())
				throw new ObjectDisposedException(GetType().FullName);
		}

		int _dispose;
		public void Dispose()
		{
			if(Interlocked.CompareExchange(ref _dispose, 1, 0) == 1)
				return;
			
			// Get reference to all the active listeners so we can stop them.
			ITcpListener[] activeListeners;
			lock(_syncRoot)
			{
				activeListeners = _listeners;
				_listeners = null;
			}
			
			// Stop the listeners
			if(activeListeners != null)
			{
				foreach(var listener in activeListeners)
					listener.Stop();
			}
		}
	}
}
