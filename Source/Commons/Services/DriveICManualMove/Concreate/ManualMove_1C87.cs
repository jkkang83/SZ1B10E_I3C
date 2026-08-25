using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FZ4P.Commons.Services.DriveICManualMove.Concreate
{
    public class ManualMove_1C87 : IManaualMove
    {
        private readonly AK73XX _driver;
        public ManualMove_1C87(AK73XX driver)
        {
            _driver = driver;
        }

        public void SingleAxisExecute(int ich, string axis, int iCodePosition)
        {
            try
            {
                if(axis.Contains("AF") || axis.Contains("Z"))
                    _driver.AFMove(ich, iCodePosition);
                else
                    _driver.Move(ich,axis,iCodePosition);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"동작 이상  : {ex.Message}");
            }
        }

        public void Execute(int ich, int iCodePositionAxisX, int iCodePositionAxisY, int iCodePositionAxisZ)
        {
            try
            {
                _driver.Move(ich, "X", iCodePositionAxisX);
                _driver.Move(ich, "Y", iCodePositionAxisY);
                _driver.AFMove(ich, iCodePositionAxisZ);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"동작 이상  : {ex.Message}");
            }
        }
    }
}
