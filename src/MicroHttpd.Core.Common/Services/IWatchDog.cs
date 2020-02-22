using System;

namespace MicroHttpd.Core
{
	/// <summary>
	/// A tool that helps disposing IDisposable objects if they are idle for a period of time.
	/// </summary>
	public interface IWatchDog : IDisposable
	{
		/// <summary>
		/// Objects being watched by the Watch() method are disposed if they are idle longer than this threshold.
		/// </summary>
		TimeSpan MaxSessionDuration { get; set; }

		/// <summary>
		/// Watch the specified disposable object, unless you call Refresh() from the returned
		/// session object, the provided IDisposable will be disposed after SessionDuration.
		/// </summary>
		IWatchDogSession Watch(IDisposable member);
	}
}
