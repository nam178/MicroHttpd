using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace MicroHttpd.Core.Tests
{
    public static class MockHttpChunkedMessageBody
	{
		/// <summary>
		/// The new line character to use
		/// </summary>
		public static string NewLine
		{ get; set; } = "\r\n";

		/// <summary>
		/// Generate chunked data for testing.
		/// </summary>
		public static void Generate(
			int[] chunkLengths, 
			out List<MemoryStream> chunks, 
			out byte[] httpMessageBodyBlob)
			=> Generate(
				chunkLengths, 
				new Dictionary<string, string> { }, 
				out chunks, 
				out httpMessageBodyBlob);

		/// <summary>
		/// Generate chunked data for testing.
		/// </summary>
		public static void Generate(
			int[] chunkLengths,
			Dictionary<string, string> trailers,
			out List<MemoryStream> chunks, 
			out byte[] httpMessageBodyBlob)
		{
			// Generate chunks
			chunks = new List<MemoryStream>();
			for(var i = 0; i < chunkLengths.Length; i++)
				chunks.Add(MockData.MockNetworkStream(chunkLengths[i], seed: i));

			// Write body
			var httpMessageBodyStream = new MemoryStream();
			foreach(var chunk in chunks)
			{
				httpMessageBodyStream.Write(chunk.Length.ToString("X", CultureInfo.InvariantCulture));
				httpMessageBodyStream.Write(";some extra attributes = 1133");
				httpMessageBodyStream.Write(NewLine);
				httpMessageBodyStream.Write(chunk.ToArray());
				httpMessageBodyStream.Write(NewLine);
			}

			// Always end with a empty chunk
			httpMessageBodyStream.Write($"0{NewLine}");

			// Write trailer
			foreach(var kv in trailers)
			{
				httpMessageBodyStream.Write($"{kv.Key}: {kv.Value}");
				httpMessageBodyStream.Write(NewLine);
			}

			// End the chunk request body with an empty line
			httpMessageBodyStream.Write(NewLine);
			httpMessageBodyBlob = httpMessageBodyStream.ToArray();
		}
	}

	public static class ChunkHelper
	{
		public static void Write(this Stream stream, byte[] data)
		{
			stream.Write(data, 0, data.Length);
		}

		public static void Write(this Stream stream, string data)
		{
			stream.Write(data.ToBytes());
		}
	}
}
