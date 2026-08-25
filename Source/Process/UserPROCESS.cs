using Dln;
using Dln.Exceptions;
using FZ4P.Commons.Helper;
using FZ4P.Commons.Services.ActuatorServo.Context;
using FZ4P.Commons.Type;
using FZ4P.DriverIc.SlaveID.Context;
using FZ4P.Logic;
using FZ4P.Logic.OISPeakCurrent;
using FZ4P.Logic.OISPeakCurrent.Interfaces;
using FZ4P.Logic.OISPeakCurrent.Params;
using FZ4P.Logic.PeakCurrent.Configration;
using FZ4P.Logic.PeakCurrent.Params;
using MathNet.Numerics;
using MathNet.Numerics.Financial;
using MathNet.Numerics.Optimization.TrustRegion;
using OpenCvSharp;
using OpenCvSharp.Dnn;
using OpenCvSharp.Flann;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Xml.Linq;
using System.Xml.Schema;
using static alglib;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;

namespace FZ4P
{
    public partial class Process
    {
        int SinewaveXMaxDiff = 0;
        int SinewaveYMaxDiff = 0;
        int RingingXStabilizer = 0;
        int RingingYStabilizer = 0;
        int[] g_IME = new int[2];
        double[] AFCurrentMinMax = new double[2];
        double[] OISXCurrentMinMax = new double[2];
        double[] OISYCurrentMinMax = new double[2];

        private byte[] IC_DATA_AF = new byte[1];
        private byte[] IC_DATA_AF_REG = new byte[1];
        private byte[] IC_SETTING_AF = new byte[1];
        private byte[] IC_SETTING_AF_REG = new byte[1];

        void AddSequence()
        {
            ItemList.Add(new ActItems() { Name = "FW Download", Func = Act_FW_Download, IsMulti = true });
            //ItemList.Add(new ActItems() { Name = "AF HallCalibration", Func = Act_AFHallCalibration, IsMulti = true });
            //ItemList.Add(new ActItems() { Name = "OIS HallCalibration", Func = Act_OISHallCalubration, IsMulti = true });
            //ItemList.Add(new ActItems() { Name = "OIS IC Mount Error", Func = IME_Test, IsMulti = true });
            //ItemList.Add(new ActItems() { Name = "OIS XYZ Temperature", Func = TempTest, IsMulti = true });
            //ItemList.Add(new ActItems() { Name = "OIS XYZ Aging", Func = Act_CloseLoopAging });
            //ItemList.Add(new ActItems() { Name = "AF LinearityCompensation", Func = Act_AFLinComp });
            //ItemList.Add(new ActItems() { Name = "OIS LinearityCompensation", Func = Act_OISLinComp });
            //ItemList.Add(new ActItems() { Name = "X/Y Servo Decenter", Func = ServoDecenter, IsMulti = true });
            //ItemList.Add(new ActItems() { Name = "OIS X/Y OpenLoop", Func = OISOpenLoopTest, IsMulti = true });
            //ItemList.Add(new ActItems() { Name = "OIS X/Y OpenLoop1", Func = OISOpenLoopTest, IsMulti = true });
            //ItemList.Add(new ActItems() { Name = "OIS X/Y OpenLoop2", Func = OISOpenLoopTest, IsMulti = true });
            //ItemList.Add(new ActItems() { Name = "Auto Test", Func = AutoTest, IsMulti = true });
            //ItemList.Add(new ActItems() { Name = "OIS Sensitivity Test", Func = OISSensitivityTest, IsMulti = true });
            //ItemList.Add(new ActItems() { Name = "AF Aging", Func = Act_AFScanAging });
            //ItemList.Add(new ActItems() { Name = "AF Scan Driving", Func = Act_PreAFDriving });
            //ItemList.Add(new ActItems() { Name = "X/Y Drift Test", Func = Act_OISShift2, IsMulti = true });
            //ItemList.Add(new ActItems() { Name = "through Peak", Func = throughFRA, IsMulti = true });
            //ItemList.Add(new ActItems() { Name = "OIS Phase Margin", Func = OISPhasemargin, IsMulti = true });
            //ItemList.Add(new ActItems() { Name = "OIS Gain Margin", Func = OISGainmargin, IsMulti = true });
            //ItemList.Add(new ActItems() { Name = "OIS Loopgain", Func = OISLoopGain, IsMulti = true });
            //ItemList.Add(new ActItems() { Name = "AF Gain Margin", Func = AFGainMargin, IsMulti = true });
            //ItemList.Add(new ActItems() { Name = "AF Phase Margin", Func = AFPhaseMargin, IsMulti = true });
            //ItemList.Add(new ActItems() { Name = "Regster Check", Func = RegisterCheck, IsMulti = true });
            //ItemList.Add(new ActItems() { Name = "AF PID Verify", Func = AFPID_Verify, IsMulti = true });
            //ItemList.Add(new ActItems() { Name = "OIS PID Verify", Func = OIS_PIDVerify, IsMulti = true });
            //ItemList.Add(new ActItems() { Name = "OIS Stroke Accuracy", Func = OIS_Stroke_Accuracy, IsMulti = true });
            //ItemList.Add(new ActItems() { Name = "AF Stroke Accuracy", Func = AF_Stroke_Accuracy, IsMulti = true });
            //ItemList.Add(new ActItems() { Name = "OIS Accuracy Hall Inf", Func = OIS_Accuracy_Hall_Inf, IsMulti = true });
            //ItemList.Add(new ActItems() { Name = "OIS Accuracy Hall Mac", Func = OIS_Accuracy_Hall_Inf, IsMulti = true });
            //ItemList.Add(new ActItems() { Name = "AF Open Mode Area", Func = AF_OpenMode_Area, IsMulti = true });
            //ItemList.Add(new ActItems() { Name = "OIS square Test", Func = OIS_square_Test, IsMulti = true });
            //ItemList.Add(new ActItems() { Name = "OIS Shaking Test", Func = OIS_Shaking_Test, IsMulti = true });

            //PeakCurrent R&D 대상이고, Line에서는 제외...
            //ItemList.Add(new ActItems() { Name = "OIS X PeakCurrent", Func = PeakCurrent, IsMulti = true });
            //ItemList.Add(new ActItems() { Name = "OIS Y PeakCurrent", Func = PeakCurrent, IsMulti = true });
            //ItemList.Add(new ActItems() { Name = "AF PeakCurrent", Func = PeakCurrent, IsMulti = true });
        }
        #region CircleTestExample
        //private void OIS_Stroke_Accuracy(int port, string testItem, int InspCnt)
        //{
        //    int ch = port * 2;
        //    DrvIC.AFOnOff(ch, true);
        //    DrvIC.AFMove(ch, BestAFPos);
        //    AddLog(ch, $"Move AF Best Position : {BestAFPos}");
        //    Thread.Sleep(100);
        //    AddLog(ch, $"AF Read Hall : {DrvIC.ReadAFHall(ch)}");

        //    int centerCode = 4096;
        //    int rangeMinMax = 3200;
        //    double Sensitivity_X_3200 = 0;
        //    double Sensitivity_Y_3200 = 0;
        //    double d3deg_Target = 367;

        //    double X_3deg_code = 0;
        //    double Y_3deg_code = 0;

        //    foreach (var Cal in CalList[ch])
        //    {
        //        if (Cal.Name == "OIS X Scan")
        //        {
        //            Sensitivity_X_3200 = Cal.CalSensitivity(Cal.CodeX, Cal.StrokeX, centerCode - rangeMinMax, centerCode + rangeMinMax,
        //                Condition.XSensMinStroke, Condition.XSensMaxStroke, 0, Condition.iXDrvCodeMin, Condition.iXDrvCodeMax);
        //        }
        //        else if (Cal.Name == "OIS Y Scan")
        //        {
        //            Sensitivity_Y_3200 = Cal.CalSensitivity(Cal.CodeY, Cal.StrokeY, centerCode - rangeMinMax, centerCode + rangeMinMax,
        //                Condition.YSensMinStroke, Condition.YSensMaxStroke, 0, Condition.iYDrvCodeMin, Condition.iYDrvCodeMax);
        //        }
        //    }
        //    AddLog(ch, $"X Sensitivity Range Min : {centerCode - rangeMinMax}, Max : {centerCode + rangeMinMax}, Sensitivity : {Sensitivity_X_3200:F4}");
        //    AddLog(ch, $"Y Sensitivity Range Min : {centerCode - rangeMinMax}, Max : {centerCode + rangeMinMax}, Sensitivity : {Sensitivity_Y_3200:F4}");
        //    AddLog(ch, $"3 Db Target : {d3deg_Target}");
        //    X_3deg_code = d3deg_Target / Sensitivity_X_3200;
        //    Y_3deg_code = d3deg_Target / Sensitivity_Y_3200;

        //    AddLog(ch, $"X_3deg_code : {X_3deg_code:F4}");
        //    AddLog(ch, $"Y_3deg_code : {Y_3deg_code:F4}");

        //    List<int> X_Target = new List<int>();
        //    List<int> Y_Target = new List<int>();
        //    List<int> X_TargetHall = new List<int>();
        //    List<int> Y_TargetHall = new List<int>();

        //    for (int i = 0; i < 72; i++)
        //    {
        //        // 1. 스텝 인덱스(i)에 5를 곱해 각도(Degree)를 구하고, 이를 라디안(Radian)으로 변환
        //        double degree = i * 5; // 360 / 72 
        //        double radian = degree * Math.PI / 180.0;

        //        // 2. 중심 좌표(4096) + 진폭 * 삼각함수 적용 및 정수로 반올림
        //        int currentX = (int)Math.Round(centerCode + X_3deg_code * Math.Cos(radian));
        //        int currentY = (int)Math.Round(centerCode + Y_3deg_code * Math.Sin(radian));

        //        // 3. 계산된 좌표를 리스트에 추가
        //        X_Target.Add(currentX);
        //        Y_Target.Add(currentY);
        //    }

        //    DrvIC.OISOn(ch, "X", true);
        //    DrvIC.OISOn(ch, "Y", true);
        //    AddLog(ch, $"Step\tX_Target\tY_Target\tX_Hall\tY_Hall\tDiff_X(um)\tDiff_Y(um)");

        //    bool isAccuracyPass = true;
        //    double maxErrorUmX = 0.0;
        //    double maxErrorUmY = 0.0;
        //    // 엑셀 조건: 2 Cycle (72 step * 2 = 144 번 반복)
        //    int totalSteps = 72 * 2;

        //    for (int i = 0; i < totalSteps; i++)
        //    {
        //        // 72 이상 넘어가면 다시 0부터 타겟을 가져오도록 나머지 연산(%) 사용
        //        int targetIdx = i % 72;

        //        DrvIC.Move(ch, "X", X_Target[targetIdx]);
        //        DrvIC.Move(ch, "Y", Y_Target[targetIdx]);

        //        // 엑셀 조건: Step Delay 33ms
        //        Thread.Sleep(33);

        //        int hallX = DrvIC.ReadHall(ch, "X");
        //        int hallY = DrvIC.ReadHall(ch, "Y");

        //        X_TargetHall.Add(hallX);
        //        Y_TargetHall.Add(hallY);

        //        // --- 8um 오차 판정 로직 ---

        //        // 1. 목표 코드와 실제 홀 센서 코드의 차이 계산 (절댓값)
        //        int diffCodeX = Math.Abs(X_Target[targetIdx] - hallX);
        //        int diffCodeY = Math.Abs(Y_Target[targetIdx] - hallY);

        //        // 2. 코드 오차를 물리적 거리(um)로 변환 (오차 코드 * Sensitivity)
        //        double diffUmX = diffCodeX * Sensitivity_X_3200;
        //        double diffUmY = diffCodeY * Sensitivity_Y_3200;

        //        maxErrorUmX = Math.Max(maxErrorUmX,diffUmX);

        //        maxErrorUmY = Math.Max(maxErrorUmY, diffUmY);

        //        // 로그에 um 단위 오차도 함께 출력되도록 수정
        //        AddLog(ch, $"{i}\t{X_Target[targetIdx]}\t{Y_Target[targetIdx]}\t{hallX}\t{hallY}\t{diffUmX:F2}\t{diffUmY:F2}");
        //    }
        //    AddLog(ch, $"maxErrorUmX : {maxErrorUmX:F2} um)");
        //    AddLog(ch, $"maxErrorUmY : {maxErrorUmY:F2} um)");
        //    //PassFails[ch].Results[(int)SpecItem.OISX_Accuracy_Error].Val = maxErrorUmX;
        //    //PassFails[ch].Results[(int)SpecItem.OISY_Accuracy_Error].Val = maxErrorUmY;
        //    //ShowDataResults(ch, (int)SpecItem.OISX_Accuracy_Error, (int)SpecItem.OISX_Accuracy_Error, InspType.Normal, new double[] { });
        //    //ShowDataResults(ch, (int)SpecItem.OISX_Accuracy_Error, (int)SpecItem.OISX_Accuracy_Error, InspType.Normal, new double[] { });
        //}
        // 가상의 Vision 측정 함수 (실제 작성해두신 함수로 대체하세요)
        // C# 튜플(Tuple)을 사용하여 X, Y 두 개의 값을 한 번에 반환한다고 가정합니다.
        private (double X, double Y) MeasureVisionUm(int ch)
        {
            // 예: Vision 라이브러리 트리거 및 결과 산출 로직
            // double visX = VisionLib.GetX(ch);
            // double visY = VisionLib.GetY(ch);
            // return (visX, visY);
            return (0.0, 0.0); // 임시 반환값
        }

