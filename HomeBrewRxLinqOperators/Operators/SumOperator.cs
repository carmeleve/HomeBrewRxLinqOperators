using System;
using System.Collections.Generic;
using System.Text;

namespace HomeBrewRxLinqOperators
{
    internal class SumOperator<TSource> : IObservable<TSource>
    {
        private IObservable<TSource> source;

        public SumOperator(IObservable<TSource> source)
        {
            this.source = source;
        }

        public IDisposable Subscribe(IObserver<TSource> observer)
        {
            var summer = new Summer<TSource>(observer);
            return source.Subscribe(summer);
        }

        private class Summer<TSource> : IObserver<TSource>
        {
            private IObserver<TSource> observer;
            private TSource sum;

            public Summer(IObserver<TSource> observer)
            {
                this.observer = observer;
            }

            public void OnCompleted()
            {
                this.observer.OnNext(sum);
                this.observer.OnCompleted();
            }

            public void OnError(Exception error)
            {
                this.observer.OnError(error);
            }

            public void OnNext(TSource value)
            {
                sum += (dynamic)value;
            }
        }
    }
}
