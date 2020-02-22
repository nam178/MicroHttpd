using System;

namespace MicroHttpd.Core
{
	public struct HttpSettings
	{
		public TimeSpan KeepAliveTimeout { get; set; }

		public int MaxKeepAliveConnectionsGlobally { get; set; }

		/// <summary>
		/// In chunked response, dynamic sized message 
		/// are broken down into smaller chunks of this size.
		/// </summary>
		public int MaxBodyChunkSize { get; set; }

		/// <summary>
		/// Writes to http message is held in memory until exceed this size.
		/// </summary>
		public int MaxBodySizeInMemory { get; set; }

		public static HttpSettings Default
		{
			get
			{
				return new HttpSettings
				{
					MaxKeepAliveConnectionsGlobally = 128,
					KeepAliveTimeout = TimeSpan.FromSeconds(15),
					MaxBodyChunkSize = 8 * 1024,
					MaxBodySizeInMemory = 4 * 1024
				};
			}
		}

		public static void Validate(HttpSettings value)
		{
			Validation.RequireValidBufferSize(value.MaxBodyChunkSize);
			Validation.RequireValidBufferSize(value.MaxBodySizeInMemory);
			Validation.RequireNonNegative(value.MaxKeepAliveConnectionsGlobally);
			Validation.RequirePositive(value.KeepAliveTimeout);
		}
    }
}
