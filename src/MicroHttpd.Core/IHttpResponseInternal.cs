namespace MicroHttpd.Core
{
	interface IHttpResponseInternal : IHttpResponse
	{
		new HttpResponseBody Body
		{ get; }
	}
}