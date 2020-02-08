using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace MicroHttpd.Core.Tests
{
	public class TcpServerTests
    {
		[Fact]
		public void StartThenAcceptConnectionsThenStop()
		{
			var notifyTcpClientConnected443 = new AutoResetEvent(false);
			var notifyTcpClientConnected8443 = new AutoResetEvent(false);
			var mockTcpListenerFactory = new Mock<ITcpListenerFactory>();
			mockTcpListenerFactory
				.Setup(inst => inst.Create(It.IsAny<string>(), 443))
				.Returns<string, int>((addr, port) => MockUpTcpClient(port, notifyTcpClientConnected443));
			mockTcpListenerFactory
				.Setup(inst => inst.Create(It.IsAny<string>(), 8443))
				.Returns<string, int>((addr, port) => MockUpTcpClient(port, notifyTcpClientConnected8443));
			var mockHandler = new Mock<ITcpClientHandler>();

			// Create and start the server
			var server = new Server(
				mockTcpListenerFactory.Object,
				mockHandler.Object
				);
			server.Start(new int[] { 8443, 443 });

			// Wait until we served minimum 10 clients on port 443 and 8443
			WaitForClients(notifyTcpClientConnected443, 10);
			WaitForClients(notifyTcpClientConnected8443, 10);

			// Verify that we got at least 9 connections on port 8443 and 443
			// (The last one may not handled yet)
			VerifyConnectionsOnPort(mockHandler, 8443, Times.AtLeast(9));
			VerifyConnectionsOnPort(mockHandler, 443, Times.AtLeast(9));

			// Now stop the server.
			server.Dispose();

			// Now, we should no longer receive connections on both port
			mockHandler.ResetCalls();
			// Wait to see how many clients keep connecting after the server is stopped.
			// There should be at most one on each port.
			WaitForClients(notifyTcpClientConnected443, 10);
			WaitForClients(notifyTcpClientConnected8443, 10);
			VerifyConnectionsOnPort(mockHandler, 8443, Times.AtMostOnce());
			VerifyConnectionsOnPort(mockHandler, 443, Times.AtMostOnce());
		}

		static void WaitForClients(AutoResetEvent handle, int count)
		{
			for(var i = 0; i < count; i++)
			{
				handle.WaitOne(ApproxTimeForEachClient() * 2);
			}
		}

		static TimeSpan ApproxTimeForEachClient() => TimeSpan.FromMilliseconds(100);

		static ITcpListener MockUpTcpClient(int port, AutoResetEvent notifyTcpClientConnected)
		{
			var tcpListener = new Mock<ITcpListener>();
			var isStarted = false;
			var wh = new ManualResetEvent(false);
			tcpListener.Setup(x => x.Start()).Callback(() => isStarted = true);
			tcpListener.Setup(x => x.Stop()).Callback(() =>
			{
				isStarted = false;
				wh.Set();
			});
			tcpListener
				.Setup(x => x.AcceptTcpClientAsync())
				.Returns(() => Task.Run(delegate
				{
					if(false == isStarted)
						throw new InvalidOperationException();
					// Pretend to wait on network until stopped
					wh.WaitOne(ApproxTimeForEachClient());

					// Woke up, Im I disposed?
					if(false == isStarted)
						throw new ObjectDisposedException("System.Net.Sockets.Socket");

					// Notify our test there was a client connected
					notifyTcpClientConnected.Set();

					// All good, pretending a tcpClient connected 
					var tcpClient = new Mock<ITcpClient>();
					tcpClient.Name = $"ConnectedOnPort:{port}";
					return tcpClient.Object;
				}));

			return tcpListener.Object;
		}

		static void VerifyConnectionsOnPort(
			Mock<ITcpClientHandler> mockHandler, 
			int port, 
			Times times)
		{
			mockHandler
				.Verify(inst => inst.Handle(
					It.Is<ITcpClient>(x => Mock.Get<ITcpClient>(x).Name == $"ConnectedOnPort:{port}")),
					times
					);
		}
	}
}
