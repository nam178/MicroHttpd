using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MicroHttpd.Core.Content
{
	static class StaticRangeMultiRangeWriter
    {
		// The boundary string used to seperate different ranges,
		// in case of multi-range response.
		const string MultiRangeBoundaryString = "3d6b6a416f9b5";

		// The boundary
		static readonly byte[] MultiRangeBoundaryFooterString
			= Encoding.ASCII.GetBytes($"--{MultiRangeBoundaryString}--");

		// The new line character
		static readonly byte[] NewLine
			= Encoding.ASCII.GetBytes(SpecialChars.CRNL);

		public static async Task WriteAsync(
			this IStaticFileServer staticFileServer,
			IHttpRequest request,
			StaticRangeRequest[] ranges,
			IHttpResponse response,
			string pathToContentFile,
			int writeBufferSize)
		{
			var contentType = staticFileServer.GetContentTypeHeader(pathToContentFile);

			using(var fs = staticFileServer.OpenRead(pathToContentFile))
			{
				for(var i = 0; i < ranges.Length; i++)
					ranges[i] = ranges[i].ToAbsolute(fs.Length);

				// Write header
				var chunks = GenerateChunks(ranges, fs.Length, contentType);
				response.Header[HttpKeys.ContentType]
					= $"multipart/byteranges; boundary={MultiRangeBoundaryString}";
				response.Header[HttpKeys.ContentLength]
					= CalculateContentLength(chunks).Str();

				// Write body and footer (GET method only)
				if(request.Header.Method == HttpRequestMethod.GET)
				{
					for(var i = 0; i < chunks.Count; i++)
						await WriteChunkBody(response, writeBufferSize, fs, chunks[i]);
					await response.Body.WriteAsync(
						MultiRangeBoundaryFooterString,
						writeBufferSize
						);
				}
			}
		}

		static async Task WriteChunkBody(
			IHttpResponse response,
			int writeBufferSize,
			Stream src,
			(StaticRangeRequest, byte[]) chunk)
		{
			// Write chunk header
			await response.Body.WriteAsync(
				chunk.Item2,
				bufferSize: writeBufferSize
				);
			// Write chunk body
			src.Position = chunk.Item1.From;
			await src.CopyToAsync(
				response.Body,
				count: chunk.Item1.To - chunk.Item1.From + 1,
				bufferSize: writeBufferSize
				);
			// Write a  new line, chunk body always end with a new line.
			await response.Body.WriteAsync(
				NewLine,
				bufferSize: writeBufferSize
				);
		}

		static long CalculateContentLength(
			IReadOnlyList<(StaticRangeRequest, byte[])> headers)
		{
			var total = 0L;
			for(var i = 0; i < headers.Count; i++)
			{
				// Add the length of header, includes trailing newlines
				total += headers[i].Item2.Length;
				// Add the length of body, includes trailing newlines
				total += headers[i].Item1.To - headers[i].Item1.From + 1
					+ NewLine.Length;
			}
			// Add the length of footer, exclude any newlines
			total += MultiRangeBoundaryFooterString.Length;
			return total;
		}

		static IReadOnlyList<(StaticRangeRequest, byte[])> GenerateChunks(
			IReadOnlyList<StaticRangeRequest> ranges,
			long totalLength,
			string contentType)
		{
			var result = new(StaticRangeRequest, byte[])[ranges.Count];
			for(var i = 0; i < ranges.Count; i++)
			{
				result[i].Item1 = ranges[i];
				result[i].Item2 = GenerateHeader(ranges[i], totalLength, contentType);
			}
			return result;
		}

		static byte[] GenerateHeader(
			StaticRangeRequest range,
			long totalLength,
			string contentType)
		{
			var t = new StringBuilder();
			t.Append($"--{MultiRangeBoundaryString}");
			t.Append(SpecialChars.CRNL);
			t.Append($"{HttpKeys.ContentType}: {contentType}");
			t.Append(SpecialChars.CRNL);
			t.Append(range.GenerateRangeHeaderKeyAndValue(totalLength));
			t.Append(SpecialChars.CRNL);
			t.Append(SpecialChars.CRNL);
			return Encoding.ASCII.GetBytes(t.ToString());
		}
	}
}
