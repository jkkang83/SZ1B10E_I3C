using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ4P.Commons.Services.ActuatorServo.Concreate
{
    public interface IActuatorActionExecutor
    {
        void Execute(int ich, string axis, bool OnOff);
    }
}
