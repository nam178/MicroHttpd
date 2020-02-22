using Autofac;
using MicroHttpd.Core.TcpServer.IoC;
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

		public IAsyncOperation Create(ITcpClient client, Stream connection)
		{
			var child = _rootLifeTimeScope.BeginLifetimeScope(
				Tags.TCP_SESSION, 
				x => AddInstanceToRegistration(x, client, connection));
			return new AsyncOperationWithLifeTimeScope(
				child,
				child.Resolve<IAsyncOperation>()
				);
		}

		public void Destroy(IAsyncOperation tcpSession)
		{
			// We just need to dispose the LifetimeScope,
			// it will then dispose all services resolved from it.
			((AsyncOperationWithLifeTimeScope)tcpSession).LifetimeScope.Dispose();
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
				.As<Stream>()
				.ExternallyOwned() // we don't own this stream, the caller does.
				.SingleInstance(); // Single instance for this TCP session
		}
	}
}
