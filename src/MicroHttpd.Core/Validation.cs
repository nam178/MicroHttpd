using System;

namespace MicroHttpd.Core
{
	/// <summary>
	/// Validation helper
	/// </summary>
	static class Validation
    {
		/// <summary>
		/// Validate the provided 'start' and 'count' argument so that
		/// we can have a valid buffer starting from 'start' with length of 'count'
		/// </summary>
		public static void RequireValidBuffer(byte[] buffer, int start, int count)
		{
			if(buffer == null)
				throw new System.ArgumentNullException(nameof(buffer));
			if(start < 0 || start >= buffer.Length)
				throw new System.ArgumentException(nameof(start));
			if(count <= 0)
				throw new System.ArgumentException(nameof(count));
			if((start + count) > buffer.Length)
				throw new System.ArgumentException(nameof(count));
		}

		/// <summary>
		/// Check whenever the specified port is a valid port number
		/// </summary>
		public static void RequireValidPort(int port)
		{
			if(false == (port > 0 && port <= ushort.MaxValue))
				throw new ArgumentException(nameof(port));
		}

		/// <summary>
		/// Validate the provided HttpSettings and throw exception when fail.
		/// </summary>
		public static void RequireValidHttpSettings(HttpSettings httpSettings)
		{
			RequireValidBufferSize(httpSettings.MaxBodyChunkSize);
			RequireValidBufferSize(httpSettings.MaxBodySizeInMemory);
			RequireNonNegative(httpSettings.MaxKeepAliveConnectionsGlobally);
			RequirePositive(httpSettings.KeepAliveTimeout);
		}

		/// <summary>
		/// Validate the provided TcpSettings
		/// </summary>
		public static void RequireValidTcpSettings(TcpSettings tcpSettings)
		{
			if(tcpSettings.ReadWriteBufferSize <= 0 
				|| tcpSettings.ReadWriteBufferSize >= (8 * 1024 * 1024))
			{
				throw new ArgumentOutOfRangeException(nameof(tcpSettings.ReadWriteBufferSize));
			}
		}

		/// <summary>
		/// Validate chunk length
		/// </summary>
		public static void RequireChunkLengthWithinLimit(long chunkLength)
		{
			if(chunkLength < 0)
				throw new ArgumentException(
					$"Chunk length cannot be negative {chunkLength}"
					);

			// TODO
			// Should we have any other maximum limit for the chunk length?
			// We already have limit for request body, is that enough?

			if(chunkLength > global::System.Int32.MaxValue)
				throw new ArgumentException(
					$"Chunk length too large: {chunkLength}"
					);
		}

		/// <summary>
		/// Validate buffer size used for reading/writing data
		/// </summary>
		public static void RequireValidBufferSize(int bufferSize)
		{
			if(false == (bufferSize > 0 && bufferSize < Int32.MaxValue))
				throw new ArgumentException(
					$"Invalid buffer size: {bufferSize}b"
					);
		}

		/// <summary>
		/// Throw ArgumentException if provided number is negative
		/// </summary>
		public static void RequireNonNegative(long number)
		{
			if(number < 0)
				throw new ArgumentException(nameof(number));
		}

		/// <summary>
		/// Throw ArgumentException if provided number is negative
		/// </summary>
		public static void RequireNonNegative(int number)
		{
			if(number < 0)
				throw new ArgumentException(nameof(number));
		}

		/// <summary>
		/// Throw ArgumentException if provided number is negative or zero
		/// </summary>
		public static void RequirePositive(TimeSpan timeSpan)
		{
			if(timeSpan.TotalSeconds <= 0)
				throw new ArgumentException(nameof(timeSpan));
		}
	}
}
