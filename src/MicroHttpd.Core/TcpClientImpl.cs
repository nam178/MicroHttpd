﻿using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace MicroHttpd.Core
{
	sealed class TcpClientImpl : ITcpClient, IDisposable
	{
		readonly TcpClient _tcpClient;

		public bool Connected
		{ get { return _tcpClient.Connected; } }

		public int ReceiveTimeout
		{
			get { return _tcpClient.ReceiveTimeout; }
			set { _tcpClient.ReceiveTimeout = value; }
		}

		public int SendTimeout
		{
			get { return _tcpClient.SendTimeout; }
			set { _tcpClient.SendTimeout = value; }
		}

		public int ReceiveBufferSize
		{
			get { return _tcpClient.ReceiveBufferSize; }
			set { _tcpClient.ReceiveBufferSize = value; }
		}

		public int SendBufferSize
		{
			get { return _tcpClient.SendBufferSize; }
			set { _tcpClient.SendBufferSize = value; }
		}

		public IPAddress RemoteAddress
		{ get; }

		public int LocalPort
		{ get; }

		readonly string _debugName;

		public TcpClientImpl()
			: this(new TcpClient())
		{
		}

		public TcpClientImpl(TcpClient tcpClient)
		{
			_tcpClient = tcpClient 
				?? throw new ArgumentNullException(nameof(tcpClient));
			RemoteAddress = ((IPEndPoint)_tcpClient.Client.RemoteEndPoint).Address;
			LocalPort = ((IPEndPoint)_tcpClient.Client.LocalEndPoint).Port;
			_debugName = _tcpClient.Client.RemoteEndPoint.ToString();
		}

		public Stream GetStream()
			=> _tcpClient.GetStream();

		public Task ConnectAsync(string host, int port)
			=> _tcpClient.ConnectAsync(host, port);

		public override string ToString() =>  $"[TcpClient {_debugName}]";

		void IDisposable.Dispose() => _tcpClient.Close();
	}
}
