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

		public IAsyncExecutable Create()
		{
			var child = _tcpSessionLifetimeScope.BeginLifetimeScope();
			return new AsyncExecutableWithLifetimeScope(
				child,
				child.Resolve<HttpSession>()
				);
				
		}

		public void Destroy(IAsyncExecutable httpSession)
			=> ((AsyncExecutableWithLifetimeScope)httpSession).LifetimeScope.Dispose();
	}
}