        private void OIS_Stroke_Accuracy(int port, string testItem, int InspCnt)
        {
            LEDs_All_On(0, true);

            int ch = port * 2;

            DrvIC.AFOnOff(ch, true);
            DrvIC.AFMove(ch, BestAFPos);
            AddLog(ch, $"Move AF Best Position : {BestAFPos}");
            Thread.Sleep(100);

            int centerCode = 4096;
            int rangeMinMax = 3200;
            double Sensitivity_X_3200 = 0;
            double Sensitivity_Y_3200 = 0;
            //double d3deg_Target = 367; // 목표 반경 367um
            double strokeAccuRadius = Condition.OISstrokeAccuRadius; // 반지름 Target 방식으로 변경
            double X_3deg_code = 0;
            double Y_3deg_code = 0;

            // 1. 민감도(Sensitivity) 산출
            foreach (var Cal in CalList[ch])
            {
                if (Cal.Name == "OIS X Scan")
                {
                    Sensitivity_X_3200 = Cal.CalSensitivity(Cal.CodeX, Cal.StrokeX, centerCode - rangeMinMax, centerCode + rangeMinMax,
                        Condition.XSensMinStroke, Condition.XSensMaxStroke, 0, Condition.iXDrvCodeMin, Condition.iXDrvCodeMax);
                }
                else if (Cal.Name == "OIS Y Scan")
                {
                    Sensitivity_Y_3200 = Cal.CalSensitivity(Cal.CodeY, Cal.StrokeY, centerCode - rangeMinMax, centerCode + rangeMinMax,
                        Condition.YSensMinStroke, Condition.YSensMaxStroke, 0, Condition.iYDrvCodeMin, Condition.iYDrvCodeMax);
                }
            }

            X_3deg_code = strokeAccuRadius / Sensitivity_X_3200;
            Y_3deg_code = strokeAccuRadius / Sensitivity_Y_3200;

            // 제어용 Code 리스트 & 판정용 이상적 위치(um) 리스트
            List<int> X_TargetCode = new List<int>();
            List<int> Y_TargetCode = new List<int>();
            List<double> X_TargetUm = new List<double>();
            List<double> Y_TargetUm = new List<double>();

            // 2. 목표 궤적 72스텝 사전 계산
            for (int i = 0; i < 72; i++)
            {
                double degree = i * 5;
                double radian = degree * Math.PI / 180.0;

                // [제어용] 목표 Code 계산
                X_TargetCode.Add((int)Math.Round(centerCode + X_3deg_code * Math.Cos(radian)));
                Y_TargetCode.Add((int)Math.Round(centerCode + Y_3deg_code * Math.Sin(radian)));

                // [비교용] 이상적인 물리적 위치(um) 계산 (중앙 0,0 기준)
                X_TargetUm.Add(strokeAccuRadius * Math.Cos(radian));
                Y_TargetUm.Add(strokeAccuRadius * Math.Sin(radian));
            }

            DrvIC.OISOn(ch, "X", true);
            DrvIC.OISOn(ch, "Y", true);

            DrvIC.Move(ch, "X", 4096);
            DrvIC.Move(ch, "Y", 4096);

            Thread.Sleep(50);

            AddLog(ch, $"Move X,Y Position : {4096}");

            FindResult result = Measure();
            double CenterOffsetX = result.cx[0];
            double CenterOffsetY = result.cy[0];

            AddLog(ch, $"CenterOffsetX : {CenterOffsetX:F3}, CenterOffsetY : {CenterOffsetY:F3}");

            AddLog(ch, $"Step\tTrgUm_X\tTrgUm_Y\tVis_X(um)\tVis_Y(um)\tDiff_X(um)\tDiff_Y(um)");

            double maxErrorUmX = 0.0;
            double maxErrorUmY = 0.0;
            int totalSteps = 72; // 1 Cycle

            // ==============================================================
            // 3. 실시간 구동 및 검사 루프 (Move -> Wait -> Measure -> Compare)
            // ==============================================================
            for (int i = 0; i < totalSteps; i++)
            {
                int targetIdx = i % 72;

                // [STEP 1 : 이동명령] 산출해둔 제어 코드로 OIS 이동
                DrvIC.Move(ch, "X", X_TargetCode[targetIdx]);
                DrvIC.Move(ch, "Y", Y_TargetCode[targetIdx]);

                // [STEP 2 : 렌즈 안정화] 물리적 이동 완료 대기 
                // ※ Vision 촬영 환경(노출 시간 등)에 따라 대기 시간을 조절하세요.
                Thread.Sleep(33);

                // [STEP 3 : 측정] Vision 측정 함수를 호출하여 현재 위치(um) 획득

                result = Measure();

                //var visionPos = MeasureVisionUm(ch);
                double currentVisX = result.cx[0] - CenterOffsetX;
                double currentVisY = result.cy[0] - CenterOffsetY;

                // [STEP 4 : 판정] 이상적인 목표 위치(um)와 Vision 측정 위치(um) 오차 계산
                double diffUmX = Math.Abs(X_TargetUm[targetIdx] - currentVisX);
                double diffUmY = Math.Abs(Y_TargetUm[targetIdx] - currentVisY);

                // 최대 오차 갱신
                maxErrorUmX = Math.Max(maxErrorUmX, diffUmX);
                maxErrorUmY = Math.Max(maxErrorUmY, diffUmY);

                // 로그 출력
                AddLog(ch, $"{i}\t{X_TargetUm[targetIdx]:F2}\t{Y_TargetUm[targetIdx]:F2}\t{currentVisX:F2}\t{currentVisY:F2}\t{diffUmX:F2}\t{diffUmY:F2}");
            }
            // ==============================================================

            AddLog(ch, $"Vision Based MaxErrorUmX : {maxErrorUmX:F2} um");
            AddLog(ch, $"Vision Based MaxErrorUmY : {maxErrorUmY:F2} um");

            // 최종 판정 데이터 기록
            PassFails[ch].Results[(int)SpecItem.OISX_Accuracy].Val = maxErrorUmX;
            PassFails[ch].Results[(int)SpecItem.OISY_Accuracy].Val = maxErrorUmY;
            ShowDataResults(ch, (int)SpecItem.OISX_Accuracy, (int)SpecItem.OISX_Accuracy, InspType.Normal, new double[] { });
            ShowDataResults(ch, (int)SpecItem.OISY_Accuracy, (int)SpecItem.OISY_Accuracy, InspType.Normal, new double[] { });

            LEDs_All_On(0, false);

            PowerSequence(ch);
            AddLog(ch, $"PowerSequence Off & On");
        }
        private void AF_Stroke_Accuracy(int port, string testItem, int InspCnt)
        {
            int ch = port * 2;

            //// --- 6db gain up 시퀀스 ---
            //AddLog(ch, $"6db gain up Start == ");
            //byte[] rbuf = new byte[1];
            //Dln.ReadArray(ch, DrvIC.AF_Addr, 0x10, rbuf);
            //AddLog(ch, $"Read Mem : 0x10 , Data : 0x{rbuf[0]:X2}"); // F2 -> X2 (16진수)

            //Dln.WriteArray(ch, DrvIC.AF_Addr, 0x02, new byte[] { 0x40 });
            //AddLog(ch, $"Write Mem : 0x02 , Data : 0x40");
            //Thread.Sleep(50);

            //Dln.WriteArray(ch, DrvIC.AF_Addr, 0xAE, new byte[] { 0x3B });
            //AddLog(ch, $"Write Mem : 0xAE , Data : 0x3B");
            //Thread.Sleep(50);

            //// Overflow 방지
            //byte wBuf = (byte)Math.Min(255, rbuf[0] * 2);
            //Dln.WriteArray(ch, DrvIC.AF_Addr, 0x10, new byte[] { wBuf });
            //AddLog(ch, $"Write Mem : 0x10 , Data : 0x{wBuf:X2}"); // F2 -> X2
            //Thread.Sleep(50);

            //Dln.WriteArray(ch, DrvIC.AF_Addr, 0x02, new byte[] { 0x00 });
            //AddLog(ch, $"Write Mem : 0x02 , Data : 0x00");
            //Thread.Sleep(50);

            DrvIC.AFOnOff(ch, true); 
            DrvIC.OISOn(ch, "X", true);
            DrvIC.OISOn(ch, "Y", true);

            DrvIC.Move(ch, "X", 4096);
            DrvIC.Move(ch, "Y", 4096);
            Thread.Sleep(200);
            AddLog(ch, $"OIS Move X,Y: 4096");

            // --- AF Position Sweep 설정 ---
            int delayMs = Condition.AfAccuDelayHall;       // 딜레이 (10 ms)
            int targetCycles = Condition.AfAccuCycleHall;   // 진행할 사이클 횟수

            // Full Stroke 계산 (End Position - Start Position)
            //double fullStroke = endPos - startPos; // 3600 - 400 = 3200

            List<int> target = new List<int>();
            List<int> hall = new List<int>();

            // Sensitivity 값 가져오기
            double AF_Sensitivity = PassFails[ch].Results[(int)SpecItem.AF_Sensitivity].Val;
            if (IsVirtual) AF_Sensitivity = 0.146;
            // Sensitivity 가 0이면 0으로 나누기 오류가 발생하므로 예외 처리
            if (AF_Sensitivity == 0)
            {
                AddLog(ch, "Error: AF_Sensitivity is 0. Cannot calculate accuracy.");
                return; // 혹은 에러 처리 로직
            }


            double maxErrorUm = 0;
            int cnt = 0;
            //int center = 2048;
            int minRangeCode = 0;// center - (int)(Condition.AfAccuRange / AF_Sensitivity);
            int maxRangeCode = (int)(Condition.AfAccuRange / AF_Sensitivity);
            int startPos = minRangeCode;     // 시작 위치 (Target Position)
            int endPos = maxRangeCode;      // 끝 위치

            int step = (int)(maxRangeCode / Condition.AfAccuStep);         // 이동 스텝 (Stroke / Step)

            AddLog(ch, $"AF Sweep Start : {startPos} -> {endPos} -> {startPos}, Step: {step}, Delay: {delayMs}ms, Cycles: {targetCycles}");

            AddLog(ch, $"cycle\tAF_Target\tAF_Hall\tError");


            // 지정된 사이클 횟수만큼 반복
            for (int cycle = 1; cycle <= targetCycles; cycle++)
            {
                // 1. Forward Sweep: 400 -> 3600
                for (int pos = startPos; pos <= endPos; pos += step)
                {
                    target.Add(pos);
                    DrvIC.AFMove(ch, pos);
                    if (pos == startPos)
                    {
                        Thread.Sleep(200);
                        AddLog(ch, $"First Dley Add : 200ms");
                    }

                    int currentDelayMs = GetAfAccuracyDelay(pos);
                    Thread.Sleep(currentDelayMs);

                    hall.Add(DrvIC.ReadAFHall(ch));

                    double error = Math.Abs(hall[cnt] - target[cnt]) * AF_Sensitivity;

                    if (target[cnt] >= minRangeCode && target[cnt] < maxRangeCode)
                    {
                        maxErrorUm = Math.Max(maxErrorUm, error);
                        AddLog(
                            ch,
                            $"{cycle}\t{target[cnt]}\t{hall[cnt]}\t{error:F2}\tDelay:{currentDelayMs}ms");
                    }
                    cnt++;
                }

                // 2. Backward Sweep: 3500 -> 400 
                for (int pos = endPos - step; pos >= startPos; pos -= step)
                {
                    target.Add(pos);
                    DrvIC.AFMove(ch, pos);

                    int currentDelayMs = GetAfAccuracyDelay(pos);
                    Thread.Sleep(currentDelayMs);

                    hall.Add(DrvIC.ReadAFHall(ch));
                    double error = Math.Abs(hall[cnt] - target[cnt]) * AF_Sensitivity;
                    if (target[cnt] > 0 && target[cnt] < maxRangeCode)
                    {
                        maxErrorUm = Math.Max(maxErrorUm, error);
                    }
                    AddLog(
                        ch,
                        $"{cycle}\t{target[cnt]}\t{hall[cnt]}\t{error:F2}\tDelay:{currentDelayMs}ms");
                    cnt++;
                }
                AddLog(ch, $"--- Cycle {cycle} End ---");
            }
            AddLog(ch, $"AF Sweep All Cycles Completed.");


            AddLog(ch, $"maxErrorUmX : {maxErrorUm:F2} um)");
            PassFails[ch].Results[(int)SpecItem.AF_Accuracy].Val = maxErrorUm;
            ShowDataResults(ch, (int)SpecItem.AF_Accuracy, (int)SpecItem.AF_Accuracy, InspType.Normal, new double[] { });

            PowerSequence(ch);
            AddLog(ch, $"PowerSequence Off & On");
        }
        private int GetAfAccuracyDelay(int afCode)
        {
            if (afCode < Condition.AfAccuCodeRange1)
            {
                return Condition.AfAccuDelayHall;
            }
            else if (afCode < Condition.AfAccuCodeRange2)
            {
                return Condition.AfAccuDelayHall2;
            }
            else
            {
                return Condition.AfAccuDelayHall3;
            }
        }
        private void OIS_Accuracy_Hall_Inf(int port, string testItem, int InspCnt)
        {
            int ch = port * 2;
            DrvIC.AFOnOff(ch, true);
            if (testItem.Contains("Mac"))
            {
                DrvIC.AFMove(ch, 3465);
                AddLog(ch, $"Move AF Best Position : {3465}");
                Thread.Sleep(100);
                AddLog(ch, $"AF Read Hall : {DrvIC.ReadAFHall(ch)}");
            }
            else
            {
                DrvIC.AFMove(ch, BestAFPos);
                AddLog(ch, $"Move AF Best Position : {BestAFPos}");
                Thread.Sleep(100);
                AddLog(ch, $"AF Read Hall : {DrvIC.ReadAFHall(ch)}");
            }
            //AddLog(ch, $"6db gain up Start == ");
            //byte[] rbuf = new byte[1];
            //Dln.ReadArray(ch, DrvIC.XSlaveAddr, 0x10, rbuf);
            //AddLog(ch, $"Read X Mem : 0x10 , Data : 0x{rbuf[0]:X2}"); // F2 -> X2 (16진수)

            //Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x02, new byte[] { 0x40 });
            //AddLog(ch, $"Write X Mem : 0x02 , Data : 0x40");
            //Thread.Sleep(50);

            //Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0xAE, new byte[] { 0x3B });
            //AddLog(ch, $"Write X Mem : 0xAE , Data : 0x3B");
            //Thread.Sleep(50);

            //// Overflow 방지
            //byte wBuf = (byte)Math.Min(255, rbuf[0] * 2);
            //Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x10, new byte[] { wBuf });
            //AddLog(ch, $"Write X Mem : 0x10 , Data : 0x{wBuf:X2}"); // F2 -> X2
            //Thread.Sleep(50);

            //Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x02, new byte[] { 0x00 });
            //AddLog(ch, $"Write X Mem : 0x02 , Data : 0x00");
            //Thread.Sleep(50);

            //rbuf = new byte[1];
            //Dln.ReadArray(ch, DrvIC.Y1SlaveAddr, 0x10, rbuf);
            //AddLog(ch, $"Read Y Mem : 0x10 , Data : 0x{rbuf[0]:X2}"); // F2 -> X2 (16진수)

            //Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x02, new byte[] { 0x40 });
            //AddLog(ch, $"Write Y Mem : 0x02 , Data : 0x40");
            //Thread.Sleep(50);

            //Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0xAE, new byte[] { 0x3B });
            //AddLog(ch, $"Write Y Mem : 0xAE , Data : 0x3B");
            //Thread.Sleep(50);

            //// Overflow 방지
            //wBuf = (byte)Math.Min(255, rbuf[0] * 2);
            //Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x10, new byte[] { wBuf });
            //AddLog(ch, $"Write Y Mem : 0x10 , Data : 0x{wBuf:X2}"); // F2 -> X2
            //Thread.Sleep(50);

            //Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x02, new byte[] { 0x00 });
            //AddLog(ch, $"Write Y Mem : 0x02 , Data : 0x00");
            //Thread.Sleep(50);

            DrvIC.OISOn(ch, "X", true);
            DrvIC.OISOn(ch, "Y", true);

            List<int> X_Target = new List<int>();
            List<int> Y_Target = new List<int>();
            List<int> X_TargetHall = new List<int>();
            List<int> Y_TargetHall = new List<int>();

            double SensitivityRef = 0.1147;
            int center = 4096;
            int minCode = 0;
            int maxCode = 8191;
            int stepDeg = 5;
            int radiusCode = (int)(Condition.OISAccuRadius / SensitivityRef);

            int minRangeCode = center - (int)(Condition.OISAccuRange / SensitivityRef);
            int maxRangeCode = (int)(Condition.OISAccuRange / SensitivityRef) + center;

            AddLog(ch, $"Target Radius Stroke : {Condition.OISAccuRadius} um, Taerget Code : {radiusCode}");

            for (int deg = 0; deg < 360; deg += stepDeg)
            {
                double rad = deg * Math.PI / 180.0;

                int x = (int)Math.Round(center + radiusCode * Math.Cos(rad));
                int y = (int)Math.Round(center + radiusCode * Math.Sin(rad));

                x = Math.Max(minCode, Math.Min(maxCode, x));
                y = Math.Max(minCode, Math.Min(maxCode, y));

                X_Target.Add(x);
                Y_Target.Add(y);
            }

            AddLog(ch, $"Step\tX_Target\tY_Target\tX_Hall\tY_Hall\tDiff_X(um)\tDiff_Y(um)");


            double maxErrorUmX = 0;
            double maxErrorUmY = 0;
            int delayMs = Condition.OISAccuDelayHall;       // 딜레이 (10 ms)
            int targetCycles = Condition.OISAccuCycleHall;   // 진행할 사이클 횟수

            int totalSteps = X_Target.Count * targetCycles;

            for (int i = 0; i < totalSteps; i++)
            {
                // 72 이상 넘어가면 다시 0부터 타겟을 가져오도록 나머지 연산(%) 사용
                int targetIdx = i % X_Target.Count;

                DrvIC.Move(ch, "X", X_Target[targetIdx]);
                DrvIC.Move(ch, "Y", Y_Target[targetIdx]);
                if (i == 0)
                {
                    Thread.Sleep(200);
                    AddLog(ch, $"First Dley Add : 200ms");
                }
                Thread.Sleep(delayMs);

                int hallX = DrvIC.ReadHall(ch, "X");
                int hallY = DrvIC.ReadHall(ch, "Y");

                X_TargetHall.Add(hallX);
                Y_TargetHall.Add(hallY);

                // --- 8um 오차 판정 로직 ---

                // 1. 목표 코드와 실제 홀 센서 코드의 차이 계산 (절댓값)
                int diffCodeX = Math.Abs(X_Target[targetIdx] - hallX);
                int diffCodeY = Math.Abs(Y_Target[targetIdx] - hallY);

                // 2. 코드 오차를 물리적 거리(um)로 변환 (오차 코드 * Sensitivity)
                double diffUmX = diffCodeX * SensitivityRef;
                double diffUmY = diffCodeY * SensitivityRef;

                if (X_Target[targetIdx] > minRangeCode && X_Target[targetIdx] < maxRangeCode && Y_Target[targetIdx] > minRangeCode && Y_Target[targetIdx] < maxRangeCode)
                {
                    maxErrorUmX = Math.Max(maxErrorUmX, diffUmX);

                    maxErrorUmY = Math.Max(maxErrorUmY, diffUmY);

                    // 로그에 um 단위 오차도 함께 출력되도록 수정
                    AddLog(ch, $"{i}\t{X_Target[targetIdx]}\t{Y_Target[targetIdx]}\t{hallX}\t{hallY}\t{diffUmX:F2}\t{diffUmY:F2}");
                }
            }
            AddLog(ch, $"maxErrorUmX : {maxErrorUmX:F2} um)");
            AddLog(ch, $"maxErrorUmY : {maxErrorUmY:F2} um)");

            if (testItem.Contains("Mac"))
            {
                PassFails[ch].Results[(int)SpecItem.OISX_AccuracyMac].Val = maxErrorUmX;
                PassFails[ch].Results[(int)SpecItem.OISY_AccuracyMac].Val = maxErrorUmY;
                ShowDataResults(ch, (int)SpecItem.OISX_AccuracyMac, (int)SpecItem.OISX_AccuracyMac, InspType.Normal, new double[] { });
                ShowDataResults(ch, (int)SpecItem.OISY_AccuracyMac, (int)SpecItem.OISY_AccuracyMac, InspType.Normal, new double[] { });
            }
            else
            {
                PassFails[ch].Results[(int)SpecItem.OISX_AccuracyInf].Val = maxErrorUmX;
                PassFails[ch].Results[(int)SpecItem.OISY_AccuracyInf].Val = maxErrorUmY;
                ShowDataResults(ch, (int)SpecItem.OISX_AccuracyInf, (int)SpecItem.OISX_AccuracyInf, InspType.Normal, new double[] { });
                ShowDataResults(ch, (int)SpecItem.OISY_AccuracyInf, (int)SpecItem.OISY_AccuracyInf, InspType.Normal, new double[] { });
            }

            PowerSequence(ch);
            AddLog(ch, $"PowerSequence Off & On");
        }
        private void AF_OpenMode_Area(int port, string testItem, int InspCnt)
        {
            int ch = port * 2;

            short stepSize = (short)Condition.AFOMstep;
            short stepDelay = (short)Condition.AFOMdelay;
            short startDAC = (short)Condition.AFOMstartDAC;
            short endDAC = (short) Condition.AFOMEndDAC;

            if (stepDelay < 0)
                stepDelay = 0;

            // 원본 C++은 0~511 DAC range를 sweep 한다.
            int dacMin = 0;
            int dacMax = 511;

            int start = Math.Max(dacMin, Math.Min(dacMax, startDAC));
            int end = Math.Max(dacMin, Math.Min(dacMax, endDAC));

            float totalArea = 0.0f;

            byte[] icDataBackup = new byte[10];

            int maxIndex = dacMax / stepSize;

            ushort[] risingPos = new ushort[maxIndex + 1];
            ushort[] fallingPos = new ushort[maxIndex + 1];

            try
            {
                // 원본: AK7316_move_lens(AK7316_MID_CODE, 100);
                DrvIC.AFMove(ch, Condition.AFOMmin);
                AddLog(ch, string.Format("Move AF Min Code {0}", Condition.AFOMmin));
                Wait(100);

                AddLog(ch, string.Format(
                    "--- Open test start (Range: {0}~{1}) ---",
                    start,
                    end));

                AK7316_OpenSequence(ch, true, icDataBackup);

                // 원본: AK7316_Write_2byte(0x00, (UInt16)(256 << 7));
                Dln.Write2Byte(ch, DrvIC.AF_Addr, 0x00, 1, (ushort)(256 << 7));
                Wait(100);

                // Rising sweep: 0 -> 511
                for (int testCurrent = 0; testCurrent <= dacMax; testCurrent += stepSize)
                {
                    Dln.Write2Byte(ch, DrvIC.AF_Addr, 0x00, 1, (ushort)(testCurrent << 7));
                    Wait(stepDelay);

                    ushort afCurCode = (ushort)DrvIC.ReadAFHall(ch);

                    int idx = testCurrent / stepSize;
                    if (idx >= 0 && idx < risingPos.Length)
                        risingPos[idx] = afCurCode;

                    // 필요하면 디버그 로그 활성화
                    AddLog(ch, string.Format("Current : {0} Hall : {1}", testCurrent, afCurCode));
                }

                // Falling sweep: nearest step <= 511 -> 0
                int fallingStart = (dacMax / stepSize) * stepSize;

                for (int testCurrent = fallingStart; testCurrent >= 0; testCurrent -= stepSize)
                {
                    Dln.Write2Byte(ch, DrvIC.AF_Addr, 0x00, 1, (ushort)(testCurrent << 7));
                    Wait(stepDelay);

                    ushort afCurCode = (ushort)DrvIC.ReadAFHall(ch);

                    int idx = testCurrent / stepSize;
                    if (idx >= 0 && idx < fallingPos.Length)
                        fallingPos[idx] = afCurCode;

                    // 필요하면 디버그 로그 활성화
                    AddLog(ch, string.Format("Current : {0} Hall : {1}", testCurrent, afCurCode));
                }

                int startIdx = start / stepSize;
                int endIdx = end / stepSize;

                if (startIdx < 0) startIdx = 0;
                if (endIdx > maxIndex) endIdx = maxIndex;

                for (int i = startIdx; i < endIdx; i++)
                {
                    float h1 = fallingPos[i] - risingPos[i];
                    float h2 = fallingPos[i + 1] - risingPos[i + 1];

                    totalArea += (h1 + h2) * stepSize / 2.0f;
                }

                // 원본 코드와 동일: /= 1000.0
                totalArea /= 1000.0f;

                AddLog(ch, "--- Calculation Result ---");
                AddLog(ch, string.Format("StepSize: {0}, StepDelay: {1}", stepSize, stepDelay));
                AddLog(ch, string.Format("Target Range: DAC {0} to {1}", start, end));
                AddLog(ch, string.Format("DC Area(/=1000): {0:0.0}", totalArea));
                AddLog(ch, "--------------------------");
            }
            catch
            {
                AddLog(ch, "AK7316 OpenMode close sequence failed.");
            }
        }
        private void AK7316_OpenSequence(int ch, bool isOn, byte[] backupData)
        {
            if (backupData == null || backupData.Length == 0)
                throw new ArgumentException("backupData must have at least 1 byte.", nameof(backupData));

            if (isOn)
            {
                AddLog(ch, "AK7316 open sequence start");

                Dln.WriteByte(ch, DrvIC.AF_Addr, 0x02, 1, 0x40);
                Wait(10);

                backupData[0] = Dln.ReadByte(ch, DrvIC.AF_Addr, 0x0B, 1);

                AddLog(ch, string.Format("Reg.0B = {0:X2}", backupData[0]));

                Dln.WriteByte(ch, DrvIC.AF_Addr, 0xAE, 1, 0x3B);                         // Setting mode
                Dln.WriteByte(ch, DrvIC.AF_Addr, 0x1A, 1, 0x00);
                Dln.WriteByte(ch, DrvIC.AF_Addr, 0x0B, 1, (byte)(backupData[0] & 0x7F));  // disable epa
                Dln.WriteByte(ch, DrvIC.AF_Addr, 0xA6, 1, 0x7B);                         // DAC mode on
                Dln.WriteByte(ch, DrvIC.AF_Addr, 0x02, 1, 0x00);
            }
            else
            {
                AddLog(ch, "AK7316 open sequence finish");

                Dln.WriteByte(ch, DrvIC.AF_Addr, 0x02, 1, 0x40);
                Wait(10);

                Dln.WriteByte(ch, DrvIC.AF_Addr, 0x0B, 1, backupData[0]);    // reload epa option
                Dln.WriteByte(ch, DrvIC.AF_Addr, 0xA6, 1, 0x00);             // DAC mode off
                Dln.WriteByte(ch, DrvIC.AF_Addr, 0xAE, 1, 0x00);             // exit Setting mode
                Dln.WriteByte(ch, DrvIC.AF_Addr, 0x02, 1, 0x00);
            }
        }
        private void OIS_square_Test(int port, string testItem, int InspCnt)
        {
            int ch = port * 2;

            try
            {
                DrvIC.AFOnOff(ch, true);

                DrvIC.AFMove(ch, Condition.OISSquareAFPos);
                AddLog(ch, $"Move AF Move 0 Position : {Condition.OISSquareAFPos}");
                Thread.Sleep(100);
                AddLog(ch, $"AF Read Hall : {DrvIC.ReadAFHall(ch)}");

                DrvIC.OISReset(ch);
                AddLog(ch, $"OIS Reset==");

                DrvIC.OISOn(ch, "X", true);
                DrvIC.OISOn(ch, "Y", true);

                double sensitivityRef = 0.1147; // um/code

                int centerCode = 4096;
                int minCode = 0;
                int maxCode = 8191;

                int radiusCode =
                    (int)(Condition.OISSquareRadius / sensitivityRef);

                int minTargetCode = centerCode - radiusCode;
                int maxTargetCode = centerCode + radiusCode;

                minTargetCode =
                    Math.Max(minCode, Math.Min(maxCode, minTargetCode));

                maxTargetCode =
                    Math.Max(minCode, Math.Min(maxCode, maxTargetCode));

                int minRangeCode =
                    centerCode -
                    (int)(Condition.OISSquareRange / sensitivityRef);

                int maxRangeCode =
                    centerCode +
                    (int)(Condition.OISSquareRange / sensitivityRef);

                AddLog(ch, $"Square Corner Hold Test Start");

                AddLog(ch,
                    $"Square Target Stroke : {Condition.OISSquareRadius} um, " +
                    $"Target Code Radius : {radiusCode}");

                AddLog(ch,
                    $"Square Min Code : {minTargetCode}, " +
                    $"Max Code : {maxTargetCode}");

                AddLog(ch,
                    $"Square Range Code : {minRangeCode} ~ {maxRangeCode}");

                List<SquareCornerTarget> corners =
                    new List<SquareCornerTarget>
                    {
                new SquareCornerTarget(
                    "LeftBottom",
                    minTargetCode,
                    minTargetCode),

                new SquareCornerTarget(
                    "RightBottom",
                    maxTargetCode,
                    minTargetCode),

                new SquareCornerTarget(
                    "RightTop",
                    maxTargetCode,
                    maxTargetCode),

                new SquareCornerTarget(
                    "LeftTop",
                    minTargetCode,
                    maxTargetCode)
                    };

                int settleDelayMs = 200;
                int sampleIntervalMs = Condition.OISSquareDelay;
                int holdTimeMs = Condition.OISSquareCycle;

                if (sampleIntervalMs <= 0)
                    sampleIntervalMs = 1;

                if (holdTimeMs <= 0)
                    holdTimeMs = sampleIntervalMs;

                double maxJitterUmX = 0.0;
                double maxJitterUmY = 0.0;

                double maxStdUmX = 0.0;
                double maxStdUmY = 0.0;

                double maxAvgErrorUmX = 0.0;
                double maxAvgErrorUmY = 0.0;

                string worstJitterCornerX = "";
                string worstJitterCornerY = "";

                int measuredCornerCount = 0;

                AddLog(ch,
                    $"Corner\tTargetX\tTargetY\t" +
                    $"JitterX(um)\tJitterY(um)");

                foreach (SquareCornerTarget corner in corners)
                {
                    bool targetInRange =
                        corner.X >= minRangeCode &&
                        corner.X <= maxRangeCode &&
                        corner.Y >= minRangeCode &&
                        corner.Y <= maxRangeCode;

                    if (!targetInRange)
                    {
                        AddLog(ch,
                            $"{corner.Name} Skip. Target out of range. " +
                            $"X:{corner.X}, Y:{corner.Y}");

                        continue;
                    }
                    DrvIC.Move(ch, "X", centerCode);
                    DrvIC.Move(ch, "Y", centerCode);
                    AddLog(ch, $"Move X,Y : {4096}");

                    Thread.Sleep(100);

                    DrvIC.Move(ch, "X", corner.X);
                    DrvIC.Move(ch, "Y", corner.Y);

                    Thread.Sleep(settleDelayMs);

                    List<int> hallXs = new List<int>();
                    List<int> hallYs = new List<int>();

                    DateTime startTime = DateTime.Now;

                    while ((DateTime.Now - startTime).TotalMilliseconds < holdTimeMs)
                    {
                        int hallX = DrvIC.ReadHall(ch, "X");
                        int hallY = DrvIC.ReadHall(ch, "Y");

                        hallXs.Add(hallX);
                        hallYs.Add(hallY);

                        Thread.Sleep(sampleIntervalMs);
                    }

                    if (hallXs.Count == 0 || hallYs.Count == 0)
                    {
                        AddLog(ch,
                            $"{corner.Name} measurement failed. No Hall samples.");

                        continue;
                    }

                    SquareCornerResult result =
                        CalcSquareCornerResult(
                            corner,
                            hallXs,
                            hallYs,
                            sensitivityRef);

                    measuredCornerCount++;

                    if (result.JitterUmX > maxJitterUmX)
                    {
                        maxJitterUmX = result.JitterUmX;
                        worstJitterCornerX = corner.Name;
                    }

                    if (result.JitterUmY > maxJitterUmY)
                    {
                        maxJitterUmY = result.JitterUmY;
                        worstJitterCornerY = corner.Name;
                    }

                    maxStdUmX =
                        Math.Max(maxStdUmX, result.StdUmX);

                    maxStdUmY =
                        Math.Max(maxStdUmY, result.StdUmY);

                    maxAvgErrorUmX =
                        Math.Max(
                            maxAvgErrorUmX,
                            result.AvgErrorUmX);

                    maxAvgErrorUmY =
                        Math.Max(
                            maxAvgErrorUmY,
                            result.AvgErrorUmY);

                    AddLog(ch,
                        $"{corner.Name}\t" +
                        $"{corner.X}\t" +
                        $"{corner.Y}\t" +
                        $"{result.JitterUmX:F3}\t" +
                        $"{result.JitterUmY:F3}");
                }

                if (measuredCornerCount != corners.Count)
                {
                    AddLog(ch,
                        $"Square Test NG : " +
                        $"Measured Corner Count = {measuredCornerCount}/{corners.Count}");

                    maxJitterUmX = double.NaN;
                    maxJitterUmY = double.NaN;
                }

                AddLog(ch,
                    $"Square Corner Max Jitter X : " +
                    $"{maxJitterUmX:F3} um, " +
                    $"Worst Corner : {worstJitterCornerX}");

                AddLog(ch,
                    $"Square Corner Max Jitter Y : " +
                    $"{maxJitterUmY:F3} um, " +
                    $"Worst Corner : {worstJitterCornerY}");

                AddLog(ch,
                    $"Square Corner Max Std X : " +
                    $"{maxStdUmX:F3} um");

                AddLog(ch,
                    $"Square Corner Max Std Y : " +
                    $"{maxStdUmY:F3} um");

                AddLog(ch,
                    $"Square Corner Max Avg Error X : " +
                    $"{maxAvgErrorUmX:F3} um");

                AddLog(ch,
                    $"Square Corner Max Avg Error Y : " +
                    $"{maxAvgErrorUmY:F3} um");

                // 4개 꼭지점 중 최대 Jitter를 최종 결과로 사용
                PassFails[ch]
                    .Results[(int)SpecItem.OISX_SquareTest]
                    .Val = maxJitterUmX;

                PassFails[ch]
                    .Results[(int)SpecItem.OISY_SquareTest]
                    .Val = maxJitterUmY;

                // X/Y 각각 독립 판정
                ShowDataResults(
                    ch,
                    (int)SpecItem.OISX_SquareTest,
                    (int)SpecItem.OISX_SquareTest,
                    InspType.Normal,
                    new double[] { });

                ShowDataResults(
                    ch,
                    (int)SpecItem.OISY_SquareTest,
                    (int)SpecItem.OISY_SquareTest,
                    InspType.Normal,
                    new double[] { });

                PowerSequence(ch);
                AddLog(ch, $"PowerSequence Off & On");
            }
            catch (Exception ex)
            {
                AddLog(ch,
                    $"OIS_square_Test Error : {ex.Message}");
            }
        }
        private void OIS_Shaking_Test(int port, string testItem, int InspCnt)
        {
            int ch = port * 2;

            try
            {
                DrvIC.AFOnOff(ch, true);

                DrvIC.AFMove(ch, Condition.OISShakingAFPos);
                AddLog(ch, $"Move AF Position : {Condition.OISShakingAFPos}");

                Thread.Sleep(100);

                AddLog(ch, $"AF Read Hall : {DrvIC.ReadAFHall(ch)}");

                DrvIC.OISOn(ch, "X", true);
                DrvIC.OISOn(ch, "Y", true);

                double sensitivityRef = 0.1147; // um/code

                int targetX = Condition.OISShakingXPos;
                int targetY = Condition.OISShakingYPos;

                int measureTimeMs = Condition.OISShakingCycle;
                int sampleIntervalMs = Condition.OISShakingDelay;
                int settleDelayMs = 200;

                if (measureTimeMs <= 0)
                    measureTimeMs = 5000;

                if (sampleIntervalMs <= 0)
                    sampleIntervalMs = 1;

                AddLog(ch, $"OIS Servo On Shaking Test Start");
                AddLog(ch, $"Target Position X : {targetX}, Y : {targetY}");
                AddLog(ch, $"Measure Time : {measureTimeMs} ms");
                AddLog(ch, $"Sample Interval : {sampleIntervalMs} ms");

                // 목표 위치로 이동
                DrvIC.Move(ch, "X", targetX);
                DrvIC.Move(ch, "Y", targetY);

                // 이동 후 안정화
                Thread.Sleep(settleDelayMs);

                List<int> hallXs = new List<int>();
                List<int> hallYs = new List<int>();

                double maxErrorUmX = 0.0;
                double maxErrorUmY = 0.0;

                int maxErrorHallX = 0;
                int maxErrorHallY = 0;

                int maxErrorSampleX = -1;
                int maxErrorSampleY = -1;

                DateTime startTime = DateTime.Now;
                int sampleNo = 0;

                AddLog(ch,
                    $"Sample\tElapsed(ms)\t" +
                    $"TargetX\tTargetY\t" +
                    $"HallX\tHallY\t" +
                    $"ErrorX(um)\tErrorY(um)");

                while ((DateTime.Now - startTime).TotalMilliseconds < measureTimeMs)
                {
                    int hallX = DrvIC.ReadHall(ch, "X");
                    int hallY = DrvIC.ReadHall(ch, "Y");

                    hallXs.Add(hallX);
                    hallYs.Add(hallY);

                    int errorHallX = Math.Abs(hallX - targetX);
                    int errorHallY = Math.Abs(hallY - targetY);

                    double errorUmX = errorHallX * sensitivityRef;
                    double errorUmY = errorHallY * sensitivityRef;

                    if (errorUmX > maxErrorUmX)
                    {
                        maxErrorUmX = errorUmX;
                        maxErrorHallX = errorHallX;
                        maxErrorSampleX = sampleNo;
                    }

                    if (errorUmY > maxErrorUmY)
                    {
                        maxErrorUmY = errorUmY;
                        maxErrorHallY = errorHallY;
                        maxErrorSampleY = sampleNo;
                    }

                    AddLog(ch,
                        $"{sampleNo}\t" +
                        $"{(DateTime.Now - startTime).TotalMilliseconds:F0}\t" +
                        $"{targetX}\t{targetY}\t" +
                        $"{hallX}\t{hallY}\t" +
                        $"{errorUmX:F3}\t{errorUmY:F3}");

                    sampleNo++;

                    Thread.Sleep(sampleIntervalMs);
                }

                if (hallXs.Count == 0 || hallYs.Count == 0)
                {
                    AddLog(ch, $"OIS Shaking Test NG : No Hall samples.");

                    maxErrorUmX = double.NaN;
                    maxErrorUmY = double.NaN;
                }
                else
                {
                    double avgHallX = hallXs.Average();
                    double avgHallY = hallYs.Average();

                    int minHallX = hallXs.Min();
                    int maxHallX = hallXs.Max();

                    int minHallY = hallYs.Min();
                    int maxHallY = hallYs.Max();

                    double jitterUmX =
                        (maxHallX - minHallX) * sensitivityRef;

                    double jitterUmY =
                        (maxHallY - minHallY) * sensitivityRef;

                    double avgErrorUmX =
                        Math.Abs(avgHallX - targetX) * sensitivityRef;

                    double avgErrorUmY =
                        Math.Abs(avgHallY - targetY) * sensitivityRef;

                    AddLog(ch, $"OIS Shaking Sample Count : {hallXs.Count}");

                    //AddLog(ch,
                    //    $"OIS Shaking Hall X - " +
                    //    $"Avg:{avgHallX:F2}, Min:{minHallX}, Max:{maxHallX}");

                    //AddLog(ch,
                    //    $"OIS Shaking Hall Y - " +
                    //    $"Avg:{avgHallY:F2}, Min:{minHallY}, Max:{maxHallY}");

                    //AddLog(ch,
                    //    $"OIS Shaking Jitter X : {jitterUmX:F3} um");

                    //AddLog(ch,
                    //    $"OIS Shaking Jitter Y : {jitterUmY:F3} um");

                    //AddLog(ch,
                    //    $"OIS Shaking Average Error X : {avgErrorUmX:F3} um");

                    //AddLog(ch,
                    //    $"OIS Shaking Average Error Y : {avgErrorUmY:F3} um");
                }

                AddLog(ch,
                    $"OIS Shaking Max Error X : {maxErrorUmX:F3} um, " +
                    $"Code Difference : {maxErrorHallX}, " +
                    $"Sample : {maxErrorSampleX}");

                AddLog(ch,
                    $"OIS Shaking Max Error Y : {maxErrorUmY:F3} um, " +
                    $"Code Difference : {maxErrorHallY}, " +
                    $"Sample : {maxErrorSampleY}");

                // 목표 위치와 실제 위치의 최대 차이를 최종 결과로 저장
                PassFails[ch]
                    .Results[(int)SpecItem.OISX_ShakingTest]
                    .Val = maxErrorUmX;

                PassFails[ch]
                    .Results[(int)SpecItem.OISY_ShakingTest]
                    .Val = maxErrorUmY;

                // 규격이 ≤ 3 um이므로 OnlyMax 판정
                ShowDataResults(
                    ch,
                    (int)SpecItem.OISX_ShakingTest,
                    (int)SpecItem.OISX_ShakingTest,
                    InspType.Normal,
                    new double[] { });

                ShowDataResults(
                    ch,
                    (int)SpecItem.OISY_ShakingTest,
                    (int)SpecItem.OISY_ShakingTest,
                    InspType.Normal,
                    new double[] { });

                PowerSequence(ch);

                AddLog(ch, $"PowerSequence Off & On");
            }
            catch (Exception ex)
            {
                AddLog(ch, $"OIS_Shaking_Test Error : {ex.Message}");
            }
        }
        private class SquareCornerTarget
        {
            public string Name { get; private set; }
            public int X { get; private set; }
            public int Y { get; private set; }

            public SquareCornerTarget(string name, int x, int y)
            {
                Name = name;
                X = x;
                Y = y;
            }
        }

        private class SquareCornerResult
        {
            public double AvgX;
            public double AvgY;

            public int MinX;
            public int MaxX;
            public int MinY;
            public int MaxY;

            public double JitterUmX;
            public double JitterUmY;

            public double StdUmX;
            public double StdUmY;

            public double AvgErrorUmX;
            public double AvgErrorUmY;
        }

        private SquareCornerResult CalcSquareCornerResult(
            SquareCornerTarget target,
            List<int> hallXs,
            List<int> hallYs,
            double sensitivity)
        {
            SquareCornerResult result = new SquareCornerResult();

            if (hallXs == null || hallYs == null || hallXs.Count == 0 || hallYs.Count == 0)
                return result;

            result.MinX = hallXs.Min();
            result.MaxX = hallXs.Max();
            result.MinY = hallYs.Min();
            result.MaxY = hallYs.Max();

            result.AvgX = hallXs.Average();
            result.AvgY = hallYs.Average();

            result.JitterUmX = (result.MaxX - result.MinX) * sensitivity;
            result.JitterUmY = (result.MaxY - result.MinY) * sensitivity;

            result.StdUmX = CalcStdDev(hallXs) * sensitivity;
            result.StdUmY = CalcStdDev(hallYs) * sensitivity;

            result.AvgErrorUmX = Math.Abs(target.X - result.AvgX) * sensitivity;
            result.AvgErrorUmY = Math.Abs(target.Y - result.AvgY) * sensitivity;

            return result;
        }

        private double CalcStdDev(List<int> values)
        {
            if (values == null || values.Count <= 1)
                return 0;

            double avg = values.Average();
            double sum = 0;

            for (int i = 0; i < values.Count; i++)
            {
                double diff = values[i] - avg;
                sum += diff * diff;
            }

            return Math.Sqrt(sum / values.Count);
        }
        private void PeakCurrent(int port, string testItem, int InspCnt)
        {
            int ch = port * 2;

            PeakCurrentFactory _factory = new PeakCurrentFactory();
            IPeakCurrentExecutor executor;

            PeakCurrentConfigration config = new PeakCurrentConfigration()
            {
                AFFunction = DrvIC,
                OIS_Function = DrvIC,
                FRA_Function = DrvIC,
                Dln = DrvIC,
                GPIOOutput = Dln,
                LogView = this,
            };

            var _param = _factory.CreatePeakCurrentParams(testItem);
            _param.Channel = ch;

            executor = _factory.CreatePeakCurrentLogic(_param, config);
            executor.SetPeakCurrent(_param);

            var result = executor.Execute();
        }

