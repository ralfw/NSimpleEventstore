using System;
using System.Collections.Generic;
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

        
        public long Record(Event e) => Record(new[] {e});
        public long Record(Event[] events) {
            _lock.AcquireWriterLock(LOCK_ACQUISITION_TIMEOUT_MS);
            try {
                var n = _repo.Count;
                events.ToList().ForEach(e => _repo.Store(n++, e));
                OnRecorded(n, events);

                return n;
            }
            finally {
                _lock.ReleaseWriterLock();
            }
        }


        public IEnumerable<Event> Replay() => Replay(new Type[0]);
        public IEnumerable<Event> Replay(params Type[] eventTypes) {
            _lock.AcquireReaderLock(LOCK_ACQUISITION_TIMEOUT_MS);
            try {
                return Filter(AllEvents());
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