using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MicroHttpd.Core
{
	/// <summary>
	/// Collection of http header key - value,
	/// Notes: Http header keys are case insensitive.
	/// </summary>
	/// <remarks>Not thread safe.</remarks>
	sealed class HttpHeaderEntries :  System.Collections.IEnumerable
	{
		readonly Dictionary<StringCI, List<string>> _entries;
		readonly List<StringCI> _keys = new List<StringCI>();

		public IReadOnlyList<StringCI> Keys
		{ get { return _keys; } }

		public HttpHeaderEntries()
		{
			_entries = new Dictionary<StringCI, List<string>>();
		}

		public bool ContainsKey(StringCI key)
		{
			if (key == null)
				throw new ArgumentNullException(nameof(key));
			return _entries.ContainsKey(key);
		}

		public string GetFirst(StringCI key)
		{
			if (!_entries.ContainsKey(key) || _entries[key].Count == 0)
				throw new InvalidOperationException($"Key does not exist {key}");

			return _entries[key][0];
		}

		public IReadOnlyList<string> Get(StringCI key, bool getAsCopy)
		{
			if (!_entries.ContainsKey(key))
				throw new InvalidOperationException($"Key does not exist {key}");
			if(getAsCopy)
				return _entries[key].ToList();
			return _entries[key];
		}

		public void Remove(StringCI key)
		{
			if(ContainsKey(key))
			{
				_keys.Remove(key);
				_entries.Remove(key);
			}
		}

		public void Add(StringCI key, string value)
		{
			if (value == null)
				throw new ArgumentNullException(nameof(value));
			if (ContainsKey(key))
				_entries[key].Add(value);
			else
			{
				_entries[key] = new List<string>();
				_entries[key].Add(value);
				_keys.Add(key);
			}
		}

		public IEnumerator GetEnumerator() => _entries.GetEnumerator();
	}
}