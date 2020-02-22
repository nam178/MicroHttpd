using System;

namespace MicroHttpd.Core
{
	/// <summary>
	/// Creates a timer. We'll use this in the unit test project to create a fake timer.
	/// </summary>
	interface ITimerFactory
	{
		ITimer Create(TimeSpan interval);
	}
}
