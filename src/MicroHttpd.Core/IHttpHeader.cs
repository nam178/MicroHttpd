namespace MicroHttpd.Core
{
    public interface IHttpHeader : IHttpHeaderReadOnly
	{
		new string this[StringCI key]
		{ get; set; }

		void Add(StringCI key, string value);

		void Remove(StringCI key);
	}
}