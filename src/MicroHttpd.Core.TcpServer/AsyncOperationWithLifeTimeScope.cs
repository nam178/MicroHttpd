using Autofac;
using System;
using System.Threading.Tasks;

namespace MicroHttpd.Core
{
	sealed class AsyncOperationWithLifeTimeScope : IAsyncOperation
	{
		readonly IAsyncOperation _original;

		public ILifetimeScope LifetimeScope
		{ get; }

		public AsyncOperationWithLifeTimeScope(
			ILifetimeScope scope,
			IAsyncOperation original)
		{
			LifetimeScope = scope
				?? throw new ArgumentNullException(nameof(scope));
			_original = original
				?? throw new ArgumentNullException(nameof(original));
		}

		public Task ExecuteAsync() => _original.ExecuteAsync();
	}
}
