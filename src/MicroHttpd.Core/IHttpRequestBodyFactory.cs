namespace MicroHttpd.Core
{
	/// <summary>
	/// Based on HTTP specs, there are 2 types of Http request body:
	/// The fixed-length body, and the variable-length (chunked).
	/// 
	/// This factory takes care of their creation.
	/// </summary>
	/// <see cref="HttpChunkedRequestBody"/>
	/// <see cref="HttpFixedLengthRequestBody"/>
	interface IHttpRequestBodyFactory
    {
		ReadOnlyStream Create(
			TcpSettings tcpSettings,
			HttpRequestHeader requestHeader,
			RollbackableStream requestStream
			);
    }
}
