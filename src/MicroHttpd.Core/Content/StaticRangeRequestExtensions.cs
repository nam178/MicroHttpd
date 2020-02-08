using System;

namespace MicroHttpd.Core.Content
{
	static class StaticRangeRequestExtensions
    {
		public static StaticRangeRequest ToAbsolute(
			this StaticRangeRequest original, 
			long contentLength)
		{
			long fromByte;
			long toByte;

			if(contentLength < 0)
				throw new ArgumentException(nameof(contentLength));
			if(original.From == long.MinValue && original.To == long.MinValue)
				throw new HttpBadRequestException(
					"Must specify either 'from' or 'to' value of the range"
					);
			else if(original.From == long.MinValue)
				TreatAsLastXBytes(original, contentLength, out fromByte, out toByte);
			else if(original.To == long.MinValue)
			{
				fromByte = original.From;
				toByte = contentLength - 1;
			} else {
				fromByte = original.From;
				toByte = original.To;
			}

			Validate(fromByte, toByte, contentLength);

			return new StaticRangeRequest(fromByte, toByte);
		}
		
		static void TreatAsLastXBytes(
			StaticRangeRequest original, 
			long contentLength, 
			out long fromByte, 
			out long toByte)
		{
			var numLastBytes = original.To;
			if(numLastBytes < 0)
				throw new HttpBadRequestException();
			if(numLastBytes == 0)
				throw new StaticRangeNotSatisfiableException(
					"Last number of bytes must be larger than 0"
					);
			fromByte = contentLength - numLastBytes;
			toByte = contentLength - 1;
		}

		static void Validate(
			long from,
			long to,
			long contentLength)
		{
			if(from < 0
				|| from >= contentLength
				|| to < 0
				|| to >= contentLength)
			{
				throw new StaticRangeNotSatisfiableException();
			}
			if(to < from)
				throw new HttpBadRequestException();
		}
	}
}
