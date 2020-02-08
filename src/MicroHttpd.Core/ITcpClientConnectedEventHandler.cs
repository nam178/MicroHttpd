namespace MicroHttpd.Core
{
	/// <summary>
	/// An event handler, called when a TcpClient is connected.
	/// </summary>
	internal interface ITcpClientConnectedEventHandler
	{
		/// <summary>
		/// Called when the provided TCP client is connected.
		/// This instance is responsible for disposing of the TCP client.
		/// </summary>
		void TcpClientConnected(ITcpClient tcpClient);
	}
}