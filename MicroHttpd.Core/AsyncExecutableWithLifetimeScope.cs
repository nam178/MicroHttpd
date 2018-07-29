using Autofac;
using System;
using System.Threading.Tasks;

namespace MicroHttpd.Core
{
	sealed class AsyncExecutableWithLifetimeScope : IAsyncExecutable
	{
		readonly IAsyncExecutable _original;

		public ILifetimeScope LifetimeScope
		{ get; }

		public AsyncExecutableWithLifetimeScope(
			ILifetimeScope scope,
			IAsyncExecutable original)
		{
			LifetimeScope = scope
				?? throw new ArgumentNullException(nameof(scope));
			_original = original
				?? throw new ArgumentNullException(nameof(original));
		}

		public Task ExecuteAsync() => _original.ExecuteAsync();
	}
}
