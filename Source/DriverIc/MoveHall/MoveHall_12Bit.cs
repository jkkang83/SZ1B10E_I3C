using FZ4P.DriverIc.SlaveID.Context;
using FZ4P.DriverIc.SlaveID.ResultData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ4P.DriverIc.MoveHall
{
    public class MoveHall_12Bit : IMoveHall
    {
        private readonly IDlnInterface _dln;
        private readonly ActuatorSlaveData slaveID;
        public MoveHall_12Bit(IDlnInterface dln)
        {
            _dln = dln;
            var strategySlaveIDContext = new StrategySlaveIDContext();
            slaveID = strategySlaveIDContext.GetSlaveID(ActuatorType.Type1C87);
        }

        public bool Move(int ch, string name, int pos, bool openLoop = false)
        {
            int data = pos << 4;
            byte[] buff = new byte[2] { (byte)(data >> 8), (byte)(data % 256) };


            if (name.Contains("X"))
            {
                if (!_dln.WriteArray(ch, slaveID.XSlaveAddr, 0x00, buff)) return false;
            }
            else if (name.Contains("Y1"))
            {
                if (!_dln.WriteArray(ch, slaveID.Y1SlaveAddr, 0x00, buff)) return false;
            }
            else if (name.Contains("Y2"))
            {
                if (slaveID.Y2SlaveAddr != 0x00)
                {
                    if (!_dln.WriteArray(ch, slaveID.Y2SlaveAddr, 0x00, buff)) return false;
                }
            }
            else if (name.Contains("Y"))
            {
                if (!_dln.WriteArray(ch, slaveID.Y1SlaveAddr, 0x00, buff)) return false;
                if (slaveID.Y2SlaveAddr != 0x00)
                {
                    if (!_dln.WriteArray(ch, slaveID.Y2SlaveAddr, 0x00, buff)) return false;
                }
            }
            return true;
        }
    }
}
