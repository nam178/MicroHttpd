using Moq;
using System;
using Xunit;

namespace MicroHttpd.Core.Tests
{
    public class WatchDogTests
    {
        [Theory]
		[InlineData(5, false)]
		[InlineData(15, false)]
		[InlineData(25, false)]
		[InlineData(40, true)]
		[InlineData(50, true)]
		[InlineData(40, false, 25)]
		[InlineData(50, false, 25)]
		[InlineData(65, true, 25)]
		public void WatchDogCorrectlyDisposeTargets(double checkTime, bool expectDisposed, double refreshSessionAt = -1)
        {
			var mock = new MockUp();

			// Begin test
			using(var watchDog = new WatchDog(mock.TimerFactory.Object, mock.MockClock.Object))
			using(var session = watchDog.Watch(mock.Target.Object))
			{
				watchDog.MaxSessionDuration = TimeSpan.FromSeconds(30);
				Assert.True(mock.TimerInterval.TotalSeconds > 1);

				// 'Tick' the clock, with increased current time,
				// until we reach the check time
				while(mock.CurrentTime <= checkTime)
				{
					// Increase the time
					mock.CurrentTime += mock.TimerInterval.TotalSeconds;

					// Should we 'refresh' the session?
					if(refreshSessionAt > 0 && mock.CurrentTime >= refreshSessionAt)
					{
						refreshSessionAt = -1;
						session.Refresh();
					}

					mock.Timer.Raise(t => t.Ticked += null, EventArgs.Empty);
				}

				// Reached the check time, let's check.
				mock.Target.Verify(
					inst => inst.Dispose(),
					expectDisposed ? Times.Once() : Times.Never());
			}
		}

		[Fact]
		public void WatchDogWontDisposeTheTargetIfSessionIsAborted()
		{
			var mock = new MockUp();
			using(var watchDog = new WatchDog(mock.TimerFactory.Object, mock.MockClock.Object))
			{
				watchDog.MaxSessionDuration = TimeSpan.FromSeconds(30);

				var session = watchDog.Watch(mock.Target.Object);

				// First tick
				mock.CurrentTime += mock.TimerInterval.TotalSeconds;
				mock.Timer.Raise(t => t.Ticked += null, EventArgs.Empty);

				// Cancel the session
				session.Dispose();

				// Tick until ~1minutes
				while(mock.CurrentTime <= 60)
				{
					mock.CurrentTime += mock.TimerInterval.TotalSeconds;
					mock.Timer.Raise(t => t.Ticked += null, EventArgs.Empty);
				}

				// The target object must still alive
				mock.Target.Verify(inst => inst.Dispose(), Times.Never);
			}
		}

		class MockUp
		{
			public double CurrentTime
			{ get; set; }

			public Mock<IClock> MockClock
			{ get; set; }

			public TimeSpan TimerInterval
			{ get; set; }

			public Mock<ITimer> Timer
			{ get; set; }

			public Mock<ITimerFactory> TimerFactory
			{ get; set; }

			public Mock<IDisposable> Target;

			public MockUp()
			{
				// Mock ups
				CurrentTime = 0d;
				MockClock = new Mock<IClock>();
				MockClock
					.Setup(_ => _.UtcNow)
					.Returns(() => new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(CurrentTime));
				TimerInterval = TimeSpan.Zero;
				Timer = new Mock<ITimer>();
				TimerFactory = new Mock<ITimerFactory>();
				TimerFactory
					.Setup(inst => inst.Create(It.IsAny<TimeSpan>()))
					.Returns<TimeSpan>(interval =>
					{
						TimerInterval = interval;
						return Timer.Object;
					});
				Target = new Mock<IDisposable>();
			}
		}

		
    }
}
