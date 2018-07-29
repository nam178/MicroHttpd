using System;
using System.Runtime.Serialization;

namespace MicroHttpd.Core
{
	[Serializable]
	public class HttpHeaderAlreadySentException : InvalidOperationException
	{
		public HttpHeaderAlreadySentException()
		{
		}

		public HttpHeaderAlreadySentException(string message) : base(message)
		{
		}

		public HttpHeaderAlreadySentException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected HttpHeaderAlreadySentException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}