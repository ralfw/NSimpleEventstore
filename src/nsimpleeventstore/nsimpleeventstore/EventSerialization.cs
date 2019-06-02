using System;
using System.Linq;
using Newtonsoft.Json;

namespace nsimpleeventstore
{
    /*
     * Serialization format:
     *     <Assembly qualified name of event class>
     *     <Lines of JSON encoded event data>
     *
     * The qualified name is necessary before the JSON to be able to deserialize it properly.
     */
    static class EventSerialization
    {
        public static string Serialize(Event e) {
            var eventName = e.GetType().AssemblyQualifiedName;
            var data = JsonConvert.SerializeObject(e);
            var parts = new[]{eventName, data};
            return string.Join("\n", parts);
        }

        public static Event Deserialize(string e) {
            var lines = e.Split('\n');
            var eventName = lines.First();
            var data = string.Join("\n", lines.Skip(1));
            return (Event)JsonConvert.DeserializeObject(data, Type.GetType(eventName));
        }
    }
}