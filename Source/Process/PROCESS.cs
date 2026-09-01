using FZ4P.Commons.Type;
using FZ4P.Processes.Interfaces;
using FZ4P.UI.CustomUI;
using S2System.Vision;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Xml.Linq;

namespace FZ4P
{
    public partial class Process : ILogView
    {
        public IDlnInterface Dln { get { return STATIC.Dln; } }
        public AK73XX DrvIC { get { return STATIC.DrvIC; } }
        public Recipe Rcp { get { return STATIC.Rcp; } }
        public Condition Condition { get { return STATIC.Rcp.Condition; } }
        public Spec Spec { get { return STATIC.Rcp.Spec; } }
        public Option Option { get { return STATIC.Rcp.Option; } }
        public Model Model { get { return STATIC.Rcp.Model; } }
        public CurrentPath Current { get { return STATIC.Rcp.Current; } }
        public List<PassFail> PassFails { get { return STATIC.Rcp.PassFails; } }
        public TotalYield yield { get { return STATIC.Rcp.yield; } }
        

        Global m__G = null;


        public ObservableCollection<ActItems> ItemList = new ObservableCollection<ActItems>();
        public List<NVMHallParam> HallParam = new List<NVMHallParam>();
        public List<Task> RunTasks = new List<Task>();
        public List<ReCount> ActionListReCount { get; set; } = new List<ReCount>();
        public int RunTaskId1 = 0;
        public int RunTaskId2 = 0;

        public bool m_bAllLEDOn = false;
        public bool IsVirtual = false;
        public bool SuddenStop = false;
        public int RepeatRun = 0;
        public int CurrentRun = 0;
        public bool IsHallComplete = false;
        public int PortCnt { get; set; }
        public int ChannelCnt { get; set; }


        public List<string> errMsg = new List<string>();
        public List<bool> m_ChannelOn = new List<bool>();
        public List<string> m_StrIndex = new List<string>();
        public List<bool> IsScan = new List<bool>();
        public List<int> framCnt = new List<int>();
        public List<byte[]> FWCode = new List<byte[]>();
        public List<string> m_strBarcoe = new List<string>();

        public event EventHandler<int> RunStart = null;
        public event EventHandler<int> RunEnd = null;

        public List<LogText> ViewLog = new List<LogText>();

        public List<InfoButton> InfoBtn = new List<InfoButton>();

        public List<DrvParam> DrvValue = new List<DrvParam>();

        public List<List<CalResult>> CalList = new List<List<CalResult>>();

        public ucBarcodePannel BarcodePannel = new ucBarcodePannel()
        {
            Location = new System.Drawing.Point(100, 500)
        };

        public DataGridView ResultDataGrid = new DataGridView()
        { Size = new System.Drawing.Size(780, 828) };
        public Label lblFailList = new Label();
        public List<ChartList> ChartTop = new List<ChartList>();
        public List<ChartList> ChartBtm = new List<ChartList>();
        public List<TiltGraph> tiltChart = new List<TiltGraph>();

        public ActroPannel ProcessTitle = new ActroPannel();

