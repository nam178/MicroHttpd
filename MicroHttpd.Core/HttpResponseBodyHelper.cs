using System.Globalization;

namespace MicroHttpd.Core
{
	static class HttpResponseBodyHelper
    {
		public static void AddChunkedTransferEncodingHeaderIfRequired(this IHttpResponseHeader header)
		{
			header[HttpKeys.TransferEncoding] = HttpKeys.ChunkedValue;
		}

		public static void AddContentLengthHeaderWhenRequired(
			this IHttpResponseHeader responseHeader,
			long proposedContentLength)
		{
			// Don't have to add 'Content-length' header if there is no content
			if(proposedContentLength <= 0)
				return;
			// Don't have to add 'Content-length' header if it is already added.
			if(responseHeader.ContainsKey(HttpKeys.ContentLength))
				return;

			// Add it
			responseHeader[HttpKeys.ContentLength]
				= proposedContentLength.ToString(CultureInfo.InvariantCulture);
		}

		public static bool IsBodyTypeExplicitlySpecified(this IHttpResponse response)
		{
			return response.Header.ContainsKey(HttpKeys.ContentLength)
				|| response.Header.ContainsKey(HttpKeys.TransferEncoding)
				;
		}
	}
}
