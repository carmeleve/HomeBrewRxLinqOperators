using System;
using System.Collections.Generic;
using System.Text;

namespace HomeBrewRxLinqOperators
{
    public class MyObserver : IObserver<int>
    {
        public void OnNext(int value)
        {
            Console.WriteLine(value);
        }
        public void OnCompleted() { }

        public void OnError(Exception error) { }

    }
}
