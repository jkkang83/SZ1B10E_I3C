using Dln;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Xml.Linq;

namespace FZ4P
{
    public delegate void FunctionPointer(int ch, string testItem, int InspCnt);
    public class ActItems
    {
        public string Name { get; set; }
        public FunctionPointer Func { get; set; }
        public bool IsMulti { get; set; }

        public bool IsRetry { get; set; }
        public string Time { get; set; }
    }
    public class FindResult
    {
        public double[] cx = new double[2];
        public double[] cy = new double[2];
        public double[] cz = new double[2];
        public long nFound = 0;
        public double[] tx = new double[2];
        public double[] ty = new double[2];
        public double[] tz = new double[2];

        public double[] cy1 = new double[2];
        public double[] cy2 = new double[2];
    }
    public class NVMHallParam
    {
        public int XHOffset;
        public int YHOffset;
        public int XHBias;
        public int YHBias;
        public int XHmin;
        public int XHmax;
        public int YHmin;
        public int YHmax;
        public int XHmid;
        public int YHmid;
        public int XDrv_Min;
        public int XDrv_Max;
        public int YDrv_Min;
        public int YDrv_Max;
        public int XNEPAmin;
        public int XNEPAmax;
        public int YNEPAmin;
        public int YNEPAmax;
        public void Clear()
        {
            XHOffset = 0;
            YHOffset = 0;
            XHBias = 0;
            YHBias = 0;
            XHmin = 0;
            XHmax = 0;
            YHmin = 0;
            YHmax = 0;
            XHmid = 0;
            YHmid = 0;
            XDrv_Min = 0;
            XDrv_Max = 0;
            YDrv_Min = 0;
            YDrv_Max = 0;
            XNEPAmin = 0;
            XNEPAmax = 0;
            YNEPAmin = 0;
            YNEPAmax = 0;
        }
    };
    public class LogText
    {
        public TextBox box = new TextBox();
        public LogText()
        {
            box.BackColor = Color.Black;
            box.BorderStyle = BorderStyle.FixedSingle;
            box.Font = new Font("맑은 고딕", 9F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            box.ForeColor = Color.LemonChiffon;
            box.Multiline = true;
            box.ReadOnly = true;
            box.ScrollBars = ScrollBars.Vertical;
            box.Size = new System.Drawing.Size(474, 70);
            box.TabIndex = 127;
            box.Tag = "S";
            box.MouseDoubleClick += new MouseEventHandler(MouseDoubleClick);
        }
        private void MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (box.Tag.ToString() == "S")
            {
                box.Size = new System.Drawing.Size(475, 638);
                box.Tag = "L";
            }
            else
            {
                box.Size = new System.Drawing.Size(475, 70);
                box.Tag = "S";
            }
        }
        public void Log(string e)
        {
            if (box.InvokeRequired)
            {
                box.BeginInvoke((MethodInvoker)delegate
                {
                    box.AppendText(e + "\r\n");
                });
            }
            else
                box.AppendText(e + "\r\n");
        }
        public void Clear()
        {
            if (box.InvokeRequired)
            {
                box.BeginInvoke((MethodInvoker)delegate
                {
                    box.Text = string.Empty;
                });
            }
            else box.Text = string.Empty;

        }
    }
    public class InfoButton
    {
        public Button btn = new Button();
        public InfoButton()
        {
            btn.BackColor = Color.Black;
            btn.Font = new Font("Microsoft Sans Serif", 60F, System.Drawing.FontStyle.Bold);
            btn.ForeColor = Color.Transparent;
            btn.Size = new System.Drawing.Size(470, 200);
            btn.Text = "Start Test";
            btn.UseVisualStyleBackColor = false;
            btn.Click += new EventHandler(this.Info_Click);
        }
        private void Info_Click(object sender, EventArgs e)
        {
            btn.Hide();
        }
    }
    public class DrvParam
    {
        public int XPCAL;
        public int XNCAL;
        public int YPCAL;
        public int YNCAL;

        public int XPOSVT;
        public int XNEGVT;
        public int YPOSVT;
        public int YNEGVT;

        public int XLC2C;
        public int XLC2D;
        public int XLC2E;

        public int YLC2C;
        public int YLC2D;
        public int YLC2E;

        public void Clear()
        {
            XPCAL = 0;
            XNCAL = 0;
            YPCAL = 0;
            YNCAL = 0;

            XPOSVT = 0;
            XNEGVT = 0;
            YPOSVT = 0;
            YNEGVT = 0;

            XLC2C = 0;
            XLC2D = 0;
            XLC2E = 0;

            YLC2C = 0;
            YLC2D = 0;
            YLC2E = 0;
        }
    };
    public class CalParams
    {
        public int CodeZ { get; set; } = 0;
        public int CodeX { get; set;} = 0;
        public int CodeY { get; set; } = 0; 
        public double StrokeX  { get; set; } = 0;
        public double StrokeY  { get; set; } = 0;
        public double StrokeZ  { get; set; } = 0;
        public double StrokeY1 { get; set; } = 0;
        public double StrokeY2 { get; set; }= 0;
        public int HallX { get; set; } = 0;
        public int HallY  { get; set; } = 0;
        public int HallZ  { get; set; } = 0;
        public int HallY1 { get; set; } = 0;
        public int HallY2 { get; set; } = 0;
        public double Current {get;set;} = 0;
        public double Time    {get;set;} = 0;
        public double TiltX   {get;set;} = 0;
        public double TiltY   {get;set;} = 0;
        public double TiltZ { get; set; } = 0;

        public double MasterStrokeX { get; set; } = 0;
        public double MasterStrokeY { get; set; } = 0;
    }
    public class CalResult
    {
        public string Name { get; set; }
        public List<int> CodeZ = new List<int>();
        public List<int> CodeX = new List<int>();
        public List<int> CodeY = new List<int>();
        public List<double> StrokeX = new List<double>();
        public List<double> StrokeY = new List<double>();
        public List<double> StrokeZ = new List<double>();
        public List<double> StrokeY1 = new List<double>();
        public List<double> StrokeY2 = new List<double>();
        public List<int> HallX = new List<int>();
        public List<int> HallY = new List<int>();
        public List<int> HallZ = new List<int>();
        public List<int> HallY1 = new List<int>();
        public List<int> HallY2 = new List<int>();
        public List<double> Current = new List<double>();
        public List<double> Time = new List<double>();
        public List<double> TiltX = new List<double>();
        public List<double> TiltY = new List<double>();
        public List<double> TiltZ = new List<double>();

        public List<double> CircleStrokeX = new List<double>();
        public List<double> CircleStrokeY = new List<double>();

        public struct SPoint
        {
            public double x;
            public double y;
        };
        public struct SLine
        {
            public double dSlope;
            public double dYintercept;
        };

