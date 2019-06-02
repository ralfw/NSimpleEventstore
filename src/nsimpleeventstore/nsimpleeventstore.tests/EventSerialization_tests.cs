using System;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace nsimpleeventstore.tests
{
    public class EventSerialization_tests
    {
        class TestEvent : Event
        {
            public string Foo;
        }
        
        
        private readonly ITestOutputHelper _output;

        public EventSerialization_tests(ITestOutputHelper output) { _output = output; }
        
        
        [Fact]
        public void Serialize()
        {
            var e = new TestEvent {Foo = "Hello!"};

            var result = EventSerialization.Serialize(e).Split('\n');
            
            Assert.StartsWith("nsimpleeventstore.tests.EventSerialization_tests+TestEvent", result[0]);
            Assert.StartsWith("{\"Foo\":\"Hello!\",\"Id\":\"", result[1]);
        }

        
        [Fact]
        public void Deserialize()
        {
            var e = new TestEvent {Foo = "Hello!"};
            var serialized = EventSerialization.Serialize(e);

            var result = (TestEvent)EventSerialization.Deserialize(serialized);
            
            Assert.NotSame(e, result);
            Assert.Equal(e.Id, result.Id);
            Assert.Equal(e.Foo, result.Foo);
        }
    }
}