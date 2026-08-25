using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FZ4P.Commons.Services.ActuatorServo.Concreate
{
    public class ActuatorActionExecutor_1C87 : IActuatorActionExecutor
    {
        private readonly AK73XX _driver;
        public ActuatorActionExecutor_1C87(AK73XX driver)
        {
            _driver = driver;
        }

        public void Execute(int ich, string axis, bool OnOff)
        {
            try
            {
                if (axis.Contains("AF") || axis.Contains("Z"))
                {
                    _driver.AFOnOff(ich, OnOff);
                }
                else
                {
                    _driver.OISOn(ich, axis, OnOff);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"동작 이상  : {ex.Message}");
            }
        }
    }
}