        public void allAddBuffer(CalParams cal)
        {
            CodeZ.Add(cal.CodeX);
            CodeX.Add(cal.CodeX);
            CodeY.Add(cal.CodeY);
            StrokeX.Add(cal.StrokeX);
            StrokeY.Add(cal.StrokeY);
            StrokeZ.Add(cal.StrokeZ);
            StrokeY1.Add(cal.StrokeY1);
            StrokeY2.Add(cal.StrokeY2);
            HallX.Add(cal.HallX);
            HallY.Add(cal.HallY);
            HallZ.Add(cal.HallZ);
            HallY1.Add(cal.HallY1);
            HallY2.Add(cal.HallY2);
            Current.Add(cal.Current);
            Time.Add(cal.Time);
            TiltX.Add(cal.TiltX);
            TiltY.Add(cal.TiltY);
            TiltZ.Add(cal.TiltZ);

            CircleStrokeX.Add(cal.MasterStrokeX);
            CircleStrokeY.Add(cal.MasterStrokeY);
        }
        public SLine Line_fitting(SPoint[] data, int dataSize)
        {
            SLine rtnLine;
            double SUMx = 0; //sum of x values
            double SUMy = 0; //sum of y values
            double SUMxy = 0; //sum of x * y
            double SUMxx = 0; //sum of x^2 
            double slope = 0; //slope of regression line
            double y_intercept = 0; //y intercept of regression line 
            double AVGy = 0; //mean of y
            double AVGx = 0; //mean of x

            //calculate various sums 
            for (int i = 0; i < dataSize; i++)
            {
                SUMx = SUMx + data[i].x;
                SUMy = SUMy + data[i].y;
                SUMxy = SUMxy + data[i].x * data[i].y;
                SUMxx = SUMxx + data[i].x * data[i].x;
            }

            //calculate the means of x and y
            AVGy = SUMy / dataSize;
            AVGx = SUMx / dataSize;

            //slope or a1
            slope = (dataSize * SUMxy - SUMx * SUMy) / (dataSize * SUMxx - SUMx * SUMx);

            //y itercept or a0
            y_intercept = AVGy - slope * AVGx;

            rtnLine.dSlope = slope;
            rtnLine.dYintercept = y_intercept;

            return rtnLine;
        }
        public SLine Line_fitting(List<SPoint> data)
        {
            SLine rtnLine;
            double SUMx = 0; //sum of x values
            double SUMy = 0; //sum of y values
            double SUMxy = 0; //sum of x * y
            double SUMxx = 0; //sum of x^2 
            double slope = 0; //slope of regression line
            double y_intercept = 0; //y intercept of regression line 
            double AVGy = 0; //mean of y
            double AVGx = 0; //mean of x

            //calculate various sums 
            for (int i = 0; i < data.Count; i++)
            {
                SUMx = SUMx + data[i].x;
                SUMy = SUMy + data[i].y;
                SUMxy = SUMxy + data[i].x * data[i].y;
                SUMxx = SUMxx + data[i].x * data[i].x;
            }

            //calculate the means of x and y
            AVGy = SUMy / data.Count;
            AVGx = SUMx / data.Count;

            //slope or a1
            slope = (data.Count * SUMxy - SUMx * SUMy) / (data.Count * SUMxx - SUMx * SUMx);

            //y itercept or a0
            y_intercept = AVGy - slope * AVGx;

            rtnLine.dSlope = slope;
            rtnLine.dYintercept = y_intercept;

            return rtnLine;
        }
        public void Clear()
        {
            CodeZ.Clear();
            CodeX.Clear();
            CodeY.Clear();
            StrokeX.Clear();
            StrokeY.Clear();
            StrokeZ.Clear();
            StrokeY1.Clear();
            StrokeY2.Clear();
            HallX.Clear();
            HallY.Clear();
            HallZ.Clear();
            HallY1.Clear();
            HallY2.Clear();


            Current.Clear();
            Time.Clear();
            TiltX.Clear();
            TiltY.Clear();
            TiltZ.Clear();
            CircleStrokeX.Clear();
            CircleStrokeY.Clear();
        }
        public CalResult(string name)
        {
            Name = name;
        }
        // mode 0 : code, mode 1 : stroke
        public double CalFwdStoke(List<int> code, List<double> stroke, int centerCode)
        {
            double min = 9999;
            double max = -9999;

            for (int i = 2; i < code.Count / 2; i++)
            {
                if (code[i] >= centerCode)
                {
                    if (min > stroke[i])
                        min = stroke[i];
                    if (max < stroke[i])
                        max = stroke[i];
                }
            }
            return Math.Abs(max - min);
        }
        public double CalBwdStoke(List<int> code, List<double> stroke, int centerCode)
        {
            double min = 9999;
            double max = -9999;
            for (int i = code.Count / 2; i < code.Count; i++)
            {
                if (code[i] <= centerCode)
                {
                    if (min > stroke[i])
                        min = stroke[i];
                    if (max < stroke[i])
                        max = stroke[i];
                }
            }

            return Math.Abs(max - min);
        }
        public double CalSensitivity(List<int> code, List<double> stroke, int CodeMin, int CodeMax, double MinStroke, double MaxStroke, int Mode, int DrvMinCode, int DrvMaxCode)
        {
            // 1. 데이터를 Fwd/Bwd로 분류 (CalLinearity와 동일한 로직)
            List<CalcList> fwdIndex = new List<CalcList>();
            for (int i = 1; i < code.Count; i++)
            {
                if (code[i] == DrvMinCode || code[i] == DrvMaxCode)
                {
                    fwdIndex.Add(new CalcList { code = code[i], Val1 = stroke[i] });
                }
                else
                {
                    int dy = code[i] - code[i - 1];
                    if (dy > 0) fwdIndex.Add(new CalcList { code = code[i], Val1 = stroke[i] });
                }
            }
            fwdIndex = fwdIndex.OrderBy(a => a.code).ToList();

            // 2. Mode에 따른 기준점(Fitting Point) 추출
            List<SPoint> pt = new List<SPoint>();

            if (Mode == 0) // Code 범위 기준
            {
                for (int i = 0; i < fwdIndex.Count - 1; i++)
                {
                    if (fwdIndex[i].code <= CodeMin && fwdIndex[i + 1].code >= CodeMin)
                    {
                        pt.Add(Math.Abs(fwdIndex[i].code - CodeMin) <= Math.Abs(fwdIndex[i + 1].code - CodeMin)
                            ? new SPoint { x = fwdIndex[i].code, y = fwdIndex[i].Val1 }
                            : new SPoint { x = fwdIndex[i + 1].code, y = fwdIndex[i + 1].Val1 });
                    }
                    if (fwdIndex[i].code <= CodeMax && fwdIndex[i + 1].code >= CodeMax)
                    {
                        pt.Add(Math.Abs(fwdIndex[i].code - CodeMax) <= Math.Abs(fwdIndex[i + 1].code - CodeMax)
                            ? new SPoint { x = fwdIndex[i].code, y = fwdIndex[i].Val1 }
                            : new SPoint { x = fwdIndex[i + 1].code, y = fwdIndex[i + 1].Val1 });
                    }
                }
            }
            //else if (Mode == 1) // 인덱스(Step) 기준
            //{
            //    if (fwdIndex.Count > MinStep + MaxStep)
            //    {
            //        pt.Add(new SPoint { x = fwdIndex[MinStep].code, y = fwdIndex[MinStep].Val1 });
            //        pt.Add(new SPoint { x = fwdIndex[fwdIndex.Count - MaxStep - 1].code, y = fwdIndex[fwdIndex.Count - MaxStep - 1].Val1 });
            //    }
            //}
            else // Stroke 범위 기준
            {
                for (int i = 0; i < fwdIndex.Count - 1; i++)
                {
                    if (fwdIndex[i].Val1 <= MinStroke && fwdIndex[i + 1].Val1 >= MinStroke)
                    {
                        pt.Add(Math.Abs(fwdIndex[i].Val1 - MinStroke) <= Math.Abs(fwdIndex[i + 1].Val1 - MinStroke)
                            ? new SPoint { x = fwdIndex[i].code, y = fwdIndex[i].Val1 }
                            : new SPoint { x = fwdIndex[i + 1].code, y = fwdIndex[i + 1].Val1 });
                    }
                    if (fwdIndex[i].Val1 <= MaxStroke && fwdIndex[i + 1].Val1 >= MaxStroke)
                    {
                        pt.Add(Math.Abs(fwdIndex[i].Val1 - MaxStroke) <= Math.Abs(fwdIndex[i + 1].Val1 - MaxStroke)
                            ? new SPoint { x = fwdIndex[i].code, y = fwdIndex[i].Val1 }
                            : new SPoint { x = fwdIndex[i + 1].code, y = fwdIndex[i + 1].Val1 });
                    }
                }
            }

            // 3. 추출된 점들을 바탕으로 직선 피팅 후 기울기(Sensitivity) 반환
            if (pt.Count < 2) return 0; // 점이 부족할 경우 예외 처리

            SLine res = Line_fitting(pt);
            return res.dSlope;
        }
        public double CalLinearity(List<int> code, List<double> stroke, int CodeMin, int CodeMax, double MinStroke, double MaxStroke, int TargetStrokeCode, int Mode, int DrvMinCode, int DrvMaxCode)
        {

            double max = -9999;
            List<CalcList> fwdIndex = new List<CalcList>();
            List<CalcList> bwdIndex = new List<CalcList>();
            for (int i = 1; i < code.Count; i++)
            {
                if (code[i] == DrvMinCode || code[i] == DrvMaxCode)
                {
                    fwdIndex.Add(new CalcList { code = code[i], Val1 = stroke[i] });
                    bwdIndex.Add(new CalcList { code = code[i], Val1 = stroke[i] });
                }
                else
                {
                    int dy = code[i] - code[i - 1];
                    if (dy > 0) fwdIndex.Add(new CalcList { code = code[i], Val1 = stroke[i] });
                    else bwdIndex.Add(new CalcList { code = code[i], Val1 = stroke[i] });
                }


            }
            fwdIndex = fwdIndex.OrderBy(a => a.code).ToList();
            bwdIndex = bwdIndex.OrderBy(a => a.code).ToList();
            List<SPoint> pt = new List<SPoint>();

            SLine res = new SLine();
            if (Mode == 0)
            {
                for (int i = 0; i < fwdIndex.Count - 1; i++)
                {
                    if (fwdIndex[i].code <= CodeMin && fwdIndex[i + 1].code >= CodeMin)
                    {
                        if (Math.Abs(fwdIndex[i].code - CodeMin) <= Math.Abs(fwdIndex[i + 1].code - CodeMin))
                        { pt.Add(new SPoint { x = fwdIndex[i].code, y = fwdIndex[i].Val1 }); }
                        else { pt.Add(new SPoint { x = fwdIndex[i + 1].code, y = fwdIndex[i + 1].Val1 }); }
                    }
                    if (fwdIndex[i].code <= CodeMax && fwdIndex[i + 1].code >= CodeMax)
                    {
                        if (Math.Abs(fwdIndex[i].code - CodeMax) <= Math.Abs(fwdIndex[i + 1].code - CodeMax))
                        { pt.Add(new SPoint { x = fwdIndex[i].code, y = fwdIndex[i].Val1 }); }
                        else { pt.Add(new SPoint { x = fwdIndex[i + 1].code, y = fwdIndex[i + 1].Val1 }); }
                    }

                }

            }
            else if (Mode == 1)
            {
                for (int i = 0; i < fwdIndex.Count - 1; i++)
                {
                    if (fwdIndex[i].Val1 <= MinStroke && fwdIndex[i + 1].Val1 >= MinStroke)
                    {
                        if (Math.Abs(fwdIndex[i].Val1 - MinStroke) <= Math.Abs(fwdIndex[i + 1].Val1 - MinStroke))
                        { pt.Add(new SPoint { x = fwdIndex[i].code, y = fwdIndex[i].Val1 }); }
                        else { pt.Add(new SPoint { x = fwdIndex[i + 1].code, y = fwdIndex[i + 1].Val1 }); }
                    }
                    if (fwdIndex[i].Val1 <= MaxStroke && fwdIndex[i + 1].Val1 >= MaxStroke)
                    {
                        if (Math.Abs(fwdIndex[i].Val1 - MaxStroke) <= Math.Abs(fwdIndex[i + 1].Val1 - MaxStroke))
                        { pt.Add(new SPoint { x = fwdIndex[i].code, y = fwdIndex[i].Val1 }); }
                        else { pt.Add(new SPoint { x = fwdIndex[i + 1].code, y = fwdIndex[i + 1].Val1 }); }
                    }

                }
            }
            else if(Mode == 2)
            {
                for (int i = 0; i < fwdIndex.Count - 1; i++)
                {
                    if (fwdIndex[i].code <= 0 && fwdIndex[i + 1].code >= 0)
                    {
                        if (Math.Abs(fwdIndex[i].code - 0) <= Math.Abs(fwdIndex[i + 1].code - 0))
                        { pt.Add(new SPoint { x = fwdIndex[i].code, y = fwdIndex[i].Val1 }); }
                        else { pt.Add(new SPoint { x = fwdIndex[i + 1].code, y = fwdIndex[i + 1].Val1 }); }
                    }
                    if (fwdIndex[i].code <= TargetStrokeCode && fwdIndex[i + 1].code >= TargetStrokeCode)
                    {
                        if (Math.Abs(fwdIndex[i].code - TargetStrokeCode) <= Math.Abs(fwdIndex[i + 1].code - TargetStrokeCode))
                        { pt.Add(new SPoint { x = fwdIndex[i].code, y = fwdIndex[i].Val1 }); }
                        else { pt.Add(new SPoint { x = fwdIndex[i + 1].code, y = fwdIndex[i + 1].Val1 }); }
                    }

                }

            }

            res = Line_fitting(pt);
            if (Mode == 0)
            {
                for (int i = 0; i < fwdIndex.Count; i++)
                {
                    if (fwdIndex[i].code >= CodeMin && fwdIndex[i].code <= CodeMax)
                    {
                        double newS = res.dSlope * fwdIndex[i].code + res.dYintercept;
                        //임시 Log === 
                        STATIC.Process.AddLog(0, string.Format("Target {0:F2} stroke {1:F2} Error {2:F2}", newS, fwdIndex[i].Val1, newS - fwdIndex[i].Val1));
                        if (max < Math.Abs(newS - fwdIndex[i].Val1))
                            max = Math.Abs(newS - fwdIndex[i].Val1);
                    }
                }
            }
            else if (Mode == 1)
            {
                for (int i = 0; i < fwdIndex.Count; i++)
                {
                    if (fwdIndex[i].Val1 >= MinStroke && fwdIndex[i].Val1 <= MaxStroke)
                    {
                        double newS = res.dSlope * fwdIndex[i].code + res.dYintercept;
                        STATIC.Process.AddLog(0, string.Format("Target {0:F2} stroke {1:F2} Error {2:F2}", newS, fwdIndex[i].Val1, newS - fwdIndex[i].Val1));
                        if (max < Math.Abs(newS - fwdIndex[i].Val1))
                            max = Math.Abs(newS - fwdIndex[i].Val1);
                    }
                }
            }
            if (Mode == 2)
            {
                for (int i = 0; i < fwdIndex.Count; i++)
                {
                    if (fwdIndex[i].code >= 0 && fwdIndex[i].code <= TargetStrokeCode)
                    {
                        double newS = res.dSlope * fwdIndex[i].code + res.dYintercept;
                        //임시 Log === 
                        STATIC.Process.AddLog(0, string.Format("Target {0:F2} stroke {1:F2} Error {2:F2}", newS, fwdIndex[i].Val1, newS - fwdIndex[i].Val1));
                        if (max < Math.Abs(newS - fwdIndex[i].Val1))
                            max = Math.Abs(newS - fwdIndex[i].Val1);
                    }
                }
            }
            return max;
        }

