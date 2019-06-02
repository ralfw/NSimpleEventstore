using System;
using System.Collections.Generic;

namespace nsimpleeventstore
{
    public interface IEventstore : IDisposable
    {
        event Action<long,Event[]> OnRecorded;
        
        long Record(Event e);
        long Record(Event[] events);
        
        IEnumerable<Event> Replay();
        IEnumerable<Event> Replay(params Type[] eventTypes);
    }
}