using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace nsimpleeventstore
{
    public class InMemoryEventstore : IEventstore
    {
        public event Action<(string Version, long FinalEventNumber, Event[] Events)> OnRecorded = _ => { };

        private readonly Lock _lock;
        private readonly List<Event> _events;
        
        
        public InMemoryEventstore() : this(new Event[0]) {}
        public InMemoryEventstore(IEnumerable<Event> events)  {
            _events = new List<Event>(events);
            _lock = new Lock();
        }


        public (string Version, long FinalEventNumber) Record(Event e, string expectedVersion = "") => Record(new[] {e}, expectedVersion);
        public (string Version, long FinalEventNumber) Record(Event[] events, string expectedVersion = "")
        {
            try {
                _lock.TryWrite(() => {
                    var n = _events.Count;
                    var currentVersion = n.ToString();

                    Check_for_version_conflict(currentVersion);
                    _events.AddRange(events);
                });

                var (version, finalEventNumber) = State;
                OnRecorded((version, finalEventNumber, events));
                return (version, finalEventNumber);
            } finally{}


            void Check_for_version_conflict(string currentVersion) {
                if (!string.IsNullOrEmpty(expectedVersion) && 
                    expectedVersion != currentVersion) throw new VersionNotFoundException($"Event store version conflict! Version '{expectedVersion}' expected, but is '{currentVersion}'!");
            }
        }

        public (string Version, Event[] Events) Replay(params Type[] eventTypes) => Replay(-1, eventTypes);
        public (string Version, Event[] Events) Replay(long firstEventNumber = -1) => Replay(firstEventNumber, new Type[0]);
        public (string Version, Event[] Events) Replay(long firstEventNumber, params Type[] eventTypes) {
            return _lock.TryRead(
                () => (_events.Count.ToString(),
                       Filter(AllEvents()).ToArray()));


            IEnumerable<Event> AllEvents() {
                var n = _events.Count;
                for (var i = firstEventNumber < 0 ? 0 : firstEventNumber; i < n; i++)
                    yield return _events[(int)i];
            }

            IEnumerable<Event> Filter(IEnumerable<Event> events) {
                if (eventTypes.Length <= 0) return events;
                
                var eventTypes_ = new HashSet<Type>(eventTypes);
                return events.Where(e => eventTypes_.Contains(e.GetType()));
            }
        }

        public (string Version, long FinalEventNumber) State             
            => _lock.TryRead(() =>  {
                var n = _events.Count;
                return (n.ToString(), n - 1);
            });

        
        public void Dispose() {}
    }
}