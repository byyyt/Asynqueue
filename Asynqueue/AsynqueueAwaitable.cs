//namespace Asynqueue
//{
//    using System;
//    using System.Collections.Concurrent;
//    using System.Threading;

//    /// <summary>
//    /// AsynqueueAwaitable is the meat of the Asynqueue system. It's a bit complicated, but basically,
//    /// this is an awaitable object, with each call to await returning an item off of the queue. This 
//    /// serves as a much more efficient way to communicate between threads than traditional signals.
//    /// The only portion of this which really needs to be synchronized is the queue access. Only
//    /// one thread should ever await this at any given time.
//    /// </summary>
//    /// <typeparam name="T">The type of message being queued.</typeparam>
//    internal class AsynqueueAwaitable<T> : IAwaitable<T>
//    {
//        private Action _continuation;
//        private int count = 0;
//        private int runCount = 0;
//        private ConcurrentQueue<T> q = new ConcurrentQueue<T>();

//        public bool IsCompleted
//        {
//            get
//            {
//                return count > 0;
//            }
//        }

//        public void Enqueue(T message)
//        {
//            Interlocked.Increment(ref count);
//            q.Enqueue(message);

//            Execute();
//        }

//        public IAwaitable<T> GetAwaiter()
//        {
//            return this;
//        }

//        public T GetResult()
//        {
//            Interlocked.Decrement(ref count);

//            T item;
//            if (q.TryDequeue(out item))
//                return item;
//            else
//                return default(T);
//        }

//        public void OnCompleted(Action continuation)
//        {
//            _continuation = continuation;

//            Execute();
//        }

//        private void Execute()
//        {
//            var call = _continuation;

//            if (call != null && count > 0 && Interlocked.CompareExchange(ref runCount, 1, 0) == 0)
//            {
//                call();
//                runCount = 0;
//                Execute();
//            }
//        }
//    }
//}
