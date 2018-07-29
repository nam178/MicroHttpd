using System;
using System.Threading;
using System.Timers;

namespace MicroHttpd.Core
{
	sealed class TimerImpl : ITimer
	{
		readonly global::System.Timers.Timer _timer;

		public event EventHandler Ticked;
		void OnTicked(EventArgs e) => Ticked?.Invoke(this, e);

		public bool AutoReset
		{
			get { return _timer.AutoReset; }
			set { _timer.AutoReset = value; }
		}

		public TimerImpl(TimeSpan interval)
		{
			_timer = new global::System.Timers.Timer(interval.TotalMilliseconds);
			_timer.Elapsed += Timer_Elapsed;
		}

		void Timer_Elapsed(object sender, ElapsedEventArgs e) => OnTicked(EventArgs.Empty);

		public void Start() => _timer.Start();

		public void Stop() => _timer.Stop();

		int _d;
		void IDisposable.Dispose()
		{
			if (Interlocked.CompareExchange(ref _d, 1, 0) == 0)
			{
				try
				{
					_timer.Stop();
					_timer.Dispose();
				}
				catch (Exception ex) {
					System.Console.Error.WriteLine(ex);
				}
			}
		}
	}
}
