namespace MicroHttpd.Core
{
	interface IHttpSessionFactory
	{
		IAsyncOperation Create();

		void Destroy(IAsyncOperation httpSession);
	}
}