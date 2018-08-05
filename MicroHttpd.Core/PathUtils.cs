using System.IO;
using System.Reflection;

namespace MicroHttpd.Core
{
	public static class PathUtils
    {
		public static string RelativeToAssembly(string path)
		{
			return Path.GetFullPath(
				Path.Combine(
					Path.GetDirectoryName(
						Assembly.GetEntryAssembly().Location),
						path));
		}
	}
}
