using FZ4P.Commons.Events;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ4P.Commons.Helper
{
    public class IndexValueType<T>
    {
        public int Index;
        public T Value;

        public override string ToString() => $"[{Index}]{Value}";
    }

    public static class PropertiesHelper
    {
        public static T GetValue<T>(PropertyChangedEventArgs e)
        {
            switch (e)
            {
                case CustomPropertyChangedEventArgs<T> ev:
                    return ev.Value;
                default:
                    return default;
            }
        }
        public static IndexValueType<T> GetValueIndex<T>(PropertyChangedEventArgs e)
        {
            IndexValueType<T> result = new IndexValueType<T>();

            switch (e)
            {
                case ArrayChangedEventArgs<T> ev:
                    result.Index = ev.Index;
                    result.Value = ev.Value;
                    return result;
                default:
                    return default;
            }
        }
    }
}
