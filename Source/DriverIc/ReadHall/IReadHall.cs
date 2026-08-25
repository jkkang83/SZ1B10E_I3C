using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ4P.DriverIc.ReadHall
{
    public interface IReadHall
    {
        int ReadHall(int ch, string name);
    }
}
