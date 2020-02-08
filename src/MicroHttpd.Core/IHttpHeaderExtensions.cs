using System;
using System.Collections.Generic;
using System.Globalization;

namespace MicroHttpd.Core
{
	public static class IHttpHeaderExtensions
    {
		public static long GetContentLength(this IHttpHeaderReadOnly header)
		{
			var contentLengths = header.Get(HttpKeys.ContentLength, false);
			RequireContentLengthEqualContentLengthValues(contentLengths);

			if(false == long.TryParse(
				contentLengths[0],
				NumberStyles.Integer,
				CultureInfo.InvariantCulture,
				out long contentLength))
			{
				ThrowForInvalidContentLengthHeaderField(contentLengths[0]);
			}

			return contentLength;
		}

		static void RequireContentLengthEqualContentLengthValues(IReadOnlyList<string> contentLengths)
		{
			if(contentLengths == null)
				throw new ArgumentNullException(nameof(contentLengths));
			if(contentLengths.Count < 1)
				throw new ArgumentException(nameof(contentLengths));
			for(var i = 1; i < contentLengths.Count; i++)
			{
				if(string.Compare(
					contentLengths[i],
					contentLengths[0],
					true,
					CultureInfo.InvariantCulture) != 0)
				{
					ThrowForMultipleContentLengthWithDifferentValue();
				}
			}
		}

		static void ThrowForInvalidContentLengthHeaderField(string actualValue)
		{
			throw new HttpBadRequestException(
				$"Invalid Content-length header field: {actualValue}"
				);
		}

		static void ThrowForMultipleContentLengthWithDifferentValue()
		{
			throw new HttpBadRequestException(
				"Multiple Content-Length header fields present with different value"
				);
		}
	}
}
