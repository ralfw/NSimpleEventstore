using System;
using System.Collections.Generic;
using System.Linq;
using nsimpleeventstore;
using nsimpleeventstore.contract;
using nsimpleeventstore.adapters.eventrepositories;

namespace todomanager
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var es = new Eventstore<FilesInFolderEventRepository>();
            var be = new Backend(es);
            var fe = new Frontend(be);
            
            fe.Show();
        }
    }


    class Frontend
    {
        private readonly Backend _be;

        public Frontend(Backend be)
        {
            _be = be;
        }
        
        public void Show()
        {
            var toDoIds = Refresh();
            do {
                Console.Write($"A(dd, C(eck off, eX(it: ");
                switch (Console.ReadLine().ToUpper())
                {
                    case "A":
                        Console.Write("  What to do?: ");
                        var subject = Console.ReadLine();
                        if (subject == "") break;

                        _be.Handle(new CreateToDo {Subject = subject});
                        toDoIds = Refresh();
                        break;
                    
                    case "C":
                        Console.Write("  Index of todo item to check-off?: ");
                        var index = Console.ReadLine();
                        if (index == "") break;

                        var entityId = toDoIds[int.Parse(index)-1];
                        _be.Handle(new CheckOffToDo{ToDoId = entityId});
                        toDoIds = Refresh();
                        break;
                    
                    case "X":
                        return;
                }
            } while (true);



            string[] Refresh() {
                var result = _be.Handle(new ToDosQuery());
                DisplayToDoList(result);
                return result.ToDoList.Select(x => x.EntityId).ToArray();
            }
        }

        
        private void DisplayToDoList(ToDosQueryResult todoList)  {
            var i = 1;
            foreach(var e in todoList.ToDoList)
                Console.WriteLine($"{i++}. {(e.Done ? '√' : ' ')} {e.Subject}");
        }
    }


    class Backend
    {
        private readonly IEventstore _es;

        public Backend(IEventstore es)
        {
            _es = es;
        }

        public EventId Handle(CreateToDo cmd)
        {
            var e = new ToDoCreated{Subject = cmd.Subject};
            _es.Record(null, e); //expectedLastEventId can be null because this is a single user and single thread app
            return e.Id;
        }

        public void Handle(CheckOffToDo cmd)
        {
            var e = new ToDoDone{ToDoId = cmd.ToDoId};
            _es.Record(null, e);
        }

        public ToDosQueryResult Handle(ToDosQuery query)
        {
            return new ToDosQueryResult{ToDoList = Load()};


            IEnumerable<(string EntityId, string Subject, bool Done)> Load() {
                var entities = new Dictionary<string,(string Subject, bool Done)>();
                foreach(var e in _es.Replay())
                    switch (e)
                    {
                        case ToDoCreated tdc:
                            entities[tdc.Id.Value.ToString()] = (tdc.Subject, false);
                            break;
                        case ToDoDone tdd:
                            entities[tdd.ToDoId] = (entities[tdd.ToDoId].Subject, true);
                            break;
                    }
                return entities.Select(x => (x.Key, x.Value.Subject, x.Value.Done));
            }
        }
    }

    

    internal class ToDosQuery  {}
    
    internal class ToDosQueryResult
    {
        public IEnumerable<(string EntityId, string Subject, bool Done)> ToDoList;
    }

    internal class CheckOffToDo
    {
        public string ToDoId;
    }

    internal class CreateToDo
    {
        public string Subject;
    }
    


    class ToDoCreated : Event
    {
        public string Subject;
    }

    class ToDoDone : Event
    {
        public string ToDoId;
    }
}