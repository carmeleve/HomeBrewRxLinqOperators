using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
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
                    group.Subscribe(value => Console.WriteLine($"New value in group {group.Key}: {value}"));
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

        static IObservable<TSource> Where<TSource>(this IObservable<TSource> source, Func<TSource, bool> predicate)
        {
            return new WhereOperator<TSource>(source, predicate);
        }

        static IObservable<TOutput> Select<TSource, TOutput>(this IObservable<TSource> source, Func<TSource, TOutput> selection)
        {
            return new SelectOperator<TSource, TOutput>(source, selection);
        }

        public static IObservable<TOutput> Aggregate<TSource, TOutput>(this IObservable<TSource> source, Func<IList<TSource>, TOutput> aggregation)
        {
            return new AggregateOperator<TSource,TOutput>(source, aggregation);
        }

        public static IObservable<TSource> Sum<TSource>(this IObservable<TSource> source)
        {
            return new SumOperator<TSource>(source);
        }
    }
}
