using System.IO;

namespace MicroHttpd.Core
{
	public interface IHttpRequest
	{
		IHttpRequestHeader Header
		{ get; }

		Stream Body
		{ get; }
	}
}