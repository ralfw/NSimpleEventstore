using System;
using System.IO;
using Xunit;

namespace nsimpleeventstore.tests
{
    public class EventRepository_tests
    {
        class TestEvent : Event
        {
            public string Foo;
        }
        
        
        [Fact]
        public void No_events_in_empty_repo() {
            const string PATH = nameof(EventRepository_tests) + "_" + nameof(No_events_in_empty_repo);
            if (Directory.Exists(PATH)) Directory.Delete(PATH, true);

            using (var sut = new EventRepository(PATH)) {
                Assert.Equal(0, sut.Count);
            }
        }
        
        [Fact]
        public void Counting_events() {
            const string PATH = nameof(EventRepository_tests) + "_"  + nameof(Counting_events);
            if (Directory.Exists(PATH)) Directory.Delete(PATH, true);
            using (var sut = new EventRepository(PATH)) {
                sut.Store(0, new TestEvent());
                Assert.Equal(1, sut.Count);
                sut.Store(1, new TestEvent());
                Assert.Equal(2, sut.Count);
            }
        }
        
        [Fact]
        public void Index_must_be_geq_0() {
            const string PATH = nameof(EventRepository_tests) + "_" + nameof(Index_must_be_geq_0);
            if (Directory.Exists(PATH)) Directory.Delete(PATH, true);
            using (var sut = new EventRepository(PATH)) {
                Assert.Throws<InvalidOperationException>(() => sut.Store(-1, new TestEvent()));
                Assert.Equal(0, sut.Count);
            }
        }
        
        
        [Fact]
        public void Store_once() {
            const string PATH = nameof(EventRepository_tests) + "_"  + nameof(Store_once);
            if (Directory.Exists(PATH)) Directory.Delete(PATH, true);
            using (var sut = new EventRepository(PATH)) {
                sut.Store(0, new TestEvent());

                Assert.Throws<InvalidOperationException>(() => sut.Store(0, new TestEvent()));
                Assert.Equal(1, sut.Count);
            }
        }
        
        
        [Fact]
        public void Store_and_load() {
            const string PATH = nameof(EventRepository_tests) + "_"  + nameof(Store_and_load);
            if (Directory.Exists(PATH)) Directory.Delete(PATH, true);
            using (var sut = new EventRepository(PATH)) {
                var e = new TestEvent {Foo = "Hello"};
                sut.Store(0, e);

                var result = (TestEvent) sut.Load(0);

                Assert.NotSame(e, result);
                Assert.Equal(e.Id, result.Id);
                Assert.Equal(e.Foo, result.Foo);
            }
        }
    }
}