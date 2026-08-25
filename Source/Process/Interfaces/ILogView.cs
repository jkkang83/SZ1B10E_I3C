using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ4P.Processes.Interfaces
{
    public interface ILogView
    {
        void AddLog(int iCh, string msg);
    }
}
