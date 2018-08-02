using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MicroHttpd.Core.Content
{
	static class StaticRangeFileServerExtensions
    {
		// The boundary string used to seperate different ranges,
		// in case of multi-range response.
		const string MultiRangeBoundaryString = "3d6b6a416f9b5";

		static readonly byte[] MultiRangeBoundaryFooterString = Encoding.ASCII.GetBytes($"--{MultiRangeBoundaryString}--");
		static readonly byte[] NewLine = Encoding.ASCII.GetBytes(SpecialChars.CRNL);

		public static async Task ServeSingleRangeAsync(
			this IStaticFileServer inst,
			StaticRangeRequest rangeRequest,
			IHttpResponse response,
			string pathToContentFile,
			int writeBufferSize)
		{
			Validation.RequireValidBufferSize(writeBufferSize);
			rangeRequest.RequireValidRange();

			// Set headers
			response.Header[HttpKeys.ContentType] =
				inst.GetContentTypeHeader(pathToContentFile);

			// Open the file
			using(var fs = inst.OpenRead(pathToContentFile))
			{
				// Set headers (cont)
				var contentLength = rangeRequest.To - rangeRequest.From + 1L;
				rangeRequest.RequireValidRangeWithin(contentLength: fs.Length);

				response.Header[HttpKeys.ContentLength] = contentLength.Str();
				response.Header[HttpKeys.ContentRange] = rangeRequest.GenerateRangeHeaderValue(fs.Length);

				// Write body
				fs.Position = rangeRequest.From;
				await fs.CopyToAsync(response.Body, count: contentLength, bufferSize: writeBufferSize);
			}
		}

		public static async Task ServeMultiRangeAsync(
			this IStaticFileServer staticFileServer,
			IReadOnlyList<StaticRangeRequest> ranges,
			IHttpResponse response,
			string pathToContentFile,
			int writeBufferSize)
		{
			var contentType = staticFileServer.GetContentTypeHeader(pathToContentFile);

			using(var fs = staticFileServer.OpenRead(pathToContentFile))
			{
				// Write header
				var headers = GenerateHeaders(ranges, fs.Length, contentType);
				response.Header[HttpKeys.ContentType]
					= $"multipart/byteranges; boundary={MultiRangeBoundaryString}";
				response.Header[HttpKeys.ContentLength]
					= CalculateContentLength(headers).Str();

				// Write body
				for(var i = 0; i < headers.Count; i++)
				{
					// Write chunk header
					await response.Body.WriteAsync(
						headers[i].Item2,
						bufferSize: writeBufferSize
						);
					// Write chunk body
					fs.Position = headers[i].Item1.From;
					await fs.CopyToAsync(
						response.Body,
						count: headers[i].Item1.To - headers[i].Item1.From + 1,
						bufferSize: writeBufferSize
						);
					// Write a  new line, chunk body always end with a new line.
					await response.Body.WriteAsync(
						NewLine,
						bufferSize: writeBufferSize
						);
				}

				// Write the footer
				await response.Body.WriteAsync(
					MultiRangeBoundaryFooterString,
					writeBufferSize
					);
			}
		}

		static long CalculateContentLength(IReadOnlyList<(StaticRangeRequest, byte[])> headers)
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

		static IReadOnlyList<(StaticRangeRequest, byte[])> GenerateHeaders(
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

		static byte[] GenerateHeader(StaticRangeRequest range, long totalLength, string contentType)
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
