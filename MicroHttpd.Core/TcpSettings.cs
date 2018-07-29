using System;

namespace MicroHttpd.Core
{
	public struct TcpSettings
    {
		/// <summary>
		/// The TCP server kills the TCP connection if there is no 
		/// read/write activity on the underluing TCP stream for longer than this threshold.
		/// 
		/// Must be larger than HTTP keep-alive timeout value.
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
