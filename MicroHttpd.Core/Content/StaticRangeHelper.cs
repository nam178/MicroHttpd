using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace MicroHttpd.Core.Content
{
	static class StaticRangeHelper
    {
		static Regex _pattern = new Regex(@"^\s*bytes\s*=(\s*(\d+)\s*\-\s*(\d+)?,?)+$");

		public static IReadOnlyList<StaticRangeRequest> GetRequestedRanges(string rangeHeaderValue)
		{
			var matches = _pattern.Match(rangeHeaderValue);
			var result = new List<StaticRangeRequest>();
			if(false == matches.Success)
				throw new ArgumentException(nameof(rangeHeaderValue));
			for(var i = 0; i < matches.Groups[1].Captures.Count; i++)
			{
				result.Add(GetRequestedRanges(matches, i));
			}
			return result;
		}

		static StaticRangeRequest GetRequestedRanges(Match match, int i)
		{
			const int GroupOneIndex = 2;
			const int GroupTwoIndex = 3;

			if(false == long.TryParse(
				match.Groups[GroupOneIndex].Captures[i].Value,
				NumberStyles.Integer,
				CultureInfo.InvariantCulture,
				out long from))
			{
				throw new ArgumentException(
						$"Invalid range: {match.Groups[GroupOneIndex].Value} provided"
						);
			}

			if(match.Groups[GroupTwoIndex].Success)
			{
				if(false == long.TryParse(
					match.Groups[GroupTwoIndex].Captures[i].Value,
					NumberStyles.Integer,
					CultureInfo.InvariantCulture,
					out long to))
				{
					throw new ArgumentException(
						$"Invalid range: {match.Groups[GroupTwoIndex].Value} provided"
						);
				}
				return new StaticRangeRequest(from, to);
			}
			else
			{
				return new StaticRangeRequest(from, long.MinValue);
			}
		}
	}
}
