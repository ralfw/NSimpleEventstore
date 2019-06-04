using System;
using System.Collections.Generic;

namespace nsimpleeventstore
{
    public class InMemoryEventstore : IEventstore
    {
        public event Action<(string Version, long FinalEventNumber, Event[] Events)> OnRecorded;
        
        
        public InMemoryEventstore() {}
        public InMemoryEventstore(IEnumerable<Event> events) {}

        
        public (string Version, long FinalEventNumber) Record(Event e, string expectedVersion = "")
        {
            throw new NotImplementedException();
        }

        public (string Version, long FinalEventNumber) Record(Event[] events, string expectedVersion = "")
        {
            throw new NotImplementedException();
        }

        public (string Version, Event[] Events) Replay(long firstEventNumber = -1)
        {
            throw new NotImplementedException();
        }

        public (string Version, Event[] Events) Replay(params Type[] eventTypes)
        {
            throw new NotImplementedException();
        }

        public (string Version, Event[] Events) Replay(long firstEventNumber, params Type[] eventTypes)
        {
            throw new NotImplementedException();
        }

        public (string Version, long FinalEventNumber) State { get; }

        
        public void Dispose() {}
    }
}