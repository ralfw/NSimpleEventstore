using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using nsimpleeventstore.adapters.eventrepositories;

namespace nsimpleeventstore
{
    public class FileEventstore : Eventstore<FileEventRepository>
    {
        public FileEventstore() : base() { }
        public FileEventstore(string path) : base(path) { }
    }
}