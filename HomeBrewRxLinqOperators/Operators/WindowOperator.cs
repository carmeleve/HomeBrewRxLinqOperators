using System;
using System.Reactive.Concurrency;
using System.Reactive.Subjects;

namespace HomeBrewRxLinqOperators
{
    internal class WindowOperator<TSource> : IObservable<IObservable<TSource>>
    {
        private IObservable<TSource> source;
        private TimeSpan timeSpan;

        public WindowOperator(IObservable<TSource> source, TimeSpan timeSpan)
        {
            this.source = source;
            this.timeSpan = timeSpan;
        }

        public IDisposable Subscribe(IObserver<IObservable<TSource>> observer)
        {
            var bufferer = new Windower(observer, this.timeSpan);
            return this.source.Subscribe(bufferer);
        }

        private class Windower : IObserver<TSource>
        {
            private readonly IObserver<IObservable<TSource>> observer;
            private TimeSpan timeSpan;
            private Subject<TSource> currentWindow;
            private IDisposable endOfWindowSchedulerWorkItem;

            public Windower(IObserver<IObservable<TSource>> observer, TimeSpan timeSpan)
            {
                this.observer = observer;
                this.timeSpan = timeSpan;

                StartNewWindow();
            }

            private void StartNewWindow()
            {
                var scheduler = Scheduler.Default;
                this.currentWindow = new Subject<TSource>();
                this.observer.OnNext(currentWindow);
                this.endOfWindowSchedulerWorkItem = scheduler.Schedule(this.timeSpan, OnWindowCompleteDueTime);
            }

            private void OnWindowCompleteDueTime()
            {
                this.currentWindow.OnCompleted();
                this.StartNewWindow();
            }

            public void OnCompleted()
            {
                CloseLastWindow();

                this.observer.OnCompleted();
            }

            private void CloseLastWindow()
            {
                this.currentWindow.OnCompleted();
                this.endOfWindowSchedulerWorkItem.Dispose();
            }

            public void OnError(Exception error)
            {
                CloseLastWindow();

                this.observer.OnError(error);
            }

            public void OnNext(TSource value)
            {
                this.currentWindow.OnNext(value);
            }
        }
    }
}