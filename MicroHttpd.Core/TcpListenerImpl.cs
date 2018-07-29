using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace MicroHttpd.Core
{
	sealed class TcpListenerImpl : ITcpListener
	{
		readonly TcpListener _tcpListener;
		readonly int _port;
		readonly string _host;

		public TcpListenerImpl(string host, int port)
		{
			if (port <= 0 || port > ushort.MaxValue)
				throw new ArgumentException($"Invalid port: {port}");
			_host = host ?? throw new ArgumentNullException(nameof(host));
			_port = port;
			_tcpListener = new TcpListener(IPAddress.Parse(host), port);
		}

		public void Start()
		{
			_tcpListener.Start();
		}

		public void Stop()
		{
			_tcpListener.Stop();
		}

		public async Task<ITcpClient> AcceptTcpClientAsync()
		{
			return new TcpClientImpl(await _tcpListener.AcceptTcpClientAsync());
		}

		public override string ToString()
		{
			return string.Format("[TcpListenerAdapter Host={0}, Port={1}]", _host, _port);
		}
	}
}
