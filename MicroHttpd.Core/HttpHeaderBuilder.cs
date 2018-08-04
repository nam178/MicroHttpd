using System;

namespace MicroHttpd.Core
{
	sealed class HttpHeaderBuilder<THeader>
	{
		readonly HttpLineBuilder _lineBuilder = new HttpLineBuilder();
		readonly HttpHeaderEntries _entries = new HttpHeaderEntries();
		readonly ActivateHeader _activator;

		string _requestLine;
		int _totalLines = 0;

		public delegate THeader ActivateHeader(string startLine, HttpHeaderEntries headerEntries);

		/// <summary>
		/// To prevent hacks, we'll have a maximum number of lines a request header can have.
		/// </summary>
		const int MaxHeaderLines = 1024;

		THeader _result;
		public THeader Result
		{
			get
			{
				RequireNonNullResult();
				return _result;
			}
		}

		internal HttpHeaderBuilder(ActivateHeader activator)
		{
			_activator = activator 
				?? throw new ArgumentNullException(nameof(activator));
		}

		/// <summary>
		/// Continue building the header by appending the specified buffer.
		/// </summary>
		/// <returns>True once building header finishes, 
		/// the Result property becomes available for access.</returns>
		public bool AppendBuffer(
			byte[] buffer, 
			int start, 
			int count, 
			out int bodyStartIndex)
		{
			Validation.RequireValidBuffer(buffer, start, count);

			bodyStartIndex = default(int);

			// Unless there is no new line in this buffer, keep continue.
			int nextLineStartIndex;
			while(_lineBuilder.AppendBuffer(buffer, start, count, out nextLineStartIndex))
			{
				RequireTotalLinesLessThanLimit(_totalLines);

				// Process this line:
				// AppendResult() returns false indicates
				// header ends here. Body starts on the next line.
				if(false == AppendResult(_lineBuilder.Result, ref _requestLine, _entries))
				{
					bodyStartIndex = nextLineStartIndex;

					// Build result to make the 'Result' property available
					// for caller.
					MakeResultPropertyAvailable();
					return true;
				}

				// Prep for the next line
				_lineBuilder.Reset();
				count -= nextLineStartIndex - start;
				start  = nextLineStartIndex;

				// We didn't see the message ends with a blank line,
				// however the message already end - this is invalid.
				HttpPrematureFinishException.ThrowIfZero(count);
			}

			// Header didn't end within this buffer;
			return false;
		}

		void MakeResultPropertyAvailable()
		{
			RequireNonNullRequestLine();
			_result = _activator.Invoke(_requestLine, _entries);
		}

		static void RequireTotalLinesLessThanLimit(int totalLines)
		{
			if(totalLines >= MaxHeaderLines)
				throw new HttpPayloadTooLargeException(
					$"Reached maximum header lines of {MaxHeaderLines}"
					);
		}

		static bool AppendResult(
			string line, 
			ref string currentRequestLine, 
			HttpHeaderEntries currentEntries)
		{
			string key, value;

			// We found a blank line! Header ends here.
			if(string.IsNullOrWhiteSpace(line))
			{
				return false;
			}
			// We found a request line
			else if(currentRequestLine == null)
				currentRequestLine = line;
			// We found a header key:value line
			else
			{
				HttpHeaderLineParser.Parse(line, out key, out value);
				currentEntries.Add(key, value);
			}

			return true;
		}

		void RequireNonNullRequestLine()
		{
			if(null == _requestLine)
				throw new HttpBadRequestException(
					"Http header contains no request line."
					);
		}

		void RequireNonNullResult()
		{
			if(_result == null)
				throw new InvalidOperationException(
					"Result has not been built"
					);
		}
	}
}
