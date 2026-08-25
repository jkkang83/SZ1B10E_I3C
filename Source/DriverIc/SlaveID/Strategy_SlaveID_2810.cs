using FZ4P.DriverIc.SlaveID.ResultData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ4P.DriverIc.SlaveID
{
    public class Strategy_SlaveID_2810 : IStrategySlaveID
    {
        public ActuatorSlaveData GetSlaveID()
        {
            return new ActuatorSlaveData()
            {
                AFOriginAddr = 0x0C,
                XOriginAddr = 0x0E,
                Y1OriginAddr = 0x4E,
                Y2OriginAddr = 0x00,
                //SU2810
                AF_Addr = 0x28,
                XSlaveAddr = 0x70,
                Y1SlaveAddr = 0x30,
                Y2SlaveAddr = 0x00,
                FRA_Addr = 0x14,
                FRA_AFSlaveAddr = 0x50,
                FRA_XSlaveAddr = 0xE0,
                FRA_Y1SlaveAddr = 0x60,
                FRA_Y2SlaveAddr = 0x00,

                AF_MID_CODE = 2048,
                AF_MAX_CODE = 4095,
                AF_MIN_CODE = 0,
            };
        }
    }
}
