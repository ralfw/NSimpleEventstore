    using System;
using System.Collections.Generic;
using nsimpleeventstore.contract;

namespace nsimpleeventstore.adapters.eventrepositories
{
    /*
     * The event repository maintains a persistent 0-based array of events.
     * The array is write-once, i.e. an array element can only we written to/stored once.     
     */
    public class InMemoryEventRepository : IEventRepository
    {
        private readonly Dictionary<long, Event> _directory = new Dictionary<long, Event>();
        
        public InMemoryEventRepository(string path) {
            Path = path; //not used
        }

        public void Store(long index, Event e) {
            if (index < 0) throw new InvalidOperationException("Event index must be >= 0!");
            if (_directory.ContainsKey(index)) throw new InvalidOperationException($"Event with index {index} has already been stored and cannot be overwritten!");
            
            _directory.Add(index, e);
        }

        public Event Load(long index) {
            if (index < 0) throw new InvalidOperationException("Event index must be >= 0!");
            if (_directory.ContainsKey(index) is false) throw new InvalidOperationException($"Event with index {index} was not stored!");

            return _directory[index];
        }

        public long Count => _directory.Count;

        public string Path { get; }

        public void Dispose() { }
    }
}