using System.IO;

namespace MicroHttpd.Core
{
	interface ITcpSessionFactory
    {
		/// <summary>
		/// Create new TCP session over the supplied TCP client and stream
		/// </summary>
		IAsyncOperation Create(ITcpClient client, Stream connection);

		/// <summary>
		/// Destroy and cleanup resources used by the supplied 
		/// TCP session which this factory creates.
		/// </summary>
		void Destroy(IAsyncOperation tcpSession);
    }
}
