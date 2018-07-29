namespace MicroHttpd.Core
{
	interface IHttpSessionFactory
	{
		IAsyncExecutable Create();

		void Destroy(IAsyncExecutable httpSession);
	}
}