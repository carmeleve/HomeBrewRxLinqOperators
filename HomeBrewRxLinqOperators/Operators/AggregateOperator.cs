using System;
using System.Collections.Generic;
using System.Text;

namespace HomeBrewRxLinqOperators
{
    internal class AggregateOperator<TSource, TOutput> : IObservable<TOutput>
    {
        private IObservable<TSource> source;
        private Func<IList<TSource>, TOutput> aggregation;

        public AggregateOperator(IObservable<TSource> source, Func<IList<TSource>, TOutput> aggregation)
        {
            this.source = source;
            this.aggregation = aggregation;
        }

        public IDisposable Subscribe(IObserver<TOutput> observer)
        {
            var aggregator = new Aggregator(observer, this.aggregation);
            return this.source.Subscribe(aggregator);
        }

        private class Aggregator : IObserver<TSource>
        {
            private IObserver<TOutput> observer;
            private Func<IList<TSource>, TOutput> aggregation;
            IList<TSource> values = new List<TSource>();

            public Aggregator(IObserver<TOutput> observer, Func<IList<TSource>, TOutput> aggregation)
            {
                this.observer = observer;
                this.aggregation = aggregation;
            }

            public void OnCompleted()
            {
                observer.OnNext(aggregation(values));
                observer.OnCompleted();
            }

            public void OnError(Exception error)
            {
                this.observer.OnError(error);
            }

            public void OnNext(TSource value)
            {
                values.Add(value);
            }
        }
    }
}
