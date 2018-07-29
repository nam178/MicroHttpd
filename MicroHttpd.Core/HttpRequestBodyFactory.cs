using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace MicroHttpd.Core
{
	sealed class HttpRequestBodyFactory : IHttpRequestBodyFactory
	{
		public ReadOnlyStream Create(
			TcpSettings tcpSettings, 
			HttpRequestHeader requestHeader, 
			RollbackableStream requestStream)
		{
			// Message body length https://tools.ietf.org/html/rfc7230#section-3.3.3
			// 1.
			// If a Transfer - Encoding header field is present and the chunked
			// transfer coding(Section 4.1) is the final encoding, the message
			// body length is determined by reading and decoding the chunked
			// data until the transfer coding indicates the data is complete.
			if(requestHeader.ContainsKey(HttpKeys.TransferEncoding))
			{
				var encodings = Split(requestHeader.Get(HttpKeys.TransferEncoding, false).Last());
				var finalEncoding = encodings[encodings.Length - 1];
				if(HeaderValueEquals(finalEncoding, HttpKeys.ChunkedValue))
				{
					// If a message is received with both a Transfer - Encoding and a
					// Content - Length header field, the Transfer - Encoding overrides the
					// Content - Length.Such a message might indicate an attempt to
					// perform request smuggling(Section 9.5) or response splitting
					// (Section 9.4) and ought to be handled as an error.A sender MUST
					// remove the received Content - Length field prior to forwarding such
					// a message downstream.
					if(requestHeader.ContainsKey(HttpKeys.ContentLength))
						requestHeader.Remove(HttpKeys.ContentLength);

					return  CreateChunkedRequestBody(requestStream, requestHeader, tcpSettings);
				}
				// If a Transfer - Encoding header field
				// is present in a request and the chunked transfer coding is not
				// the final encoding, the message body length cannot be determined
				// reliably; the server MUST respond with the 400(Bad Request)
				// status code and then close the connection.
				else ThrowForLastTransferEncodingIsNotChunked();
			}
			// If a message is received without Transfer - Encoding and with
			// either multiple Content - Length header fields having differing
			// field - values or a single Content-Length header field having an
			// invalid value, then the message framing is invalid and the
			// recipient MUST treat it as an unrecoverable error.  If this is a
			// request message, the server MUST respond with a 400(Bad Request)
			// status code and then close the connection.
			//
			// If a valid Content-Length header field is present without
			// Transfer - Encoding, its decimal value defines the expected message
			// body length in octets.
			else if(requestHeader.ContainsKey(HttpKeys.ContentLength))
			{
				return CreateFixedLengthRequestBody(requestStream, requestHeader.GetContentLength());
			}
			// If this is a request message and none of the above are true, then
			// the message body length is zero (no message body is present).
			else
			{
				return CreateEmptyRequestBody();
			}

			// Woudln't reach here,
			// But just to make the compiler happy.
			return null; 
		}

		/// <summary>
		/// Split header field value, i.e. 'gzip, chunked' => array of ['gzip', 'chunked']
		/// </summary>
		/// <param name="input"></param>
		static string[] Split(string input)
		{
			if(input == null)
				throw new ArgumentNullException(nameof(input));
			return input.Split(',').Select(w => w.Trim()).ToArray();
		}

		static HttpFixedLengthRequestBody CreateFixedLengthRequestBody(
			RollbackableStream requestStream, 
			long contentLength)
		{
			return new HttpFixedLengthRequestBody(
				requestStream,
				contentLength);
		}

		static HttpChunkedRequestBody CreateChunkedRequestBody(
			RollbackableStream requestStream, 
			HttpRequestHeader requestHeader, 
			TcpSettings tcpSettings)
		{
			return new HttpChunkedRequestBody(
				requestStream, requestHeader, tcpSettings);
		}

		static void ThrowForLastTransferEncodingIsNotChunked()
		{
			throw new HttpInvalidMessageException(
				$"The last {HttpKeys.TransferEncoding} header " +
				$"value must be '{HttpKeys.ChunkedValue}'"
				);
		}

		static bool HeaderValueEquals(string actual, string expected)
			=> string.Compare(actual, expected, true, CultureInfo.InvariantCulture) == 0;

		static MemoryStream _empty = new MemoryStream();
		static ReadOnlyStream CreateEmptyRequestBody() => new HttpFixedLengthRequestBody(_empty, 0);
	}
}
