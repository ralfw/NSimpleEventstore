using System;

namespace nsimpleeventstore.contract
{
    public interface IEventstore : IDisposable
    {
        event Action<long, Event[]> OnRecorded;
        
        long Record(Event e, long expectedEventNumber = -1);
        long Record(Event[] events, long expectedEventNumber = -1);
        
        (long FinalEventNumber, Event[] Events) Replay(long firstEventNumber = -1);
        (long FinalEventNumber, Event[] Events) Replay(params Type[] eventTypes);
        (long FinalEventNumber, Event[] Events) Replay(long firstEventNumber, params Type[] eventTypes);
        
        long State { get; }
    }
}