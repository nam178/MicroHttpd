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
	sealed class SslImpl : ISsl
	{
		public async Task<Stream> AuthenticateAsServerAsync(
			Stream plainTextStream, 
			X509Certificate2 cert,
			SslProtocols allowedProtocols)
		{
			var sslStream = new SslStream(plainTextStream, false);
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
