using System;
using System.Linq;
using Newtonsoft.Json;
using nsimpleeventstore.adapters.eventrepositories;
using nsimpleeventstore.contract;
using Xunit;
using Xunit.Abstractions;

namespace nsimpleeventstore.tests
{
    public class EventSerialization_scenario_tests
    {
        public class MyEvent : IEvent
        {
            public AbstractClass Item { get; set; }

            public MyEvent() { Id = new EventId(); }
            public EventId Id { get; set; }
        }

        public class AbstractClass { }

        public class ConcreteClass : AbstractClass
        {
            public int Number { get; set; }
        }

        private readonly ITestOutputHelper _output;

        public EventSerialization_scenario_tests(ITestOutputHelper output) { _output = output; }
        
        
        [Fact]
        public void EventSerialization_acceptance_test()
        {
            var myEvent = new MyEvent { Item = new ConcreteClass { Number = 2, }, };

            EventSerialization.JsonSerializerSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto, };

            var serializationResult = EventSerialization.Serialize(myEvent);
            var deserializationResult = EventSerialization.Deserialize(serializationResult);

            Assert.IsType<ConcreteClass>(((MyEvent)deserializationResult).Item);
        }
    }
}