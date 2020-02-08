using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace MicroHttpd.Core.Tests
{
    public class HttpResponseHeaderTests
    {
		[Theory]
		[InlineData(200, "HTTP/1.1 200 OK")]
		[InlineData(400, "HTTP/1.1 400 Bad Request")]
		public void GenerateCorrectStartLineFromStatusCode(int statusCode, string expectedStartLine)
		{
			var header = new HttpResponseHeader();
			header.StatusCode = statusCode;
			Assert.Equal(expectedStartLine, header.AsPlainText.Split("\r\n")[0]);
		}

		[Fact]
		public void AsPlainTextPropertyChangesAfterUpdatingStatusCode()
		{
			var header = new HttpResponseHeader();
			// Default header
			Assert.Equal("HTTP/1.1 200 OK", header.AsPlainText.Split("\r\n")[0]);

			// Now change the StatusCode
			header.StatusCode = 400;
			Assert.Equal("HTTP/1.1 400 Bad Request", header.AsPlainText.Split("\r\n")[0]);
		}
    }
}
