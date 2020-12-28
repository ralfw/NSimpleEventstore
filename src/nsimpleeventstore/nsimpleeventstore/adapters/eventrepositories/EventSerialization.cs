using System;
using System.Dynamic;
using System.Linq;
using Newtonsoft.Json;
using nsimpleeventstore.contract;

namespace nsimpleeventstore.adapters.eventrepositories
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
        public static JsonSerializerSettings JsonSerializerSettings { get; set; } = default;

        public static string Serialize(IEvent e) {
            var eventName = e.GetType().AssemblyQualifiedName;
            var data = JsonSerializerSettings == null
                ? JsonConvert.SerializeObject(e)
                : JsonConvert.SerializeObject(e, JsonSerializerSettings);
            var parts = new[]{eventName, data};
            return string.Join("\n", parts);
        }

        public static IEvent Deserialize(string e) {
            var lines = e.Split('\n');
            var eventName = lines.First();
            var data = string.Join("\n", lines.Skip(1));
            return JsonSerializerSettings == null
                ? (IEvent)JsonConvert.DeserializeObject(data, Type.GetType(eventName))
                : (IEvent)JsonConvert.DeserializeObject(data, Type.GetType(eventName), JsonSerializerSettings);
        }
    }
}