using FZ4P.DriverIc.SlaveID.Context;
using FZ4P.DriverIc.SlaveID.ResultData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ4P.DriverIc.ReadHall
{
    public class ReadHall_13bit : IReadHall
    {
        private readonly IDlnInterface _dln;
        private readonly ActuatorSlaveData slaveID;

        public ReadHall_13bit(IDlnInterface dln)
        {
            _dln = dln;
            var strategySlaveIDContext = new StrategySlaveIDContext();
            slaveID = strategySlaveIDContext.GetSlaveID(ActuatorType.Type1C87);
        }

        public int ReadHall(int ch, string name)
        {
            int addr = 0x00;

            if (name.Contains("X")) addr = slaveID.XSlaveAddr;
            else if (name.Contains("Y")) addr = slaveID.Y1SlaveAddr;
            else if (name.Contains("Y2")) addr = slaveID.Y2SlaveAddr;

            byte[] data = new byte[2];
            if (addr != 0x00) _dln.ReadArray(ch, addr, 0x84, data);
            if (slaveID.Y2SlaveAddr != 0x00) _dln.ReadArray(ch, addr, 0x84, data);

            return ((data[0] << 8) + data[1]) >> 3;
        }
    }
}
