using Moq;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace MicroHttpd.Core.Tests
{
	public class HttpResponseTests
    {
		[Fact]
		public async Task CompleteReadingRequestBeforeWrittingResponse()
		{
			var body = MockData.MockNetworkStream(1024 * 16 + 88);
			var request = new Mock<IHttpRequest>();
			request.Setup(inst => inst.Body)
				.Returns(new ReadOnlyStreamAdapter(body));

			var response = new HttpResponse(
				request.Object,
				new MemoryStream(),
				TcpSettings.Default,
				HttpSettings.Default
				);

			await response.SendHeaderAsync();

			Assert.Equal(body.Length, body.Position);
		}
	}
}
