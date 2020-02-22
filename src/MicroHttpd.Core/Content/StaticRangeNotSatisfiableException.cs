using System;

namespace MicroHttpd.Core.Content
{
    [Serializable]
	sealed class StaticRangeNotSatisfiableException : Exception
	{
		public StaticRangeNotSatisfiableException()
		{
		}

		public StaticRangeNotSatisfiableException(string message) : base(message)
		{
		}

		public StaticRangeNotSatisfiableException(string message, Exception innerException) : base(message, innerException)
		{
		}
	}
}