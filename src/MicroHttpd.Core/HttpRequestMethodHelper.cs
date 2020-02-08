using System;

namespace MicroHttpd.Core
{
	public static class HttpRequestMethodHelper
    {
		public static HttpRequestMethod FromString(string value)
		{
			if(string.IsNullOrWhiteSpace(value))
				throw new System.ArgumentException("message", nameof(value));

			if(Compare(value, "GET"))
				return HttpRequestMethod.GET;
			if(Compare(value, "POST"))
				return HttpRequestMethod.POST;
			if(Compare(value, "PUT"))
				return HttpRequestMethod.PUT;
			if(Compare(value, "DELETE"))
				return HttpRequestMethod.DELETE;
			if(Compare(value, "TRACE"))
				return HttpRequestMethod.TRACE;
			if(Compare(value, "HEAD"))
				return HttpRequestMethod.HEAD;
			if(Compare(value, "OPTIONS"))
				return HttpRequestMethod.OPTIONS;
			if(Compare(value, "PATH"))
				return HttpRequestMethod.PATH;
			if(Compare(value, "CONNECT"))
				return HttpRequestMethod.CONNECT;
			throw new ArgumentOutOfRangeException();
		}

		static bool Compare(string x, string y)
		{
			return string.Compare(x, y, StringComparison.InvariantCultureIgnoreCase) == 0;
		}
	}
}
