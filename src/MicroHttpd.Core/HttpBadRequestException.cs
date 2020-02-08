using System;
using System.Runtime.Serialization;

namespace MicroHttpd.Core
{
	[Serializable]
	class HttpBadRequestException : Exception
	{
		public HttpBadRequestException()
		{
		}

		public HttpBadRequestException(string message) : base(message)
		{
		}

		public HttpBadRequestException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected HttpBadRequestException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}