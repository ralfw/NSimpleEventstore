using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace nsimpleeventstore
{
    /*
     * Events are stored by expanding a persistent array incrementally per event.
     * 
     * The chronological sequence of events is maintained by numbering events in their chronological order
     * starting with 0.
     *
     * The event store is thread-safe; only one batch of events gets processed a at time.
     * (Many threads can replay events, though, but only one can record new ones.)
     * There must be only one event store instance for a certain event store path within a process.
     * Several processes working on the same event store path need to share an event store instance via
     * a common event store server process. Only that way the consistency of the events stored in a path
     * can be guaranteed.
     *
     * The event store is versioned. The version number is opaque to clients; they should not expect version numbers
     * to be ordered in any way or increase over time. The version changes whenever events got recorded.
     *
     * Storing a batch of events is not transactional. But due to the thread-safety of the event store and the
     * simplicity of the persistence approach it is assumed that failure during writing the events to files
     * is very unlikely.
     */
    public class FilebasedEventstore : Eventstore<EventRepository>
    {
        public FilebasedEventstore() : base() { }
        public FilebasedEventstore(string path) : base(path) { }
    }
}