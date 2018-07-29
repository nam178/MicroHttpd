using System;
using System.IO;
using System.Threading.Tasks;

namespace MicroHttpd.Core
{
	/// <summary>
	/// An "encoder" that doesn't really encoding anything,
	/// it just write directly to the underlying stream.
	/// </summary>
	sealed class HttpPassthroughResponseEncoder : IHttpResponseEncoder
	{
		readonly long _contentLength;
		readonly Stream _target;
		long _bytesWritten;

		public HttpPassthroughResponseEncoder(Stream target, long contentLength)
		{
			if(contentLength < 0)
				throw new ArgumentException(nameof(contentLength));
			_target = target 
				?? throw new ArgumentNullException(nameof(target));
			_contentLength = contentLength;
		}
		
		public Task CompleteAsync()
		{
			RequireEnoughBytesWritten();
			return Task.FromResult(true);
		}

		public async Task AppendAsync(byte[] buffer, int offset, int count)
		{
			RequireWithinContentLength(extraBytesToWrite: count);
			await _target.WriteAsync(buffer, offset, count);
			_bytesWritten += count;
		}

		void RequireWithinContentLength(int extraBytesToWrite)
		{
			if((_bytesWritten + extraBytesToWrite) > _contentLength)
				throw new InvalidOperationException(
					$"Cannot write more than {_contentLength} bytes"
					);
		}

		void RequireEnoughBytesWritten()
		{
			if(_bytesWritten < _contentLength)
				throw new InvalidOperationException(
					$"Must complete encoding with exact {_contentLength} byte(s) written."
					);
		}
	}
}
