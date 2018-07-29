using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace MicroHttpd.Core
{
	sealed class HttpRequestHeader : HttpHeader, IHttpRequestHeader
	{
		string _cachedVerb = null;
		public string Verb
		{
			get
			{
				if(_cachedVerb == null)
					CacheVerbAndUri(StartLine, out _cachedVerb, out _cachedUri);
				return _cachedVerb;
			}
		}

		string _cachedUri;
		public string Uri
		{
			get
			{
				if(_cachedUri == null)
					CacheVerbAndUri(StartLine, out _cachedVerb, out _cachedUri);
				return _cachedUri;
			}
		}

		public override string StartLine {
			get => base.StartLine;
			set
			{
				base.StartLine = value;
				_protocol = GetProtocol(value);
				CacheVerbAndUri(value, out _cachedVerb, out _cachedUri);
			}
		}

		HttpProtocol _protocol = HttpProtocol.Unknown;
		public HttpProtocol Protocol
		{ get => _protocol; }

		public HttpRequestHeader(
			string startLine = null,
			HttpHeaderEntries entries = null) 
			: base(startLine, entries)
		{
			if(startLine != null)
			{
				_protocol = GetProtocol(startLine);
				CacheVerbAndUri(startLine, out _cachedVerb, out _cachedUri);
			}
		}

		static void CacheVerbAndUri(string startLine, out string _cachedVerb, out string _cachedUri)
		{
			if(string.IsNullOrWhiteSpace(startLine))
				throw new InvalidOperationException(
					"Start line must be set first"
					);
			var match = Regex.Match(startLine, @"^\s*([^\s]+)\s+([^\s]+)");
			if((match != null) && (match.Groups.Count == 3))
			{
				_cachedVerb = match.Groups[1].Value.ToUpper();
				_cachedUri = match.Groups[2].Value;
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
