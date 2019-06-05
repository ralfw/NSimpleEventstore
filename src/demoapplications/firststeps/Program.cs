using System;
using System.IO;
using nsimpleeventstore;

namespace firststeps
{
    class NumberEntered : Event
    {
        public int Number;
    }

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

            EventArchive.Write("myarchive.json", es.Replay().Events);

            var events = EventArchive.Read("myarchive.json");

            var es2 = new InMemoryEventstore(events);
            Console.WriteLine(es2.Replay().Events.Length);
        }
    }
}