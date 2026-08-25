using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ4P.Commons.Services.DriveICManualMove.Concreate
{
    public interface IManaualMove
    {
        void SingleAxisExecute(int ich, string axis, int iCodePosition);

        void Execute(int ich, int iCodePositionAxisX, int iCodePositionAxisY, int iCodePositionAxisZ);
    }
}
