using System;
using System.Collections.Generic;

namespace MicroHttpd.Core
{
	/// <summary>
	/// Represents the header part of the HTTP message.
	/// </summary>
	/// <remarks>Not thread safe.</remarks>
	abstract class HttpHeader
	{
		readonly HttpHeaderEntries _entries = new HttpHeaderEntries();

		string _cachedAsPlainTextValue = null;
		public string AsPlainText
		{
			get
			{
				if(_cachedAsPlainTextValue == null)
					_cachedAsPlainTextValue = _entries.GeneratePlainHeader(StartLine);
				return _cachedAsPlainTextValue;
			}
		}

		string _startLine = String.Empty;
		public virtual string StartLine
		{
			get { return _startLine; }
			set
			{
				if(string.IsNullOrWhiteSpace(value))
					throw new ArgumentException(nameof(value));
				if(value == _startLine)
					return;
				_startLine = value;
				InvalidCachedPlainText();
			}
		}

		public string this[StringCI key]
		{
			get {
				var values = Get(key, false);
				if(values.Count == 0)
					throw new IndexOutOfRangeException(nameof(key));
				return values[0];
			}
			set {
				if(ContainsKey(key)) Remove(key);
				Add(key, value);
			}
		}

		public IReadOnlyList<StringCI> Keys => _entries.Keys;

		protected HttpHeader(
			string startLine = null,
			HttpHeaderEntries entries = null)
		{
			_startLine = startLine ?? String.Empty;
			if(entries != null)
				_entries.CopyEntries(from: entries);
		}

		public bool ContainsKey(StringCI key) => _entries.ContainsKey(key);

		public string GetFirst(StringCI key) => _entries.GetFirst(key);

		public IReadOnlyList<string> Get(StringCI key, bool getAsCopy)
			=> _entries.Get(key, getAsCopy);

		public virtual void Add(StringCI key, string value)
		{
			_entries.Add(key, value);
			InvalidCachedPlainText();
		}

		public virtual void Remove(StringCI key)
		{
			_entries.Remove(key);
			InvalidCachedPlainText();
		}

		protected void InvalidCachedPlainText() => _cachedAsPlainTextValue = null;
	}
}
