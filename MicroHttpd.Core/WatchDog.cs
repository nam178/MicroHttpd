using log4net;
using System;
using System.Collections.Generic;

namespace MicroHttpd.Core
{
	sealed class WatchDogImpl : IDisposable, IWatchDog
	{
		readonly ILog _logger;
		readonly List<WatchDogSession> _sessions = new List<WatchDogSession>();
		readonly object _syncRoot = new object();
		readonly ITimer _timer;
		readonly IClock _clock;

		const int CheckIntervalSeconds = 3;
		const int DefaultMaxSessionDuration = 60;

		public TimeSpan MaxSessionDuration
		{ get; set; } = TimeSpan.FromSeconds(DefaultMaxSessionDuration);

		public WatchDogImpl(ILog logger, ITimerFactory timerFactory, IClock clock)
		{
			if(timerFactory == null)
				throw new ArgumentNullException(nameof(timerFactory));
			_logger = logger 
				?? throw new ArgumentNullException(nameof(logger));
			_clock = clock 
				?? throw new ArgumentNullException(nameof(clock));
			_timer = timerFactory.Create(TimeSpan.FromSeconds(CheckIntervalSeconds));
			_timer.Ticked += Timer_Ticked;
			_timer.Start();
		}

		bool _isTimerTicking = false;
		void Timer_Ticked(object sender, EventArgs e)
		{
			lock(_syncRoot)
			{
				if (_isDispsoed || _isTimerTicking)
					return;
				_isTimerTicking = true;
			}
			
			try
			{
				Purge();
			}
			catch (Exception ex) {
				_logger.Error(ex);
			}
			finally {
				lock(_syncRoot)
				{
					_isTimerTicking = false;
				}
			}
		}

		public IWatchDogSession Watch(IDisposable member)
		{
			if (member == null)
				throw new ArgumentNullException(nameof(member));

			var membership = new WatchDogSession(member, Remove, _clock);
			lock(_syncRoot)
			{
				_sessions.Add(membership);
			}
			return membership;
		}

		void Remove(WatchDogSession session)
		{
			if (session == null)
				throw new ArgumentNullException(nameof(session));
			lock(_syncRoot)
			{
				if(_sessions.Contains(session))
					_sessions.Remove(session);
			}
		}

		void Purge()
		{
			// Go ahead and remove all outdated members
			var remain = new List<WatchDogSession>();
			var now = UnixTimestamp.FromDateTime(_clock.UtcNow);

			lock (_syncRoot)
			{
				foreach (var session in _sessions.ToArray())
				{
					var age = now - session.LastActivityUnixTimestamp;

					if (age > MaxSessionDuration.TotalSeconds)
					{
						session.DisposeTarget();
						session.Dispose();
					}
					else
						remain.Add(session);
				}
				_sessions.Clear();
				_sessions.AddRange(remain);
			}
		}

		bool _isDispsoed = false;
		public void Dispose()
		{
			lock (_syncRoot)
			{
				// Never do a dispose twice.
				if (_isDispsoed)
					return;
				_isDispsoed = true;
			}

			// Dispose all members
			var t = _sessions.ToArray();
			foreach(var member in t)
				member.Dispose();

			// Stop and dispose the timer
			_timer.Stop();
			_timer.Ticked -= Timer_Ticked;
			_timer.Dispose();
		}
	}
}
