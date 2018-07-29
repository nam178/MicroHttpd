namespace MicroHttpd.Core
{
	interface ITcpKeepAlivePolicyReadOnly
	{
		/// <summary>
		/// Check the policy to see if we can keep one more client alive.
		/// </summary>
		bool CanKeepAlive();
	}
}