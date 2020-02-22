using MicroHttpd.Core.Content;
using Moq;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace MicroHttpd.Core.Tests
{
    public class StaticRangeFileServerExtensionsTest
    {
		[Fact]
		public async Task ServeSingleRangeAsync()
		{
			MockUp(
				out Mock<IHttpRequest> request,
				out Mock<IStaticFileServer> mockStaticFileServer,
				out Mock<IHttpResponse> mockResponse,
				out MemoryStream mockResponseBody,
				out HttpResponseHeader mockResponseHeader
				);

			// Test
			await StaticRangeSingleRangeWriter.WriteAsync(
				mockStaticFileServer.Object,
				request.Object,
				new StaticRangeRequest(3, 7),
				mockResponse.Object,
				"foo.txt",
				1024
				);

			// Check
			Assert.Equal("5", mockResponseHeader["content-length"]);
			Assert.Equal("bytes 3-7/10", mockResponseHeader["content-range"]);
			Assert.Equal("text/plain", mockResponseHeader["content-type"]);
			Assert.True(mockResponseBody.ToArray().SequenceEqual("34567".ToBytes()));
		}

		[Fact]
		public async Task ServeMultiRangeAsync()
		{
			MockUp(
				out Mock<IHttpRequest> request,
				out Mock<IStaticFileServer> mockStaticFileServer, 
				out Mock<IHttpResponse> mockResponse, 
				out MemoryStream mockResponseBody, 
				out HttpResponseHeader mockResponseHeader
				);

			// Test
			await StaticRangeMultiRangeWriter.WriteAsync(
				mockStaticFileServer.Object,
				request.Object,
				new StaticRangeRequest[]
				{
					new StaticRangeRequest(3, 7),
					new StaticRangeRequest(8, 9),
				},
				mockResponse.Object,
				"foo.txt",
				1024
				);

			// Check
			Assert.True(mockResponseBody.Length > 0);
			Assert.Equal(int.Parse(mockResponseHeader["content-length"]), mockResponseBody.Length);
			Assert.False(mockResponseHeader.ContainsKey("content-range"));
			var expected = @"--3d6b6a416f9b5" + "\r\n" +
				"Content-Type: text/plain" + "\r\n" +
				"Content-Range: bytes 3-7/10" + "\r\n" + "\r\n" +
				"34567" + "\r\n" + 
				"--3d6b6a416f9b5" + "\r\n" +
				"Content-Type: text/plain" + "\r\n" + 
				"Content-Range: bytes 8-9/10" + "\r\n" + "\r\n" +
				"89" + "\r\n" +
				"--3d6b6a416f9b5--";

			Assert.Equal(expected, Encoding.ASCII.GetString(mockResponseBody.ToArray()));
		}

		static void MockUp(
			out Mock<IHttpRequest> request,
			out Mock<IStaticFileServer> mockStaticFileServer, 
			out Mock<IHttpResponse> mockResponse, 
			out MemoryStream mockResponseBody, 
			out HttpResponseHeader mockResponseHeader)
		{
			var content = Encoding.ASCII.GetBytes("0123456789");
			request = new Mock<IHttpRequest>();
			request
				.Setup(x => x.Header)
				.Returns(new HttpRequestHeader("GET / HTTP/1.1"));

			mockStaticFileServer = new Mock<IStaticFileServer>();
			mockStaticFileServer
.Setup(x => x.OpenRead("foo.txt"))
.Returns(new MemoryStream(content));

			mockStaticFileServer
				.Setup(x => x.GetContentTypeHeader("foo.txt"))
				.Returns("text/plain");

			mockResponse = new Mock<IHttpResponse>();
			mockResponseBody = new MemoryStream();
			mockResponseHeader = new HttpResponseHeader();
			mockResponse.Setup(x => x.Body).Returns(mockResponseBody);
			mockResponse.Setup(x => x.Header).Returns(mockResponseHeader);
		}
	}
}
