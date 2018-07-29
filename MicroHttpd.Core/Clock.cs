using System;

namespace MicroHttpd.Core
{
	sealed class Clock : IClock
	{
		public DateTime UtcNow => DateTime.UtcNow;
	}
}
