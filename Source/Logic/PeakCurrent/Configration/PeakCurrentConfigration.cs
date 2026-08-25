using FZ4P.DriverIc.Interfaces;
using FZ4P.Logic.PeakCurrent.Configration.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ4P.Logic.PeakCurrent.Configration
{
    public class PeakCurrentConfigration : PeakCurrentConfigBase
    {
        public IDriverIC_AF_Function AFFunction { get; set; } = null;
        public IDriverIC_OIS_Function OIS_Function { get; set; } = null;
        public IDriverIC_FRA_Function FRA_Function { get; set; } = null;
        public IGPIOOutput GPIOOutput { get; set; } = null;
    }
}
