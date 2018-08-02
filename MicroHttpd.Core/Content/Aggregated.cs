using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MicroHttpd.Core.Content
{
	sealed class Aggregated : IContent
	{
		readonly IReadOnlyList<IContent> _contents;

		public Aggregated(IReadOnlyList<IContent> contents)
		{
			_contents = contents 
				?? throw new ArgumentNullException(nameof(contents));
		}

		public async Task<bool> ServeAsync(
			IHttpRequest request, 
			IHttpResponse response)
		{
			for(var i = 0; i < _contents.Count; i++)
			{
				if(await _contents[i].ServeAsync(request, response))
				{
					return true;
				}
			}
			return false;
		}
	}
}
