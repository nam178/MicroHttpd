using Autofac;
using System;

namespace MicroHttpd.Core
{
	sealed class HttpSessionFactory : IHttpSessionFactory
	{
		readonly ILifetimeScope _tcpSessionLifetimeScope;

		public HttpSessionFactory(ILifetimeScope tcpSessionLifetimeScope)
		{
			_tcpSessionLifetimeScope = tcpSessionLifetimeScope 
				?? throw new ArgumentNullException(nameof(tcpSessionLifetimeScope));
		}

		public IAsyncOperation Create()
		{
			var child = _tcpSessionLifetimeScope.BeginLifetimeScope(Module.Tags.HttpSession);
			return new AsyncOperationWithLifeTimeScope(
				child,
				child.Resolve<HttpSession>()
				);
				
		}

		public void Destroy(IAsyncOperation httpSession)
			=> ((AsyncOperationWithLifeTimeScope)httpSession).LifetimeScope.Dispose();
	}
}
