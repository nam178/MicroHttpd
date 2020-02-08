using System.Threading.Tasks;

namespace MicroHttpd.Core
{
	interface IHttpRequestInternal : IHttpRequest
	{
		/// <summary>
		/// Wait for the Http header to arrive, 
		/// so the Header property becomes available.
		/// 
		/// This method does not read the body. 
		/// It's up to the user to deal with the reading of body.
		/// </summary>
		Task WaitForHttpHeaderAsync();
	}
}