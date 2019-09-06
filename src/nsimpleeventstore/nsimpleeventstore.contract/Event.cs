using System;

namespace nsimpleeventstore.contract
{
    /*
     * All events to be stored in the event store have to be derived
     * from the Event class. Also they need to be JSON-(de)serializable.
     * That means they need a public parameter-less ctor.
     */
    public abstract class Event {
        public string Id { get; set; }

        protected Event() { Id = Guid.NewGuid().ToString(); }
    }
}