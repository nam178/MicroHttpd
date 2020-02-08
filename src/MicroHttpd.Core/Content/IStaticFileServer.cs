using System.IO;

namespace MicroHttpd.Core.Content
{
	interface IStaticFileServer
	{
		/// <summary>
		/// Try to resolve the provided request to see if it is requesting a static file.
		/// </summary>
		/// <param name="resolvedPathToContentFile">If asking for a static file, the absolute path to that file.</param>
		/// <returns>True if the request asked for a static file</returns>
		bool TryResolve(
			IHttpRequest request, 
			out string resolvedPathToContentFile
			);

		/// <summary>
		/// Get the Content-Type header for the file at specified path.
		/// </summary>
		string GetContentTypeHeader(string pathToContentFile);

		/// <summary>
		/// Open the file for reading
		/// </summary>
		Stream OpenRead(string pathToContentFile);
	}
}