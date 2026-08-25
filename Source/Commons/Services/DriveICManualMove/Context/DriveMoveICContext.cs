using FZ4P.Commons.Services.DriveICManualMove.Concreate;
using FZ4P.DriverIc.Interfaces;
using FZ4P.DriverIc.SlaveID.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ4P.Commons.Services.DriveICManualMove
{
    public class DriveMoveICContext
    {
        private readonly IManaualMove move;
        public DriveMoveICContext(ActuatorType type, AK73XX driver)
        {
            switch (type)
            {
                case ActuatorType.Type2810:
                    move = new ManualMove_2810(driver);
                    break;
                case ActuatorType.Type1C87:
                    move = new ManualMove_1C87(driver);
                    break;
                default:
                    move = new ManualMove_2810(driver);
                    break;
            }
        }
        public DriveMoveICContext(ActuatorType type, IAK73XX driver)
        {
            if (driver is AK73XX ak)
            {
                switch (type)
                {
                    case ActuatorType.Type2810:
                        move = new ManualMove_2810(ak);
                        break;
                    case ActuatorType.Type1C87:
                        move = new ManualMove_1C87(ak);
                        break;
                    default:
                        move = new ManualMove_2810(ak);
                        break;
                }
            }
        }

        public void ManualMove(int iCh,string axis,int iPositionCode)
        {
            move.SingleAxisExecute(iCh, axis, iPositionCode);
        }

        public void ManualMoveAxis(int iCh, int iPositionCodeX, int iPositionCodeY, int iPositionCodeZ)
        {
            move.Execute(iCh, iPositionCodeX, iPositionCodeY, iPositionCodeZ);
        }
    }
}
