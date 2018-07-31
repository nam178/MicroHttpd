using Xunit;

namespace MicroHttpd.Core.Tests
{
	public class ChunkHeaderBuilderTests
    {
		[Theory]
		[InlineData("50f\r\n", 1295, 5)]
		[InlineData("50f\n", 1295, 4)]
		[InlineData("50f; Some Extensions=Value\nBodyContents", 1295, 27)]
		[InlineData("50f\nBodyContents", 1295, 4)]
		public void WorksWhenHeaderEndsInTheFirstBuffer(
			string bufferString, 
			long expectedBodyLength, 
			int expectedContentStarts)
		{
			var buffer = bufferString.ToBytes();
			var builder = new HttpChunkHeaderBuilder();
			int expectedContentStartIndex;
			Assert.True(builder.AppendBuffer(buffer, 0, buffer.Length, out expectedContentStartIndex));
			Assert.Equal(expectedContentStarts, expectedContentStartIndex);
			Assert.Equal(expectedBodyLength, builder.Result.Length);
		}

		[Fact]
		public void WorksWithOffsetAndLength()
		{
			var buffer = "___8; Extension\r\n".ToBytes();
			var builder = new HttpChunkHeaderBuilder();
			Assert.False(builder.AppendBuffer(buffer, 3, 5, out _));
			Assert.True(builder.AppendBuffer(buffer, 8, 9, out _));
		}

		[Fact]
		public void AcceptZeroLengthHeader()
		{
			var buffer = "0; Some dummy extensions = value;\r\n".ToBytes();
			var builder = new HttpChunkHeaderBuilder();
			Assert.True(builder.AppendBuffer(buffer, 0, buffer.Length, out _));
		}

		[Theory]
		[InlineData("80000000\r\n")]
		[InlineData("-50f\r\n")]
		public void ThrowsExceptionOnInvalidHexString(string hexString)
		{
			var buffer = hexString.ToBytes();
			var builder = new HttpChunkHeaderBuilder();
			Assert.Throws<HttpBadRequestException>(delegate
			{
				builder.AppendBuffer(buffer, 0, buffer.Length, out _);
			});
		}
    }
}
