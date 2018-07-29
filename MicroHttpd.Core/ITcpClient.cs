using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace MicroHttpd.Core
{
	interface ITcpClient : ITcpClientMetadata, IDisposable
	{
		bool Connected
		{ get; }

		int ReceiveTimeout
		{ get; set; }

		int SendTimeout
		{ get; set; }

		int SendBufferSize
		{ get; set; }

		int ReceiveBufferSize
		{ get; set; }

		Stream GetStream();

		Task ConnectAsync(string host, int port);
	}
}
