namespace MicroHttpd.Core
{
	public interface IHttpRequest
	{
		IHttpRequestHeader Header
		{ get; }

		ReadOnlyStream Body
		{ get; }
	}
}