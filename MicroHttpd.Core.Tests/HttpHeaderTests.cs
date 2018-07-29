using System.Linq;
using Xunit;

namespace MicroHttpd.Core.Tests
{
	public class HttpHeaderTests
    {
		[Fact]
		public void HeaderKeysAreCaseInsensitive()
		{
			var entries = new HttpHeaderEntries();
			entries.Add("KEY", "value");
			var header = new HttpRequestHeader(
				"GET /myFile HTTP/1.1",
				entries);

			Assert.True(header.ContainsKey("key"));
			Assert.True(header.ContainsKey("KEY"));
			Assert.True(header.ContainsKey("KeY"));
			Assert.False(header.ContainsKey("KeY1"));
			Assert.False(header.ContainsKey(" key"));
			Assert.False(header.ContainsKey("key "));
			Assert.Equal("value", header.GetFirst("Key"));

			// overwrite the value, see if it confuses 'Key' vs 'KEY'
			header.Remove("KEY");
			header.Add("KEY", "newValue");
			Assert.Equal("newValue", header.GetFirst("Key"));
			Assert.Equal("newValue", header.GetFirst("KEY"));
		}

		[Fact]
		public void CanRemoveHeaderKey()
		{
			var entries = new HttpHeaderEntries();
			entries.Add("KEY", "value");
			var header = new HttpRequestHeader(
				"GET /myFile HTTP/1.1",
				entries);

			header.Remove("KEY");
			Assert.False(header.ContainsKey("KEY"));
			Assert.False(header.ContainsKey("key"));
		}

		[Fact]
		public void CanHaveMiltipleValuesPerKey()
		{
			var header = new HttpRequestHeader("GET /myFile HTTP/1.1");
			header.Add("key", "value1");
			header.Add("key", "value2");

			Assert.Equal("value1", header.GetFirst("key"));
			header
				.Get("key", getAsCopy: true)
				.SequenceEqual(new string[] { "value1", "value2" });
		}

		[Fact]
		public void ParseCorrectVerbAndURI()
		{
			var header = new HttpRequestHeader("GET /myFile HTTP/1.1");
			Assert.Equal("GET", header.Verb);
			Assert.Equal("/myFile", header.Uri);
		}

		[Theory]
		[InlineData("HTTP/1.1", HttpProtocol.Http11)]
		[InlineData("HTTP/1.0", HttpProtocol.Http10)]
		[InlineData("HTTP/1.3", HttpProtocol.Unknown)]
		[InlineData("random", HttpProtocol.Unknown)]
		internal void ParseCorrectHttpVersion(string httpVersionText, HttpProtocol expectedProtocol)
		{
			var header = new HttpRequestHeader($"GET /myFile {httpVersionText}");
			Assert.Equal(header.Protocol, expectedProtocol);
		}

		[Fact]
		public void ExportHeaderToStringCorrectly()
		{
			var entries = new HttpHeaderEntries();
			entries.Add("KEY1", "value1");
			entries.Add("KEY2", "value1");
			entries.Add("KEY2", "value2");
			var header = new HttpRequestHeader(
				"GET /myFile HTTP/1.1",
				entries);

			Assert.Equal(
				"GET /myFile HTTP/1.1\r\n"
				+ "KEY1: value1\r\n"
				+ "KEY2: value1\r\n"
				+ "KEY2: value2\r\n\r\n"
				,
				header.AsPlainText
				);
		}
    }
}