        public double CalHysteresis(List<int> code, List<double> stroke, int CodeMin, int CodeMax, double MinStroke, double MaxStroke, int TargetStrokeCode, int Mode, int DrvMinCode, int DrvMaxCode)
        {

            double max = -9999;
            List<CalcList> fwdIndex = new List<CalcList>();
            List<CalcList> bwdIndex = new List<CalcList>();
            for (int i = 1; i < code.Count; i++)
            {
                if (code[i] == DrvMinCode || code[i] == DrvMaxCode)
                {
                    fwdIndex.Add(new CalcList { code = code[i], Val1 = stroke[i] });
                    bwdIndex.Add(new CalcList { code = code[i], Val1 = stroke[i] });
                }
                else
                {
                    int dy = code[i] - code[i - 1];
                    if (dy > 0) fwdIndex.Add(new CalcList { code = code[i], Val1 = stroke[i] });
                    else bwdIndex.Add(new CalcList { code = code[i], Val1 = stroke[i] });
                }

            }
            fwdIndex = fwdIndex.OrderBy(a => a.code).ToList();
            bwdIndex = bwdIndex.OrderBy(a => a.code).ToList();

            if (Mode == 0)
            {
                for (int i = 0; i < fwdIndex.Count; i++)
                {
                    for (int j = 0; j < bwdIndex.Count; j++)
                    {
                        if (fwdIndex[i].code == bwdIndex[j].code)
                        {
                            if (fwdIndex[i].code >= CodeMin && fwdIndex[i].code <= CodeMax)
                            {
                                double hyst = Math.Abs(fwdIndex[i].Val1 - bwdIndex[j].Val1);
                                if (max < hyst) max = hyst;
                            }
                        }
                    }
                }
            }
            else if (Mode == 1)
            {
                for (int i = 0; i < fwdIndex.Count; i++)
                {
                    for (int j = 0; j < bwdIndex.Count; j++)
                    {
                        if (fwdIndex[i].code == bwdIndex[j].code)
                        {
                            if (fwdIndex[i].Val1 >= MinStroke && fwdIndex[i].Val1 <= MaxStroke)
                            {
                                double hyst = Math.Abs(fwdIndex[i].Val1 - bwdIndex[j].Val1);
                                if (max < hyst) max = hyst;
                            }
                        }
                    }
                }
            }
            else if(Mode == 2)
            {
                for (int i = 0; i < fwdIndex.Count; i++)
                {
                    for (int j = 0; j < bwdIndex.Count; j++)
                    {
                        if (fwdIndex[i].code == bwdIndex[j].code)
                        {
                            if (fwdIndex[i].code >= 0 && fwdIndex[i].code <= TargetStrokeCode)
                            {
                                double hyst = Math.Abs(fwdIndex[i].Val1 - bwdIndex[j].Val1);
                                if (max < hyst) max = hyst;
                            }
                        }
                    }
                }
            }
            return max;
        }

