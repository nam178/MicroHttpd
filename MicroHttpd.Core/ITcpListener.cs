using System.Threading.Tasks;

namespace MicroHttpd.Core
{
	interface ITcpListener
	{
		void Start();

		void Stop();

		Task<ITcpClient> AcceptTcpClientAsync();
	}
}
