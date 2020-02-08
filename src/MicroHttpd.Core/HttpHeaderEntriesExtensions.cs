using System;
using System.Text;

namespace MicroHttpd.Core
{
	static class HttpHeaderEntriesExtensions
	{
		public static void CopyEntries(this HttpHeaderEntries to, HttpHeaderEntries from)
		{
			if(to == null)
				throw new ArgumentNullException(nameof(to));
			if(from == null)
				throw new ArgumentNullException(nameof(from));

			foreach (var key in from.Keys)
			{
				var values = from.Get(key, false);
				for (int i = 0; i < values.Count; i++)
				{
					string value = values[i];
					to.Add(key, value);
				}
			}
		}

		public static string GeneratePlainHeader(
			this HttpHeaderEntries entries, 
			string startLine)
		{
			var headerBuilder = new StringBuilder();

			headerBuilder.Append(startLine);
			headerBuilder.Append(SpecialChars.CRNL);

			foreach (var key in entries.Keys)
			{
				foreach (var value in entries.Get(key, false))
				{
					headerBuilder.Append(String.Format("{0}: {1}", key, value));
					headerBuilder.Append(SpecialChars.CRNL);
				}
			}

			headerBuilder.Append(SpecialChars.CRNL);

			return headerBuilder.ToString();
		}
	}
}
