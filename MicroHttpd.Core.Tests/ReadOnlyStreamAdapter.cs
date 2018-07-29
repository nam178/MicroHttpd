using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MicroHttpd.Core.Tests
{
	sealed class ReadOnlyStreamAdapter : ReadOnlyStream
    {
		readonly Stream _inner;

		public ReadOnlyStreamAdapter(Stream inner)
		{
			_inner = inner 
				?? throw new ArgumentNullException(nameof(inner));
		}

		public override int Read(byte[] buffer, int offset, int count)
			=> _inner.Read(buffer, offset, count);

		public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
			=> _inner.ReadAsync(buffer, offset, count, cancellationToken);

		public override int ReadByte()
			=> _inner.ReadByte();

		protected override void Dispose(bool disposing)
		{
			if(disposing)
			{
				_inner.Dispose();
			}
			base.Dispose(disposing);
		}
	}
}
