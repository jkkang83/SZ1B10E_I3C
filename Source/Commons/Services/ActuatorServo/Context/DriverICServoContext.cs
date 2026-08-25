using FZ4P.Commons.Services.ActuatorServo.Concreate;
using FZ4P.Commons.Services.DriveICManualMove.Concreate;
using FZ4P.DriverIc.Interfaces;
using FZ4P.DriverIc.SlaveID.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ4P.Commons.Services.ActuatorServo.Context
{
    public class DriverICServoContext
    {
        private readonly IActuatorActionExecutor _executor;
        public DriverICServoContext(ActuatorType type, AK73XX driver)
        {
            switch (type)
            {
                case ActuatorType.Type2810:
                    
                    break;
                case ActuatorType.Type1C87:
                    _executor = new ActuatorActionExecutor_1C87(driver);
                    break;
                default:
                    break;
            }
        }
        public DriverICServoContext(ActuatorType type, IAK73XX driver)
        {
            if (driver is AK73XX _ak)
            {
                switch (type)
                {
                    case ActuatorType.Type2810:

                        break;
                    case ActuatorType.Type1C87:
                        _executor = new ActuatorActionExecutor_1C87(_ak);
                        break;
                    default:
                        break;
                }
            }
        }

        public void ServoOnOff(int iCh, string axisName, bool OnOff)
        {
            _executor.Execute(iCh, axisName, OnOff);
        }
    }
}
