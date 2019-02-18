using System;
using System.Collections.Generic;
using System.Text;

namespace HomeBrewRxLinqOperators.Operators
{
    public class JoinOperator<TLeft, TRight, TResult, TLeftDuration, TRightDuration> : IObservable<TResult>
    {
        private IObservable<TLeft> left;
        private IObservable<TRight> right;
        private Func<TLeft, IObservable<TLeftDuration>> leftDurationSelector;
        private Func<TRight, IObservable<TRightDuration>> rightDurationSelector;
        private Func<TLeft, TRight, TResult> resultSelector;

        public JoinOperator(IObservable<TLeft> left, 
            IObservable<TRight> right, 
            Func<TLeft, IObservable<TLeftDuration>> leftDurationSelector, 
            Func<TRight, IObservable<TRightDuration>> rightDurationSelector, 
            Func<TLeft, TRight, TResult> resultSelector)
        {
            this.left = left;
            this.right = right;
            this.leftDurationSelector = leftDurationSelector;
            this.rightDurationSelector = rightDurationSelector;
            this.resultSelector = resultSelector;
        }

        public IDisposable Subscribe(IObserver<TResult> observer)
        {
            return new Joiner(observer, this);
        }

        private class Joiner : IDisposable
        {
            private IObserver<TResult> observer;
            private JoinOperator<TLeft, TRight, TResult, TLeftDuration, TRightDuration> joinOperator;
            private IList<TLeft> currentlyOpenLeft;
            private IList<TRight> currentlyOpenRight;
            private IList<IDisposable> openSubs;

            public Joiner(IObserver<TResult> observer, JoinOperator<TLeft, TRight, TResult, TLeftDuration, TRightDuration> joinOperator)
            {
                this.observer = observer;
                this.joinOperator = joinOperator;
                this.openSubs = new List<IDisposable>();
                this.currentlyOpenRight = new List<TRight>();
                this.currentlyOpenLeft = new List<TLeft>();
                this.openSubs.Add(this.joinOperator.left.Subscribe(new CallbackObserver<TLeft>(OnLeftNext, OnError, OnCompleted)));
                this.openSubs.Add(this.joinOperator.right.Subscribe(new CallbackObserver<TRight>(OnRightNext, OnError, OnCompleted)));
            }

            public void Dispose()
            {
                foreach (var sub in openSubs)
                {
                    sub.Dispose();
                }

                openSubs.Clear();
            }

            public void OnCompleted()
            {
                Dispose();

                this.observer.OnCompleted();
            }

            public void OnError(Exception error)
            {
                Dispose();

                this.observer.OnError(error);
            }

            public void OnLeftNext(TLeft value)
            {
                this.currentlyOpenLeft.Add(value);

                openSubs.Add(this.joinOperator.leftDurationSelector(value).Subscribe(new CallbackObserver<TLeftDuration>(_ => OnFinished(),
                    _ => OnFinished(), OnFinished)));

                foreach (var rightValue in this.currentlyOpenRight)
                {
                    this.observer.OnNext(this.joinOperator.resultSelector(value, rightValue));
                }

                void OnFinished()
                {
                    this.currentlyOpenLeft.Remove(value);
                }
            }

            public void OnRightNext(TRight value)
            {
                this.currentlyOpenRight.Add(value);

                openSubs.Add(this.joinOperator.rightDurationSelector(value).Subscribe(new CallbackObserver<TRightDuration>(_ => { },
                    _ => OnFinished(), OnFinished)));

                foreach (var leftValue in this.currentlyOpenLeft)
                {
                    this.observer.OnNext(this.joinOperator.resultSelector(leftValue, value));
                }

                void OnFinished()
                {
                    this.currentlyOpenRight.Remove(value);
                }
            }
        }
    }
}
