using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MicroHttpd.Core
{
	public abstract class ReadOnlyStream : Stream
	{
		public sealed override bool CanRead
		{ get => true; }

		public sealed override bool CanSeek
		{ get => false; }

		public sealed override bool CanWrite
		{ get => false; }

		public sealed override long Length
		{ get => throw new NotSupportedException(); }

		public sealed override long Position
		{
			get => throw new NotSupportedException();
			set => throw new NotSupportedException();
		}

		public sealed override void Flush()
			=> throw new NotSupportedException();

		public sealed override Task FlushAsync(CancellationToken cancellationToken)
			=> throw new NotImplementedException();

		public sealed override long Seek(long offset, SeekOrigin origin)
			=> throw new NotSupportedException();

		public sealed override void SetLength(long value)
			=> throw new NotSupportedException();

		public abstract override int Read(byte[] buffer, int offset, int count);

		public sealed override IAsyncResult BeginRead(
			byte[] buffer, 
			int offset, 
			int count, 
			AsyncCallback callback, 
			object state)
			=> throw new NotImplementedException($"Use {nameof(ReadAsync)} instead.");

		public abstract override int ReadByte();

		public abstract override Task<int> ReadAsync(
			byte[] buffer, 
			int offset, 
			int count, 
			CancellationToken cancellationToken);

		public sealed override void Write(byte[] buffer, int offset, int count)
			=> throw new NotSupportedException();

		public sealed override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
			=> throw new NotSupportedException();

		public sealed override IAsyncResult BeginWrite(
			byte[] buffer, 
			int offset, 
			int count, 
			AsyncCallback callback, 
			object state)
			=> throw new NotSupportedException();

		public sealed override void WriteByte(byte value)
			=> throw new NotSupportedException();
	}
}
