using System;

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
	}
}