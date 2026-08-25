using FZ4P.Logic.OISPeakCurrent.Params.Base;
using FZ4P.Logic.PeakCurrent.ReturnType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ4P.Logic.OISPeakCurrent.Interfaces
{
    public interface ISetParamsPeakCurrent
    {
        void SetPeakCurrent(PeakCurrentParamBase _param);
    }
    public interface IPeakCurrentExecutor : ISetParamsPeakCurrent
    {
        PeakCurrentReturn Execute();
    }
}