        public double CalResolution(List<int> code, List<double> stroke, int CodeRange, int StrokeRange, int centerCode)
        {
            double Resolution = 0;
            double max = -9999;

            for (int i = 0; i < code.Count; i++)
            {
                if (code[i] >= centerCode - CodeRange && code[i] <= centerCode + CodeRange && stroke[i] >= -StrokeRange && stroke[i] <= StrokeRange)
                {
                    if (i > 1 && (code[i] != code[i - 1]))
                        Resolution = (stroke[i] - stroke[i - 1]) / (code[i] - code[i - 1]);
                    if (max < Resolution)
                        max = Resolution;
                }
            }

            return max;
        }


        //public double CalMaxCurrent(List<int> code, List<double> stroke, int CodeRange, int StrokeRange)
        //{
        //    double max = 0;

        //    for (int i = 2; i < code.Count; i++)
        //    {
        //        if (code[i] >= 2048 - CodeRange && code[i] <= 2048 + CodeRange && stroke[i] >= -StrokeRange && stroke[i] <= StrokeRange)
        //        {
        //            if (Math.Abs(max) < Math.Abs(Current[i]))
        //                max = Current[i];
        //        }
        //    }
        //    return max;
        //}

        public double[] CalCurrent(List<int> code, List<double> stroke, List<double> current, int CodeMin, int CodeMax, double MinStroke, double MaxStroke, int TargetStrokeCode, int Mode, int DrvMinCode, int DrvMaxCode)
        {
            double min = 9999;
            double max = -9999;
            List<CalcList> fwdIndex = new List<CalcList>();
            List<CalcList> bwdIndex = new List<CalcList>();
            for (int i = 1; i < code.Count; i++)
            {
                if (code[i] == DrvMinCode || code[i] == DrvMaxCode)
                {
                    fwdIndex.Add(new CalcList { code = code[i], Val1 = stroke[i], Val2 = current[i] });
                    bwdIndex.Add(new CalcList { code = code[i], Val1 = stroke[i], Val2 = current[i] });
                }
                else
                {
                    int dy = code[i] - code[i - 1];
                    if (dy > 0) fwdIndex.Add(new CalcList { code = code[i], Val1 = stroke[i], Val2 = current[i] });
                    else bwdIndex.Add(new CalcList { code = code[i], Val1 = stroke[i], Val2 = current[i] });
                }

            }
            fwdIndex = fwdIndex.OrderBy(a => a.code).ToList();
            bwdIndex = bwdIndex.OrderBy(a => a.code).ToList();


            if (Mode == 0)
            {
                for (int i = 0; i < fwdIndex.Count; i++)
                {
                    if (fwdIndex[i].code >= CodeMin && fwdIndex[i].code <= CodeMax)
                    {
                        if (max < fwdIndex[i].Val2) { max = fwdIndex[i].Val2; }
                        if (min > fwdIndex[i].Val2) { min = fwdIndex[i].Val2; }
                    }
                }
                for (int i = 0; i < bwdIndex.Count; i++)
                {
                    if (bwdIndex[i].code >= CodeMin && bwdIndex[i].code <= CodeMax)
                    {
                        if (max < bwdIndex[i].Val2) { max = bwdIndex[i].Val2; }
                        if (min > bwdIndex[i].Val2) { min = bwdIndex[i].Val2; }
                    }
                }
            }
            else if(Mode == 1)
            {
                for (int i = 0; i < fwdIndex.Count; i++)
                {
                    if (fwdIndex[i].Val1 >= MinStroke && fwdIndex[i].Val1 <= MaxStroke)
                    {
                        if (max < fwdIndex[i].Val2) { max = fwdIndex[i].Val2; }
                        if (min > fwdIndex[i].Val2) { min = fwdIndex[i].Val2; }
                    }

                }

                for (int i = 0; i < bwdIndex.Count; i++)
                {
                    if (bwdIndex[i].Val1 >= MinStroke && bwdIndex[i].Val1 <= MaxStroke)
                    {
                        if (max < bwdIndex[i].Val2) { max = bwdIndex[i].Val2; }
                        if (min > bwdIndex[i].Val2) { min = bwdIndex[i].Val2; }
                    }
                }



            }
            else if (Mode == 2)
            {
                for (int i = 0; i < fwdIndex.Count; i++)
                {
                    if (fwdIndex[i].code >= 0 && fwdIndex[i].code <= TargetStrokeCode)
                    {
                        if (max < fwdIndex[i].Val2) { max = fwdIndex[i].Val2; }
                        if (min > fwdIndex[i].Val2) { min = fwdIndex[i].Val2; }
                    }
                }
                for (int i = 0; i < bwdIndex.Count; i++)
                {
                    if (bwdIndex[i].code >= 0 && bwdIndex[i].code <= TargetStrokeCode)
                    {
                        if (max < bwdIndex[i].Val2) { max = bwdIndex[i].Val2; }
                        if (min > bwdIndex[i].Val2) { min = bwdIndex[i].Val2; }
                    }
                }
            }
            return new double[] { max, min };
        }


        //public double CalCenterCurrent(List<int> code, List<double> stroke, int CodeRange, int StrokeRange)
        //{
        //    double min = 9999;
        //    double max = 0;

