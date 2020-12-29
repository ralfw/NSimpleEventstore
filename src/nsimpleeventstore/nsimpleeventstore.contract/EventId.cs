using System;

namespace nsimpleeventstore.contract
{
    public sealed record EventId
    {
        public Guid Value { get; set; }
        private EventId() { }
        public EventId(Guid value) => Value = value;
    }
}
