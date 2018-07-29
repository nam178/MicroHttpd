using System.Threading.Tasks;

namespace MicroHttpd.Core
{
	public interface IHttpResponse
	{
		bool IsHeaderSent
		{ get; }

		IHttpResponseHeader Header
		{ get; }

		WriteOnlyStream Body
		{ get; }

		Task SendHeaderAsync();

		void SendHeader();
	}
}