        //    for (int i = 2; i < code.Count; i++)
        //    {
        //        //if (code[i] >= 2048 - CodeRange && code[i] <= 2048 + CodeRange && stroke[i] >= -StrokeRange && stroke[i] <= StrokeRange)
        //        if (code[i] >= 480 && code[i] <= 2895)
        //        {
        //            if (min > Math.Abs(Current[i]))
        //                min = Math.Abs(Current[i]);
        //            if (max < Math.Abs(Current[i]))
        //                max = Math.Abs(Current[i]);
        //        }
        //    }
        //    return ((max - min) / 2) + min;
        //}
        //public double CalHoldingCurrent(List<int> code, List<double> stroke, int CodeRange, int StrokeRange)
        //{
        //    double min = 9999;
        //    double max = 0;

        //    for (int i = 2; i < code.Count; i++)
        //    {
        //        if (code[i] >= 480 && code[i] <= 2895)
        //        {
        //            //if (min > Math.Abs(Current[i]))
        //            //    min = Math.Abs(Current[i]);
        //            if (max < Math.Abs(Current[i]))
        //                max = Math.Abs(Current[i]);
        //        }
        //    }
        //    return max;
        //}
        //public double[] CalCrosstalk(List<int> code, List<double> Stroke, List<double> CrossStroke, int CodeRange, int StrokeRange, int CenterCode)
        //{
        //    double minStroke = 9999;
        //    double maxStroke = -9999;

        //    for (int i = 0; i < code.Count; i++)
        //    {
        //        if (code[i] >= CenterCode - CodeRange && code[i] <= CenterCode + CodeRange && Stroke[i] >= -StrokeRange && Stroke[i] <= StrokeRange)
        //        {
        //            if (minStroke > CrossStroke[i])
        //                minStroke = CrossStroke[i];
        //            if (maxStroke < CrossStroke[i])
        //                maxStroke = CrossStroke[i];
        //        }
        //    }

        //    double crosstalk = 0;
        //    if (Math.Abs(maxStroke) >= Math.Abs(minStroke))
        //        crosstalk = maxStroke;
        //    else if (Math.Abs(maxStroke) < Math.Abs(minStroke))
        //        crosstalk = minStroke;

        //    double crosstalkLog = 20 * Math.Log10(StrokeRange * 2 / Math.Abs(crosstalk));
        //    double crosstalkp2p = Math.Abs(maxStroke - minStroke);
        //    double crosstalkp2plog = 20 * Math.Log10(StrokeRange * 2 / crosstalkp2p);

        //    return new double[4] { crosstalk, crosstalkLog, crosstalkp2p, crosstalkp2plog };


        //    //  return Math.Abs(maxStroke - minStroke);

