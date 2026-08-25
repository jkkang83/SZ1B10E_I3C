using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ4P.DriverIc.SlaveID.ResultData
{
    public class ActuatorSlaveData
    {
        public int AFOriginAddr { get; set; } = 0x00;
        public int XOriginAddr { get; set; } = 0x00;
        public int Y1OriginAddr { get; set; } = 0x00;
        public int Y2OriginAddr { get; set; } = 0x00;
        public int AF_Addr { get; set; } = 0x00;
        public int XSlaveAddr { get; set; } = 0x00;
        public int Y1SlaveAddr { get; set; } = 0x00;
        public int Y2SlaveAddr { get; set; } = 0x00;
        public int FRA_Addr { get; set; } = 0x00;

        public int FRA_AFSlaveAddr { get; set; } = 0x00;
        public int FRA_XSlaveAddr { get; set; } = 0x00;
        public int FRA_Y1SlaveAddr { get; set; } = 0x00;
        public int FRA_Y2SlaveAddr { get; set; } = 0x00;

        public int AF_ADC_Addr { get; set; } = 0x00;
        public int OIS_ADC_Addr { get; set; } = 0x00;

        public int AF_MID_CODE { get; set; } = 0x00;
        public int AF_MIN_CODE { get; set; } = 0x00;
        public int AF_MAX_CODE { get; set; } = 0x00;

        public int OIS_MID_CODE { get; set; } = 0x00;
        public int OIS_MIN_CODE { get; set; } = 0x00;
        public int OIS_MAX_CODE { get; set; } = 0x00;
    }
}
