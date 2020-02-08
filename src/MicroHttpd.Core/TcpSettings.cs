using System;

namespace MicroHttpd.Core
{
	public struct TcpSettings
    {
		/// <summary>
		/// The maximum amount of time a TCP connection
		/// allowed to live without sending or receiving any data.
		/// </summary>
		public TimeSpan IdleTimeout
		{ get; set; }

		/// <summary>
		/// Maximum number of TCP clients we can have, application-wide.
		/// </summary>
		public int MaxTcpClients
		{ get; set; }

		/// <summary>
		/// Use the buffer of this size for each call to Stream.Read() and Stream.Write();
		/// </summary>
		public int ReadWriteBufferSize
		{ get; set; }

		public static TcpSettings Default
		{
			get
			{
				return new TcpSettings
				{
					IdleTimeout = TimeSpan.FromSeconds(60),
					MaxTcpClients = 1024,
					ReadWriteBufferSize = 1024 * 8
				};
			}
		}
	}
}