        //}
        public double CalCrosstalk(List<int> code, List<double> Stroke, List<double> CrossStroke,int CodeMin, int CodeMax,double MinStroke, double MaxStroke, int TargetStrokeCode, int Mode)
        {
            double minStroke = 9999;
            double maxStroke = -9999;
            if (Mode == 0)
            {
                for (int i = 0; i < code.Count; i++)
                {
                    if (code[i] >= CodeMin && code[i] <= CodeMax)
                    {
                        if (minStroke > CrossStroke[i])
                            minStroke = CrossStroke[i];
                        if (maxStroke < CrossStroke[i])
                            maxStroke = CrossStroke[i];
                    }
                }
            }
            else if (Mode == 1)
            {
                for (int i = 0; i < code.Count; i++)
                {
                    if (Stroke[i] >= MinStroke && Stroke[i] <= MaxStroke)
                    {
                        if (minStroke > CrossStroke[i])
                            minStroke = CrossStroke[i];
                        if (maxStroke < CrossStroke[i])
                            maxStroke = CrossStroke[i];
                    }
                }
            }
            if (Mode == 2)
            {
                for (int i = 0; i < code.Count; i++)
                {
                    if (code[i] >= 0 && code[i] <= TargetStrokeCode)
                    {
                        if (minStroke > CrossStroke[i])
                            minStroke = CrossStroke[i];
                        if (maxStroke < CrossStroke[i])
                            maxStroke = CrossStroke[i];
                    }
                }
            }
            return Math.Abs(maxStroke - minStroke);
        }
        public double CalCrosstalkAF(List<int> code, List<double> stroke, List<double> crossstroke,int CodeMin, int CodeMax,double MinStroke, double MaxStroke, int TargetStrokeCode, int Mode)
        {
            double minStroke = 9999;
            double maxStroke = -9999;

            if (Mode == 0)
            {
                for (int i = 0; i < code.Count; i++)
                {
                    if (code[i] >= CodeMin && code[i] <= CodeMax)
                    {
                        if (minStroke > crossstroke[i])
                            minStroke = crossstroke[i];
                        if (maxStroke < crossstroke[i])
                            maxStroke = crossstroke[i];
                    }
                }
            }
            else if (Mode == 1)
            {
                for (int i = 0; i < code.Count; i++)
                {
                    if (stroke[i] >= MinStroke && stroke[i] <= MaxStroke)
                    {
                        if (minStroke > crossstroke[i])
                            minStroke = crossstroke[i];
                        if (maxStroke < crossstroke[i])
                            maxStroke = crossstroke[i];
                    }
                }
            }
            if (Mode == 2)
            {
                for (int i = 0; i < code.Count; i++)
                {
                    if (code[i] >= 0 && code[i] <= TargetStrokeCode)
                    {
                        if (minStroke > crossstroke[i])
                            minStroke = crossstroke[i];
                        if (maxStroke < crossstroke[i])
                            maxStroke = crossstroke[i];
                    }
                }
            }
            return Math.Abs(maxStroke - minStroke);
        }
        public double CalCrosstalkR(List<int> code, List<double> stroke1, List<double> stroke2, int CodeRange, int StrokeRange, int centerCode)
        {
            double minR = 9999;
            double maxR = -9999;

            for (int i = 0; i < code.Count; i++)
            {
                if (code[i] >= centerCode - CodeRange && code[i] <= centerCode + CodeRange)
                {
                    double news = Math.Sqrt( Math.Pow( stroke1[i], 2) + Math.Pow(stroke2[i], 2));
                    if (minR > news)
                        minR = news;
                    if (maxR < news)
                        maxR = news;
                }
            }

            return Math.Abs(maxR - minR);

        }
        public double CalRolling(List<int> x, List<double> y, int CodeRange, int StrokeRange, int centerCode)
        {
            double minTilt = 9999;
            double maxTilt = -9999;

            for (int i = 2; i < x.Count; i++)
            {
                if (x[i] >= centerCode - CodeRange && x[i] <= centerCode + CodeRange && y[i] >= -StrokeRange && y[i] <= StrokeRange)
                {
                    if (minTilt > TiltZ[i])
                        minTilt = TiltZ[i];
                    if (maxTilt < TiltZ[i])
                        maxTilt = TiltZ[i];
                }
            }

            return Math.Abs(maxTilt - minTilt) / 60.0;

        }
        public (double, double, double, double[]) CalTilt(List<int> x, List<double> TiltX, List<double> TiltY, int CodeMin, int CodeMax, int RefCode, int DrvMinCode, int DrvMaxCode)
        {

            double fwdMaxX = 0, bwdMaxX = 0;
            double fwdMaxY = 0, bwdMaxY = 0;
            List<CalcList> fwdIndex = new List<CalcList>();
            List<CalcList> bwdIndex = new List<CalcList>();
            double RefFwdTiltX = 0, RefBwdTiltX = 0;
            double RefFwdTiltY = 0, RefBwdtiltY = 0;

            for (int i = 1; i < x.Count; i++)
            {

                if (x[i] == DrvMinCode || x[i] == DrvMaxCode)
                {
                    if (x[i] >= CodeMin && x[i] <= CodeMax)
                    {
                        fwdIndex.Add(new CalcList { code = x[i], Val1 = TiltX[i], Val2 = TiltY[i] });
                        bwdIndex.Add(new CalcList { code = x[i], Val1 = TiltX[i], Val2 = TiltY[i] });
                    }

                }
                else
                {
                    int dy = x[i] - x[i - 1];
                    if (dy > 0)
                    {
                        if (x[i] >= CodeMin && x[i] <= CodeMax)
                            fwdIndex.Add(new CalcList { code = x[i], Val1 = TiltX[i], Val2 = TiltY[i] });
                    }
                    else
                    {
                        if (x[i] >= CodeMin && x[i] <= CodeMax)
                            bwdIndex.Add(new CalcList { code = x[i], Val1 = TiltX[i], Val2 = TiltY[i] });
                    }
                }

            }
            fwdIndex = fwdIndex.OrderBy(a => a.code).ToList();
            bwdIndex = bwdIndex.OrderBy(a => a.code).ToList();
            for (int i = 0; i < fwdIndex.Count - 1; i++)
            {
                if (RefCode >= fwdIndex[i].code && RefCode <= fwdIndex[i + 1].code)
                {
                    if (Math.Abs(fwdIndex[i].code - RefCode) > Math.Abs(fwdIndex[i + 1].code - RefCode))
                    { RefFwdTiltX = fwdIndex[i + 1].Val1; RefFwdTiltY = fwdIndex[i + 1].Val2; }
                    else { RefFwdTiltX = fwdIndex[i].Val1; RefFwdTiltY = fwdIndex[i].Val2; }
                }
            }
            for (int i = 0; i < bwdIndex.Count - 1; i++)
            {
                if (RefCode >= bwdIndex[i].code && RefCode <= bwdIndex[i + 1].code)
                {
                    if (Math.Abs(bwdIndex[i].code - RefCode) > Math.Abs(bwdIndex[i + 1].code - RefCode))
                    { RefBwdTiltX = bwdIndex[i + 1].Val1; RefBwdtiltY = bwdIndex[i + 1].Val2; }
                    else { RefBwdTiltX = bwdIndex[i].Val1; RefBwdtiltY = bwdIndex[i].Val2; }
                }
            }

            double MaxFwdT = 0;
            double MaxBwdT = 0;
            for (int i = 0; i < fwdIndex.Count; i++)
            {
                double tx = fwdIndex[i].Val1 - RefFwdTiltX;
                double ty = fwdIndex[i].Val2 - RefFwdTiltY;
                double t = Math.Sqrt(Math.Pow(tx, 2) + Math.Pow(ty, 2));
                if (MaxFwdT < t)
                {
                    MaxFwdT = t;
                    fwdMaxX = fwdIndex[i].Val1;
                    fwdMaxY = fwdIndex[i].Val2;
                }

            }
            for (int i = 0; i < bwdIndex.Count; i++)
            {
                double tx = bwdIndex[i].Val1 - RefBwdTiltX;
                double ty = bwdIndex[i].Val2 - RefBwdtiltY;
                double t = Math.Sqrt(Math.Pow(tx, 2) + Math.Pow(ty, 2));
                if (MaxBwdT < t)
                {
                    MaxBwdT = t;
                    bwdMaxX = bwdIndex[i].Val1;
                    bwdMaxY = bwdIndex[i].Val2;
                }
            }
            double[] refArr = new double[2];

            if (MaxFwdT >= MaxBwdT)
            {

                return (MaxFwdT, fwdMaxX, fwdMaxY, refArr = new double[2] { RefFwdTiltX, RefFwdTiltY });
            }
            else
            {
                return (MaxBwdT, bwdMaxX, bwdMaxY, refArr = new double[2] { RefBwdTiltX, RefBwdtiltY });
            }



        }
        public (double, double, double, double[]) CalTilt(
     List<int> x,
     List<double> TiltX,
     List<double> TiltY,
     int CodeMin,
     int CodeMax,
     int TargetStrokeCode,
     int RefCode,
     int DrvMinCode,
     int DrvMaxCode,
     int Mode)
        {
            double fwdMaxX = 0;
            double bwdMaxX = 0;
            double fwdMaxY = 0;
            double bwdMaxY = 0;

            List<CalcList> fwdIndex = new List<CalcList>();
            List<CalcList> bwdIndex = new List<CalcList>();

            double RefFwdTiltX = 0;
            double RefBwdTiltX = 0;
            double RefFwdTiltY = 0;
            double RefBwdtiltY = 0;

            for (int i = 1; i < x.Count; i++)
            {
                bool isInRange = IsTiltRange(
                    x[i],
                    CodeMin,
                    CodeMax,
                    TargetStrokeCode,
                    Mode);

                if (!isInRange)
                    continue;

                if (x[i] == DrvMinCode || x[i] == DrvMaxCode)
                {
                    fwdIndex.Add(new CalcList
                    {
                        code = x[i],
                        Val1 = TiltX[i],
                        Val2 = TiltY[i]
                    });

                    bwdIndex.Add(new CalcList
                    {
                        code = x[i],
                        Val1 = TiltX[i],
                        Val2 = TiltY[i]
                    });
                }
                else
                {
                    int dy = x[i] - x[i - 1];

                    if (dy > 0)
                    {
                        fwdIndex.Add(new CalcList
                        {
                            code = x[i],
                            Val1 = TiltX[i],
                            Val2 = TiltY[i]
                        });
                    }
                    else
                    {
                        bwdIndex.Add(new CalcList
                        {
                            code = x[i],
                            Val1 = TiltX[i],
                            Val2 = TiltY[i]
                        });
                    }
                }
            }

            fwdIndex = fwdIndex
                .OrderBy(a => a.code)
                .ToList();

            bwdIndex = bwdIndex
                .OrderBy(a => a.code)
                .ToList();

            for (int i = 0; i < fwdIndex.Count - 1; i++)
            {
                if (RefCode >= fwdIndex[i].code &&
                    RefCode <= fwdIndex[i + 1].code)
                {
                    if (Math.Abs(fwdIndex[i].code - RefCode) >
                        Math.Abs(fwdIndex[i + 1].code - RefCode))
                    {
                        RefFwdTiltX = fwdIndex[i + 1].Val1;
                        RefFwdTiltY = fwdIndex[i + 1].Val2;
                    }
                    else
                    {
                        RefFwdTiltX = fwdIndex[i].Val1;
                        RefFwdTiltY = fwdIndex[i].Val2;
                    }
                }
            }

            for (int i = 0; i < bwdIndex.Count - 1; i++)
            {
                if (RefCode >= bwdIndex[i].code &&
                    RefCode <= bwdIndex[i + 1].code)
                {
                    if (Math.Abs(bwdIndex[i].code - RefCode) >
                        Math.Abs(bwdIndex[i + 1].code - RefCode))
                    {
                        RefBwdTiltX = bwdIndex[i + 1].Val1;
                        RefBwdtiltY = bwdIndex[i + 1].Val2;
                    }
                    else
                    {
                        RefBwdTiltX = bwdIndex[i].Val1;
                        RefBwdtiltY = bwdIndex[i].Val2;
                    }
                }
            }

            double MaxFwdT = 0;
            double MaxBwdT = 0;

            for (int i = 0; i < fwdIndex.Count; i++)
            {
                double tx = fwdIndex[i].Val1 - RefFwdTiltX;
                double ty = fwdIndex[i].Val2 - RefFwdTiltY;

                double t = Math.Sqrt(
                    Math.Pow(tx, 2) +
                    Math.Pow(ty, 2));

                if (MaxFwdT < t)
                {
                    MaxFwdT = t;
                    fwdMaxX = fwdIndex[i].Val1;
                    fwdMaxY = fwdIndex[i].Val2;
                }
            }

            for (int i = 0; i < bwdIndex.Count; i++)
            {
                double tx = bwdIndex[i].Val1 - RefBwdTiltX;
                double ty = bwdIndex[i].Val2 - RefBwdtiltY;

                double t = Math.Sqrt(
                    Math.Pow(tx, 2) +
                    Math.Pow(ty, 2));

                if (MaxBwdT < t)
                {
                    MaxBwdT = t;
                    bwdMaxX = bwdIndex[i].Val1;
                    bwdMaxY = bwdIndex[i].Val2;
                }
            }

            if (MaxFwdT >= MaxBwdT)
            {
                return (
                    MaxFwdT,
                    fwdMaxX,
                    fwdMaxY,
                    new double[2]
                    {
                RefFwdTiltX,
                RefFwdTiltY
                    });
            }

            return (
                MaxBwdT,
                bwdMaxX,
                bwdMaxY,
                new double[2]
                {
            RefBwdTiltX,
            RefBwdtiltY
                });
        }

