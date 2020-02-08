using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MicroHttpd.Core
{
	public abstract class WriteOnlyStream : Stream
    {
		public sealed override bool CanRead
		{ get => false; }

		public sealed override bool CanSeek
		{ get => false; }

		public sealed override bool CanWrite
		{ get => true; }

		public sealed override long Length
		{ get => throw new NotSupportedException(); }

		public sealed override long Position
		{
			get => throw new NotSupportedException();
			set => throw new NotSupportedException();
		}

		public sealed override long Seek(long offset, SeekOrigin origin)
			=> throw new NotSupportedException();

		public sealed override void SetLength(long value)
			=> throw new NotSupportedException();

		public sealed override int Read(byte[] buffer, int offset, int count)
			=> throw new NotSupportedException();

		public sealed override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
			=> throw new NotSupportedException();

		public sealed override IAsyncResult BeginRead(
			byte[] buffer,
			int offset,
			int count,
			AsyncCallback callback,
			object state)
			=> throw new NotSupportedException();

		public sealed override int ReadByte()
			=> throw new NotSupportedException();

		public sealed override Task<int> ReadAsync(
			byte[] buffer,
			int offset,
			int count,
			CancellationToken cancellationToken) => throw new NotSupportedException();

		public abstract override void Write(byte[] buffer, int offset, int count);

		public abstract override Task WriteAsync(byte[] buffer, int offset, int count,
			CancellationToken cancellationToken);

		public sealed override IAsyncResult BeginWrite(
			byte[] buffer,
			int offset,
			int count,
			AsyncCallback callback,
			object state)
			=> throw new NotImplementedException($"Use {nameof(WriteAsync)} instead");

		public abstract override void WriteByte(byte value);
	}
}
