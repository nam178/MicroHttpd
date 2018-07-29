using System;
using System.Runtime.Serialization;

namespace MicroHttpd.Core
{
	[Serializable]
	internal class TcpException : Exception
	{
		private Exception ex;

		public TcpException()
		{
		}

		public TcpException(Exception ex)
		{
			this.ex = ex;
		}

		public TcpException(string message) : base(message)
		{
		}

		public TcpException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected TcpException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}