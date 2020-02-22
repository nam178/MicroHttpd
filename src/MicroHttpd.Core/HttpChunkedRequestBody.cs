using System;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace MicroHttpd.Core
{
    sealed class HttpChunkedRequestBody : ReadOnlyStream
	{
		readonly HttpChunkHeaderBuilder _chunkHeaderBuilder = new HttpChunkHeaderBuilder();
		readonly HttpLineBuilder _lineBuilder = new HttpLineBuilder();
		readonly RollbackableStream _requestStream;
		readonly HttpRequestHeader _header;
		readonly HttpChunkedTrailerReader _trailerReader;
		readonly byte[] _readBuffer;

		// Current chunk info, if any.
		HttpChunkHeader	_currentChunkHeader;
		long			_currentChunkRemainingBytes;

		public HttpChunkedRequestBody(
			RollbackableStream requestStream, 
			HttpRequestHeader header, 
			TcpSettings tcpSettings)
		{
			Validation.RequireValidTcpSettings(tcpSettings);
			_requestStream = requestStream 
				?? throw new ArgumentNullException(nameof(requestStream));
			_header = header 
				?? throw new ArgumentNullException(nameof(header));
			_readBuffer = new byte[tcpSettings.ReadWriteBufferSize];
			_trailerReader = new HttpChunkedTrailerReader(
				_requestStream, 
				tcpSettings.ReadWriteBufferSize);
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			throw new NotImplementedException(
				$"To be implemented, use {nameof(ReadAsync)} for now"
				);
		}

		public override int ReadByte()
		{
			throw new NotImplementedException(
				$"To be implemented, use {nameof(ReadAsync)} for now"
				);
		}

		public override async Task<int> ReadAsync(
			byte[] buffer, 
			int offset, 
			int count, 
			CancellationToken cancellationToken)
		{
			Validation.RequireValidBuffer(buffer, offset, count);

			// Finished?
			if(_trailerReader.IsCompleted)
				return 0;

			// Before reading the body, 
			// make sure we have the header (so we know its length)
			await EnsureChunkHeaderWasReadAsync(cancellationToken);

			// Here, chunk header is defined,
			// If the chunk we are reading is the last chunk,
			// Read the remaining chunk trailer, and always return zero byte.
			if(_currentChunkHeader.Length == 0)
			{
				await _trailerReader.ReadTrailerAsync(_header);
				return 0;
			}
			// If the chunk we are reading is not the last chunk,
			// read it and return number of bytes read.
			else
				return await ContinueReadChunkBodyAsync(buffer, offset, count);
		}

		async Task EnsureChunkHeaderWasReadAsync(
			CancellationToken cancellationToken)
		{
			while(_currentChunkHeader == null)
			{
				cancellationToken.ThrowIfCancellationRequested();

				// Read
				var bytesRead = await _requestStream.ReadAsync(_readBuffer, 0, _readBuffer.Length);
				HttpPrematureFinishException.ThrowIfZero(bytesRead);

				// Append the data we just read to the header builder.
				// If header is built after this call, we are done here.
				int contentStartIndex;
				if(_chunkHeaderBuilder.AppendBuffer(_readBuffer, 0, bytesRead, out contentStartIndex))
				{
					// Set current chunk metadata
					_currentChunkHeader = _chunkHeaderBuilder.Result;
					_currentChunkRemainingBytes = _chunkHeaderBuilder.Result.Length;

					// If we read some bytes from the body, rollback those read.
					_requestStream.TryRollbackFromIndex(
						_readBuffer, 
						srcLength: bytesRead, 
						startIndex: contentStartIndex);
				}
			}
		}

		async Task<int> ContinueReadChunkBodyAsync(byte[] buffer, int offset, int count)
		{
			Contract.Requires(_currentChunkHeader != null);
			// Pass this point,
			// It would be a bug if _currentChunkRemainingBytes is 0
			Contract.Assert(
				_currentChunkRemainingBytes > 0,
				$"{nameof(_currentChunkRemainingBytes)} cannot less than or equal to zero"
				);

			// Now read.
			var bytesRead = await _requestStream.ReadAsync(
				buffer,
				0, 
				Math.Min(buffer.Length, (int)Math.Min(count, _currentChunkRemainingBytes)));
			HttpPrematureFinishException.ThrowIfZero(bytesRead);
			_currentChunkRemainingBytes -= bytesRead;

			// No more data to read?
			if(_currentChunkRemainingBytes == 0)
				await CompleteReadingCurrentChunkAsync();

			return bytesRead;
		}

		/// <summary>
		/// Called when current chunk has no more data to read,
		/// we'll do some cleaning here.
		/// </summary>
		async Task CompleteReadingCurrentChunkAsync()
		{
			// A chunk always nend with a new line character,
			// let's skip it.
			var bytesRead = await _requestStream.ReadAsync(_readBuffer, 0, _readBuffer.Length);
			HttpPrematureFinishException.ThrowIfZero(bytesRead);
			if(false == _lineBuilder.AppendBuffer(_readBuffer, 0, bytesRead, out int nextLineStartIndex))
			{
				throw new HttpBadRequestException(
					"Chunk body must end with an empty line"
					);
			}
			_lineBuilder.Reset();
			_requestStream.TryRollbackFromIndex(_readBuffer, bytesRead, nextLineStartIndex);

			// Prepare to read the next header
			_chunkHeaderBuilder.Reset();

			// Clear the header, so next ReadAsync() operation 
			// will attempt to read the next header
			_currentChunkHeader = null;
		}
	}
}
