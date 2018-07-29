using System;
using System.IO;
using System.Threading.Tasks;

namespace MicroHttpd.Core
{
	static class StreamExtensions
    {
		public static async Task WriteAsync(this Stream stream, byte[] data, int bufferSize)
		{
			if(stream == null)
				throw new ArgumentNullException(nameof(stream));
			if(data == null)
				throw new ArgumentNullException(nameof(data));
			Validation.RequireValidBufferSize(bufferSize);

			var bytesWritten = 0;
			while(bytesWritten < data.Length)
			{
				var remain = data.Length - bytesWritten;
				var bytesToWrite = Math.Min(remain, bufferSize);
				await stream.WriteAsync(data, offset: bytesWritten, count: bytesToWrite);
				bytesWritten += bytesToWrite;
			}
		}
	}
}
