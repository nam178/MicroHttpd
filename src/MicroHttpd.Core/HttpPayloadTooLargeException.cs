using System;
using System.Runtime.Serialization;

namespace MicroHttpd.Core
{
	[Serializable]
	internal class HttpPayloadTooLargeException : Exception
	{
		public HttpPayloadTooLargeException()
		{
		}

		public HttpPayloadTooLargeException(string message) : base(message)
		{
		}

		public HttpPayloadTooLargeException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected HttpPayloadTooLargeException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}