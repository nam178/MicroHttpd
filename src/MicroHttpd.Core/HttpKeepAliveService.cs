using System;
using System.Collections.Generic;
using System.Threading;

namespace MicroHttpd.Core
{
	/// <remarks>
	/// Designed to be a global singleton, thread safe.
	/// </remarks>
	/// <see cref="IHttpKeepAliveService"/>
	sealed class HttpKeepAliveService : IHttpKeepAliveService
	{
		readonly IWatchDog _watchDog;
		readonly HttpSettings _httpSettings;
		readonly Dictionary<IDisposable, IWatchDogSession> _watchedConnections; // Registered connection -> watch dog session
		readonly object _sync = new object();

		int _total;

		public HttpKeepAliveService(IWatchDog watchDog, HttpSettings httpSettings)
		{
			Validation.RequireValidHttpSettings(httpSettings);
			_watchDog = watchDog
				?? throw new ArgumentNullException(nameof(watchDog));
			_httpSettings = httpSettings;
			_watchedConnections
				= new Dictionary<IDisposable, IWatchDogSession>();
		}

		public bool CanRegister(IDisposable connection)
		{
			return Interlocked.CompareExchange(ref _total, 0, 0) 
				< _httpSettings.MaxKeepAliveConnectionsGlobally;
		}

		public void Deregister(IDisposable connection)
		{
			if(connection == null)
				throw new ArgumentNullException(nameof(connection));
			IDisposable watchDogSession = null;
			lock(_sync)
			{
				if(_watchedConnections.ContainsKey(connection))
				{
					watchDogSession = _watchedConnections[connection];
					_watchedConnections.Remove(connection);
				}
			}

			// Decrease the total count if the connection was removed
			if(watchDogSession != null)
			{
				watchDogSession.Dispose();
				Interlocked.Decrement(ref _total);
			}
		}

		public bool IsRegistered(IDisposable connection)
		{
			if(connection == null)
				throw new ArgumentNullException(nameof(connection));
			lock(_sync)
			{
				return _watchedConnections.ContainsKey(connection);
			}
		}

		public void Register(IDisposable connection)
		{
			if(connection == null)
				throw new ArgumentNullException(nameof(connection));

			// Can we add?
			if(false == CanRegister(connection))
				throw new InvalidOperationException("Already registered");

			bool didAdd = false;

			// As we are calling external code within lock,
			// Let's enter the lock with a proper timeout.
			if(false == Monitor.TryEnter(_sync, TimeSpan.FromSeconds(15)))
				throw new Exception("Potentially deadlock");
			try
			{
				if(_watchedConnections.ContainsKey(connection))
					throw new InvalidOperationException("Already registered");
				_watchedConnections[connection] = _watchDog.Watch(connection);
				didAdd = true;
			} finally {
				Monitor.Exit(_sync);
			}

			// Increase the count in case of a successful add.
			if(didAdd)
				Interlocked.Increment(ref _total);
		}
	}
}
