using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;

namespace HomeBrewRxLinqOperators
{
    internal class BufferOperator<TSource> : IObservable<IList<TSource>>
    {
        private IObservable<TSource> source;
        private TimeSpan timeSpan;

        public BufferOperator(IObservable<TSource> source, TimeSpan timeSpan)
        {
            this.source = source;
            this.timeSpan = timeSpan;
        }

        public IDisposable Subscribe(IObserver<IList<TSource>> observer)
        {
            var bufferer = new Bufferer(observer, this.timeSpan);
            return this.source.Subscribe(bufferer);
        }

        private class Bufferer : IObserver<TSource>
        {
            private readonly IObserver<IList<TSource>> observer;
            private TimeSpan timeSpan;
            private IList<TSource> currentList;
            private IDisposable endOfWindowSchedulerWorkItem;

            public Bufferer(IObserver<IList<TSource>> observer, TimeSpan timeSpan)
            {
                this.observer = observer;
                this.timeSpan = timeSpan;

                StartNewWindow();
            }

            private void StartNewWindow()
            {
                var scheduler = Scheduler.Default;
                this.currentList = new List<TSource>();

                this.endOfWindowSchedulerWorkItem = scheduler.Schedule(this.timeSpan, OnWindowCompleteDueTime);
            }

            private void OnWindowCompleteDueTime()
            {
                this.observer.OnNext(currentList);

                this.StartNewWindow();
            }

            public void OnCompleted()
            {
                CloseLastWindow();

                this.observer.OnCompleted();
            }

            private void CloseLastWindow()
            {
                this.endOfWindowSchedulerWorkItem.Dispose();
                this.observer.OnNext(currentList);
            }

            public void OnError(Exception error)
            {
                CloseLastWindow();

                this.observer.OnError(error);
            }

            public void OnNext(TSource value)
            {
                this.currentList.Add(value);
            }
        }
    }
}