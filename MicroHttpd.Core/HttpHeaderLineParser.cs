using System;

namespace MicroHttpd.Core
{
	/// <summary>
	/// Helps parsing a header line to key - value
	/// </summary>
	static class HttpHeaderLineParser
    {
		/// <summary>
		/// Parse the supplied header line to key-value
		/// </summary>
		public static void Parse(string headerLine, out string key, out string value)
		{
			if(headerLine == null)
				throw new ArgumentNullException(nameof(headerLine));

			var seperatorIndex = headerLine.IndexOf(":");
			if(seperatorIndex == -1 || seperatorIndex == (headerLine.Length - 1))
				ThrowForInvalidHeaderSeperator(headerLine);
			key = headerLine.Substring(0, seperatorIndex);
			value = headerLine.Substring(seperatorIndex + 1).Trim();
		}

		static void ThrowForInvalidHeaderSeperator(string headerLine)
		{
			throw new HttpInvalidMessageException(
				$"Invalid header '{headerLine}'"
				);
		}
	}
}
