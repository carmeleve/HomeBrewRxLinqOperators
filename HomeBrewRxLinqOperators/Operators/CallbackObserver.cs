using System;

namespace HomeBrewRxLinqOperators.Operators
{
    internal class CallbackObserver<TLeft> : IObserver<TLeft>
    {
        private Action<TLeft> onNext;
        private Action<Exception> onError;
        private Action onCompleted;

        public CallbackObserver(Action<TLeft> onNext, Action<Exception> onError, Action onCompleted)
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

        public void OnNext(TLeft value)
        {
            this.onNext(value);
        }
    }
}