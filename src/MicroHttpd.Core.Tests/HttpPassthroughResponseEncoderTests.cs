using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace MicroHttpd.Core.Tests
{
	public class HttpPassthroughResponseEncoderTests
    {
		[Theory]
		[InlineData(0, 1024)]
		[InlineData(1, 1024)]
		[InlineData(1023, 1024)]
		[InlineData(1024, 1024)]
		[InlineData(1025, 1024)]
		[InlineData(8162, 1024)]
		public async Task WriteDataAsIt(int testSize, int appendBufferSize)
		{
			var targetStream = new MemoryStream();
			var encoder = new HttpPassthroughResponseEncoder(targetStream, testSize);
			var testData = MockData.Bytes(testSize, seed: 7);

			// Begin test
			await encoder.AppendAsync(testData, appendBufferSize);
			await encoder.CompleteAsync();

			Assert.True(targetStream.ToArray().SequenceEqual(testData));
		}

		[Fact]
		public async Task WontLetCallerToWriteMoreThanPredefinedLength()
		{
			var encoder = new HttpPassthroughResponseEncoder(new MemoryStream(), 8);
			await encoder.AppendAsync(new byte[2], 0, 2);
			Assert.Throws<InvalidOperationException>(() =>
			{
				try
				{
					encoder.AppendAsync(new byte[2], 0, 7).Wait(TimeSpan.FromSeconds(5));
				}
				catch(AggregateException ex) {
					throw ex.InnerException;
				}
			});
		}

		[Fact]
		public async Task WontLetCallerWriteLessThanPredefinedLength()
		{
			var encoder = new HttpPassthroughResponseEncoder(new MemoryStream(), 8);
			await encoder.AppendAsync(new byte[2], 0, 2);
			Assert.Throws<InvalidOperationException>(() =>
			{
				try
				{
					encoder.CompleteAsync().Wait(TimeSpan.FromSeconds(5));
				}
				catch(AggregateException ex)
				{
					throw ex.InnerException;
				}
			});
		}
    }
}