        private bool IsTiltRange(
            int code,
            int codeMin,
            int codeMax,
            int targetStrokeCode,
            int mode)
        {
            switch (mode)
            {
                case 0:
                    {
                        int minCode = Math.Min(codeMin, codeMax);
                        int maxCode = Math.Max(codeMin, codeMax);

                        return code >= minCode &&
                               code <= maxCode;
                    }

                case 1:
                    {
                        int minCode = Math.Min(0, targetStrokeCode);
                        int maxCode = Math.Max(0, targetStrokeCode);

                        return code >= minCode &&
                               code <= maxCode;
                    }

                case 2:
                    {
                        int minCode = Math.Min(codeMin, targetStrokeCode);
                        int maxCode = Math.Max(codeMin, targetStrokeCode);

                        return code >= minCode &&
                               code <= maxCode;
                    }

                default:
                    return false;
            }
        }
        public double CalSlopeForOISShift(List<int> x, List<double> y)
        {

            List<SPoint> pt = new List<SPoint>();

            for (int i = 0; i < x.Count; i++)
            {
                if (i >= 15 && i <= 20)
                {
                    pt.Add(new SPoint { x = x[i], y = y[i] });

                }
            }
            SLine res = Line_fitting(pt);
            return res.dSlope;

        }


    }
    public class ChartList
    {
        public Chart C = new Chart();
        public string Title = "";
        public string Type = "";
        public int Ch = 0;
        public bool IsFalg = false;
        public bool IsAEnable = true;
        public int StrokeNum = 0;
        public int TiltNum = 0;

