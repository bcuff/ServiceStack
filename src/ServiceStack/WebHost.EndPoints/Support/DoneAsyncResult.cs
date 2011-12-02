using System;
using System.Linq;
using System.Threading;

namespace ServiceStack.WebHost.Endpoints.Support
{
	internal class DoneAsyncResult : IAsyncResult
	{
        static readonly ManualResetEvent _waitHandle = new ManualResetEvent(true);

        public DoneAsyncResult(object state)
        {
            AsyncState = state;
        }

        public object AsyncState { get; private set; }

        public WaitHandle AsyncWaitHandle
        {
            get { return _waitHandle; }
        }

        public bool CompletedSynchronously
        {
            get { return true; }
        }

        public bool IsCompleted
        {
            get { return true; }
        }
    }
}
