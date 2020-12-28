using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using nsimpleeventstore.adapters;
using nsimpleeventstore.adapters.eventrepositories;
using nsimpleeventstore.contract;

namespace nsimpleeventstore
{
    /*
     * Events are stored by expanding a persistent array incrementally per event.
     * 
     * The chronological sequence of events is maintained by numbering events in their chronological order
     * starting with 0.
     *
     * The event store is thread-safe; only one batch of events gets processed a at time.
     * (Many threads can replay events, though, but only one can record new ones.)
     * There must be only one event store instance for a certain event store path within a process.
     * Several processes working on the same event store path need to share an event store instance via
     * a common event store server process. Only that way the consistency of the events stored in a path
     * can be guaranteed.
     *
     * The event store is versioned. The version number is opaque to clients; they should not expect version numbers
     * to be ordered in any way or increase over time. The version changes whenever events got recorded.
     *
     * Storing a batch of events is not transactional. But due to the thread-safety of the event store and the
     * simplicity of the persistence approach it is assumed that failure during writing the events to files
     * is very unlikely.
     */
    public class Eventstore<T> : IEventstore where T : IEventRepository
    {
        private const string DEFAUL_PATH = "eventstore.db";

        public event Action<IEvent[]> OnRecorded = (e) => { };

        private readonly Lock _lock;
        private readonly IEventRepository _repo;

        public Eventstore() : this(DEFAUL_PATH) { }
        public Eventstore(string path)
        {
            _repo = (T)Activator.CreateInstance(typeof(T), path);
            _lock = new Lock();
        }

        /*
        public long Record(Event e, long expectedEventNumber = -1) => Record(new[] { e }, expectedEventNumber);
        public long Record(Event[] events, long expectedEventNumber = -1)
        {
            try
            {
                _lock.TryWrite(() =>
                {
                    var nextEventNumber = _repo.Count;
                    Check_for_version_conflict(nextEventNumber);
                    Store_all_events(nextEventNumber);
                });

                var finalEventNumber = State;
                OnRecorded(finalEventNumber, events);
                return finalEventNumber;
            }
            finally { }


            void Check_for_version_conflict(long nextEventNumber)
            {
                if (expectedEventNumber >= 0 &&
                    expectedEventNumber != nextEventNumber) throw new VersionNotFoundException($"Event store version conflict! Version '{expectedEventNumber}' expected, but is '{nextEventNumber}'!");
            }

            void Store_all_events(long index) => events.ToList().ForEach(e => _repo.Store(index++, e));
        }

        public (long FinalEventNumber, Event[] Events) Replay(long firstEventNumber = -1) => Replay(firstEventNumber, new Type[0]);
        public (long FinalEventNumber, Event[] Events) Replay(params Type[] eventTypes) => Replay(-1, eventTypes);
        public (long FinalEventNumber, Event[] Events) Replay(long firstEventNumber, params Type[] eventTypes)
        {
            return _lock.TryRead(
                () => (_repo.Count - 1,
                       Filter(AllEvents()).ToArray()));


            IEnumerable<Event> AllEvents()
            {
                var n = _repo.Count;
                for (var i = firstEventNumber < 0 ? 0 : firstEventNumber; i < n; i++)
                    yield return _repo.Load(i);
            }

            IEnumerable<Event> Filter(IEnumerable<Event> events)
            {
                if (eventTypes.Length <= 0) return events;

                var eventTypes_ = new HashSet<Type>(eventTypes);
                return events.Where(e => eventTypes_.Contains(e.GetType()));
            }
        }

        public long State
            => _lock.TryRead(() =>
            {
                return _repo.Count - 1;
            });
        */

        public string Path => _repo.Path;

        public EventId LastEventId
        {
            get 
            {
                var lastIndex = _repo.Count - 1;
                if (lastIndex >= 0) return _repo.Load(lastIndex).Id;
                return null; 
            }
        }

        public void Dispose() { _repo.Dispose(); }
       
        public IEnumerable<IEvent> Replay(EventId startEventId = null)
        {
            return _lock.TryRead(() => (Filter(AllEvents())));

            IEnumerable<IEvent> AllEvents()
            {
                var n = _repo.Count;
                for (var i = 0; i < n; i++)
                    yield return _repo.Load(i);
            }

            IEnumerable<IEvent> Filter(IEnumerable<IEvent> events)
            {
                if (startEventId == null) return events;
                return events.SkipWhile(x => x.Id.Equals(startEventId) is false);
            }
        }        

        public void Record(EventId expectedLastEventId, params IEvent[] events)
        {
            try
            {
                _lock.TryWrite(() =>
                {
                    var nextEventNumber = _repo.Count;
                    Check_for_version_conflict(expectedLastEventId);
                    Store_all_events(nextEventNumber);
                });
                
                OnRecorded(events);                
            }
            finally { }

            void Check_for_version_conflict(EventId expectedLastEventId)
            {
                if (expectedLastEventId != null && expectedLastEventId.Equals(LastEventId) is false) throw new VersionNotFoundException($"Event store version conflict! Version '{expectedLastEventId}' expected, but is '{LastEventId}'!");
            }

            void Store_all_events(long index) => events.ToList().ForEach(e => _repo.Store(index++, e));
        }
    }
}