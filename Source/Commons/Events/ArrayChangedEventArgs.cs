using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ4P.Commons.Events
{
    public class ArrayChangedEventArgs<T> : PropertyChangedEventArgs
    {
        public int Index { get; }
        public T Value { get; }

        public ArrayChangedEventArgs(int index, string propertyName, T propertyValue) : base(propertyName)
        {
            Index = index;
            Value = propertyValue;
        }
    }
}
