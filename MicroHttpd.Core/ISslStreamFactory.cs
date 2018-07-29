using System.IO;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace MicroHttpd.Core
{
	/// <summary>
	/// Represents an SSL implementation that can convert a plain text stream into an SSL stream.
	/// </summary>
	interface ISsl
	{
		/// <summary>
		/// Establish SSL connection around the provided stream, 
		/// act as a server and supply the provided certificate.
		/// </summary>
		Task<Stream> AuthenticateAsServerAsync(
			Stream plainTextStream, 
			X509Certificate2 cert,
			SslProtocols allowedProtocols
			);
	}
}
