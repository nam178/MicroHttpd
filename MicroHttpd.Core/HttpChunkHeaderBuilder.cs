using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace MicroHttpd.Core
{
	sealed class HttpChunkHeaderBuilder
    {
		readonly HttpLineBuilder _lineBuilder = new HttpLineBuilder();

		HttpChunkHeader _result;
		public HttpChunkHeader Result
		{
			get {
				RequireNonNullResult();
				return _result;
			}
		}

		public void Reset()
		{
			_lineBuilder.Reset();
			_result = null;
		}

		public bool AppendBuffer(
			byte[] buffer, 
			int start, 
			int count, 
			out int contentStartIndex)
		{
			Validation.RequireValidBuffer(buffer, start, count);
			RequireNullResult();

			// Early exit if there is no new line in the appended buffer;
			contentStartIndex = default(int);
			int nextLineStartIndex;
			if(false == _lineBuilder.AppendBuffer(buffer, start, count, out nextLineStartIndex))
				return false;

			// New line found in the supplied buffer!
			// The chunk header ends here
			contentStartIndex = nextLineStartIndex;
			_result = BuildResult(_lineBuilder.Result);
			return true;
		}

		void RequireNullResult()
		{
			if(_result != null)
				throw new InvalidOperationException(
					"Chunk header already built"
					);
		}

		void RequireNonNullResult()
		{
			if(_result == null)
				throw new InvalidOperationException(
					"Chunk header has not been built"
					);
		}

		static HttpChunkHeader BuildResult(string chunkHeaderLine)
		{
			var matches = Regex.Match(chunkHeaderLine, "^([a-fA-F0-9]+)");
			if(matches.Success && matches.Groups.Count == 2)
			{
				try
				{
					return new HttpChunkHeader(
					ParseChunkLengthFromHexString(matches.Groups[1].Value)
					);
				}
				catch(ArgumentException ex)
				{
					throw new HttpBadRequestException(
						$"Failed building chunk header: {ex.Message}", 
						ex);
				}
				
			}
			throw new HttpBadRequestException(
				$"Invalid chunk header '{chunkHeaderLine}'"
				);
		}

		static long ParseChunkLengthFromHexString(string chunkLengthHexString)
		{
			long result;
			if(false == Int64.TryParse(
				chunkLengthHexString,
				NumberStyles.HexNumber,
				CultureInfo.InvariantCulture,
				out result
				))
			{
				throw new HttpBadRequestException(
					$"Invalid chunk length hex string {chunkLengthHexString}"
					);
			}
			return result;
		}
	}
}
