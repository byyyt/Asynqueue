namespace Asynqueue
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading;

    /// <summary>
    /// Asynqueue is a class that behaves similarly to golang's channels. It provides a simple,
    /// lightweight, and fast mechanism for sending messages to a background processing function.
    /// </summary>
    /// <typeparam name="T">The type of message to be sent</typeparam>
    public class Asynqueue<T> : IDisposable
    {
        //private Task processor;
        //private AsynqueueAwaitable<T> awaitableQ = new AsynqueueAwaitable<T>();
        
        private readonly ConcurrentQueue<T> _queue;
        private readonly Action<T> _handler;

        //private readonly Thread _thread;
        //private readonly ManualResetEventSlim _signal;
        //private volatile bool _starving;
        //private volatile bool _stop;
        private volatile bool _stop;
        private int _processing;

        /// <summary>
        /// Constructs a new instance of Asynqueue.
        /// </summary>
        /// <param name="handler">The action which handles messages in a background thread/task</param>
        public Asynqueue(Action<T> handler)
        {
            //processor = Task.Run(async () =>
            //{
            //    while (true)
            //    {
            //        var request = await awaitableQ;
            //        handler(request);
            //    }
            //});
            _handler = handler;
            _queue = new ConcurrentQueue<T>();

            //_signal = new ManualResetEventSlim(false);
            //_thread = new Thread(Execute);
            //_thread.IsBackground = true;
            //_thread.Start();
        }

        //private void Execute()
        //{
        //    while(!_stop)
        //    {
        //        T message;
        //        if (!_queue.TryDequeue(out message))
        //        {
        //            Wait();
        //            continue;
        //        }

        //        _handler(message);
        //    }

        //}

        //private void Wait()
        //{
        //    _starving = true;
        //    _signal.Wait();
        //    //Woken up. Reset.
        //    _starving = false;
        //    _signal.Reset();
        //}

        /// <summary>
        /// Send sends a message to the background process.
        /// </summary>
        /// <param name="message">The message to be sent.</param>
        public void Send(T message)
        {
            //awaitableQ.Enqueue(message);
            _queue.Enqueue(message);
            
            //if (_starving)
            //{
            //    _signal.Set();
            //}

            if (Interlocked.CompareExchange(ref _processing, 1, 0) == 0)
            {
                ThreadPool.QueueUserWorkItem(_ => {
                    Process();
                });
            }
        }

        private void Process()
        {
            T item;
            while(!_stop && _queue.TryDequeue(out item))
            {
                _handler(item);
            }

            Interlocked.Exchange(ref _processing, 0);
        }

        /// <summary>
        /// Dispose cleans up the asynqueue class.
        /// </summary>
        public void Dispose()
        {
            //processor.Dispose();
            _stop = true;
            //_signal.Set();
            //_thread.Join();
        }
    }
}
