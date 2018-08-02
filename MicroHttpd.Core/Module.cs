using System;
using System.Collections.Generic;
using Autofac;
using MicroHttpd.Core.Content;

namespace MicroHttpd.Core
{
	sealed class Module : global::Autofac.Module
    {
		public enum Tags
		{
			/// <summary>
			/// The application always have 1 root life time scope.
			/// </summary>
			Root,

			/// <summary>
			/// The application may have multiple child TcpSession life time scopes,
			/// </summary>
			TcpSession,

			/// <summary>
			/// For each TcpSession, 
			/// there are one or more request-response http child scopes.
			/// </summary>
			HttpSession,
		}

		protected override void Load(ContainerBuilder builder)
		{
			base.Load(builder);

			RegisterGlobalSingleton(builder);
			RegisterPerTcpSessionSingleton(builder);
			RegisterPerHttpSessionSingleton(builder);
			RegisterPerDependencyInstances(builder);
		}

		static void RegisterGlobalSingleton(ContainerBuilder builder)
		{
			// We'll register 2 global watch dogs:
			// one to detect HTTP keep-alive idle, with a timeout from HttpSettings.
			// one to detect TCP read/write idle, with a timeout fro TcpSettings.
			builder
				.Register(x => CreateWatchDogWithSessionTTL(x, x.Resolve<HttpSettings>().KeepAliveTimeout))
				.Named<IWatchDog>("HTTP")
				.SingleInstance();
			builder
				.Register(x => CreateWatchDogWithSessionTTL(x, x.Resolve<TcpSettings>().IdleTimeout))
				.Named<IWatchDog>("TCP")
				.SingleInstance();

			// register HttpKeepAliveService as a singleton,
			// decorated with HttpKeepAliveServiceWatchDogDecorator
			builder
				.RegisterType<HttpKeepAliveService>()
				.WithParameter(
					(p, x) => p.ParameterType == typeof(IWatchDog),
					(p, x) => x.ResolveNamed<IWatchDog>("HTTP"))
				.AsImplementedInterfaces()
				.SingleInstance();

			// register TcpSessionInitializer as a singleton
			builder
				.RegisterType<TcpSessionInitializer>()
				.WithParameter( /* use the watch dog with TCP timeout */
					(p, x) => p.ParameterType == typeof(IWatchDog),
					(p, x) => x.ResolveNamed<IWatchDog>("TCP")
				)
				.AsSelf().AsImplementedInterfaces()
				.SingleInstance();

			// Singleton dictionary to allow looking up
			// MIME type by file extension
			builder
				.RegisterType<MimeTypeIndexByExtension>()
				.As<IReadOnlyDictionary<StringCI, MimeTypeEntry>>()
				.SingleInstance();

			// Contents
			builder.RegisterType<Content.StaticRange>().AsSelf();
			builder.RegisterType<Content.Static>().AsSelf();
			builder.RegisterType<Content.NoContent>().AsSelf();
			builder
				.Register(x => new Content.Aggregated(new IContent[]
				{
					// Order matters!
					x.Resolve<Content.StaticRange>(),
					x.Resolve<Content.Static>(),
					x.Resolve<Content.NoContent>()
				}))
				.AsImplementedInterfaces()
				.SingleInstance();

			// Register TcpSessionFactory as a singleton in global scope,
			// just to make sure it creates TcpSession as a direct
			// child of the root scope.
			builder
				.RegisterType<TcpSessionFactory>()
				.AsImplementedInterfaces()
				.SingleInstance();

			// SSL service
			// Singleton so it won't repeatedly resolve X509Certificate
			builder
				.RegisterType<SslService>()
				.AsImplementedInterfaces()
				.SingleInstance();

			// Content settings - it is read-only
			// so let's make it a singleton to avoid creating too many objects
			builder
				.RegisterType<ContentSettings>()
				.As<IContentSettingsReadOnly>()
				.SingleInstance();

			builder
				.RegisterType<HttpRequestBodyFactory>()
				.AsImplementedInterfaces()
				.SingleInstance();

			builder
				.RegisterType<StaticFileServer>()
				.AsSelf().AsImplementedInterfaces()
				.SingleInstance();
		}

		static void RegisterPerTcpSessionSingleton(ContainerBuilder builder)
		{
			// We'll have one HttpConnectionLoop per TCP connection
			builder
				.RegisterType<HttpConnectionLoop>()
				.AsSelf().AsImplementedInterfaces()
				.InstancePerMatchingLifetimeScope(Tags.TcpSession);

			builder
				.RegisterType<HttpSessionFactory>()
				.AsSelf().AsImplementedInterfaces()
				.InstancePerMatchingLifetimeScope(Tags.TcpSession);
		}

		static void RegisterPerHttpSessionSingleton(ContainerBuilder builder)
		{
			builder
				.RegisterType<HttpRequest>()
				.AsSelf().AsImplementedInterfaces()
				.InstancePerMatchingLifetimeScope(Tags.HttpSession);

			builder
				.RegisterType<HttpResponse>()
				.AsSelf().AsImplementedInterfaces()
				.InstancePerMatchingLifetimeScope(Tags.HttpSession);

			builder
				.RegisterType<HttpSession>()
				.AsSelf().AsImplementedInterfaces()
				.InstancePerMatchingLifetimeScope(Tags.HttpSession);
		}

		static void RegisterPerDependencyInstances(ContainerBuilder builder)
		{
			builder
				.RegisterType<TcpServer>()
				.AsSelf();

			builder
				.RegisterType<TcpListenerFactory>()
				.AsImplementedInterfaces();

			builder
				.RegisterType<TcpClientConnectedEventHandler>()
				.AsImplementedInterfaces();

			builder
				.RegisterType<SslService>()
				.AsImplementedInterfaces();

			builder.RegisterType<WatchDog>().AsSelf();

			builder.RegisterType<TimerFactory>().AsImplementedInterfaces();

			builder.RegisterType<Clock>().AsImplementedInterfaces();
		}

		static WatchDog CreateWatchDogWithSessionTTL(
			IComponentContext serviceLocator, 
			TimeSpan keepAliveTimeout)
		{
			var watchdog = serviceLocator.Resolve<WatchDog>();
			if(keepAliveTimeout.TotalSeconds <= 0)
				throw new ArgumentException(nameof(keepAliveTimeout));
			watchdog.MaxSessionDuration = keepAliveTimeout;
			return watchdog;
		}
	}
}
