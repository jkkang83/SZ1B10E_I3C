using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ4P.BarcodeReader.CommandLine
{
    public interface IScanCommandLine
    {
        string TrigerCommandToString();
        string TrigerStopCommandToString();
    }
}
