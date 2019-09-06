using System.Collections.Generic;
using System.IO;
using System.Linq;
using nsimpleeventstore.contract;
using Xunit;

namespace nsimpleeventstore.tests
{
    public class Eventstore_scenario_tests
    {
        /*
         * Store a couple of events revolving around task management.
         * Afterwards build a context model from the events.
         */
        [Fact]
        public void FilebasedEventstore_acceptance_test() {
            const string PATH =  nameof(Eventstore_scenario_tests) + "_" + nameof(FilebasedEventstore_acceptance_test);
            if (Directory.Exists(PATH)) Directory.Delete(PATH, true);
            using (var sut = new FileEventstore(PATH))
            {
                Event e0 = new TodoAdded("do dishes");
                sut.Record(e0);
                sut.Record(new TodoAdded("walk dog"));

                Event e2 = new TodoAdded("write report");
                sut.Record(new Event[] {e2, new TodoCategorized(e2.Id, "work")});
                sut.Record(new TodoDone(e0.Id));

                var result = sut.Replay();
                var todos = result.Events.Aggregate(new Dictionary<string, ToDoItem>(), Map);

                Assert.Equal(2, todos.Count);
                Assert.Equal("write report", todos[e2.Id].Subject);
                Assert.Contains("work", todos[e2.Id].Categories);


                Dictionary<string, ToDoItem> Map(Dictionary<string, ToDoItem> items, Event e)
                {
                    switch (e)
                    {
                        case TodoAdded a:
                            items[a.Id] = new ToDoItem {Id = a.Id, Subject = a.Subject};
                            break;
                        case TodoDone d:
                            items.Remove(d.EntityId);
                            break;
                        case TodoCategorized c:
                            foreach (var cat in c.Categories)
                                items[c.EntityId].Categories.Add(cat);
                            break;
                    }

                    return items;
                }
            }
        }
        
        
        [Fact]
        public void InMemoryEventstore_acceptance_test() {
            using (var sut = new InMemoryEventstore())
            {
                Event e0 = new TodoAdded("do dishes");
                sut.Record(e0);
                sut.Record(new TodoAdded("walk dog"));

                Event e2 = new TodoAdded("write report");
                sut.Record(new Event[] {e2, new TodoCategorized(e2.Id, "work")});
                sut.Record(new TodoDone(e0.Id));

                var result = sut.Replay();
                var todos = result.Events.Aggregate(new Dictionary<string, ToDoItem>(), Map);

                Assert.Equal(2, todos.Count);
                Assert.Equal("write report", todos[e2.Id].Subject);
                Assert.Contains("work", todos[e2.Id].Categories);


                Dictionary<string, ToDoItem> Map(Dictionary<string, ToDoItem> items, Event e)
                {
                    switch (e)
                    {
                        case TodoAdded a:
                            items[a.Id] = new ToDoItem {Id = a.Id, Subject = a.Subject};
                            break;
                        case TodoDone d:
                            items.Remove(d.EntityId);
                            break;
                        case TodoCategorized c:
                            foreach (var cat in c.Categories)
                                items[c.EntityId].Categories.Add(cat);
                            break;
                    }

                    return items;
                }
            }
        }
        
        
        class TodoAdded : Event
        {
            public string Subject { get; }

            public TodoAdded(string subject)
            {
                Subject = subject;
            }
        }

        class TodoDone : Event
        {
            public string EntityId { get; }

            public TodoDone(string entityId)
            {
                EntityId = entityId;
            }
        }
        
        class TodoCategorized : Event
        {
            public string EntityId { get; }
            public string[] Categories { get; }

            public TodoCategorized(string entityId, params string[] categories) {
                EntityId = entityId;
                Categories = categories ?? new string[0];
            }
        }
        
        class ToDoItem
        {
            public string Id;
            public string Subject;
            public HashSet<string> Categories = new HashSet<string>();
        }
    }
}