        //    public List<ChartList> ChartBtm = new List<ChartList>();
        public int BestAFPos = 731;
        //public int OISCenter = 2048;
        public int OISCenter = 4096;
        public int AFCenter = 2048;
        double SlopeX = 0;
        double SlopeY = 0;
        public bool I2CMonitorStartFlag = false;
        bool isI2cMonitoring = false;
        public int AfVer = 0;
        public int OisVer = 0;
        public Process()
        {
            PortCnt = 1;
            ChannelCnt = 1;

            for (int i = 0; i < PortCnt; i++)
            {

                IsScan.Add(false);
                framCnt.Add(0);
            }
            for (int i = 0; i < ChannelCnt; i++)
            {
                errMsg.Add("");
                m_ChannelOn.Add(false);
                m_StrIndex.Add("");
                HallParam.Add(new NVMHallParam());
                DrvValue.Add(new DrvParam());
                m_strBarcoe.Add("");
                CalList.Add(new List<CalResult>());
                CalList[i].Add(new CalResult("AF Scan"));
                CalList[i].Add(new CalResult("AF Settling"));
                CalList[i].Add(new CalResult("AF Settling2"));
                CalList[i].Add(new CalResult("AF Settling3"));
                CalList[i].Add(new CalResult("OIS X Scan"));
                CalList[i].Add(new CalResult("OIS Y Scan"));
                CalList[i].Add(new CalResult("OIS X Scan Mac"));
                CalList[i].Add(new CalResult("OIS Y Scan Mac"));
                CalList[i].Add(new CalResult("Circle"));

                ChartTop.Add(new ChartList("Stroke", i));
                ChartBtm.Add(new ChartList("Settling", i));
                tiltChart.Add(new TiltGraph
                {
                    title = "AF Tilt",
                    range = 15,
                });
                tiltChart[i].SetRings(new double[] { tiltChart[i].range / 2, tiltChart[i].range });


                InfoBtn.Add(new InfoButton()); //test
                InfoBtn.Add(new InfoButton());
                ViewLog.Add(new LogText());
            }
            //ItemList.Add(new ActItems() { Name = "AF Scan", Func = Act_ScanCode });
            //ItemList.Add(new ActItems() { Name = "OIS X Scan", Func = Act_ScanCode });
            //ItemList.Add(new ActItems() { Name = "OIS Y Scan", Func = Act_ScanCode });
            //ItemList.Add(new ActItems() { Name = "OIS X Scan Mac", Func = Act_ScanCode });
            //ItemList.Add(new ActItems() { Name = "OIS Y Scan Mac", Func = Act_ScanCode });
            //ItemList.Add(new ActItems() { Name = "AF Settling", Func = Act_ScanTimeCode });
            //ItemList.Add(new ActItems() { Name = "AF Settling2", Func = Act_ScanTimeCode2 });
            //ItemList.Add(new ActItems() { Name = "AF Settling3", Func = Act_ScanTimeCode3 });

            AddSequence();

            Rcp.RetryCnt = new RetryCount();
            Rcp.RetryCnt.RetryOption.Add(new Retry { InspName = "All", Count = 0 });

            for (int i = 0; i < ItemList.Count; i++)
                Rcp.RetryCnt.RetryOption.Add(new Retry { InspName = ItemList[i].Name, Count = 0 });
            if (File.Exists(STATIC.RetryCountDir))
            {
                RetryCount compare = new RetryCount();
                compare = DataIO.DeserializeXMLFileToObject<RetryCount>(STATIC.RetryCountDir);
                for (int i = 0; i < compare.RetryOption.Count; i++)
                {
                    int index = Rcp.RetryCnt.RetryOption.FindIndex(x => x.InspName == compare.RetryOption[i].InspName);
                    if (index != -1) Rcp.RetryCnt.RetryOption[index].Count = compare.RetryOption[i].Count;
                }
            }

            m__G = Global.GetInstance();
        }
        #region Default
        void MonitorI2C()
        {
            //if (IsVirtual)
            //{
            //    isI2cMonitoring = false;
            //    return;
            //}


            while (true)
            {
                if (!I2CMonitorStartFlag) { m__G.mIDLEcount = 0; break; }
                Thread.Sleep(5000);
                if (!I2CMonitorStartFlag) { m__G.mIDLEcount = 0; break; }
                if (!Dln.IsRun)
                {
                    m__G.mIDLEcount++;
                    if (m__G.mIDLEcount > 7)
                    {
                        List<double> led = new List<double>() { 0.5, 0.5 };
                        LEDs_All_On(0, true, led);
                        Thread.Sleep(1);
                        if (m__G.mIDLEcount > 7)
                        {
                            if (!Dln.IsRun)
                            {
                                LEDs_All_On(0, false);
                                m__G.mIDLEcount = 0;
                            }
                        }

                    }
                }
                else
                {
                    m__G.mIDLEcount = 0;
                }
                if (!I2CMonitorStartFlag) { m__G.mIDLEcount = 0; break; }
            }
            isI2cMonitoring = false;
        }
        public bool CheckFail(int ch, string Item)
        {
            for (int i = 0; i < Spec.specList.Count; i++)
            {
                if (Spec.specList[i].Category == Item)
                {
                    if (!PassFails[ch].Results[i].bPass) return false;
                }
            }
            return true;
        }
        public void SetFailList(int ch)
        {
            if (lblFailList.InvokeRequired)
            {
                lblFailList.BeginInvoke((MethodInvoker)delegate
                {
                    for (int i = 0; i < Spec.specList.Count; i++)
                    {
                        if (!PassFails[ch].Results[i].bPass) { STATIC.FailNumber += $"{i + 1},"; lblFailList.Text = STATIC.FailNumber; }

                    }

                });
            }
            else
            {
                for (int i = 0; i < Spec.specList.Count; i++)
                {
                    if (!PassFails[ch].Results[i].bPass) { STATIC.FailNumber += $"{i + 1},"; lblFailList.Text = STATIC.FailNumber; }
                }
            }
        }
        public void ShowDataResults(int ch, int start, int end, InspType type, double[] MtoMRes)
        {
            bool bothXYFail = false;

            if (type == InspType.BothXYFail)
            {
                int xIndex = start;
                int yIndex = end;

                double xMin = Convert.ToDouble(Spec.specList[xIndex].MinSpec);
                double xMax = Convert.ToDouble(Spec.specList[xIndex].MaxSpec);

                double yMin = Convert.ToDouble(Spec.specList[yIndex].MinSpec);
                double yMax = Convert.ToDouble(Spec.specList[yIndex].MaxSpec);

                double xValue = PassFails[ch].Results[xIndex].Val;
                double yValue = PassFails[ch].Results[yIndex].Val;

                bool xNaN = double.IsNaN(xValue);
                bool yNaN = double.IsNaN(yValue);

                bool xFail = xValue < xMin || xValue > xMax;
                bool yFail = yValue < yMin || yValue > yMax;

                // NaN은 측정 실패이므로 단독으로도 최종 NG 처리
                bothXYFail = xNaN || yNaN || (xFail && yFail);
            }

            for (int i = start; i < end + 1; i++)
            {
                if (!Spec.specList[i].OnOff) continue;

                double lmin, lmax;
                lmin = Convert.ToDouble(Spec.specList[i].MinSpec);
                lmax = Convert.ToDouble(Spec.specList[i].MaxSpec);

                switch (type)
                {
                    case InspType.Normal:
                        if (PassFails[ch].Results[i].Val < lmin || PassFails[ch].Results[i].Val > lmax || double.IsNaN(PassFails[ch].Results[i].Val))
                        {
                            PassFails[ch].Results[i].msg = Spec.specList[i].DisplayName;
                            PassFails[ch].Results[i].bPass = false;

                        }
                        else
                        {
                            PassFails[ch].Results[i].msg = "";
                            PassFails[ch].Results[i].bPass = true;

                        }
                        break;
                    case InspType.OKNG:
                        if (PassFails[ch].Results[i].Val != 0)
                        {
                            PassFails[ch].Results[i].msg = Spec.specList[i].DisplayName;
                            PassFails[ch].Results[i].bPass = false;

                        }
                        else
                        {
                            PassFails[ch].Results[i].msg = "";
                            PassFails[ch].Results[i].bPass = true;

                        }
                        break;
                    case InspType.OnlyMax:
                        if (PassFails[ch].Results[i].Val > lmax || double.IsNaN(PassFails[ch].Results[i].Val))
                        {
                            PassFails[ch].Results[i].msg = Spec.specList[i].DisplayName;
                            PassFails[ch].Results[i].bPass = false;

                        }
                        else
                        {
                            PassFails[ch].Results[i].msg = "";
                            PassFails[ch].Results[i].bPass = true;

                        }
                        break;
                    case InspType.OnlyMin:
                        if (PassFails[ch].Results[i].Val < lmin || double.IsNaN(PassFails[ch].Results[i].Val))
                        {
                            PassFails[ch].Results[i].msg = Spec.specList[i].DisplayName;
                            PassFails[ch].Results[i].bPass = false;

                        }
                        else
                        {
                            PassFails[ch].Results[i].msg = "";
                            PassFails[ch].Results[i].bPass = true;

                        }
                        break;
                    case InspType.MintoMax:

                        if (MtoMRes[1] < lmin || MtoMRes[0] > lmax || double.IsNaN(MtoMRes[0]) || double.IsNaN(MtoMRes[1]))
                        {
                            PassFails[ch].Results[i].msg = Spec.specList[i].DisplayName;
                            PassFails[ch].Results[i].bPass = false;

                        }
                        else
                        {
                            PassFails[ch].Results[i].msg = "";
                            PassFails[ch].Results[i].bPass = true;

                        }
                        break;
                }



            }
            for (int i = start; i < end + 1; i++)
            {
                if (!PassFails[ch].Results[i].bPass)
                {
                    if (PassFails[ch].FirstFailIndex == 0)
                    {
                        PassFails[ch].FirstFailIndex = (i + 1);
                        PassFails[ch].FirstFail = PassFails[ch].Results[i].msg;

                        int failCnt = Convert.ToInt32(Spec.specList[i].FailCnt); failCnt++;
                        Spec.specList[i].FailCnt = failCnt;
                    }


                }
            }

            if (ResultDataGrid.InvokeRequired)
            {
                ResultDataGrid.BeginInvoke((MethodInvoker)delegate
                {
                    for (int i = start; i <= end; i++)
                    {
                        if (type == InspType.MintoMax)
                            ResultDataGrid[ch + 4, i].Value = $"{MtoMRes[1]} ~ {MtoMRes[0]}";
                        else if (type == InspType.OKNG)
                        {
                            if (PassFails[ch].Results[i].Val == 0)
                                ResultDataGrid[ch + 4, i].Value = "OK";
                            else ResultDataGrid[ch + 4, i].Value = "NG";
                        }
                        else
                        {
                            ResultDataGrid[ch + 4, i].Value = PassFails[ch].Results[i].Val.ToString("F3");

                        }
                        if (PassFails[ch].Results[i].bPass) { ResultDataGrid[ch + 4, i].Style.BackColor = Color.White; ResultDataGrid[ch, i].Style.BackColor = Color.White; }
                        else { ResultDataGrid[ch + 4, i].Style.BackColor = Color.Orange; ResultDataGrid[ch, i].Style.BackColor = Color.Orange; }


                    }

                });
            }
            else
            {
                for (int i = start; i <= end; i++)
                {
                    if (type == InspType.MintoMax)
                        ResultDataGrid[ch + 4, i].Value = $"{MtoMRes[1]} ~ {MtoMRes[0]}";
                    else if (type == InspType.OKNG)
                    {
                        if (PassFails[ch].Results[i].Val == 0)
                            ResultDataGrid[ch + 4, i].Value = "OK";
                        else ResultDataGrid[ch + 4, i].Value = "NG";
                    }
                    else
                    {
                        ResultDataGrid[ch + 4, i].Value = PassFails[ch].Results[i].Val.ToString("F3");
                    }
                    if (PassFails[ch].Results[i].bPass) { ResultDataGrid[ch + 4, i].Style.BackColor = Color.White; ResultDataGrid[ch, i].Style.BackColor = Color.White; }
                    else { ResultDataGrid[ch + 4, i].Style.BackColor = Color.Orange; ResultDataGrid[ch, i].Style.BackColor = Color.Orange; }

                }
            }


            for (int i = start; i <= end; i++)
            {
                if (!PassFails[ch].Results[i].bPass)
                {
                    //if (!Option.ContinueTestingOnFail) m_ChannelOn[ch] = false;
                }


            }

        }
        public void InitResultData()
        {
            Type dgvType = ResultDataGrid.GetType();
            PropertyInfo pi = dgvType.GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
            pi.SetValue(ResultDataGrid, true, null);

            ResultDataGrid.AllowUserToAddRows = false;
            ResultDataGrid.AllowUserToDeleteRows = false;
            ResultDataGrid.AllowUserToResizeColumns = false;
            ResultDataGrid.AllowUserToResizeRows = false;
            ResultDataGrid.Tag = "S";
            ResultDataGrid.ColumnCount = 6; //  Group, Item, min, max, r0, r1, r2, r3, unit, Fratio
            ResultDataGrid.Font = new Font("Calibri", 10, FontStyle.Bold);
            for (int i = 0; i < ResultDataGrid.ColumnCount; i++)
            {
                ResultDataGrid.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
            }
            ResultDataGrid.RowHeadersVisible = false;
            ResultDataGrid.BackgroundColor = Color.LightGray;

            //// Column
            //    ResultDataGrid.Columns[0].Name = "Axis";
            ResultDataGrid.Columns[0].Name = "Item No.";
            ResultDataGrid.Columns[1].Name = "Item Name";
            ResultDataGrid.Columns[2].Name = "Min";
            ResultDataGrid.Columns[3].Name = "Max";
            ResultDataGrid.Columns[4].Name = "Result";
            //  ResultDataGrid.Columns[5].Name = "#2 Result";
            ResultDataGrid.Columns[5].Name = "unit";

            //   ResultDataGrid.Columns[0].DefaultCellStyle.Alignment = DataGridViewContentAlignment.TopCenter;
            ResultDataGrid.Columns[0].DefaultCellStyle.Alignment = DataGridViewContentAlignment.TopCenter;
            ResultDataGrid.Columns[1].DefaultCellStyle.Alignment = DataGridViewContentAlignment.TopLeft;
            ResultDataGrid.Columns[2].DefaultCellStyle.Alignment = DataGridViewContentAlignment.TopRight;
            ResultDataGrid.Columns[3].DefaultCellStyle.Alignment = DataGridViewContentAlignment.TopRight;
            ResultDataGrid.Columns[4].DefaultCellStyle.Alignment = DataGridViewContentAlignment.TopRight;
            ResultDataGrid.Columns[5].DefaultCellStyle.Alignment = DataGridViewContentAlignment.TopLeft;

            //    ResultDataGrid.Columns[0].Width = 150;
            ResultDataGrid.Columns[0].Width = 70;
            ResultDataGrid.Columns[1].Width = 215;
            ResultDataGrid.Columns[2].Width = 70;
            ResultDataGrid.Columns[3].Width = 70;
            ResultDataGrid.Columns[4].Width = 100;
            ResultDataGrid.Columns[5].Width = 65;

            ResultDataGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            ResultDataGrid.ColumnHeadersHeight = 28;


            ResultDataGrid.Rows.Clear();
            for (int i = 0; i < Spec.specList.Count; i++)
            {
                switch (Spec.specList[i].InspectionType)
                {
                    case InspType.Normal:
                    case InspType.MintoMax:
                        ResultDataGrid.Rows.Add(i + 1, Spec.specList[i].DisplayName, Spec.specList[i].MinSpec, Spec.specList[i].MaxSpec, "", Spec.specList[i].Unit);
                        break;
                    case InspType.OnlyMax:
                        ResultDataGrid.Rows.Add(i + 1, Spec.specList[i].DisplayName, "", Spec.specList[i].MaxSpec, "", Spec.specList[i].Unit);
                        break;
                    case InspType.OnlyMin:
                        ResultDataGrid.Rows.Add(i + 1, Spec.specList[i].DisplayName, Spec.specList[i].MinSpec, "", "", Spec.specList[i].Unit);
                        break;
                    case InspType.OKNG:
                        ResultDataGrid.Rows.Add(i + 1, Spec.specList[i].DisplayName, "", "", "", Spec.specList[i].Unit);
                        break;


                }

                ResultDataGrid.Rows[i].Visible = Convert.ToBoolean(Spec.specList[i].OnOff);
                for (int k = 0; k < ResultDataGrid.ColumnCount; k++) ResultDataGrid[k, i].Style.BackColor = Color.White;

                ResultDataGrid.Rows[i].Height = 22;
                ResultDataGrid.Rows[i].Resizable = DataGridViewTriState.False;
                ResultDataGrid.Rows[i].DefaultCellStyle.Font = new Font("Calibri", 10, FontStyle.Bold);
                ResultDataGrid[0, i].Style.Font = new Font("Calibri", 10, FontStyle.Bold);
                ResultDataGrid[2, i].Style.Font = new Font("Calibri", 10, FontStyle.Bold);
                ResultDataGrid[5, i].Style.Font = new Font("Calibri", 10, FontStyle.Italic);

                ResultDataGrid.ReadOnly = true;
            }

            //string old = string.Empty;/*ResultGrid.Rows[0].Cells[0].Value.ToString();*/
            //for (int i = 0; i < Spec.specList.Count; i++)
            //{
            //    if (ResultDataGrid.Rows[i].Visible)
            //    {
            //        string newKey = ResultDataGrid.Rows[i].Cells[0].Value.ToString();

            //        if (old != newKey)
            //            bColorChange = !bColorChange;
            //        if (bColorChange) for (int k = 0; k < ResultDataGrid.ColumnCount; k++) ResultDataGrid[k, i].Style.BackColor = Color.Lavender;
            //        else for (int k = 0; k < ResultDataGrid.ColumnCount; k++) ResultDataGrid[k, i].Style.BackColor = Color.White;

            //        if (old == newKey)
            //            ResultDataGrid.Rows[i].Cells[0].Style.ForeColor = ResultDataGrid.Rows[i].Cells[0].Style.BackColor;
            //        old = newKey;
            //    }
            //}
        }
        public void InitResult(int ch, string Item)
        {
            if (ResultDataGrid.InvokeRequired)
            {
                ResultDataGrid.BeginInvoke((MethodInvoker)delegate
                {
                    for (int i = 0; i < Spec.specList.Count; i++)
                    {
                        if (Spec.specList[i].Category == Item)
                        {
                            if (PassFails[ch].FirstFailIndex == i + 1)
                            {
                                PassFails[ch].FirstFail = "";
                                PassFails[ch].FirstFailIndex = 0;
                            }
                            PassFails[ch].Results[i].Val = double.MaxValue;
                            PassFails[ch].Results[i].msg = ""; PassFails[ch].Results[i].bPass = true;

                            ResultDataGrid[ch + 4, i].Value = "";
                            ResultDataGrid[ch + 4, i].Style.BackColor = Color.White;
                            ResultDataGrid[ch, i].Style.BackColor = Color.White;

                        }

                    }

                });
            }
            else
            {

                for (int i = 0; i < Spec.specList.Count; i++)
                {
                    if (Spec.specList[i].Category == Item)
                    {
                        if (PassFails[ch].FirstFailIndex == i + 1)
                        {
                            PassFails[ch].FirstFail = "";
                            PassFails[ch].FirstFailIndex = 0;
                        }
                        PassFails[ch].Results[i].Val = double.MaxValue;
                        PassFails[ch].Results[i].msg = ""; PassFails[ch].Results[i].bPass = true;

                        ResultDataGrid[ch + 4, i].Value = "";
                        ResultDataGrid[ch + 4, i].Style.BackColor = Color.White;
                        ResultDataGrid[ch, i].Style.BackColor = Color.White;
                    }

                }
            }

            m_ChannelOn[ch] = true;
        }
        public void InitResult(int ch)
        {

            PassFails[ch].FirstFail = "";
            PassFails[ch].FirstFailIndex = 0;
            for (int i = 0; i < (int)SpecItem.Length; i++)
            {
                PassFails[ch].Results[i].Val = double.MaxValue;
                PassFails[ch].Results[i].msg = ""; PassFails[ch].Results[i].bPass = true;
            }
        }
        public void ShowDataResultsInit(int ch)
        {
            if (ResultDataGrid.InvokeRequired)
            {
                ResultDataGrid.BeginInvoke((MethodInvoker)delegate
                {
                    InitResult(ch);
                    for (int i = 0; i < Spec.specList.Count; i++)
                    {
                        ResultDataGrid[ch + 4, i].Value = "";
                        ResultDataGrid[ch + 4, i].Style.BackColor = Color.White;
                        ResultDataGrid[ch, i].Style.BackColor = Color.White;
                    }
                });
            }
            else
            {
                InitResult(ch);
                for (int i = 0; i < Spec.specList.Count; i++)
                {
                    ResultDataGrid[ch + 4, i].Value = "";
                    ResultDataGrid[ch + 4, i].Style.BackColor = Color.White;
                    ResultDataGrid[ch, i].Style.BackColor = Color.White;
                }
            }

            if (lblFailList.InvokeRequired)
            {
                lblFailList.BeginInvoke((MethodInvoker)delegate
                {
                    lblFailList.Text = "";
                });
            }
            else lblFailList.Text = "";
            STATIC.FailNumber = "Fail No. : ";
        }
        public void AddLog(int ch, string msg)
        {
            STATIC.SaveLogData += msg + "\r\n";
            if(ch < 1) ViewLog[ch].Log(msg);
           //  if (ViewLog[ch] != null ) ViewLog[ch].Log(msg);
        }
        public void ProcessInfor(string msg)
        {
            if (ProcessTitle.InvokeRequired)
            {
                ProcessTitle.BeginInvoke((MethodInvoker)delegate
                {
                    ProcessTitle.Text(msg, Color.White);
                });
            }
            else
                ProcessTitle.Text(msg, Color.White);
        }       
        public void ClearChart()
        {
            if (ChartTop[0].C.InvokeRequired)
            {
                ChartTop[0].C.BeginInvoke((MethodInvoker)delegate
                {
                    for (int i = 0; i < ChartTop[0].C.Series.Count; i++)
                    {
                        ChartTop[0].C.Series[i].Points.Clear();
                    }
                    ChartTop[0].C.Series[0].Points.AddXY(0, 0);
                });
            }
            else
            {
                for (int i = 0; i < ChartTop[0].C.Series.Count; i++)
                {
                    ChartTop[0].C.Series[i].Points.Clear();
                }
                ChartTop[0].C.Series[0].Points.AddXY(0, 0);
            }
            if (ChartBtm[0].C.InvokeRequired)
            {
                ChartBtm[0].C.BeginInvoke((MethodInvoker)delegate
                {
                    for (int i = 0; i < ChartBtm[0].C.Series.Count; i++)
                    {
                        ChartBtm[0].C.Series[i].Points.Clear();
                    }
                    ChartBtm[0].C.Series[0].Points.AddXY(0, 0);
                });
            }
            else
            {
                for (int i = 0; i < ChartBtm[0].C.Series.Count; i++)
                {
                    ChartBtm[0].C.Series[i].Points.Clear();
                }
                ChartBtm[0].C.Series[0].Points.AddXY(0, 0);
            }

            if (tiltChart[0].InvokeRequired)
            {
                tiltChart[0].BeginInvoke((MethodInvoker)delegate
                {
                    tiltChart[0].ClearPoint();
                });
            }
            else
            {
                tiltChart[0].ClearPoint();
            }
        }
        public void RunTest(int InspType) // 0:btn 1:switch 2:handler
        {
            I2CMonitorStartFlag = false;
            if (RepeatRun == 1 || InspType != 0)
            {
                CurrentRun = 1;
                if (Dln.IsRun) return;

                if (!Dln.IsRun)
                {
                    Dln.IsRun = true;
                    Task.Factory.StartNew(() => LoadTestUnload(0, InspType));
                }
            }
            else
            {
                CurrentRun = 1;
                if (Dln.IsRun) return;
                Dln.IsRun = true;
                while (true)
                {
                    //   ClearChart();
                    I2CMonitorStartFlag = false;
                    foreach (var l in ViewLog) l.Clear();

                    Task tasks = null;
                    tasks = Task.Factory.StartNew(() => LoadTestUnload(0, InspType));
                    Task.WaitAll(tasks);

                    if (CurrentRun >= RepeatRun || SuddenStop) break;
                    CurrentRun++;
                    Process.Wait(1500);
                }

            }

        }
        public void LoadSeq()
        {
            try
            {
                Stopwatch st = new Stopwatch();

                Dln.CoverUp();
                Thread.Sleep(700);
                Dln.LoadSocket();
                Dln.CoverDn();

                Thread.Sleep(500);

            }
            catch
            { }
        }
        public void UnloadSeq()
        {
            try
            {
                Stopwatch st = new Stopwatch();
                Dln.CoverUp();
                Thread.Sleep(700);
                Dln.UnloadSocket();
            }
            catch
            { }
        }
        public void LoadTestUnload(int port, int InspType) //inspType 0:btn 1:switch 2:handler
        {
            try
            {
                bool barcodeCheckedState = true;
                int ch = port * 2;

                LoadSeq();
                Process.Wait(100);

                RunStart?.Invoke(null, port);

                Process_Start(port, barcodeCheckedState);

                RunEnd?.Invoke(null, InspType);

                if (InspType != 2) UnloadSeq();
                Dln.IsRun = false;
                //StartI2CMonitor();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                Dln.IsRun = false;
            }
        }
        void SaveLogData()
        {
            string dateDir = STATIC.CreateDateDir();
            dateDir += "LogData\\";
            if (!Directory.Exists(dateDir))
                Directory.CreateDirectory(dateDir);
            for (int j = 0; j < ChannelCnt; j++)
            {

                string path = string.Format("{0}{1}_{2}.txt", dateDir, m_StrIndex[0], $"{STATIC.LogDate.Hour}h{STATIC.LogDate.Minute}m{STATIC.LogDate.Second}s");

                if (path != "")
                {
                    string FilePath = path;
                    //if (!File.Exists(FilePath)) return;
                    StreamWriter sw = new StreamWriter(FilePath);
                    sw.WriteLine(STATIC.SaveLogData);
                    sw.Close();
                }
            }
        }   
        public void Process_Start(int port,bool barcodeCheckState)
        {
            if (!IsVirtual)while(isI2cMonitoring) Thread.Sleep(10);

            STATIC.SaveLogData = string.Empty;

            int index = Rcp.RetryCnt.RetryOption.FindIndex(x => x.InspName == "All");
            int LoopCnt = 1 + Rcp.RetryCnt.RetryOption[index].Count;
            STATIC.Rcp.tt.CurrentST = 0;
            for (int Loop = 0; Loop < LoopCnt; Loop++)
            {
                try
                {
                    STATIC.I2CFailcnt = 0;
                    STATIC.LogDate = DateTime.Now;
                    STATIC.ActID = string.Empty;
                    ShowDataResultsInit(0);
                    ClearChart();
                    int ch = port * 2;

                    int count = Condition.ToDoList.Count;
                    if (count == 0)
                    {
                        for (int i = ch; i < ch + ChannelCnt; i++)
                            errMsg[i] = "Test Item is Empty";
                        return;
                    }
                    for (int k = ch; k < ch + ChannelCnt; k++)
                    {
                        m_ChannelOn[k] = true;
                        errMsg[k] = "";
                        PassFails[k].FirstFailIndex = 0;
                        if (!Dln.ReInitTarget(ch))
                        {
                            AddLog(ch, "Target Init Fail");
                            errMsg[k] = "I3C Fail";
                        }
                    }

                    if (errMsg[ch] != "" /*&& errMsg[ch + 1] != ""*/)
                    {
                        return;
                    }

                    Stopwatch sw = new Stopwatch();
                    sw.Start();

                    bool loopContinue = true;

                    int todoCnt = 0;
                    SuddenStop = false;

                    while (todoCnt < count)
                    {
                        string testItem = Condition.ToDoList[todoCnt];
                        Process_Function(port, testItem);

                        if (errMsg[ch] != "")
                        {
                            loopContinue = false;
                            AddLog(ch, errMsg[ch]);

                        }
                        if (SuddenStop)
                        {
                            loopContinue = false;
                            errMsg[ch] = "User Stop !";
                            AddLog(ch, errMsg[ch]);

                        }

                        if (!loopContinue) break;
                        else todoCnt++;
                        Process.Wait(100);
                    }

                    double ellipse = (double)sw.ElapsedMilliseconds / 1000;
                    sw.Stop();

                    yield.LastSampleNum++;

                    for (int k = ch; k < ch + ChannelCnt; k++)
                    {
                        AddLog(k, string.Format("Total Test Time\t{0:0.000} sec", ellipse));
                        PassFails[k].TotalTime = ellipse.ToString("F3");
                        STATIC.Rcp.tt.St += ellipse;
                        STATIC.Rcp.tt.CurrentST += ellipse;
                    }

                    if (!SuddenStop)
                    {
                        if (LoopCnt > 1)
                        {
                            if (errMsg[0] == "" && PassFails[0].FirstFailIndex == 0)
                            {

                                //if (Option.WriteResultToDriverIC)
                                //{
                                //    if (errMsg[0] == "" && PassFails[0].FirstFailIndex == 0)
                                //        WriteUserMem(ch, true);
                                //    else WriteUserMem(ch, false);
                                //}
                                //WriteResult(port);
                                SaveLogData();
                                SetFailList(ch);
                                SaveRetryLogData();
                            }
                            else
                            {
                                if (Loop == LoopCnt - 1)
                                {
                                    //if (Option.WriteResultToDriverIC)
                                    //{
                                    //    if (errMsg[0] == "" && PassFails[0].FirstFailIndex == 0)
                                    //        WriteUserMem(ch, true);
                                    //    else WriteUserMem(ch, false);
                                    //}
                                    WriteResult(port);
                                    SaveLogData();
                                    SetFailList(ch);
                                    SaveRetryLogData();
                                }
                                else
                                {
                                    AddLog(ch, $"Fail Retry =  {errMsg[0]}");
                                    yield.LastSampleNum--;
                                }
                            }
                        }
                        else
                        {

                            //if (Option.WriteResultToDriverIC)
                            //{
                            //    if (errMsg[0] == "" && PassFails[0].FirstFailIndex == 0)
                            //        WriteUserMem(ch, true);
                            //    else WriteUserMem(ch, false);
                            //}
                            WriteResult(port);
                            SaveLogData();
                            SetFailList(ch);
                            SaveRetryLogData();
                        }
                    }
                    m_strBarcoe[0] = string.Empty;
                }
                catch
                {
                    //Dln.PowerOnOff(port, false);
                }
                if (errMsg[0] == "" && PassFails[0].FirstFailIndex == 0) { STATIC.Rcp.tt.Count++; return; }
            }
            STATIC.Rcp.tt.Count++;
            return;

        }
        public void Process_Function(int port, string testItem)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            int index = 0;
            int RetryIndex = Rcp.RetryCnt.RetryOption.FindIndex(x => x.InspName == testItem);
            int RetryCnt = Rcp.RetryCnt.RetryOption[RetryIndex].Count + 1;

