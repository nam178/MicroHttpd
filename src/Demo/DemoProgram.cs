using MicroHttpd.Core;
using MicroHttpd.Core.StringMatch;

namespace Demo
{
	class DemoProgram
    {
		static void Main(string[] args)
		{

			// Create the HTTP server, 
			// it doesn't do anything until you configure and start it.
			var httpService = HttpServiceFacade.Create();

			// Configure the server to use SSL,
			// specifying path to the PFX file and its password.
			httpService.AddSSL(
				PathUtils.RelativeToAssembly("ssl/microhttp.localhost.pfx"), 
				",g9e/~ArH=aH.k8C",
				8443);

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
				ListenOnPorts = new int[] { 8443, 8080 }
			});

			// Start the server
			httpService.Start();

			// Wait, this prevents the console program from exiting.
			httpService.Wait();
		}
	}
}
