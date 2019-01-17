using System;
using System.Collections.Generic;
using System.Text;

namespace HomeBrewRxLinqOperators
{
    public interface IGroupedObservable<out TKey, out TElement> : IObservable<TElement>
    {
        // Gets the common key.
        TKey Key { get; }
    }

}
