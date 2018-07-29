using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace MicroHttpd.Core.Tests
{
	public class HttpRequestTests
    {
		[Fact]
		public async Task CanParsePostRequestWithContentLength()
		{
			var httpRequest = new HttpRequest(
				MockData.EmbededResource("MockData.HttpPostMessage1.txt"),
				TcpSettings.Default,
				new HttpRequestBodyFactory()
				);

			Assert.Throws<InvalidOperationException>(() => httpRequest.Header);
			Assert.Throws<InvalidOperationException>(() => httpRequest.Body);

			await httpRequest.WaitForHttpHeaderAsync();

			Assert.Equal("/auth/index?redirect=https%3A%2F%2Fau_alpha.catapultsports.com%2F", httpRequest.Header.Uri);
			using(var reader = new StreamReader(httpRequest.Body))
			{
				Assert.Equal(
					"All headers have now gone so this is the http message body", 
					await reader.ReadToEndAsync());
			}
				
		}

		[Fact]
		public async Task CanParseGetRequestWithContentLength()
		{
			var httpRequest = new HttpRequest(
				MockData.EmbededResource("MockData.HttpGetMessage1.txt"),
				TcpSettings.Default,
				new HttpRequestBodyFactory()
				);

			await httpRequest.WaitForHttpHeaderAsync();

			using(var reader = new StreamReader(httpRequest.Body))
				Assert.Equal(string.Empty, await reader.ReadToEndAsync());
		}

		[Fact]
		public async Task CanParseChunkedRequest()
		{
			var httpRequest = new HttpRequest(
					MockData.EmbededResource("MockData.HttpPostMessage2.txt"),
					TcpSettings.Default,
					new HttpRequestBodyFactory()
					);

			await httpRequest.WaitForHttpHeaderAsync();

			using(var reader = new StreamReader(httpRequest.Body))
				Assert.Equal("MozillaDeveloperNetwork", await reader.ReadToEndAsync());

		}

		[Fact]
		public async Task ThrowsExceptionWhenRequestEndsPrematurely()
		{
			var httpRequest = new HttpRequest(
				MockData.EmbededResource("MockData.HttpPostMessage3.txt"),
				TcpSettings.Default,
				new HttpRequestBodyFactory()
			);

			await httpRequest.WaitForHttpHeaderAsync();

			bool didThrowEx = false;
			try
			{
				using(var reader = new StreamReader(httpRequest.Body))
					await reader.ReadToEndAsync();
			}
			catch(Exception ex)
			{
				Assert.True(ex is HttpPrematureFinishException);
				didThrowEx = true;
			}

			Assert.True(didThrowEx);
		}
    }
}
