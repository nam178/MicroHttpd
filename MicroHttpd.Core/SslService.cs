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
		readonly SslSettings[] _sslSettings;

		public SslService(ILifetimeScope di)
		{
			if(di.IsRegistered<SslSettings[]>())
				_sslSettings = di.Resolve<SslSettings[]>();
			else
				_sslSettings = new SslSettings[0];
		}

		public async Task<Stream> WrapSslAsync(ITcpClient client, Stream stream)
		{
			for(var i = 0; i < _sslSettings.Length; i++)
			{
				if(_sslSettings[i].Port == client.LocalPort)
				{
					return await AuthenticateAsServerAsync(
						stream,
						_sslSettings[i].Cert,
						SslProtocols.Default
					);
				}
			}
			return null;
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
