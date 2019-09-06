using System;
using System.Threading;

namespace nsimpleeventstore.adapters
{
    class Lock
    {
        private const int LOCK_ACQUISITION_TIMEOUT_MSEC = 5000;
        private readonly ReaderWriterLock _lock;

        public Lock() {
            _lock = new ReaderWriterLock();
        }
        
        
        public void TryWrite(Action f)
        {
            _lock.AcquireWriterLock(LOCK_ACQUISITION_TIMEOUT_MSEC);
            try {
                f();
            }
            finally  {
                _lock.ReleaseWriterLock();
            }
        }
        
        public T TryRead<T>(Func<T> f)
        {
            _lock.AcquireReaderLock(LOCK_ACQUISITION_TIMEOUT_MSEC);
            try {
                return f();
            }
            finally  {
                _lock.ReleaseReaderLock();
            }
        }
    }
}