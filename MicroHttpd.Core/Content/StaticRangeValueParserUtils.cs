using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace MicroHttpd.Core.Content
{
	static class StaticRangeValueParserUtils
    {
		static Regex _pattern = new Regex(@"^\s*bytes\s*=(\s*(\d*)\s*\-\s*(\d*),?)+$");

		public static StaticRangeRequest[] GetRequestedRanges(
			string rangeHeaderValue)
		{
			var matches = _pattern.Match(rangeHeaderValue);
			var result = new StaticRangeRequest[matches.Groups[1].Captures.Count];
			if(false == matches.Success)
				throw new ArgumentException(nameof(rangeHeaderValue));
			for(var i = 0; i < matches.Groups[1].Captures.Count; i++)
				result[i] = GetRequestedRanges(matches, i);
			return result;
		}

		static StaticRangeRequest GetRequestedRanges(Match match, int captureIndex)
		{
			const int lefValueGroupIndex = 2;
			const int rightValueGroupIndex = 3;

			return new StaticRangeRequest(
					ParseRangeValue(match, captureIndex, lefValueGroupIndex),
					ParseRangeValue(match, captureIndex, rightValueGroupIndex)
				);
		}

		static long ParseRangeValue(Match match, int captureIndex, int groupIndex)
		{
			var valueStr = match.Groups[groupIndex].Captures[captureIndex].Value;
			if(string.IsNullOrWhiteSpace(valueStr))
				return long.MinValue;

			if(false == long.TryParse(
				valueStr,
				NumberStyles.Integer,
				CultureInfo.InvariantCulture,
				out long valueLong))
			{
				throw new ArgumentException(
						$"Invalid range: {valueStr} provided"
						);
			}

			return valueLong;
		}

	}
}
