using nsimpleeventstore.adapters.eventrepositories;

namespace nsimpleeventstore
{
    public class LiteDBEventstore : Eventstore<LiteDBEventRepository>
    {
        public LiteDBEventstore() : base() { }
        public LiteDBEventstore(string path) : base(path) { }
    }
}
