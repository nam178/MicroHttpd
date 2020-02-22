using System;

namespace MicroHttpd.Core
{
	/// <summary>
	/// Represents session of a disposable object that watched by the IWatchDog.
	/// The session can be cancelled by disposing it
	/// </summary>
	public interface IWatchDogSession : IDisposable
	{
		/// <summary>
		/// This method prevents the watched IDisposable from being disposed
		/// by the watch dog, by resetting the timer.
		/// </summary>
		void Refresh();
	}
}
