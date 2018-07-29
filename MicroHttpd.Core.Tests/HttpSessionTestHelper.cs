using System.IO;

namespace MicroHttpd.Core.Tests
{
	static class HttpSessionTestHelper
    {
		public static void ReadResponseHeader(
			this MemoryStream responseStream,
			out HttpResponseHeader responseHeader)
		{
			var headerBuilder = HttpHeaderBuilderFactory.CreateResponseHeaderBuilder();
			var buff = new byte[4096];
			responseStream.Position = 0;
			while(true)
			{
				var r = responseStream.Read(buff, 0, 4096);
				if(r == 0)
					throw new EndOfStreamException("Unexpected end of stream");
				if(headerBuilder.AppendBuffer(buff, 0, r, out int bodyStartIndex))
				{
					responseHeader = headerBuilder.Result;
					return;
				}
			}
		}
	}
}
