using log4net;
using log4net.Config;
using MicroHttpd.Core;
using MicroHttpd.Core.StringMatch;
using System.Reflection;

namespace Demo
{
	class DemoProgram
    {
		static void Main(string[] args)
		{
			// Enable logging if you like
			BasicConfigurator.Configure(
				LogManager.CreateRepository(
					Assembly.GetEntryAssembly(),
					typeof(log4net.Repository.Hierarchy.Hierarchy)));

			// Create the HTTP server, 
			// it doesn't do anything until you configure and start it.
			var httpService = HttpServiceFacade.Create();

#if SSL
			// Configure the server to use SSL,
			// specifying path to the PFX file and its password.
			httpService.AddSSL(
				PathUtils.RelativeToAssembly("ssl/cert.pfx"), 
				",g9e/~ArH=aH.k8C");
#endif

			// Configure virtual host - the domain name,
			// the web root and ports.
			httpService.AddVirtualHost(new VirtualHostConfig
			{
				// We'll set document root to the www directory of this project.
				DocumentRoot = PathUtils.RelativeToAssembly("www"),

				// Accept all host names,
				// Other than MatchAll, you can also use
				// MicroHttpd.Core.StringMatch.Regex, or,
				// MicroHttpd.Core.StringMatch.ExactCaseSensitive
				HostName = new MatchAll(),

				// Accept incoming connections on port 8443,
				// If you want to use port 443 and/or 80, add them here.
				// (You'll need to start the application as root)
				ListenOnPorts = new int[] { 8443 }
			});

			// Start the server
			httpService.Start();

			// Wait, this prevents the console program from exiting.
			httpService.Wait();
		}
	}
}
