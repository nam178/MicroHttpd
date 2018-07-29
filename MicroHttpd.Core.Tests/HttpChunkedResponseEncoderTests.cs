using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace MicroHttpd.Core.Tests
{
    public class HttpChunkedResponseEncoderTests
    {
		[Theory]
		[InlineData(0, 4096)]
		[InlineData(1, 4096)]
		[InlineData(16121, 4096)]
		[InlineData(16121, 1024)]
		[InlineData(16121, 1023)]
		[InlineData(16121, 1025)]
		[InlineData(144, 4096)]
		[InlineData(144, 1024)]
		[InlineData(144, 1023)]
		[InlineData(144, 1025)]
		[InlineData(0, 4096, false)]
		[InlineData(1, 4096, false)]
		[InlineData(16121, 4096, false)]
		[InlineData(16121, 1024, false)]
		[InlineData(16121, 1023, false)]
		[InlineData(16121, 1025, false)]
		[InlineData(144, 4096, false)]
		[InlineData(144, 1024, false)]
		[InlineData(144, 1023, false)]
		[InlineData(144, 1025, false)]
		public async Task CanDecodeEncodedData(
			int testDataSize, 
			int appendBufferSize,
			bool asyncVersion = true)
		{
			// Mockup
			var httpSettings = HttpSettings.Default;
			httpSettings.MaxBodyChunkSize = 1024;
			var targetStream = new MemoryStream();
			var testData = MockData.Bytes(testDataSize, 7);
			var encoder = new HttpChunkedResponseEncoder(
				targetStream,
				TcpSettings.Default,
				httpSettings
				);

			if(asyncVersion)
			{
				await encoder.AppendAsync(testData, appendBufferSize);
				await encoder.CompleteAsync();
			}
			else
			{
				encoder.Append(testData, appendBufferSize);
				encoder.Complete();
			}

			// Check
			targetStream.Position = 0;
			var decoder = new HttpChunkedRequestBody(
				new RollbackableStream(targetStream, TcpSettings.Default),
				new HttpRequestHeader(),
				TcpSettings.Default
			);
			var result = new MemoryStream();
			await decoder.CopyToAsync(result);
			Assert.True(result.ToArray().SequenceEqual(testData));
		}
	}
}
