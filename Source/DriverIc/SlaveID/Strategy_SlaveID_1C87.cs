using FZ4P.DriverIc.SlaveID.ResultData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ4P.DriverIc.SlaveID
{
    public class Strategy_SlaveID_1C87 : IStrategySlaveID
    {
        public ActuatorSlaveData GetSlaveID()
        {
            return new ActuatorSlaveData()
            {
                // AFOriginAddr = 0x0C;
                XOriginAddr = 0x0E,
                Y1OriginAddr = 0x4E,
                Y2OriginAddr = 0x00,

                //SO1C87
                AF_Addr = 0x0C,
                XSlaveAddr = 0x0E,      
                Y1SlaveAddr = 0x4E,     
                Y2SlaveAddr = 0x00,
                FRA_Addr = 0x14,

                FRA_AFSlaveAddr = 0x18,
                FRA_XSlaveAddr = 0x1C,
                FRA_Y1SlaveAddr = 0x9C,
                FRA_Y2SlaveAddr = 0xD8,

                AF_ADC_Addr = 0x48,
                OIS_ADC_Addr = 0x49,

                AF_MID_CODE = 2048,
                AF_MAX_CODE = 4095,
                AF_MIN_CODE = 0,

                OIS_MID_CODE = 4096,
                OIS_MAX_CODE = 8191,
                OIS_MIN_CODE = 0,
            };
        }
    }
}
