using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace MicroHttpd.Core.Tests
{
	public class RollbackableStreamTests
    {
		[Theory]
		[InlineData(0)]
		[InlineData(7)]
		[InlineData(1024)]
		[InlineData(1024 * 4)]
		[InlineData(1024 * 8)]
		[InlineData(1024 * 8 + 21)]
		public async Task WorksWhenReadAheadBufferIsNotUsed(int testDataSize)
		{
			var testData = MockData.MockNetworkStream(testDataSize);
			var inst = new RollbackableStream(
				testData,
				TcpSettings.Default
				);
			var result = await ReadResult(testDataSize, inst);

			Assert.True(testData.ToArray().SequenceEqual(result.ToArray()));
		}

		static async Task<MemoryStream> ReadResult(int testDataSize, RollbackableStream inst)
		{
			var result = new MemoryStream();
			var buffer = new byte[4096];
			while(result.Length < testDataSize)
			{
				var bytesRead = await inst.ReadAsync(buffer, 0, buffer.Length);
				if(bytesRead == 0)
					break;
				result.Write(buffer, 0, bytesRead);
			}

			return result;
		}

		[Theory]
		[InlineData(1024, 32)]
		[InlineData(1024, 32, 37, 39)]
		public async Task WorksWhenRollingBack(int bytesReadBeforeRollingback, params int[] rollbackBytes)
		{
			var testData = MockData.MockNetworkStream(4096);
			var inst = new RollbackableStream(
				testData,
				TcpSettings.Default
				);
			var buffer = new byte[testData.Length];

			// Read minimum 4 bytes
			int bytesRead = 0;
			while(bytesRead < bytesReadBeforeRollingback)
				bytesRead += await inst.ReadAsync(
					buffer, 
					bytesRead, 
					(int)testData.Length - bytesRead);

			// Rollback
			foreach(var bytesToRollback in rollbackBytes)
			{
				bytesRead -= bytesToRollback;
				inst.TryRollbackFromIndex(
					src: buffer,
					srcLength: bytesRead + bytesToRollback,
					startIndex: bytesRead
					);
			}
			
			// Read the rest
			while(bytesRead < testData.Length)
			{
				var t = await inst.ReadAsync(buffer, bytesRead, (int)testData.Length - bytesRead);
				if(t == 0)
					break;
				bytesRead += t;
			}

			Assert.Equal(testData.Length, bytesRead);
			Assert.True(testData.ToArray().SequenceEqual(buffer));
		}
    }
}
