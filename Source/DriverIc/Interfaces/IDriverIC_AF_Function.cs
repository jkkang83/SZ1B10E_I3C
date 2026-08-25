using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ4P.DriverIc.Interfaces
{
    public interface IDriverIC_AF_Function
    {
        #region AF Function
        bool ChangeSlaveAddr(int ch);
        bool AF_Memory_Update(int ch, int mode);
        void AF_IC_Data(int ch);
        bool ICReset(int ch);
        void AFSleep(int ch);
        void AFOnOff(int ch, bool isOn);
        void AFMove(int ch, int code);

        void RegisterZeroSet(int iCh, int TargetSlaveID, byte address);
        int ReadAFHall(int ch);
        #endregion
    }
}
