using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MicroHttpd.Core.Tests
{
	/// <summary>
	/// MemoryStream, but simulate network slowness by returning less data for each read
	/// </summary>
	sealed class MockNetworkStream : MemoryStream
	{
		readonly Random _rnd;

		public MockNetworkStream(int seed = 7)
		{
			_rnd = new Random(seed);
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			if(count > 1)
				return base.Read(buffer, offset, count - GetLoss(count));
			else
				return base.Read(buffer, offset, count);
		}

		public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
		{
			if(count > 1)
				return base.ReadAsync(buffer, offset, count - GetLoss(count), cancellationToken);
			else
				return base.ReadAsync(buffer, offset, count, cancellationToken);
		}

		int GetLoss(int maxExclusiveLoss) => _rnd.Next(0, Math.Min(maxExclusiveLoss, 1024));
	}
}
