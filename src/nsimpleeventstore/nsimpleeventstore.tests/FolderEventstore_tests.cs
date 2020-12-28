using System;
using System.Data;
using System.IO;
using System.Linq;
using nsimpleeventstore.adapters.eventrepositories;
using nsimpleeventstore.contract;
using Xunit;

namespace nsimpleeventstore.tests
{
    public class FolderEventstore_tests
    {
        class TestEvent : IEvent
        {
            public string Foo;

            public TestEvent() { Id = new EventId(); }
            public EventId Id { get; set; }
        }

        class AnotherTestEvent : IEvent
        {
            public int Bar;

            public AnotherTestEvent() { Id = new EventId(); }
            public EventId Id { get; set; }
        }
        
        
        [Fact]
        public void Basic_recording_and_replaying()
        {
            const string PATH = nameof(FolderEventstore_tests) + "_" + nameof(Basic_recording_and_replaying);
            if (Directory.Exists(PATH)) Directory.Delete(PATH, true);
            var sut = new Eventstore<FilesInFolderEventRepository>(PATH);

            IEvent e0 = new TestEvent { Foo = "a" };
            sut.Record(null, e0);
            IEvent e1 = new TestEvent { Foo = "b" };
            IEvent e2 = new TestEvent { Foo = "c" };
            sut.Record(e0.Id, new[] {e1, e2});

            var result = sut.Replay();
            
            Assert.Equal(new[]{"a", "b", "c"}, result.Select(e => ((TestEvent)e).Foo).ToArray());
        }
        
        
        [Fact]
        public void Version_number_changes()
        {
            const string PATH = nameof(FolderEventstore_tests) + "_" + nameof(Version_number_changes);
            if (Directory.Exists(PATH)) Directory.Delete(PATH, true);
            var sut = new Eventstore<FilesInFolderEventRepository>(PATH);

            IEvent e0 = new TestEvent();
            sut.Record(null, e0);
            var result0 = sut.LastEventId;
            Assert.Equal(e0.Id, result0);

            IEvent e1 = new TestEvent();
            sut.Record(e0.Id, e1);
            var result1 = sut.LastEventId;
            Assert.Equal(e1.Id, result1);
            
            IEvent e2 = new TestEvent();
            IEvent e3 = new TestEvent();
            sut.Record(e1.Id, new[]{e2, e3});
            var result2 = sut.LastEventId;
            Assert.Equal(e3.Id, result2);
        }        
        
        [Fact]
        public void Notification_about_events_recorded()
        {
            const string PATH = nameof(FolderEventstore_tests) + "_" + nameof(Notification_about_events_recorded);
            if (Directory.Exists(PATH)) Directory.Delete(PATH, true);
            var sut = new Eventstore<FilesInFolderEventRepository>(PATH);

            IEvent[] result = (null);
            sut.OnRecorded += (e) => result = (e);

            IEvent e0 = new TestEvent { Foo = "a" };
            sut.Record(null, e0);
            Assert.Equal(e0.Id, result.Last().Id);
            Assert.Single(result);

            IEvent e1 = new TestEvent { Foo = "b" };
            IEvent e2 = new TestEvent { Foo = "c" };
            sut.Record(e0.Id, new[]{e1, e2});
            Assert.Equal(e2.Id, result.Last().Id);
            Assert.Equal(new[]{"b", "c"}, result.Select(e => ((TestEvent)e).Foo).ToArray());   
        }        
        
