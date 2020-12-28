using System;
using System.IO;
using System.Runtime.InteropServices;
using nsimpleeventstore.adapters.eventrepositories;
using nsimpleeventstore.contract;
using Xunit;

namespace nsimpleeventstore.tests
{
    public class LiteDBEventRepository_tests
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
            const string PATH = nameof(LiteDBEventRepository_tests) + "_" + nameof(No_events_in_empty_repo);
            RemoveDB(PATH);

            using (var sut = new LiteDBEventRepository(PATH))
            {
                Assert.Equal(0, sut.Count);
            }
        }

        [Fact]
        public void Counting_events()
        {
            const string PATH = nameof(LiteDBEventRepository_tests) + "_" + nameof(Counting_events);
            RemoveDB(PATH);

            using (var sut = new LiteDBEventRepository(PATH))
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
            const string PATH = nameof(LiteDBEventRepository_tests) + "_" + nameof(Index_must_be_geq_0);
            RemoveDB(PATH);

            using (var sut = new LiteDBEventRepository(PATH))
            {
                Assert.Throws<InvalidOperationException>(() => sut.Store(-1, new TestEvent()));
                Assert.Equal(0, sut.Count);
            }
        }


        [Fact]
        public void Store_once()
        {
            const string PATH = nameof(LiteDBEventRepository_tests) + "_" + nameof(Store_once);
            RemoveDB(PATH);

            using (var sut = new LiteDBEventRepository(PATH))
            {
                sut.Store(0, new TestEvent());

                Assert.Throws<InvalidOperationException>(() => sut.Store(0, new TestEvent()));
                Assert.Equal(1, sut.Count);
            }
        }


        [Fact]
        public void Store_and_load()
        {
            const string PATH = nameof(LiteDBEventRepository_tests) + "_" + nameof(Store_and_load);
            RemoveDB(PATH);

            using (var sut = new LiteDBEventRepository(PATH))
            {
                var e = new TestEvent { Foo = "Hello" };
                sut.Store(0, e);

                var result = (TestEvent)sut.Load(0);

                Assert.NotSame(e, result);
                Assert.Equal(e.Id, result.Id);
                Assert.Equal(e.Foo, result.Foo);
            }
        }
        private void RemoveDB(string path)
        {
            string logPath = path + "-log";
            if (File.Exists(path)) File.Delete(path);
            if (File.Exists(logPath)) File.Delete(logPath);
        }
    }
}
