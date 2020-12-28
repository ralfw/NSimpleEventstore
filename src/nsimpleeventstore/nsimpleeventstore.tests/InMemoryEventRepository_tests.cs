using System;
using nsimpleeventstore.adapters.eventrepositories;
using nsimpleeventstore.contract;
using Xunit;

namespace nsimpleeventstore.tests
{
    public class InMemoryEventRepository_tests
    {
        class TestEvent : IEvent
        {
            public string Foo;

            public TestEvent() { Id = new EventId(); }
            public EventId Id { get; set; }
        }

        [Fact]
        public void No_events_in_empty_repo()
        {
            using (var sut = new InMemoryEventRepository(""))
            {
                Assert.Equal(0, sut.Count);
            }
        }

        [Fact]
        public void Counting_events()
        {
            using (var sut = new InMemoryEventRepository(""))
            {
                sut.Store(0, new TestEvent());
                Assert.Equal(1, sut.Count);
                sut.Store(1, new TestEvent());
                Assert.Equal(2, sut.Count);
            }
        }

        [Fact]
        public void Index_must_be_geq_0()
        {
            using (var sut = new InMemoryEventRepository(""))
            {
                Assert.Throws<InvalidOperationException>(() => sut.Store(-1, new TestEvent()));
                Assert.Equal(0, sut.Count);
            }
        }

        [Fact]
        public void Store_once()
        {
            using (var sut = new InMemoryEventRepository(""))
            {
                sut.Store(0, new TestEvent());

                Assert.Throws<InvalidOperationException>(() => sut.Store(0, new TestEvent()));
                Assert.Equal(1, sut.Count);
            }
        }

        [Fact]
        public void Store_and_load()
        {
            using (var sut = new InMemoryEventRepository(""))
            {
                var e = new TestEvent { Foo = "Hello" };
                sut.Store(0, e);

                var result = (TestEvent)sut.Load(0);

                Assert.Equal(e, result);
                Assert.Equal(e.Id, result.Id);
                Assert.Equal(e.Foo, result.Foo);
            }
        }

    }
}
