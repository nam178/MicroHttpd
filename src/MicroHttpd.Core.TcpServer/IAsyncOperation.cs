using System.Threading.Tasks;

namespace MicroHttpd.Core
{
	interface IAsyncOperation
    {
		Task ExecuteAsync();
    }
}
