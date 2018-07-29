using System;

namespace MicroHttpd.Core
{
	public sealed class MimeTypeEntry
	{
		public string Name
		{ get; }

		public string HttpContentType
		{ get; }

		public string FileExtension
		{ get; }

		public MimeTypeEntry(
			string name,
			string httpContentType,
			string fileExtension)
		{
			Name = name
				?? throw new ArgumentNullException(nameof(name));
			HttpContentType = httpContentType
				?? throw new ArgumentNullException(nameof(httpContentType));
			FileExtension = fileExtension
				?? throw new ArgumentNullException(nameof(fileExtension));
		}
	}
}
