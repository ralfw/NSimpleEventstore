using System;

namespace nsimpleeventstore.contract
{
    public abstract class Event : IEvent 
    {
        public EventId Id { get; }
        protected Event() => Id = new EventId(Guid.NewGuid());
        protected Event(EventId id) =>  Id = id;
    }
}