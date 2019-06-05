using System;
using System.Collections.Generic;

namespace nsimpleeventstore
{
    public class InMemoryEventstore : IEventstore
    {
        public event Action<(string Version, long FinalEventNumber, Event[] Events)> OnRecorded;

        private readonly  List<Event> _events;
        
        public InMemoryEventstore() : this(new Event[0]) {}
        public InMemoryEventstore(IEnumerable<Event> events) { _events = new List<Event>(events); }


        public (string Version, long FinalEventNumber) Record(Event e, string expectedVersion = "") => Record(new[] {e}, expectedVersion);
        public (string Version, long FinalEventNumber) Record(Event[] events, string expectedVersion = "")
        {
            // lock!
            throw new NotImplementedException();
        }

        public (string Version, Event[] Events) Replay(params Type[] eventTypes) => Replay(-1, eventTypes);
        public (string Version, Event[] Events) Replay(long firstEventNumber = -1) => Replay(firstEventNumber, new Type[0]);
        public (string Version, Event[] Events) Replay(long firstEventNumber, params Type[] eventTypes)
        {
            throw new NotImplementedException();
        }

        public (string Version, long FinalEventNumber) State { get; }

        
        public void Dispose() {}
    }
}