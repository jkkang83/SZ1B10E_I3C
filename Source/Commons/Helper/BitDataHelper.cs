using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ4P.Commons.Helper
{
    public static class BitDataHelper
    {
        public static byte SetBits(byte source,byte targetByte)
        {
            return (byte)(source | targetByte);
        }

        public static byte ClearBits(byte source, byte targetByte)
        {
            byte mask = (byte)~targetByte;
            return (byte)(source & mask);
        }
    }
}
