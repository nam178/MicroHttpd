using Moq;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace MicroHttpd.Core.Tests
{
	public class HttpSessionExceptionHandlingTests
    {
		[Fact]
		public async Task Returns500ErrorsOnUnhandledExceptions()
		{
			var mockResponseStream = new MemoryStream();
			var mockContent = new Mock<IContent>();
			var mockRequest = new HttpRequest(
				"GET / HTTP/1.1\r\nHost: google.com\r\n\r\n".ToMemoryStream(),
				TcpSettings.Default,
				new HttpRequestBodyFactory());
			var mockResponse = new HttpResponse(
				mockRequest,
				mockResponseStream,
				TcpSettings.Default,
				HttpSettings.Default);
			mockContent
				.Setup(inst => inst.ServeAsync(It.IsAny<IHttpRequest>(), It.IsAny<IHttpResponse>()))
				.Callback(() => throw new Exception("My Custom Message"));
			var session = new HttpSession(
				new MemoryStream(),
				TcpSettings.Default,
				mockContent.Object,
				new ContentSettings { DefaultCharsetForTextContents = "utf-8" },
				new Mock<IHttpKeepAliveService>().Object,
				mockRequest,
				mockResponse
				);

			// Test
			await session.ExecuteAsync();

			// Check
			mockResponseStream.ReadResponseHeader(out HttpResponseHeader responseHeader);
			Assert.Equal(500, responseHeader.StatusCode);
			Assert.True(string.Equals(
				responseHeader["connection"],
				"close",
				StringComparison.InvariantCultureIgnoreCase
				));
		}

		[Fact]
		public async Task Returns400ErrorOnMalformedReuest()
		{
			var mockResponseStream = new MemoryStream();
			var mockContent = new Mock<IContent>();
			var mockRequest = new HttpRequest(
				"GET / HTTP/1.1\r\n############\r\n\r\n".ToMemoryStream(),
				TcpSettings.Default,
				new HttpRequestBodyFactory());
			var mockResponse = new HttpResponse(
				mockRequest,
				mockResponseStream,
				TcpSettings.Default,
				HttpSettings.Default);
			var session = new HttpSession(
				new MemoryStream(),
				TcpSettings.Default,
				mockContent.Object,
				new ContentSettings { DefaultCharsetForTextContents = "utf-8" },
				new Mock<IHttpKeepAliveService>().Object,
				mockRequest,
				mockResponse);

			// Test
			await session.ExecuteAsync();

			// Check
			mockResponseStream.ReadResponseHeader(out HttpResponseHeader responseHeader);
			Assert.Equal(400, responseHeader.StatusCode);
			Assert.True(string.Equals(
				responseHeader["connection"], 
				"close", 
				StringComparison.InvariantCultureIgnoreCase
				));
		}
    }
}
