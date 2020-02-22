namespace MicroHttpd.Core.StringMatch
{
    public class MatchAll : IStringMatch
	{
		public bool IsMatch(string testString) => true;
	}
}
