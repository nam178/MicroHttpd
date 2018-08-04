using log4net;
using log4net.Config;
using MicroHttpd.Core;
using MicroHttpd.Core.StringMatch;
using System.IO;
using System.Reflection;

namespace Demo
{
	class DemoProgram
    {
		static void Main(string[] args)
		{
			// Enable logging
			BasicConfigurator.Configure(
				LogManager.CreateRepository(
					Assembly.GetEntryAssembly(),
					typeof(log4net.Repository.Hierarchy.Hierarchy)));

			var httpService = HttpServiceFacade.Create();
			httpService.AddSSL(RelativeToAssembly("ssl/microhttp.localhost.pfx"), ",g9e/~ArH=aH.k8C");
			httpService.AddVirtualHost(new VirtualHostConfig
			{
				// We'll set document root to the www directory of this project.
				DocumentRoot = RelativeToAssembly("../../../www"),

				// Accept all host names
				HostName = new MatchAll(),

				// Accept incoming connections on port 8443
				ListenOnPorts = new int[] { 8443 }
			});

			// Start the server
			httpService.Start();

			// Wait, this prevents the console program from exiting.
			httpService.Wait();
		}

		static string RelativeToAssembly(string path)
		{
			return Path.GetFullPath(
				Path.Combine(
					Path.GetDirectoryName(
						Assembly.GetEntryAssembly().Location), 
						path));
		}
	}
}
