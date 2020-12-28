using System;
using System.Collections.Generic;
using System.Text;

namespace nsimpleeventstore.contract
{
    public sealed record EventId
    {
        public Guid Value { get; set; }
        public EventId() => Value = Guid.NewGuid();
        public EventId(Guid value) => Value = value;
        public override string ToString() => Value.ToString();
    }
}
