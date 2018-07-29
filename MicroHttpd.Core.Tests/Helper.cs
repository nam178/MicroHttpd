using System.IO;

namespace MicroHttpd.Core.Tests
{
	static class Helper
    {
		public static byte[] ToBytes(this string src)
			=> System.Text.Encoding.UTF8.GetBytes(src);

		public static MemoryStream ToMemoryStream(this string src)
		{
			return new MemoryStream(src.ToBytes());
		}
	}
}
