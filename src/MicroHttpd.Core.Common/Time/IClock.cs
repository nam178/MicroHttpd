using System;

namespace MicroHttpd.Core
{
	/// <summary>
	/// Interface around DateTime.UtcNow so time-dependent can be unit-tested easily.
	/// </summary>
	interface IClock
    {
		DateTime UtcNow { get; }
    }
}
