namespace MicroHttpd.Core
{
	static class HttpKeys
    {
		public static readonly StringCI Host = "Host";
		public static readonly StringCI ContentLength = "Content-Length";
		public static readonly StringCI TransferEncoding = "Transfer-Encoding";
		public static readonly StringCI Cookie = "Cookie";
		public static readonly StringCI Connection = "Connection";
		public static readonly StringCI KeepAliveValue = "keep-alive";
		public static readonly StringCI CloseValue = "close";
		public static readonly StringCI ChunkedValue = "chunked";
		public static readonly StringCI ContentType = "Content-Type";
	}
}
