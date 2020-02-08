namespace MicroHttpd.Core
{
	public interface IHttpRequestHeader : IHttpHeaderReadOnly
	{
		HttpRequestMethod Method
		{ get; }

		string Uri
		{ get; }

		HttpProtocol Protocol
		{ get; }
	}
}