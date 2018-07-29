using System;

namespace MicroHttpd.Core
{
	sealed class TcpListenerFactory : ITcpListenerFactory
	{
		public ITcpListener Create(string listenAddress, int port)
		{
			if (listenAddress == null)
				throw new ArgumentNullException(nameof(listenAddress));
			return new TcpListenerImpl(listenAddress, port);
		}
	}
}