        [Fact]
        public void Replaying_a_subset()
        {
            const string PATH = nameof(FolderEventstore_tests) + "_" + nameof(Replaying_a_subset);
            if (Directory.Exists(PATH)) Directory.Delete(PATH, true);
            var sut = new Eventstore<FilesInFolderEventRepository>(PATH);

            IEvent e0 = new TestEvent { Foo = "a" };
            IEvent e1 = new AnotherTestEvent { Bar = 1 };
            IEvent e2 = new AnotherTestEvent { Bar = 2 };
            IEvent e3 = new TestEvent { Foo = "b" };
            IEvent e4 = new AnotherTestEvent { Bar = 3 };
            sut.Record(null, new IEvent[] {e0, e1, e2, e3, e4});

            var result = sut.Replay();
            Assert.Equal(e4.Id, result.Last().Id);
            Assert.Equal(5, result.Count());
        }
        
        
        [Fact]
        public void Replaying_from_event_id()
        {
            const string PATH = nameof(FolderEventstore_tests) + "_" + nameof(Replaying_from_event_id);
            if (Directory.Exists(PATH)) Directory.Delete(PATH, true);
            var sut = new Eventstore<FilesInFolderEventRepository>(PATH);

            IEvent e0 = new TestEvent { Foo = "a" };
            IEvent e1 = new AnotherTestEvent { Bar = 1 };
            IEvent e2 = new AnotherTestEvent { Bar = 2 };
            IEvent e3 = new TestEvent { Foo = "b" };
            IEvent e4 = new AnotherTestEvent { Bar = 3 };
            sut.Record(null, new IEvent[] {e0, e1, e2, e3, e4});

            Replay_from_somewhere_in_the_middle();
            Replay_from_before_the_beginning();
            Replay_from_after_the_end();            

            void Replay_from_somewhere_in_the_middle() {
                var result = sut.Replay(e2.Id);
                Assert.Equal(3, result.Count());
                Assert.Equal(2, ((AnotherTestEvent)result.ElementAt(0)).Bar);
                Assert.Equal("b", ((TestEvent) result.ElementAt(1)).Foo);
                Assert.Equal(3, ((AnotherTestEvent) result.ElementAt(2)).Bar);

                result = sut.Replay(e4.Id);
                Assert.Single(result);
                
                result = sut.Replay(e0.Id);
                Assert.Equal(5, result.Count());
            }

            void Replay_from_before_the_beginning() {
                var result = sut.Replay();
                Assert.Equal(5, result.Count());
            }

            void Replay_from_after_the_end() {
                var result = sut.Replay(e4.Id);
                Assert.Single(result);
            }
        }


        [Fact]
        public void Recording_succeeds_with_right_version()
        {
            const string PATH = nameof(FolderEventstore_tests) + "_" + nameof(Recording_succeeds_with_right_version);
            if (Directory.Exists(PATH)) Directory.Delete(PATH, true);
            var sut = new Eventstore<FilesInFolderEventRepository>(PATH);

            var state0 = sut.LastEventId;
            IEvent e0 = new TestEvent { Foo = "a" };
            sut.Record(null, e0);
            var result = sut.LastEventId;

            Assert.NotEqual(state0,  result);
            IEvent e1 = new AnotherTestEvent { Bar = 1 };
            sut.Record(e0.Id, e1);
            var result2 = sut.LastEventId;
            Assert.NotEqual(result, result2);
        }
        
        [Fact]
        public void Recording_fails_with_wrong_version()
        {
            const string PATH = nameof(FolderEventstore_tests) + "_" + nameof(Recording_fails_with_wrong_version);
            if (Directory.Exists(PATH)) Directory.Delete(PATH, true);
            var sut = new Eventstore<FilesInFolderEventRepository>(PATH);
            
            IEvent e0 = new TestEvent { Foo = "a" };
            sut.Record(null, e0);
            var state0 = sut.LastEventId;
            
            IEvent e1 = new TestEvent { Foo = "a" };
            sut.Record(e0.Id, e1);
            var result = sut.LastEventId;
            Assert.NotEqual(state0,  result);
            
            Assert.Throws<VersionNotFoundException>(() =>  sut.Record(state0, new AnotherTestEvent {Bar = 1}));
        }
        
        [Fact]
        public void Replay_in_the_right_order_with_many_events()
        {
            const string PATH = nameof(FolderEventstore_tests) + "_" + nameof(Replay_in_the_right_order_with_many_events);
            if (Directory.Exists(PATH)) Directory.Delete(PATH, true);
            var sut = new Eventstore<FilesInFolderEventRepository>(PATH);

            IEvent e0 = null;
            
            for (var i = 0; i < 50; i++) {
                IEvent ePrevious = e0;
                e0 = new TestEvent { Foo = i.ToString() };
                sut.Record(ePrevious == null ? null : ePrevious.Id , e0);
            }

            var result = sut.Replay();

            var prev = -1;
            foreach (var v in result.Select(x => int.Parse(((TestEvent)x).Foo))) {
                Assert.Equal(1, v-prev);
                prev = v;
            }
        }
    }
}