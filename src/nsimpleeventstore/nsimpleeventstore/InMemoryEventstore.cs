using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using nsimpleeventstore.adapters.eventrepositories;
using nsimpleeventstore.contract;

namespace nsimpleeventstore
{
    public class InMemoryEventstore : Eventstore<InMemoryEventRepository>
    {
        public InMemoryEventstore() : this(new Event[0]) {}
        public InMemoryEventstore(IEnumerable<Event> events) : base("") {
            this.Record(events.ToArray());
        }
    }
}