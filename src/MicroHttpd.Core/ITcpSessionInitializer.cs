namespace MicroHttpd.Core
{
	/// <summary>
	/// Responsible for the creation and execution of a TCP session,
	/// for instance it's the HTTP connection loop.
	/// </summary>
	/// <see cref="HttpConnectionLoop"/>
	/// <see cref="TcpSessionInitializer"/>
	/// <remarks>Designed to be a singleton, thread safe</remarks>
	interface ITcpSessionInitializer
	{
		/// <summary>
		/// Have we reached maximum number of alive TCP connections?
		/// </summary>
		bool IsLimitReached
		{ get; }

		/// <summary>
		/// Initialize a new TCP session, 
		/// caller should check IsLimitreached property prior calling this method.
		/// 
		/// This instance takes ownership of the provided TCP client and is responsible
		/// for disposing it once the session finishes.
		/// </summary>
		/// <param name="client">The TCP client.</param>
		void InitializeNewTcpSession(ITcpClient client); 
	}
}