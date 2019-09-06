using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace nsimpleeventstore
{
    public class InMemoryEventstore : Eventstore<MemoryRepository>
    {
        public InMemoryEventstore() : base() { }
        public InMemoryEventstore(string path) : base(path) { }
    }
}