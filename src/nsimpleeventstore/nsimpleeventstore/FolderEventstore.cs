using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using nsimpleeventstore.adapters.eventrepositories;

namespace nsimpleeventstore
{
    public class FolderEventstore : Eventstore<FilesInFolderEventRepository>
    {
        public FolderEventstore() : base() { }
        public FolderEventstore(string path) : base(path) { }
    }
}