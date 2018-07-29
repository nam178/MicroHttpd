using System;
using System.Threading;

namespace MicroHttpd.Core
{
	sealed class WatchDogSession : IWatchDogSession, IDisposable
	{
		readonly Action<WatchDogSession> _disposer;
		readonly IClock _clock;
		readonly IDisposable _target;

		double _lastActivityUnixTimestamp;
		public double LastActivityUnixTimestamp
		{
			get {
				// Thread safety notes:
				// Reading this property probably done in another thread,
				// i.e. timer thread, so we do an atomic read
				return Interlocked.CompareExchange(ref _lastActivityUnixTimestamp, 0, 0);
			}
		}

		public WatchDogSession(
			IDisposable target,
			Action<WatchDogSession> disposer,
			IClock clock)
		{
			_target = target ?? throw new ArgumentNullException(nameof(target));
			_lastActivityUnixTimestamp = UnixTimestamp.FromDateTime(clock.UtcNow);
			_disposer = disposer ?? throw new ArgumentNullException(nameof(disposer));
			_clock = clock ?? throw new ArgumentNullException(nameof(clock));
		}

		public void Refresh()
		{
			// Thread safety notes:
			// Writing into this variable probably done from another 
			// thread than the thread reading it,
			// so we do an atomic write here.
			Interlocked.Exchange(
				ref _lastActivityUnixTimestamp, 
				UnixTimestamp.FromDateTime(_clock.UtcNow)
				);
		}

		int _purgeStatus;
		internal void DisposeTarget()
		{
			if(Interlocked.CompareExchange(ref _purgeStatus, 1, 0) == 0)
			{
				_target.Dispose();
			}
		}

		bool _isDisposed = false;
		public void Dispose()
		{
			if (_isDisposed)
				return;
			_isDisposed = true;
			_disposer(this);
		}
	}
}
