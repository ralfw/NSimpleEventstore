using System;
using nsimpleeventstore.contract;

namespace nsimpleeventstore.adapters.eventrepositories
{
    public interface IEventRepository : IDisposable
    {
        long Count { get; }
        string Path { get; }
        IEvent Load(long index);
        void Store(long index, IEvent e);
    }
}