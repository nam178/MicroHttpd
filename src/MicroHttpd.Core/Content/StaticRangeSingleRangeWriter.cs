using System.Threading.Tasks;

namespace MicroHttpd.Core.Content
{
	static class StaticRangeSingleRangeWriter
    {
		public static async Task WriteAsync(
			this IStaticFileServer inst,
			IHttpRequest request,
			StaticRangeRequest rangeRequest,
			IHttpResponse response,
			string pathToContentFile,
			int writeBufferSize)
		{
			Validation.RequireValidBufferSize(writeBufferSize);

			// Set headers
			response.Header[HttpKeys.ContentType] =
				inst.GetContentTypeHeader(pathToContentFile);

			// Open the file
			using(var fs = inst.OpenRead(pathToContentFile))
			{
				// The requested range may contains relative values,
				// Convert them to absolute values here, as we knew the
				// content length
				rangeRequest = rangeRequest.ToAbsolute(fs.Length);

				// Set headers (cont)
				var contentLength = rangeRequest.To - rangeRequest.From + 1L;
				response.Header[HttpKeys.ContentLength] 
					= contentLength.Str();
				response.Header[HttpKeys.ContentRange] 
					= rangeRequest.GenerateRangeHeaderValue(fs.Length);

				// Write body (for GET method only)
				if(request.Header.Method == HttpRequestMethod.GET)
				{
					fs.Position = rangeRequest.From;
					await fs.CopyToAsync(
						response.Body, 
						count: contentLength, 
						bufferSize: writeBufferSize);
				}
			}
		}
	}
}
