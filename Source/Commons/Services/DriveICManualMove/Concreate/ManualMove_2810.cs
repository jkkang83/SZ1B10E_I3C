using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FZ4P.Commons.Services.DriveICManualMove.Concreate
{
    public class ManualMove_2810 : IManaualMove
    {
        private readonly AK73XX _driver;
        public ManualMove_2810(AK73XX driver)
        {
            _driver = driver;
        }

        public void SingleAxisExecute(int ich, string axis, int iCodePosition)
        {
            try
            {
                if (axis.Contains("AF") || axis.Contains("Z"))
                {
                    _driver.RegisterZeroSet(ich, _driver.AF_Addr, 0x02);
                    _driver.AFMove(ich, iCodePosition);
                }
                else if (axis.Contains("X"))
                {
                    _driver.RegisterZeroSet(ich, _driver.XSlaveAddr, 0x02);
                    _driver.Move(ich, "X", iCodePosition);
                }
                else if (axis.Contains("Y"))
                {
                    _driver.RegisterZeroSet(ich, _driver.Y1SlaveAddr, 0x02);
                    _driver.Move(ich, "Y", iCodePosition);
                }
                else
                {
                    throw new ArgumentException("Aixs 파라미터가 정의되어 있지 않습니다.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"동작 이상  : {ex.Message}");
            }
        }

        public void Execute(int ich, int iCodePositionAxisX, int iCodePositionAxisY, int iCodePositionAxisZ)
        {
            _driver.RegisterZeroSet(ich, _driver.AF_Addr, 0x02);
            _driver.RegisterZeroSet(ich, _driver.XSlaveAddr, 0x02);
            _driver.RegisterZeroSet(ich, _driver.Y1SlaveAddr, 0x02);

            _driver.AFMove(ich, iCodePositionAxisZ);
            _driver.Move(ich, "X", iCodePositionAxisX);
            _driver.Move(ich, "Y", iCodePositionAxisY);
        }
    }
}
