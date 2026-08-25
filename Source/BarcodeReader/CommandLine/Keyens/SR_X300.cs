using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ4P.BarcodeReader.CommandLine.Keyens
{
    public class SR_X300 : IScanCommandLine
    {
        private string EndOfLine = "\r\n";
        public string TrigerCommandToString()
        {
            return "LON" + EndOfLine;
        }

        public string TrigerStopCommandToString()
        {
            return "QUIT" + EndOfLine;
        }
    }
}
