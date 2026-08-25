using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ4P.DriverIc.ReadHall.Context
{
    public enum IC_BITUSE
    {
        BIT_12 = 0,
        BIT_13 = 1,
    };

    public class ReadHallContext
    {
        private readonly IReadHall readHall;
        public ReadHallContext(IC_BITUSE type, IDlnInterface dln)
        {
            switch (type)
            {
                case IC_BITUSE.BIT_12:
                    readHall = new ReadHall_12bit(dln);
                    break;

                case IC_BITUSE.BIT_13:
                    readHall = new ReadHall_13bit(dln);
                    break;
            }
        }

        public int ReadHall(int ch, string name)
        {
            return readHall.ReadHall(ch, name);
        }
    }
}
