using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MicroHttpd.Core.Content
{
	sealed class StaticRange : IContent
	{
		readonly IStaticFileServer _staticFileServer;
		readonly TcpSettings _tcpSettings;

		// Maximum number of ranges a client can request,
		// should be a reasonable number so the server don't
		// go crazy seeking back and forth.
		const int MaxRangeCount = 32;

		// The boundary string used to seperate different ranges,
		// in case of multi-range response.
		const string MultiRangeBoundaryString = "3d6b6a416f9b5";

		static readonly byte[] MultiRangeBoundaryFooterString = Encoding.ASCII.GetBytes($"--{MultiRangeBoundaryString}--");
		static readonly byte[] NewLine = Encoding.ASCII.GetBytes(SpecialChars.CRNL);

		public StaticRange(IStaticFileServer staticFileServer, TcpSettings tcpSettings)
		{
			Validation.RequireValidTcpSettings(tcpSettings);
			_staticFileServer = staticFileServer 
				?? throw new ArgumentNullException(nameof(staticFileServer));
			_tcpSettings = tcpSettings;
		}

		public async Task<bool> ServeAsync(IHttpRequest request, IHttpResponse response)
		{
			// Alter the header to indicate that we accept range requests;
			response.Header["Accept-Ranges"] = "bytes";
			try
			{
				// Handle the request if client requested range
				if(request.Header.Method == HttpRequestMethod.GET
					&& request.Header.ContainsKey(HttpKeys.Range)
					&& _staticFileServer.TryResolve(request, out string resolvedFile))
				{
					await ServeRangeContentAsync(request, response, resolvedFile);
					return true;
				}
				// Else, we won't be serving range content for this request,
				// leave it untouched.
				return false;
			}
			catch (StaticRangeNotSatisfiableException) {
				// Bad range requested by the client,
				// We'll handle this
				await response.Return416Async();
				return true;
			}
		}

		Task ServeRangeContentAsync(
			IHttpRequest request,
			IHttpResponse response,
			string pathToContentFile)
		{
			// Get and validate the range
			var requestedRanges = StaticRangeHelper.GetRequestedRanges(request.Header[HttpKeys.Range]);
			if(requestedRanges.Count == 0 || requestedRanges.Count > MaxRangeCount)
				throw new StaticRangeNotSatisfiableException();

			// Range is good, from this point forward,
			// we will be serving partial content.
			response.Header.StatusCode = 206;

			if(requestedRanges.Count == 1)
				return ServeSingleRangeContentAsync(requestedRanges[0], request, response, pathToContentFile);
			else
				return ServeMultiRangesContentAsync(requestedRanges, request, response, pathToContentFile);
		}

		async Task ServeSingleRangeContentAsync(
			StaticRangeRequest rangeRequest,
			IHttpRequest request,
			IHttpResponse response,
			string pathToContentFile)
		{
			rangeRequest.RequireValidRange();

			response.Header[HttpKeys.ContentType] =
				_staticFileServer.GetContentTypeHeader(pathToContentFile);

			using(var fs = _staticFileServer.OpenRead(pathToContentFile))
			{
				var contentLength = rangeRequest.To - rangeRequest.From + 1L;
				rangeRequest.RequireValidRangeWithin(contentLength: fs.Length);

				response.Header[HttpKeys.ContentLength] = contentLength.Str();
				response.Header[HttpKeys.Range] = rangeRequest.GenerateRangeHeader(fs.Length);

				fs.Position = rangeRequest.From;
				await fs.CopyToAsync(response.Body, contentLength, _tcpSettings.ReadWriteBufferSize);
			}
		}

		async Task ServeMultiRangesContentAsync(
			IReadOnlyList<StaticRangeRequest> ranges, 
			IHttpRequest request, 
			IHttpResponse response,
			string pathToContentFile)
		{
			var contentType = _staticFileServer.GetContentTypeHeader(pathToContentFile); 

			using(var fs = _staticFileServer.OpenRead(pathToContentFile))
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
						bufferSize: _tcpSettings.ReadWriteBufferSize
						);
					// Write chunk body
					fs.Position = headers[i].Item1.From;
					await fs.CopyToAsync(
						response.Body,
						count: headers[i].Item1.To - headers[i].Item1.From + 1,
						bufferSize: _tcpSettings.ReadWriteBufferSize
						);
					// Write a  new line, chunk body always end with a new line.
					await response.Body.WriteAsync(
						NewLine, 
						bufferSize: _tcpSettings.ReadWriteBufferSize
						);
				}

				// Write the footer
				await response.Body.WriteAsync(
					MultiRangeBoundaryFooterString, 
					_tcpSettings.ReadWriteBufferSize
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
			var result = new (StaticRangeRequest, byte[])[ranges.Count];
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
			t.Append(range.GenerateRangeHeader(totalLength));
			t.Append(SpecialChars.CRNL);
			t.Append(SpecialChars.CRNL);
			return Encoding.ASCII.GetBytes(t.ToString());
		}
	}
}

