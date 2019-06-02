using System;
using System.Collections.Generic;

namespace nsimpleeventstore
{
    public interface IEventstore : IDisposable
    {
        event Action<(string Version, long NumberOfFirstEvent, Event[] Events)> OnRecorded;
        
        (string Version, long FinalEventNumber) Record(Event e, string expectedVersion="");
        (string Version, long FinalEventNumber) Record(Event[] events, string expectedVersion="");
        
        (Event[] Events, string Version) Replay(long firstEventNumber=-1);
        (Event[] Events, string Version) Replay(long firstEventNumber=-1, params Type[] eventTypes);
    }
}