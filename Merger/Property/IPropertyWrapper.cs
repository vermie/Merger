using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Merger
{
    internal interface IPropertyWrapper<T>
    {
        string Name { get; }

        void Copy(T source, T destination);

        bool AreEqual(T source, T destination, out Conflict conflict);

        bool IsDefaultValueOn(T instance);
    }
}
