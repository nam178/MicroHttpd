using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace MicroHttpd.Core.Tests
{
	public class HttpSessionKeepAliveTests 
    {
		[Theory]
		[MemberData(nameof(Data))]
		public async Task KeepAliveByDefaultHttp11(
			string httpVersion,
			Dictionary<string, string> headers,
			bool isKeepAliveServiceReachedLimit,
			bool expectResponseKeepAlive)
		{
			MockUp(
				httpVersion, 
				headers, 
				isKeepAliveServiceReachedLimit, 
				out MemoryStream mockResponseStream, 
				out Mock<IHttpKeepAliveService> mockKeepAliveService, 
				out HttpResponse mockResponse, 
				out HttpSession session
				);

			// Execute the session
			await session.ExecuteAsync();

			// Now check the response header
			mockResponseStream.ReadResponseHeader(out HttpResponseHeader responseHeader);

			// Assert
			Assert.Equal(200, responseHeader.StatusCode);
			if(expectResponseKeepAlive)
			{
				Assert.True(
					string.Equals(responseHeader["connection"], "keep-alive",
						StringComparison.InvariantCultureIgnoreCase));
				mockKeepAliveService.Verify(inst => inst.Register(It.IsAny<MemoryStream>()),
					Times.Once);
			}
			else
			{
				Assert.True(
					string.Equals(responseHeader["connection"], "close",
						StringComparison.InvariantCultureIgnoreCase));
				mockKeepAliveService.Verify(inst => inst.Register(It.IsAny<MemoryStream>()),
					Times.Never);
			}
		}

		static void MockUp(
			string httpVersion, 
			Dictionary<string, string> headers, 
			bool isKeepAliveServiceReachedLimit, 
			out MemoryStream mockResponseStream, 
			out Mock<IHttpKeepAliveService> mockKeepAliveService, 
			out HttpResponse mockResponse, 
			out HttpSession session)
		{
			var headerLines = new List<string>
			{
				$"GET / {httpVersion}",
				"Host: google.com"
			};
			foreach(var kv in headers)
				headerLines.Add($"{kv.Key}: {kv.Value}");
			headerLines.Add("\r\n\r\n");
			var mockRequestStream = string.Join("\r\n", headerLines).ToMemoryStream();
			mockResponseStream = new MemoryStream();
			var mockContent = new Mock<IContent>();
			mockKeepAliveService = new Mock<IHttpKeepAliveService>();
			mockKeepAliveService
				.Setup(x => x.CanRegister(It.IsAny<IDisposable>()))
				.Returns(!isKeepAliveServiceReachedLimit);
			var mockRequest = new HttpRequest(
				mockRequestStream,
				TcpSettings.Default,
				new HttpRequestBodyFactory());
			var mockResponseTmp = new HttpResponse(
				mockRequest,
				mockResponseStream,
				TcpSettings.Default,
				HttpSettings.Default);
			mockResponse = mockResponseTmp;
			mockContent
				.Setup(inst => inst.WriteContentAsync(
					mockRequest,
					mockResponseTmp))
				.Returns(Task.FromResult(true));
			session = new HttpSession(
				new MemoryStream(),
				TcpSettings.Default,
				mockContent.Object,
				new ContentSettings { DefaultCharsetForTextContents = "utf-8" },
				mockKeepAliveService.Object,
				mockRequest,
				mockResponse
				);
		}

		public static IEnumerable<object[]> Data()
		{
			// HTTP 1.1, should keep alive by default
			yield return new object[]
			{
				"HTTP/1.1",
				new Dictionary<string, string>(),
				false,
				true
			};

			// HTTP 1.0, shouldnt keep alive by default
			yield return new object[]
			{
				"HTTP/1.0",
				new Dictionary<string, string>(),
				false,
				false
			};

			// HTTP 1.1 client says close, 
			// the server should close the connection
			yield return new object[]
			{
				"HTTP/1.1",
				new Dictionary<string, string> {
					{ "Connection", "close" }
				},
				false,
				false
			};

			// HTTP 1.1 client says keep alive, 
			// the server should keep the connection alive
			yield return new object[]
			{
				"HTTP/1.1",
				new Dictionary<string, string> {
					{ "Connection", "keep-alive" }
				},
				false,
				true
			};

			// HTTP 1.1 client says keep alive, 
			// But we hit the limit.
			// the server should close the connection
			yield return new object[]
			{
				"HTTP/1.1",
				new Dictionary<string, string> {
					{ "Connection", "keep-alive" }
				},
				true,
				false
			};
		}
	}

		//[Fact]
		//public void ReturnsInternalServerErrorOnUnhandledException()
		//{

		//}
}
