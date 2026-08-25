using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ4P.DriverIc.Interfaces
{
    public interface IDriverIC_OIS_Function
    {
        #region OIS Function
        bool ChangeSlaveAddrOIS(int ch);
        void OIS_drift_test_mode_init(int ch, bool status);
        void OIS_drift_test_mode_close(int ch, bool status);
        void OISOn(int ch, string name, bool isOn);
        bool Move(int ch, string name, int pos, bool openLoop = false);
        int ReadHall(int ch, string name);
        int ReadHallOpenLoop(int ch, string name);
        #endregion
    }
}
