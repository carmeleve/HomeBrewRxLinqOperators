using System;
using System.Collections.Generic;
using System.Text;

namespace HomeBrewRxLinqOperators
{
    internal class WhereOperator<T> : IObservable<T>
    {
        private readonly IObservable<T> source;
        private readonly Func<T, bool> predicate;

        public WhereOperator(IObservable<T> source, Func<T, bool> predicate)
        {
            this.source = source;
            this.predicate = predicate;
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            var filter = new Filter(observer, predicate);
            return source.Subscribe(filter);
        }

        private class Filter : IObserver<T>
        {
            private IObserver<T> observer;
            private Func<T, bool> predicate;

            public Filter(IObserver<T> observer, Func<T, bool> predicate)
            {
                this.observer = observer;
                this.predicate = predicate;
            }

            public void OnCompleted()
            {
                this.observer.OnCompleted();
            }

            public void OnError(Exception error)
            {
                this.observer.OnError(error);
            }

            public void OnNext(T value)
            {
                if (this.predicate(value))
                {
                    this.observer.OnNext(value);
                }
            }
        }
    }
}
