using System;
using System.Linq;
using System.Threading;

namespace ServiceStack.WebHost.Endpoints.Support
{
    internal class SimpleAsyncResult : IAsyncResult, IDisposable
    {
        ManualResetEvent _waitHandle;
        bool _isCompleted;
        readonly AsyncCallback _callback;

        public SimpleAsyncResult(AsyncCallback callback, object state)
        {
            _callback = callback;
            AsyncState = state;
        }

        public object AsyncState { get; private set; }

        public WaitHandle AsyncWaitHandle
        {
            get
            {
                if (_waitHandle != null)
                {
                    return _waitHandle;
                }

                var newHandle = new ManualResetEvent(false);

                if (Interlocked.CompareExchange(ref _waitHandle, newHandle, null) != null)
                {
                    newHandle.Close();
                }

                if (_isCompleted)
                {
                    _waitHandle.Set();
                }

                return _waitHandle;
            }
        }

        public bool CompletedSynchronously { get; private set; }

        public bool IsCompleted
        {
            get { return _isCompleted; }
        }

        public void SetComplete(bool completedSynchronously)
        {
            CompletedSynchronously = completedSynchronously;
            _isCompleted = true;
            Thread.MemoryBarrier();
            if (_waitHandle != null)
            {
                _waitHandle.Set();
            }

            if (_callback != null)
            {
                _callback(this);
            }
        }

        public void Dispose()
        {
            if (_waitHandle != null)
            {
                _waitHandle.Close();
            }
        }
    }
}
