using System;
using System.Collections.Generic;
using System.Text;

namespace nsimpleeventstore.contract
{
    public interface IEvent
    {
        EventId Id { get; set; }
    }
}
