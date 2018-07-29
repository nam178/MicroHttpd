using System.Threading.Tasks;

namespace MicroHttpd.Core
{
	interface IAsyncExecutable
    {
		Task ExecuteAsync();
    }
}
