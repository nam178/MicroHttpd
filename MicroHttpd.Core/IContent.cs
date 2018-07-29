using System.Threading.Tasks;

namespace MicroHttpd.Core
{
	public interface IContent
	{
		Task<bool> WriteContentAsync(
			IHttpRequest request, 
			IHttpResponse response
			);
	}
}