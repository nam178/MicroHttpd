using System.IO;
using System.Net;

namespace MicroHttpd.Core
{
	public interface ITcpClientMetadata
	{
		IPAddress RemoteAddress
		{ get; }
	}
}