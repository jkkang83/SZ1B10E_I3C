using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ4P.Commons.Events
{
    public class CustomPropertyChangedEventArgs<T> : PropertyChangedEventArgs
    {
        public T Value { get; }
        public CustomPropertyChangedEventArgs(string propertyName, T propertyValue) : base(propertyName)
        {
            Value = propertyValue;
        }
    }
}
