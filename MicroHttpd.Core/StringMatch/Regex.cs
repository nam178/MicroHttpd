using System;

namespace MicroHttpd.Core.StringMatch
{
	public sealed class Regex : IStringMatch
	{
		readonly Regex _regex;
		readonly string _pattern;

		public Regex(string pattern)
		{
			if(pattern == null)
				throw new ArgumentNullException(nameof(pattern));
			_regex = new Regex(pattern);
			_pattern = pattern;
		}

		public bool IsMatch(string testString)
		{
			return _regex.IsMatch(testString);
		}
	}
}
