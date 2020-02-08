using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MicroHttpd.Core
{
	/// <summary>
	/// When reading HTTP headers, 
	/// we don't know exactly when the header ends, and usually read
	/// more than the actual length of the header, causing the next read
	/// operation to have missing data,
	/// 
	/// This reader allows us to 'rollback' the extra bytes that we've read, 
	/// given that those bytes is within the size of the internal buffer.
	/// </summary>
	sealed class RollbackableStream : ReadOnlyStream
    {
		// The raw TCP stream, this is where
		// we will be reading from.
		readonly Stream _original;

		// The read-ahead buffer, implemented as a binary stack.
		// the bytes rolled back by the caller are stored here,
		// and is read before _rawTcpStream is read.
		readonly BinaryStack _readAheadBuffer;

		public RollbackableStream(Stream tcpStream, TcpSettings tcpSettings)
		{
			Validation.RequireValidTcpSettings(tcpSettings);

			_original = tcpStream 
				?? throw new ArgumentNullException(nameof(tcpStream));

			// Allocate read-ahead buffer:
			// It should be large enough, at least equal to the TCP read buffer,
			// otherwise rollback calls will fail.
			_readAheadBuffer = new BinaryStack(tcpSettings.ReadWriteBufferSize);
		}

		/// <summary>
		/// Read data from the underlying stream into the specified buffer;
		/// If caller already Push() some data into the stream, those data are read first.
		/// </summary>
		public override async Task<int> ReadAsync(
			byte[] buffer, 
			int offset, 
			int count, 
			CancellationToken cancellationToken)
		{
			Validation.RequireValidBuffer(buffer, offset, count);

			// Do we have data in the _readAheadBuffer?
			// if so, read from it first;
			if(_readAheadBuffer.Length > 0)
			{
				var bytesToRead = Math.Min(count, _readAheadBuffer.Length);
				_readAheadBuffer.Pop(dest: buffer, offset: offset, count: bytesToRead);
				return bytesToRead;
			}

			// Pass this point, the _readAheadBuffer is empty,
			// we can read directly from the _rawTcpStream
			return await _original.ReadAsync(buffer, offset, count, cancellationToken);
		}

		/// <summary>
		/// Push, or rollback the data that the caller has read.
		/// This prepends specified data to the head of the stream we're reading from.
		/// </summary>
		/// <param name="src">Contains the data to be pushed back</param>
		/// <param name="offset">Where the data starts in the provided buffer</param>
		/// <param name="count">Length of the data to rollback</param>
		public void Rollback(byte[] src, int offset, int count)
		{
			_readAheadBuffer.Push(src, offset, count);
		}

		// Won't get used, not worth implemnenting
		public override int Read(byte[] buffer, int offset, int count)
			=> throw new NotImplementedException();

		// Won't get used, not worth implemnenting
		public override int ReadByte()
			=> throw new NotImplementedException();
	}

	static class RollbackableStreamExtensions
	{
		/// <summary>
		/// Similar to Rollback(), but specifying the start and end index from the source buffer instead.
		/// This ignores the request if there is nothing to rollback.
		/// </summary>
		/// <param name="src">Contains the data to be pushed back.</param>
		/// <param name="srcLength">Length of the source buffer, i.e. where in the source buffer the data ends.</param>
		/// <param name="startIndex">Where in the source buffer the data starts</param>
		public static bool TryRollbackFromIndex(this RollbackableStream inst, byte[] src, int srcLength, int startIndex)
		{
			if(startIndex < srcLength)
			{
				inst.Rollback(src, startIndex, srcLength - startIndex);
				return true;
			}
			return false;
		}
	}
}