            for (int i = 0; i < ItemList.Count; i++)
            {
                if (testItem == ItemList[i].Name)
                {
                    index = i; break;
                }
            }

            int ch = port * 2;
            if (!m_ChannelOn[ch]) return;

            for (int k = ch; k < ch + ChannelCnt; k++)
            {
                if (m_ChannelOn[k])
                {
                    m_StrIndex[k] = (yield.LastSampleNum + k + 1).ToString();
                    AddLog(k, "\r\n");
                    AddLog(k, m_StrIndex[k] + ">> " + testItem + " Start");
                }
            }

            try
            {
                for (int i = 0; i < RetryCnt; i++)
                {
                    // 기존 PC 제어 방식 (DLN 장비이거나 펌웨어 미지원 항목)
                    Task Func1 = null, Func2 = null;
                    ProcessInfor(ItemList[index].Name);
                    if (!ItemList[index].IsMulti)
                    {
                        Func1 = new Task(() => ItemList[index].Func(port, testItem, i));
                        Func1.Start();
                        if (Func1 != null) Task.WaitAll(Func1);
                    }
                    else
                    {
                        if (m_ChannelOn[ch])
                        {
                            Func1 = new Task(() => ItemList[index].Func(ch, testItem, i));
                            Func1.Start();
                            AddLog(ch, testItem + " Start");
                        }
                        if (ChannelCnt > 1 && m_ChannelOn[ch + 1])
                        {
                            Func2 = new Task(() => ItemList[index].Func(ch + 1, testItem, i));
                            Func2.Start();
                            AddLog(ch + 1, testItem + " Start");
                        }

                        if (Func1 != null && Func2 != null) Task.WaitAll(Func1, Func2);
                        else
                        {
                            if (Func1 != null) Task.WaitAll(Func1);
                            if (Func2 != null) Task.WaitAll(Func2);
                        }
                    }
                    RetryDataInsert(testItem, i); 
                    if (i < RetryCnt - 1)
                    {
                        bool res = CheckFail(ch, testItem);
                        if (res) break;
                        else InitResult(ch, testItem);
                    }
                }
            }
            catch (Exception e)
            {
                for (int k = ch; k < ch + ChannelCnt; k++)
                {
                    AddLog(k, testItem + " Exception : " + e.ToString() + " ch : " + k.ToString());
                    errMsg[k] = testItem + " Error";
                    m_ChannelOn[k] = false;
                    PassFails[k].FirstFailIndex = -1;
                }
            }

