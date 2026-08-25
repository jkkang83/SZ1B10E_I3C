using FZ4P.Extensions;
using S2System.Vision;
using System;
using System.Diagnostics;

namespace FZ4P
{
    public class OpenLoopStrockCheck : IOpenLoopExecutor
    {
        private readonly MILlib _cam;
        private readonly FVision _vision;
        private readonly AK73XX _driveIC;
        private readonly LogText _logText;
        private readonly Global _G;

        private int agingCount = 0;
        private double OldStroke = 0, NewStroke = 0;
        private FindResult tmpres = new FindResult();
        private double[] zVal = new double[2];

        public OpenLoopStrockCheck(MILlib Cam , FVision vision, AK73XX driveIC, LogText logText, Global global)
        {
            _cam = Cam;
            _vision = vision;
            _driveIC = driveIC;
            _logText = logText;
            _G = global;
        }

        public void Execute(int iCh)
        {
            Measure(0);

            for (agingCount = 0, NewStroke = 0; (agingCount < 10) || ((agingCount < 20) && (NewStroke > OldStroke)); agingCount++)
            {
                OldStroke = NewStroke;
                _driveIC.AFMove(iCh, 4095);
                Wait(50);
                tmpres = Measure(1);
                Wait(50);
                zVal[0] = tmpres.cz[0];
                _driveIC.AFMove(iCh, 0);
                Wait(50);
                tmpres = Measure(1);
                Wait(50);
                zVal[1] = tmpres.cz[0];
                NewStroke = Math.Abs(zVal[1] - zVal[0]);
                AddLog(iCh, $"{agingCount + 1} : {NewStroke.ToString("F3")}");
            }
        }

        private void AddLog(int ch, string msg)
        {
            STATIC.SaveLogData += msg + "\r\n";
            _logText.Log(msg);
        }
        private FindResult Measure(int index, string filePath = "")
        {
            FindResult res = new FindResult();

            _cam.Grab(index);
            if (filePath == "")
            {
                res = _vision.MeasureTxTyTz(index);
            }
            else
            {
                res = _vision.MeasureTxTyTz(index).SaveImage(_G, index, filePath);
            }
            return res;
        }
        private void Wait(int ms)
        {
            //       Thread.Sleep(ms);
            ms = ms * 1000;
            Stopwatch startNew = Stopwatch.StartNew();

            long usDelayTick = (ms * Stopwatch.Frequency) / 1000000;

            while (startNew.ElapsedTicks < usDelayTick) ;
        }
    }
}
