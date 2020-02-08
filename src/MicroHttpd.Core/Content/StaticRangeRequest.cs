namespace MicroHttpd.Core.Content
{
	struct StaticRangeRequest
    {
		public long From
		{ get; }

		public long To
		{ get; }

		public StaticRangeRequest(long from, long to)
		{
			From = from;
			To = to;
		}

		public override string ToString()
		{
			if(To != long.MinValue)
				return $"[StaticRangeRequest {From}-{To}]";
			else
				return $"[StaticRangeRequest {From}-]";
		}
	}
}
