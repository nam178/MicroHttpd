using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace MicroHttpd.Core
{
	struct SslSettings : IEquatable<SslSettings>
	{
		public int Port
		{ get; set; }

		public X509Certificate2 Cert
		{ get; set; }

		public static SslSettings None
		{ get { return new SslSettings(); } }

		public static bool operator ==(SslSettings x, SslSettings y)
		{
			return x.Equals(y);
		}

		public static bool operator !=(SslSettings x, SslSettings y)
		{
			return false == x.Equals(y);
		}

		public override bool Equals(object obj)
		{
			return obj is SslSettings && Equals((SslSettings)obj);
		}

		public bool Equals(SslSettings other)
		{
			return Port == other.Port &&
				   EqualityComparer<X509Certificate2>.Default.Equals(Cert, other.Cert);
		}

		public override int GetHashCode()
		{
			var hashCode = 1734322649;
			hashCode = hashCode * -1521134295 + Port.GetHashCode();
			hashCode = hashCode * -1521134295 + EqualityComparer<X509Certificate2>.Default.GetHashCode(Cert);
			return hashCode;
		}
	}
}
