namespace Asynqueue
{
    using System;

    /// <summary>
    /// Asynquery is a helper class that represents a single query being sent to and
    /// returned from a background worker.
    /// </summary>
    /// <typeparam name="TIn">The type of message being sent to the background worker</typeparam>
    /// <typeparam name="TOut">The type of response coming from the background worker</typeparam>
    internal class Asynquery<TIn, TOut> : IAwaitable<TOut>
    {
        private TOut response;
        private Exception exception;
        private volatile Action continuation;

        public bool IsCompleted => _isCompleted;
        private bool _isCompleted;

        public TIn Input { get; set; }

        public Asynquery(TIn input)
        {
            Input = input;
        }

        public void Respond(Exception ex)
        {
            Respond(() => exception = ex);
        }

        public void Respond(TOut response)
        {
            Respond(() => this.response = response);
        }

        public IAwaitable<TOut> GetAwaiter()
        {
            return this;
        }

        public void OnCompleted(Action continuation)
        {
            this.continuation = continuation;
            if (IsCompleted)
            {
                continuation?.Invoke();
            }

        }

        public TOut GetResult()
        {
            if (exception != null) throw exception;

            return response;
        }

        private void Respond(Action fn)
        {
            fn?.Invoke();
            _isCompleted = true;

            continuation?.Invoke();
        }
    }
}
