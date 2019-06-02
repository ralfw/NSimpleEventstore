using System;
using System.Collections.Generic;

namespace nsimpleeventstore
{
    public interface IEventstore : IDisposable
    {
        event Action<long,Event[]> OnRecorded;
        
        long Record(Event e, long version=-1);
        long Record(Event[] events, long version=-1);
        
        (Event[] Events, long Version) Replay();
        (Event[] Events, long Version) Replay(params Type[] eventTypes);
    }
}