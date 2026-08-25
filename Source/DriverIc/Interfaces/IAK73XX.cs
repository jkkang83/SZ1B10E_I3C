using FZ4P.DriverIc.MoveHall.Context;
using FZ4P.DriverIc.ReadHall.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FZ4P.DriverIc.Interfaces
{
    public interface IAK73XX
    {
        #region OIS Function 동작이지만 AK73XX에 매우 근접한 로직
        void AK7326_IC_Mode(int ch, int axis, byte mode);
        void AK7326_IC_Data(int ch);
        void AK7326_IC_reset(int ch);
        bool AK7326_memory_update(int ch, byte dir, int mode);
        void AK7326_PM_set_slave(int ch, int axis);
        void AK7326_EEPROM_Writecheck(int ch, byte dir, byte address, byte value);

        void CurrentSetRegister(int ch, int iAxis);
        short GetPeakCurrent(int ch, int iAxis);
        #endregion
    }
}
