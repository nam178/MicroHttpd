using System;
using System.IO;
using System.Reflection;

namespace MicroHttpd.Core.Tests
{
	static class MockData
    {
		public static MemoryStream MemoryStream(int length, int seed = 7)
		{
			var random = new Random(seed);
			var result = new MemoryStream();
			for(var i = 0; i < length; i++)
			{
				result.WriteByte((byte)random.Next(0, 256));
			}
			result.Position = 0;
			return result;
		}

		public static MockNetworkStream MockNetworkStream(int length, int seed = 7)
		{
			var random = new Random(seed);
			var result = new MockNetworkStream();
			for(var i = 0; i < length; i++)
			{
				result.WriteByte((byte)random.Next(0, 256));
			}
			result.Position = 0;
			return result;
		}

		public static byte[] Bytes(int length, int seed)
			=> MockNetworkStream(length, seed).ToArray();

		public static Stream EmbededResource(this string fileName)
		{
			var assembly = Assembly.GetExecutingAssembly();
			var resourceName = $"{typeof(MockData).Namespace}.{fileName}";
			return assembly.GetManifestResourceStream(resourceName);
		}
	}
}
