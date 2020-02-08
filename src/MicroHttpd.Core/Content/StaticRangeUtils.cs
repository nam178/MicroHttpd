using System;
using System.Globalization;
using System.Threading.Tasks;

namespace MicroHttpd.Core.Content
{
	static class StaticRangeUtils
    {
		public static string Str(this long to) 
			=> to.ToString(CultureInfo.InvariantCulture);

		public static Task Return416Async(this IHttpResponse response)
		{
			if(response.IsHeaderSent)
				throw new InvalidOperationException();
			ClearHeader(response, HttpKeys.ContentType);
			ClearHeader(response, HttpKeys.ContentLength);
			response.Header.StatusCode = 416;
			response.Header[HttpKeys.ContentLength] = "0";
			return response.SendHeaderAsync();
		}

		static void ClearHeader(this IHttpResponse response, StringCI key)
		{
			if(response.Header.ContainsKey(key))
				response.Header.Remove(key);
		}

		public static string GenerateRangeHeaderKeyAndValue(
			this StaticRangeRequest rangeRequest, 
			long length)
		{
			return $"{HttpKeys.ContentRange}: {GenerateRangeHeaderValue(rangeRequest, length)}";
		}

		public static string GenerateRangeHeaderValue(
			this StaticRangeRequest range, 
			long length)
			=> $"bytes {range.From.Str()}-{range.To.Str()}/{length.Str()}";

	}
}
