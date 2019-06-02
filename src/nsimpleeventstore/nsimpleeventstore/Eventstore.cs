using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;

namespace nsimpleeventstore
{
    public class Eventstore : IEventstore
    {
        private const string DEFAUL_PATH = "eventstore.db";
        private const int LOCK_ACQUISITION_TIMEOUT_MS = 5000;
        
        public event Action<long, Event[]> OnRecorded = (t,es) => { };

        
        private readonly ReaderWriterLock _lock;
        private readonly EventRepository _repo;
        
        
        public Eventstore() : this(DEFAUL_PATH) {}
        public Eventstore(string path) {
            _repo = new EventRepository(path);
            _lock = new ReaderWriterLock();
        }

        
        public long Record(Event e, long version=-1) => Record(new[] {e}, version);
        public long Record(Event[] events, long version=-1) {
            _lock.AcquireWriterLock(LOCK_ACQUISITION_TIMEOUT_MS);
            try {
                var n = _repo.Count;
                Check_for_version_conflict(n);
                Store_all_events(n);
                OnRecorded(n+events.Length, events);

                return n;
            }
            finally {
                _lock.ReleaseWriterLock();
            }


            void Check_for_version_conflict(long currentVersion) {
                if (version >= 0 && version != currentVersion) throw new VersionNotFoundException($"Event store version conflict! Version {version} expected, but is {currentVersion}!");
            }

            void Store_all_events(long index) => events.ToList().ForEach(e => _repo.Store(index++, e));
        }


        public (Event[] Events, long Version) Replay() => Replay(new Type[0]);
        public (Event[] Events, long Version) Replay(params Type[] eventTypes) {
            _lock.AcquireReaderLock(LOCK_ACQUISITION_TIMEOUT_MS);
            try {
                return (Filter(AllEvents()).ToArray(),
                        _repo.Count);
            }
            finally  {
                _lock.ReleaseReaderLock();
            }


            IEnumerable<Event> AllEvents() {
                var n = _repo.Count;
                for (long i = 0; i < n; i++)
                    yield return _repo.Load(i);
            }

            IEnumerable<Event> Filter(IEnumerable<Event> events) {
                if (eventTypes.Length > 0) {
                    var eventTypes_ = new HashSet<Type>(eventTypes);
                    events = events.Where(e => eventTypes_.Contains(e.GetType()));
                }
                return events;
            }
        }


        public void Dispose() { _repo.Dispose(); }
    }
}