using System;
using System.Threading;

namespace Xunit.Sdk
{
    internal static partial class Extensions
    {
        // Silverlight doesn't support the compiler generated BeginInvoke/EndInvoke. Not entirely sure why,
        // but here's a great explanation of what's going on: 
        // http://stackoverflow.com/questions/4192342/anyone-know-why-delegate-begininvoke-isnt-supported-on-silverlight/4200008#4200008
        public static IAsyncResult WorkingBeginInvoke(this Delegate @delegate, AsyncCallback callback, object state)
        {
            var @event = new ManualResetEvent(false);
            var asyncResult = new AsyncResult(state, @event);
            ThreadPool.QueueUserWorkItem(_ =>
                                             {
                                                 var result = @delegate.DynamicInvoke();
                                                 @event.Set();
                                                 if (callback != null)
                                                 {
                                                     asyncResult.Result = result;
                                                     callback(asyncResult);
                                                 }
                                             });
            return asyncResult;
        }

        public static object WorkingEndInvoke(this Delegate @delegate, IAsyncResult asyncResult)
        {
            var ourAsyncResult = (AsyncResult)asyncResult;
            ourAsyncResult.AsyncWaitHandle.WaitOne();
            return ourAsyncResult.Result;
        }

        internal class AsyncResult : IAsyncResult
        {
            private readonly ManualResetEvent @event;

            public AsyncResult(object state, ManualResetEvent @event)
            {
                this.@event = @event;
                AsyncState = state;
            }

            public bool IsCompleted
            {
                get { return @event.WaitOne(0); }
            }

            public WaitHandle AsyncWaitHandle
            {
                get { return @event; }
            }

            public object AsyncState { get; private set; }

            public bool CompletedSynchronously
            {
                get { return false; }
            }

            public object Result { get; set; }
        }
    }
}