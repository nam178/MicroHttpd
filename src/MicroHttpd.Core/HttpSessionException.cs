using System;
using System.Runtime.Serialization;

namespace MicroHttpd.Core
{
	[Serializable]
	internal class HttpSessionException : Exception
	{
		public HttpSessionException()
		{
		}

		public HttpSessionException(string message) : base(message)
		{
		}

		public HttpSessionException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected HttpSessionException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}