        public Rectangle OldPt;
        public ChartList(string type, int ch)
        {
            Type = type;
            Ch = ch;
            C.ChartAreas.Add(new ChartArea());
            C.ChartAreas[0].Position.X = 0;
            C.ChartAreas[0].Position.Y = 0;
            C.ChartAreas[0].Position.Height = 99;
            C.ChartAreas[0].Position.Width = 100;
            C.ChartAreas[0].AxisX.LabelStyle.Font = new Font("Calibri", 9, FontStyle.Bold);
            C.ChartAreas[0].AxisY.LabelStyle.Font = new Font("Calibri", 9, FontStyle.Bold);
            C.ChartAreas[0].AxisX.ScaleView.Position = 0;
            C.ChartAreas[0].AxisY.ScaleView.Position = 0;
            C.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.LightGray;
            C.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.LightGray;
            C.ChartAreas[0].AxisX.MinorGrid.LineColor = Color.WhiteSmoke;
            C.ChartAreas[0].AxisY.MinorGrid.LineColor = Color.WhiteSmoke;
            C.ChartAreas[0].AxisX.LineColor = Color.DarkGray;
            C.ChartAreas[0].AxisY.LineColor = Color.DarkGray;
            C.BackColor = SystemColors.ControlLightLight;
            C.Legends.Add(new Legend());
            C.Legends[0].Position = new ElementPosition(5, 0, 30, 20);
            C.Legends[0].BackColor = Color.Transparent;
            C.Size = new System.Drawing.Size(475, 280);
            C.MouseDoubleClick += new MouseEventHandler(MouseDoubleClick);
            C.Tag = "S";
            int numSeries = 0;
            if (type == "Stroke")
            {
                C.ChartAreas[0].AxisY.MinorGrid.Interval = .1;
                C.ChartAreas[0].AxisX.MinorGrid.Enabled = false;
                C.ChartAreas[0].AxisY.MinorGrid.Enabled = false;
                C.ChartAreas[0].AxisX.Minimum = 0;
                C.ChartAreas[0].AxisX.Maximum = 4100;
                C.ChartAreas[0].AxisX.Interval = 400;
                C.ChartAreas[0].AxisY.Minimum = 0;
                C.ChartAreas[0].AxisY.Maximum = 1500;
                C.ChartAreas[0].AxisY.Interval = 150;
                C.Titles.Add("Stroke vs Code");
                //  Stroke, Current, Cross Stroke
                C.Series.Add("X Code Stroke"); //0
                C.Series[numSeries].Label = "X Code Stroke";
                C.Series[numSeries].ChartType = SeriesChartType.FastLine;
                C.Series[numSeries].Color = Color.Red;
                C.Series[numSeries].IsVisibleInLegend = true;
                numSeries++;
                C.Series.Add("Y Code Stroke"); //1
                C.Series[numSeries].Label = "Y Code Stroke";
                C.Series[numSeries].ChartType = SeriesChartType.FastLine;
                C.Series[numSeries].Color = Color.Blue;
                C.Series[numSeries].IsVisibleInLegend = true;
                numSeries++;
                C.Series.Add("AF Code Stroke"); //2
                C.Series[numSeries].Label = "AF Code Stroke";
                C.Series[numSeries].ChartType = SeriesChartType.FastLine;
                C.Series[numSeries].Color = Color.Green;
                C.Series[numSeries].IsVisibleInLegend = true;
                numSeries++;
                C.Series.Add("X Current"); //3
                C.Series[numSeries].Label = "X Current";
                C.Series[numSeries].ChartType = SeriesChartType.FastLine;
                C.Series[numSeries].Color = Color.LightPink;
                C.Series[numSeries].IsVisibleInLegend = true;
                C.Series[numSeries].YAxisType = AxisType.Secondary;
                numSeries++;
                C.Series.Add("Y Current"); //4
                C.Series[numSeries].Label = "Y Current";
                C.Series[numSeries].ChartType = SeriesChartType.FastLine;
                C.Series[numSeries].Color = Color.Turquoise;
                C.Series[numSeries].IsVisibleInLegend = true;
                C.Series[numSeries].YAxisType = AxisType.Secondary;
                numSeries++;
                C.Series.Add("AF Current"); //5
                C.Series[numSeries].Label = "AF Current";
                C.Series[numSeries].ChartType = SeriesChartType.FastLine;
                C.Series[numSeries].Color = Color.Violet;
                C.Series[numSeries].IsVisibleInLegend = true;
                C.Series[numSeries].YAxisType = AxisType.Secondary;
                numSeries++;
                C.Series.Add("X Hall"); //6 
                C.Series[numSeries].Label = "X Hall";
                C.Series[numSeries].ChartType = SeriesChartType.FastLine;
                C.Series[numSeries].Color = Color.Bisque;
                C.Series[numSeries].IsVisibleInLegend = true;
                C.Series[numSeries].YAxisType = AxisType.Secondary;
                numSeries++;
                C.Series.Add("Y Hall"); //7
                C.Series[numSeries].Label = "Y Hall";
                C.Series[numSeries].ChartType = SeriesChartType.FastLine;
                C.Series[numSeries].Color = Color.LightSkyBlue;
                C.Series[numSeries].IsVisibleInLegend = true;
                C.Series[numSeries].YAxisType = AxisType.Secondary;
                numSeries++;
                C.Series.Add("AF Hall"); //8
                C.Series[numSeries].Label = "AF Hall";
                C.Series[numSeries].ChartType = SeriesChartType.FastLine;
                C.Series[numSeries].Color = Color.LightSlateGray;
                C.Series[numSeries].IsVisibleInLegend = true;
                C.Series[numSeries].YAxisType = AxisType.Secondary;
                numSeries++;
                C.Series.Add("Y1 Code Stroke"); //9
                C.Series[numSeries].Label = "Y1 Code Stroke";
                C.Series[numSeries].ChartType = SeriesChartType.FastLine;
                C.Series[numSeries].Color = Color.BlueViolet;
                C.Series[numSeries].IsVisibleInLegend = true;
                numSeries++;
                C.Series.Add("Y2 Code Stroke"); //10
                C.Series[numSeries].Label = "Y2 Code Stroke";
                C.Series[numSeries].ChartType = SeriesChartType.FastLine;
                C.Series[numSeries].Color = Color.DarkBlue;
                C.Series[numSeries].IsVisibleInLegend = true;
            }
            else if (type == "Settling")
            {
                C.ChartAreas[0].AxisY.MinorGrid.Interval = .1;
                C.ChartAreas[0].AxisX.MinorGrid.Enabled = false;
                C.ChartAreas[0].AxisY.MinorGrid.Enabled = false;
                C.ChartAreas[0].AxisX.Minimum = 0;
                C.ChartAreas[0].AxisX.Maximum = 4100;
                C.ChartAreas[0].AxisX.Interval = 400;
                C.ChartAreas[0].AxisY.Minimum = -40;
                C.ChartAreas[0].AxisY.Maximum = 40;
                C.ChartAreas[0].AxisY.Interval = 8;

                C.Titles.Add("Settling vs time");

                C.Series.Add("AF Settle");
                C.Series[numSeries].Label = "AF Settle";
                C.Series[numSeries].ChartType = SeriesChartType.FastLine;
                C.Series[numSeries].Color = Color.Red;
                C.Series[numSeries].IsVisibleInLegend = true;
                numSeries++;
                C.Series.Add("AF Settle2");
                C.Series[numSeries].Label = "AF Settle2";
                C.Series[numSeries].ChartType = SeriesChartType.FastLine;
                C.Series[numSeries].Color = Color.Blue;
                C.Series[numSeries].IsVisibleInLegend = true;
                //numSeries++;
                //C.Series.Add("X Tz");
                //C.Series[numSeries].Label = "X Tz";
                //C.Series[numSeries].ChartType = SeriesChartType.FastLine;
                //C.Series[numSeries].Color = Color.Red;
                //C.Series[numSeries].IsVisibleInLegend = true;
                //C.Series[numSeries].YAxisType = AxisType.Secondary;
                //numSeries++;
                //C.Series.Add("Y Tx");
                //C.Series[numSeries].Label = "Y Tx";
                //C.Series[numSeries].ChartType = SeriesChartType.FastLine;
                //C.Series[numSeries].Color = Color.Blue;
                //C.Series[numSeries].IsVisibleInLegend = true;
                //numSeries++;
                //C.Series.Add("Y Ty");
                //C.Series[numSeries].Label = "Y Ty";
                //C.Series[numSeries].ChartType = SeriesChartType.FastLine;
                //C.Series[numSeries].Color = Color.BlueViolet;
                //C.Series[numSeries].IsVisibleInLegend = true;
                //numSeries++;
                //C.Series.Add("Y Tz");
                //C.Series[numSeries].Label = "Y Tz";
                //C.Series[numSeries].ChartType = SeriesChartType.FastLine;
                //C.Series[numSeries].Color = Color.Blue;
                //C.Series[numSeries].IsVisibleInLegend = true;
                //C.Series[numSeries].YAxisType = AxisType.Secondary;
                //numSeries++;
                //C.Series.Add("AF Tx");
                //C.Series[numSeries].Label = "AF Tx";
                //C.Series[numSeries].ChartType = SeriesChartType.FastLine;
                //C.Series[numSeries].Color = Color.Green;
                //C.Series[numSeries].IsVisibleInLegend = true;
                //numSeries++;
                //C.Series.Add("AF Ty");
                //C.Series[numSeries].Label = "AF Ty";
                //C.Series[numSeries].ChartType = SeriesChartType.FastLine;
                //C.Series[numSeries].Color = Color.GreenYellow;
                //C.Series[numSeries].IsVisibleInLegend = true;
                //numSeries++;
                //C.Series.Add("AF Tz");
                //C.Series[numSeries].Label = "AF Tz";
                //C.Series[numSeries].ChartType = SeriesChartType.FastLine;
                //C.Series[numSeries].Color = Color.Green;
                //C.Series[numSeries].IsVisibleInLegend = true;
                //C.Series[numSeries].YAxisType = AxisType.Secondary;
            }//Tilt
        }
        private void MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.X >= (C.Width - 40))
            {
                //if (IsAEnable)
                //{
                //    foreach (var c in C.Series) if (c.YAxisType == AxisType.Primary) c.Enabled = false;
                //    IsAEnable = false;
                //}
                //else
                //{
                //    foreach (var c in C.Series) c.Enabled = true;
                //    IsAEnable = true;
                //}
                foreach (var c in C.Series) c.Enabled = false;
                if (StrokeNum == 0) foreach (var c in C.Series) if (c.Name.Contains("X")) c.Enabled = true;
                if (StrokeNum == 1) foreach (var c in C.Series) if (c.Name.Contains("Y")) c.Enabled = true;
                if (StrokeNum == 2) foreach (var c in C.Series) if (c.Name.Contains("AF")) c.Enabled = true;
                if (StrokeNum == 3) foreach (var c in C.Series) c.Enabled = true;
                StrokeNum++;
                if (StrokeNum > 3) StrokeNum = 0;
                return;
            }
            if (e.X <= 40)
            {
                //if (IsAEnable)
                //{
                //    foreach (var c in C.Series) if (c.YAxisType != AxisType.Primary) c.Enabled = false;
                //    IsAEnable = false;
                //}
                //else
                //{
                //    foreach (var c in C.Series) c.Enabled = true;
                //    IsAEnable = true;
                //}
                return;
            }
            if (C.Tag.ToString() == "S")
            {
                OldPt.Width = C.Width;
                OldPt.Height = C.Height;
                OldPt.X = C.Left;
                OldPt.Y = C.Top;

                C.Width = 953;
                C.Height = 563;
                C.Left = 3 + (Ch / 2) * 953;
                C.Top = 117;
                Title = C.Titles[0].Text;
                C.Titles[0].Text = Title + " Ch " + Ch.ToString();
                C.Titles[0].Font = new Font("Malgun Gothic", 14, FontStyle.Bold); ;
                C.ChartAreas[0].AxisY.MinorGrid.Enabled = true;
                C.ChartAreas[0].AxisY.LabelStyle.Format = "0";
                if (Type == "Stroke")
                    C.Legends[0].Position = new ElementPosition(5, 0, 30, 50);
                else
                    C.Legends[0].Position = new ElementPosition(5, 0, 20, 20);
                C.BringToFront();
                C.Tag = "L";
            }
            else
            {
                C.Width = OldPt.Width;
                C.Height = OldPt.Height;
                C.Left = OldPt.X;
                C.Top = OldPt.Y;
                C.SendToBack();
                C.Titles[0].Text = Title;
                C.Titles[0].Font = new Font("Malgun Gothic", 9, FontStyle.Bold); ;
                C.ChartAreas[0].AxisY.MinorGrid.Enabled = false;
                C.ChartAreas[0].AxisY.LabelStyle.Format = "0";
                C.Legends[0].Position = new ElementPosition(5, 0, 30, 20);
                C.Tag = "S";
            }
        }
    }
    public class CalcList
    {
        public int code { get; set; }
        public double Val1 { get; set; }
        public double Val2 { get; set; }
        public double Val3 { get; set; }
    }
}
