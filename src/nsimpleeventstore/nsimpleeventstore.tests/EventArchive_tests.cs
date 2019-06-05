using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Xunit;

namespace nsimpleeventstore.tests
{
    public class EventArchive_tests
    {
        class TestEvent : Event
        {
            public string Foo;
        }

        class AnotherTestEvent : Event
        {
            public int Bar;
        }
        
        
        [Fact]
        public void Store_and_load()
        {
            const string FILENAME = nameof(EventArchive_tests) + "_" + nameof(Store_and_load) + ".json";
            if (File.Exists(FILENAME)) File.Delete(FILENAME);
            
            var events = new Event[] {
                new TestEvent{Foo = "a"},
                new AnotherTestEvent{Bar = 1},
                new TestEvent{Foo = "b"}
            };
            
            EventArchive.Write(FILENAME, events);
            var result = EventArchive.Read(FILENAME).ToArray();
            
            Assert.Equal("a", ((TestEvent)result[0]).Foo);
            Assert.Equal(1, ((AnotherTestEvent)result[1]).Bar);
            Assert.Equal("b", ((TestEvent)result[2]).Foo);
        }
    }
}