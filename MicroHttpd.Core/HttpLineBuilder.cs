using System;
using System.IO;
using System.Text;
using System.Threading;

namespace MicroHttpd.Core
{
	/// <summary>
	/// Utility to read binary data line-by-line.
	/// </summary>
	sealed class HttpLineBuilder : IDisposable
    {
		readonly MemoryStream _lineBuilder = new MemoryStream();
		readonly Encoding _encoding;

		string _result;
		public string Result
		{
			get {
				RequireNonDisposed();
				RequireResultNotNull();
				return _result;
			}
		}

		/// <summary>
		/// Clear result, ready to build the next line
		/// </summary>
		public void Reset()
		{
			_lineBuilder.SetLength(0);
			_result = null;
		}

		// HTTP spec, header should be in ASCII,
		// however ASCII have the exact same encoding as UTF8,
		// so let's default to UTF8
		public HttpLineBuilder() : this(Encoding.UTF8)
		{

		}

		public HttpLineBuilder(Encoding encoding)
		{
			_encoding = encoding;
		}

		/// <summary>
		/// Continue building the line by appending a buffer into this builder,
		/// this method returns true when the appended buffer has a newline
		/// character in it (\r\n or \n);
		/// </summary>
		/// <param name="nextLineStartIndex"></param>
		/// <returns></returns>
		public bool AppendBuffer(
			byte[] buffer, 
			int offset, 
			int count, 
			out int nextLineStartIndex)
		{
			RequireNonDisposed();
			RequireResultNull();
			Validation.RequireValidBuffer(buffer, offset, count);

			// Append those bytes to our temporary line
			_lineBuilder.Write(buffer, offset, count);

			// Now check our temp line see if it is ended
			var lineBuffer  = _lineBuilder.GetBuffer();
			for(var i = 0
				; i < _lineBuilder.Length /*tmp.Length, not tmpBuffer.Length!*/
				; i++)
			{
				// Yay! a new line detected,
				// Notes that according HTTP standard, \r\n is a newline,
				// however they recommended to support \n as well.
				// See https://stackoverflow.com/questions/5757290/http-header-line-break-style
				if(lineBuffer[i] == SpecialChars.NL)
				{
					SetResult(lineBuffer, i);
					nextLineStartIndex = i - ((int)_lineBuilder.Length - count) + offset + 1;
					return true;
				}
			}

			nextLineStartIndex = default(int);
			return false;
		}

		void SetResult(byte[] lineBuffer, int endOfLineCharIndex)
		{
			_result = _encoding.GetString(
				lineBuffer,
				0,
				// Ignore the previous character in the resulting
				// line if it is a CR character
				(int)(IsPreviousCharacterCR(lineBuffer, endOfLineCharIndex)
					? (endOfLineCharIndex - 1)
					: (endOfLineCharIndex))
				);
		}

		static bool IsPreviousCharacterCR(byte[] lineBuffer, int currentCharIndex)
		{
			return ((currentCharIndex - 1) >= 0) 
				&& (lineBuffer[currentCharIndex - 1] == SpecialChars.CR);
		}

		void RequireResultNotNull()
		{
			if(null == _result)
				throw new InvalidOperationException(
					"Line has not been built"
					);
		}

		void RequireResultNull()
		{
			if(null != _result)
				throw new InvalidOperationException(
					$"Line already built, call {nameof(Reset)} first."
					);
		}

		void RequireNonDisposed()
		{
			if(Interlocked.CompareExchange(ref _disposed, 0, 0) != 0)
				throw new ObjectDisposedException(
					GetType().FullName
					);
		}

		int _disposed;
		void IDisposable.Dispose()
		{
			if(Interlocked.CompareExchange(ref _disposed, 1, 0) == 0)
			{
				_lineBuilder.Dispose();
			}
		}
	}
}
