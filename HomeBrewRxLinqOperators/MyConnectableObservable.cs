using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Text;

namespace HomeBrewRxLinqOperators
{
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
}
