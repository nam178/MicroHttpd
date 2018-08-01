using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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

		public static void RequireValidRange(this StaticRangeRequest rangeRequest)
		{
			if(rangeRequest.To < rangeRequest.From)
				throw new HttpBadRequestException($"Invalid range requested {rangeRequest}");
		}

		public static void RequireValidRangeWithin(this StaticRangeRequest rangeRequest, long contentLength)
		{
			if(rangeRequest.From < 0
				|| rangeRequest.From >= contentLength
				|| rangeRequest.To < 0
				|| rangeRequest.To >= contentLength)
			{
				throw new StaticRangeNotSatisfiableException();
			}
		}

		public static string Str(this long to) => to.ToString(CultureInfo.InvariantCulture);

		public static Task Return416Async(this IHttpResponse response)
		{
			if(response.IsHeaderSent)
				throw new InvalidOperationException();
			ClearHeader(response, HttpKeys.ContentType);
			ClearHeader(response, HttpKeys.ContentLength);
			response.Header.StatusCode = 416;
			return response.SendHeaderAsync();
		}

		public static void ClearHeader(this IHttpResponse response, StringCI key)
		{
			if(response.Header.ContainsKey(key))
				response.Header.Remove(key);
		}

		public static string GenerateRangeHeader(this StaticRangeRequest rangeRequest, long length)
		{
			return $"{HttpKeys.ContentRange}: bytes {rangeRequest.From.Str()}-{rangeRequest.To.Str()}/{length.Str()}";
		}
	}
}
