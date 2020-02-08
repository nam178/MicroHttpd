using System;

namespace MicroHttpd.Core.StringMatch
{
	public sealed class ExactCaseInsensitive : IStringMatch
	{
		readonly string _value;

		public ExactCaseInsensitive(string value)
		{
			if(string.IsNullOrWhiteSpace(value))
				throw new ArgumentException("message", nameof(value));
			_value = value;
		}

		public bool IsMatch(string testString)
		{
			return string.Equals(
				testString, 
				_value, 
				StringComparison.InvariantCultureIgnoreCase);
		}
	}
}
