using System;
using System.IO;
using nsimpleeventstore.contract;

namespace nsimpleeventstore.adapters.eventrepositories
{
    /*
     * The event repository maintains a persistent 0-based array of events.
     * The array is write-once, i.e. an array element can only we written to/stored once.
     *
     * Events are stored in files whose name consists of their array index as a hex number, e.g. 000000000002F303.txt
     * for the array element 193283.
     */
    public class FilesInFolderEventRepository : IEventRepository
    {
        private readonly string _path;

        public FilesInFolderEventRepository(string path) {
            _path = path;
            if (Directory.Exists(_path) is false)
                Directory.CreateDirectory(_path);
        }
        
        
        public void Store(long index, IEvent e) {
            var text = EventSerialization.Serialize(e);
            Store(index, text);
        }

        private void Store(long index, string text) {
            if (index < 0) throw new InvalidOperationException("Event index must be >= 0!");
            
            var filepath = FilepathFor(index);
            if (File.Exists(filepath)) throw new InvalidOperationException($"Event with index {index} has already been stored and cannot be overwritten!");
            
            File.WriteAllText(filepath, text);
        }

        
        public IEvent Load(long index) {
            var text = File.ReadAllText(FilepathFor(index));
            return EventSerialization.Deserialize(text);
        }


        public long Count => Directory.GetFiles(_path).Length;

        public string Path => _path;
        
        
        private string FilepathFor(long index) => System.IO.Path.Combine(_path, $"{index:x16}.txt");

        
        public void Dispose() { }
    }
}