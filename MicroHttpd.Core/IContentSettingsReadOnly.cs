namespace MicroHttpd.Core
{
	public interface IContentSettingsReadOnly
	{
		/// <summary>
		/// Default charset parameter for text contents, 
		/// used in Content-Type header field.
		/// 
		/// Example: 'utf-8' for Content-Type: text/html; charset=utf-8
		/// </summary>
		string DefaultCharsetForTextContents
		{ get; }
	}
}