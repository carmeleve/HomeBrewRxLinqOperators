using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Text;

namespace HomeBrewRxLinqOperators
{
    internal class GroupByOperator<TSource, TKey> : IObservable<IGroupedObservable<TKey, TSource>>
    {
        private IObservable<TSource> source;
        private Func<TSource, TKey> keySelector;
        public GroupByOperator(IObservable<TSource> source, Func<TSource, TKey> keySelector)
        {
            this.source = source;
            this.keySelector = keySelector;
        }

        public IDisposable Subscribe(IObserver<IGroupedObservable<TKey, TSource>> observer)
        {
            var grouper = new Grouper(observer, this.keySelector);
            return source.Subscribe(grouper);
        }

        private class Grouper : IObserver<TSource>
        {
            private readonly IObserver<IGroupedObservable<TKey, TSource>> observer;
            private readonly Func<TSource, TKey> keySelector;
            private readonly Dictionary<TKey, GroupedObservable> groupedObservables;

            internal Grouper(IObserver<IGroupedObservable<TKey, TSource>> observer, Func<TSource, TKey> keySelector)
            {
                this.observer = observer;
                this.keySelector = keySelector;
                this.groupedObservables = new Dictionary<TKey, GroupedObservable>();
            }

            public void OnCompleted()
            {
                CompleteGroupedObservables();
                this.observer.OnCompleted();
            }

            public void OnError(Exception error)
            {
                CompleteGroupedObservables();
                this.observer.OnError(error);
            }

            private void CompleteGroupedObservables()
            {
                foreach (var groupedObservable in this.groupedObservables)
                {
                    groupedObservable.Value.Subject.OnCompleted();
                }
            }

            public void OnNext(TSource value)
            {
                var key = this.keySelector(value);

                if (!this.groupedObservables.TryGetValue(key, out var groupedObservable))
                {
                    groupedObservable = new GroupedObservable(key);
                    this.groupedObservables.Add(key, groupedObservable);
                    this.observer.OnNext(groupedObservable);
                }

                groupedObservable.Subject.OnNext(value);
            }

            private class GroupedObservable : IGroupedObservable<TKey, TSource>
            {
                internal GroupedObservable(TKey key)
                {
                    this.Key = key;
                    this.Subject = new Subject<TSource>();
                }
                public TKey Key { get; }
                public ISubject<TSource> Subject { get; }

                public IDisposable Subscribe(IObserver<TSource> observer)
                {
                    return this.Subject.Subscribe(observer);
                }


            }
        }
    }
}
