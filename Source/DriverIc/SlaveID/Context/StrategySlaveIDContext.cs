using FZ4P.DriverIc.SlaveID.ResultData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ4P.DriverIc.SlaveID.Context
{
    public enum ActuatorType
    {
        Type2810,
        Type1C86,
        Type1C87,
    };

    public class StrategySlaveIDContext
    {
        private IStrategySlaveID Executor;

        public ActuatorSlaveData GetSlaveID(ActuatorType Type)
        {
            switch (Type)
            {
                case ActuatorType.Type2810:
                    Executor = new Strategy_SlaveID_2810();
                    break;
                case ActuatorType.Type1C86:
                    throw new Exception($"현재 구성된 객체가 없습니다. Type Name {nameof(ActuatorType.Type1C86)}");
                    break;
                case ActuatorType.Type1C87:
                    Executor = new Strategy_SlaveID_1C87();
                    break;
                default
                    : return null;
            }
            return Executor.GetSlaveID();
        }
    }
}
