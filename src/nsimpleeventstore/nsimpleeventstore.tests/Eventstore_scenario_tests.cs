using System.Collections.Generic;
using System.Linq;
using nsimpleeventstore.adapters.eventrepositories;
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
        public void InMemoryEventstore_acceptance_test() {
            using (var sut = new Eventstore<InMemoryEventRepository>())
            {
                IEvent e0 = new TodoAdded("do dishes");
                sut.Record(null, e0);
                IEvent e1 = new TodoAdded("walk dog");
                sut.Record(e0.Id, e1);

                IEvent e2 = new TodoAdded("write report");
                IEvent e3 = new TodoCategorized(e2.Id.ToString(), "work");
                sut.Record(e1.Id, new IEvent[] {e2, e3});
                IEvent e4 = new TodoDone(e0.Id.ToString());
                sut.Record(e3.Id, e4);

                var result = sut.Replay();
                var todos = result.Aggregate(new Dictionary<string, ToDoItem>(), Map);

                Assert.Equal(2, todos.Count);
                Assert.Equal("write report", todos[e2.Id.ToString()].Subject);
                Assert.Contains("work", todos[e2.Id.ToString()].Categories);


                Dictionary<string, ToDoItem> Map(Dictionary<string, ToDoItem> items, IEvent e)
                {
                    switch (e)
                    {
                        case TodoAdded a:
                            items[a.Id.ToString()] = new ToDoItem {Id = a.Id.ToString(), Subject = a.Subject};
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