namespace nsimpleeventstore.contract
{
    /*
     * All events to be stored in the event store have to implement
     * the IEvent interface or derive from the Event class. Also they need to be JSON-(de)serializable.
     * That means they need a public parameter-less constructor.
     */
    public interface IEvent
    {
        EventId Id { get; }
    }
}
