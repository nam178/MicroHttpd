using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace MicroHttpd.Core.Content
{
	sealed class StaticRange : IContent
	{
		readonly IStaticFileServer _staticFileServer;
		readonly TcpSettings _tcpSettings;

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
				await Return416Async(response);
				return true;
			}
		}

		Task ServeRangeContentAsync(
			IHttpRequest request,
			IHttpResponse response,
			string pathToContentFile)
		{
			var requestedRanges = StaticRangeHelper.GetRequestedRanges(request.Header[HttpKeys.Range]);
			if(requestedRanges.Count == 0)
				throw new InvalidOperationException();

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
			Validate(rangeRequest);

			response.Header[HttpKeys.ContentType] =
				_staticFileServer.GetContentTypeHeader(pathToContentFile);

			using(var fs = _staticFileServer.OpenRead(pathToContentFile))
			{
				var contentLength = rangeRequest.To - rangeRequest.From + 1L;
				response.Header[HttpKeys.ContentLength] = Str(contentLength);
				response.Header[HttpKeys.Range]
					= $"Content-Range: bytes {Str(rangeRequest.From)}-{Str(rangeRequest.To)}/{Str(fs.Length)}";

				Validate(rangeRequest, fs.Length);

				fs.Position = rangeRequest.To;
				var bytesCopied = 0L;
				var buffer = new byte[_tcpSettings.ReadWriteBufferSize];
				while(bytesCopied < contentLength)
				{
					var desiredBytesToCopy = (int)Math.Min(buffer.Length, contentLength - bytesCopied);
					var actualBytesToCopy = await fs.ReadAsync(buffer, 0, desiredBytesToCopy);
					await response.Body.WriteAsync(buffer, 0, actualBytesToCopy);
					bytesCopied += actualBytesToCopy;
				}
			}
		}

		Task ServeMultiRangesContentAsync(
			IReadOnlyList<StaticRangeRequest> ranges, 
			IHttpRequest request, 
			IHttpResponse response,
			string pathToContentFile)
		{
			throw new NotImplementedException();
		}

		static void Validate(StaticRangeRequest rangeRequest)
		{
			if(rangeRequest.To < rangeRequest.From)
				throw new HttpBadRequestException($"Invalid range requested {rangeRequest}");
		}

		static void Validate(StaticRangeRequest rangeRequest, long length)
		{
			if(rangeRequest.From < 0
				|| rangeRequest.From >= length
				|| rangeRequest.To < 0
				|| rangeRequest.To >= length)
			{
				throw new StaticRangeNotSatisfiableException();
			}
		}

		static string Str(long to) => to.ToString(CultureInfo.InvariantCulture);

		static Task Return416Async(IHttpResponse response)
		{
			if(response.IsHeaderSent)
				throw new InvalidOperationException();
			ClearHeader(response, HttpKeys.ContentType);
			ClearHeader(response, HttpKeys.ContentLength);
			response.Header.StatusCode = 416;
			return response.SendHeaderAsync();
		}

		static void ClearHeader(IHttpResponse response, StringCI key)
		{
			if(response.Header.ContainsKey(key))
				response.Header.Remove(key);
		}
	}
}

