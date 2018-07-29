using System;
using System.Collections.Generic;
using System.Text;

namespace MicroHttpd.Core
{
	interface ITcpListenerFactory
	{
		ITcpListener Create(string listenAddress, int port);
	}
}
