using LiteDB;
using nsimpleeventstore.contract;
using System;

namespace nsimpleeventstore.adapters.eventrepositories
{
    public class LiteDBEventRepository : IEventRepository
    {
        class EventDocument
        {
            public long Id { get; set; }
            public string Text { get; set; }
        }

        private readonly LiteDatabase _db;
        private readonly ILiteCollection<EventDocument> _directory;

        public LiteDBEventRepository(string path)
        {
            Path = path;
            _db = new LiteDatabase(path);
            _directory = _db.GetCollection<EventDocument>("events");
            _directory.EnsureIndex(x => x.Id, true);
        }

        public Event Load(long index)
        {
            if (index < 0) throw new InvalidOperationException("Event index must be >= 0!");
            var doc = _directory.FindOne(x => x.Id.Equals(index));
            if (doc is null) throw new InvalidOperationException($"Event with index {index} was not stored!");

            return EventSerialization.Deserialize(doc.Text);
        }

        public void Store(long index, Event e)
        {
            var text = EventSerialization.Serialize(e);
            Store(index, text);
        }

        private void Store(long index, string text)
        {
            if (index < 0) throw new InvalidOperationException("Event index must be >= 0!");
            if (_directory.Exists(x => x.Id.Equals(index))) throw new InvalidOperationException($"Event with index {index} has already been stored and cannot be overwritten!");

            _directory.Insert(index, new EventDocument() { Id = index, Text = text });
        }

        public long Count => _directory.Count();

        public string Path { get; }

        public void Dispose() => _db.Dispose();
    }
}
