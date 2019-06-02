using System;
using System.IO;

namespace nsimpleeventstore
{
    /*
     * The event repository maintains a persistent 0-based array of events.
     * The array is write-once, i.e. an array element can only we written to/stored once.
     *
     * Events are stored in files whose name consists of their array index as a hex number, e.g. 000000000002F303.txt
     * for the array element 193283.
     */
    class EventRepository : IDisposable
    {
        private readonly string _path;

        public EventRepository(string path) {
            _path = path;
            if (Directory.Exists(_path) is false)
                Directory.CreateDirectory(_path);
        }
        
        
        public bool TryStore(long index, Event e) {
            var text = EventSerialization.Serialize(e);
            return TryStore(index, text);
        }

        private bool TryStore(long index, string text) {
            if (index < 0) return false;
            
            var filepath = FilepathFor(index);
            if (File.Exists(filepath)) return false;
            
            File.WriteAllText(filepath, text);
            return true;
        }

        
        public Event Load(long index) {
            var text = File.ReadAllText(FilepathFor(index));
            return EventSerialization.Deserialize(text);
        }


        public long Count => Directory.GetFiles(_path).Length;
        
        
        private string FilepathFor(long index) => Path.Combine(_path, $"{index:x16}.txt");

        
        public void Dispose() { }
    }
}