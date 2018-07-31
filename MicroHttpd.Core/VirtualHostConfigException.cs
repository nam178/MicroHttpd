using System;
using System.Runtime.Serialization;

namespace MicroHttpd.Core
{
	[Serializable]
	sealed class VirtualHostConfigException : Exception
	{
		public VirtualHostConfigException()
		{
		}

		public VirtualHostConfigException(string message) : base(message)
		{
		}

		public VirtualHostConfigException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected VirtualHostConfigException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}