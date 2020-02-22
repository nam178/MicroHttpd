using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace MicroHttpd.Core.Tests
{
    public class HttpFixedLengthRequestBodyTests
    {
		[Theory]
		[InlineData(0)]
		[InlineData(12)]
		[InlineData(1024)]
		[InlineData(1023)]
		[InlineData(1025)]
		[InlineData(4096)]
		[InlineData(4095)]
		[InlineData(4097)]
		[InlineData(5000)]
		[InlineData(5555)]
		public async Task ReadAsyncMethodWontReadPastContentLength(int contentLength)
		{
			var mockDataStream = MockData.MockNetworkStream(1024 * 12);
			var inst = new HttpFixedLengthRequestBody(mockDataStream, contentLength);
			var result = new MemoryStream();

			await inst.CopyToAsync(result, 1024);

			mockDataStream.SetLength(contentLength);
			Assert.Equal(contentLength, result.Length);
			Assert.True(result.ToArray().SequenceEqual(mockDataStream.ToArray()));
		}

		[Theory]
		[InlineData(0)]
		[InlineData(12)]
		[InlineData(1024)]
		[InlineData(1023)]
		[InlineData(1025)]
		[InlineData(4096)]
		[InlineData(4095)]
		[InlineData(4097)]
		[InlineData(5000)]
		[InlineData(5555)]
		public void ReadMethodWontReadPastContentLength(int contentLength)
		{
			var mockDataStream = MockData.MockNetworkStream(1024 * 12);
			var inst = new HttpFixedLengthRequestBody(mockDataStream, contentLength);
			var result = new MemoryStream();

			inst.CopyTo(result, 1024);

			mockDataStream.SetLength(contentLength);
			Assert.Equal(contentLength, result.Length);
			Assert.True(result.ToArray().SequenceEqual(mockDataStream.ToArray()));
		}
    }
}
