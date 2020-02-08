using System;

namespace MicroHttpd.Core
{
	interface IHttpKeepAliveService
	{
		bool CanRegister(IDisposable connection);

		void Register(IDisposable connection);

		bool IsRegistered(IDisposable connection);

		void Deregister(IDisposable connection);
	}
}