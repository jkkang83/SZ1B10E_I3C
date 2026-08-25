using FZ4P.DriverIc.Interfaces;
using FZ4P.Processes.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ4P.Logic.PeakCurrent.Configration.Base
{
    public abstract class PeakCurrentConfigBase
    {
        public IAK73XX Dln { get; set; } = null;
        public ILogView LogView { get; set; } = null;
    }
}
