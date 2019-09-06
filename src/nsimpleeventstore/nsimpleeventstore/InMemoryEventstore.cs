using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using nsimpleeventstore.adapters.eventrepositories;

namespace nsimpleeventstore
{
    public class InMemoryEventstore : Eventstore<InMemoryEventRepository>
    {
        public InMemoryEventstore() : base("") { }
    }
}