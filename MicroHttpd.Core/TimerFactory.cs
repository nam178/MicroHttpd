using System;

namespace MicroHttpd.Core
{
	sealed class TimerFactory : ITimerFactory
	{
		public ITimer Create(TimeSpan interval) => new TimerImpl(interval);
	}
}
