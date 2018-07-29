using log4net;
using System;

namespace MicroHttpd.Core
{
	/// <summary>
	/// Stateless, threadsafe.
	/// </summary>
	sealed class TcpClientConnectedEventHandler : ITcpClientConnectedEventHandler
	{
		readonly ILog _logger = LogManager.GetLogger(typeof(TcpClientConnectedEventHandler));
		readonly ITcpSessionInitializer _tcpSessionInitializer;

		public TcpClientConnectedEventHandler(
			ITcpSessionInitializer tcpSessionInitializer)
		{
			_tcpSessionInitializer = tcpSessionInitializer 
				?? throw new ArgumentNullException(nameof(tcpSessionInitializer));
		}

		public void TcpClientConnected(ITcpClient client)
		{
			if(_tcpSessionInitializer.IsLimitReached)
				Reject(client);
			else
				Accept(client);
		}

		void Accept(ITcpClient client) => _tcpSessionInitializer.InitializeNewTcpSession(client);

		void Reject(ITcpClient client)
		{
			try
			{
				client.Dispose();
			} catch (Exception) { }
			_logger.Warn($"Maximum number of clients reached");
		}
	}
}
