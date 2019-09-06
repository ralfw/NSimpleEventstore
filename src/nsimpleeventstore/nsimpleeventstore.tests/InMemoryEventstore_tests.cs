using System.Linq;
using nsimpleeventstore.contract;
using Xunit;

namespace nsimpleeventstore.tests
{
    public class InMemoryEventstore_tests
    {
        public class TestEvent : Event
        {
            public string Text;
        }
        
        
        [Fact]
        public void Initialize_record_and_replay()
        {
            var sut = new InMemoryEventstore(new[]
            {
                new TestEvent{Text = "foo"},
                new TestEvent{Text = "bar"}
            });

            sut.Record(new TestEvent {Text = "baz"});

            var result = sut.Replay().Events.Select(e => (e as TestEvent).Text);
            
            Assert.Equal(new[]{"foo", "bar", "baz"}, result);
        }
    }
}