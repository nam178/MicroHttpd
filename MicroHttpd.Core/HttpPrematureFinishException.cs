using System;
using System.Runtime.Serialization;

namespace MicroHttpd.Core
{
	[Serializable]
	class HttpPrematureFinishException : HttpInvalidMessageException
	{
		public HttpPrematureFinishException()
		{
		}

		public HttpPrematureFinishException(string message) : base(message)
		{
		}

		public HttpPrematureFinishException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected HttpPrematureFinishException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public static void ThrowIfZero(int bytesReadFromStream)
		{
			if(bytesReadFromStream <= 0)
				throw new HttpPrematureFinishException(
					"Invalid HTTP message - other party finished prematurely"
					);
		}
	}
}