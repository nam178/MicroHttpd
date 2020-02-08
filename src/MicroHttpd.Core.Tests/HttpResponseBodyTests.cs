using NLog;
using Moq;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace MicroHttpd.Core.Tests
{
	public sealed class HttpResponseBodyTests
	{
		[Theory]
		[InlineData(4096, 8000)]
		[InlineData(7000, 8000)]
		[InlineData(8200, 8000)]
		[InlineData(4096, 4096)]
		[InlineData(4097, 4096)]
		[InlineData(8200, 4096)]
		[InlineData(36002, 4096)]
		[InlineData(4096, 8000, true)]
		[InlineData(7000, 8000, true)]
		[InlineData(8200, 8000, true)]
		[InlineData(4096, 4096, true)]
		[InlineData(4097, 4096, true)]
		[InlineData(8200, 4096, true)]
		[InlineData(36002, 4096, true)]
		public async Task UseChunkedEncodingWhenWritesExceedInternalBuffer(
			int testDataSize,
			int writeBufferSize,
			bool endResponseBodyByDisposingIt = false)
		{
			// Mockup
			MockUp(testDataSize,
				out HttpSettings httpSettings,
				out MemoryStream senderData,
				out Mock<IHttpResponse> mockHttpResponse,
				out MemoryStream receiverData,
				out _);

			// Begin test
			await PerformWriteIntoBody(endResponseBodyByDisposingIt, httpSettings,
				senderData, mockHttpResponse, receiverData, writeBufferSize);

			// Test decode
			await VerifyReceivedChunkedMessage(senderData, receiverData);
		}

		static void MockUp(
			int testDataSize,
			out HttpSettings httpSettings,
			out MemoryStream senderData,
			out Mock<IHttpResponse> mockHttpResponse,
			out MemoryStream receiverData,
			out HttpResponseHeader responseHeader)
		{
			httpSettings = HttpSettings.Default;
			httpSettings.MaxBodySizeInMemory = 4 * 1024;
			senderData = MockData.MemoryStream(testDataSize);
			var responseHeaderTmp = new HttpResponseHeader();
			responseHeader = responseHeaderTmp;
			mockHttpResponse = new Mock<IHttpResponse>(MockBehavior.Strict);
			var isHeaderSent = false;
			var t = mockHttpResponse.Object;
			mockHttpResponse
				.Setup(inst => inst.Header)
				.Returns(responseHeader);
			mockHttpResponse
				.Setup(inst => inst.IsHeaderSent)
				.Returns(() => isHeaderSent);
			mockHttpResponse
				.Setup(inst => inst.SendHeaderAsync())
				.Returns(delegate
				{
					if(isHeaderSent)
						throw new InvalidDataException();
					isHeaderSent = true;
					responseHeaderTmp.IsWritable = false;
					return Task.FromResult(true);
				});
			receiverData = new MemoryStream();
		}

		static async Task VerifyReceivedChunkedMessage(
			MemoryStream senderData,
			MemoryStream receiverData)
		{
			receiverData.Position = 0;
			var chunkedDecoder = new HttpChunkedRequestBody(
				new RollbackableStream(receiverData, TcpSettings.Default),
				new HttpRequestHeader(),
				TcpSettings.Default
				);
			var receiverDecodedData = new MemoryStream();
			await chunkedDecoder.CopyToAsync(receiverDecodedData);
			Xunit.Assert.True(senderData.ToArray().SequenceEqual(receiverDecodedData.ToArray()));
		}

		static async Task PerformWriteIntoBody(
			bool endResponseBodyByDisposingIt,
			HttpSettings httpSettings,
			MemoryStream senderData,
			Mock<IHttpResponse> mockHttpResponse,
			MemoryStream receiverData,
			int writeBufferSize = 4096)
		{
			var body = new HttpResponseBody(
							receiverData,
							TcpSettings.Default,
							httpSettings,
							mockHttpResponse.Object);

			await senderData.CopyToAsync(body, writeBufferSize);

			if(endResponseBodyByDisposingIt)
				body.Dispose();
			else
				await body.CompleteAsync();
		}

		[Fact]
		async Task UseChunkedEncodingWhenExplicitlySetInHeader()
		{
			// Mockup
			MockUp(2000 /* We'll send only 2000 bytes so it won't overflow 
				the internal buffer, and won't force chunked encoder tobe used. */,
				out HttpSettings httpSettings,
				out MemoryStream senderData,
				out Mock<IHttpResponse> mockHttpResponse,
				out MemoryStream receiverData,
				out HttpResponseHeader responseHeader);

			responseHeader[HttpKeys.TransferEncoding] = "chunked";

			// Begin test
			using(var body = new HttpResponseBody(
				receiverData,
				TcpSettings.Default,
				httpSettings,
				mockHttpResponse.Object))
			{
				await senderData.CopyToAsync(body, 4096);
			}

			// Test decode
			await VerifyReceivedChunkedMessage(senderData, receiverData);
		}

		[Theory]
		[InlineData(4095, true)]
		[InlineData(4095, false)]
		[InlineData(2000, true)]
		[InlineData(2000, false)]
		[InlineData(1, true)]
		[InlineData(1, false)]
		async Task UsePassthroughEncodingIfInternalBufferUnderflow(int length, bool endResponseBodyByDisposingIt = false)
		{
			// Mockup
			MockUp(length /* We'll supply test data that smaller than the internal buffer */,
				out HttpSettings httpSettings,
				out MemoryStream senderData,
				out Mock<IHttpResponse> mockHttpResponse,
				out MemoryStream receiverData,
				out HttpResponseHeader responseHeader);

			// Begin test
			await PerformWriteIntoBody(endResponseBodyByDisposingIt, httpSettings,
				senderData, mockHttpResponse, receiverData);

			// Test decode
			Assert.True(senderData.ToArray().SequenceEqual(receiverData.ToArray()));
			Assert.True(responseHeader.ContainsKey("Content-Length"));
			Assert.Equal(length, responseHeader.GetContentLength());
		}

		[Theory]
		[InlineData(4096, true)]
		[InlineData(4096, false)]
		[InlineData(8000, true)]
		[InlineData(8000, false)]
		async Task UseFixedLengthBodyIfContentLengthHeaderIsExplicitlySet(int length, bool endResponseBodyByDisposingIt)
		{
			// Mockup
			MockUp(length /* We'll overflow the internal buffer,
					to test see if it incorrectly pickup chunked encoding */,
				out HttpSettings httpSettings,
				out MemoryStream senderData,
				out Mock<IHttpResponse> mockHttpResponse,
				out MemoryStream receiverData,
				out HttpResponseHeader responseHeader);

			responseHeader["content-length"] = length.ToString(CultureInfo.InvariantCulture);

			// Begin test
			await PerformWriteIntoBody(endResponseBodyByDisposingIt, httpSettings,
				senderData, mockHttpResponse, receiverData);

			// Test decode
			Assert.True(senderData.ToArray().SequenceEqual(receiverData.ToArray()));
			Assert.True(responseHeader.ContainsKey("Content-Length"));
			Assert.Equal(length, responseHeader.GetContentLength());
		}

		[Theory]
		[InlineData(true, true)]
		[InlineData(true, false)]
		[InlineData(false, true)]
		[InlineData(false, false)]
		async Task EmptyMessageBody(bool withContentLengthHeader, bool endResponseBodyByDisposingIt)
		{
			// Mockup
			MockUp(0,
				out HttpSettings httpSettings,
				out MemoryStream senderData,
				out Mock<IHttpResponse> mockHttpResponse,
				out MemoryStream receiverData,
				out HttpResponseHeader responseHeader);

			if(withContentLengthHeader)
				responseHeader["content-length"] = "0";

			// Begin test
			await PerformWriteIntoBody(endResponseBodyByDisposingIt, httpSettings,
				senderData, mockHttpResponse, receiverData);

			if(withContentLengthHeader)
			{
				// Verify that the content-length header retained as 0
				// and there is no body either.
				Assert.Equal(0, receiverData.Length);
				Assert.Equal("0", responseHeader["content-Length"]);
			}
			else
			{
				// Verify that we won't see content-length header,
				// and there is no body either.
				Assert.Equal(0, receiverData.Length);
				Assert.False(responseHeader.ContainsKey("Content-Length"));
			}
		}

		[Fact]
		async Task CanUseDisposeAndCompleteTogether()
		{
			// Mockup
			MockUp(10000,
				out HttpSettings httpSettings,
				out MemoryStream senderData,
				out Mock<IHttpResponse> mockHttpResponse,
				out MemoryStream receiverData,
				out HttpResponseHeader responseHeader);

			// Begin test
			using(var body = new HttpResponseBody(
							receiverData,
							TcpSettings.Default,
							httpSettings,
							mockHttpResponse.Object))
			{
				await senderData.CopyToAsync(body, 4096);
				await body.CompleteAsync();
			}

			await VerifyReceivedChunkedMessage(senderData, receiverData);
		}

		[Fact]
		async Task ThrowExceptionIfWriteLessThanPromisedEncoding()
		{
			// Mockup
			MockUp(10000,
				out HttpSettings httpSettings,
				out MemoryStream senderData,
				out Mock<IHttpResponse> mockHttpResponse,
				out MemoryStream receiverData,
				out HttpResponseHeader responseHeader);

			responseHeader["content-length"] = 10001.ToString(CultureInfo.InvariantCulture);

			// Begin test
			var isInvalidOperationCaught = false;
			try
			{
				await PerformWriteIntoBody(false, httpSettings,
					senderData, mockHttpResponse, receiverData);
			}
			catch(InvalidOperationException)
			{
				isInvalidOperationCaught = true;
			}
			Assert.True(isInvalidOperationCaught);
		}

		[Fact]
		async Task LogWarningIfWriteLessThanPromisedEncodingOnDisposeMethod()
		{
			// Mockup
			MockUp(10000,
				out HttpSettings httpSettings,
				out MemoryStream senderData,
				out Mock<IHttpResponse> mockHttpResponse,
				out MemoryStream receiverData,
				out HttpResponseHeader responseHeader);
			var logger = new Mock<ILogger>();

			responseHeader["content-length"] = 10001.ToString(CultureInfo.InvariantCulture);

			// Begin test
			using(var body = new HttpResponseBody(
							receiverData,
							TcpSettings.Default,
							httpSettings,
							mockHttpResponse.Object,
							logger.Object))
			{
				await senderData.CopyToAsync(body);
			}
			// Check logger
			logger.Verify(
				l => l.Warn(It.IsAny<string>(), It.Is<Exception>(e => e is InvalidOperationException)),
				Times.Once);
		}

		[Fact]
		async Task WontBufferInMemoryIfContentLengthHeaderIsSet()
		{
			var header = new HttpResponseHeader();
			header["content-length"] = "100";

			var mockResponse = new Mock<IHttpResponse>();
			mockResponse.Setup(inst => inst.Header).Returns(header);

			var mockStream = new MemoryStream();
			var body = new HttpResponseBody(
					mockStream,
					TcpSettings.Default,
					HttpSettings.Default,
					mockResponse.Object
				);

			// Write 99 at the end
			await body.WriteAsync(new byte[] { 0x99 }, 1024);

			// Now verify that the response stream got 99 at the end,
			// as we set the content-length, data must go directly to the response stream
			// and won't buffered in memory.
			Assert.True(mockStream.ToArray().Last() == 0x99);
		}

		[Fact]
		async Task CanSendHeaderBeforeWritingIntoBody_WithContentLength()
		{
			// Mockup
			MockUp(100,
				out HttpSettings httpSettings,
				out MemoryStream senderData,
				out Mock<IHttpResponse> mockHttpResponse,
				out MemoryStream receiverData,
				out HttpResponseHeader header);

			mockHttpResponse.Setup(inst => inst.IsHeaderSent).Returns(true);
			header["content-length"] = "100";

			// Begin test
			await PerformWriteIntoBody(false, httpSettings,
				senderData, mockHttpResponse, receiverData, 1024);

			// Verify 
			Assert.True(senderData.ToArray().SequenceEqual(receiverData.ToArray()));
		}

		[Fact]
		async Task CanSendHeaderBeforeWritingIntoBody_WithContentLengthNoBodyForHeadRequest()
		{
			// Mockup
			MockUp(0,
				out HttpSettings httpSettings,
				out MemoryStream senderData,
				out Mock<IHttpResponse> mockHttpResponse,
				out MemoryStream receiverData,
				out HttpResponseHeader header);

			mockHttpResponse.Setup(inst => inst.IsHeaderSent).Returns(true);
			header["content-length"] = "100";

			// Begin test
			await PerformWriteIntoBody(false, httpSettings,
				senderData, mockHttpResponse, receiverData, 1024);

			// Verify 
			Assert.True(receiverData.Length == 0);
		}

		[Fact]
		async Task CanSendHeaderBeforeWritingIntoBody_Chunked()
		{
			// Mockup
			MockUp(100,
				out HttpSettings httpSettings,
				out MemoryStream senderData,
				out Mock<IHttpResponse> mockHttpResponse,
				out MemoryStream receiverData,
				out HttpResponseHeader header);

			mockHttpResponse.Setup(inst => inst.IsHeaderSent).Returns(true);
			header["tranfer-encoding"] = "chunked";

			// Begin test
			await PerformWriteIntoBody(false, httpSettings,
				senderData, mockHttpResponse, receiverData, 1024);

			await VerifyReceivedChunkedMessage(senderData, receiverData);
		}
	}
}
