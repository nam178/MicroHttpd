namespace MicroHttpd.Core
{
    interface ITcpListenerFactory
	{
		ITcpListener Create(string listenAddress, int port);
	}
}
