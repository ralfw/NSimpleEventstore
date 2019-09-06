using System;
using Microsoft.Isam.Esent.Collections.Generic;
using nsimpleeventstore.contract;

namespace nsimpleeventstore.adapters.eventrepositories
{
    /*
     * The event repository maintains a persistent 0-based array of events.
     * The array is write-once, i.e. an array element can only we written to/stored once.     
     * The Esent DB can only contain 2^31 entries per collection, see https://github.com/microsoft/ManagedEsent/blob/master/Documentation/PersistentDictionaryDocumentation.md
     */
    public class EsentEventRepository : IEventRepository
    {
        private readonly PersistentDictionary<long, string> _directory;

        public EsentEventRepository(string path) {
            Path = path;
            _directory = new PersistentDictionary<long, string>(path);
        }        
        
        
        public void Store(long index, Event e) {
            var text = EventSerialization.Serialize(e);
            Store(index, text);
        }

        private void Store(long index, string text) {
            if (index < 0) throw new InvalidOperationException("Event index must be >= 0!");
            if (_directory.ContainsKey(index)) throw new InvalidOperationException($"Event with index {index} has already been stored and cannot be overwritten!");

            _directory.Add(index, text);            
        }
        
        
        public Event Load(long index) {
            if (index < 0) throw new InvalidOperationException("Event index must be >= 0!");
            if (_directory.ContainsKey(index) is false) throw new InvalidOperationException($"Event with index {index} was not stored!");
            
            var text = _directory[index];
            return EventSerialization.Deserialize(text);
        }

        
        public long Count => _directory.Count;

        public string Path { get; }

        public void Dispose() => _directory.Dispose();
    }
}