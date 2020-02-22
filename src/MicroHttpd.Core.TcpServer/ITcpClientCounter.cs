namespace MicroHttpd.Core.TcpServer
{
    interface ITcpClientCounter
    {
        int Count { get; }

        /// <summary>
        /// Increase the count for connected TCP clients, use this when the client is connected.
        /// </summary>
        void Increase();

        /// <summary>
        /// Decrease the count for connected TCP clients, use this when the client disconnects;
        /// </summary>
        void Decrease();
    }
}
