using Autofac;
using Autofac.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

namespace MicroHttpd.Core
{
    /// <summary>
    ///  Use this to start/stop the HTTP server.
    /// </summary>
    /// <remarks>Not thread safe</remarks>
    public sealed class HttpServiceFacade : IHttpService
    {
		readonly List<IVirtualHostConfigReadOnly> _vhosts = new List<IVirtualHostConfigReadOnly>();
		readonly List<IModule> _autofacModules = new List<IModule>
		{
			new global::MicroHttpd.Core.Module()
		};

		TcpSettings _tcpSettings = TcpSettings.Default;
		HttpSettings _httpSettings = HttpSettings.Default;
		SslSettings _ssl;

		public static IHttpService Create() => new HttpServiceFacade();

		public void SetTcpSettings(TcpSettings tcpSettings)
		{
			Validation.RequireValidTcpSettings(tcpSettings);
			_tcpSettings = tcpSettings;
		}

		public void SetHttpSettings(HttpSettings httpSettings)
		{
			Validation.RequireValidHttpSettings(httpSettings);
			_httpSettings = httpSettings;
		}

		public void AddModule(IModule autofacModule)
		{
			RequireNotStarted();
			if(autofacModule == null)
				throw new ArgumentNullException(nameof(autofacModule));
			_autofacModules.Add(autofacModule);
		}

		public void AddVirtualHost(IVirtualHostConfigReadOnly virtualHostConfig)
		{
			if(virtualHostConfig == null)
				throw new ArgumentNullException(nameof(virtualHostConfig));
			RequireNotStarted();
			_vhosts.Add(virtualHostConfig);
		}

		IContainer _container;
		ManualResetEvent _waitHandle = new ManualResetEvent(false);

		public void Start()
		{
			RequireNotStarted();

			var containerBuilder = new ContainerBuilder();

			RegisterSettings(containerBuilder, _tcpSettings, _httpSettings);
			RegisterSSL(containerBuilder, _ssl);
			RegisterVHosts(containerBuilder, _vhosts.ToArray());
			RegisterModules(containerBuilder, _autofacModules);

			_waitHandle.Reset();

			// Start the TcpServer
			_container = containerBuilder.Build();
			_container.Resolve<Server>().Start(
				_container
					.Resolve<IEnumerable<IVirtualHostConfigReadOnly>>()
					.SelectMany(vhost => vhost.ListenOnPorts)
					.ToArray());
		}

		public void Stop()
		{
			// Kill the container,
			// This triggers the Dispose() method, stopping the TCP server.
			_container?.Dispose();
			_container = null;

			// Free the wait handle,
			// So Wait() method can return.
			_waitHandle.Set();
		}

		public void Wait() => _waitHandle.WaitOne();

		public void AddSSL(string pfxCertificate, string password, int port)
		{
			Validation.RequireValidPort(port);
			_ssl = new SslSettings
			{
				Cert = new X509Certificate2(
					File.ReadAllBytes(
						PathUtils.RelativeToAssembly(pfxCertificate)), 
						password),
				Port = port
			};
		}

		void RequireNotStarted()
		{
			if(_container != null)
				throw new InvalidOperationException();
		}

		static void RegisterModules(
			ContainerBuilder containerBuilder,
			IEnumerable<IModule> modules)
		{
			foreach(var module in modules)
				containerBuilder.RegisterModule(module);
		}

		static void RegisterVHosts(
			ContainerBuilder containerBuilder,
			IVirtualHostConfigReadOnly[] vhosts)
		{
			containerBuilder
				.RegisterInstance(vhosts)
				.AsSelf()
				.AsImplementedInterfaces()
				.SingleInstance();
		}

		static void RegisterSettings(
			ContainerBuilder containerBuilder,
			TcpSettings tcpSettings,
			HttpSettings httpSettings)
		{
			containerBuilder
				.Register(x => tcpSettings).AsSelf().SingleInstance();
			containerBuilder
				.Register(x => httpSettings).AsSelf().SingleInstance();
		}

		static void RegisterSSL(ContainerBuilder containerBuilder, SslSettings? sslSettings)
		{
			if(sslSettings != null)
			{
				var t = (SslSettings)sslSettings;
				containerBuilder.Register(x => t).AsSelf();
			}
		}
	}
}

