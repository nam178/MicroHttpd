using System.Collections.Generic;

namespace MicroHttpd.Core
{
	public interface IVirtualHostConfigReadOnly
	{
		IStringMatch HostName
		{ get; }

		string DocumentRoot
		{ get; }

		IReadOnlyList<int> ListenOnPorts
		{ get; }
	}
}