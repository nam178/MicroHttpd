using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace MicroHttpd.Core
{
	sealed class HttpRequestHeader : HttpHeader, IHttpRequestHeader
	{
		HttpRequestMethod _cachedMethod;
		public HttpRequestMethod Method
		{ get => _cachedMethod; }

		string _cachedUri;
		public string Uri
		{ get => _cachedUri; }

		public override string StartLine {
			get => base.StartLine;
			set
			{
				base.StartLine = value;
				RefreshCachedValues(value);
			}
		}

		HttpProtocol _cachedProtocol = HttpProtocol.Unknown;
		public HttpProtocol Protocol
		{ get => _cachedProtocol; }

		public HttpRequestHeader(
			string startLine = null,
			HttpHeaderEntries entries = null) 
			: base(startLine, entries)
		{
			if(startLine != null)
				RefreshCachedValues(startLine);
		}

		void RefreshCachedValues(string startLine)
		{
			_cachedProtocol = GetProtocol(startLine);
			GetMethodAndUri(startLine, out _cachedMethod, out _cachedUri);
		}

		static void GetMethodAndUri(
			string startLine, 
			out HttpRequestMethod method, 
			out string uri)
		{
			if(string.IsNullOrWhiteSpace(startLine))
				throw new InvalidOperationException(
					"Start line must be set first"
					);
			var match = Regex.Match(startLine, @"^\s*([^\s]+)\s+([^\s]+)");
			if((match != null) && (match.Groups.Count == 3))
			{
				method = HttpRequestMethodHelper.FromString(match.Groups[1].Value);
				uri = match.Groups[2].Value;
				return;
			}

			throw new HttpInvalidMessageException(
				$"Invalid request line: {startLine}"
				);
		}

		static HttpProtocol GetProtocol(string startLine)
		{
			if(startLine == null)
				throw new ArgumentNullException(nameof(startLine));

			var match = Regex.Match(startLine, @"HTTP\/(\d+)\.(\d+)\s*$", RegexOptions.IgnoreCase);
			if ((match != null) && (match.Groups != null) && (match.Groups.Count == 3))
			{
				var major = match.Groups[1].Value;
				var minor = match.Groups[2].Value;

				if (Compare(major, "1") && Compare(minor, "0"))
					return HttpProtocol.Http10;
				else if (Compare(major, "1") && Compare(minor, "1"))
					return HttpProtocol.Http11;
			}

			return HttpProtocol.Unknown;
		}

		static bool Compare(string x, string y)
			=> string.Compare(x, y, true, CultureInfo.InvariantCulture) == 0;
	}
}
