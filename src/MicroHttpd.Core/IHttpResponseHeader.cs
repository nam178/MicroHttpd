namespace MicroHttpd.Core
{
	public interface IHttpResponseHeader : IHttpHeader
	{
		int StatusCode
		{ get; set; }

		void SetStartLine(int code, string text);
	}
}