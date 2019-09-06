namespace nsimpleeventstore
{
    /*
     * Events are stored by expanding a persistent array incrementally per event.
     * 
     * The chronological sequence of events is maintained by numbering events in their chronological order
     * starting with 0.
     *
     * The event store is thread-safe.
     *
     * The event store is versioned. The version number is opaque to clients; they should not expect version numbers
     * to be ordered in any way or increase over time. The version changes whenever events got recorded.
     */
    public class EsentEventstore : Eventstore<EsentRepository>
    {
        public EsentEventstore() : base() { }
        public EsentEventstore(string path) : base(path) { }
    }
}