using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ4P.DriverIc.MoveHall
{
    public interface IMoveHall
    {
        bool Move(int ch, string name, int pos, bool openLoop = false);
    }
}
