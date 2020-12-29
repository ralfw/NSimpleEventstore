using System;
using System.Collections.Generic;

namespace nsimpleeventstore.contract
{
    public interface IEventstore : IDisposable
    {
        event Action<IEvent[]> OnRecorded;
        EventId LastEventId { get; } //returns the last recorded Event.Id
        IEnumerable<IEvent> Replay();
        IEnumerable<IEvent> Replay(EventId startEventId);
        void Record(EventId expectedLastEventId, params IEvent[] events);
    }
}