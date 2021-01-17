using System;
using System.Linq;
using nsimpleeventstore;
using nsimpleeventstore.adapters;
using nsimpleeventstore.adapters.eventrepositories;
using nsimpleeventstore.contract;

namespace firststeps
{
    class A : Event {}
    class B : Event {}
    class C : Event {}
    
    
    // Just a sandbox project to play around
    internal class Program
    {
        public static void Main(string[] args)
        {
            var es = new Eventstore<InMemoryEventRepository>();
            var a = new A();
            var b = new B();
            var c = new C();
            es.Record(null, a);
            es.Record(a.Id, c);
            es.Record(c.Id, b);

            EventArchive.Write("myarchive.json", es.Replay());

            var events = EventArchive.Read("myarchive.json");

            var es2 = new Eventstore<InMemoryEventRepository>();
            EventId id = null;
            events.ToList().ForEach(delegate (IEvent e)
            {
                es2.Record(id, e);
                id = e.Id;                
            });

            Console.WriteLine(es2.Replay().ToList().Count);
        }
    }
}