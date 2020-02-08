using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MicroHttpd.Core
{
	sealed class HttpFixedLengthRequestBody : ReadOnlyStream
	{
		readonly Stream _requestStream;
		long _contentLength = 0L;
		long _position = 0L;

		public HttpFixedLengthRequestBody(
			Stream requestStream,
			long contentLength)
		{
			if(contentLength < 0)
				throw new ArgumentException(nameof(contentLength));
			_contentLength = contentLength;
			_requestStream = requestStream 
				?? throw new ArgumentNullException(nameof(requestStream));
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			var maxBytesToRead = (int)Math.Min(count, _contentLength - _position);
			if(maxBytesToRead == 0)
				return 0;
			var bytesRead = _requestStream.Read(buffer, offset, maxBytesToRead);
			HttpPrematureFinishException.ThrowIfZero(bytesRead);
			_position += bytesRead;
			return bytesRead;
		}

		public override async Task<int> ReadAsync(
			byte[] buffer,
			int offset,
			int count,
			CancellationToken cancellationToken)
		{
			var maxBytesToRead = (int)Math.Min(count, _contentLength - _position);
			if(maxBytesToRead == 0)
				return 0;
			var bytesRead = await  _requestStream.ReadAsync(
				buffer, 
				offset, 
				maxBytesToRead, 
				cancellationToken);
			HttpPrematureFinishException.ThrowIfZero(bytesRead);
			_position += bytesRead;
			return bytesRead;
		}

		public override int ReadByte()
			=> throw new NotImplementedException();
	}
}
