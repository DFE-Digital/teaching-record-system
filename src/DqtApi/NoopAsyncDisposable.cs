using System;
using System.Threading.Tasks;

namespace DqtApi
{
    public sealed class NoopAsyncDisposable : IAsyncDisposable
    {
        private NoopAsyncDisposable()
        {
        }

        public static NoopAsyncDisposable Instance { get; } = new NoopAsyncDisposable();

        public ValueTask DisposeAsync() => default;
    }
}
