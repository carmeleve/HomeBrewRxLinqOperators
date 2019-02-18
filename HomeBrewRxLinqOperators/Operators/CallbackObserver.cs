using System;

namespace HomeBrewRxLinqOperators.Operators
{
    internal class CallbackObserver<TSource> : IObserver<TSource>
    {
        private Action<TSource> onNext;
        private Action<Exception> onError;
        private Action onCompleted;

        public CallbackObserver(Action<TSource> onNext, Action<Exception> onError, Action onCompleted)
        {
            this.onNext = onNext;
            this.onError = onError;
            this.onCompleted = onCompleted;
        }

        public void OnCompleted()
        {
            this.onCompleted();
        }

        public void OnError(Exception error)
        {
            this.onError(error);
        }

        public void OnNext(TSource value)
        {
            this.onNext(value);
        }
    }
}