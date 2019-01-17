using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;

namespace HomeBrewRxLinqOperators
{
    static class Program
    {
        static void Main(string[] args)
        {
            IObservable<int> sourceNumbers = System.Reactive.Linq.Observable.Range(1, 25);


            sourceNumbers.GroupBy(number => number % 3).Subscribe(
                group =>
                {
                    Console.WriteLine($"New group:{group.Key}");
                    group.Subscribe(value => Console.WriteLine($"{group.Key}: {value}"));
                });

            
            
            
            
            
            
            
            
            
            
            
            
            
            //IObservable<string> helloStrings = System.Reactive.Linq.Observable.Repeat("hello", 10);

            //IObservable<int> sum = sourceNumbers.Sum();
            //sum.Subscribe(n => Console.WriteLine(n));


            //IObservable<string> formattedOutputs = sourceNumbers.Select(n => $"OUTPUT: {n}");
            //formattedOutputs.Subscribe(n => Console.WriteLine(n));

            //IObservable<string> combinedOutputs = sourceNumbers.Select(n => $"OUTPUT: {n}").Aggregate(CombineStrings);
            //combinedOutputs.Subscribe(n => Console.WriteLine(n));
            //myConnectableObservable.Connect();


            //IConnectableObservable<int> multicastFilteredNumbers = filteredNumbers.Publish();


            //multicastFilteredNumbers.Subscribe(n => Console.WriteLine(n));
            //multicastFilteredNumbers.Subscribe(n => Console.WriteLine(n));

            //multicastFilteredNumbers.Connect();
            Console.ReadKey();
        }

        static string CombineStrings(IList<string> values)
        {
            string listedStrings = "";
            foreach (var str in values)
            {
                listedStrings += str + ",";
            }
            return listedStrings;
        }



        static IObservable<IGroupedObservable<TKey, TSource>> GroupBy<TSource, TKey>(this IObservable<TSource> source, Func<TSource, TKey> keySelector)
        {
            return new GroupByOperator<TSource, TKey>(source, keySelector);
        }

        public class GroupByOperator<TSource, TKey> : IObservable<IGroupedObservable<TKey, TSource>>
        {
            private IObservable<TSource> source;
            private Func<TSource, TKey> keySelector;
            public GroupByOperator(IObservable<TSource> source, Func<TSource,TKey> keySelector)
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
                private readonly IObserver<IGroupedObservable<TKey,TSource>> observer;
                private readonly Func<TSource, TKey> keySelector;
                private readonly Dictionary<TKey, GroupedObservable> groupedObservables;

                internal Grouper(IObserver<IGroupedObservable<TKey, TSource>> observer, Func<TSource,TKey> keySelector)
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

        

        public interface IGroupedObservable<out TKey, out TElement> : IObservable<TElement>
        {
            // Summary:
            // Gets the common key.
            TKey Key { get; }
        }

        static IObservable<TSource> Where<TSource>(this IObservable<TSource> source, Func<TSource, bool> predicate)
        {
            return new WhereOperator<TSource>(source, predicate);
        }

        public class MyConnectableObservable<T> : IConnectableObservable<T>
        {
            private ISubject<T> subject;
            private IObservable<T> source;

            public MyConnectableObservable(IObservable<T> source)
            {
                subject = new Subject<T>();
                this.source = source;
            }
            public IDisposable Connect()
            {
                return source.Subscribe(subject);
            }

            public IDisposable Subscribe(IObserver<T> observer)
            {
                return this.subject.Subscribe(observer);
            }
        }

        static IObservable<TOutput> Select<TSource, TOutput>(this IObservable<TSource> source, Func<TSource, TOutput> selection)
        {
            return new SelectOperator<TSource, TOutput>(source, selection);
        }

        public class SelectOperator<TSource, TOutput> : IObservable<TOutput>
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

        public static IObservable<TOutput> Aggregate<TSource, TOutput>(this IObservable<TSource> source, Func<IList<TSource>, TOutput> aggregation)
        {
            return new AggregateOperator<TSource,TOutput>(source, aggregation);
        }

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

        public static IObservable<TSource> Sum<TSource>(this IObservable<TSource> source)
        {
            return new SumOperator<TSource>(source);
        }

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



        public class MyObserver : IObserver<int>
        {
            public void OnNext(int value)
            {
                Console.WriteLine(value);
            }
            public void OnCompleted() { }

            public void OnError(Exception error) { }
            
        }


        private class WhereOperator<T> : IObservable<T>
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

    
}
