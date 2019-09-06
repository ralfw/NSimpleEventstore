using System;

namespace nsimpleeventstore.contract
{
    public interface IEventstore : IDisposable
    {
        event Action<string, long, Event[]> OnRecorded;
        
        (string Version, long FinalEventNumber) Record(Event e, string expectedVersion="");
        (string Version, long FinalEventNumber) Record(Event[] events, string expectedVersion="");
        
        (string Version, Event[] Events) Replay(long firstEventNumber=-1);
        (string Version, Event[] Events) Replay(params Type[] eventTypes);
        (string Version, Event[] Events) Replay(long firstEventNumber, params Type[] eventTypes);
        
        (string Version, long FinalEventNumber) State { get; }
    }
}