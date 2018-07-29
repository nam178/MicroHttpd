using Autofac.Core;

namespace MicroHttpd.Core
{
	public interface IHttpService
	{
		/// <summary>
		/// Use the specified TCP settings
		/// </summary>
		void SetTcpSettings(TcpSettings tcpSettings);

		/// <summary>
		/// Use the specified HTTP settings
		/// </summary>
		void SetHttpSettings(HttpSettings httpSettings);

		/// <summary>
		/// Configure vhost
		/// </summary>
		void AddVirtualHost(IVirtualHostConfigReadOnly virtualHostConfig);

		/// <summary>
		/// Configure SSL
		/// </summary>
		/// <param name="pfxCertificate">Relative or absolute path to a PFX certificate</param>
		/// <param name="password">Password for the PFX certificate</param>
		void AddSSL(string pfxCertificate, string password);

		/// <summary>
		/// Add an autofac module to extend or override the HTTP server's functionalities.
		/// </summary>
		void AddModule(IModule autofacModule);

		/// <summary>
		/// Start the HTTP server
		/// </summary>
		void Start();

		/// <summary>
		/// Stop the HTTP server
		/// </summary>
		void Stop();

		/// <summary>
		/// Usually you will want to wait for the service to run till completion,
		/// i.e. prevent the console program from exiting.
		/// </summary>
		void Wait();
	}
}