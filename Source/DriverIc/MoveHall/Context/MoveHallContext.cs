using FZ4P.DriverIc.ReadHall;
using FZ4P.DriverIc.ReadHall.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ4P.DriverIc.MoveHall.Context
{
    public class MoveHallContext
    {
        private readonly IMoveHall moveHall;
        public MoveHallContext(IC_BITUSE type, IDlnInterface dln)
        {
            switch (type)
            {
                case IC_BITUSE.BIT_12:
                    moveHall = new MoveHall_12Bit(dln);
                    break;

                case IC_BITUSE.BIT_13:
                    moveHall = new MoveHall_13Bit(dln);
                    break;
            }
        }

        public bool Move(int ch, string name, int pos, bool openLoop = false)
        {
            return moveHall.Move(ch, name, pos, openLoop);
        }
    }
}