        void oisOL(int ch, int axis, string testItem)
        {
            string axisName = axis == 0 ? "X" : "Y";
            int addr = axis == 0 ? DrvIC.XSlaveAddr : DrvIC.Y1SlaveAddr;
            int test_time = Condition.OISOLMoveDelay; 

            int open_data = 0; 
            int open_input;
            ushort open_output;
            int t_count;
            int[] start_pos = new int[2] { 0, 0 };
            int[] end_pos = new int[2] { 512, 512 };
            int[] square = new int[500];
            uint sum_square = 0;
            int[] Ya = new int[500]; int[] Yb = new int[500]; int[] height = new int[500];
            int dc_count_rising, dc_count_falling = 0, dc_count;
            short[,] dc_value = new short[2, 200];
            uint square_spec = (uint)Condition.OISOLSpec;
            byte dc_result = 0;

            DrvIC.AFOnOff(ch, true);
            DrvIC.AFMove(ch, BestAFPos);
            AddLog(ch, $"AF Best Pos Move = {BestAFPos}");

            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x02, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x02, new byte[] { 0x00 });
            int xPos = 2048;
            int yPos = 2048;
            int tp1 = Condition.OISOLtp1;
            int tp2 = Condition.OISOLtp2;
            if (testItem.Contains("1"))
            {
                xPos = Condition.OISOLXPos1;
                yPos = Condition.OISOLYPos1;
                tp1 = Condition.OISOLtp1_1;
                tp2 = Condition.OISOLtp2_1;
            }
            else if (testItem.Contains("2"))
            {
                xPos = Condition.OISOLXPos2;
                yPos = Condition.OISOLYPos2;
                tp1 = Condition.OISOLtp1_2;
                tp2 = Condition.OISOLtp2_2;
            }

            int test_size = (tp2 - tp1) / Condition.OISOLStepNum;

            if (test_size <= 0)
            {
                AddLog(ch, $"Invalid OL step size. tp1={tp1}, tp2={tp2}, StepNum={Condition.OISOLStepNum}");
                return;
            }

            DrvIC.Move(ch, "X", xPos);
            DrvIC.Move(ch, "Y", yPos);
            AddLog(ch, $"OIS X Pos Move = {xPos}");
            AddLog(ch, $"OIS Y Pos Move = {yPos}");
            AddLog(ch, $"OL tp1 = {tp1}");
            AddLog(ch, $"OL tp2 = {tp2}");
            Wait(100);


            Dln.WriteArray(ch, addr, 0xAE, new byte[] { 0x3B });
            Dln.WriteArray(ch, addr, 0xA6, new byte[] { 0x7B });
            Dln.WriteArray(ch, addr, 0x02, new byte[] { 0x00 });

            byte[] rbuf2 = new byte[2];
            dc_count_rising = 0;
            for (open_input = start_pos[axis]; open_input < end_pos[1]; open_input += test_size)
            {
                int pos = open_input << 7;
                Dln.WriteArray(ch, addr, 0x00, new byte[] { (byte)(pos >> 8), (byte)pos });
                Wait(test_time);
                Dln.ReadArray(ch, addr, 0x80, rbuf2);
                open_output = (ushort)((rbuf2[0] << 8) + rbuf2[1]);
                open_data = open_output >> 3;
                if (open_data > 0x1000) { open_data -= 0x2000; }

                if ((open_input >= tp1) && (open_input <= tp2))
                {
                    dc_value[0, dc_count_rising] = (short)open_data;
                    dc_count_rising++;

                }
            }
            dc_count = dc_count_rising;
            dc_count_rising--;
            dc_count_falling = dc_count_rising;
            for (open_input -= test_size; open_input >= start_pos[axis]; open_input -= test_size)
            {
                int pos = open_input << 7;
                Dln.WriteArray(ch, addr, 0x00, new byte[] { (byte)(pos >> 8), (byte)pos });
                Wait(test_time);
                Dln.ReadArray(ch, addr, 0x80, rbuf2);
                open_output = (ushort)((rbuf2[0] << 8) + rbuf2[1]);
                open_data = open_output >> 3;
                if (open_data > 0x1000) { open_data -= 0x2000; }
                if ((open_input >= tp1) && (open_input <= tp2))
                {
                    dc_value[1, dc_count_falling] = (short)open_data;
                    dc_count_falling--;

                }
            }
            AddLog(ch, $"dc_count : {dc_count}");
            Dln.WriteArray(ch, addr, 0xA6, new byte[] { 0x00 });
            t_count = 0;
            byte[] rbuf = new byte[1];
            while (true)
            {
                Dln.ReadArray(ch, addr, 0x4C, rbuf);
                dc_result = rbuf[0];
                Wait(1);
                if ((dc_result & 0x10) == 0x00) break;
                t_count++;
                if (t_count > 100)
                {

                    if (testItem.Contains("1"))
                    {
                        PassFails[ch].Results[(int)SpecItem.OLTestXResult1].Val = 0;
                        ShowDataResults(ch, (int)SpecItem.OLTestXResult, (int)SpecItem.OLTestXResult1, InspType.Normal, new double[] { });
                    }
                    else if (testItem.Contains("2"))
                    {
                        PassFails[ch].Results[(int)SpecItem.OLTestXResult2].Val = 0;
                        ShowDataResults(ch, (int)SpecItem.OLTestXResult, (int)SpecItem.OLTestXResult2, InspType.Normal, new double[] { });
                    }
                    else
                    {
                        PassFails[ch].Results[(int)SpecItem.OLTestXResult].Val = 0;
                        ShowDataResults(ch, (int)SpecItem.OLTestXResult, (int)SpecItem.OLTestXResult, InspType.Normal, new double[] { });
                    }
                    return;
                }
            }
            Dln.WriteArray(ch, addr, 0xAE, new byte[] { 0x00 });
            dc_result = 0;
            sum_square = 0;
            for (int i = 0; i < dc_count - 1; i++)
            {
                Ya[i] = dc_value[1, i] - dc_value[0, i];
                Yb[i] = dc_value[1, i + 1] - dc_value[0, i + 1];
                height[i] = test_size;
                square[i] = ((Ya[i] + Yb[i]) * height[i]) >> 1;
                sum_square += (uint)Math.Abs(square[i]);
            }
            sum_square = (sum_square / 10);

            if (testItem.Contains("1"))
            {
                if (axis == 0)
                {
                    PassFails[ch].Results[(int)SpecItem.OLTestXResult1].Val = sum_square;
                    ShowDataResults(ch, (int)SpecItem.OLTestXResult1, (int)SpecItem.OLTestXResult1, InspType.Normal, new double[] { });
                }

                else
                {
                    PassFails[ch].Results[(int)SpecItem.OLTestYResult1].Val = sum_square;
                    ShowDataResults(ch, (int)SpecItem.OLTestYResult1, (int)SpecItem.OLTestYResult1, InspType.Normal, new double[] { });
                }
            }
            else if (testItem.Contains("2"))
            {
                if (axis == 0)
                {
                    PassFails[ch].Results[(int)SpecItem.OLTestXResult2].Val = sum_square;
                    ShowDataResults(ch, (int)SpecItem.OLTestXResult2, (int)SpecItem.OLTestXResult2, InspType.Normal, new double[] { });
                }

                else
                {
                    PassFails[ch].Results[(int)SpecItem.OLTestYResult2].Val = sum_square;
                    ShowDataResults(ch, (int)SpecItem.OLTestYResult2, (int)SpecItem.OLTestYResult2, InspType.Normal, new double[] { });
                }
            }
            else
            {
                if (axis == 0)
                {
                    PassFails[ch].Results[(int)SpecItem.OLTestXResult].Val = sum_square;
                    ShowDataResults(ch, (int)SpecItem.OLTestXResult, (int)SpecItem.OLTestXResult, InspType.Normal, new double[] { });
                }

                else
                {
                    PassFails[ch].Results[(int)SpecItem.OLTestYResult].Val = sum_square;
                    ShowDataResults(ch, (int)SpecItem.OLTestYResult, (int)SpecItem.OLTestYResult, InspType.Normal, new double[] { });
                }
            }


