using System.IO;
using Xunit;

namespace nsimpleeventstore.tests
{
    public class EventRepository_tests
    {
        public class TestEvent : Event
        {
            public string Foo;
        }
        
        
        [Fact]
        public void No_events_in_empty_repo() {
            const string PATH = "test_no_events";
            if (Directory.Exists(PATH)) Directory.Delete(PATH, true);

            using (var sut = new EventRepository(PATH)) {
                Assert.Equal(0, sut.Count);
            }
        }
        
        [Fact]
        public void Counting_events() {
            const string PATH = "counting_events";
            if (Directory.Exists(PATH)) Directory.Delete(PATH, true);
            using (var sut = new EventRepository(PATH)) {
                sut.TryStore(0, new TestEvent());
                Assert.Equal(1, sut.Count);
                sut.TryStore(1, new TestEvent());
                Assert.Equal(2, sut.Count);
            }
        }
        
        [Fact]
        public void Index_must_be_geq_0() {
            const string PATH = "index_geq_0";
            if (Directory.Exists(PATH)) Directory.Delete(PATH, true);
            using (var sut = new EventRepository(PATH)) {
                var result = sut.TryStore(-1, new TestEvent());

                Assert.False(result);
                Assert.Equal(0, sut.Count);
            }
        }
        
        
        [Fact]
        public void Store_once() {
            const string PATH = "store_once";
            if (Directory.Exists(PATH)) Directory.Delete(PATH, true);
            using (var sut = new EventRepository(PATH)) {
                Assert.True(sut.TryStore(0, new TestEvent()));

                var result = sut.TryStore(0, new TestEvent());

                Assert.False(result);
                Assert.Equal(1, sut.Count);
            }
        }
        
        
        [Fact]
        public void Store_and_load() {
            const string PATH = "store_and_load";
            if (Directory.Exists(PATH)) Directory.Delete(PATH, true);
            using (var sut = new EventRepository(PATH)) {
                var e = new TestEvent {Foo = "Hello"};
                sut.TryStore(0, e);

                var result = (TestEvent) sut.Load(0);

                Assert.NotSame(e, result);
                Assert.Equal(e.Id, result.Id);
                Assert.Equal(e.Foo, result.Foo);
            }
        }
    }
}