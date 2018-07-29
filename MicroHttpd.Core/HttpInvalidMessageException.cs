using System;
using System.Runtime.Serialization;

namespace MicroHttpd.Core
{
	[Serializable]
	internal class HttpInvalidMessageException : Exception
	{
		public HttpInvalidMessageException()
		{
		}

		public HttpInvalidMessageException(string message) : base(message)
		{
		}

		public HttpInvalidMessageException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected HttpInvalidMessageException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}