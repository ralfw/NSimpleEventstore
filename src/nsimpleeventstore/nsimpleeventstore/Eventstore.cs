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

        public IEnumerable<IEvent> Replay() => Replay(null);

        public IEnumerable<IEvent> Replay(EventId startEventId)
        {
            return _lock.TryRead(() => (Filter(AllEvents())));

            IEnumerable<IEvent> Filter(IEnumerable<IEvent> events)
            {
                if (startEventId == null) return events;
                return events.SkipWhile(x => x.Id.Equals(startEventId) is false);
            }

            IEnumerable<IEvent> AllEvents()
            {
                var n = _repo.Count;
                for (var i = 0; i < n; i++)
                    yield return _repo.Load(i);
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
                if (expectedLastEventId != null && expectedLastEventId.Equals(LastEventId) is false) 
                    throw new VersionNotFoundException($"Event store version conflict! Version '{expectedLastEventId}' expected, but is '{LastEventId}'!");
            }

            void Store_all_events(long eventNumber) => events.ToList().ForEach(e => _repo.Store(eventNumber++, e));
        }
    }
}