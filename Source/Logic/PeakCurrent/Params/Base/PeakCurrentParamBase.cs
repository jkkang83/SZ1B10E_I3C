using FZ4P.DriverIc.Interfaces;
using FZ4P.Processes.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ4P.Logic.OISPeakCurrent.Params.Base
{
    public abstract class PeakCurrentParamBase
    {
        public int Channel { get; set; } = -1;                  //검사 채널
        public int SelectADCNumber { get; set; } = -1;          //ADC 선택 0 -> ADC1 , 1-> ADC2

    }
}
