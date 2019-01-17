using System;
using System.Collections.Generic;
using System.Text;

namespace HomeBrewRxLinqOperators
{
    internal class SelectOperator<TSource, TOutput> : IObservable<TOutput>
    {
        private IObservable<TSource> source;
        private Func<TSource, TOutput> selection;

        public SelectOperator(IObservable<TSource> source, Func<TSource, TOutput> selection)
        {
            this.source = source;
            this.selection = selection;
        }
        public IDisposable Subscribe(IObserver<TOutput> observer)
        {
            var selector = new Selector(observer, selection);
            return this.source.Subscribe(selector);
        }

        private class Selector : IObserver<TSource>
        {
            private IObserver<TOutput> observer;
            private Func<TSource, TOutput> selection;

            public Selector(IObserver<TOutput> observer, Func<TSource, TOutput> selection)
            {
                this.observer = observer;
                this.selection = selection;
            }

            public void OnCompleted()
            {
                this.observer.OnCompleted();
            }

            public void OnError(Exception error)
            {
                this.observer.OnError(error);
            }

            public void OnNext(TSource value)
            {
                TOutput selectedValue = this.selection(value);
                this.observer.OnNext(selectedValue);
            }
        }
    }
}
