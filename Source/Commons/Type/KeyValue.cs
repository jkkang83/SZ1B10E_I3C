using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ4P.Commons.Type
{
    public class IndexValue <TIndexType, TValueType>
    {
        public TIndexType Index { get; set; }
        public TValueType Value { get; set; }
    }
}
