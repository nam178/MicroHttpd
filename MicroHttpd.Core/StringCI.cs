using System;
using System.Globalization;

namespace MicroHttpd.Core
{
	/// <summary>
	/// Like System.string, but case-insensitive, designed to be used as keys for a Dictionary.
	/// </summary>
	public sealed class StringCI
    {
		readonly string _value;
		readonly int _hashCode;

		public StringCI(string value)
		{
			_value = value 
				?? throw new ArgumentNullException(nameof(value));
			_hashCode = value.ToLowerInvariant().GetHashCode();
		}

		public override int GetHashCode() => _hashCode;

		public override bool Equals(object obj)
		{
			return obj != null
				&& obj is StringCI
				&& string.Compare(
					((StringCI)obj)._value, 
					_value, 
					true, 
					CultureInfo.InvariantCulture) == 0;
		}

		public override string ToString() => _value;

		public static bool operator==(StringCI x, StringCI y)
		{
			return (ReferenceEquals(x, null) && ReferenceEquals(y, null))
				|| 
				(!ReferenceEquals(x, null) && !ReferenceEquals(y, null) && x.Equals(y));
		}

		public static bool operator !=(StringCI x, StringCI y)
		{
			return false == (x == y);
		}

		public static implicit operator string(StringCI d)
		{
			return d._value;
		}

		public static implicit operator StringCI(string value)
		{
			return new StringCI(value);
		}

		public static bool Compare(string x, string y)
		{
			return string.Compare(x, y, true, CultureInfo.InvariantCulture) == 0;
		}
	}
}
