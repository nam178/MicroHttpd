namespace MicroHttpd.Core
{
    static class HttpHeaderBuilderFactory
    {
		/// <summary>
		/// Create a header builder that builds HttpRequestHeader
		/// </summary>
		public static HttpHeaderBuilder<HttpRequestHeader> CreateRequestHeaderBuilder()
		{
			return new HttpHeaderBuilder<HttpRequestHeader>(
				(startLine, entries) => new HttpRequestHeader(startLine, entries)
				);
		}

		/// <summary>
		/// Create a header builder that builds HttpResponseHeader
		/// </summary>
		public static HttpHeaderBuilder<HttpResponseHeader> CreateResponseHeaderBuilder()
		{
			return new HttpHeaderBuilder<HttpResponseHeader>(
				(startLine, entries) => new HttpResponseHeader(entries)
				{
					StartLine = startLine
				});
		}
	}
}
