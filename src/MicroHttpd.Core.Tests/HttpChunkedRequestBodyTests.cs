using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace MicroHttpd.Core.Tests
{
	public class HttpChunkedRequestBodyTests
    {
		[Theory]
		[InlineData(1)]
		[InlineData(16)]
		[InlineData(4095)]
		[InlineData(4096)]
		[InlineData(5000)]
		[InlineData(37, 4096, 8199, 32, 20000)]
		public async Task CanDecodeChunkedMessage(params int[] chunkLengths)
		{
			await DecodeTestWith(chunkLengths);
		}

		[Fact]
		public async Task CanDecodeEmptyChunkedMessage()
		{
			await DecodeTestWith(new int[0]);
		}

		static async Task DecodeTestWith(int[] chunkLengths)
		{
			MockHttpChunkedMessageBody.Generate(
							chunkLengths,
							out List<MemoryStream> chunks,
							out byte[] httpBody
							);
			var bodyStream = new MemoryStream(httpBody);
			var inst = new HttpChunkedRequestBody(
				new RollbackableStream(bodyStream, TcpSettings.Default),
				new HttpRequestHeader(),
				TcpSettings.Default
				);

			var result = new MemoryStream();
			await inst.CopyToAsync(result);

			Assert.True(
				result.ToArray().SequenceEqual(chunks.SelectMany(m => m.ToArray()))
				);
		}

		[Theory]
		[InlineData("\r\n")]
		[InlineData("\n")]
		public async Task AcceptBothLineEndingTypes(string newLine)
		{
			MockHttpChunkedMessageBody.NewLine = newLine;
			await DecodeTestWith(new int[] { 23, 25, 40961, 5010 });
		}

		[Fact]
		public async Task CanHaveTrailer()
		{
			MockHttpChunkedMessageBody.Generate(
				new int[] { 400, 4000 },
				new Dictionary<string, string> {
					{ @"Content-Type", "application/json" }
				},
				out List<MemoryStream> chunks,
				out byte[] httpBody
				);
			var header = new HttpRequestHeader();
			var inst = new HttpChunkedRequestBody(
				new RollbackableStream(new MemoryStream(httpBody), TcpSettings.Default),
				header,
				TcpSettings.Default
				);

			await inst.CopyToAsync(new MemoryStream());

			Assert.True(header.ContainsKey("Content-Type"));
			Assert.Equal("application/json", header["Content-Type"]);
		}
    }
}
