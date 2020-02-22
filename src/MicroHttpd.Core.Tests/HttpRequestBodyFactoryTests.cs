using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace MicroHttpd.Core.Tests
{
    public class HttpRequestBodyFactoryTests
    {
		[Theory]
		[MemberData(nameof(TrueData))]
		internal void CreateCorrectConcrete(HttpHeaderEntries entries, Type expectedType)
		{
			var inst = new HttpRequestBodyFactory();
			var result = inst.Create(
				TcpSettings.Default,
				new HttpRequestHeader("GET / HTTP/1.1", entries),
				new RollbackableStream(new MemoryStream(), TcpSettings.Default));
			Assert.Equal(expectedType, result.GetType());
		}

		[Theory]
		[MemberData(nameof(FalseData))]
		internal void ThrowsInvalidHttpException(HttpHeaderEntries entries)
		{
			var inst = new HttpRequestBodyFactory();
			Assert.Throws<HttpBadRequestException>(delegate
			{
				inst.Create(
				TcpSettings.Default,
				new HttpRequestHeader("GET / HTTP/1.1", entries),
				new RollbackableStream(new MemoryStream(), TcpSettings.Default));
			});
		}

		public static IEnumerable<object[]> TrueData()
		{
			yield return new object[] {
				new HttpHeaderEntries { { "Transfer-Encoding", "chunked" } },
				typeof(HttpChunkedRequestBody)
			};

			yield return new object[] {
				new HttpHeaderEntries { { "Transfer-Encoding", "gzip, chunked" } },
				typeof(HttpChunkedRequestBody)
			};

			yield return new object[] {
				new HttpHeaderEntries { { "Content-Length", "16"} },
				typeof(HttpFixedLengthRequestBody)
			};
		}

		public static IEnumerable<object[]> FalseData()
		{
			yield return new object[] {
				new HttpHeaderEntries { { "Transfer-Encoding", "chunked, gzip" } }
			};

			yield return new object[] {
				new HttpHeaderEntries { { "Transfer-Encoding", "gzip" } }
			};
		}
    }
}
