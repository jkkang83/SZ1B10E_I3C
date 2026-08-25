using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ4P.DriverIc.Interfaces
{
    public interface IDriverIC_FRA_Function
    {
        bool FRA_Single(int ch, string name, int amp, int mode, List<double> freq, ref List<double> gain, ref List<double> phase);
        bool FRAModeEnable(int ch);
        bool FRAModeDisable(int ch);
        bool Set_Amp(int ch, int val);
        bool Set_Freq(int ch, int val);
        double Get_Gain(int ch);
        double Get_Phase(int ch, int mode);
        bool SetSlaveAddr(int ch, int addr);
    }
}
