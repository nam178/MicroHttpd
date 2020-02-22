using System.IO;
using System.Threading.Tasks;

namespace MicroHttpd.Core
{
	interface ISslService
	{
		/// <summary>
		/// If SSL is available for the specified client, return the SSL wrapped stream,
		/// otherwise NULL.
		/// </summary>
		Task<Stream> WrapSslAsync(ITcpClient client, Stream stream);
	}
}
