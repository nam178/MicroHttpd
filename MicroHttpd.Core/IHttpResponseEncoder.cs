using System;
using System.IO;
using System.Threading.Tasks;

namespace MicroHttpd.Core
{
	/// <summary>
	/// Responsible for encoding the body of the Http message.
	/// 
	/// For instance we have a 'Passthrough' encoder which does 
	/// nothing, used for fixed-length content (Content-Length header is set),
	/// and a 'chunked' encoder, used when the length of content is unknown.
	/// </summary>
	interface IHttpResponseEncoder
    {
		/// <summary>
		/// Continue encoding the specified data into the target stream
		/// </summary>
		Task AppendAsync(byte[] buffer, int offset, int count);

		/// <summary>
		/// Continue encoding the specified data into the target stream,
		/// 
		/// Non-async version, can be used in the caller's Dispose method.
		/// </summary>
		void Append(byte[] buffer, int offset, int count);

		/// <summary>
		/// Called to tell the encoder to flush any unwritten data into
		/// the target stream.
		/// </summary>
		Task CompleteAsync();

		/// <summary>
		/// Called to tell the encoder to flush any unwritten data into
		/// the target stream.
		/// 
		/// Non-async version, can be used in the caller's Dispose method.
		/// </summary>
		void Complete();
	}
}
