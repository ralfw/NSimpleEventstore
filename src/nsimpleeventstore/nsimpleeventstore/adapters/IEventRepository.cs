using System;

namespace nsimpleeventstore
{
    public interface IEventRepository : IDisposable
    {
        long Count { get; }
        string Path { get; }

        Event Load(long index);
        void Store(long index, Event e);
    }
}