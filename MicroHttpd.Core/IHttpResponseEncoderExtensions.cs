using System;
using System.Threading.Tasks;

namespace MicroHttpd.Core
{
	static class IHttpResponseEncoderExtensions
    {
		/// <summary>
		/// Append specified buffer into the encoder, but break the
		/// buffer into multiple AppendAsync() calls of 'bufferSize'
		/// </summary>
		internal static Task AppendAsync(
			this IHttpResponseEncoder encoder,
			byte[] buffer,
			int bufferSizeEachCall)
		{
			return AppendAsync(
				encoder, buffer, 0, buffer.Length, bufferSizeEachCall);
		}

		/// <summary>
		/// Append specified buffer into the encoder, but break the
		/// buffer into multiple AppendAsync() calls of 'bufferSize'
		/// </summary>
		internal static void Append(
			this IHttpResponseEncoder encoder,
			byte[] buffer,
			int bufferSizeEachCall)
		{
			Append(
				encoder, buffer, 0, buffer.Length, bufferSizeEachCall);
		}

		/// <summary>
		/// Append specified buffer into the encoder, but break the
		/// buffer into multiple AppendAsync() calls of 'bufferSize'
		/// </summary>
		internal static async Task AppendAsync(
			this IHttpResponseEncoder encoder,
			byte[] buffer,
			int offset,
			int count,
			int bufferSizeEachCall)
		{
			var bytesWrote = 0;
			while(bytesWrote < count)
			{
				var remain = count - bytesWrote;
				var bytesToWrite = Math.Min(remain, bufferSizeEachCall);
				await encoder.AppendAsync(
					buffer, offset + bytesWrote, bytesToWrite);
				bytesWrote += bytesToWrite;
			}
		}

		/// <summary>
		/// Append specified buffer into the encoder, but break the
		/// buffer into multiple AppendAsync() calls of 'bufferSize'
		/// </summary>
		internal static void Append(
			this IHttpResponseEncoder encoder,
			byte[] buffer,
			int offset,
			int count,
			int bufferSizeEachCall)
		{
			var bytesWrote = 0;
			while(bytesWrote < count)
			{
				var remain = count - bytesWrote;
				var bytesToWrite = Math.Min(remain, bufferSizeEachCall);
				encoder.Append(
					buffer, offset + bytesWrote, bytesToWrite);
				bytesWrote += bytesToWrite;
			}
		}
	}
}