            AddLog(ch, $"sum square : {sum_square}");
            //if (sum_square > square_spec || sum_square <= 0)
            //{
            //    dc_result = 0x01;
            //    AddLog(ch, $"NG Over DC SR, {square_spec}");
            //    SetError(ch, NonSpecItem.OIS_Openloop_Test);
            //}
            AddLog(ch, $"[Final] {axisName} sum square : {sum_square}, result : {dc_result}");
            Dln.WriteArray(ch, addr, 0x02, new byte[] { 0x40 });
            DrvIC.Move(ch, "X", 2048);
            DrvIC.Move(ch, "Y", 2048);
            Wait(100);
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x02, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x02, new byte[] { 0x00 });

        }
        void OISOpenLoopTest(int ch, string testItem, int InspCnt)
        {
            AddLog(ch, $"<<<  X Open loop test Start  >>>");
            if (m_ChannelOn[ch]) oisOL(ch, 0, testItem);
            AddLog(ch, $"<<<  X Open loop test End  >>>");
            AddLog(ch, "");
            AddLog(ch, $"<<<  Y Open loop test Start  >>>");
            if (m_ChannelOn[ch]) oisOL(ch, 1, testItem);
            AddLog(ch, $"<<<  Y Open loop test End  >>>");

            PowerSequence(ch);
            AddLog(ch, $"PowerSequence Off & On");
        }
        void Act_AFScanAging(int ch, string testItem, int InspCnt)
        {


            AddLog(ch, "<<<  AF Scan aging Start  >>>");
            int target = 0, readhall = 0;
            int stepSize = Condition.AFScanAgingStep;
            int stepDelay = Condition.AFScanAgingDelay;
            stepSize = 256;
            stepDelay = 30;

            AddLog(ch, $"Start aging {Condition.AFSCanAgingCount} cycle for AF Driving");
            Dln.WriteArray(ch, DrvIC.AF_Addr, 0x02, new byte[] { 0x00 });
            DrvIC.AFMove(ch, AFCenter);
            Wait(100);


            for (int i = 0; i < Condition.AFSCanAgingCount; i++)
            {
                for (target = 2047; target >= 0; target -= stepSize)
                {
                    if (target <= 0) target = 0;
                    DrvIC.AFMove(ch, target); Wait(stepDelay);

                }
                for (target = 0; target <= 4095; target += stepSize)
                {
                    if (target >= 4095) target = 4095;
                    DrvIC.AFMove(ch, target); Wait(stepDelay);
                }
                for (target = 4095; target >= 2047; target -= stepSize)
                {
                    if (target <= 2047) target = 2047;
                    DrvIC.AFMove(ch, target); Wait(stepDelay);
                }
            }
            AddLog(ch, "<<<  AF Scan aging End  >>>");
            PassFails[0].Results[(int)SpecItem.AFScanAging].Val = 1;
            ShowDataResults(ch, (int)SpecItem.AFScanAging, (int)SpecItem.AFScanAging, InspType.Normal, new double[] { });

            //Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x02, new byte[] { 0x00 });
            //Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x02, new byte[] { 0x00 });

            //for (int i = 0; i < 15; i++)
            //{
            //    DrvIC.Move(ch, "X", 0);
            //    Wait(50);
            //    DrvIC.Move(ch, "X", 8191);
            //    Wait(50);
            //}
            //for (int i = 0; i < 15; i++)
            //{
            //    DrvIC.Move(ch, "Y", 0);
            //    Wait(50);
            //    DrvIC.Move(ch, "Y", 8191);
            //    Wait(50);
            //}
        }
        void Act_PreAFDriving(int ch, string testItem, int InspCnt)
        {
            LEDs_All_On(0, true);
            AddLog(ch, "AF Pre Driving");
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x02, new byte[] { 0x40 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x02, new byte[] { 0x40 });
            Dln.WriteArray(ch, DrvIC.AF_Addr, 0x02, new byte[] { 0x00 });
            FindResult res = new FindResult();

            for (int i = 0; i < 5; i++)
            {
                double[] MtoM = new double[2];
                DrvIC.AFMove(ch, 2048); Wait(50);

                DrvIC.AFMove(ch, 100); Wait(20);
                DrvIC.AFMove(ch, 20); Wait(20);
                DrvIC.AFMove(ch, 10); Wait(20);
                DrvIC.AFMove(ch, 0); Wait(50);
                res = Measure();
                MtoM[0] = res.cz[0];
                DrvIC.AFMove(ch, 4095 - 100); Wait(20);
                DrvIC.AFMove(ch, 4095 - 20); Wait(20);
                DrvIC.AFMove(ch, 4095 - 10); Wait(20);
                DrvIC.AFMove(ch, 4095); Wait(50);
                res = Measure();
                MtoM[1] = res.cz[0];

                AddLog(ch, $"{i + 1} scan stroke : {Math.Abs(MtoM[1] - MtoM[0]).ToString("F3")}");
            }
            LEDs_All_On(0, false);
        }

        int AFPOSVT, AFNEGVT;
        void AF_OpenLoopCal(int ch)
        {
            DrvIC.AFOnOff(ch, false);
            Wait(5);

            Dln.WriteByte(ch, DrvIC.AF_Addr, 0xAE, 1, 0x3B);
            byte backdata = Dln.ReadByte(ch, DrvIC.AF_Addr, 0x0B, 1);
            Dln.WriteByte(ch, DrvIC.AF_Addr, 0x1A, 1, 0x00);
            Dln.WriteByte(ch, DrvIC.AF_Addr, 0x0B, 1, (byte)(backdata & 0x7F));
            Dln.WriteByte(ch, DrvIC.AF_Addr, 0xA6, 1, 0x7B);
            Dln.WriteByte(ch, DrvIC.AF_Addr, 0xAE, 1, 0x00);

            DrvIC.AFOnOff(ch, true);
            AddLog(ch, $"AF Openloop Stroke Check");

            LEDs_All_On(0, true);

            //Openloop Strock Check
            var param = new DependencyParams()
            {
                Cam = m__G.oCam[0],
                driveIC = DrvIC,
                logText = ViewLog[ch],
                vision = STATIC.fVision,
                global = m__G
            };

            var logicFacotry = new OpenLoopStrockCheckedFactory();
            var executor = logicFacotry.CreateStrock(Option.SaveImageMode, param);
            if (executor is ISetSavePath path)
            {
                path.SetSavePath("C:\\6AxisTester\\Result\\Image\\AFHalCal\\");
            }
            executor.Execute(ch);
            LEDs_All_On(0, false);
        }
        bool AFMomoryUpdateMode(int ch, byte[] mode)
        {
            bool res = false;
            Dln.WriteByte(ch, DrvIC.AF_Addr, 0xAE, 1, 0x3B);

            for (int i = 0; i < mode.Length; i++)
                res = DrvIC.AF_Memory_Update(ch, mode[i]);
            Dln.WriteByte(ch, DrvIC.AF_Addr, 0xAE, 1, 0x00);

            return res;
        }
        void Act_FW_Download(int ch, string testItem, int InspCnt)
        {
            bool FWUpdateResult = false;
            AddLog(ch, $"OIS FW Ver : " + Path.GetFileName(Current.FWPath));
            if (Current.FWPath != "")
            {
                int FWLength = 48 * 1024;
                byte[] FWCode = null;
                try
                {
                    BinaryReader br = new BinaryReader(File.Open(Current.FWPath, FileMode.Open));
                    FWCode = br.ReadBytes(FWLength);
                    br.Close();

                    if (FWCode.Length != FWLength)
                    {
                        AddLog(ch, "Error! : Firmware Size is " + FWCode.Length + ". it should be " + FWLength.ToString());
                        FWUpdateResult = false;
                    }
                    uint fwver = 0;
                    fwver = Dln.Read4Byte(ch, 0x5A, 0x1008, 2);
                    if (fwver == Condition.iFWversionCheck)
                    {
                        AddLog(ch, string.Format("[PASS] Read FW ver : {0} ", fwver));
                        FWUpdateResult = true;
                    }
                    else
                    {
                        fwver = Dln.Read4Byte(ch, 0x5A, 0x1008, 2);
                        AddLog(ch, string.Format("Retry FW Check"));

                        if (fwver != Condition.iFWversionCheck)
                        {
                            AddLog(ch, string.Format("[FAIL] Read FW ver : {0} ", fwver));
                            FWUpdateResult = false;
                        }
                        else
                        {
                            AddLog(ch, string.Format("[PASS] Read FW ver : {0} ", fwver));
                            FWUpdateResult = true;
                        }
                    }

                    if (FWUpdateResult) FWUpdateResult = FWUpdate(ch, FWCode);
                }
                catch
                {
                    AddLog(ch, "Error! : Check FW File Path ");
                    FWUpdateResult = false;

                }
            }
            else
            {
                AddLog(ch, "Need to assign FW File");
                FWUpdateResult = false;
            }

            if (FWUpdateResult)
            {
                PassFails[0].Results[(int)SpecItem.FW_DOWNLOAD].Val = 0;
                ShowDataResults(ch, (int)SpecItem.FW_DOWNLOAD, (int)SpecItem.FW_DOWNLOAD, InspType.OKNG, new double[] { });
                m_ChannelOn[ch] = false;
                return;
            }
            else
            {
                PassFails[0].Results[(int)SpecItem.FW_DOWNLOAD].Val = 10;
                ShowDataResults(ch, (int)SpecItem.FW_DOWNLOAD, (int)SpecItem.FW_DOWNLOAD, InspType.OKNG, new double[] { });
            }
        }
        bool FWUpdate(int ch, byte[] FWData)
        {
            byte[] readData = new byte[4];
            byte rbyte = 0;
            byte[] SendData = new byte[256];

            uint crc = 0;
            try
            {
                uint current_fw_ver = Dln.Read4Byte(ch, 0x5A, 0x1008, 2);
                if (current_fw_ver != 0)
                {
                    rbyte = Dln.ReadByte(ch, 0x5A, 0x0001, 2);
                    if (rbyte != 0x01)
                        Dln.WriteByte(ch, 0x5A, 0x0000, 2, 0x00);
                    rbyte = Dln.ReadByte(ch, 0x5A, 0x0201, 2);
                    if (rbyte != 0x01)
                        Dln.WriteByte(ch, 0x5A, 0x0200, 2, 0x00);
                }

                byte FwCtrl = 0x07;
                FwCtrl |= 0x01;
                Dln.WriteByte(ch, 0x5A, 0x1000, 2, FwCtrl);
                Thread.Sleep(200);
                for (int i = 0; i < FWData.Length / 256; i++)
                {
                    SendData = new byte[256];
                    Array.Copy(FWData, 256 * i, SendData, 0, 256);
                    bool res = Dln.WriteArray(ch, 0x5A, 0x1100, SendData);
                    if (!res)
                    {
                        AddLog(ch, "Write Fail");
                        return false;
                    }

                    Wait(20);
                }

                crc = CalculateCRC32(FWData, (uint)FWData.Length);
                Dln.Write4Byte(ch, 0x5A, 0x1010, 2, crc);
                Thread.Sleep(200);
                rbyte = Dln.ReadByte(ch, 0x5A, 0x1001, 2);
                if (rbyte != 0x00)
                {
                    AddLog(ch, "Check Fail");
                    return false;
                }


                Dln.WriteByte(ch, 0x5A, 0x1000, 2, 0x80);
                Thread.Sleep(200);

                current_fw_ver = Dln.Read4Byte(ch, 0x5A, 0x1008, 2);
                AddLog(ch, string.Format("Read FW ver : {0} ", current_fw_ver));

                return true;
            }
            catch (Exception ex)
            {
                AddLog(ch, string.Format(ex.ToString()));
                return false;
            }
        }
        uint CalculateCRC32(byte[] data, uint size)
        {
            uint[] crc_table = new uint[256];
            uint crc_accum = 0;

            for (int i = 0; i < 256; i++)
            {
                crc_accum = (uint)(i << 24);
                for (int j = 0; j < 8; j++)
                {
                    if ((crc_accum & 0x80000000u) != 0)
                        crc_accum = (crc_accum << 1) ^ 0x04C11DB7u;
                    else crc_accum = (crc_accum << 1);
                }
                crc_table[i] = crc_accum;

            }
            crc_accum = 0;

            for (int i = 0; i < size; i++)
            {
                uint index = ((uint)(crc_accum >> 24) ^ data[i]) & 0xff;
                crc_accum = (crc_accum << 8) ^ crc_table[index];
            }
            return crc_accum;
        }
        void Act_AFHallCalibration(int ch, string testItem, int InspCnt)
        {
            //Slave Adress Change =============================================================
            //AF
            bool res = DrvIC.ChangeSlaveAddr(ch);
            if (!res)
            {
                PassFails[0].Results[(int)SpecItem.AF_NonEPAStroke].Val = 0;
                ShowDataResults(ch, (int)SpecItem.AF_NonEPAStroke, (int)SpecItem.AF_NonEPAStroke, InspType.Normal, new double[] { });
                return;
            }
            //OIS
            res = DrvIC.ChangeSlaveAddrOIS(ch);
            if (!res)
            {
                return;
            }
            //OpenLoopCal ============================================================
            AF_OpenLoopCal(ch);

            //PID Load =============================================================
            AddLog(ch, $"{Current.FWPath}");
            res = Load_AFPID(Current.FWPath);
            if (!res)
            {
                AddLog(ch, $"Load PID NG");
                PassFails[0].Results[(int)SpecItem.AF_NonEPAStroke].Val = 0;
                ShowDataResults(ch, (int)SpecItem.AF_NonEPAStroke, (int)SpecItem.AF_NonEPAStroke, InspType.Normal, new double[] { });
                return;
            }
            //PID Set =============================================================
            // 기존 DLN 방식 (유지)
            Dln.WriteByte(ch, DrvIC.AF_Addr, 0xAE, 1, 0x3B);
            DrvIC.AFOnOff(ch, false);
            Wait(5);
            Dln.WriteByte(ch, DrvIC.AF_Addr, 0xA6, 1, 0x00);
            AddLog(ch, $"\r\nAuto calibration\r\n");

            Dln.WriteByte(ch, DrvIC.AF_Addr, IC_SETTING_AF_REG[1], 1, IC_SETTING_AF[1]);
            Dln.WriteByte(ch, DrvIC.AF_Addr, IC_SETTING_AF_REG[0], 1, IC_SETTING_AF[0]);
            Dln.WriteByte(ch, DrvIC.AF_Addr, IC_SETTING_AF_REG[3], 1, IC_SETTING_AF[3]);
            Dln.WriteByte(ch, DrvIC.AF_Addr, IC_SETTING_AF_REG[4], 1, IC_SETTING_AF[4]);
            Dln.WriteByte(ch, DrvIC.AF_Addr, IC_SETTING_AF_REG[5], 1, IC_SETTING_AF[5]);
            Dln.WriteByte(ch, DrvIC.AF_Addr, IC_SETTING_AF_REG[6], 1, IC_SETTING_AF[6]);

            for (int i = 0xC0; i <= 0xC3; i++)
            {
                Dln.WriteByte(ch, DrvIC.AF_Addr, i, 1, 0x00);

            }
            AddLog(ch, $"Reset EPA Data.");
            for (int i = 0xC5; i <= 0xDF; i++)
            {
                Dln.WriteByte(ch, DrvIC.AF_Addr, i, 1, 0x00);

            }
            AddLog(ch, $"Reset Linearity comp coeff data.");

            for (int i = 0; i < IC_DATA_AF.Length; i++)
            {
                Dln.WriteByte(ch, DrvIC.AF_Addr, IC_DATA_AF_REG[i], 1, IC_DATA_AF[i]);
            }
            AddLog(ch, "DLN: PID Parameter setting.");

            Dln.WriteByte(ch, DrvIC.AF_Addr, 0xAE, 1, 0x00);
            //PID Set =============================================================

            //calibration Instruction ============================================

            Dln.WriteByte(ch, DrvIC.AF_Addr, 0xAE, 1, 0x3B);
            Dln.WriteByte(ch, DrvIC.AF_Addr, 0x5D, 1, 0x00);
            Dln.WriteByte(ch, DrvIC.AF_Addr, 0x02, 1, 0x80);
            Wait(10);
            byte rbuf = Dln.ReadByte(ch, DrvIC.AF_Addr, 0x70, 1);
            byte cBackup;
            byte cTemp;

            cBackup = cTemp = rbuf;
            AddLog(ch, $"1 Reg 0x70 : 0x{cBackup.ToString("X2")}");
            cBackup &= 0x80;
            AddLog(ch, $"2 Reg 0x70 : 0x{cBackup.ToString("X2")}");
            cTemp = (byte)((cTemp << 1) & 0x7E);
            cTemp |= cBackup;
            AddLog(ch, $"3 Reg 0x70 : 0x{cTemp.ToString("X2")}");
            rbuf = Dln.ReadByte(ch, DrvIC.AF_Addr, 0x71, 1);
            cBackup = rbuf;
            cBackup &= 0x80;
            AddLog(ch, $"4 Reg 0x71 : 0x{cBackup.ToString("X2")}");
            cTemp |= (byte)(cBackup >> 7);
            AddLog(ch, $"4 Reg 0x5D : 0x{cTemp.ToString("X2")}");

            Dln.WriteByte(ch, DrvIC.AF_Addr, 0x5D, 1, cTemp);
            int index = 0;
            cTemp = 0xff;
            while (cTemp > 0x10 && index < 5)
            {
                Dln.WriteByte(ch, DrvIC.AF_Addr, IC_SETTING_AF_REG[2], 1, IC_SETTING_AF[2]);
                for (int i = 0; i < 2; i++)
                {
                    Dln.WriteByte(ch, DrvIC.AF_Addr, 0x02, 1, 0x18);
                    Wait(300);
                }

                cTemp = Dln.ReadByte(ch, DrvIC.AF_Addr, 0x19, 1);
                int iTemp = cTemp;
                //   AddLog(ch, $"Reg : 0x19 = 0x{iTemp.ToString("X2")}");
                iTemp = iTemp * 3 / 4;
                if (iTemp < 0) iTemp = 0;
                if (iTemp > 255) iTemp = 255;
                cTemp = (byte)iTemp;
                AddLog(ch, $"Reg : 0x19 = 0x{iTemp.ToString("X2")}");
                if (cTemp > 0x10)
                {
                    //Error처리
                    AddLog(ch, $"AF calibration 2 (Reg 19) error[over 0x30]");
                }
                index++;
            }
            Dln.WriteByte(ch, DrvIC.AF_Addr, 0x19, 1, cTemp);
            Dln.WriteByte(ch, DrvIC.AF_Addr, 0xAE, 1, 0x00);

            if (index >= 5)
            {
                AddLog(ch, $"AF Hall Calibration NG");
                PassFails[0].Results[(int)SpecItem.AF_NonEPAStroke].Val = 0;
                ShowDataResults(ch, (int)SpecItem.AF_NonEPAStroke, (int)SpecItem.AF_NonEPAStroke, InspType.Normal, new double[] { });
                return;
            }


            // 증요 ==============================================================================================================
            res = AFMomoryUpdateMode(ch, new byte[] {1, 2, 3, 4 });
            if (!res)
            {
                AddLog(ch, $"AF Memory update NG");
                PassFails[0].Results[(int)SpecItem.AF_NonEPAStroke].Val = 0;
                ShowDataResults(ch, (int)SpecItem.AF_NonEPAStroke, (int)SpecItem.AF_NonEPAStroke, InspType.Normal, new double[] { });
                return;
            }

            //=====================================================================================================================

            PowerSequence(ch);
            AddLog(ch, $"PowerSequence Off & On");

            DrvIC.AFOnOff(ch, false);
            DrvIC.AF_IC_Data(ch);

            (bool EPARes, int stroke) = AF_EPA(ch);
            if (!EPARes)
            {
                AddLog(ch, $"AF EPA NG");
                PassFails[0].Results[(int)SpecItem.AF_NonEPAStroke].Val = stroke;
                ShowDataResults(ch, (int)SpecItem.AF_NonEPAStroke, (int)SpecItem.AF_NonEPAStroke, InspType.Normal, new double[] { });
                return;
            }
            //AddLog(ch, "<<<  AF Lin. Comp Start  >>>");
            //bool LinRes = AFLinComp(ch, 200, 3900, 10, 0, 0, 0, 0, 0);
            //AddLog(ch, "<<<  AF Lin. Comp End  >>>");
            //if (!LinRes)
            //{
            //    PassFails[0].Results[(int)SpecItem.AF_NonEPAStroke].Val = 0;
            //    ShowDataResults(ch, (int)SpecItem.AF_NonEPAStroke, (int)SpecItem.AF_NonEPAStroke, InspType.Normal, new double[] { });
            //    return;
            //}
            PassFails[0].Results[(int)SpecItem.AF_NonEPAStroke].Val = stroke;
            ShowDataResults(ch, (int)SpecItem.AF_NonEPAStroke, (int)SpecItem.AF_NonEPAStroke, InspType.Normal, new double[] { });
        }
        (bool, int) AF_EPA(int ch)
        {
            int BTM_POS = 30;
            //int BTM_POS = 30;
            //int TTL_RNG = 400;
            int TTL_RNG = 650;
            int TOP_POS = BTM_POS + TTL_RNG;
            int TOP_MARGIN = 10;
            LEDs_All_On(0, true);
            //AF EPA
            AddLog(ch, "<<<  AF EPA Start  >>>");
            int btm_position, tmp_position, top_position, ctr_position;
            int step, inf_cut, mac_cut;
            uint posvt, negvt, target_code;
            int stroke;
            int loop = 0, mac_loop = 0;
            int new_con = 0, old_con = 0, cond = 0;
            int mac_loop_max = 50;
            uint inf_tag_code, mac_tag_code;	// save code value
            FindResult res = new FindResult();
            DrvIC.AFOnOff(ch, true); Wait(100);
            DrvIC.AFMove(ch, DrvIC.AF_MID_CODE); Wait(50);
            res = Measure();
            ctr_position = (int)res.cz[0];
            DrvIC.AFMove(ch, DrvIC.AF_MIN_CODE + 100); Wait(20);
            DrvIC.AFMove(ch, DrvIC.AF_MIN_CODE + 20); Wait(20);
            DrvIC.AFMove(ch, DrvIC.AF_MIN_CODE + 10); Wait(20);
            DrvIC.AFMove(ch, DrvIC.AF_MIN_CODE); Wait(150);
            res = Measure1();
            int refPos = btm_position = (int)res.cz[0];
            tmp_position = 0;
            AddLog(ch, $"Inf Cut Start");

            for (target_code = 0, step = 0x200; step > 0; step >>= 1)
            {

                AddLog(ch, $"tmp_pos:{tmp_position}, tar_code:{target_code}, step:{step}");
                if (tmp_position < BTM_POS - 1) target_code += (ushort)step;
                else if (tmp_position > BTM_POS + 1) target_code -= (ushort)step;
                else break;
                DrvIC.AFMove(ch, (int)target_code);
                Wait(200);
                res = Measure1();
                tmp_position = btm_position = (short)(res.cz[0] - refPos);
                loop++;
            }
            inf_tag_code = target_code;
            AddLog(ch, $"Inf_loop : {loop}");
            negvt = target_code;
            AddLog(ch, $"negvt = {negvt}");
            inf_cut = tmp_position;
            if ((inf_cut < (BTM_POS - 1)) || (inf_cut > (BTM_POS + 1)))
            {
                AddLog(ch, $"EPA Error");

                LEDs_All_On(0, false);
                return (false, 0);
            }
            AddLog(ch, $"");
            DrvIC.AFMove(ch, DrvIC.AF_MAX_CODE - 100); Wait(20);
            DrvIC.AFMove(ch, DrvIC.AF_MAX_CODE - 20); Wait(20);
            DrvIC.AFMove(ch, DrvIC.AF_MAX_CODE - 10); Wait(20);
            DrvIC.AFMove(ch, DrvIC.AF_MAX_CODE); Wait(150);
            res = Measure1();
            top_position = (int)res.cz[0];
            tmp_position = 0;
            stroke = Math.Abs(refPos - top_position);

            if (stroke > TOP_POS + TOP_MARGIN)
            {
                mac_cut = (short)(stroke - (TOP_POS));
                step = 0x300;
            }
            else
            {
                mac_cut = TOP_MARGIN;
                step = 0x200;
            }
            AddLog(ch, "Mac Cut Start");
            AddLog(ch, $"Mac_Cut:{mac_cut}, Mac_Step:{step}");

            for (target_code = 4095; step > 0; step >>= 1)
            {
                string s = string.Empty;
                s += $"tmp_pos:{tmp_position}, tar_code:{target_code},";

                if (tmp_position < -1 - mac_cut)
                {
                    if (cond == 2)
                    {
                        step = (short)(step << 1);
                    }
                    target_code += (ushort)step;
                    cond = 2;
                    s += $"step:{step}, cond:{cond}";
                    AddLog(ch, s);
                }
                else if (tmp_position > 1 - mac_cut)
                {
                    if (cond == 3)
                    {
                        step = (short)(step << 1);
                    }
                    target_code -= (ushort)step;
                    cond = 3;
                    s += $"step:{step}, cond:{cond}";
                    AddLog(ch, s);
                }
                else break;
                DrvIC.AFMove(ch, (int)target_code);
                Wait(200);
                res = Measure1();
                tmp_position = (int)(res.cz[0] - top_position);
                mac_loop++;

                if (mac_loop > mac_loop_max) break;
            }
            mac_tag_code = target_code;

            if (mac_loop > mac_loop_max)
            {
                AddLog(ch, "Mac Cut Error");
                AddLog(ch, $"EPA Error");
                LEDs_All_On(0, false);
                return (false, 3);
            }
            AddLog(ch, $"tmp_pos:{tmp_position}, tar_code:{target_code}, mac_loop:{mac_loop}");
            posvt = target_code;
            AddLog(ch, $"posvt = {posvt}");
            AddLog(ch, "---------------------------------");
            AddLog(ch, $"Target stroke : {TTL_RNG}um");
            AddLog(ch, $"Target btm_top MG : {BTM_POS}_{TOP_MARGIN} um");
            AddLog(ch, $"Measured stroke : {stroke}um");
            AddLog(ch, $"Measured Mac_cut : {mac_cut}um");
            AddLog(ch, $"Inf cut-off size : {inf_cut}um");
            AddLog(ch, $"Mac cut-off size : {Math.Abs(tmp_position)}um");
            AddLog(ch, "---------------------------------");

            DrvIC.AFMove(ch, DrvIC.AF_MID_CODE); Wait(50);

            // 기존 DLN 방식 유지
            Dln.WriteByte(ch, DrvIC.AF_Addr, 0xAE, 1, 0x3B);
            Dln.WriteByte(ch, DrvIC.AF_Addr, 0x0B, 1, IC_SETTING_AF[1]);

            AFPOSVT = (int)((posvt - 4095) << 4);
            AFNEGVT = (int)(negvt << 4);

            AddLog(ch, $"AFPOSVT = {AFPOSVT}");
            AddLog(ch, $"AFNEGVT = {AFNEGVT}");

            Dln.Write2Byte(ch, DrvIC.AF_Addr, 0xC0, 1, (ushort)AFPOSVT);
            Dln.Write2Byte(ch, DrvIC.AF_Addr, 0xC2, 1, (ushort)AFNEGVT);

            bool WriteCheck = DrvIC.AF_Memory_Update(ch, 1);
            WriteCheck &= DrvIC.AF_Memory_Update(ch, 5);
            Dln.WriteByte(ch, DrvIC.AF_Addr, 0xAE, 1, 0x00);

            DrvIC.AFOnOff(ch, true);
            DrvIC.AF_IC_Data(ch);

            LEDs_All_On(0, false);

            if (Option.SaveRawData)
            {
                StreamWriter sw = null;
                string dateDir = STATIC.CreateDateDir();
                if (!Directory.Exists(dateDir)) Directory.CreateDirectory(dateDir);
                string path = dateDir + $"AF_EPA_CODE.csv";

                if (!File.Exists(path))
                {
                    sw = File.AppendText(path);
                    string s = $"SPL No, Date, Time, INF Code, MAC Code";
                    sw.WriteLine(s);
                    sw.Close();
                }
                sw = File.AppendText(path);
                string data = $"{m_StrIndex[ch]},{STATIC.LogDate.ToString("yyyy-MM-dd")},{STATIC.LogDate.Hour}h{STATIC.LogDate.Minute}m{STATIC.LogDate.Second}s," +
                    $"{inf_tag_code},{mac_tag_code}";
                sw.WriteLine(data);
                sw.Close();
            }
            AddLog(ch, "<<<  AF EPA End  >>>");


            return (true, stroke);
        }
        bool AFLinComp(int ch, int startpos, int endpos, int step, int margin_start, int margin_end, int s_value, int e_value, int linear_spec)
        {
            LEDs_All_On(0, true);
            int NUM_COEF = 27;
            FindResult tmpres = new FindResult();
            float[] targPosi = new float[step + 1]; // Array for storing target position data
            float[] lensPosi = new float[step + 1]; // Array for storing lens position data
            int[] readHall = new int[step + 1];
            float[] refLensPosi = new float[step + 1];
            int valueStepsize = step - s_value - e_value;
            float[] valueLensPosi = new float[valueStepsize + 1];
            float refStepsize = 0, gap = 0, valueStep = 0, valuegap = 0;
            float max_gap = 0, max_valuegap = 0;
            int temp_table = startpos;
            int ignInf = 0;
            int ignMac = 0;
            int numLinCompData;
            int pVt, nVt;
            pVt = (Dln.Read2Byte(ch, DrvIC.AF_Addr, 0xC0, 1) >> 6) & 0x03FF;
            nVt = (Dln.Read2Byte(ch, DrvIC.AF_Addr, 0xC2, 1) >> 6) & 0x03FF;

            int[] linCoef = new int[NUM_COEF]; // Array for storing line compensation coefficients
            int pVtNew = 0;    // Recalculation "POSVT" after linearity compensation
            int nVtNew = 0;    // Recalculation "NEGVT" after linearity compensation
            float resError = 0;   // Variable for storing residual error after linearity compensation

            int result;
            int step_size = (endpos - startpos) / step;
            int LZValue, HallValue;
            float RefData = 0;
            AddLog(ch, $"AK7316 Linearity Compensation start");
            AddLog(ch, $"pVt : {pVt}");
            AddLog(ch, $"nVt : {nVt}");
            AddLog(ch, $"step size : {step_size}");

            DrvIC.AFOnOff(ch, true);
            DrvIC.AFMove(ch, startpos); Wait(100);
            DrvIC.OISOn(ch, "X", false); DrvIC.OISOn(ch, "Y", false); Wait(200);

            Dln.WriteByte(ch, DrvIC.AF_Addr, 0xAE, 1, 0x3B);
            for (int i = 0; i < 27; i++) Dln.WriteByte(ch, DrvIC.AF_Addr, (byte)(0xC5 + i), 1, 0x00);
            Dln.WriteByte(ch, DrvIC.AF_Addr, 0xAE, 1, 0x00);


            AddLog(ch, $"Target\tReadHall\tPos");
            for (int i = 0; i <= step; i++)
            {
                targPosi[i] = temp_table;
                DrvIC.AFMove(ch, (int)targPosi[i]); Wait(50);
                readHall[i] = DrvIC.ReadAFHall(ch);
                tmpres = Measure();
                if (i != 0) lensPosi[i] = (float)tmpres.cz[0] - RefData;
                else { lensPosi[i] = 0; RefData = (float)tmpres.cz[0]; }

                temp_table += step_size;
                AddLog(ch, $"{targPosi[i]}\t{readHall[i]}\t{lensPosi[i].ToString("F2")}");
            }

            valueStep = (lensPosi[step - e_value] - lensPosi[s_value]) / (valueStepsize);
            valueLensPosi[0] = lensPosi[s_value];
            valueLensPosi[valueStepsize] = lensPosi[s_value + valueStepsize];

            AddLog(ch, "");
            AddLog(ch, "=== Linearity check ===");
            AddLog(ch, $"ValueStepSize = {valueStepsize}");
            AddLog(ch, $"ValueStep = {valueStep}");
            AddLog(ch, "=======================");
            AddLog(ch, $"{lensPosi[s_value].ToString("F3")}, {valueLensPosi[0].ToString("F3")}");

            for (int i = 1; i < valueStepsize; i++)
            {
                valueLensPosi[i] = valueLensPosi[i - 1] + valueStep;
                valuegap = valueLensPosi[i] - lensPosi[i + s_value];
                if (valuegap >= 0) { }
                else { valuegap *= -1; }
                AddLog(ch, $"{lensPosi[i + s_value].ToString("F3")}, {valueLensPosi[i].ToString("F3")}, {valuegap.ToString("F3")}");

                if (max_valuegap < valuegap) max_valuegap = valuegap;

            }
            AddLog(ch, $"{lensPosi[valueStepsize + s_value].ToString("F3")}, {valueLensPosi[valueStepsize].ToString("F3")}");
            AddLog(ch, $"max valuegap= {max_valuegap.ToString("F3")}");
            if (max_valuegap > linear_spec)
            {

                if (targPosi.Length == lensPosi.Length)
                {
                    AFLinCompCoef coef = new AFLinCompCoef();
                    numLinCompData = targPosi.Length;
                    AddLog(ch, $"numLinCompData = {numLinCompData}");

                    result = coef.LinCompMain(targPosi, lensPosi, numLinCompData, pVt, nVt, ignInf, ignMac, linCoef, ref resError);
                    if (result != 0)
                    {
                        AddLog(ch, $"Linearity Comp Fail");
                        LEDs_All_On(0, false);
                        return false;  // Error: please check return value of LinCompMain()
                    }

                }
                else
                {
                    AddLog(ch, $"Number of targetd ata and lens data is different");
                    LEDs_All_On(0, false);
                    return false;   // Error: Number of targetd ata and lens data is different.
                                    //return 1;	// Error: Number of targetd ata and lens data is different.
                }

                //// Result display example.
                DrvIC.AFMove(ch, DrvIC.AF_MID_CODE); Wait(50);              // Lens Move Mid Code : 0x03 update -> positon store			

                // 기존 DLN 방식 (유지)
                Dln.WriteByte(ch, DrvIC.AF_Addr, 0xAE, 1, 0x3B);
                for (int i = 0; i < 27; i++)
                {
                    Dln.WriteByte(ch, DrvIC.AF_Addr, (byte)(0xC5 + i), 1, (byte)linCoef[i]);
                }
                DrvIC.AF_Memory_Update(ch, 4);
                Dln.WriteByte(ch, DrvIC.AF_Addr, 0xAE, 1, 0x00);
            }

            else
            {
                // Laser disable

                AddLog(ch, " Skip Linearity Compensation");
                LEDs_All_On(0, false);
                return false;
            }


            LEDs_All_On(0, false);
            DrvIC.OISOn(ch, "X", false);
            DrvIC.OISOn(ch, "Y", false);
            Wait(50);
            return true;
        }

        //private void Act_AFInit(int ch, string testItem)
        //{
        //    byte[] rbuf = new byte[1];
        //    FindResult res = new FindResult();
        //    double[] zVal = new double[2];


        //    Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x02, new byte[] { 0x40 });
        //    Wait(50);
        //    Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xAE, new byte[] { 0x3B });
        //    //AF OpenLoop Seq 추가
        //    Dln.ReadArray(ch, DrvIC.AFSlaveAddr, 0x0B, rbuf);
        //    rbuf[0] = (byte)(rbuf[0] & 0x7F);
        //    Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x0B, rbuf);
        //    Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xA6, new byte[] { 0x7B });
        //    Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x02, new byte[] { 0x00 });
        //    Wait(50);
        //    AddLog(ch, $"AF Openloop Stroke Check");

        //    LEDs_All_On(0, true);
        //    for (int i = 0; i < 11; i++)
        //    {
        //        DrvIC.Move(ch, "AF", 4095);
        //        Wait(50);
        //        res = Measure();
        //        zVal[0] = res.cz[0];
        //        DrvIC.Move(ch, "AF", 0);
        //        Wait(50);
        //        res = Measure();
        //        zVal[1] = res.cz[0];

        //        AddLog(ch, $"{i + 1} : {Math.Abs(zVal[1] - zVal[0]).ToString("F3")}");

        //    }
        //    LEDs_All_On(0, false);
        //    Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x02, new byte[] { 0x40 });
        //    Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xA6, new byte[] { 0x00 });

        //    AF_EPA_Reset(ch);
        //    AF_LinearityComp_Reset(ch);
        //    AddLog(ch, "PID parameter setting");
        //    for (int i = 0; i < AFPID.Count; i++)
        //    {
        //        Dln.WriteArray(ch, DrvIC.AFSlaveAddr, AFPID[i][0], new byte[] { AFPID[i][1] });
        //    }


        //    AddLog(ch, "Temp register setting");
        //    Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xC9, new byte[] { 0x00 });
        //    Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x02, new byte[] { 0x80 });
        //    Wait(10);
        //    Dln.ReadArray(ch, DrvIC.AFSlaveAddr, 0x70, rbuf);
        //    AddLog(ch, $"Read 0x70 : 0x{rbuf[0].ToString("X")}");


        //    Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xC9, rbuf);

        //    AddLog(ch, "Calibration instruction");
        //    Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x0C, new byte[] { 0x62 });
        //    Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x02, new byte[] { 0x18 });
        //    Wait(150);
        //    Dln.ReadArray(ch, DrvIC.AFSlaveAddr, 0x19, rbuf);
        //    AddLog(ch, $"Read 0x19 : 0x{rbuf[0].ToString("X")}");

        //    byte tmpData = (byte)(rbuf[0] * 0.75);
        //    AddLog(ch, $"CalcData : 0x{tmpData.ToString("X")}");

        //    if (tmpData >= 0x00 && tmpData <= 0x30)
        //    {
        //        Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x19, new byte[] { tmpData });
        //    }
        //    else
        //    {
        //        SetError(ch, NonSpecItem.AF_Init);
        //        return;
        //        //Error처리
        //    }
        //    Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xF3, new byte[] { 0x1E });
        //    Wait(10);
        //    Store(ch, 0);
        //    Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xAE, new byte[] { 0x00 });
        //    Dln.PowerSequence(0);
        //    AK7314_ICReset(0);
        //    CheckData(ch, 0);
        //}

        void Act_CloseLoopAging(int ch, string testitem, int InspCnt)
        {
            CloseLoopAging(ch);
        }
        //private void Act_AFEPA(int ch, string testItem)
        //{

        //    Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x02, new byte[] { 0x40 });
        //    Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x02, new byte[] { 0x40 });
        //    if(DrvIC.Y2SlaveAddr != 0x00) Dln.WriteArray(ch, DrvIC.Y2SlaveAddr, 0x02, new byte[] { 0x40 });

        //    LEDs_All_On(0, true);
        //    FindResult res = new FindResult();
        //    int findcount = 0;

        //    double Target = Condition.AFEPATarget;
        //    int InfCut = 10;
        //    int macCut = 6;
        //    byte[] rbuf2 = new byte[2];
        //    byte[] rbuf = new byte[1];
        //    byte backData = 0;
        //    double InitPos = 0; double EndPos = 0;

        //    //move 0 code Position
        //    Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x02, new byte[] { 0x00 });
        //    Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x00, new byte[] { 0x80, 0x00 });
        //    Wait(50);
        //    Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x00, new byte[] { 0x19, 0x00 });
        //    Wait(50);
        //    Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x00, new byte[] { 0x05, 0x00 });
        //    Wait(50);
        //    Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x00, new byte[] { 0x02, 0x80 });
        //    Wait(50);
        //    Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x00, new byte[] { 0x00, 0x00 });
        //    Wait(100);
        //    //측정하고 값 초기화         
        //    AddLog(ch, $"af pos(t, c) : {0},{DrvIC.ReadHall(ch, "AF")}");
        //    Wait(50);
        //    res = Measure();

        //    InitPos = res.cz[0];
        //    int dir = 1;

        //    int step = 512;
        //    int pos = step;
        //    InfCut = (int)(InitPos + 10);
        //    while (true)
        //    {

        //        if(findcount > 50)
        //        {
        //            AddLog(ch, "EPA Find NG");
        //            SetError(ch, NonSpecItem.AF_EPA);
        //            return;
        //        }
        //        DrvIC.Move(ch, "AF", pos);
        //        int a = DrvIC.ReadHall(ch, "AF");
        //        Wait(100);
        //        res = Measure();


        //        AddLog(ch, $"Pos:{(int)(res.cz[0] - InitPos)}, Code:{pos}, Step:{step}");

        //        if (res.cz[0] > InfCut + 1)
        //        {
        //            if (dir == 1)
        //            {
        //                dir = 0;
        //                step = step / 2;
        //                pos = pos - step;
        //            }
        //            else
        //            {
        //                dir = 0;
        //                pos = pos - step;
        //            }

        //        }
        //        else if (res.cz[0] < InfCut - 1)
        //        {
        //            if (dir == 1)
        //            {
        //                dir = 1;
        //                pos = pos + step;
        //            }
        //            else
        //            {
        //                dir = 1;
        //                step = step / 2;
        //                pos = pos + step;
        //            }

        //        }
        //        else { break; }
        //        findcount++;

        //    }

        //    int InfPos = pos;
        //    AddLog(ch, $"Inf Code : {InfPos}");

        //    Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x00, new byte[] { 0x80, 0x00 });
        //    Wait(50);
        //    res = Measure();
        //    Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x00, new byte[] { 0xE6, 0xF0 });
        //    Wait(50);
        //    Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x00, new byte[] { 0xFA, 0xF0 });
        //    Wait(50);
        //    Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x00, new byte[] { 0xFD, 0x70 });
        //    Wait(50);
        //    Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x00, new byte[] { 0xFF, 0xF8 });
        //    Wait(100);
        //    //측정하고 값 초기화, Measure Stroke 구해서 담음
        //    double measureStroke = 0;


        //    Dln.ReadArray(ch, DrvIC.AFSlaveAddr, 0x84, rbuf2); // check AF Current Hall
        //    AddLog(ch, $"af pos(t, c) : {4095},{DrvIC.ReadHall(ch, "AF")}");
        //    Wait(50);
        //    res = Measure();

        //    EndPos = res.cz[0];
        //    measureStroke = Math.Abs(EndPos - InitPos);
        //    AddLog(ch, $"Full Stroke = {measureStroke.ToString("F3")}");
        //    PassFails[ch].Results[(int)SpecItem.AF_NonEPAStroke].Val = measureStroke;
        //    ShowDataResults(ch, (int)SpecItem.AF_NonEPAStroke, (int)SpecItem.AF_NonEPAStroke);
        //    if (measureStroke - Target - 10 > 6) macCut = (int)(measureStroke - Target - 10);
        //    AddLog(ch, $"Find macCut = {macCut}");

        //    dir = 0;
        //    step = 512;
        //    pos = 4095 - step;
        //    macCut = (int)(EndPos - macCut);
        //    findcount = 0;
        //    while (true)
        //    {
        //        if (findcount > 50)
        //        {
        //            AddLog(ch, "EPA Find NG");
        //            SetError(ch, NonSpecItem.AF_EPA);
        //            return;
        //        }
        //        DrvIC.Move(ch, "AF", pos);
        //        Wait(100);
        //        res = Measure();

        //        AddLog(ch, $"Pos:{(int)(res.cz[0] - EndPos)}, Code:{pos}, Step:{step}");
        //        //측정하고 값 기입
        //        if (res.cz[0] > macCut + 1)
        //        {
        //            if (dir == 1)
        //            {
        //                dir = 0;
        //                step = step / 2;
        //                pos = pos - step;
        //            }
        //            else
        //            {
        //                dir = 0;
        //                pos = pos - step;
        //            }

        //        }
        //        else if (res.cz[0] < macCut - 1)
        //        {
        //            if (dir == 1)
        //            {
        //                dir = 1;
        //                pos = pos + step;
        //            }
        //            else
        //            {
        //                dir = 1;
        //                step = step / 2;
        //                pos = pos + step;
        //            }

        //        }
        //        else { break; }
        //        findcount++;

        //    }
        //    int macPos = pos;
        //    AddLog(ch, $"Mac Code : {macPos}");
        //    //   Inf, Mac EPA 기입 계산

        //    byte POSVT = (byte)((4096 - macPos) / 16); byte NEGVT = (byte)(InfPos / 16);

        //    //   byte POSVT = (byte)((-Condition.AFPOSVT) / 16); byte NEGVT = (byte)(Condition.AFNEGVT / 16);

        //    //     AddLog(ch, $"POSVT = {Condition.AFPOSVT}, NEGVT = {Condition.AFNEGVT}");
        //    AddLog(ch, $"0x0E : 0x{POSVT.ToString("X")}, 0x0F : 0x{NEGVT.ToString("X")}");


        //    Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x02, new byte[] { 0x40 });
        //    Wait(5);
        //    Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xAE, new byte[] { 0x3B });
        //    Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x0E, new byte[] { POSVT });
        //    Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x0F, new byte[] { NEGVT });
        //    Dln.ReadArray(ch, DrvIC.AFSlaveAddr, 0x0B, rbuf);
        //    backData = rbuf[0];
        //    Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x0B, new byte[] { (byte)(rbuf[0] | 0x80) });//0x0B값 읽어서 백업해야하는지 확인

        //    DrvIC.Move(ch, "AF", AFCenter);

        //    Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x03, new byte[] { 0x01 });
        //    Wait(100);
        //    Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x03, new byte[] { 0x10 });
        //    Wait(200);
        //    Dln.ReadArray(ch, DrvIC.AFSlaveAddr, 0x4B, rbuf);
        //    if ((byte)(rbuf[0] & 0x04) != 0x00)
        //    {
        //        SetError(ch, NonSpecItem.AF_EPA);
        //        return;
        //    }

        //    Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xAE, new byte[] { 0x00 });
        //    CheckData(ch, 0);

        //    if (Option.SaveRawData)
        //    {
        //        StreamWriter sw = null;
        //        string dateDir = STATIC.CreateDateDir();
        //        if (!Directory.Exists(dateDir)) Directory.CreateDirectory(dateDir);
        //        string path = dateDir + $"AF_EPA_CODE.csv";

        //        if (!File.Exists(path))
        //        {
        //            sw = File.AppendText(path);
        //            string s = $"SPL No, Date, Time, INF Code, MAC Code";
        //            sw.WriteLine(s);
        //            sw.Close();
        //        }
        //        sw = File.AppendText(path);
        //        string data = $"{m_StrIndex[ch]},{STATIC.LogDate.ToString("yyyy-MM-dd")},{STATIC.LogDate.Hour}h{STATIC.LogDate.Minute}m{STATIC.LogDate.Second}s," +
        //            $"{InfPos},{macPos}";
        //        sw.WriteLine(data);
        //        sw.Close();
        //    }

        //}
        //private void Act_OISEPA(int ch, string testItem)
        //{
        //    byte[] rbuf = new byte[1];
        //    byte backData = 0;

        //    int Xposvt = -Condition.XPOSVT, Xnegvt = Condition.XNEGVT, Yposvt = -Condition.YPOSVT, Ynegvt = Condition.YNEGVT;
        //    AddLog(ch, $"X POSVT = {Xposvt}, X NEGVT = {Xnegvt}");
        //    AddLog(ch, $"Y POSVT = {Yposvt}, Y NEGVT = {Ynegvt}");

        //    AddLog(ch, $"X = 0x0E : 0x{((Xposvt / 4) >> 2).ToString("X")}, 0x0F : 0x{((Xnegvt / 4) & 0x03).ToString("X")}");
        //    AddLog(ch, $"Y = 0x0E : 0x{((Yposvt / 4) >> 2).ToString("X")}, 0x0F : 0x{((Ynegvt / 4) & 0x03).ToString("X")}");

        //    Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x02, new byte[] { 0x40 });
        //    Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x02, new byte[] { 0x40 });
        //    Wait(5);
        //    Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0xAE, new byte[] { 0x3B });
        //    Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0xAE, new byte[] { 0x3B });
        //    Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x0E, new byte[] { (byte)((Xposvt / 4) >> 2) });
        //    Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x0F, new byte[] { (byte)((Xnegvt / 4) >> 2) });
        //    Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x0D, new byte[] { (byte)(((Xposvt / 4) & 0x03 << 2) | ((Xnegvt) & 0x03)) });

        //    Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x0E, new byte[] { (byte)((Yposvt / 4) >> 2) });
        //    Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x0F, new byte[] { (byte)((Ynegvt / 4) >> 2) });
        //    Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x0D, new byte[] { (byte)(((Yposvt / 4) & 0x03 << 2) | ((Ynegvt) & 0x03)) });


        //    Dln.ReadArray(ch, DrvIC.XSlaveAddr, 0x0B, rbuf);
        //    backData = rbuf[0];
        //    Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x0B, new byte[] { (byte)(rbuf[0] | 0X80) });//0x0B값 읽어서 백업해야하는지 확인
        //    Dln.ReadArray(ch, DrvIC.Y1SlaveAddr, 0x0B, rbuf);
        //    backData = rbuf[0];
        //    Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x0B, new byte[] { (byte)(rbuf[0] | 0X80) });//0x0B값 읽어서 백업해야하는지 확인
        //    Wait(120);

        //    Store(ch, 1);
        //    Store(ch, 2);
        //    Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0xAE, new byte[] { 0x00 });
        //    Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0xAE, new byte[] { 0x00 });
        //}





        void OIS_EPA_Reset(int ch)
        {
            AddLog(ch, "OIS EPA Reset");
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x0E, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x0E, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x0F, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x0F, new byte[] { 0x00 });
        }
        void OIS_LinearityComp_Reset(int ch, int Axis)
        {
            if (Axis == 0)
            {
                AddLog(ch, "X Linearity Comp Reset");
                Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0xAE, new byte[] { 0x3B });


                Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x2A, new byte[] { 0x00 });
                Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x2B, new byte[] { 0x00 });
                Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x2C, new byte[] { 0x00 });
                Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x2D, new byte[] { 0x00 });
                Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x2E, new byte[] { 0x00 });
                Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x2F, new byte[] { 0x00 });
                Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x30, new byte[] { 0x00 });
                Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x31, new byte[] { 0x00 });
                Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x32, new byte[] { 0x00 });
                Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x33, new byte[] { 0x00 });
                Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x34, new byte[] { 0x00 });
                Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x35, new byte[] { 0x00 });
                Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x36, new byte[] { 0x00 });
                Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x03, new byte[] { 0x08 });
                Wait(100);
                Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0xAE, new byte[] { 0x00 });
            }
            else
            {
                AddLog(ch, "Y Linearity Comp Reset");
                Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0xAE, new byte[] { 0x3B });
                Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x2A, new byte[] { 0x00 });
                Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x2B, new byte[] { 0x00 });
                Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x2C, new byte[] { 0x00 });
                Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x2D, new byte[] { 0x00 });
                Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x2E, new byte[] { 0x00 });
                Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x2F, new byte[] { 0x00 });
                Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x30, new byte[] { 0x00 });
                Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x31, new byte[] { 0x00 });
                Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x32, new byte[] { 0x00 });
                Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x33, new byte[] { 0x00 });
                Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x34, new byte[] { 0x00 });
                Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x35, new byte[] { 0x00 });
                Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x36, new byte[] { 0x00 });
                Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x03, new byte[] { 0x08 });
                Wait(100);
                Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0xAE, new byte[] { 0x00 });
            }




        }

        void CloseLoopAging(int ch)
        {
            int AFMin = Condition.CLAgingAFMin, AFMax = Condition.CLAgingAFMax, OISMin = Condition.CLAgingOISMin, OISMax = Condition.CLAgingOISMax, count = Condition.CLAgingCount;
            int delay = 1000 / Condition.CLAgingFreq / 2;
            int[] check_hall = new int[3];

            AddLog(ch, "<<<  XYZ Aging Start  >>>");
            AddLog(ch, $"Frequency : {Condition.CLAgingFreq}");
            AddLog(ch, $"Aging Count : {count}");
            AddLog(ch, $"AF Range : {AFMin} - {AFMax}");
            AddLog(ch, $"OIS Range : {OISMin} - {OISMax}");

            //Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x02, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x02, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x02, new byte[] { 0x00 });

            //DrvIC.Move(ch, "AF", AFCenter);
            DrvIC.Move(ch, "X", OISCenter);
            DrvIC.Move(ch, "Y", OISCenter);

            for (int i = 0; i < count; i++)
            {
                DrvIC.AFMove(ch, AFMin);
                DrvIC.Move(ch, "X", OISMin);
                DrvIC.Move(ch, "Y", OISMin);
                Wait(delay);
                DrvIC.AFMove(ch, AFMax);
                DrvIC.Move(ch, "X", OISMax);
                DrvIC.Move(ch, "Y", OISMax);
            }


            DrvIC.AFMove(ch, AFCenter);
            DrvIC.Move(ch, "X", OISCenter);
            DrvIC.Move(ch, "Y", OISCenter);
            Wait(delay);
            //   Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x02, new byte[] { 0x40 });
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x02, new byte[] { 0x40 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x02, new byte[] { 0x40 });
            AddLog(ch, "<<<  XYZ Aging End  >>>");

            PassFails[0].Results[(int)SpecItem.XYZAging].Val = 1;
            ShowDataResults(ch, (int)SpecItem.XYZAging, (int)SpecItem.XYZAging, InspType.Normal, new double[] { });

        }

        void Act_OISLinComp(int ch, string testItem, int InspCnt)
        {
            bool resX = false;
            bool resY = false;
            if (m_ChannelOn[ch]) resX = OISLinComp(ch, 0);
            if (m_ChannelOn[ch]) resY = OISLinComp(ch, 1);
            if (!resX || !resY)
            {
                PassFails[0].Results[(int)SpecItem.XYLinearComp].Val = 10;
                ShowDataResults(ch, (int)SpecItem.XYLinearComp, (int)SpecItem.XYLinearComp, InspType.Normal, new double[] { });
            }
            else
            {
                PassFails[0].Results[(int)SpecItem.XYLinearComp].Val = 0;
                ShowDataResults(ch, (int)SpecItem.XYLinearComp, (int)SpecItem.XYLinearComp, InspType.Normal, new double[] { });
            }

        }
        void Act_AFLinComp(int ch, string testItem, int InspCnt)
        {
            AddLog(ch, "<<<  AF Lin. Comp Start  >>>");
            bool LinRes = AFLinComp(ch, Condition.AfLinCompStart, Condition.AfLinCompEnd, Condition.AFLinCompStep, 0, 0, 0, 0, 0);
            AddLog(ch, "<<<  AF Lin. Comp End  >>>");

            if (!LinRes)
            {
                PassFails[0].Results[(int)SpecItem.AF_LinearComp].Val = 10;
                ShowDataResults(ch, (int)SpecItem.AF_LinearComp, (int)SpecItem.AF_LinearComp, InspType.Normal, new double[] { });
            }
            else
            {
                PassFails[0].Results[(int)SpecItem.AF_LinearComp].Val = 0;
                ShowDataResults(ch, (int)SpecItem.AF_LinearComp, (int)SpecItem.AF_LinearComp, InspType.Normal, new double[] { });
            }

        }
        bool OISLinComp(int ch, int axis)
        {
            AddLog(ch, "<<<  OIS Linearity comp. Start  >>>");


            int addr = axis == 0 ? DrvIC.XSlaveAddr : DrvIC.Y1SlaveAddr;
            string AxisName = axis == 0 ? "X" : "Y";
            float[] dbTargetPosi = new float[Condition.OISLincompStep + 1];
            float[] dbLensPosi = new float[Condition.OISLincompStep + 1];
            int[] dbHalldata = new int[Condition.OISLincompStep + 1];
            float RefData = 0;
            byte[] ucResultCoef = new byte[13];
            int temp_table = Condition.OISLincompCodeMargin, step = 128;
            step = (8192 - 2 * Condition.OISLincompCodeMargin) / Condition.OISLincompStep;

            LEDs_All_On(0, true);

            OIS_LinearityComp_Reset(ch, axis);
            DrvIC.AFOnOff(ch, true);
            DrvIC.AFMove(ch, BestAFPos); Thread.Sleep(50);
            AddLog(ch, $"Best AF for linear_comp : {BestAFPos}");

            DrvIC.OISOn(ch, "X", true);
            DrvIC.OISOn(ch, "Y", true);
            DrvIC.Move(ch, "X", OISCenter);
            DrvIC.Move(ch, "Y", OISCenter);
            Wait(50);

            FindResult tmpres = new FindResult();

            AddLog(ch, $"Target\tDisplacement\tReadHall");
            AddLog(ch, "---------------------------------");

            for (int i = 0; i < Condition.OISLincompStep + 1; i++)
            {
                if (temp_table > 8191) temp_table = 8191;
                dbTargetPosi[i] = temp_table;

                if (axis == 0) { DrvIC.Move(ch, "X", (int)dbTargetPosi[i]); DrvIC.Move(ch, "Y", 4096); }
                else if (axis == 1) { DrvIC.Move(ch, "X", 4096); DrvIC.Move(ch, "Y", (int)dbTargetPosi[i]); }
                if (i == 0) Wait(100);
                else Wait(30);
                Wait(20);
                dbHalldata[i] = DrvIC.ReadHall(ch, AxisName);
                tmpres = Measure();
                if (axis == 0)
                {
                    if (i != 0) dbLensPosi[i] = (float)(tmpres.cx[0] - RefData);
                    else { dbLensPosi[i] = 0; RefData = (float)tmpres.cx[0]; }
                }
                else
                {
                    if (i != 0) dbLensPosi[i] = (float)(tmpres.cy[0] - RefData);
                    else { dbLensPosi[i] = 0; RefData = (float)tmpres.cy[0]; }
                }
                temp_table += step;
                AddLog(ch, $"{dbTargetPosi[i]}\t{dbLensPosi[i].ToString("F2")}\t{dbHalldata[i]}");
                if (i > 1 && dbHalldata[i] <= dbHalldata[i - 1])
                {
                    AddLog(ch, "OIS Linearity comp. error.");

                    return false;

                }
            }
            AddLog(ch, "---------------------------------");


            byte pvt = 0, nvt = 0;
            byte[] rbuf = new byte[1];
            int ignInf = 0;
            int ignMac = 0;
            int numLinCompData;
            int[] linCoef = new int[OISLinCompCoef.NUM_COEF];
            float resError = 0;

            if (axis == 0)
            {
                pvt = (byte)Condition.OISLincompXEPAPos;
                nvt = (byte)Condition.OISLincompXEPANeg;
            }
            else
            {
                pvt = (byte)Condition.OISLincompYEPAPos;
                nvt = (byte)Condition.OISLincompYEPANeg;
            }


            Dln.ReadArray(ch, addr, 0x0E, rbuf);
            pvt = rbuf[0];
            Dln.ReadArray(ch, addr, 0x0F, rbuf);
            nvt = rbuf[0];

            AddLog(ch, $"POSVT = {pvt}, NEGVT = {nvt}");

            OISLinCompCoef coef = new OISLinCompCoef();

            ///임시 Test 코드 =======================================================================================
            //dbTargetPosi[0] = 400; dbLensPosi[0] = (float)0.00; dbHalldata[0] = 400;
            //dbTargetPosi[1] = 1139; dbLensPosi[1] = (float)73.81; dbHalldata[1] = 1147;
            //dbTargetPosi[2] = 1878; dbLensPosi[2] = (float)153.39; dbHalldata[2] = 1881;
            //dbTargetPosi[3] = 2617; dbLensPosi[3] = (float)237.24; dbHalldata[3] = 2616;
            //dbTargetPosi[4] = 3356; dbLensPosi[4] = (float)327.03; dbHalldata[4] = 3356;
            //dbTargetPosi[5] = 4095; dbLensPosi[5] = (float)419.10; dbHalldata[5] = 4094;
            //dbTargetPosi[6] = 4834; dbLensPosi[6] = (float)513.74; dbHalldata[6] = 4835;
            //dbTargetPosi[7] = 5573; dbLensPosi[7] = (float)607.23; dbHalldata[7] = 5573;
            //dbTargetPosi[8] = 6312; dbLensPosi[8] = (float)698.70; dbHalldata[8] = 6310;
            //dbTargetPosi[9] = 7051; dbLensPosi[9] = (float)784.50; dbHalldata[9] = 7053;
            //dbTargetPosi[10] = 7790; dbLensPosi[10] = (float)865.20; dbHalldata[10] = 7792;

            int res = coef.LinCompMain(dbTargetPosi, dbLensPosi, dbTargetPosi.Length, pvt, nvt, ignInf, ignMac, ref linCoef, ref resError);
            if (res != 0)
            {
                AddLog(ch, $"Linearity Comp Fail");


                return false;
            }
            Dln.WriteArray(ch, addr, 0xAE, new byte[] { 0x3B });
            Dln.WriteArray(ch, addr, 0x2A, new byte[] { (byte)linCoef[0] });
            Dln.WriteArray(ch, addr, 0x2B, new byte[] { (byte)linCoef[1] });
            Dln.WriteArray(ch, addr, 0x2C, new byte[] { (byte)linCoef[2] });
            Dln.WriteArray(ch, addr, 0x2D, new byte[] { (byte)linCoef[3] });
            Dln.WriteArray(ch, addr, 0x2E, new byte[] { (byte)linCoef[4] });
            Dln.WriteArray(ch, addr, 0x2F, new byte[] { (byte)linCoef[5] });
            Dln.WriteArray(ch, addr, 0x30, new byte[] { (byte)linCoef[6] });
            Dln.WriteArray(ch, addr, 0x31, new byte[] { (byte)linCoef[7] });
            Dln.WriteArray(ch, addr, 0x32, new byte[] { (byte)linCoef[8] });
            Dln.WriteArray(ch, addr, 0x33, new byte[] { (byte)linCoef[9] });
            Dln.WriteArray(ch, addr, 0x34, new byte[] { (byte)linCoef[10] });
            Dln.WriteArray(ch, addr, 0x35, new byte[] { (byte)linCoef[11] });
            Dln.WriteArray(ch, addr, 0x36, new byte[] { (byte)linCoef[12] });

            bool result = DrvIC.AK7326_memory_update(ch, (byte)axis, 0);
            if (!result)
            {
                AddLog(ch, $"Linearity Comp Fail");


                return false;
            }
            Dln.WriteArray(ch, addr, 0x03, new byte[] { 0x01 });
            Wait(200);
            Dln.WriteArray(ch, addr, 0x03, new byte[] { 0x02 });
            Wait(250);
            Dln.WriteArray(ch, addr, 0x03, new byte[] { 0x04 });
            Wait(200);
            Dln.WriteArray(ch, addr, 0x03, new byte[] { 0x08 });
            Wait(200);
            string s = $"0x2A : 0x{linCoef[0].ToString("X")}, 0x2B : 0x{linCoef[1].ToString("X")}, 0x2C : 0x{linCoef[2].ToString("X")}, 0x2D : 0x{linCoef[3].ToString("X")}, 0x2E : 0x{linCoef[4].ToString("X")}\r\n" +
             $"0x2F : 0x{linCoef[5].ToString("X")}, 0x30 : 0x{linCoef[6].ToString("X")}, 0x31 : 0x{linCoef[7].ToString("X")}, 0x32 : 0x{linCoef[8].ToString("X")}, 0x33 : 0x{linCoef[9].ToString("X")}\r\n" +
             $"0x34 : 0x{linCoef[10].ToString("X")}, 0x35 : 0x{linCoef[11].ToString("X")}, 0x36 : 0x{linCoef[12].ToString("X")}";

            AddLog(ch, s);


            Dln.WriteArray(ch, addr, 0xAE, new byte[] { 0x00 });
            LEDs_All_On(0, false);
            AddLog(ch, "<<<  OIS Linearity comp. End  >>>");
            return true;
        }

        void Act_FindBestAFPosition(int ch, string testitem, int InspCnt, bool IsTwice)
        {

            //int[] step = new int[9] { 0, 511, 1023, 1535, 2047, 2559, 3071, 3585, 4095 };
            //int[] hallX = new int[9];
            //int[] hallY = new int[9];

            //Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x02, new byte[] { 0x00 });
            //DrvIC.Move(ch, "AF", 200);
            //Wait(50);
            //DrvIC.Move(ch, "AF", 0);

            //Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0xAE, new byte[] { 0x3B });
            //Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0xAE, new byte[] { 0x3B });

            ////중간 셋팅값 확인 

            ////
            //Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x02, new byte[] { 0x40 });
            //Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x02, new byte[] { 0x40 });

            //Wait(100);

            //for (int i = 0; i < 9; i++)
            //{
            //    int[] tmphallX = new int[6];
            //    int[] tmphallY = new int[6];
            //    DrvIC.Move(ch, "AF", step[i]);
            //    Wait(100);
            //    for (int j = 0; j < 6; j++)
            //    {
            //        tmphallX[j] = DrvIC.ReadHall(ch, "X");
            //        tmphallY[j] = DrvIC.ReadHall(ch, "Y");
            //        hallX[i] += tmphallX[j];
            //        hallY[i] += tmphallY[j];
            //    }
            //    hallX[i] /= 6;
            //    hallY[i] /= 6;

            //    AddLog(ch, $"Pos = {step[i]}, DataX[{i}] = {hallX[i]}, DataY[{i}] = {hallY[i]}");
            //}
            //int xMin = hallX.Min(); int xMax = hallX.Max();
            //int yMin = hallY.Min(); int yMax = hallY.Max();
            //int xCenter = (xMin + xMax) / 2;
            //int yCenter = (yMin + yMax) / 2;
            //int xMinIndex = 0; int yMinIndex = 0;
            //int xMaxIndex = 0; int yMaxIndex = 0;
            //bool XMinFind = false; bool YMinFind = false;
            //bool XMaxFind = false; bool YMaxFind = false;
            //int xBestPos = 0; int yBestPos = 0;
            //for (int i = 0; i < 9; i++)
            //{
            //    if (xMin == hallX[i] && !XMinFind) { XMinFind = true; xMinIndex = i; }
            //    if (xMax == hallX[i] && !XMaxFind) { XMaxFind = true; xMaxIndex = i; }
            //    if (yMin == hallY[i] && !YMinFind) { YMinFind = true; yMinIndex = i; }
            //    if (yMax == hallY[i] && !YMaxFind) { YMaxFind = true; yMaxIndex = i; }
            //}
            //int startXIndex = 0; int endXIndex = 0; int startYIndex = 0; int endYIndex = 0;
            //if (xMinIndex > xMaxIndex)
            //{
            //    startXIndex = xMaxIndex;
            //    endXIndex = xMinIndex;
            //}
            //else
            //{
            //    startXIndex = xMinIndex;
            //    endXIndex = xMaxIndex;
            //}
            //if (yMinIndex > yMaxIndex)
            //{
            //    startYIndex = yMaxIndex;
            //    endYIndex = yMinIndex;
            //}
            //else
            //{
            //    startYIndex = yMinIndex;
            //    endYIndex = yMaxIndex;
            //}
            //string s = $"[MAX/MIN Index] 0, start:{startXIndex}, end:{endXIndex}\r\n" +
            //           $"[MAX/MIN Index] 1, start:{startYIndex}, end:{endYIndex}\r\n" +
            //           $"X Min : {xMin}, X Max : {xMax} ({xMax - xMin})\r\n" +
            //           $"Y Min : {yMin}, Y Max : {yMax} ({yMax - yMin})\r\n" +
            //           $"X Center :{xCenter}, Y Center : {yCenter}\r\n";
            //AddLog(ch, s);

            //for (int i = startXIndex; i <= endXIndex; i++)
            //{
            //    if (i == 0) continue;
            //    if (hallX[i - 1] <= xCenter && hallX[i] >= xCenter || hallX[i - 1] >= xCenter && hallX[i] <= xCenter)
            //    {

            //        xBestPos = (int)(step[i - 1] + (step[i] - step[i - 1]) * (xCenter - hallX[i - 1]) / (hallX[i] - hallX[i - 1]));


            //        break;
            //    }
            //}
            //for (int i = startYIndex; i <= endYIndex; i++)
            //{
            //    if (i == 0) continue;
            //    if (hallY[i - 1] <= yCenter && hallY[i] >= yCenter || hallY[i - 1] >= yCenter && hallY[i] <= yCenter)
            //    {
            //        yBestPos = (int)(step[i - 1] + (step[i] - step[i - 1]) * (yCenter - hallY[i - 1]) / (hallY[i] - hallY[i - 1]));

            //        break;
            //    }
            //}
            //AddLog(ch, $"X_AF : {xBestPos}, Y_AF : {yBestPos}");
            //if (xMax - xMin > yMax - yMin)
            //    BestAFPos = xBestPos;
            //else BestAFPos = yBestPos;
            //AddLog(ch, $"Chosen Best AF : {BestAFPos}");
        }

        void Act_OISHallCalubration(int ch, string testItem, int InspCnt)
        {
            byte[] rbuf = new byte[1];
            AddLog(ch, "");
            AddLog(ch, "<<<  OIS Hall Calibration Start  >>>");
            DrvIC.AK7326_IC_Data(ch);
            DrvIC.AFOnOff(ch, true);
            DrvIC.AFMove(ch, BestAFPos);
            AddLog(ch, $"Move AF Best Position : {BestAFPos}");

            AddLog(ch, $"Auto calibration");
            for (int i = 0; i < 2; i++)
            {
                //int slaveAddr = i == 0 ? DrvIC.XSlaveAddr : DrvIC.Y1SlaveAddr;
                int slaveAddr = i == 0 ? DrvIC.XSlaveAddr : DrvIC.Y1SlaveAddr;
                string pidPath = i == 0 ? Current.XPidPath : Current.YPidPath;
                int index = i == 0 ? 1 : 2;
                Dln.WriteArray(ch, slaveAddr, 0x02, new byte[] { 0x40 });
                Dln.WriteArray(ch, slaveAddr, 0xAE, new byte[] { 0x3B });

                //File 방식으로 변경 ================================================================================

                //====================================================================================================
                Load_OISPID(ch, slaveAddr, pidPath);
                //=======================================================================================================
                /*
                for (int j = 0; j < OIS_Set.Count; j++)
                    Dln.WriteArray(ch, slaveAddr, OIS_Set[j][0], new byte[] { OIS_Set[j][index] });

                for (int j = 0; j < OIS_reg.Count; j++)
                    Dln.WriteArray(ch, slaveAddr, OIS_reg[j][0], new byte[] { OIS_reg[j][index] });

                for (int j = 0; j < OISPID.Count; j++)
                    Dln.WriteArray(ch, slaveAddr, OISPID[j][0], new byte[] { OISPID[j][index] });
                */
                DrvIC.AK7326_IC_Mode(ch, 0, 0);
                DrvIC.AK7326_IC_Mode(ch, 1, 0);
                Wait(50);
                //Cal Instruction??
                for (int j = 0; j < 3; j++)
                {
                    Dln.WriteArray(ch, slaveAddr, 0x02, new byte[] { 0x09 });
                    Wait(220);
                }
                Dln.WriteArray(ch, slaveAddr, 0x19, new byte[] { 0x8C });
                //Dln.WriteArray(ch, slaveAddr, 0x19, new byte[] { 0x88 });
                //Dln.WriteArray(ch, slaveAddr, 0x5D, new byte[] { 0x68 });
                byte[] calData = new byte[2];
                Dln.ReadArray(ch, slaveAddr, 0x04, rbuf);
                calData[0] = rbuf[0];
                Dln.ReadArray(ch, slaveAddr, 0x06, rbuf);
                calData[1] = rbuf[0];
                if (((calData[0] < 0x7F) && (calData[1] > 0x7F)) || ((calData[0] > 0x7F) && (calData[1] < 0x7F)))
                {
                    if (i == 0) AddLog(ch, $"OIS Cal X -> {calData[0].ToString("X2")}, {calData[1].ToString("X2")} OK");
                    else AddLog(ch, $"OIS Cal Y -> {calData[0].ToString("X2")}, {calData[1].ToString("X2")} OK");

                }
                else
                {
                    if (i == 0) AddLog(ch, $"OIS Cal X -> {calData[0].ToString("X2")}, {calData[1].ToString("X2")} NG");
                    else AddLog(ch, $"OIS Cal Y -> {calData[0].ToString("X2")}, {calData[1].ToString("X2")} NG");
                }

                // Store
                Dln.WriteArray(ch, slaveAddr, 0x03, new byte[] { 0x01 }); Wait(170);
                Dln.WriteArray(ch, slaveAddr, 0x03, new byte[] { 0x02 }); Wait(270);
                Dln.WriteArray(ch, slaveAddr, 0x03, new byte[] { 0x04 }); Wait(170);
                Dln.WriteArray(ch, slaveAddr, 0x03, new byte[] { 0x08 }); Wait(120);
                Dln.WriteArray(ch, slaveAddr, 0x03, new byte[] { 0x10 }); Wait(70);
                Dln.WriteArray(ch, slaveAddr, 0xAE, new byte[] { 0x00 });
                DrvIC.AK7326_IC_Mode(ch, 0, 1);
                DrvIC.AK7326_IC_Mode(ch, 1, 1);
            }

            Dln.ReadArray(ch, DrvIC.XSlaveAddr, 0x03, rbuf);
            byte check_3f_x = rbuf[0];
            Dln.ReadArray(ch, DrvIC.Y1SlaveAddr, 0x03, rbuf);
            byte check_3f_y = rbuf[0];
            AddLog(ch, $"Need to check 0x3F : {check_3f_x.ToString("X2")}, {check_3f_y.ToString("X2")}");
            if (check_3f_x != 0x85 || check_3f_y != 0x85)
            {
                PassFails[0].Results[(int)SpecItem.XYHallCalibration].Val = 10;
                ShowDataResults(ch, (int)SpecItem.XYHallCalibration, (int)SpecItem.XYHallCalibration, InspType.OKNG, new double[] { });
                //AddLog(ch, "0x3F register, wrong parameter");
                //SetError(ch, NonSpecItem.OIS_HallCalibration);              
                return;
            }
            else
            {
                PassFails[0].Results[(int)SpecItem.XYHallCalibration].Val = 0;
                ShowDataResults(ch, (int)SpecItem.XYHallCalibration, (int)SpecItem.XYHallCalibration, InspType.OKNG, new double[] { });
            }
            DrvIC.Move(ch, "X", OISCenter);
            DrvIC.Move(ch, "Y", OISCenter);
            DrvIC.OISOn(ch, "X", true);
            DrvIC.OISOn(ch, "Y", true);
            AddLog(ch, "<<<  OIS Hall Calibration End  >>>");
        }

        public int FindPhaseIndex(List<double> gain)
        {
            bool isNeg = false;
            for (int i = 0; i < gain.Count; i++)
            {
                if (gain[i] >= 0 && !isNeg)
                {
                    continue;
                }
                isNeg = true;
                if (gain[i] >= 0)
                {
                    if (i == 0) return 0;
                    return i - 1;
                }
            }
            return gain.Count - 1;
        }
        //public int FindGainIndex(List<double> phase)
        //{
        //    for (int i = 0; i < phase.Count; i++)
        //    {
        //        if (phase[i] >= 0)
        //        {
        //            if (i == 0) return 0;
        //            return i - 1;
        //        }
        //    }
        //    return 0;
        //}
        //private void Act_Gain_Margin(int ch, string testItem)
        //{
        //    string axis;
        //    int startFreq;
        //    int EndFreq;
        //    int amp;

        //    DrvIC.OISOn(ch, testItem, false);
        //    //X
        //    axis = "X";
        //    startFreq = Condition.iXGainFrom;
        //    EndFreq = Condition.iXGainTo;
        //    amp = (int)Condition.iXAmplitudeGain;

        //    AddLog(ch, string.Format("{0} FRA ==", axis));

        //    List<double> freq = new List<double>();
        //    List<double> gain = new List<double>();
        //    List<double> phase = new List<double>();

        //    for (int i = 0; i < Condition.iGainLoop; i++)
        //    {
        //        while (true)
        //        {
        //            freq.Add(startFreq);
        //            startFreq -= Condition.iGainStep;
        //            if (startFreq < EndFreq) break;

        //        }
        //    }
        //    if (!DrvIC.FRA_Single(ch, axis, amp, 1, freq, ref gain, ref phase))
        //    {
        //        errMsg[ch] = string.Format("{0} Error", testItem);
        //        m_ChannelOn[ch] = false;
        //    }
        //    int gainIndex = FindGainIndex(phase);
        //    if (gainIndex < 1)
        //    {
        //        AddLog(ch, "X Find Gain Margin Failed.. Freq Range Check Please.");
        //        errMsg[ch] = string.Format("{0} Error", testItem);
        //        m_ChannelOn[ch] = false;
        //    }
        //    else
        //    {
        //        AddLog(ch, string.Format("FRA X GM = {0}", PassFails[ch].Results[(int)SpecItem.FRAX_GainMargin].Val = Math.Abs(gain[gainIndex])));
        //        SetResult(ch, (int)SpecItem.FRAX_GainMargin, (int)SpecItem.FRAX_GainMargin);
        //        ShowDataResults(ch, "FRA X", (int)SpecItem.FRAX_GainMargin, (int)SpecItem.FRAX_GainMargin);
        //    }

        //    //Y1
        //    axis = "Y1";
        //    startFreq = Condition.iYGainFrom;
        //    EndFreq = Condition.iYGainTo;
        //    amp = (int)Condition.iYAmplitudeGain;
        //    AddLog(ch, string.Format("{0} FRA ==", axis));

        //    gain = new List<double>();
        //    phase = new List<double>();

        //    if (!DrvIC.FRA_Single(ch, axis, amp, 1, freq, ref gain, ref phase))
        //    {
        //        errMsg[ch] = string.Format("{0} Error", testItem);
        //        m_ChannelOn[ch] = false;
        //    }
        //    gainIndex = FindGainIndex(phase);
        //    if (gainIndex < 1)
        //    {
        //        AddLog(ch, "Y1 Find Gain Margin Failed.. Freq Range Check Please.");
        //        errMsg[ch] = string.Format("{0} Error", testItem);
        //        m_ChannelOn[ch] = false;
        //    }
        //    else
        //    {
        //        AddLog(ch, string.Format("FRA Y1 GM = {0}", PassFails[ch].Results[(int)SpecItem.FRAY1_GainMargin].Val = Math.Abs(gain[gainIndex])));

        //        SetResult(ch, (int)SpecItem.FRAY1_PMFreq, (int)SpecItem.FRAY1_GainMargin);
        //        ShowDataResults(ch, "FRA Y1", (int)SpecItem.FRAY1_PMFreq, (int)SpecItem.FRAY1_GainMargin);
        //    }

        //    //Y2
        //    //axis = "Y2";
        //    //AddLog(ch, string.Format("{0} FRA ==", axis));

        //    //gain = new List<double>();
        //    //phase = new List<double>();

        //    //if (!DrvIC.FRA_Single(ch, axis, amp, freq, ref gain, ref phase))
        //    //{
        //    //    errMsg[ch] = string.Format("{0} Error", testItem);
        //    //    m_ChannelOn[ch] = false;
        //    //}
        //    //gainIndex = FindGainIndex(phase);
        //    //if (gainIndex < 1)
        //    //{
        //    //    AddLog(ch, "Y2 Find Gain Margin Failed.. Freq Range Check Please.");
        //    //    errMsg[ch] = string.Format("{0} Error", testItem);
        //    //    m_ChannelOn[ch] = false;
        //    //}
        //    //else
        //    //{

        //    //    AddLog(ch, string.Format("FRA Y2 GM = {0}", Spec.PassFails[ch].Results[(int)SpecItem.FRAY2_GainMargin].Val = Math.Abs(gain[gainIndex])));

        //    //    Spec.SetResult(ch, (int)SpecItem.FRAY2_GainMargin, (int)SpecItem.FRAY2_GainMargin);
        //    //    ShowDataResults(ch, "FRA Y2");
        //    //}
        //}

        public void ServoDecenter(int ch, string name, int InspCnt)
        {
            AddLog(ch, "<<<  OIS Servo Decenter Start  >>>");


            FindResult[] fX = new FindResult[2] { new FindResult(), new FindResult() };
            FindResult[] fY = new FindResult[2] { new FindResult(), new FindResult() };

            LEDs_All_On(0, true);

            DrvIC.AFOnOff(ch, true);
            DrvIC.AFMove(ch, BestAFPos);
            Wait(300);
            AddLog(ch, $"AF Position : {DrvIC.ReadAFHall(ch)}");

            DrvIC.OISOn(0, "X", true);
            DrvIC.OISOn(0, "Y", true);

            DrvIC.Move(0, "X", OISCenter);
            DrvIC.Move(0, "Y", OISCenter);

            Wait(200);
            fX[0] = Measure();

            DrvIC.OISOn(0, "X", false);
            DrvIC.OISOn(0, "Y", false);
            Wait(Condition.ServoDecenterDelay);

            fX[1] = Measure();
            PassFails[0].Results[(int)SpecItem.x_ServoDecenter].Val = fX[1].cx[0] - fX[0].cx[0];
            AddLog(ch, $"Decenter X = {(fX[1].cx[0] - fX[0].cx[0]).ToString("F2")}");
            AddLog(ch, "<<<  OIS X Servo Decenter End  >>>");
            AddLog(ch, "");
            AddLog(ch, "<<<  OIS Y Servo Decenter Start  >>>");

            DrvIC.AFMove(ch, BestAFPos);
            Wait(100);
            AddLog(ch, $"AF Position : {DrvIC.ReadAFHall(ch)}");


            DrvIC.OISOn(0, "X", true);
            DrvIC.OISOn(0, "Y", true);

            DrvIC.Move(0, "X", OISCenter);
            DrvIC.Move(0, "Y", OISCenter);

            Wait(200);
            fY[0] = Measure();

            DrvIC.OISOn(0, "X", false);
            DrvIC.OISOn(0, "Y", false);
            Wait(Condition.ServoDecenterDelay);
            fY[1] = Measure();

            PassFails[0].Results[(int)SpecItem.y_ServoDecenter].Val = fY[0].cy[0] - fY[1].cy[0];
            ShowDataResults(0, (int)SpecItem.x_ServoDecenter, (int)SpecItem.y_ServoDecenter, InspType.Normal, new double[] { });
            AddLog(ch, $"Decenter Y = {(fY[0].cy[0] - fY[1].cy[0]).ToString("F2")}");
            LEDs_All_On(0, false);
            AddLog(ch, "<<<  OIS Y Servo Decenter End  >>>");

            PowerSequence(ch);
            AddLog(ch, $"PowerSequence Off & On");
        }


        void AutoTest(int ch, string testItem, int InspCnt)
        {

            try
            {
                byte METM = 0, WSEC = 0;
                byte result = 0;
                int sinetest_X = 0, sinetest_Y = 0;
                int ringing_X = 0, ringing_Y = 0;
                int sinetest_X1 = 0, sinetest_Y1 = 0;
                bool result_x1, result_y1, result_x2, result_y2;

                byte sine_result = 0, ringing_result = 0;
                byte sine_result_2nd;
                byte[] rbuf = new byte[1];
                byte[] rbuf2 = new byte[2];

                AddLog(ch, "<<<  OIS Auto test Start  >>>");
                for (int i = 0; i < 3; i++)
                    Dln.WriteArray(ch, DrvIC.FRA_Addr, 0xAF, new byte[] { 0xCE });
                DrvIC.AK7326_PM_set_slave(ch, 2);
                AddLog(ch, "AK7326 Autotest get started.");
                AddLog(ch, "Sinewave Test Error Count Spec = 0");
                AddLog(ch, "Ringing Test Stabilize Time Spec = 100");
                Dln.WriteArray(ch, DrvIC.AF_Addr, 0x02, new byte[] { 0x00 });
                DrvIC.AFMove(ch, 2048);
                Wait(100);
                AddLog(ch, $"AF Pos : {DrvIC.ReadAFHall(ch)}");


                AddLog(ch, "<<<  OIS Auto test End  >>>");
                for (int i = 0; i < 5; i++)
                {
                    Dln.WriteArray(ch, DrvIC.FRA_Addr, 0xAD, new byte[] { 0x53 });
                    Wait(2);
                    Dln.WriteArray(ch, DrvIC.FRA_Addr, 0x60, new byte[] { (byte)Condition.AutoTest_THD });
                    Dln.WriteArray(ch, DrvIC.FRA_Addr, 0x61, new byte[] { 0 });
                    Dln.WriteArray(ch, DrvIC.FRA_Addr, 0x62, new byte[] { 5 });
                    Dln.WriteArray(ch, DrvIC.FRA_Addr, 0x63, new byte[] { (byte)Condition.AutoTest_AMP });
                    Dln.WriteArray(ch, DrvIC.FRA_Addr, 0x3E, new byte[] { (byte)Condition.AutoTest_AMP });
                    Dln.WriteArray(ch, DrvIC.FRA_Addr, 0x64, new byte[] { 18 });

                    Dln.WriteArray(ch, DrvIC.FRA_Addr, 0xA8, new byte[] { 0xC5 });
                    Wait(700);
                    Dln.WriteArray(ch, DrvIC.FRA_Addr, 0xA8, new byte[] { 0x00 });
                    Wait(1);
                    Dln.WriteArray(ch, DrvIC.FRA_Addr, 0xAD, new byte[] { 0x00 });
                    Wait(2);

                    Dln.ReadArray(ch, DrvIC.FRA_Addr, 0x6E, rbuf);
                    sine_result = (byte)(0x0F & rbuf[0]);
                    if (sine_result == 0x00) { AddLog(ch, $"index : {i}"); break; }

                }
                Dln.ReadArray(ch, DrvIC.FRA_Addr, 0x9A, rbuf);
                sinetest_X1 = rbuf[0];
                Dln.ReadArray(ch, DrvIC.FRA_Addr, 0x9B, rbuf);
                sinetest_Y1 = rbuf[0];
                Dln.ReadArray(ch, DrvIC.FRA_Addr, 0xE4, rbuf2);
                SinewaveXMaxDiff = sinetest_X = ((rbuf2[0] << 8) + rbuf2[1]) >> 4;
                Dln.ReadArray(ch, DrvIC.FRA_Addr, 0xE6, rbuf2);
                SinewaveYMaxDiff = sinetest_Y = ((rbuf2[0] << 8) + rbuf2[1]) >> 4;

                if (sine_result != 0)
                {
                    AddLog(ch, $"Error flag : 0x{sine_result.ToString("X2")}");
                    AddLog(ch, $"Sinetest NG Error Count - x-diff : {sinetest_X1}, y-diff : {sinetest_Y1}");
                    AddLog(ch, $"Sinetest NG Max Diff - x-diff : {sinetest_X}, y-diff : {sinetest_Y}");
                }
                else
                {
                    AddLog(ch, $"Sinetest Error Count - x-diff : {sinetest_X1}, y-diff : {sinetest_Y1}");
                    AddLog(ch, $"Sinetest Max Diff - x-diff : {sinetest_X}, y-diff : {sinetest_Y}");
                    AddLog(ch, $"Sinewave is passed : 0x{sine_result.ToString("X2")}");
                }
                Dln.WriteArray(ch, DrvIC.FRA_Addr, 0xAF, new byte[] { 0xCE });

                METM = 100;
                WSEC = 50;

                for (int i = 0; i < 5; i++)
                {
                    Dln.WriteArray(ch, DrvIC.FRA_Addr, 0xAD, new byte[] { 0x23 });
                    Wait(2);
                    Dln.WriteArray(ch, DrvIC.FRA_Addr, 0x65, new byte[] { (byte)Condition.AutoTest_ErrTHD });
                    Dln.WriteArray(ch, DrvIC.FRA_Addr, 0x66, new byte[] { (byte)Condition.AutoTest_InitPos });
                    Dln.WriteArray(ch, DrvIC.FRA_Addr, 0x68, new byte[] { METM });
                    Dln.WriteArray(ch, DrvIC.FRA_Addr, 0x69, new byte[] { WSEC });

                    Dln.WriteArray(ch, DrvIC.FRA_Addr, 0xA8, new byte[] { 0xC5 });
                    Wait(250);
                    Dln.WriteArray(ch, DrvIC.FRA_Addr, 0xA8, new byte[] { 0x00 });
                    Wait(1);
                    Dln.WriteArray(ch, DrvIC.FRA_Addr, 0xAD, new byte[] { 0x00 });
                    Wait(2);
                    Dln.ReadArray(ch, DrvIC.FRA_Addr, 0x6E, rbuf);
                    ringing_result = (byte)(0x0F & rbuf[0]);
                    if (ringing_result == 0x00) { AddLog(ch, $"index : {i}"); break; }

                }

                Dln.ReadArray(ch, DrvIC.FRA_Addr, 0x9C, rbuf);
                RingingXStabilizer = ringing_X = (METM + WSEC) - rbuf[0];
                Dln.ReadArray(ch, DrvIC.FRA_Addr, 0x9D, rbuf);
                RingingYStabilizer = ringing_Y = (METM + WSEC) - rbuf[0];

                if (ringing_result != 0)
                {
                    AddLog(ch, $"Error flag : 0x{ringing_result.ToString("X2")}");
                    AddLog(ch, $"Ringing NG Time - X : {ringing_X}, Y : {ringing_Y}");
                }
                else
                {

                    AddLog(ch, $"Ringing NG Time - X : {ringing_X}, Y : {ringing_Y}");
                    AddLog(ch, $"Ringing test is passed : 0x{ringing_result.ToString("X2")}");
                }
                Dln.WriteArray(ch, DrvIC.FRA_Addr, 0xAF, new byte[] { 0xCE });

                PassFails[ch].Results[(int)SpecItem.AutoTestRes].Val = sine_result + ringing_result;
                ShowDataResults(ch, (int)SpecItem.AutoTestRes, (int)SpecItem.AutoTestRes, InspType.Normal, new double[] { });

                if (Option.SaveRawData)
                {
                    StreamWriter sw = null;
                    string dateDir = STATIC.CreateDateDir();
                    if (!Directory.Exists(dateDir)) Directory.CreateDirectory(dateDir);
                    string path = dateDir + $"OIS_AutoTest.csv";

                    if (!File.Exists(path))
                    {
                        sw = File.AppendText(path);
                        string s = $"SPL No, Date, Time, SINE_NG_cnt_X, SINE_NG_cnt_Y, SINE_Diff_Max_X, SINE_Diff_Max_Y, RNG_NG_cnt_X, RNG_NG_cnt_Y,";
                        sw.WriteLine(s);
                        sw.Close();
                    }
                    sw = File.AppendText(path);
                    string dt = $"{m_StrIndex[ch]},{STATIC.LogDate.ToString("yyyy-MM-dd")},{STATIC.LogDate.Hour}h{STATIC.LogDate.Minute}m{STATIC.LogDate.Second}s," +
                        $"{sinetest_X1},{sinetest_Y1},{SinewaveXMaxDiff},{SinewaveYMaxDiff},{ringing_X},{ringing_Y}";
                    sw.WriteLine(dt);
                    sw.Close();
                }

            }
            catch
            {
                PassFails[ch].Results[(int)SpecItem.AutoTestRes].Val = 1;
                ShowDataResults(ch, (int)SpecItem.AutoTestRes, (int)SpecItem.AutoTestRes, InspType.Normal, new double[] { });
                if (!Dln.WriteArray(ch, DrvIC.FRA_Addr, 0xAF, new byte[] { 0xCE })) return;

            }
        }
        void OISSensitivityTest(int ch, string testItem, int InspCnt)
        {

            int[] xCode = new int[] { 2048, 0, 4095, 0, 4095 };
            int[] yCode = new int[] { 2048, 0, 0, 4095, 4095 };
            byte[] rbuf = new byte[1];

            List<byte> xVal = new List<byte>();
            List<byte> yVal = new List<byte>();
            //List<int> xHall = new List<int>();
            //List<int> yHall = new List<int>();
            List<int> checkRegX = new List<int>();
            List<int> checkRegY = new List<int>();

            //Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x02, new byte[] { 0x00 });
            //Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x00, new byte[] { 0x80, 0x00 });
            //Wait(100);
            //Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x02, new byte[] { 0x00 });
            //Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x02, new byte[] { 0x00 });

            for (int i = 0; i < xCode.Length; i++)
            {
                //DrvIC.Move(ch, "X", xCode[i]);
                //DrvIC.Move(ch, "Y", yCode[i]);
                //Wait(Condition.OISSensDelayTime);
                //Dln.ReadArray(ch, DrvIC.XSlaveAddr, 0x79, rbuf);
                xVal.Add(0);
                //Dln.ReadArray(ch, DrvIC.Y1SlaveAddr, 0x79, rbuf);
                yVal.Add(0);
                //xHall.Add(DrvIC.ReadHall(ch, "X"));              
                //yHall.Add(DrvIC.ReadHall(ch, "Y"));e
                checkRegX.Add(0);
                checkRegY.Add(0);
            }

            for (int i = 0; i < xVal.Count; i++)
            {
                AddLog(ch, $"{i * 2}, 0x{xVal[i].ToString("X2")}, 0x{yVal[i].ToString("X2")} ({xCode[i]}, {yCode[i]})");
                AddLog(ch, $"{i * 2 + 1}, 0x{checkRegX[i].ToString("X2")}, 0x{checkRegY[i].ToString("X2")} ({xCode[i]}, {yCode[i]})");
            }

            PassFails[ch].Results[(int)SpecItem.OISSensitivityTestRes].Val = 1;
            ShowDataResults(ch, (int)SpecItem.OISSensitivityTestRes, (int)SpecItem.OISSensitivityTestRes, InspType.Normal, new double[] { });

            if (Option.SaveRawData)
            {
                StreamWriter sw = null;
                string dateDir = STATIC.CreateDateDir();
                if (!Directory.Exists(dateDir)) Directory.CreateDirectory(dateDir);
                string path = dateDir + $"OIS_SENS_MODE_CHECK.csv";

                if (!File.Exists(path))
                {
                    sw = File.AppendText(path);
                    string s = $"SPL No, Date, Time, 1_X_MID, 1_Y_MID, 1_XH_MID, 1_YH_MID, 2_X_MIN, 2_Y_MIN, 2_XH_MIN, 2_YH_MIN, " +
                        $"3_X_MAX, 3_Y_MIN, 3_XH_MAX, 3_YH_MIN, 4_X_MIN, 4_Y_MAX, 4_XH_MIN, 4_YH_MAX, 5_X_MAX, 5_Y_MAX, 5_XH_MAX, 5_YH_MAX,";
                    sw.WriteLine(s);
                    sw.Close();
                }
                sw = File.AppendText(path);
                //string dt = $"{m_StrIndex[ch]},{STATIC.LogDate.ToString("yyyy-MM-dd")},{STATIC.LogDate.Hour}h{STATIC.LogDate.Minute}m{STATIC.LogDate.Second}s," +
                //    $"{checkRegX[0]}, {checkRegY[0]}, {xHall[0]}, {yHall[0]}, {checkRegX[1]}, {checkRegY[1]}, {xHall[1]}, {yHall[1]}, {checkRegX[2]}, {checkRegY[2]}, {xHall[2]}, {yHall[2]}," +
                //    $"{checkRegX[3]}, {checkRegY[3]}, {xHall[3]}, {yHall[3]}, {checkRegX[4]}, {checkRegY[4]}, {xHall[4]}, {yHall[4]}";
                //sw.WriteLine(dt);
                sw.Close();
            }
        }
        void IME_Test(int ch, string testItem, int InspCnt)
        {
            try
            {
                bool xres = false;
                bool yres = false;
                AddLog(ch, $"<<<  IME Test Start  >>>");

                PowerSequence(ch);
                AddLog(ch, $"PowerSequence Off & On");
                DrvIC.AK7326_IC_reset(ch);
                DrvIC.AF_IC_Data(ch);
                Wait(50);
                DrvIC.AK7326_IC_reset(ch);
                Wait(50);
                DrvIC.AFOnOff(ch, true);
                DrvIC.OISOn(ch, "X", true);
                DrvIC.OISOn(ch, "Y", true);
                Wait(50);

                int OISStroke = Condition.IMEOISStroke;

                byte[] rbuf = new byte[1];

                byte[] X_PNCAL = new byte[2];
                byte[] Y_PNCAL = new byte[2];

                int XPCAL = 0, XNCAL = 0;
                int YPCAL = 0, YNCAL = 0;
                int XIME = 0, YIME = 0;

                Dln.ReadArray(ch, DrvIC.XSlaveAddr, 0x04, rbuf);
                X_PNCAL[0] = rbuf[0];
                Dln.ReadArray(ch, DrvIC.XSlaveAddr, 0x06, rbuf);
                X_PNCAL[1] = rbuf[0];
                Dln.ReadArray(ch, DrvIC.Y1SlaveAddr, 0x04, rbuf);
                Y_PNCAL[0] = rbuf[0];
                Dln.ReadArray(ch, DrvIC.Y1SlaveAddr, 0x06, rbuf);
                Y_PNCAL[1] = rbuf[0];

                XPCAL = (X_PNCAL[0] < 128) ? (X_PNCAL[0] * 2) : ((X_PNCAL[0] * 2) - 512);
                XNCAL = (X_PNCAL[1] < 128) ? (X_PNCAL[1] * 2) : ((X_PNCAL[1] * 2) - 512);
                YPCAL = (Y_PNCAL[0] < 128) ? (X_PNCAL[0] * 2) : ((X_PNCAL[0] * 2) - 512);
                YNCAL = (Y_PNCAL[1] < 128) ? (Y_PNCAL[1] * 2) : ((Y_PNCAL[1] * 2) - 512);

                XIME = (((OISStroke * XPCAL) / (XPCAL - XNCAL)) - (OISStroke / 2));
                YIME = (((OISStroke * YPCAL) / (YPCAL - YNCAL)) - (OISStroke / 2));

                AddLog(ch, $"Stroke : {OISStroke}, {XIME}, {YIME}");

                if ((XIME < Condition.IMEMinThd) || (XIME > Condition.IMEMaxThd)) // -220 ~ 220
                {
                    AddLog(ch, "X IME Test NG");
                    xres = false;

                }
                else
                {
                    xres = true;

                }
                if ((YIME < Condition.IMEMinThd) || (YIME > Condition.IMEMaxThd)) // -220 ~ 220
                {
                    AddLog(ch, "Y IME Test NG");
                    yres = false;

                }
                else
                {
                    yres = true;

                }

                g_IME[0] = XIME; g_IME[1] = YIME;
                change_autosensitivitymode(ch, XIME, YIME);

                if (xres && yres)
                {
                    PassFails[ch].Results[(int)SpecItem.OISIMERes].Val = 0;
                    ShowDataResults(ch, (int)SpecItem.OISIMERes, (int)SpecItem.OISIMERes, InspType.Normal, new double[] { });
                }
                else
                {
                    PassFails[ch].Results[(int)SpecItem.OISIMERes].Val = 1;
                    ShowDataResults(ch, (int)SpecItem.OISIMERes, (int)SpecItem.OISIMERes, InspType.Normal, new double[] { });
                }


                if (Option.SaveRawData)
                {
                    StreamWriter sw = null;
                    string dateDir = STATIC.CreateDateDir();
                    if (!Directory.Exists(dateDir)) Directory.CreateDirectory(dateDir);
                    string path = dateDir + $"OIS_IC_Mount_Error.csv";

                    if (!File.Exists(path))
                    {
                        sw = File.AppendText(path);
                        string s = $"SPL No, Date, Time, OIS Stroke, X P Reg, X N Reg, Y P Reg, Y N Reg, X PCAL, X NCAL, Y PCAL, Y NCAL, X IME, Y IME";
                        sw.WriteLine(s);
                        sw.Close();
                    }
                    sw = File.AppendText(path);
                    string dt = $"{m_StrIndex[ch]},{STATIC.LogDate.ToString("yyyy-MM-dd")},{STATIC.LogDate.Hour}h{STATIC.LogDate.Minute}m{STATIC.LogDate.Second}s," +
                        $"{OISStroke},{X_PNCAL[0]},{X_PNCAL[1]},{Y_PNCAL[0]},{Y_PNCAL[1]},{XPCAL},{XNCAL},{YPCAL},{YNCAL},{XIME},{YIME}";
                    sw.WriteLine(dt);
                    sw.Close();
                }
                AddLog(ch, $"<<<  IME Test End  >>>");
            }
            catch
            {
                PassFails[ch].Results[(int)SpecItem.OISIMERes].Val = 1;
                ShowDataResults(ch, (int)SpecItem.OISIMERes, (int)SpecItem.OISIMERes, InspType.Normal, new double[] { });
            }


        }
        void change_autosensitivitymode(int ch, int x_ime, int y_ime)
        {
            int imeTH = 130;
            byte[] xbuf = new byte[2];
            byte[] ybuf = new byte[2];
            byte[] rbuf = new byte[1];

            AddLog(ch, $"x_ime : {x_ime}, y_ime : {y_ime}, abs(x_ime) : {Math.Abs(x_ime)}, abs(y_ime) : {Math.Abs(y_ime)}");

            Dln.ReadArray(ch, DrvIC.XSlaveAddr, 0x24, rbuf); xbuf[0] = rbuf[0];
            Dln.ReadArray(ch, DrvIC.XSlaveAddr, 0x25, rbuf); xbuf[1] = rbuf[0];
            Dln.ReadArray(ch, DrvIC.Y1SlaveAddr, 0x24, rbuf); ybuf[0] = rbuf[0];
            Dln.ReadArray(ch, DrvIC.Y1SlaveAddr, 0x25, rbuf); ybuf[1] = rbuf[0];

            AddLog(ch, $"[Before] Auto Sensitivity mode");
            AddLog(ch, $"0x{xbuf[0].ToString("X2")}, 0x{xbuf[1].ToString("X2")}");
            AddLog(ch, $"0x{ybuf[0].ToString("X2")}, 0x{ybuf[1].ToString("X2")}");

            if ((Math.Abs(x_ime) >= imeTH) || (Math.Abs(y_ime) >= imeTH))
            {
                Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0xAE, new byte[] { 0x3B });
                Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0xAE, new byte[] { 0x3B });
                Wait(10);
                Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x24, new byte[] { 0x5A });
                Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x25, new byte[] { 0x22 });
                Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x03, new byte[] { 0x02 });
                Wait(300);
                Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x24, new byte[] { 0x50 });
                Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x25, new byte[] { 0x1E });
                Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x03, new byte[] { 0x02 });
                Wait(300);
                Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0xAE, new byte[] { 0x00 });
                Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0xAE, new byte[] { 0x00 });
            }

            Dln.ReadArray(ch, DrvIC.XSlaveAddr, 0x24, rbuf); xbuf[0] = rbuf[0];
            Dln.ReadArray(ch, DrvIC.XSlaveAddr, 0x25, rbuf); xbuf[1] = rbuf[0];
            Dln.ReadArray(ch, DrvIC.Y1SlaveAddr, 0x24, rbuf); ybuf[0] = rbuf[0];
            Dln.ReadArray(ch, DrvIC.Y1SlaveAddr, 0x25, rbuf); ybuf[1] = rbuf[0];

            AddLog(ch, $"[After] Auto Sensitivity mode");
            AddLog(ch, $"0x{xbuf[0].ToString("X2")}, 0x{xbuf[1].ToString("X2")}");
            AddLog(ch, $"0x{ybuf[0].ToString("X2")}, 0x{ybuf[1].ToString("X2")}");

        }

        bool AK7314_ICReset(int ch)
        {
            byte[] rbuf = new byte[1];
            Dln.WriteArray(ch, DrvIC.AF_Addr, 0x02, new byte[] { 0x40 });
            Wait(50);
            Dln.WriteArray(ch, DrvIC.AF_Addr, 0x03, new byte[] { 0x10 });
            Wait(100);
            Dln.ReadArray(ch, DrvIC.AF_Addr, 0x4B, rbuf);
            if ((byte)(rbuf[0] & 0x04) != 0x00)
            {

                AddLog(ch, "Store fail");
                return false;
            }
            Dln.WriteArray(ch, DrvIC.AF_Addr, 0x02, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.AF_Addr, 0x00, new byte[] { 0x80, 0x00 });
            Wait(50);
            return true;
        }

        void throughFRA_Enable(int ch, int axis)
        {
            byte[] rbuf = new byte[1];
            int addr = axis == 0 ? DrvIC.XSlaveAddr : DrvIC.Y1SlaveAddr;
            int check_count = 0;
            Dln.WriteArray(ch, addr, 0xAE, new byte[] { 0x3B });

            Dln.WriteArray(ch, DrvIC.FRA_Addr, 0x56, new byte[] { 0x80 }); AddLog(ch, $"Write Mem : 0x{0x56:X2}, Data : 0x{0x80:X2}");
            Dln.WriteArray(ch, DrvIC.FRA_Addr, 0xAD, new byte[] { 0x02 }); AddLog(ch, $"Write Mem : 0x{0xAD:X2}, Data : 0x{0x02:X2}");
            Wait(5);
            Dln.WriteArray(ch, DrvIC.FRA_Addr, 0x54, new byte[] { 0x0F }); AddLog(ch, $"Write Mem : 0x{0x54:X2}, Data : 0x{0x0F:X2}");
            Dln.WriteArray(ch, DrvIC.FRA_Addr, 0x55, new byte[] { 0x00 }); AddLog(ch, $"Write Mem : 0x{0x55:X2}, Data : 0x{0x00:X2}");
            Wait(5);

            while (true)
            {
                Dln.ReadArray(ch, DrvIC.FRA_Addr, 0x4C, rbuf);
                Wait(1);
                if ((rbuf[0] & 0x10) == 0x10)
                    break;
                check_count++;
                if (check_count > 100) { AddLog(ch, "FRA Mode change timeout"); }
            }
            AddLog(ch, $"Read Mem : 0x{0x4C:X2}, Data : 0x{rbuf[0]:X2}");
            Dln.WriteArray(ch, DrvIC.FRA_Addr, 0xA8, new byte[] { 0xC5 });
            AddLog(ch, $"Write Mem : 0x{0xA8:X2}, Data : 0x{0xC5:X2}");
            Wait(150);
        }
        void throughFRA_disable(int ch, int axis)
        {
            int addr = axis == 0 ? DrvIC.XSlaveAddr : DrvIC.Y1SlaveAddr;
            Dln.WriteArray(ch, DrvIC.FRA_Addr, 0xA8, new byte[] { 0x00 }); AddLog(ch, $"Write Mem : 0x{0xA8:X2}, Data : 0x{0x00:X2}");
            Dln.WriteArray(ch, DrvIC.FRA_Addr, 0xAF, new byte[] { 0xEE }); AddLog(ch, $"Write Mem : 0x{0xAF:X2}, Data : 0x{0xEE:X2}");
            Wait(5);
            Dln.WriteArray(ch, DrvIC.FRA_Addr, 0xAD, new byte[] { 0xFF }); AddLog(ch, $"Write Mem : 0x{0xAD:X2}, Data : 0x{0xFF:X2}");
            Wait(15);
            Dln.WriteArray(ch, addr, 0xAE, new byte[] { 0x00 }); AddLog(ch, $"Write Mem : 0x{0xAE:X2}, Data : 0x{0x00:X2}");
            DrvIC.AK7326_IC_reset(ch);
        }
        double throughFRA_gain(int ch, int axis)
        {
            string axisName = axis == 0 ? "X" : "Y";
            int addr = axis == 0 ? DrvIC.XSlaveAddr : DrvIC.Y1SlaveAddr;

            DrvIC.AFMove(ch, BestAFPos);
            AddLog(ch, $"AF Best Pos Move = {BestAFPos}");
            Dln.WriteArray(ch, DrvIC.AF_Addr, 0x02, new byte[] { 0x00 });
            AddLog(ch, $"X Pos : {4096} Y Pos : {4096}");
            DrvIC.Move(ch, "X", 4096);
            DrvIC.Move(ch, "Y", 4096);
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x02, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x02, new byte[] { 0x00 });

            Wait(100);

            AddLog(ch, $"{axisName} Test start == ");

            AddLog(ch, $"FRA Slave Addr Set");
            DrvIC.AK7326_PM_set_slave(ch, axis);

            AddLog(ch, $"FRA Setting Mode");
            Dln.WriteArray(ch, addr, 0x02, new byte[] { 0x40 }); AddLog(ch, $"Write Mem : 0x{0x02:X2}, Data : 0x{0x40:X2}");
            Wait(30);
            Dln.WriteArray(ch, addr, 0xAE, new byte[] { 0x3B }); AddLog(ch, $"Write Mem : 0x{0xAE:X2}, Data : 0x{0x3B:X2}");

            int Amp = Condition.ThroughAmp;
            int strtFrq = Condition.ThroughStart;
            int endFrq = Condition.ThroughEnd;

            AddLog(ch, $"FRA Enable Mode");
            throughFRA_Enable(ch, axis);
            AddLog(ch, $"Amp Set : {Amp} ======");
            DrvIC.Set_Amp(ch, Amp);

            AddLog(ch, $"Amp\tFreq\tGain");

            // 스텝을 고정값으로 설정 (예: 15Hz)
            int freqStep = Condition.ThroughStep;
            if (freqStep < 1) freqStep = 1;

            double startGain = 0;
            double maxPositiveDiff = 0; // ★ 0으로 초기화 (전부 낮으면 0 반환)

            int oldFreq = strtFrq;
            int freqVal = strtFrq;

            // Sweep 방향 결정 (시작 주파수가 종료 주파수보다 작으면 증가, 크면 감소)
            int stepDirection = (strtFrq <= endFrq) ? freqStep : -freqStep;

            for (freqVal = strtFrq; (stepDirection > 0 ? freqVal <= endFrq : freqVal >= endFrq); freqVal += stepDirection)
            {
                DrvIC.Set_Freq(ch, freqVal);

                //선형 보상에 의한 Delay 적용 AKM 가이드 
                //delay_1ms(1 / Frequency[n - 1] + Cycle / Frequency[n] + Offset);
                //고정 딜레이
                int delay = (100 / oldFreq) + (500 / freqVal) + 10;
                //int delay = 100;
                Wait(delay);

                oldFreq = freqVal;

                double gain = DrvIC.Get_Gain(ch);
                AddLog(ch, $"{Amp}\t{freqVal}\t{gain:F2}");

                // 시작 주파수에서의 Gain을 기준값으로 기록
                if (freqVal == strtFrq)
                {
                    startGain = gain;
                }
                else
                {
                    // ★ 현재 Gain이 시작 Gain보다 얼마나 높은지 계산
                    double currentDiff = gain - startGain;

                    // 기존에 찾은 최대 상승폭보다 크면 갱신
                    if (currentDiff > maxPositiveDiff)
                    {
                        maxPositiveDiff = currentDiff;
                    }
                }
            }

            AddLog(ch, $"--- Sweep Result ---");
            AddLog(ch, $"Start Freq ({strtFrq}Hz) Base Gain : {startGain:F2}");
            AddLog(ch, $"Max Positive Gain Diff : {maxPositiveDiff:F2}");

            // 종료 시퀀스
            throughFRA_disable(ch, axis);

            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x02, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x02, new byte[] { 0x00 });
            Wait(30);
            Dln.WriteArray(ch, DrvIC.FRA_Addr, 0x00, new byte[] { 0x01 });
            Dln.WriteArray(ch, DrvIC.FRA_Addr, 0x00, new byte[] { 0x00 });
            Wait(30);

            // 산출된 최대 상승폭(Diff) 반환 (모두 기준 이하라면 초기값 0 반환)
            return maxPositiveDiff;
        }

        void throughFRA(int ch, string testItem, int InspCnt)
        {
            double gain = 0;
            gain = throughFRA_gain(ch, 0);
            PassFails[ch].Results[(int)SpecItem.ThroughPeak_X_Gain].Val = gain;
            ShowDataResults(ch, (int)SpecItem.ThroughPeak_X_Gain, (int)SpecItem.ThroughPeak_X_Gain, InspType.OnlyMax, new double[] { });
            gain = throughFRA_gain(ch, 1);
            PassFails[ch].Results[(int)SpecItem.ThroughPeak_Y_Gain].Val = gain;
            ShowDataResults(ch, (int)SpecItem.ThroughPeak_Y_Gain, (int)SpecItem.ThroughPeak_Y_Gain, InspType.OnlyMax, new double[] { });

            PowerSequence(ch);
            AddLog(ch, $"PowerSequence Off & On");
        }

        void OISPhasemargin(int ch, string testItem, int InspCnt)
        {
            double freqPM, pm_val, freqGM, gainMargin = 0;
            (freqPM, pm_val, freqGM, gainMargin) = OISPMGM(ch, 0, Condition.iXChirpFrom, Condition.iXChirpTo, Condition.PMXMinPhase, Condition.PMXGainTH, Condition.iXAmplitude, true, false);
            PassFails[ch].Results[(int)SpecItem.FRAX_PhaseMargin].Val = pm_val;
            ShowDataResults(ch, (int)SpecItem.FRAX_PhaseMargin, (int)SpecItem.FRAX_PhaseMargin, InspType.Normal, new double[] { });

            (freqPM, pm_val, freqGM, gainMargin) = OISPMGM(ch, 1, Condition.iYChirpFrom, Condition.iYChirpTo, Condition.PMYMinPhase, Condition.PMYGainTH, Condition.iYAmplitude, true, false);
            PassFails[ch].Results[(int)SpecItem.FRAY1_PhaseMargin].Val = pm_val;
            ShowDataResults(ch, (int)SpecItem.FRAY1_PhaseMargin, (int)SpecItem.FRAY1_PhaseMargin, InspType.Normal, new double[] { });

            PowerSequence(ch);
            AddLog(ch, $"PowerSequence Off & On");
        }
        void OISGainmargin(int ch, string testItem, int InspCnt)
        {
            double freqPM, pm_val, freqGM, gainMargin = 0;
            (freqPM, pm_val, freqGM, gainMargin) = OISPMGM(ch, 0, Condition.iGMXChirpFrom, Condition.iGMXChirpTo, Condition.GMXMinPhase, Condition.GMXGainTH, Condition.iGMXAmplitude, false, true);
            PassFails[ch].Results[(int)SpecItem.FRAX_GainMargin].Val = Math.Abs(gainMargin);
            ShowDataResults(ch, (int)SpecItem.FRAX_GainMargin, (int)SpecItem.FRAX_GainMargin, InspType.Normal, new double[] { });

            (freqPM, pm_val, freqGM, gainMargin) = OISPMGM(ch, 1, Condition.iGMYChirpFrom, Condition.iGMYChirpTo, Condition.GMYMinPhase, Condition.GMYGainTH, Condition.iGMYAmplitude, false, true);
            PassFails[ch].Results[(int)SpecItem.FRAY1_GainMargin].Val = Math.Abs(gainMargin);
            ShowDataResults(ch, (int)SpecItem.FRAY1_GainMargin, (int)SpecItem.FRAY1_GainMargin, InspType.Normal, new double[] { });

            PowerSequence(ch);
            AddLog(ch, $"PowerSequence Off & On");
        }
        void OISLoopGain(int ch, string testItem, int InspCnt)
        {
            double gain = 0;
            gain = LoopGain(ch, 0);
            PassFails[ch].Results[(int)SpecItem.FRAX_Gain10Hz].Val = gain;
            ShowDataResults(ch, (int)SpecItem.FRAX_Gain10Hz, (int)SpecItem.FRAX_Gain10Hz, InspType.Normal, new double[] { });

            gain = LoopGain(ch, 1);
            PassFails[ch].Results[(int)SpecItem.FRAY1_Gain10Hz].Val = gain;
            ShowDataResults(ch, (int)SpecItem.FRAY1_Gain10Hz, (int)SpecItem.FRAY1_Gain10Hz, InspType.Normal, new double[] { });

            PowerSequence(ch);
            AddLog(ch, $"PowerSequence Off & On");
        }
        double LoopGain(int ch, int axis)
        {
            int addr = axis == 0 ? DrvIC.XSlaveAddr : DrvIC.Y1SlaveAddr;
            int FRAaddr = axis == 0 ? DrvIC.FRA_XSlaveAddr : DrvIC.FRA_Y1SlaveAddr;
            string axisName = axis == 0 ? "X" : "Y";

            //int startFreq = axis == 0 ? Condition.iXChirpFrom : Condition.iYChirpFrom;
            //int finalFreq = axis == 0 ? Condition.iYChirpTo : Condition.iYChirpTo;
            //int minphase = axis == 0 ? Condition.PMXMinPhase : Condition.PMYMinPhase;
            //int gainTH = axis == 0 ? Condition.PMXGainTH : Condition.PMYGainTH;
            int amp = axis == 0 ? (int)Condition.iLoppgainXAmp : (int)Condition.iLoppgainYAmp;

            DrvIC.AFOnOff(ch, true);
            DrvIC.AFMove(ch, BestAFPos);
            AddLog(ch, $"Move AF Best Position : {BestAFPos}");
            Wait(100);
            if (axis == 0)
            {
                Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x02, new byte[] { 0x00 });
                DrvIC.Move(ch, "Y", 4096);
                AddLog(ch, $"Move Y Position : {4096}");
            }
            else
            {
                Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x02, new byte[] { 0x00 });
                DrvIC.Move(ch, "X", 4096);
                AddLog(ch, $"Move X Position : {4096}");
            }

            DrvIC.SetSlaveAddr(ch, FRAaddr);
            double gainVal = 0;

            AddLog(ch, $"{axisName} LoopGain Test start");

            Dln.WriteArray(ch, addr, 0x02, new byte[] { 0x40 });

            Wait(30);
            Dln.WriteArray(ch, addr, 0xAE, new byte[] { 0x3B });
            DrvIC.FRAModeEnable(ch);
            DrvIC.Set_Amp(ch, amp);
            AddLog(ch, $"Amp\tFreq\tGain");

            DrvIC.Set_Freq(ch, 10);
            //Wait(100 + 5000 / 10 + 10);
            Wait(500);
            gainVal = DrvIC.Get_Gain(ch);
            AddLog(ch, $"{amp}, {10}Hz, {gainVal.ToString("F2")}dB");
            DrvIC.FRAModeDisable(ch);

            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x02, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x02, new byte[] { 0x00 });
            Wait(10);

            Dln.WriteArray(ch, DrvIC.FRA_Addr, 0x00, new byte[] { 0x01 });
            Dln.WriteArray(ch, DrvIC.FRA_Addr, 0x00, new byte[] { 0x00 });
            Wait(10);
            return gainVal;



        }
        //enablePM, enableGM 매개변수 추가(기본값 true)
        (double, double, double, double) OISPMGM(int ch, int axis, int startFreq, int finalFreq, int minphase, int gainTH, int amp, bool enablePM = true, bool enableGM = true)
        {
            int addr = axis == 0 ? DrvIC.XSlaveAddr : DrvIC.Y1SlaveAddr;
            int FRAaddr = axis == 0 ? DrvIC.FRA_XSlaveAddr : DrvIC.FRA_Y1SlaveAddr;
            string axisName = axis == 0 ? "X" : "Y";

            DrvIC.AFOnOff(ch, true);
            DrvIC.AFMove(ch, BestAFPos);
            AddLog(ch, $"Move AF Best Position : {BestAFPos}");
            Wait(100);
            if (axis == 0)
            {
                Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x02, new byte[] { 0x00 });
                DrvIC.Move(ch, "Y", 4096);
                AddLog(ch, $"Move Y Position : {4096}");
            }
            else
            {
                Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x02, new byte[] { 0x00 });
                DrvIC.Move(ch, "X", 4096);
                AddLog(ch, $"Move X Position : {4096}");
            }
            Wait(100);

            DrvIC.SetSlaveAddr(ch, FRAaddr);

            // 결과 저장용 변수 초기화 (수행하지 않은 검사는 0으로 반환됨)
            int freqPM = 0, freqGM = 0;
            double finalPM = 0, finalGM = 0;

            string testMode = (enablePM && enableGM) ? "PM & GM" : (enablePM ? "PM Only" : "GM Only");
            AddLog(ch, $"{axisName} {testMode} Test start");

            Dln.WriteArray(ch, addr, 0x02, new byte[] { 0x40 });
            Wait(30);
            Dln.WriteArray(ch, addr, 0xAE, new byte[] { 0x3B });
            DrvIC.FRAModeEnable(ch);
            DrvIC.Set_Amp(ch, amp);

            // ★ 파라미터 입력 순서에 상관없이 고주파(시작)와 저주파(종료)를 명확히 분리
            int maxFreq = Math.Max(startFreq, finalFreq); // 고주파 (시작점)
            int minFreq = Math.Min(startFreq, finalFreq); // 저주파 (종료점)

            // ==========================================================
            // 1. Phase Margin 측정 (High -> Low 방향 Sweep)
            // ==========================================================
            if (enablePM)
            {
                AddLog(ch, $"--- 1. PM Sweep (High->Low) : {maxFreq}Hz -> {minFreq}Hz ---");
                AddLog(ch, $"Amp\tFreq\tGain\tP/M");

                int freqVal, freqTemp = 0, oldFreq;
                double gainVal = 0, phaseVal = 0;
                double[] before_after_zero_gain = new double[2] { 0, 0 };
                int[] before_after_zero_freq = new int[2] { maxFreq, minFreq };
                double prePhase = 0;
                bool foundPM = false;

                // ★ maxFreq에서 시작하여 freqTemp만큼 빼면서 minFreq까지 하강
                for (oldFreq = freqVal = maxFreq; freqVal >= minFreq; freqVal -= freqTemp)
                {
                    DrvIC.Set_Freq(ch, freqVal);
                    var delayTime = 1000 / oldFreq + 5000 / freqVal + 10;
                    Wait(delayTime);
                    oldFreq = freqVal;

                    gainVal = DrvIC.Get_Gain(ch);
                    phaseVal = DrvIC.Get_Phase(ch, 1);

                    AddLog(ch, $"t{amp}\t{freqVal}\t{gainVal:F2}\t{phaseVal:F0}");

                    // ★ 고주파 -> 저주파 스윕 시, Gain은 보통 음수(-)에서 시작하여 점차 증가해 0을 교차합니다.
                    // 따라서 현재 Gain이 0보다 커지는(양수) 순간을 포착합니다.
                    if (gainVal > 0)
                    {
                        if (freqVal != maxFreq && before_after_zero_gain[0] < 0)
                        {
                            // 선형 보간으로 PM 계산
                            finalPM = ((gainVal * prePhase) - (before_after_zero_gain[0] * phaseVal)) / (gainVal - before_after_zero_gain[0]);
                            freqPM = (int)(((gainVal * before_after_zero_freq[0]) - (before_after_zero_gain[0] * freqVal)) / (gainVal - before_after_zero_gain[0]));

                            before_after_zero_freq[1] = freqVal;
                            before_after_zero_gain[1] = gainVal;
                            foundPM = true;

                            AddLog(ch, $"[PM Found] Freq: {freqPM}Hz, Phase Margin: {finalPM:F0}deg");
                            break;
                        }
                    }
                    else
                    {
                        // 0보다 작은(음수) 구간에서는 이전 값으로 계속 기록
                        before_after_zero_freq[0] = freqVal;
                        before_after_zero_gain[0] = gainVal;
                    }

                    prePhase = phaseVal;

                    // 다음 하강 스텝 폭 계산 (비율 방식)
                    freqTemp = freqVal * Condition.iFRAstep / 100;
                    if (freqTemp < 1) freqTemp = 1;
                }

                if (!foundPM)
                {
                    AddLog(ch, "Couldn`t find zero cross point for Phase Margin.");
                    finalPM = 1;
                }
            }


            // ==========================================================
            // 2. Gain Margin 측정 (High -> Low 방향 Sweep)
            // ==========================================================
            if (enableGM)
            {
                AddLog(ch, $"--- 2. GM Sweep (High->Low) : {maxFreq}Hz -> {minFreq}Hz ---");
                AddLog(ch, $"Amp\tFreq\tGain\tP/M");

                int freqVal, freqTemp = 0, oldFreq;
                double gainVal = 0, phaseVal = 0;
                double targetCrossPhase = 0.0;
                double prevPhaseGM = 0, prevGainGM = 0;
                int prevFreqGM = maxFreq;
                bool foundGM = false;

                // ★ GM 역시 maxFreq에서 minFreq 방향으로 하강
                for (oldFreq = freqVal = maxFreq; freqVal >= minFreq; freqVal -= freqTemp)
                {
                    DrvIC.Set_Freq(ch, freqVal);
                    Wait(1000 / oldFreq + 5000 / freqVal + 10);
                    oldFreq = freqVal;

                    gainVal = DrvIC.Get_Gain(ch);
                    phaseVal = DrvIC.Get_Phase(ch, 1);

                    AddLog(ch, $"{amp}\t{freqVal}\t{gainVal:F2}\t{phaseVal:F0}");

                    if (freqVal != maxFreq)
                    {
                        // 위상(Phase)이 목표 교차점(0도)을 통과하는 순간 감지 (방향 무관하게 동작)
                        if ((prevPhaseGM > targetCrossPhase && phaseVal <= targetCrossPhase) ||
                            (prevPhaseGM < targetCrossPhase && phaseVal >= targetCrossPhase))
                        {
                            // 선형 보간 (Phase 기준)
                            double ratio = Math.Abs((targetCrossPhase - prevPhaseGM) / (phaseVal - prevPhaseGM));

                            finalGM = prevGainGM + ratio * (gainVal - prevGainGM);
                            freqGM = (int)(prevFreqGM + ratio * (freqVal - prevFreqGM));

                            foundGM = true;
                            AddLog(ch, $"[GM Found] Freq: {freqGM}Hz, Gain Margin: {finalGM:F2}dB");
                            break;
                        }
                    }

                    prevPhaseGM = phaseVal;
                    prevGainGM = gainVal;
                    prevFreqGM = freqVal;

                    // 다음 하강 스텝 폭 계산 (비율 방식)
                    freqTemp = freqVal * Condition.iFRAstep / 100;
                    if (freqTemp < 1) freqTemp = 1;
                }

                if (!foundGM)
                {
                    AddLog(ch, "Couldn`t find Phase cross point for Gain Margin.");
                }
            }

            // ==========================================================
            // 종료 및 리셋 시퀀스
            // ==========================================================
            DrvIC.FRAModeDisable(ch);
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x02, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x02, new byte[] { 0x00 });

            Dln.WriteArray(ch, DrvIC.FRA_Addr, 0x00, new byte[] { 0x01 });
            Dln.WriteArray(ch, DrvIC.FRA_Addr, 0x00, new byte[] { 0x00 });
            Wait(10);

            return (freqPM, finalPM, freqGM, finalGM);
        }
        (double, double, double, double, double) AFPMGM(int ch, int startFreq, int finalFreq, int minphase, int gainTH, int amp, bool enablePM = true, bool enableGM = true)
        {
            DrvIC.SetSlaveAddr(ch, DrvIC.FRA_AFSlaveAddr);
            Dln.WriteArray(ch, DrvIC.AF_Addr, 0x02, new byte[] { 0x00 });
            Wait(50);

            DrvIC.AFMove(ch, 2048);
            Wait(50);
            AddLog(ch, $"AF PM/GM Code, Target {DrvIC.ReadAFHall(ch)}");

            Dln.WriteArray(ch, DrvIC.AF_Addr, 0x02, new byte[] { 0x40 });
            Wait(50);
            Dln.WriteArray(ch, DrvIC.AF_Addr, 0xAE, new byte[] { 0x3B });

            // ★ 결과 저장용 변수 5개 (수행하지 않은 검사는 0으로 반환됨)
            int freqPM = 0, freqGM = 0;
            double finalPM = 0, finalGM = 0;
            double final4dBPM = 0; // AF 전용 -4dB PM 추적 변수

            string testMode = (enablePM && enableGM) ? "PM & GM" : (enablePM ? "PM Only" : "GM Only");
            AddLog(ch, $"--- AF {testMode} Test Start ---");

            DrvIC.FRAModeEnable(ch);

            // 고주파(시작)와 저주파(종료) 명확히 분리
            int maxFreq = Math.Max(startFreq, finalFreq);
            int minFreq = Math.Min(startFreq, finalFreq);

            // ==========================================================
            // 1. Phase Margin 측정 (High -> Low 방향 Sweep)
            // ==========================================================
            if (enablePM)
            {
                AddLog(ch, $"[PM Sweep] High->Low : {maxFreq}Hz -> {minFreq}Hz");
                AddLog(ch, $"Amp\tFreq\tGain\tP/M");

                DrvIC.Set_Amp(ch, amp);

                int freqVal, freqTemp = 0, oldFreq;
                double gainVal = 0, phaseVal = 0;
                double preGain = 0, prePhase = 0;
                int preFreq = maxFreq;
                bool foundPM = false, found4dBPM = false;

                for (oldFreq = freqVal = maxFreq; freqVal >= minFreq; freqVal -= freqTemp)
                {
                    DrvIC.Set_Freq(ch, freqVal);
                    Wait(1000 / oldFreq + 5000 / freqVal + 15);
                    oldFreq = freqVal;

                    gainVal = DrvIC.Get_Gain(ch);
                    phaseVal = DrvIC.Get_Phase(ch, 1);

                    AddLog(ch, $"{amp}\t{freqVal}\t{gainVal:F2}\t{phaseVal:F0}");

                    if (freqVal != maxFreq)
                    {
                        // (1) Gain이 0dB를 교차하는 순간 (PM)
                        if (!foundPM && ((preGain < 0 && gainVal >= 0) || (preGain > 0 && gainVal <= 0)))
                        {
                            double ratio = Math.Abs((0 - preGain) / (gainVal - preGain));
                            finalPM = prePhase + ratio * (phaseVal - prePhase);
                            freqPM = (int)(preFreq + ratio * (freqVal - preFreq));
                            foundPM = true;
                            AddLog(ch, $"[PM Found] Freq: {freqPM}Hz, Phase Margin: {finalPM:F0}deg");
                        }

                        // (2) Gain이 -4dB를 교차하는 순간 (-4dB PM)
                        if (!found4dBPM && ((preGain <= -4 && gainVal > -4) || (preGain >= -4 && gainVal < -4)))
                        {
                            double ratio = Math.Abs((-4 - preGain) / (gainVal - preGain));
                            final4dBPM = prePhase + ratio * (phaseVal - prePhase);
                            found4dBPM = true;
                            AddLog(ch, $"[-4dB PM Found] Phase Margin: {final4dBPM:F0}deg");
                        }
                    }

                    if (foundPM && found4dBPM) break; // 둘 다 찾았으면 조기 종료

                    preGain = gainVal;
                    prePhase = phaseVal;
                    preFreq = freqVal;

                    freqTemp = freqVal * Condition.iAFFRAstep / 100;
                    if (freqTemp < 1) freqTemp = 1;
                }

                if (!foundPM) AddLog(ch, "Error : Couldn't find 0dB cross point for Phase Margin.");
            }

            // ==========================================================
            // 2. Gain Margin 측정 (High -> Low 방향 Sweep)
            // ==========================================================
            if (enableGM)
            {
                AddLog(ch, $"[GM Sweep] High->Low : {maxFreq}Hz -> {minFreq}Hz");
                AddLog(ch, $"Amp\tFreq\tGain\tP/M");

                DrvIC.Set_Amp(ch, amp);

                int freqVal, freqTemp = 0, oldFreq;
                double gainVal = 0, phaseVal = 0;
                double targetCrossPhase = 0.0;
                double preGain = 0, prePhase = 0;
                int preFreq = maxFreq;
                bool foundGM = false;

                for (oldFreq = freqVal = maxFreq; freqVal >= minFreq; freqVal -= freqTemp)
                {
                    DrvIC.Set_Freq(ch, freqVal);
                    Wait(1000 / oldFreq + 5000 / freqVal + 10);
                    oldFreq = freqVal;

                    gainVal = DrvIC.Get_Gain(ch);
                    phaseVal = DrvIC.Get_Phase(ch, 1);

                    AddLog(ch, $"{amp}\t{freqVal}\t{gainVal:F2}\t{phaseVal:F0}");

                    if (freqVal != maxFreq)
                    {
                        // Phase가 0도를 교차하는 순간 (GM)
                        if ((prePhase > targetCrossPhase && phaseVal <= targetCrossPhase) ||
                            (prePhase < targetCrossPhase && phaseVal >= targetCrossPhase))
                        {
                            double ratio = Math.Abs((targetCrossPhase - prePhase) / (phaseVal - prePhase));
                            finalGM = preGain + ratio * (gainVal - preGain);
                            freqGM = (int)(preFreq + ratio * (freqVal - preFreq));
                            foundGM = true;
                            AddLog(ch, $"[GM Found] Freq: {freqGM}Hz, Gain Margin: {finalGM:F2}dB");
                            break;
                        }
                    }

                    preGain = gainVal;
                    prePhase = phaseVal;
                    preFreq = freqVal;

                    freqTemp = freqVal * Condition.AFGMStep / 100;
                    if (freqTemp < 1) freqTemp = 1;
                }

                if (!foundGM) AddLog(ch, "Error : Couldn't find Phase cross point for Gain Margin.");
            }

            // ==========================================================
            // 종료 및 리셋 시퀀스
            // ==========================================================
            AddLog(ch, "--- Sweep Result (Use Linear Interpolation) ---");
            if (enablePM) AddLog(ch, $"Phase Margin: {finalPM:F0}deg, -4dB Phase Margin: {final4dBPM:F0}deg");
            if (enableGM) AddLog(ch, $"Gain Margin: {finalGM:F2}dB");

            DrvIC.FRAModeDisable(ch);
            Dln.WriteArray(ch, DrvIC.AF_Addr, 0xAE, new byte[] { 0x00 });
            AK7314_ICReset(ch);

            PowerSequence(ch);
            AddLog(ch, $"PowerSequence Off & On");

            // ★ 5개의 값을 튜플로 반환 (GM 값은 절댓값으로 치환)
            return (freqPM, finalPM, final4dBPM, freqGM, Math.Abs(finalGM));
        }

        void AFGainMargin(int ch, string testItem, int InspCnt)
        {
            // 5개의 결과값을 받을 변수 선언
            double freqPM, pm_val, pm_4db_val, freqGM, gainMargin;

            // GM 검사 수행 (enablePM = false, enableGM = true)
            (freqPM, pm_val, pm_4db_val, freqGM, gainMargin) = AFPMGM(ch, Condition.AFGMStartFreq, Condition.AFGMEndFreq, 0, 0, Condition.AFGMamp, false, true);

            // 결과 저장 및 UI 갱신
            PassFails[ch].Results[(int)SpecItem.FRAAF_GainMargin].Val = gainMargin;
            ShowDataResults(ch, (int)SpecItem.FRAAF_GainMargin, (int)SpecItem.FRAAF_GainMargin, InspType.Normal, new double[] { });
        }
        void AFPhaseMargin(int ch, string testItem, int InspCnt)
        {
            // 5개의 결과값을 받을 변수 선언
            double freqPM, pm_val, pm_4db_val, freqGM, gainMargin;

            // PM 검사 수행 (enablePM = true, enableGM = false)
            (freqPM, pm_val, pm_4db_val, freqGM, gainMargin) = AFPMGM(ch, Condition.iAFChirpFrom, Condition.iAFChirpTo, 0, Condition.PMAFGainTH, (int)Condition.iAFAmplitude, true, false);

            // 결과 저장 및 UI 갱신 (PM과 -4dB PM 둘 다 저장)
            PassFails[ch].Results[(int)SpecItem.FRAAF_PhaseMargin].Val = pm_val;
            PassFails[ch].Results[(int)SpecItem.FRAAF_4dB_PhaseMargin].Val = pm_4db_val;
            ShowDataResults(ch, (int)SpecItem.FRAAF_PhaseMargin, (int)SpecItem.FRAAF_4dB_PhaseMargin, InspType.Normal, new double[] { });
        }
        void RegisterCheck(int ch, string testItem, int InspCnt)
        {
            PowerSequence(ch);
            AddLog(ch, $"PowerSequence Off & On");
            // AF 단독 검사
            if (RegisterCheckJudge(ch, testItem, InspCnt, true, false))
            {
                PassFails[ch].Results[(int)SpecItem.AFPIDVerifyRes].Val = 1;
            }
            else
            {
                PassFails[ch].Results[(int)SpecItem.AFPIDVerifyRes].Val = 0;
            }
            ShowDataResults(ch, (int)SpecItem.AFPIDVerifyRes, (int)SpecItem.AFPIDVerifyRes, InspType.Normal, new double[] { });

            // OIS 단독 검사
            if (RegisterCheckJudge(ch, testItem, InspCnt, false, true))
            {
                PassFails[ch].Results[(int)SpecItem.OISPIDVerifyRes].Val = 1;
            }
            else
            {
                PassFails[ch].Results[(int)SpecItem.OISPIDVerifyRes].Val = 0;
            }
            ShowDataResults(ch, (int)SpecItem.OISPIDVerifyRes, (int)SpecItem.OISPIDVerifyRes, InspType.Normal, new double[] { });
        }
        bool RegisterCheckJudge(int ch, string testItem, int InspCnt, bool checkAF = true, bool checkOIS = true)
        {
            byte[] rbuf = new byte[1];

            // 검사 결과 종합 변수 (기본값 true)
            bool finalResult = true;

            // 둘 다 검사 안 하겠다고 파라미터가 넘어오면 false 반환
            if (!checkAF && !checkOIS) return false;

            // ==========================================
            // 1. AF Register Check
            // ==========================================
            if (checkAF)
            {
                AddLog(ch, "--- AF Register Verify Start ---");
                bool isAfAllMatch = true;
                int afErrorCount = 0;
                byte[] rbummf = new byte[1];

                if (Load_AFPID(Current.FWPath) && IC_DATA_AF_REG != null && IC_DATA_AF != null)
                {
                    for (int i = 0; i < IC_DATA_AF_REG.Length; i++)
                    {
                        byte regAddr = IC_DATA_AF_REG[i];
                        byte expectedData = IC_DATA_AF[i];

                        Dln.ReadArray(ch, DrvIC.AF_Addr, regAddr, rbuf);
                        byte actualData = rbuf[0];

                        if (actualData != expectedData)
                        {
                            AddLog(ch, $"[AF] Reg Mismatch! Addr: 0x{regAddr:X2}, Expected: 0x{expectedData:X2}, Actual: 0x{actualData:X2}");
                            isAfAllMatch = false;
                            afErrorCount++;
                        }
                    }
                }
                else
                {
                    AddLog(ch, "Error : AF PID Load NG or arrays are null.");
                    isAfAllMatch = false;
                }

                if (isAfAllMatch)
                {
                    AddLog(ch, $"AF Register Verify : PASS (Total {IC_DATA_AF_REG?.Length ?? 0} Regs matched)");
                }
                else
                {
                    AddLog(ch, $"AF Register Verify : FAIL ({afErrorCount} mismatches found)");
                    finalResult = false; // AF 실패 시 최종 결과를 false로 설정
                }
            }


            // ==========================================
            // 2. OIS Register Check
            // ==========================================
            if (checkOIS)
            {
                AddLog(ch, "--- OIS Register Verify Start ---");
                bool isOisAllMatch = true;
                int oisErrorCount = 0;
                int totalOisCheckCnt = 0;

                // ★ 검사에서 제외할 OIS 레지스터 주소 목록 (0x2A ~ 0x36)
                HashSet<byte> excludedOisAddrs = new HashSet<byte>
                {
                    0x2A, 0x2B, 0x2C, 0x2D, 0x2E, 0x2F, 0x30,
                    0x31, 0x32, 0x33, 0x34, 0x35, 0x36
                };

                // ★ X, Y 각각의 예상값을 저장할 딕셔너리
                Dictionary<byte, byte> expectedOisX = new Dictionary<byte, byte>();
                Dictionary<byte, byte> expectedOisY = new Dictionary<byte, byte>();

                string oisPidPathX = Current.XPidPath;
                string oisPidPathY = Current.YPidPath;

                // ★ 파일을 읽어 딕셔너리에 저장하는 로컬 함수
                bool LoadExpectedData(string path, Dictionary<byte, byte> dict)
                {
                    if (!File.Exists(path)) return false;
                    try
                    {
                        string textVal = File.ReadAllText(path);
                        string[] t = textVal.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        foreach (string line in t)
                        {
                            string[] b = line.Split(new string[] { ",", " ", "\t" }, StringSplitOptions.RemoveEmptyEntries);
                            if (b.Length >= 2)
                            {
                                byte regAddr = Convert.ToByte(b[0], 16);
                                byte dataAddr = Convert.ToByte(b[1], 16);
                                dict[regAddr] = dataAddr;
                            }
                        }
                        return true;
                    }
                    catch { return false; }
                }

                // X, Y 파일 파싱 진행
                bool xLoaded = LoadExpectedData(oisPidPathX, expectedOisX);
                bool yLoaded = LoadExpectedData(oisPidPathY, expectedOisY);

                if (!xLoaded || !yLoaded)
                {
                    AddLog(ch, "Error : OIS PID file(X or Y) load failed or not found.");
                    isOisAllMatch = false;
                }

                // 파일 로드에 성공했을 때만 IC와 비교 검사 진행
                if (isOisAllMatch)
                {
                    // X와 Y 파일에 있는 모든 레지스터 주소를 수집 (중복 제거)
                    HashSet<byte> allRegAddrs = new HashSet<byte>(expectedOisX.Keys);
                    allRegAddrs.UnionWith(expectedOisY.Keys);

                    foreach (byte regAddr in allRegAddrs)
                    {
                        // 제외 목록에 포함된 주소라면 건너뜀
                        if (excludedOisAddrs.Contains(regAddr)) continue;

                        // --- OIS X축 (XSlaveAddr) 체크 ---
                        if (expectedOisX.TryGetValue(regAddr, out byte expectedX))
                        {
                            Dln.ReadArray(ch, DrvIC.XSlaveAddr, regAddr, rbuf);
                            byte actualX = rbuf[0];
                            if (actualX != expectedX)
                            {
                                AddLog(ch, $"[OIS-X] Reg Mismatch! Addr: 0x{regAddr:X2}, Expected: 0x{expectedX:X2}, Actual: 0x{actualX:X2}");
                                isOisAllMatch = false;
                                oisErrorCount++;
                            }
                            totalOisCheckCnt++;
                        }

                        // --- OIS Y축 (Y1SlaveAddr) 체크 ---
                        if (expectedOisY.TryGetValue(regAddr, out byte expectedY))
                        {
                            Dln.ReadArray(ch, DrvIC.Y1SlaveAddr, regAddr, rbuf);
                            byte actualY = rbuf[0];
                            if (actualY != expectedY)
                            {
                                AddLog(ch, $"[OIS-Y] Reg Mismatch! Addr: 0x{regAddr:X2}, Expected: 0x{expectedY:X2}, Actual: 0x{actualY:X2}");
                                isOisAllMatch = false;
                                oisErrorCount++;
                            }
                            totalOisCheckCnt++;
                        }
                    }
                }

                if (isOisAllMatch)
                {
                    AddLog(ch, $"OIS Register Verify : PASS (Total {totalOisCheckCnt} Regs matched)");
                }
                else
                {
                    AddLog(ch, $"OIS Register Verify : FAIL ({oisErrorCount} mismatches found)");
                    finalResult = false;
                }
            }

            return finalResult;
        }
        // WriteUserMem 함수 내부 혹은 클래스 멤버로 위치
        private bool VerifySectionMemory(int ch, string sectionName, int targetSlaveAddr, List<(string ItemName, byte Addr, double Value)> memList)
        {
            AddLog(ch, $"--- Verify {sectionName} Memory ---");
            bool isAllMatch = true;
            byte[] rBuf = new byte[1];

            foreach (var mem in memList)
            {
                // 쓰기 때와 동일한 데이터 변환 로직 (Round 처리)
                byte expectedData = (byte)Math.Max(0, Math.Min(255, Math.Round(mem.Value)));

                // DLN 인터페이스를 통해 한 바이트씩 읽기
                if (Dln.ReadArray(ch, targetSlaveAddr, mem.Addr, rBuf))
                {
                    if (rBuf[0] != expectedData)
                    {
                        AddLog(ch, $"[{sectionName} Verify FAIL] {mem.ItemName} (Addr: 0x{mem.Addr:X2}) | Expected: 0x{expectedData:X2}, Actual: 0x{rBuf[0]:X2}");
                        isAllMatch = false;
                    }
                }
                else
                {
                    AddLog(ch, $"[{sectionName} Verify ERROR] Failed to read Addr: 0x{mem.Addr:X2}");
                    isAllMatch = false;
                }
                Thread.Sleep(5); // 통신 안정화
            }

            if (isAllMatch)
                AddLog(ch, $"[{sectionName} Verify PASS] All data properly matched after Power Reset.");

            return isAllMatch;
        }
        public void WriteUserMem(int ch, bool bPass)
        {
            // 1. 공통 데이터 및 시간 정보 계산
            int result = bPass ? 0x02 : 0x09;
            var now = DateTime.Now;
            var year = now.Year - 2000;
            var month = now.Month;
            var day = now.Day;
            var hour = now.Hour;
            var minute = now.Minute;
            var second = now.Second;

            // 2. AF Sensitivity 계산
            double calculatedValue = PassFails[ch].Results[(int)SpecItem.AF_Sensitivity].Val * 10000;
            int decValue = (int)calculatedValue;
            byte regFC = (byte)((decValue >> 8) & 0xFF);
            byte regFD = (byte)(decValue & 0xFF);

            // 3. 쓰기용 데이터 리스트 준비 (AF / OIS X / OIS Y)
            var memoryList = new List<(string ItemName, byte Addr, double Value)>
            {
                ("AF_Hys",           0xED, PassFails[ch].Results[(int)SpecItem.AF_Hysteresis].Val * 10),
                ("Test_Result",      0xF0, result),
                ("AF_Phase Margin",  0xF1, PassFails[ch].Results[(int)SpecItem.FRAAF_PhaseMargin].Val),
                ("AF_Rated Stroke",  0xF2, PassFails[ch].Results[(int)SpecItem.AF_Ratedstroke].Val - 600),
                ("Insp_Vendor",      0xF4, 12),
                ("Machine_Num_Up",   0xF5, int.Parse(Model.TesterNo) >> 8),
                ("Machine_Num_Lo",   0xF6, int.Parse(Model.TesterNo)),
                ("Year_Month",       0xF7, (byte)(((year & 0x3F) << 2) | ((month >> 2) & 0x03))),
                ("MonDayTime",       0xF8, (byte)(((month & 0x03) << 6) | ((day & 0x1F) << 1) | ((hour >> 4) & 0x01))),
                ("Insp_Time_Min",    0xF9, (byte)(((hour & 0x0F) << 4) | ((minute >> 2) & 0x0F))),
                ("Insp_Time_Min_Sec",0xFA, (byte)(((minute & 0x03) << 6) | ((second >> 2) & 0x0F))),
                ("AF_PID_Ver",       0xFB, AfVer),
                ("AF_Sens_high",     0xFC, regFC),
                ("AF_Sens_low",      0xFD, regFD),
            };

            var memoryListOisX = new List<(string ItemName, byte Addr, double Value)>
            {
                ("OISX_Hys",         0xF8, PassFails[ch].Results[(int)SpecItem.OISX_Hysteresis].Val * 10),
                ("OIS_X_Stroke800",  0xFB, PassFails[ch].Results[(int)SpecItem.OISX_Ratedstroke].Val - 800),
                ("OIS_X_Phase",      0xFE, PassFails[ch].Results[(int)SpecItem.FRAX_PhaseMargin].Val),
                ("OIS_X_PID_Ver",    0xFF, OisVer),
                ("OIS_Sens_high",    0xF6, (int)(PassFails[ch].Results[(int)SpecItem.OISX_Sensitivity].Val * 10000) >> 8),
                ("OIS_Sens_low",     0xF7, (int)(PassFails[ch].Results[(int)SpecItem.OISX_Sensitivity].Val * 10000) & 0xFF),
            };

            var memoryListOisY = new List<(string ItemName, byte Addr, double Value)>
            {
                ("OIS_Y_Hys",        0xF8, PassFails[ch].Results[(int)SpecItem.OISY_Hysteresis].Val * 10),
                ("OIS_Y_Stroke800",  0xFB, PassFails[ch].Results[(int)SpecItem.OISY_Ratedstroke].Val - 800),
                ("OIS_Y_Phase",      0xFE, PassFails[ch].Results[(int)SpecItem.FRAY1_PhaseMargin].Val),
                ("OIS_Y_Sens_high",  0xF6, (int)(PassFails[ch].Results[(int)SpecItem.OISY_Sensitivity].Val * 10000) >> 8),
                ("OIS_Y_Sens_low",   0xF7, (int)(PassFails[ch].Results[(int)SpecItem.OISY_Sensitivity].Val * 10000) & 0xFF),
                ("OIS Cal AFCode",   0xFF, BestAFPos / 10),
            };
            // --- [기존] 유선 DLN 모드 ---
            AddLog(ch, "--- [DLN Mode] Write User Memory Start ---");
            WriteSection(ch, DrvIC.AF_Addr, memoryList);
            WriteSection(ch, DrvIC.XSlaveAddr, memoryListOisX);
            WriteSection(ch, DrvIC.Y1SlaveAddr, memoryListOisY);

            AddLog(ch, "--- Executing Power Sequence (Rebooting IC) ---");
            PowerSequence(ch);
            Thread.Sleep(200);

            AddLog(ch, "--- Starting Post-Reboot NVM Verification ---");
            bool finalVerifyPass = true;
            if (!VerifySectionMemory(ch, "AF", DrvIC.AF_Addr, memoryList)) finalVerifyPass = false;
            if (!VerifySectionMemory(ch, "OIS X", DrvIC.XSlaveAddr, memoryListOisX)) finalVerifyPass = false;
            if (!VerifySectionMemory(ch, "OIS Y", DrvIC.Y1SlaveAddr, memoryListOisY)) finalVerifyPass = false;
            AddLog(ch, $"--- Final NVM Verification Result: {(finalVerifyPass ? "PASS" : "FAIL")} ---");
        }

        // 기존 DLN용 쓰기 헬퍼
        private void WriteSection(int ch, int slave, List<(string ItemName, byte Addr, double Value)> list)
        {
            Dln.WriteArray(ch, slave, 0x02, new byte[] { 0x40 });
            Dln.WriteArray(ch, slave, 0xAE, new byte[] { 0x3B });
            foreach (var mem in list)
            {
                byte data = (byte)Math.Max(0, Math.Min(255, Math.Round(mem.Value)));
                Dln.WriteArray(ch, slave, mem.Addr, new byte[] { data });
                AddLog(ch, $"Write UserMem : Addr 0x{mem.Addr:X2} , Data 0x{data:X2} ({mem.ItemName})");
                Thread.Sleep(30);
            }
            Dln.WriteArray(ch, slave, 0xAE, new byte[] { 0x00 });
        }
        //void WriteUserMem(int ch, bool bPass)
        //{
        //    int result = 0x00;
        //    if (!bPass) result = 0x09;
        //    else result = 0x02;
        //    var now = STATIC.LogDate;
        //    var year = now.Year - 2000;
        //    var month = now.Month;
        //    var day = now.Day;
        //    var hour = now.Hour;
        //    var minute = now.Minute;
        //    var second = now.Second;

        //    // ★ 메모리 검증(Verify)을 수행하는 로컬 함수
        //    bool VerifySectionMemory(string sectionName, int targetSlaveAddr, List<(string ItemName, byte Addr, double Value)> memList)
        //    {
        //        AddLog(ch, $"--- Verify {sectionName} Memory ---");
        //        bool isAllMatch = true;
        //        byte[] rBuf = new byte[1];

        //        foreach (var mem in memList)
        //        {
        //            // 쓰기 때와 동일한 데이터 변환 로직
        //            byte expectedData = (byte)Math.Max(0, Math.Min(255, Math.Round(mem.Value)));

        //            // ReadArray로 값 읽어오기
        //            if (Dln.ReadArray(ch, targetSlaveAddr, mem.Addr, rBuf))
        //            {
        //                if (rBuf[0] != expectedData)
        //                {
        //                    AddLog(ch, $"[{sectionName} Verify FAIL] {mem.ItemName} (Addr: 0x{mem.Addr:X2}) | Expected: 0x{expectedData:X2}, Actual: 0x{rBuf[0]:X2}");
        //                    isAllMatch = false;
        //                }
        //            }
        //            else
        //            {
        //                AddLog(ch, $"[{sectionName} Verify ERROR] Failed to read Addr: 0x{mem.Addr:X2}");
        //                isAllMatch = false;
        //            }
        //            Thread.Sleep(5); // 통신 부하 방지
        //        }

        //        if (isAllMatch) AddLog(ch, $"[{sectionName} Verify PASS] All data properly matched after Power Reset.");
        //        return isAllMatch;
        //    }

        //    bool finalVerifyPass = true;

        //    // ==========================================
        //    // 1. AF Memory Write
        //    // ==========================================
        //    AddLog(ch, "--- Write AF Memory Start ---");
        //    AddLog(ch, "--- User AF Mem Mode On---");
        //    Dln.WriteArray(ch, DrvIC.AF_Addr, 0x02, new byte[] { 0x40 });
        //    AddLog(ch, $"Write : Addr 0x{0x02:X2} , Data 0x{0x40:X2}");
        //    Dln.WriteArray(ch, DrvIC.AF_Addr, 0xAE, new byte[] { 0x3B });
        //    AddLog(ch, $"Write : Addr 0x{0xAE:X2} , Data 0x{0x3B:X2}");

        //    double calculatedValue = PassFails[ch].Results[(int)SpecItem.AF_Sensitivity].Val * 10000;
        //    int decValue = (int)calculatedValue;

        //    byte regFC = (byte)((decValue >> 8) & 0xFF);
        //    byte regFD = (byte)(decValue & 0xFF);

        //    var memoryList = new List<(string ItemName, byte Addr, double Value)>
        //    {
        //        ("AF_Hys",              0xED, (int)(PassFails[ch].Results[(int)SpecItem.AF_Hysteresis].Val * 10)),
        //        ("Test_Result",         0xF0, result),
        //        ("AF_Phase Margin",     0xF1, (int)(PassFails[ch].Results[(int)SpecItem.FRAAF_PhaseMargin].Val)),
        //        ("AF_Rated Stroke",     0xF2, (int)(PassFails[ch].Results[(int)SpecItem.AF_Ratedstroke].Val - 600)),
        //        ("Insp_Vendor",         0xF4, 12),
        //        ("Machine_Num_Up",      0xF5, int.Parse((Model.TesterNo)) >> 8),
        //        ("Machine_Num_Lo",      0xF6, int.Parse(Model.TesterNo)),
        //        ("Year_Month",          0xF7, (byte)(((year & 0x3F) << 2) | ((month >> 2) & 0x03))),
        //        ("MonDayTime",          0xF8, (byte)(((month & 0x03) << 6) | ((day & 0x1F) << 1) | ((hour >> 4) & 0x01))),
        //        ("Insp_Time_Min",       0xF9, (byte)(((hour & 0x0F) << 4) | ((minute >> 2) & 0x0F))),
        //        ("Insp_Time_Min_Sec",   0xFA, (byte)(((minute & 0x03) << 6) | ((second >> 2) & 0x0F))),
        //        ("AF_PID_Ver",          0xFB, AfVer),
        //        ("AF_Sens_high",        0xFC, regFC),
        //        ("AF_Sens_low",         0xFD, regFD),
        //    };

        //    foreach (var mem in memoryList)
        //    {
        //        try
        //        {
        //            byte data = (byte)Math.Max(0, Math.Min(255, Math.Round(mem.Value)));
        //            Dln.WriteArray(ch, DrvIC.AF_Addr, mem.Addr, new byte[] { data });
        //            AddLog(ch, $"Write UserMem : Addr 0x{mem.Addr:X2} , Data 0x{data:X2} ({mem.ItemName})");
        //            Thread.Sleep(10);

        //            byte[] r = new byte[1];
        //            Dln.ReadArray(ch, DrvIC.AF_Addr, mem.Addr, r);
        //        }
        //        catch (Exception ex)
        //        {
        //            AddLog(ch, $"WriteUserMem Error [{mem.ItemName}] : {ex.Message}");
        //        }
        //    }

        //    AddLog(ch, "--- User Mem AF Mode Off---");
        //    Dln.WriteArray(ch, DrvIC.AF_Addr, 0xAE, new byte[] { 0x00 });
        //    AddLog(ch, $"Write : Addr 0x{0xAE:X2} , Data 0x{0x00:X2}");



        //    // ==========================================
        //    // 2. OIS X Memory Write
        //    // ==========================================
        //    AddLog(ch, "--- Write OIS X Memory Start ---");
        //    AddLog(ch, "--- User Mem Mode On---");
        //    Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x02, new byte[] { 0x40 });
        //    AddLog(ch, $"Write : Addr 0x{0x02:X2} , Data 0x{0x40:X2}");
        //    Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0xAE, new byte[] { 0x3B });
        //    AddLog(ch, $"Write : Addr 0x{0xAE:X2} , Data 0x{0x3B:X2}");

        //    calculatedValue = PassFails[ch].Results[(int)SpecItem.OISX_Sensitivity].Val * 10000;
        //    decValue = (int)calculatedValue;

        //    regFC = (byte)((decValue >> 8) & 0xFF);
        //    regFD = (byte)(decValue & 0xFF);

        //    var memoryListOisX = new List<(string ItemName, byte Addr, double Value)>
        //    {
        //        ("OISX_Hys",            0xF8, (int)(PassFails[ch].Results[(int)SpecItem.OISX_Hysteresis].Val * 10)),
        //        ("OIS_X_Stroke800",     0xFB, (int)(PassFails[ch].Results[(int)SpecItem.OISX_Ratedstroke].Val - 800)),
        //        ("OIS_X_Phase",         0xFE, (int)(PassFails[ch].Results[(int)SpecItem.FRAX_PhaseMargin].Val)),
        //        ("OIS_X_PID_Ver",       0xFF, OisVer),
        //        ("OIS_Sens_high",       0xF6, regFC),
        //        ("OIS_Sens_low",        0xF7, regFD),
        //    };

        //    foreach (var mem in memoryListOisX)
        //    {
        //        try
        //        {
        //            byte data = (byte)Math.Max(0, Math.Min(255, Math.Round(mem.Value)));
        //            Dln.WriteArray(ch, DrvIC.XSlaveAddr, mem.Addr, new byte[] { data });
        //            AddLog(ch, $"Write UserMem : Addr 0x{mem.Addr:X2} , Data 0x{data:X2} ({mem.ItemName})");
        //            Thread.Sleep(10);
        //        }
        //        catch (Exception ex)
        //        {
        //            AddLog(ch, $"WriteUserMem Error [{mem.ItemName}] : {ex.Message}");
        //        }
        //    }

        //    AddLog(ch, "--- User Mem X Mode Off---");
        //    Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0xAE, new byte[] { 0x00 });
        //    AddLog(ch, $"Write : Addr 0x{0xAE:X2} , Data 0x{0x00:X2}");



        //    // ==========================================
        //    // 3. OIS Y Memory Write
        //    // ==========================================
        //    AddLog(ch, "--- Write OIS Y Memory Start ---");
        //    AddLog(ch, "--- User Mem Y Mode On---");
        //    Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x02, new byte[] { 0x40 });
        //    AddLog(ch, $"Write : Addr 0x{0x02:X2} , Data 0x{0x40:X2}");
        //    Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0xAE, new byte[] { 0x3B });
        //    AddLog(ch, $"Write : Addr 0x{0xAE:X2} , Data 0x{0x3B:X2}");

        //    calculatedValue = PassFails[ch].Results[(int)SpecItem.OISY_Sensitivity].Val * 10000;
        //    decValue = (int)calculatedValue;

        //    regFC = (byte)((decValue >> 8) & 0xFF);
        //    regFD = (byte)(decValue & 0xFF);

        //    var memoryListOisY = new List<(string ItemName, byte Addr, double Value)>
        //    {
        //        ("OIS_Y_Hys",           0xF8, (int)(PassFails[ch].Results[(int)SpecItem.OISY_Hysteresis].Val * 10)),
        //        ("OIS_Y_Stroke800",     0xFB, (int)(PassFails[ch].Results[(int)SpecItem.OISY_Ratedstroke].Val - 800)),
        //        ("OIS_Y_Phase",         0xFE, (int)(PassFails[ch].Results[(int)SpecItem.FRAY1_PhaseMargin].Val)),
        //      //  ("OIS_Y_PID_Ver",       0xE3, 2),
        //        ("OIS_Y_Sens_high",     0xF6, regFC),
        //        ("OIS_Y_Sens_low",      0xF7, regFD),
        //        ("OIS Cal AFCode",      0xFF, BestAFPos / 10),
        //    };

        //    foreach (var mem in memoryListOisY)
        //    {
        //        try
        //        {
        //            byte data = (byte)Math.Max(0, Math.Min(255, Math.Round(mem.Value)));
        //            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, mem.Addr, new byte[] { data });
        //            AddLog(ch, $"Write UserMem : Addr 0x{mem.Addr:X2} , Data 0x{data:X2} ({mem.ItemName})");
        //            Thread.Sleep(10);
        //        }
        //        catch (Exception ex)
        //        {
        //            AddLog(ch, $"WriteUserMem Error [{mem.ItemName}] : {ex.Message}");
        //        }
        //    }

        //    AddLog(ch, "--- User Mem Y Mode Off---");
        //    Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0xAE, new byte[] { 0x00 });
        //    AddLog(ch, $"Write : Addr 0x{0xAE:X2} , Data 0x{0x00:X2}");

        //    AddLog(ch, "--- Write User Memory End ---");


        //    // ==========================================
        //    // 4. Power Reset & NVM Verify
        //    // ==========================================
        //    AddLog(ch, "--- Executing Power Sequence (Rebooting IC) ---");
        //    Dln.PowerSequence(0);

        //    // IC 재부팅 후 통신 안정화를 위해 딜레이 추가 (기존에 딜레이가 없다면 장비에 맞춰 조절)
        //    Thread.Sleep(200);

        //    AddLog(ch, "--- Starting Post-Reboot NVM Verification ---");

        //    // 모든 모드가 꺼진 상태(재부팅 직후)에서 실제로 값이 NVM에 남았는지 읽어서 비교합니다.
        //    if (!VerifySectionMemory("AF", DrvIC.AF_Addr, memoryList)) finalVerifyPass = false;
        //    if (!VerifySectionMemory("OIS X", DrvIC.XSlaveAddr, memoryListOisX)) finalVerifyPass = false;
        //    if (!VerifySectionMemory("OIS Y", DrvIC.Y1SlaveAddr, memoryListOisY)) finalVerifyPass = false;

        //    string finalResultStr = finalVerifyPass ? "PASS" : "FAIL";
        //    AddLog(ch, $"--- Final NVM Verification Result: {finalResultStr} ---");
        //}
        public static void Wait(int ms)
        {
            //       Thread.Sleep(ms);
            ms = ms * 1000;
            Stopwatch startNew = Stopwatch.StartNew();

            long usDelayTick = (ms * Stopwatch.Frequency) / 1000000;

            while (startNew.ElapsedTicks < usDelayTick) ;
        }
        #endregion
        private bool Load_AFPID(string path)
        {
            try
            {
                string textVal = File.ReadAllText(path);
                string[] t = textVal.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                IC_DATA_AF = new byte[(t.Length - 1)];
                IC_DATA_AF_REG = new byte[(t.Length - 1)];
                for (int i = 0; i < t.Length; i++)
                {
                    if (i == 0)
                    {
                        string[] b = t[i].Split(new string[] { ",", " ", "\t", "//", "Reg" }, StringSplitOptions.RemoveEmptyEntries);
                        IC_SETTING_AF = new byte[b.Length / 2];
                        IC_SETTING_AF_REG = new byte[b.Length / 2];
                        for (int j = 0; j < b.Length; j++)
                        {
                            if (j < b.Length / 2) IC_SETTING_AF[j] = Convert.ToByte(b[j], 16);
                            else IC_SETTING_AF_REG[j - b.Length / 2] = Convert.ToByte(b[j], 16);
                        }
                    }
                    else
                    {
                        string[] b = t[i].Split(new string[] { ",", " ", "\t" }, StringSplitOptions.RemoveEmptyEntries);

                        IC_DATA_AF_REG[(i - 1)] = Convert.ToByte(b[0], 16);
                        IC_DATA_AF[(i - 1)] = Convert.ToByte(b[1], 16);

                    }

                }
                return true;
            }
            catch { return false; }

        }
        private bool Load_OISPID(int ch, int sAddr, string path)
        {
            try
            {
                string textVal = File.ReadAllText(path);
                string[] t = textVal.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < t.Length; i++)
                {

                    string[] b = t[i].Split(new string[] { ",", " ", "\t" }, StringSplitOptions.RemoveEmptyEntries);
                    byte memAddr = Convert.ToByte(b[0], 16);
                    byte dataAddr = Convert.ToByte(b[1], 16);

                    Dln.WriteByte(ch, sAddr, memAddr, 1, dataAddr);

                    AddLog(ch, $"Write Mem Addr: 0x{memAddr:X2}, Data: 0x{dataAddr:X2}");

                }
                return true;
            }
            catch { return false; }

        }
        // 간단한 데이터 구조 정의 (함수 밖 또는 클래스 상단)
        public struct RegisterItem
        {
            public byte Addr;
            public byte Val;
        }

        private List<RegisterItem> GetPidList(int ch, string path)
        {
            List<RegisterItem> pidList = new List<RegisterItem>();
            try
            {
                if (!File.Exists(path)) return pidList;

                string textVal = File.ReadAllText(path);
                string[] t = textVal.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                for (int i = 0; i < t.Length; i++)
                {
                    string[] b = t[i].Split(new string[] { ",", " ", "\t" }, StringSplitOptions.RemoveEmptyEntries);
                    if (b.Length < 2) continue;

                    byte memAddr = Convert.ToByte(b[0], 16);
                    byte dataAddr = Convert.ToByte(b[1], 16);

                    pidList.Add(new RegisterItem { Addr = memAddr, Val = dataAddr });
                }
            }
            catch (Exception ex)
            {
                AddLog(ch, $"[Error] GetPidList: {ex.Message}");
            }
            return pidList;
        }
    }
}