            for (int k = ch; k < ch + ChannelCnt; k++)
            {
                double ellipse = (double)sw.ElapsedMilliseconds / 1000;
                AddLog(k, string.Format("{0}\t{1:0.000} sec", testItem, ellipse));
                ItemList[index].Time = ellipse.ToString("F3");
            }
            sw.Stop();
        }
        public void RetryDataInsert(string targetItem,int targetCount)
        {
            var item = ActionListReCount.FirstOrDefault(x => x.testName == targetItem);

            if (item != null)
            {
                item.Count = targetCount;
            }
            else
            {
                ActionListReCount.Add(new ReCount()
                {
                    testName = targetItem,
                    Count = targetCount
                });
            }
        }    
        //ESP 용 함수 ==========================================
        public List<byte> ScanSlaves(int ch)
        {
            List<byte> slaveList = new List<byte>();

            if (Dln is Esp32WifiDevice esp)
            {
                AddLog(ch, "Scanning I2C Slaves via ESP32...");

                //"SS"(Slave Scan) 명령어 전송
                byte[] res = esp.RunInternalSequence("SS", new byte[] { });

                if (res != null && res.Length > 0)
                {
                    int count = res[0];
                    if (res.Length >= count + 1)
                    {
                        for (int i = 0; i < count; i++)
                        {
                            byte addr = res[i + 1];
                            slaveList.Add(addr);
                            AddLog(ch, $"Found Slave Address: 0x{addr:X2}");
                        }
                    }
                }
                else
                {
                    AddLog(ch, "No Slaves found or Comm Error.");
                }
            }
            else
            {
                // 기존 DLN 유선 방식 스캔 로직 (필요시)
                for (byte addr = 1; addr < 127; addr++)
                {
                    byte[] buf = new byte[1];
                    if (Dln.ReadArray(ch, addr, 0x00, buf))
                    {
                        slaveList.Add(addr);
                        //AddLog(ch, $"[DLN] Found Slave: 0x{addr:X2}");
                    }
                }
            }

            return slaveList;
        }
        public void LEDs_All_On(int port, bool isOn, List<double> volt = null)
        {
            if (IsVirtual) return;
            int ch = port * 2;

            if (volt == null)
            {
                volt = new List<double>
                {
                    STATIC.Rcp.vsFile.LEDCurrentR,
                    STATIC.Rcp.vsFile.LEDCurrentL
                };
            }

            if (m_bAllLEDOn = isOn)
            {
                //  CSH035 적용 시 
                Dln.SetLEDpower(1, (int)(volt[0] * 500));
                Dln.SetLEDpower(2, (int)(volt[1] * 500));
            }
            else
                for (int k = ch; k < ch + ChannelCnt; k++)
                {
                    Dln.SetLEDpower(1, 0);
                    Dln.SetLEDpower(2, 0);
                }
        }
        public void AddHeadResult(string sFilePath)
        {
            StreamWriter writer;
            writer = File.AppendText(sFilePath);

            string sHeader;
            //"Time,Index,PlateBCode,LotID,ACTID,Channel,PM Index,PassFail,"
            sHeader = "Date,Time,Index,PlateBCode,LotID,ACTID,McNum,PortNum,PassFail,1st Fail Item,";

            string sParam = "";
            for (int i = 0; i < (int)SpecItem.Length; i++)
            {
                if (Spec.specList[i].InspectionType == InspType.MintoMax)
                {
                    sParam += string.Format("{0},", $"{Spec.specList[i].DisplayName} Min");
                    sParam += string.Format("{0},", $"{Spec.specList[i].DisplayName} Max");
                }
                else
                {
                    sParam += string.Format("{0},", Spec.specList[i].DisplayName);
                }


            }
            sHeader += sParam;


            //Time
            sParam = "";
            for (int i = 0; i < ItemList.Count; i++)
            {
                sParam += string.Format("{0} Time ,", ItemList[i].Name);
            }
            sParam += "Total Test Time";

            sHeader += sParam;

            writer.WriteLine(sHeader);

            //"Time,Index,PlateBCode,LotID,ACTID,Channel,PM Index,PassFail,1st Fail Item,";

            sHeader = "uint,,,,,,,,,,";

            sParam = "";
            for (int i = 0; i < (int)SpecItem.Length; i++)
            {
                if (Spec.specList[i].InspectionType == InspType.MintoMax)
                {
                    sParam += string.Format("({0}),", Spec.specList[i].Unit);
                    sParam += string.Format("({0}),", Spec.specList[i].Unit);
                }
                else
                {
                    sParam += string.Format("({0}),", Spec.specList[i].Unit);
                }



            }
            sHeader += sParam;

            writer.WriteLine(sHeader);

            sHeader = "Spec Min,,,,,,,,,,";
            sParam = "";
            for (int i = 0; i < (int)SpecItem.Length; i++)
            {
                if (Spec.specList[i].InspectionType == InspType.MintoMax)
                {
                    sParam += string.Format("{0},", Spec.specList[i].MinSpec);
                    sParam += string.Format("{0},", "");
                }
                else
                {
                    sParam += string.Format("{0},", Spec.specList[i].MinSpec);
                }

            }
            sHeader += sParam;

            writer.WriteLine(sHeader);

            sHeader = "Spec Max,,,,,,,,,,";
            sParam = "";
            for (int i = 0; i < (int)SpecItem.Length; i++)
            {
                if (Spec.specList[i].InspectionType == InspType.MintoMax)
                {
                    sParam += string.Format("{0},", "");
                    sParam += string.Format("{0},", Spec.specList[i].MaxSpec);
                }
                else
                {
                    sParam += string.Format("{0},", Spec.specList[i].MaxSpec);
                }

            }
            sHeader += sParam;

            writer.WriteLine(sHeader);

            writer.Close();
        }
        public void WriteResult(int port)
        {
            try
            {
                string dateDir = STATIC.CreateDateDir();
                if (!Directory.Exists(dateDir))
                    Directory.CreateDirectory(dateDir);

                string path = string.Format("{0}res_{1}.csv", dateDir, DateTime.Now.ToString("yyMMdd"));

                if (!File.Exists(path))
                {
                    AddHeadResult(path);
                }

                int ch = port * 2;

                StreamWriter sw = File.AppendText(path);

                for (int j = ch; j < ch + ChannelCnt; j++)
                {
                    string log = "";
                    if (errMsg[j] == "I3C Fail" || errMsg[j] == "Barcode Check" || errMsg[j] == "Socket Empty\r\nVision Check") { yield.TotlaTested--; continue; }

                    //if (PassFails[j].FirstFailIndex > 0)
                    //{
                    //    for (int k = 0; k < ItemList.Count; k++)
                    //    {
                    //        if (errMsg[j].Contains(ItemList[k].Name))
                    //        {
                    //            PassFails[j].FirstFailIndex = (-(k + 2));
                    //        }
                    //    }
                    //}

                    AddLog(j, string.Format("ch : {0}, msg : {1}, PassFail : {2}", j, errMsg[j], PassFails[j].FirstFailIndex));

                    //sHeader = "Date,Time,Index,PlateBCode,LotID,ACTID,Channel,PassFail,1st Fail Item,";
                    //"Time,Index,PlateBCode,LotID,ACTID,Channel,PM Index,PassFail,"
                    log += string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},",
                        STATIC.LogDate.ToString("yyyy-MM-dd"), $"{STATIC.LogDate.Hour}h{STATIC.LogDate.Minute}m{STATIC.LogDate.Second}s", m_StrIndex[j], m_strBarcoe[j], Model.LotID, STATIC.ActID, Model.MCNum, Model.TesterNo, PassFails[j].FirstFailIndex);

                    yield.TotlaTested++;
                    //1st Fail Item
                    if (PassFails[j].FirstFailIndex > 0)
                    {
                        errMsg[j] = PassFails[j].FirstFail;
                        yield.TotlaFailed++;
                        AddLog(j, "Fail : " + errMsg[j]);
                        log += errMsg[j] + ",";
                        var Item = STATIC.Rcp.YieldItem.FirstOrDefault(x => x.ItemName == errMsg[j]);
                        if (Item != null) Item.FailCnt++;
                        else STATIC.Rcp.YieldItem.Add(new NewYield { ItemName = errMsg[j], FailCnt = 1 });
                    }
                    else if (PassFails[j].FirstFailIndex < 0)
                    {
                        //errMsg[j] = PassFails[j].FirstFail;
                        yield.TotlaFailed++;
                        log += errMsg[j] + ",";
                        var Item = STATIC.Rcp.YieldItem.FirstOrDefault(x => x.ItemName == errMsg[j]);
                        if (Item != null) Item.FailCnt++;
                        else STATIC.Rcp.YieldItem.Add(new NewYield { ItemName = errMsg[j], FailCnt = 1 });
                    }
                    else
                    {
                        if (m_ChannelOn[j])
                        {
                            yield.TotlaPassed++;
                            log += "PASS" + ",";
                        }
                        else
                        {
                            log += "NONE" + ",";
                        }
                    }

                    //  X Results

                    for (int i = (int)SpecItem.FW_DOWNLOAD; i < (int)SpecItem.Length; i++)
                    {


                        switch (Rcp.Spec.specList[i].InspectionType)
                        {
                            case InspType.Normal:
                            case InspType.OnlyMax:
                            case InspType.OnlyMin:

                                if (PassFails[j].Results[i].Val == double.MaxValue) log += " ,";
                                else log += string.Format("{0:0.000},", PassFails[j].Results[i].Val);


                                break;
                            case InspType.OKNG:
                                if (PassFails[j].Results[i].Val == 0) log += string.Format("OK") + ",";
                                else if (PassFails[j].Results[i].Val == double.MaxValue) log += string.Format(" ") + ",";
                                else log += string.Format("NG") + ",";
                                break;
                            case InspType.MintoMax:
                                break;
                        }




                    }

                    //Time
                    for (int i = 0; i < ItemList.Count; i++)
                    {

                        log += string.Format("{0:0.000},", ItemList[i].Time);
                    }

                    log += string.Format("{0:0.000},", PassFails[ch].TotalTime);

                    sw.WriteLine(log);
                }
                sw.Close();

            }
            catch (Exception ex)
            {
                Form f = Application.OpenForms["F_Main"];

                if (f != null)
                {
                    if (f.InvokeRequired)
                    {
                        f.BeginInvoke(new Action(() =>
                            MessageBox.Show(f, ex.ToString(), "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error)));
                    }
                    else
                    {
                        MessageBox.Show(f, ex.ToString(), "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    // 메인폼을 못 찾았을 때 (owner 없이 표시)
                    MessageBox.Show(ex.ToString(), "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                m_ChannelOn[0] = false;
                errMsg[0] = "Check the Result File Open!!!";
            }
        }
        public void AddRetryCountHead(string sFilePath)
        {
            StreamWriter writer;
            writer = File.AppendText(sFilePath);

            string sHeader = string.Empty;
            sHeader += "Index,";

            ActionListReCount.ForEach(retryElement => 
            {
                sHeader += retryElement.testName;
                sHeader += ",";
            });

            writer.WriteLine(sHeader);
            writer.Close();
        }
        private void SaveRetryLogData()
        {
            string dateDir = STATIC.CreateDateDir();
            dateDir += "RetryData\\";
            if (!Directory.Exists(dateDir))
                Directory.CreateDirectory(dateDir);

            string path = string.Format("{0}ReCount_{1}.csv", dateDir, DateTime.Now.ToString("yyMMdd"));

            if (!File.Exists(path))
            {
                AddRetryCountHead(path);
            }

            for (int j = 0; j < ChannelCnt; j++)
            {
                //ActionListReCount
                string retryCountLog = string.Empty;
                if (path != "")
                {
                    string FilePath = path;
                    StreamWriter sw = new StreamWriter(FilePath, true);

                    retryCountLog += m_StrIndex[j];
                    retryCountLog += ",";

                    ActionListReCount.ForEach(retryElement =>
                    {
                        retryCountLog += retryElement.Count.ToString();
                        retryCountLog += ",";
                    });

                    sw.WriteLine(retryCountLog);
                    sw.Close();
                }
            }
        }

        #endregion
    }
}
