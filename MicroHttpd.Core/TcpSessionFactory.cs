using Autofac;
using System;
using System.IO;

namespace MicroHttpd.Core
{
	sealed class TcpSessionFactory : ITcpSessionFactory
	{
		readonly ILifetimeScope _rootLifeTimeScope;

		public TcpSessionFactory(ILifetimeScope rootLifeTimeScope)
		{
			_rootLifeTimeScope = rootLifeTimeScope 
				?? throw new ArgumentNullException(nameof(rootLifeTimeScope));
		}

		public IAsyncExecutable Create(ITcpClient client, Stream connection)
		{
			var child = _rootLifeTimeScope.BeginLifetimeScope(
				Module.Tags.TcpSession, 
				x => AddInstanceToRegistration(x, client, connection));
			return new AsyncExecutableWithLifetimeScope(
				child,
				child.Resolve<HttpConnectionLoop>()
				);
		}

		public void Destroy(IAsyncExecutable tcpSession)
		{
			// We just need to dispose the LifetimeScope,
			// it will then dispose all services resolved from it.
			((AsyncExecutableWithLifetimeScope)tcpSession).LifetimeScope.Dispose();
		}

		static void AddInstanceToRegistration(ContainerBuilder builder, ITcpClient client, Stream stream)
		{
			builder
				.RegisterInstance(client)
				.AsSelf()
				.AsImplementedInterfaces()
				.ExternallyOwned() // we don't own this ITcpClient, the caller does.
				.SingleInstance(); // Single instance for this TCP session

			builder
				.RegisterInstance(stream)
				.AsSelf()
				.AsImplementedInterfaces()
				.ExternallyOwned() // we don't own this ITcpClient, the caller does.
				.SingleInstance(); // Single instance for this TCP session
		}
	}
}
