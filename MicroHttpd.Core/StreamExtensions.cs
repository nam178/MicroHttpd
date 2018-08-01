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

		public static async Task CopyToAsync(this Stream target, Stream src, long count, int bufferSize)
		{
			Validation.RequireValidBufferSize(bufferSize);
			var bytesCopied = 0L;
			var buffer = new byte[bufferSize];
			while(bytesCopied < count)
			{
				var desiredBytesToCopy = (int)Math.Min(buffer.Length, count - bytesCopied);
				var actualBytesToCopy = await src.ReadAsync(buffer, 0, desiredBytesToCopy);
				if(actualBytesToCopy == 0)
					throw new EndOfStreamException("Unexpected end of source stream");
				await target.WriteAsync(buffer, 0, actualBytesToCopy);
				bytesCopied += actualBytesToCopy;
			}
		}
	}
}
