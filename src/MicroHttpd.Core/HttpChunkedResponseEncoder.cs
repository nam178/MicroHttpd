using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MicroHttpd.Core
{
	/// <summary>
	/// An encoder that encodes binary data into HTTP chunks
	/// </summary>
	/// <remarks>Not thread safe</remarks>
	sealed class HttpChunkedResponseEncoder : IHttpResponseEncoder
	{
		readonly MemoryStream _buffer;
		readonly int _maxChunkSize;
		readonly TcpSettings _tcpSettings;
		readonly Stream _target;

		public HttpChunkedResponseEncoder(
			Stream target,
			TcpSettings tcpSettings, 
			HttpSettings httpSettings)
		{
			Validation.RequireValidTcpSettings(tcpSettings);
			Validation.RequireValidHttpSettings(httpSettings);
			_buffer = new MemoryStream(httpSettings.MaxBodyChunkSize);
			_maxChunkSize = httpSettings.MaxBodyChunkSize;
			_target = target ?? throw new ArgumentNullException(nameof(target));
			_tcpSettings = tcpSettings;
		}

		public async Task CompleteAsync()
		{
			// Flush
			await FlushAsync();
			// Always end the http body with an empty chunk + 2x Newlines
			await _target.WriteAsync(
					_finalChunk,
					_tcpSettings.ReadWriteBufferSize
					);
		}

		public async Task AppendAsync(byte[] buffer, int offset, int count)
		{
			Validation.RequireValidBuffer(buffer, offset, count);

			// Size of the requested buffer too large for our internal buffer?
			// Split it into multiple writes
			while(count > _maxChunkSize)
			{
				await AppendAsync(buffer, offset, _maxChunkSize);
				offset += _maxChunkSize;
				count -= _maxChunkSize;
			}

			// Size of the requested buffer is now smaller
			// than or equal to our internal buffer size.
			//
			// First, flush the internal buffer if 
			// it doesn't have enough space for appending
			if((_buffer.Length + count) > _maxChunkSize)
			{
				await FlushAsync();
			}

			// Now write to the internal buffer
			_buffer.Write(buffer, offset, count);
		}

		/// <summary>
		/// Flush the internal buffer, i.e. write into the target stream
		/// </summary>
		async Task FlushAsync()
		{
			// Early exit if nothing to write
			if(_buffer.Length == 0) return;

			// Write chunk header
			await _target.WriteAsync(
				Encoding.ASCII.GetBytes(
					_buffer.Length.ToString("X", CultureInfo.InvariantCulture) 
					+ SpecialChars.CRNL),
				_tcpSettings.ReadWriteBufferSize
				);

			// Append a new line to the chunk body
			_buffer.Write(_newLine, 0, _newLine.Length);

			// Write chunk body
			_buffer.Position = 0;
			await _buffer.CopyToAsync(_target, _tcpSettings.ReadWriteBufferSize);

			// After flushing, emty the internal buffer
			_buffer.SetLength(0);
		}

		static readonly byte[] _finalChunk;
		static readonly byte[] _newLine;
		static HttpChunkedResponseEncoder()
		{
			_finalChunk = Encoding.ASCII.GetBytes(
				$"0{SpecialChars.CRNL}{SpecialChars.CRNL}");
			_newLine = Encoding.ASCII.GetBytes(SpecialChars.CRNL);
		}
	}
}
