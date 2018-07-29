namespace MicroHttpd.Core
{
	/// <summary>
	/// Data represented in the chunk header, could have had more properties
	/// in here (chunk extensions), but only the length is important right now.
	/// </summary>
	public class HttpChunkHeader
	{
		public long Length
		{ get; }

		public HttpChunkHeader(long length)
		{
			Validation.RequireChunkLengthWithinLimit(length);
			Length = length;
		}
	}
}