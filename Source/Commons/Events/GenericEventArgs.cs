using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ4P.Commons.Events
{
    public class GenericEventArgs<TParamType> : EventArgs
    {
        TParamType paramType;

        public GenericEventArgs(TParamType value)
        {
            paramType = value;
        }
    }
}
