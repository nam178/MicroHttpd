using System.IO;
using System.Threading.Tasks;

namespace MicroHttpd.Core
{
	public interface IHttpResponse
	{
		bool IsHeaderSent
		{ get; }

		IHttpResponseHeader Header
		{ get; }

		Stream Body
		{ get; }

		Task SendHeaderAsync();
	}
}