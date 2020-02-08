using System.Collections.Generic;

namespace MicroHttpd.Core
{
	public interface IHttpHeaderReadOnly
	{
		string this[StringCI key]
		{ get; }

		string StartLine
		{ get; }

		IReadOnlyList<StringCI> Keys
		{ get; }

		bool ContainsKey(StringCI key);

		IReadOnlyList<string> Get(StringCI key, bool getAsCopy);
	}
}