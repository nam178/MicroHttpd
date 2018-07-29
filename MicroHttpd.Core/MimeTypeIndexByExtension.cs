using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MicroHttpd.Core
{
	sealed class MimeTypeIndexByExtension : Dictionary<StringCI, MimeTypeEntry>
	{
		public MimeTypeIndexByExtension()
		{
			using(var t = GetType().Assembly.GetManifestResourceStream(GetType(), "Resources.mime.json"))
			using(var reader = new StreamReader(t, Encoding.UTF8))
			{
				foreach(var inst in JsonConvert
					.DeserializeObject<MimeTypeEntry[]>(reader.ReadToEnd()))
				{
					if(false == ContainsKey(inst.FileExtension))
					{
						Add(inst.FileExtension, inst);
					}
				}
			}
		}
	}
}
