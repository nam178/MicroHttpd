using Xunit;

namespace MicroHttpd.Core.Tests
{
	public class HttpRequestHeaderBuilderTests
    {
		[Fact]
		public void ThrowsExceptionIfFirstLineIsBlank()
		{
			var builder = HttpHeaderBuilderFactory.CreateRequestHeaderBuilder();
			Assert.Throws<HttpInvalidMessageException>(delegate
			{
				var d = "\r\nGET /\r\n".ToBytes();
				builder.AppendBuffer(d, 0, d.Length, out _);
			});
		}

		[Theory]
		[InlineData("\r\n")]
		[InlineData("\n")]
		public void SupportsBothLineEndingTypes(string newLineChar)
		{
			var msg = @"GET /myfile.txt HTTP/1.1" + newLineChar
				+ "Header1: value1 " + newLineChar
				+ "Header2: value2" + newLineChar
				+ newLineChar
				+ "---";

			var builder = HttpHeaderBuilderFactory.CreateRequestHeaderBuilder();
			int bodyStartIndex;
			Assert.True(
				builder.AppendBuffer(
					msg.ToBytes(), 
					0, 
					msg.ToBytes().Length, 
					out bodyStartIndex));
			Assert.True(
				bodyStartIndex == msg.ToBytes().Length - 3,
				$"Incorrect value of {nameof(bodyStartIndex)}"
				);
		}

		[Fact]
		public void WillTrimHeaderValue()
		{
			var msg = "GET /myfile.txt HTTP/1.1\r\n" +
				"key: value1 \r\n\r\n";
			var builder = HttpHeaderBuilderFactory.CreateRequestHeaderBuilder();
			Assert.True(builder.AppendBuffer(msg.ToBytes(), 0, msg.ToBytes().Length, out _));
			Assert.True(builder.Result.GetFirst("key") == "value1");
		}

		[Fact]
		public void RespectOffsetAndCountParameters()
		{
			var msg = "___GET /myfile.txt HTTP/1.1\r\n" +
				"key: value1 \r\n\r\n_____";
			var builder = HttpHeaderBuilderFactory.CreateRequestHeaderBuilder();
			Assert.True(builder.AppendBuffer(msg.ToBytes(), 3, msg.ToBytes().Length - 3 -5, out _));
			Assert.True(builder.Result.GetFirst("key") == "value1");
			Assert.Equal(HttpRequestMethod.GET, builder.Result.Method);
		}
    }
}
