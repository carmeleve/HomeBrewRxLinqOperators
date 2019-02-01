using HomeBrewRxLinqOperators.Operators;
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
            IObservable<long> timer = System.Reactive.Linq.Observable.Timer(TimeSpan.FromSeconds(0.5), TimeSpan.FromSeconds(1));

            //IObservable<IList<long>> bufferedTimes = timer.Buffer(TimeSpan.FromSeconds(4));

            //DateTime start = DateTime.Now;
            //int bufferNumber = 1;
            //bufferedTimes.Subscribe(ticks =>
            //{
            //    int thisBuffer = bufferNumber++;
            //    Console.WriteLine($"New window: {thisBuffer}");

            //   foreach (var tick in ticks)
            //    {
            //        Console.WriteLine($"TIME PASSED: {(int)(DateTime.Now - start).TotalSeconds} seconds");
            //        Console.WriteLine($"tick {tick}");
            //    }
            //}
            //);

            
            IObservable<IObservable<long>> windowedTimes = timer.Window(TimeSpan.FromSeconds(4));

            DateTime start = DateTime.Now;
            int windowNumber = 1;
            windowedTimes.Subscribe(w =>
            {
                int thisWindow = windowNumber++;
                Console.WriteLine($"TIME PASSED: {(int)(DateTime.Now - start).TotalSeconds} seconds");
                Console.WriteLine($"New window: {thisWindow}");
                w.Subscribe(tick =>
                {
                    Console.WriteLine($"{(int)(DateTime.Now - start).TotalSeconds} seconds: tick {tick}");
                }
                );
            }
            );

            //IObservable<long> s1 = System.Reactive.Linq.Observable.Timer(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(3));
            //IObservable<long> s2 = System.Reactive.Linq.Observable.Timer(TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(1));

            ////DateTime start = DateTime.Now;
            //s1.Join(
            //    s2,
            //    t => System.Reactive.Linq.Observable.Timer(TimeSpan.FromSeconds(1.5)),
            //    t => System.Reactive.Linq.Observable.Timer(TimeSpan.FromSeconds(1.5)),
            //    (tl, tr) => (tl, tr))
            //    .Subscribe(x =>
            //    {
            //        Console.WriteLine($"{(int)(DateTime.Now - start).TotalSeconds} {x.tl} {x.tr}");
            //    });



            //IObservable<int> sourceNumbers = System.Reactive.Linq.Observable.Range(1, 25);


            //sourceNumbers.GroupBy(number => number % 3).Subscribe(
            //    group =>
            //    {
            //        Console.WriteLine($"New group:{group.Key}");
            //        group.Subscribe(value => Console.WriteLine($"New value in group {group.Key}: {value}"));
            //    });

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

        static IObservable<IObservable<TSource>> Window<TSource>(this IObservable<TSource> source, TimeSpan timeSpan)
        {
            return new WindowOperator<TSource>(source, timeSpan);
        }

        static IObservable<IList<TSource>> Buffer<TSource>(this IObservable<TSource> source, TimeSpan timeSpan)
        {
            return new BufferOperator<TSource>(source, timeSpan);
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

        public static IObservable<TResult> Join<TLeft, TRight, TLeftDuration, TRightDuration, TResult>(
            this IObservable<TLeft> left, 
            IObservable<TRight> right, 
            Func<TLeft, IObservable<TLeftDuration>> leftDurationSelector, 
            Func<TRight, IObservable<TRightDuration>> rightDurationSelector, 
            Func<TLeft, TRight, TResult> resultSelector)
        {
            return new JoinOperator<TLeft, TRight, TResult, TLeftDuration, TRightDuration>(left, right, leftDurationSelector, rightDurationSelector, resultSelector);
        }
    }
}
