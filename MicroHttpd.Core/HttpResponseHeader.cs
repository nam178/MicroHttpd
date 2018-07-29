using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace MicroHttpd.Core
{
	sealed class HttpResponseHeader : HttpHeader, IHttpResponseHeader
	{
		int _statusCode;
		public int StatusCode
		{
			get { return _statusCode; }
			set
			{
				if(value == _statusCode)
					return;
				if(false == HttpCommonStatusCodes.Values.ContainsKey(value))
					throw new ArgumentException(
						$"Invalid status code: {value}, use {nameof(SetStartLine)} for setting custom status instead"
						);
				_statusCode = value;
				SetStartLine(value, HttpCommonStatusCodes.Values[_statusCode]);
			}
		}

		public bool IsWritable
		{ get; set; } = true;

		public override string StartLine
		{
			get => base.StartLine;
			set {
				if(value == null)
					throw new ArgumentNullException(nameof(value));
				var matches = Regex.Match(value, @"^([^\s]+)\s+(\d+)\s+(.*)$");
				if(false == matches.Success)
					throw new ArgumentException(
						$"Start line is in an incorrect format: {value}"
						);
				// Good
				_statusCode = Int32.Parse(
					matches.Groups[2].Value, 
					NumberStyles.Integer, 
					CultureInfo.InvariantCulture);
				base.StartLine = value;
			}
		}

		public HttpResponseHeader(HttpHeaderEntries entries = null) 
			: base(null, entries)
		{
			// Default status code to 200 (Ok)
			StatusCode = 200;
		}

		public void SetStartLine(int code, string text)
		{
			if(text == null)
				throw new ArgumentNullException(nameof(text));
			if(string.IsNullOrWhiteSpace(text))
				throw new ArgumentException(nameof(text));
			RequireWritable();
			base.StartLine = $"HTTP/1.1 {code.ToString(CultureInfo.InvariantCulture)} {text}";
		}

		public override void Add(StringCI key, string value)
		{
			RequireWritable();
			base.Add(key, value);
		}

		public override void Remove(StringCI key)
		{
			RequireWritable();
			base.Remove(key);
		}

		void RequireWritable()
		{
			if(false == IsWritable)
				throw new HttpHeaderAlreadySentException();
		}
	}
}


