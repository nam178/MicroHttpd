using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MicroHttpd.Core
{
	/// <summary>
	/// Decorator of a Stream to provide some benefits:
	/// 
	///	1. More predictable exceptions, calls to member methods 
	///	and properties throw TcpStreamException on errors.
	///		
	///	2. Observe reads/writes to the underlying stream using a WatchDog,
	/// so that if the stream id idle for too long, the WatchDog will terminate it.
	/// </summary>
	public sealed class WatchDogStreamDecorator : Stream
	{
		readonly Stream _original;
		readonly IWatchDogSession _watchDogSession;

		public override bool CanRead => _original.CanRead;

		public override bool CanSeek => _original.CanSeek;

		public override bool CanWrite => _original.CanWrite;

		public override long Length => _original.Length;

		public override bool CanTimeout => _original.CanTimeout;

		public override int ReadTimeout 
		{
			get => _original.ReadTimeout;
			set => _original.ReadTimeout = value;
		}

		public override int WriteTimeout 
		{
			get => _original.WriteTimeout;
			set => _original.WriteTimeout = value;
		}

		public override long Position
		{
			get => _original.Position;
			set => _original.Position = value;
		}

		public WatchDogStreamDecorator(Stream original, IWatchDogSession watchDogSession)
		{
			_original = original 
				?? throw new ArgumentNullException(nameof(original));
			_watchDogSession = watchDogSession 
				?? throw new ArgumentNullException(nameof(watchDogSession));
		}

		public override void Flush()
		{
			_original.Flush();
			_watchDogSession.Refresh();
		}

		public override async Task FlushAsync(CancellationToken cancellationToken)
		{
			await _original.FlushAsync(cancellationToken);
			_watchDogSession.Refresh();
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			var bytesRead = _original.Read(buffer, offset, count);
			if(bytesRead > 0)
				_watchDogSession.Refresh();
			return bytesRead;
		}

		public override int ReadByte()
		{
			var bytesRead = _original.ReadByte();
			if(bytesRead > 0)
				_watchDogSession.Refresh();
			return bytesRead;
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			_original.Write(buffer, offset, count);
			_watchDogSession.Refresh();
		}

		public override async Task WriteAsync(
			byte[] buffer, 
			int offset, 
			int count, 
			CancellationToken cancellationToken)
		{
			await _original.WriteAsync(buffer, offset, count, cancellationToken);
			_watchDogSession.Refresh();
		}

		public override async Task<int> ReadAsync(
			byte[] buffer, 
			int offset, 
			int count, 
			CancellationToken cancellationToken)
		{
			var bytesRead = await _original.ReadAsync(buffer, offset, count, cancellationToken);
			if(bytesRead > 0)
				_watchDogSession.Refresh();
			return bytesRead;
		}

		public override IAsyncResult BeginRead(
			byte[] buffer, 
			int offset, 
			int count, 
			AsyncCallback callback, 
			object state)
			=> throw new NotImplementedException($"Use {nameof(ReadAsync)} instead");

		public override IAsyncResult BeginWrite(
			byte[] buffer, 
			int offset, 
			int count, 
			AsyncCallback callback, 
			object state)
			=> throw new NotImplementedException($"Use {nameof(WriteAsync)} instead");

		public override void WriteByte(byte value)
		{
			_original.WriteByte(value);
			_watchDogSession.Refresh();
		}

		public override async Task CopyToAsync(
			Stream destination,
			int bufferSize,
			CancellationToken cancellationToken)
		{
			await _original.CopyToAsync(destination, bufferSize, cancellationToken);
			_watchDogSession.Refresh();
		}

		public override long Seek(long offset, SeekOrigin origin)
			=> _original.Seek(offset, origin);

		public override void SetLength(long value)
			=> _original.SetLength(value);

		protected override void Dispose(bool disposing)
		{
			if(disposing)
			{
				_original.Dispose();
			}
			base.Dispose(disposing);
		}
	}
}
