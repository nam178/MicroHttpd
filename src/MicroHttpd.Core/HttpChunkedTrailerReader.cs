using System;
using System.Threading.Tasks;

namespace MicroHttpd.Core
{
	/// <summary>
	/// Used by HttpChunkedRequestBody, helps reading chunk trailer
	/// </summary>
	sealed class HttpChunkedTrailerReader
    {
		readonly RollbackableStream _httpMessageBodyStream;
		readonly byte[] _readBuffer;
		readonly HttpLineBuilder _lineBuilder = new HttpLineBuilder();

		bool _isCompleted;
		public bool IsCompleted
		{ get { return _isCompleted; } }

		public HttpChunkedTrailerReader(
			RollbackableStream httpMessageBodyStream, 
			int readBufferSize)
		{
			_readBuffer = new byte[readBufferSize];
			_httpMessageBodyStream = httpMessageBodyStream 
				?? throw new ArgumentNullException(nameof(httpMessageBodyStream));
		}

		/// <summary>
		/// Read trailer and append extra trailer key-values into the specified header.
		/// </summary>
		public async Task ReadTrailerAsync(HttpHeader header)
		{
			if(header == null)
				throw new ArgumentNullException(nameof(header));
			if(_isCompleted)
				throw new InvalidOperationException("Trailer already read");

			// Keep reading until we find a blank line
			while(true)
			{
				// Read
				var bytesRead = await _httpMessageBodyStream.ReadAsync(
				_readBuffer, 0, _readBuffer.Length);
				HttpPrematureFinishException.ThrowIfZero(bytesRead);

				// Process
				if(HandleBytesRead(bytesRead, header))
					break;
			}

			_isCompleted = true;
		}

		bool HandleBytesRead(int bytesRead, HttpHeader header)
		{
			int nextLineStartIndex;
			if(_lineBuilder.AppendBuffer(_readBuffer, 0, bytesRead, out nextLineStartIndex))
			{
				// The chunk trailer always ended with a blank line,
				// so as soon as we found a blank line, stop reading.
				if(string.IsNullOrWhiteSpace(_lineBuilder.Result))
					return true;

				// Line found! Let's try to parse it into a key-value,
				// and append it to our header;
				AddToHeader(_lineBuilder.Result, header);

				// If we read some data of the next line, rollback
				_httpMessageBodyStream.TryRollbackFromIndex(
					_readBuffer,
					startIndex: nextLineStartIndex,
					srcLength: bytesRead);

				// Get ready to read the next line.
				_lineBuilder.Reset();
			}

			return false;
		}

		static void AddToHeader(string line, HttpHeader header)
		{
			HttpHeaderLineParser.Parse(line, out string key, out string value);
			header.Add(key, value);
		}
	}
}
