using Autofac;
using System.IO;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace MicroHttpd.Core
{
	/// <summary>
	/// An SSL implementation that uses .NET SslStream
	/// </summary>
	sealed class SslService : ISslService
	{
		readonly X509Certificate2 _certificate;

		public bool IsAvailable
		{ get => _certificate != null; }

		public SslService(ILifetimeScope di)
		{
			if(di.IsRegistered<X509Certificate>())
				_certificate = di.Resolve<X509Certificate2>();
		}

		public Task<Stream> AddSslAsync(Stream src)
		{
			if(false == IsAvailable)
				throw new InvalidCredentialException("SSL has not been configured");
			return AuthenticateAsServerAsync(
				src,
				_certificate,
				SslProtocols.Default
				);
		}

		async Task<Stream> AuthenticateAsServerAsync(
			Stream src, 
			X509Certificate2 cert,
			SslProtocols allowedProtocols)
		{
			var sslStream = new SslStream(src, false);
			await sslStream.AuthenticateAsServerAsync(
				serverCertificate: cert,
				clientCertificateRequired: false,
				enabledSslProtocols: allowedProtocols,
				checkCertificateRevocation: true
				);
			return sslStream;
		}
	}
}
