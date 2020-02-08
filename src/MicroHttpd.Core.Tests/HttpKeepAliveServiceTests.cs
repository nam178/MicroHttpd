using Moq;
using System;
using Xunit;

namespace MicroHttpd.Core.Tests
{
	public class HttpKeepAliveServiceTests
	{
		[Fact]
		public void CannotRegisterMoreThanGlobalLimit()
		{
			MockUp(out HttpKeepAliveService service);

			for(var i = 1; i < 20; i++)
			{
				var connection = Mock.Of<IDisposable>();
				if(i <= 10)
				{
					Assert.True(service.CanRegister(connection));
					service.Register(connection);
				}
				else
					Assert.False(
						service.CanRegister(connection),
						$"{nameof(service.CanRegister)} must return false when adding item #{i}"
						);
			}
		}

		static void MockUp(out HttpKeepAliveService service) => MockUp(out service, out _);

		static void MockUp(out HttpKeepAliveService service, out Mock<IWatchDog> mockWatchDog)
		{
			var httpSettings = HttpSettings.Default;
			httpSettings.MaxKeepAliveConnectionsGlobally = 10;
			mockWatchDog = new Mock<IWatchDog>();
			mockWatchDog
				.Setup(inst => inst.Watch(It.IsAny<IDisposable>()))
				.Returns(Mock.Of<IWatchDogSession>());
			service = new HttpKeepAliveService(mockWatchDog.Object, httpSettings);
		}

		[Fact]
		public void CorrectCount()
		{
			MockUp(out HttpKeepAliveService service);

			// add 3
			var t = Mock.Of<IDisposable>();
			service.Register(Mock.Of<IDisposable>());
			service.Register(Mock.Of<IDisposable>());
			service.Register(t);

			// Remove non-existing items
			service.Deregister(Mock.Of<IDisposable>());

			// Remove an existing item, twice
			service.Deregister(t);
			service.Deregister(t);

			// Now we should have 2, which means can add 8 more
			for(var i = 1; i <= 8; i++)
				service.Register(Mock.Of<IDisposable>());

			// But cannot add 9
			Assert.Throws<InvalidOperationException>(delegate
			{
				service.Register(Mock.Of<IDisposable>());
			});
		}

		[Fact]
		public void CannotRegisterTwice()
		{
			MockUp(out HttpKeepAliveService service);
			var i = new Mock<IDisposable>().Object;
			service.Register(i);
			Assert.Throws<InvalidOperationException>(delegate
			{
				service.Register(i);
			});
		}

		[Fact]
		public void OnlyRegisteredConnectionsAreWatchedByTheWatchDog()
		{
			MockUp(out HttpKeepAliveService service, out Mock<IWatchDog> watchDog);
			var mockWatchDogSession = new Mock<IWatchDogSession>();
			watchDog
				.Setup(inst => inst.Watch(It.IsAny<IDisposable>()))
				.Returns(mockWatchDogSession.Object);

			var t = new Mock<IDisposable>();
			service.Register(t.Object);

			watchDog.Verify(inst => inst.Watch(t.Object), Times.Once);
			mockWatchDogSession.Verify(inst => inst.Dispose(), Times.Never);

			service.Deregister(t.Object);
			watchDog.Verify(inst => inst.Watch(t.Object), Times.Once);
			mockWatchDogSession.Verify(inst => inst.Dispose(), Times.Once);
		}
    }
}
