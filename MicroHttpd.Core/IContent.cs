using System.Threading.Tasks;

namespace MicroHttpd.Core
{
	public interface IContent
	{
		Task<bool> ServeAsync(
			IHttpRequest request, 
			IHttpResponse response
			);
	}
}