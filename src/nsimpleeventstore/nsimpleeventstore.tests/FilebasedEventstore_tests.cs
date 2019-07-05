using System;
using System.Data;
using System.IO;
using System.Linq;
using Xunit;

namespace nsimpleeventstore.tests
{
    public class FilebasedEventstore_tests
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
        public void Basic_recording_and_replaying()
        {
            const string PATH = nameof(FilebasedEventstore_tests) + "_" + nameof(Basic_recording_and_replaying);
            if (Directory.Exists(PATH)) Directory.Delete(PATH, true);
            var sut = new FilebasedEventstore(PATH);

            sut.Record(new TestEvent {Foo = "a"});
            sut.Record(new[] {new TestEvent {Foo = "b"}, new TestEvent {Foo = "c"}});

            var result = sut.Replay();
            
            Assert.Equal(new[]{"a", "b", "c"}, result.Events.Select(e => ((TestEvent)e).Foo).ToArray());
        }
        
        
        [Fact]
        public void Version_number_changes()
        {
            const string PATH = nameof(FilebasedEventstore_tests) + "_" + nameof(Version_number_changes);
            if (Directory.Exists(PATH)) Directory.Delete(PATH, true);
            var sut = new FilebasedEventstore(PATH);

            var state0 = sut.State;
            Assert.True(state0.FinalEventNumber < 0);

            var result0 = sut.Record(new TestEvent());
            Assert.NotEqual(state0.Version, result0.Version);
            Assert.NotEqual(state0.FinalEventNumber, result0.FinalEventNumber);
            Assert.Equal(0, result0.FinalEventNumber);
            
            var result1 = sut.Record(new TestEvent());
            Assert.NotEqual(result0.Version, result1.Version);
            Assert.Equal(1, result1.FinalEventNumber);
            
            var result2 = sut.Record(new[]{new TestEvent(), new TestEvent()});
            Assert.NotEqual(result1.Version, result2.Version);
            Assert.Equal(3, result2.FinalEventNumber);

            var state99 = sut.State;
            Assert.Equal(state99.Version, result2.Version);
            Assert.Equal(state99.FinalEventNumber, result2.FinalEventNumber);
        }
        
        
        [Fact]
        public void Notification_about_events_recorded()
        {
            const string PATH = nameof(FilebasedEventstore_tests) + "_" + nameof(Notification_about_events_recorded);
            if (Directory.Exists(PATH)) Directory.Delete(PATH, true);
            var sut = new FilebasedEventstore(PATH);

            (string Version, long FinalEventNumber, Event[] Events) result = ("", -1, null);
            sut.OnRecorded += (v,f,e) => result = (v,f,e);
            var state0 = sut.State;

            sut.Record(new TestEvent{Foo = "a"});
            Assert.NotEqual(state0.Version, result.Version);
            Assert.Equal(0, result.FinalEventNumber);
            Assert.Single(result.Events);

            var version1 = result.Version;
            sut.Record(new[]{new TestEvent{Foo = "b"}, new TestEvent{Foo = "c"}});
            Assert.NotEqual(version1, result.Version);
            Assert.Equal(2, result.FinalEventNumber);
            Assert.Equal(new[]{"b", "c"}, result.Events.Select(e => ((TestEvent)e).Foo).ToArray());
            
            var state99 = sut.State;
            Assert.Equal(state99.Version, result.Version);
            Assert.Equal(state99.FinalEventNumber, result.FinalEventNumber);
        }
        
        
        [Fact]
        public void Replaying_a_subset()
        {
            const string PATH = nameof(FilebasedEventstore_tests) + "_" + nameof(Replaying_a_subset);
            if (Directory.Exists(PATH)) Directory.Delete(PATH, true);
            var sut = new FilebasedEventstore(PATH);
            
            sut.Record(new Event[] {
                new TestEvent {Foo = "a"}, new AnotherTestEvent {Bar = 1},
                new AnotherTestEvent {Bar = 2}, new TestEvent {Foo = "b"},
                new AnotherTestEvent {Bar = 3}
            });
            var state5 = sut.State;

            var result = sut.Replay();
            Assert.Equal(state5.Version, result.Version);
            Assert.Equal(5, result.Events.Length);

            result = sut.Replay(typeof(TestEvent));
            Assert.Equal(state5.Version, result.Version);
            Assert.Equal(new[]{"a", "b"}, result.Events.Select(e => ((TestEvent)e).Foo).ToArray());
            
            result = sut.Replay(typeof(AnotherTestEvent));
            Assert.Equal(state5.Version, result.Version);
            Assert.Equal(new[]{1, 2, 3}, result.Events.Select(e => ((AnotherTestEvent)e).Bar).ToArray());
        }
        
        
        [Fact]
        public void Replaying_from_event_number()
        {
            const string PATH = nameof(FilebasedEventstore_tests) + "_" + nameof(Replaying_from_event_number);
            if (Directory.Exists(PATH)) Directory.Delete(PATH, true);
            var sut = new FilebasedEventstore(PATH);
            
            sut.Record(new Event[] {
                new TestEvent {Foo = "a"}, new AnotherTestEvent {Bar = 1},
                new AnotherTestEvent {Bar = 2}, new TestEvent {Foo = "b"},
                new AnotherTestEvent {Bar = 3}
            });

            Replay_from_somewhere_in_the_middle();
            Replay_from_before_the_beginning();
            Replay_from_after_the_end();
            

            void Replay_from_somewhere_in_the_middle() {
                var result = sut.Replay(2);
                Assert.Equal(3, result.Events.Length);
                Assert.Equal(2, ((AnotherTestEvent) result.Events[0]).Bar);
                Assert.Equal("b", ((TestEvent) result.Events[1]).Foo);
                Assert.Equal(3, ((AnotherTestEvent) result.Events[2]).Bar);

                result = sut.Replay(4);
                Assert.Single(result.Events);
                
                result = sut.Replay(0);
                Assert.Equal(5, result.Events.Length);
            }

            void Replay_from_before_the_beginning() {
                var result = sut.Replay(-1);
                Assert.Equal(5, result.Events.Length);
            }

            void Replay_from_after_the_end() {
                var result = sut.Replay(5);
                Assert.Empty(result.Events);
            }
        }


        [Fact]
        public void Recording_succeeds_with_right_version()
        {
            const string PATH = nameof(FilebasedEventstore_tests) + "_" + nameof(Recording_succeeds_with_right_version);
            if (Directory.Exists(PATH)) Directory.Delete(PATH, true);
            var sut = new FilebasedEventstore(PATH);

            var state0 = sut.State;
            var result = sut.Record(new TestEvent{Foo = "a"});
            
            Assert.NotEqual(state0.Version,  result.Version);
            var result2 = sut.Record(new AnotherTestEvent {Bar = 1}, result.Version);

            Assert.NotEqual(result.Version, result2.Version);
        }
        
        [Fact]
        public void Recording_fails_with_wrong_version()
        {
            const string PATH = nameof(FilebasedEventstore_tests) + "_" + nameof(Recording_fails_with_wrong_version);
            if (Directory.Exists(PATH)) Directory.Delete(PATH, true);
            var sut = new FilebasedEventstore(PATH);

            var state0 = sut.State;
            var result = sut.Record(new TestEvent{Foo = "a"});
            Assert.NotEqual(state0.Version,  result.Version);
            
            Assert.Throws<VersionNotFoundException>(() =>  sut.Record(new AnotherTestEvent {Bar = 1}, state0.Version));
        }
    }
}