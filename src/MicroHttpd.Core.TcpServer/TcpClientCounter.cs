using System.Threading;

namespace MicroHttpd.Core.TcpServer
{
    sealed class TcpClientCounter : ITcpClientCounter
    {
        int _count;

        public int Count  => Interlocked.CompareExchange(ref _count, 0, 0);

        public void Decrease() => Interlocked.Decrement(ref _count);

        public void Increase() => Interlocked.Increment(ref _count);
    }
}
