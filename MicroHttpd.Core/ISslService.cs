using System.IO;
using System.Threading.Tasks;

namespace MicroHttpd.Core
{
	interface ISslService
	{
		bool IsAvailable
		{ get; }

		Task<Stream> AddSslAsync(Stream src);
	}
}
