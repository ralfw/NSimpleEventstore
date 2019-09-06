using System;
using System.Linq;
using nsimpleeventstore;
using nsimpleeventstore.adapters;
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
            var es = new InMemoryEventstore();

            es.Record(new A());
            es.Record(new C());
            es.Record(new B());

            nsimpleeventstore.adapters.EventArchive.Write("myarchive.json", es.Replay().Events);

            var events = EventArchive.Read("myarchive.json");

            var es2 = new InMemoryEventstore(events);
            Console.WriteLine(es2.Replay().Events.Length);
        }
    }
}