using FZ4P.DriverIc.Interfaces;
using FZ4P.Logic.OISPeakCurrent.Interfaces;
using FZ4P.Logic.OISPeakCurrent.Params;
using FZ4P.Logic.OISPeakCurrent.Params.Base;
using FZ4P.Logic.PeakCurrent.ReturnType;
using FZ4P.Processes.Interfaces;
using System;
using System.Threading;

namespace FZ4P.Logic.OISPeakCurrent
{
    public class OIS_X_PeakCurrentLogic : IPeakCurrentExecutor
    {
        private OIS_X_PeakCurrentParam _param;

        private readonly IDriverIC_AF_Function _afFunction;
        private readonly IDriverIC_OIS_Function _oisFunction;
        private readonly IGPIOOutput _gpioOutput;
        private readonly ILogView _logView;

        public OIS_X_PeakCurrentLogic(IDriverIC_AF_Function af, IDriverIC_OIS_Function oisFunction, ILogView logView, IGPIOOutput gpioOutput)
        {
            _afFunction = af ?? throw new ArgumentNullException(nameof(af));
            _oisFunction = oisFunction ?? throw new ArgumentNullException(nameof(oisFunction));
            _logView = logView ?? throw new ArgumentNullException(nameof(logView));
            _gpioOutput = gpioOutput ?? throw new ArgumentNullException(nameof(gpioOutput));
        }

        public void SetPeakCurrent(PeakCurrentParamBase param)
        {
            if (param is OIS_X_PeakCurrentParam paramConvert)
                _param = paramConvert;
            else
                throw new InvalidCastException($"[{this.GetType().Name}] 잘못된 파라미터가 전송 되었습니다, 확인 바랍니다.");
        }

        public PeakCurrentReturn Execute()
        {
            _gpioOutput.PeakDetector(1, PeakDetectState.Hold);

            _gpioOutput.PeakDetector(1, PeakDetectState.Reset);
            return new PeakCurrentReturn();
        }
    }
}
