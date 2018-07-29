namespace MicroHttpd.Core
{
	public interface IHttpRequestHeader : IHttpHeaderReadOnly
	{
		string Verb
		{ get; }

		string Uri
		{ get; }

		HttpProtocol Protocol
		{ get; }
	}
}