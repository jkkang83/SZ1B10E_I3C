using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ4P
{
    public enum TargetSignalType
    {
        None = 0,
        Emergency = 1,
        Start = 2, 
        Stop = 3,
    }
    public class DebounceEventArgs : EventArgs
    {
        public TargetSignalType SignalType { get; }
        public bool IsSuccess { get; }
        public bool SignalState { get; }

        public DebounceEventArgs(bool success, TargetSignalType signalType = TargetSignalType.None, bool signalState = false)
        {
            IsSuccess = success;
            SignalType = signalType;
            SignalState = signalState;
        }
    }
}
