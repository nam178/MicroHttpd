using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MicroHttpd.Core
{
	sealed class TcpExceptionStreamDecorator : Stream
    {
		readonly Stream _original;

		public override bool CanRead
		{
			get
			{
				try
				{
					return _original.CanRead;
				} catch (Exception ex) {
					throw new TcpException(ex);
				}
			}
		}

		public override bool CanSeek
		{
			get
			{
				try
				{
					return _original.CanSeek;
				} catch(Exception ex) {
					throw new TcpException(ex);
				}
			}
		}

		public override bool CanWrite
		{
			get
			{
				try
				{
					return _original.CanWrite;
				} catch (Exception ex) {
					throw new TcpException(ex);
				}
			}
		}

		public override long Length
		{
			get
			{
				try
				{
					return _original.Length;
				} catch (Exception ex) {
					throw new TcpException(ex);
				}
			}
		}

		public override bool CanTimeout
		{
			get
			{
				try
				{
					return _original.CanTimeout;
				} catch (Exception ex) {
					throw new TcpException(ex);
				}
			}
		}

		public override int ReadTimeout
		{
			get {
				try
				{
					return _original.ReadTimeout;
				} catch (Exception ex) {
					throw new TcpException(ex);
				}
			}
			set
			{
				try
				{
					_original.ReadTimeout = value;
				} catch (Exception ex) {
					throw new TcpException(ex);
				}
			}
		}

		public override int WriteTimeout
		{
			get {
				try
				{
					return _original.WriteTimeout;
				} catch (Exception ex) {
					throw new TcpException(ex);
				}
			}
			set {
				try
				{
					_original.WriteTimeout = value;
				} catch (Exception ex) {
					throw new TcpException(ex);
				}
			}
		}

		public override long Position
		{
			get
			{
				try
				{
					return _original.Position;
				} catch(Exception ex) {
					throw new TcpException(ex);
				}
			}
			set
			{
				try
				{
					_original.Position = value;
				} catch(Exception ex) {
					throw new TcpException(ex);
				}
			}
		}

		public TcpExceptionStreamDecorator(Stream original)
		{
			_original = original
				?? throw new ArgumentNullException(nameof(original));
		}

		public override void Flush()
		{
			try
			{
				_original.Flush();
			} catch(Exception ex) {
				throw new TcpException(ex);
			}
		}

		public override async Task FlushAsync(CancellationToken cancellationToken)
		{
			try
			{
				await _original.FlushAsync(cancellationToken);
			} catch(Exception ex) {
				throw new TcpException(ex);
			}
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			try
			{
				return _original.Read(buffer, offset, count);
			} catch(Exception ex) {
				throw new TcpException(ex);
			}
		}

		public override int ReadByte()
		{
			try
			{
				return _original.ReadByte();
			} catch(Exception ex) {
				throw new TcpException(ex);
			}
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			try
			{
				_original.Write(buffer, offset, count);
			} catch(Exception ex) {
				throw new TcpException(ex);
			}
		}

		public override async Task WriteAsync(
			byte[] buffer,
			int offset,
			int count,
			CancellationToken cancellationToken)
		{
			try
			{
				await _original.WriteAsync(buffer, offset, count, cancellationToken);
			} catch(Exception ex) {
				throw new TcpException(ex);
			}
		}

		public override async Task<int> ReadAsync(
			byte[] buffer,
			int offset,
			int count,
			CancellationToken cancellationToken)
		{
			try
			{
				return await _original.ReadAsync(buffer, offset, count, cancellationToken);
			} catch(Exception ex) {
				throw new TcpException(ex);
			}
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
			try
			{
				_original.WriteByte(value);
			} catch(Exception ex) {
				throw new TcpException(ex);
			}
		}

		public override async Task CopyToAsync(
			Stream destination,
			int bufferSize,
			CancellationToken cancellationToken)
		{
			try
			{
				await _original.CopyToAsync(destination, bufferSize, cancellationToken);
			} catch(Exception ex) {
				throw new TcpException(ex);
			}
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			try
			{
				return _original.Seek(offset, origin);
			} catch(Exception ex) {
				throw new TcpException(ex);
			}
		}

		public override void SetLength(long value)
		{
			try
			{
				_original.SetLength(value);
			} catch(Exception ex) {
				throw new TcpException(ex);
			}
		}

		protected override void Dispose(bool disposing)
		{
			if(disposing) {
				_original.Dispose();
			}
			base.Dispose(disposing);
		}

		public override void Close()
		{
			try
			{
				base.Close();
			} catch (Exception ex) {
				throw new TcpException(ex);
			}
		}

		public override bool Equals(object obj)
		{
			try
			{
				return base.Equals(obj);
			} catch (Exception ex) {
				throw new TcpException(ex);
			}
		}

		public override int GetHashCode()
		{
			try
			{
				return base.GetHashCode();
			} catch (Exception ex) {
				throw new TcpException(ex);
			}
		}

		public override string ToString()
		{
			try
			{
				return base.ToString();
			} catch (Exception ex) {
				throw new TcpException(ex);
			}
		}
	}
}
