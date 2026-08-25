using FZ4P.Commons.Services.ActuatorServo.Context;
using FZ4P.Commons.Services.DriveICManualMove;
using FZ4P.DriverIc.Interfaces;
using FZ4P.DriverIc.SlaveID.Context;
using FZ4P.Logic.OISPeakCurrent.Interfaces;
using FZ4P.Logic.OISPeakCurrent.Params;
using FZ4P.Logic.OISPeakCurrent.Params.Base;
using FZ4P.Logic.PeakCurrent.ReturnType;
using FZ4P.Processes.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FZ4P.Logic.OISPeakCurrent
{
    public class AFPeakCurrentLogic : IPeakCurrentExecutor
    {
        public AFPeakCurrentParam _param;

        private readonly IDriverIC_AF_Function _afFunction;
        private readonly IDriverIC_OIS_Function _oisFunction;
        private readonly IAK73XX _aKM;
        private readonly IGPIOOutput _gpioOutput;
        private readonly ILogView _logView;

        public AFPeakCurrentLogic(IDriverIC_AF_Function af, IDriverIC_OIS_Function oisFunction,IAK73XX AKM, ILogView logView, IGPIOOutput gpioOutput)
        {
            _afFunction = af ?? throw new ArgumentNullException(nameof(af));
            _oisFunction = oisFunction ?? throw new ArgumentNullException(nameof(oisFunction));
            _aKM = AKM ?? throw new ArgumentNullException(nameof(AKM));
            _logView = logView ?? throw new ArgumentNullException(nameof(logView));
            _gpioOutput = gpioOutput ?? throw new ArgumentNullException(nameof(gpioOutput));
        }

        public void SetPeakCurrent(PeakCurrentParamBase param)
        {
            if (param is AFPeakCurrentParam paramConvert)
                _param = paramConvert;
            else
                throw new InvalidCastException($"[{this.GetType().Name}] 잘못된 파라미터가 전송 되었습니다, 확인 바랍니다.");
        }

        public PeakCurrentReturn Execute()
        {
            try
            {
                int SettingAxisType = 1;            //AF 
                List<short> currentBuffer = new List<short>();

                //전원 인가
                _gpioOutput.PowerOnOff(0, true);
                Thread.Sleep(50);
                ServoOnOff(true);
                //전류 측정 ADC init
                _gpioOutput.PeakDetector(0, PeakDetectState.Hold);
                _aKM.CurrentSetRegister(_param.Channel, SettingAxisType);

                WaitMoveZero();

                MoveCode(4095);

                //데이터 게더링 
                while (_afFunction.ReadAFHall(_param.Channel) < 4090)
                {
                    currentBuffer.Add(_aKM.GetPeakCurrent(_param.Channel, SettingAxisType));
                    _logView.AddLog(_param.Channel, $"Read HAll : {_afFunction.ReadAFHall(_param.Channel)}");
                }

                _logView.AddLog(_param.Channel, $"Current Data Count : {currentBuffer.Count}");

                if (currentBuffer.Count == 0)
                    return null;
                    
                //Scale연산
                double scope = 1;
                var resultCollection = currentBuffer.Select(element => element * scope).ToList();
                var peakValue = resultCollection.Max();
                var minValue = resultCollection.Min();
                var avgValue = resultCollection.Average();

                resultCollection.
                    ForEach(element => { _logView.AddLog(_param.Channel, $"Current Data : {element}"); });
                
                _logView.AddLog(_param.Channel, $"Peak : {peakValue}, Min : {minValue}, Average : {avgValue},");
                _gpioOutput.PeakDetector(0, PeakDetectState.Reset);

                //전원 종료
                ServoOnOff(false);
                Thread.Sleep(50);
                _gpioOutput.PowerOnOff(0, false);

                return new PeakCurrentReturn()
                {
                    IsSeccess = true,
                    Average = avgValue,
                    Min = minValue,
                    Max = peakValue,
                    RestulCollection = resultCollection,
                };
            }
            catch (Exception ex) 
            {
                throw new Exception($"[{this.GetType().Name}][{ex.Message}]");
            }
        }

        private void ServoOnOff(bool onoff)
        {
            var selected = ActuatorType.Type1C87;
            var ServoContext = new DriverICServoContext(selected, _aKM);
            ServoContext.ServoOnOff(0, "AF", onoff);
        }

        private void MoveCode(int iPositionCode)
        {
            var selected = ActuatorType.Type1C87;
            var Context = new DriveMoveICContext(selected, _aKM);
            Context.ManualMove(_param.Channel, "AF", iPositionCode);
        }

        private void WaitMoveZero()
        {
            MoveCode(0);
            while (_afFunction.ReadAFHall(_param.Channel) <= 0)
            {
                Thread.Sleep(1);
            }
        }
    }
}
