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
        public bool IsVirtual = true;
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
        public void StartI2CMonitor()
        {
            if (I2CMonitorStartFlag) return;
            I2CMonitorStartFlag = true;
            isI2cMonitoring = true;
            Task monitorI2C = new Task(() => MonitorI2C());
            monitorI2C.Start();
        }


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
                            //if(PassFails[ch].Results[i].Nam)
                            if (i == (int)SpecItem.AF_Sensitivity || i == (int)SpecItem.OISX_Sensitivity || i == (int)SpecItem.OISY_Sensitivity)
                            {
                                ResultDataGrid[ch + 4, i].Value = PassFails[ch].Results[i].Val.ToString("F4");
                            }
                            else
                            {
                                ResultDataGrid[ch + 4, i].Value = PassFails[ch].Results[i].Val.ToString("F3");
                            }

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
                        if (i == (int)SpecItem.AF_Sensitivity || i == (int)SpecItem.OISX_Sensitivity || i == (int)SpecItem.OISY_Sensitivity)
                        {
                            ResultDataGrid[ch + 4, i].Value = PassFails[ch].Results[i].Val.ToString("F4");
                        }
                        else
                        {
                            ResultDataGrid[ch + 4, i].Value = PassFails[ch].Results[i].Val.ToString("F3");
                        }

                    }
                    if (PassFails[ch].Results[i].bPass) { ResultDataGrid[ch + 4, i].Style.BackColor = Color.White; ResultDataGrid[ch, i].Style.BackColor = Color.White; }
                    else { ResultDataGrid[ch + 4, i].Style.BackColor = Color.Orange; ResultDataGrid[ch, i].Style.BackColor = Color.Orange; }

                }
            }


            for (int i = start; i <= end; i++)
            {
                if (!PassFails[ch].Results[i].bPass)
                {
                    if (!Option.ContinueTestingOnFail) m_ChannelOn[ch] = false;
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
        
        public void AddChart(int ch, string name, List<double> time = null, List<double> Stroke = null, double MaxtiltX = 0, double MaxtiltY = 0, double[] refArr = null)
        {
            while (ChartTop[ch].IsFalg)
                Process.Wait(10);

            int CodeRange = 0;

            foreach (var Cal in CalList[ch])
            {
                if (Cal.Name == name)
                {
                    switch (name)
                    {
                        case "OIS X Scan":

                            CodeRange = Condition.iXPlotRange;
                            //Stroke
                            if (ChartTop[ch].C.InvokeRequired)
                            {
                                ChartTop[ch].C.BeginInvoke((MethodInvoker)delegate
                                {
                                    ChartTop[ch].C.Series[0].Points.Clear();

                                    for (int i = 0; i < Cal.CodeX.Count; i++)
                                    {
                                        var ChartCode = (Cal.CodeX[i] / 2);
                                        var ChartRangeMin = OISCenter - CodeRange;
                                        var ChartRangeMax = OISCenter + CodeRange;

                                        if (ChartCode >= ChartRangeMin && ChartCode <= ChartRangeMax)
                                        {
                                            ChartTop[ch].C.Series[0].Points.AddXY(ChartCode, Cal.StrokeX[i]); //  stroke
                                            ChartTop[ch].C.Series[3].Points.AddXY(ChartCode, Cal.Current[i]); //  current
                                            //ChartTop[ch].C.Series[6].Points.AddXY(ChartCode, Cal.HallX[i] / 20); //  hall
                                        }
                                        else
                                        {
                                            int iii = 0;
                                        }
                                    }
                                });
                            }

                            //CodeRange = Condition.iXPlotRange;
                            ////Stroke
                            //if (ChartTop[ch].C.InvokeRequired)
                            //{
                            //    ChartTop[ch].C.BeginInvoke((MethodInvoker)delegate
                            //    {
                            //        ChartTop[ch].C.Series[0].Points.Clear();

                            //        for (int i = 0; i < Cal.CodeX.Count; i++)
                            //        {
                            //            if (Cal.CodeX[i] >= OISCenter - CodeRange && Cal.CodeX[i] <= OISCenter + CodeRange)
                            //            {
                            //                ChartTop[ch].C.Series[0].Points.AddXY(Cal.CodeX[i], Cal.StrokeX[i]); //  stroke
                            //                ChartTop[ch].C.Series[3].Points.AddXY(Cal.CodeX[i], Cal.Current[i]); //  current
                            //                ChartTop[ch].C.Series[6].Points.AddXY(Cal.CodeX[i], Cal.HallX[i] / 10); //  hall
                            //            }
                            //        }
                            //    });
                            //}
                            //Tilt
                            //if (ChartBtm[ch].C.InvokeRequired)
                            //{
                            //    ChartBtm[ch].C.BeginInvoke((MethodInvoker)delegate
                            //    {
                            //        for (int i = 2; i < Cal.CodeX.Count; i++)
                            //        {
                            //            if (Cal.CodeX[i] >= OISCenter - CodeRange && Cal.CodeX[i] <= OISCenter + CodeRange)
                            //            {
                            //                //ChartBtm[ch].C.Series[0].Points.AddXY(Cal.CodeX[i], Cal.TiltX[i]); //  Tilt 
                            //                //ChartBtm[ch].C.Series[1].Points.AddXY(Cal.CodeX[i], Cal.TiltY[i]); //  Tilt 
                            //                //ChartBtm[ch].C.Series[2].Points.AddXY(Cal.CodeX[i], Cal.TiltZ[i]); //  Tilt 
                            //            }
                            //        }
                            //    });
                            //}
                            break;
                        case "OIS Y Scan":

                            CodeRange = Condition.iYPlotRange;
                            //Stroke
                            if (ChartTop[ch].C.InvokeRequired)
                            {
                                ChartTop[ch].C.BeginInvoke((MethodInvoker)delegate
                                {
                                    for (int i = 0; i < Cal.CodeY.Count; i++)
                                    {
                                        var ChartCode = (Cal.CodeY[i] / 2);
                                        var ChartRangeMin = OISCenter - CodeRange;
                                        var ChartRangeMax = OISCenter + CodeRange;

                                        if (ChartCode >= ChartRangeMin && ChartCode <= ChartRangeMax)
                                        {
                                            ChartTop[ch].C.Series[1].Points.AddXY(Cal.CodeY[i] / 2, Cal.StrokeY[i]); //  stroke
                                                                                                                     //   ChartTop[ch].C.Series[9].Points.AddXY(Cal.CodeY1[i], Cal.StrokeY1[i]); //  stroke 1
                                                                                                                     // ChartTop[ch].C.Series[10].Points.AddXY(Cal.CodeY2[i], Cal.StrokeY2[i]); //  stroke 2
                                            ChartTop[ch].C.Series[4].Points.AddXY(Cal.CodeY[i] / 2, Cal.Current[i]); //  current
                                            //ChartTop[ch].C.Series[7].Points.AddXY(Cal.CodeY[i] / 2, Cal.HallY1[i] / 20); //  hall
                                        }
                                    }
                                });
                            }
                            //Tilt
                            //if (ChartBtm[ch].C.InvokeRequired)
                            //{
                            //    ChartBtm[ch].C.BeginInvoke((MethodInvoker)delegate
                            //    {
                            //        for (int i = 2; i < Cal.CodeY.Count; i++)
                            //        {
                            //            if (Cal.CodeY[i] >= OISCenter - CodeRange && Cal.CodeY[i] <= OISCenter + CodeRange)
                            //            {
                            //                //ChartBtm[ch].C.Series[3].Points.AddXY(Cal.CodeY1[i], Cal.TiltX[i]); //  Tilt 
                            //                //ChartBtm[ch].C.Series[4].Points.AddXY(Cal.CodeY1[i], Cal.TiltY[i]); //  Tilt 
                            //                //ChartBtm[ch].C.Series[5].Points.AddXY(Cal.CodeY1[i], Cal.TiltZ[i]); //  Tilt 
                            //            }
                            //        }
                            //    });
                            //}
                            break;
                        case "AF Scan":

                            CodeRange = Condition.iAFPlotRange;
                            //Stroke
                            if (ChartTop[ch].C.InvokeRequired)
                            {
                                ChartTop[ch].C.BeginInvoke((MethodInvoker)delegate
                                {
                                    for (int i = 0; i < Cal.CodeZ.Count; i++)
                                    {
                                        if (Cal.CodeZ[i] >= AFCenter - CodeRange && Cal.CodeZ[i] <= AFCenter + CodeRange)
                                        {
                                            ChartTop[ch].C.Series[2].Points.AddXY(Cal.CodeZ[i], Cal.StrokeZ[i]); //  stroke
                                            ChartTop[ch].C.Series[5].Points.AddXY(Cal.CodeZ[i], Cal.Current[i]); //  current
                                            //ChartTop[ch].C.Series[8].Points.AddXY(Cal.CodeZ[i], Cal.HallZ[i] / 10); //  hall
                                        }
                                    }
                                });
                            }
                            //Tilt
                            if (tiltChart[ch].InvokeRequired)
                            {
                                tiltChart[ch].BeginInvoke((MethodInvoker)delegate
                                {
                                    List<double> xs_List = new List<double>();
                                    List<double> ys_List = new List<double>();



                                    for (int i = 2; i < Cal.CodeZ.Count; i++)
                                    {
                                        if (Cal.CodeZ[i] >= Condition.TiltMinCode && Cal.CodeZ[i] <= Condition.TiltMaxCode)
                                        {
                                            xs_List.Add(Cal.TiltX[i]);
                                            ys_List.Add(Cal.TiltY[i]);
                                            //xs[i] = Cal.TiltX[i];
                                            //ys[i] = Cal.TiltY[i];

                                        }
                                    }
                                    double[] xs = xs_List.ToArray();
                                    double[] ys = ys_List.ToArray();
                                    tiltChart[ch].SetPoints(xs, ys, Color.Lime);
                                    tiltChart[ch].SetPoint(xs[0], ys[0], Color.LightGray);
                                    tiltChart[ch].SetPoint(xs[xs.Length - 1], ys[ys.Length - 1], Color.LightGray);
                                    tiltChart[ch].SetPoint(MaxtiltX, MaxtiltY, Color.Red);
                                    tiltChart[ch].SetPoint(refArr[0], refArr[1], Color.Orange);

                                });
                            }
                            break;
                        case "AF Settling":
                            if (ChartBtm[ch].C.InvokeRequired)
                            {
                                ChartBtm[ch].C.BeginInvoke((MethodInvoker)delegate
                                {
                                    for (int i = 0; i < time.Count - 1; i++)
                                    {
                                        ChartBtm[ch].C.Series[0].Points.AddXY(time[i], Stroke[i]); //  stroke
                                    }
                                });
                            }
                            break;
                        case "AF Settling2":
                            //Stroke
                            if (ChartBtm[ch].C.InvokeRequired)
                            {
                                ChartBtm[ch].C.BeginInvoke((MethodInvoker)delegate
                                {
                                    for (int i = 0; i < time.Count - 1; i++)
                                    {
                                        ChartBtm[ch].C.Series[1].Points.AddXY(time[i], Stroke[i]); //  stroke
                                    }
                                });
                            }
                            //Tilt
                            //if (ChartBtm[ch].C.InvokeRequired)
                            //{
                            //    ChartBtm[ch].C.BeginInvoke((MethodInvoker)delegate
                            //    {
                            //        for (int i = 2; i < Cal.Time.Count; i++)
                            //        {
                            //            ChartBtm[ch].C.Series[6].Points.AddXY(Cal.Time[i] * 1000, Cal.TiltX[i]); //  Tilt 
                            //            ChartBtm[ch].C.Series[7].Points.AddXY(Cal.Time[i] * 1000, Cal.TiltY[i]); //  Tilt 
                            //            ChartBtm[ch].C.Series[8].Points.AddXY(Cal.Time[i] * 1000, Cal.TiltZ[i]); //  Tilt 
                            //        }
                            //    });
                            //}
                            break;

                    }
                    ChartSet(ch, name);
                }
            }
        }
        private void ChartSet(int ch, string name)
        {
            //StrokeChart
            if (ChartTop[ch].C.InvokeRequired)
            {
                ChartTop[ch].C.BeginInvoke((MethodInvoker)delegate
                {
                    ChartTop[ch].C.ChartAreas[0].AxisX.MinorGrid.Enabled = false;
                    ChartTop[ch].C.ChartAreas[0].AxisY.MinorGrid.Enabled = false;
                    ChartTop[ch].C.ChartAreas[0].AxisY2.MajorGrid.Enabled = false;
                    ChartTop[ch].C.ChartAreas[0].AxisY2.MinorGrid.Enabled = false;

                    if (name.Contains("Settling"))
                    {
                        //ChartTop[ch].C.Titles[0].Text = "Stroke vs Time";
                        //ChartTop[ch].C.ChartAreas[0].AxisX.Minimum = 0;
                        //ChartTop[ch].C.ChartAreas[0].AxisX.Maximum = 600;
                        //ChartTop[ch].C.ChartAreas[0].AxisX.Interval = 100;
                        //ChartTop[ch].C.ChartAreas[0].AxisX.MajorGrid.Interval = 100;
                    }
                    else
                    {
                        ChartTop[ch].C.Titles[0].Text = "Stroke vs Code";
                        ChartTop[ch].C.ChartAreas[0].AxisX.Minimum = 0;
                        ChartTop[ch].C.ChartAreas[0].AxisX.Maximum = 4100;
                        ChartTop[ch].C.ChartAreas[0].AxisX.Interval = 512;
                        ChartTop[ch].C.ChartAreas[0].AxisX.MajorGrid.Interval = 512;
                    }


                    ChartTop[ch].C.ChartAreas[0].AxisY.Minimum = -600;
                    ChartTop[ch].C.ChartAreas[0].AxisY.Maximum = 600;
                    ChartTop[ch].C.ChartAreas[0].AxisY.Interval = 100;
                    ChartTop[ch].C.ChartAreas[0].AxisY.MajorGrid.Interval = 100;

                    ChartTop[ch].C.ChartAreas[0].AxisY2.Minimum = -50;
                    ChartTop[ch].C.ChartAreas[0].AxisY2.Maximum = 410;
                    ChartTop[ch].C.ChartAreas[0].AxisY2.Interval = 45;

                    ChartTop[ch].C.ChartAreas[0].AxisY2.Enabled = AxisEnabled.True;
                    ChartTop[ch].C.ChartAreas[0].AxisY2.LabelStyle.ForeColor = Color.DarkGreen;
                    ChartTop[ch].C.ChartAreas[0].AxisY2.LabelStyle.Font = new Font("Calibri", 9, FontStyle.Bold);

                    ChartTop[ch].IsFalg = false;
                });
            }
            //settle Chart
            if (ChartBtm[ch].C.InvokeRequired)
            {
                ChartBtm[ch].C.BeginInvoke((MethodInvoker)delegate
                {
                    ChartBtm[ch].C.ChartAreas[0].AxisX.MinorGrid.Enabled = false;
                    ChartBtm[ch].C.ChartAreas[0].AxisY.MinorGrid.Enabled = false;
                    //ChartBtm[ch].C.ChartAreas[0].AxisY2.MajorGrid.Enabled = false;
                    //ChartBtm[ch].C.ChartAreas[0].AxisY2.MinorGrid.Enabled = false;


                    if (name.Contains("Settling"))
                    {
                        ChartBtm[ch].C.Titles[0].Text = "Time to Settle";
                        ChartBtm[ch].C.ChartAreas[0].AxisX.Minimum = 0;
                        ChartBtm[ch].C.ChartAreas[0].AxisX.Maximum = 100;
                        ChartBtm[ch].C.ChartAreas[0].AxisX.Interval = 10;
                        ChartBtm[ch].C.ChartAreas[0].AxisX.MajorGrid.Interval = 10;
                    }
                    else
                    {
                        //ChartBtm[ch].C.Titles[0].Text = "Tilt vs Code";
                        //ChartBtm[ch].C.ChartAreas[0].AxisX.Minimum = 0;
                        //ChartBtm[ch].C.ChartAreas[0].AxisX.Maximum = 4100;
                        //ChartBtm[ch].C.ChartAreas[0].AxisX.Interval = 512;
                        //ChartBtm[ch].C.ChartAreas[0].AxisX.MajorGrid.Interval = 512;
                    }

                    //ChartBtm[ch].C.ChartAreas[0].AxisY.Minimum = -20;
                    //ChartBtm[ch].C.ChartAreas[0].AxisY.Maximum = 200;
                    //ChartBtm[ch].C.ChartAreas[0].AxisY.Interval = 20;
                    //ChartBtm[ch].C.ChartAreas[0].AxisY.MajorGrid.Interval = 20;

                    //ChartBtm[ch].C.ChartAreas[0].AxisY2.Minimum = -200;
                    //ChartBtm[ch].C.ChartAreas[0].AxisY2.Maximum = 200;
                    //ChartBtm[ch].C.ChartAreas[0].AxisY2.Interval = 40;
                    //ChartBtm[ch].C.ChartAreas[0].AxisY2.MajorGrid.Interval = 40;

                    //ChartBtm[ch].C.ChartAreas[0].AxisY2.Enabled = AxisEnabled.True;
                    //ChartBtm[ch].C.ChartAreas[0].AxisY2.LabelStyle.ForeColor = Color.DarkGreen;
                    //ChartBtm[ch].C.ChartAreas[0].AxisY2.LabelStyle.Font = new Font("Calibri", 9, FontStyle.Bold);

                    ChartBtm[ch].IsFalg = false;
                });
            }
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
                if (Option.SocketSensorUse)
                {
                    st.Start();
                    while (!Dln.GetGpioStatus(12) || Dln.GetGpioStatus(13))
                    {
                        if (st.ElapsedMilliseconds > 3000)
                        {
                            if (Dln.IsSafeOn)
                                MessageBox.Show("User Stop");
                            else
                                MessageBox.Show("Check Socket Sensor Status");
                            return;
                        }
                        Thread.Sleep(10);
                    }
                    st.Stop();
                    Thread.Sleep(300);
                }
                else Thread.Sleep(2000);
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
                if (Option.SocketSensorUse)
                {
                    st.Start();
                    while (Dln.GetGpioStatus(12) || !Dln.GetGpioStatus(13))
                    {
                        if (st.ElapsedMilliseconds > 3000)
                        {
                            if (Dln.IsSafeOn)
                                MessageBox.Show("User Stop");
                            else
                                MessageBox.Show("Check Socket Sensor Status");
                            return;
                        }
                        Thread.Sleep(10);
                    }
                    st.Stop();
                }
                else Thread.Sleep(500);
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

                if (STATIC.Rcp.Option.ScannerUse)
                {
                    barcodeCheckedState = STATIC.fManage.BarcodeScanDialogSync();
                    if (barcodeCheckedState)
                        LoadSeq();
                }
                else
                    LoadSeq();
                Process.Wait(100);

                RunStart?.Invoke(null, port);

                Process_Start(port, barcodeCheckedState);

                RunEnd?.Invoke(null, InspType);

                if (InspType != 2) UnloadSeq();
                Dln.IsRun = false;
                StartI2CMonitor();
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
            if(!IsVirtual)while(isI2cMonitoring) Thread.Sleep(10);
            bool isI2cFail = false;
            STATIC.SaveLogData = string.Empty;
            if (Option.DryRunMode) { Thread.Sleep(40000); return; }
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

                    //Barcode Check  =================================================
                    //if (Option.ScannerUse)
                    //{
                    //    if (!barcodeCheckState)
                    //    {
                    //        if (m_strBarcoe[0] == "")
                    //            m_ChannelOn[0] = false;
                    //        if (!m_ChannelOn[0])
                    //        {
                    //            errMsg[0] = "Barcode Check";
                    //            AddLog(0, "Barcode Check");
                    //            return;
                    //        }
                    //    }
                    //}

                    //Dln.PowerOnOff(port, true);
                    //if (!IsVirtual) PowerSequence(port);
                    //Thread.Sleep(200);
                    //DrvIC.ICReset(0);
                    //if(!IsVirtual) m__G.oCam[port].ResetmCpXY();
                    int ch = port * 2;
                    //DrvIC.FRAModeDisable(ch);
                    SinewaveXMaxDiff = 0;
                    SinewaveYMaxDiff = 0;
                    RingingXStabilizer = 0;
                    RingingYStabilizer = 0;

                    AFCurrentMinMax = new double[2];
                    OISXCurrentMinMax = new double[2];
                    OISYCurrentMinMax = new double[2];
                    g_IME = new int[2];


                    //      Dln.ReadArray(0, DrvIC.XSlaveAddr, 0xE5, b);
                    //AddLog(ch, $"AF Best Pos = {BestAFPos}");
                    //  BestAFPos = b[0] << 4;
                    //  if (BestAFPos == 0) BestAFPos = 2048;
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
                    }


                    //if (!Dln.WriteArray(ch, DrvIC.AF_Addr, 0x02, new byte[] { 0x40 }) && !Dln.WriteArray(ch, DrvIC.AF_Addr, 0x02, new byte[] { 0x40 })) m_ChannelOn[ch] = false;
                    //if (!Dln.WriteArray(ch, DrvIC.XOriginAddr, 0x02, new byte[] { 0x40 }) && !Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x02, new byte[] { 0x40 })) m_ChannelOn[ch] = false;
                    //if (!Dln.WriteArray(ch, DrvIC.Y1OriginAddr, 0x02, new byte[] { 0x40 }) && !Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x02, new byte[] { 0x40 })) m_ChannelOn[ch] = false;
                    //if (DrvIC.Y2SlaveAddr != 0x00)
                    //{
                    //    if (!Dln.WriteArray(ch, DrvIC.Y2OriginAddr, 0x02, new byte[] { 0x40 })
                    //        && !Dln.WriteArray(ch, DrvIC.Y2SlaveAddr, 0x02, new byte[] { 0x40 })) m_ChannelOn[ch] = false;
                    //}

                    for (int k = ch; k < ch + ChannelCnt; k++)
                    {
                        if (!m_ChannelOn[k])
                        {
                            errMsg[k] = "I2C Fail";
                            AddLog(k, "I2C Fail");
                            isI2cFail = true;
                        }
                    }

                    //if (!isI2cFail)
                    //{
                    //    if (!IsVirtual)
                    //    {
                    //        LEDs_All_On(0, true);
                    //        Thread.Sleep(50);
                    //        FindResult res = Measure();
                    //        LEDs_All_On(0, false);


                    //        if (res.cx[0] == 0 && res.cy[0] == 0)
                    //            m_ChannelOn[ch] = false;
                    //    }
                    //    for (int k = ch; k < ch + ChannelCnt; k++)
                    //    {
                    //        if (!m_ChannelOn[k])
                    //        {
                    //            errMsg[k] = "Socket Empty\r\nVision Check";
                    //            AddLog(k, "Socket Empty\r\nVision Check");
                    //        }
                    //    }
                    //}

                    if (errMsg[ch] != "" /*&& errMsg[ch + 1] != ""*/)
                    {
                        return;
                    }

                    //Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0xAE, new byte[] { 0x3B });
                    //Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0xA6, new byte[] { 0x7B });
                    //Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x02, new byte[] { 0x00 });

                    //Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0xAE, new byte[] { 0x3B });
                    //Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0xA6, new byte[] { 0x7B });
                    //Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x02, new byte[] { 0x00 });
                    //for (int i = 0; i < 15; i++)
                    //{
                    //    DrvIC.Move(ch, "X", 0);
                    //    Wait(50);
                    //    DrvIC.Move(ch, "X", 4095);
                    //    Wait(50);
                    //}
                    //for (int i = 0; i < 15; i++)
                    //{
                    //    DrvIC.Move(ch, "Y", 0);
                    //    Wait(50);
                    //    DrvIC.Move(ch, "Y", 4095);
                    //    Wait(50);
                    //}
                    //Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x02, new byte[] { 0x40 });
                    //Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0xA6, new byte[] { 0x00 });
                    //Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x02, new byte[] { 0x40 });
                    //Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0xA6, new byte[] { 0x00 });
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
                    //LEDs_All_On(port, false);

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
                    //else { Dln.PowerOnOff(port, false); STATIC.Rcp.tt.Count++; return; }
                    //Dln.PowerOnOff(port, false);


                    //임시 추가 바코드 정보 삭제
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
        public int StroketoCode(double stroke, double senstivity = 0.1148) // Defualt Value
        {
            if (senstivity >= 0.2) senstivity = 0.1148;
            return (int)(stroke / senstivity);
        }

        public void MakeWaveform(string name)
        {
            for (int k = 0; k < ChannelCnt; k++)
            {
                foreach (var Cal in CalList[k])
                {
                    if (Cal.Name == name)
                    {
                        Cal.Clear();

                        int min = 0;
                        int max = 0;
                        int step = 0;
                        int curPos = 0;

                        switch (name)
                        {
                            case "AF Scan":
                                //AF ========
                                MakeWaveformCode(ref Cal.CodeZ, Condition.iAFDrvCodeMin, Condition.iAFDrvCodeMax, AFCenter, Condition.iDrvAFStep);
                                break;
                            case "OIS X Scan":
                            case "OIS X Scan Mac":
                                //X =========
                                MakeWaveformCode(ref Cal.CodeX, Condition.iXDrvCodeMin, Condition.iXDrvCodeMax, OISCenter, Condition.iDrvXStep);
                                break;
                            case "OIS Y Scan":
                            case "OIS Y Scan Mac":
                                //Y1 ===========================
                                MakeWaveformCode(ref Cal.CodeY, Condition.iYDrvCodeMin, Condition.iYDrvCodeMax, OISCenter, Condition.iDrvYStep);
                                break;

                                //case "AF Settling":
                                ////case "AF Settling2":
                                //    min = Condition.iAFStandbyCode;
                                //    max = Condition.iAFStandbyCode + StroketoCode(Condition.IAFTarget);
                                //    Cal.CodeZ.Add(min);
                                //    Cal.CodeZ.Add(min);
                                //    Cal.CodeZ.Add(min);
                                //    Cal.CodeZ.Add(max);
                                //    break;
                        }
                    }
                }

            }
        }
        private void MakeWaveformCode(ref List<int> code, int min, int max, int mid, int step)
        {
            int curPos = 0;
            curPos = mid;
            do
            {
                code.Add(curPos);
                curPos += step;
            } while (curPos < max);
            //  if (max >= 4095) max = 4095;
            code.Add(max);
            curPos -= step;
            do
            {
                code.Add(curPos);
                curPos -= step;
            } while (curPos > min);
            code.Add(min);
            curPos += step;

            do
            {
                code.Add(curPos);
                curPos += step;
            } while (curPos < mid);
            code.Add(mid);

        }

        private void CrossOffsetMove(int port, string name)
        {
            int ch = port * 2;
            //Cross Offset Pos Move 
            for (int j = ch; j < ch + ChannelCnt; j++)
            {
                if (!m_ChannelOn[j]) continue;
                if (name.Contains("AF")) DrvIC.AFOnOff(j, true);
                else DrvIC.OISOn(j, name, true);


                switch (name)
                {
                    case "AF Scan":
                        Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x02, new byte[] { 0x40 });
                        Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x02, new byte[] { 0x40 });
                        if (DrvIC.Y2SlaveAddr != 0x00)
                            Dln.WriteArray(ch, DrvIC.Y2SlaveAddr, 0x02, new byte[] { 0x40 });
                        break;
                    case "OIS X Scan":
                        DrvIC.Move(j, "X", OISCenter);
                        DrvIC.OISOn(j, "Y", true);
                        DrvIC.Move(j, "Y", Condition.iXCrossOffset);
                        DrvIC.AFOnOff(j, true);
                        DrvIC.AFMove(j, BestAFPos);
                        AddLog(ch, $"Move AF Best Position : {BestAFPos}");
                        break;
                    case "OIS Y Scan":
                        DrvIC.Move(j, "Y", OISCenter);
                        DrvIC.OISOn(j, "X", true);
                        DrvIC.Move(j, "X", Condition.iYCrossOffset);
                        DrvIC.AFOnOff(j, true);
                        DrvIC.AFMove(j, BestAFPos);
                        AddLog(ch, $"Move AF Best Position : {BestAFPos}");
                        break;
                    case "OIS X Scan Mac":
                        DrvIC.Move(j, "X", OISCenter);
                        DrvIC.OISOn(j, "Y", true);
                        DrvIC.Move(j, "Y", Condition.iXCrossOffset);
                        DrvIC.AFOnOff(j, true);
                        DrvIC.AFMove(j, 3465);
                        AddLog(ch, $"Move AF Best Position : {3465}");
                        break;
                    case "OIS Y Scan Mac":
                        DrvIC.Move(j, "Y", OISCenter);
                        DrvIC.OISOn(j, "X", true);
                        DrvIC.Move(j, "X", Condition.iYCrossOffset);
                        DrvIC.AFOnOff(j, true);
                        DrvIC.AFMove(j, 3465);
                        AddLog(ch, $"Move AF Best Position : {3465}");
                        break;

                }
            }
            Process.Wait(100);
            //Initial Pos Move 

            for (int k = 0; k < 2; k++)
            {
                switch (name)
                {
                    case "AF Scan":
                        for (int j = ch; j < ch + ChannelCnt; j++)
                        {
                            if (!m_ChannelOn[j]) continue;
                            foreach (var Cal in CalList[j])
                            {
                                if (Cal.Name == name) DrvIC.AFMove(j, Cal.CodeZ[0]);
                            }
                        }
                        Process.Wait(Condition.iDrvStepIntervalZ);
                        break;
                    case "OIS X Scan":
                    case "OIS X Scan Mac":
                        for (int j = ch; j < ch + ChannelCnt; j++)
                        {
                            if (!m_ChannelOn[j]) continue;
                            foreach (var Cal in CalList[j])
                            {
                                if (Cal.Name == name) DrvIC.Move(j, name, Cal.CodeX[0]);
                            }
                        }
                        Process.Wait(Condition.iDrvStepIntervalX);
                        break;
                    case "OIS Y Scan":
                    case "OIS Y Scan Mac":
                        for (int j = ch; j < ch + ChannelCnt; j++)
                        {
                            if (!m_ChannelOn[j]) continue;
                            foreach (var Cal in CalList[j])
                            {
                                if (Cal.Name == name) DrvIC.Move(j, name, Cal.CodeY[0]);
                            }
                        }
                        Process.Wait(Condition.iDrvStepIntervalY);
                        break;
                }
            }
        }
        private void Process_ScanCodeTest(int port, string name, int InspCnt)
        {
            int ch = port * 2;

            Wait(100);

            CrossOffsetMove(port, name);


            IsScan[port] = true;
            framCnt[port] = 0;
            int curPos = 0;

            Stopwatch sw = new Stopwatch();
            sw.Reset(); sw.Start();
            long totalSleepTimeZ = 0;
            int currentAfCode = 0;
            while (IsScan[port])
            {
                for (int j = ch; j < ch + ChannelCnt; j++)
                {
                    if (!m_ChannelOn[j]) continue;

                    foreach (var Cal in CalList[j])
                        if (Cal.Name == name)
                        {
                            if (name.Contains("X"))
                            {

                                DrvIC.Move(j, "X", Cal.CodeX[framCnt[port]]);
                                DrvIC.Move(j, "Y", OISCenter);
                            }
                            else if (name.Contains("Y"))
                            {
                                DrvIC.Move(j, "X", OISCenter);
                                DrvIC.Move(j, "Y", Cal.CodeY[framCnt[port]]);


                            }
                            else if (name.Contains("AF"))
                            {
                                currentAfCode = Cal.CodeZ[framCnt[port]];
                                DrvIC.AFMove(j, Cal.CodeZ[framCnt[port]]);
                            }

                            Cal.StrokeX.Add(0);
                            Cal.StrokeY.Add(0);
                            Cal.StrokeZ.Add(0);
                            Cal.StrokeY1.Add(0);
                            Cal.StrokeY2.Add(0);
                            Cal.HallX.Add(0);
                            Cal.HallY.Add(0);
                            Cal.HallZ.Add(0);
                            Cal.HallY1.Add(0);
                            Cal.HallY2.Add(0);
                            Cal.Current.Add(0);
                            Cal.TiltX.Add(0);
                            Cal.TiltY.Add(0);
                            Cal.TiltZ.Add(0);
                        }
                }

                if (name.Contains("X"))
                {
                    Thread.Sleep(Condition.iDrvStepIntervalX);
                }
                else if (name.Contains("Y"))
                {
                    Thread.Sleep(Condition.iDrvStepIntervalY);
                }
                else if (name.Contains("AF"))
                {
                    int stepIntervalZ =
                        currentAfCode < Condition.iDrvStepCodeZ1
                            ? Condition.iDrvStepIntervalZ
                            : currentAfCode < Condition.iDrvStepCodeZ2
                                ? Condition.iDrvStepIntervalZ2
                                : Condition.iDrvStepIntervalZ3;

                    Thread.Sleep(stepIntervalZ);

                    // 실제 적용한 대기시간 누적
                    totalSleepTimeZ += stepIntervalZ;
                }
                for (int j = ch; j < ch + ChannelCnt; j++)
                {
                    if (!m_ChannelOn[j]) continue;
                    foreach (var Cal in CalList[j])
                    {
                        if (Cal.Name == name)
                        {
                            if (Dln is Esp32WifiDevice esp)
                            {
                                // [ESP32 전용 최적화] 1회 통신으로 6가지 데이터 획득
                                byte[] res = esp.RunInternalSequence("GS", null);
                                if (res != null && res.Length >= 12)
                                {
                                    // Hall 값 (펌웨어에서 이미 >> 3 처리됨)
                                    Cal.HallX[framCnt[port]] = (short)((res[0] << 8) | res[1]);
                                    Cal.HallY1[framCnt[port]] = (short)((res[2] << 8) | res[3]);
                                    Cal.HallY2[framCnt[port]] = (short)((res[4] << 8) | res[5]);
                                    Cal.HallZ[framCnt[port]] = (short)((res[6] << 8) | res[7]);

                                    // 전류값 계산식 적용
                                    double cAF = ((res[8] << 8 | res[9]) / 10.0 + 10);
                                    double cOIS = ((res[10] << 8 | res[11]) / 10.0 + 10);
                                    Cal.Current[framCnt[port]] = name.Contains("AF") ? cAF : cOIS;
                                }
                            }
                            else
                            {
                                // [DLN 방식] 기존 개별 읽기 유지
                                Cal.HallX[framCnt[port]] = DrvIC.ReadHall(j, "X");
                                Cal.HallY1[framCnt[port]] = DrvIC.ReadHall(j, "Y1");
                                if (DrvIC.Y2SlaveAddr != 0x00) Cal.HallY2[framCnt[port]] = DrvIC.ReadHall(j, "Y2");
                                Cal.HallZ[framCnt[port]] = DrvIC.ReadAFHall(j);

                                //int curMode = name.Contains("AF") ? 0 : 1;
                                //Cal.Current[framCnt[port]] = Dln.GetCurrent(j, curMode);
                            }

                            // 로그 출력 (공통)
                            string logMsg = "";
                            if (name.Contains("X")) logMsg = $"{name} == Code : {Cal.CodeX[framCnt[port]]}, Hall : {Cal.HallX[framCnt[port]]}";
                            else if (name.Contains("Y")) logMsg = $"{name} == Code : {Cal.CodeY[framCnt[port]]}, Hall1 : {Cal.HallY1[framCnt[port]]}, Hall2 : {Cal.HallY2[framCnt[port]]}";
                            else if (name.Contains("AF")) logMsg = $"{name} == Code : {Cal.CodeZ[framCnt[port]]}, Hall : {Cal.HallZ[framCnt[port]]}";
                            AddLog(j, logMsg);
                        }
                    }
                }
                STATIC.fVision.m__G.oCam[port].GrabA(framCnt[port]);

                for (int j = ch; j < ch + ChannelCnt; j++)
                {
                    if (!m_ChannelOn[j]) continue;
                    foreach (var Cal in CalList[j])
                        if (Cal.Name == name)
                        {
                            if (name.Contains("X"))
                            {
                                if (Cal.CodeX.Count - 1 == framCnt[port]) IsScan[port] = false;
                            }
                            else if (name.Contains("Y"))
                            {
                                if (Cal.CodeY.Count - 1 == framCnt[port]) IsScan[port] = false;
                            }
                            else if (name.Contains("AF"))
                            {
                                if (Cal.CodeZ.Count - 1 == framCnt[port]) IsScan[port] = false;
                            }
                            int curMode = name.Contains("AF") ? 0 : 1;
                            Cal.Current[framCnt[port]] = Dln.GetCurrent(j, curMode);
                        }
                }

                framCnt[port]++;
            }
            long esec = sw.ElapsedMilliseconds;
            sw.Stop();

            double fps = 0;
            if (name.Contains("X"))
            {
                fps = esec - Condition.iDrvStepIntervalX * framCnt[port];
            }
            else if (name.Contains("Y"))
            {
                fps = esec - Condition.iDrvStepIntervalY * framCnt[port];
            }
            else if (name.Contains("AF"))
            {
                fps = esec - totalSleepTimeZ;
            }

            fps = fps / 1000;
            fps = framCnt[port] / fps * 2.4;

            for (int j = ch; j < ch + ChannelCnt; j++)
            {
                DrvIC.AFOnOff(j, false);
                DrvIC.OISOn(j, "X", false);
                DrvIC.OISOn(j, "Y", false);
            }
            for (int j = ch; j < ch + ChannelCnt; j++)
                AddLog(j, string.Format("framCnt {0}", framCnt[port]));

            STATIC.fVision.m__G.oCam[port].CommonToReplayBuf(name, framCnt[port]);
        }
        public void Process_CalcCodeTest(int port, string name, int InspCnt)
        {
            int ch = port * 2;
            string imgSvDir = "";

            for (int j = ch; j < ch + ChannelCnt; j++)
            {
                AddLog(j, string.Format("{0} Driving Data>>", name));
            }
            List<FindResult> result = new List<FindResult>();
            int fCount = 0;

            foreach (var Cal in CalList[port])
                if (Cal.Name == name)
                {
                    if (name.Contains("X"))
                    {
                        fCount = Cal.CodeX.Count;
                        imgSvDir = "C:\\6AxisTester\\Result\\Image\\Xcan\\";
                        Directory.CreateDirectory(imgSvDir);
                    }
                    else if (name.Contains("Y"))
                    {
                        fCount = Cal.CodeY.Count;
                        imgSvDir = "C:\\6AxisTester\\Result\\Image\\Ycan\\";
                        Directory.CreateDirectory(imgSvDir);

                    }
                    else if (name.Contains("AF"))
                    {
                        fCount = Cal.CodeZ.Count;
                        imgSvDir = "C:\\6AxisTester\\Result\\Image\\AFcan\\";
                        Directory.CreateDirectory(imgSvDir);
                    }
                    else if (name.Contains("Circle"))
                    {
                        fCount = Cal.CodeX.Count;
                        Directory.CreateDirectory("C:\\6AxisTester\\Result\\Image\\Circle_can\\");
                    }
                }
            Stopwatch sn = new Stopwatch();
            sn.Start();
            for (int i = 0; i < fCount; i++)
            {
                result.Add(new FindResult());
                if (imgSvDir.Length != 0)
                {
                    string imagePath = imgSvDir + $"ScanImage_{i}";
                    //result[i] = STATIC.fVision.MeasureTxTyTz(i, name, true, false).SaveImage(m__G, i, imagePath);
                    result[i] = STATIC.fVision.MeasureTxTyTz(i, name, true, false); ;//.SaveImage(m__G, i, imagePath);
                }
            }
            sn.Stop();
            AddLog(ch, $"{sn.ElapsedMilliseconds}");
            //////////////////////////////////////////////////////////////////////////////
            //////////////////////////////////////////////////////////////////////////////


            for (int j = ch; j < ch + ChannelCnt; j++)
            {
                if (!m_ChannelOn[j]) continue;
                foreach (var Cal in CalList[j])
                    if (Cal.Name == name)
                    {
                        double centerX = 0;
                        double centerY = 0;
                        double centerY1 = 0;
                        double centerY2 = 0;
                        double centerZ = 0;
                        double centertX = 0;
                        double centertY = 0;
                        double centertZ = 0;

                        bool isCentered = false;
                        if (name.Contains("Circle"))
                        {
                            for (int i = 0; i < fCount; i++)
                            {
                                if (Cal.CodeX[i] == OISCenter &&
                                Cal.CodeY[i] == OISCenter)
                                {
                                    centerX = result[i].cx[j];
                                    centerY = result[i].cy[j];
                                    centerZ = result[i].cz[j];
                                    centertX = result[i].tx[j];
                                    centertY = result[i].ty[j];
                                    centertZ = result[i].tz[j];
                                    centerY1 = result[i].cy1[j];
                                    centerY2 = result[i].cy2[j];
                                    isCentered = true;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            for (int i = 2; i < fCount; i++)
                            {
                                if (name.Contains("X"))
                                {
                                    if (Cal.CodeX[i] == OISCenter)
                                    {
                                        centerX = result[i].cx[j];
                                        centerY = result[i].cy[j];
                                        centerZ = result[i].cz[j];
                                        centertX = result[i].tx[j];
                                        centertY = result[i].ty[j];
                                        centertZ = result[i].tz[j];
                                        centerY1 = result[i].cy1[j];
                                        centerY2 = result[i].cy2[j];
                                        isCentered = true;
                                        break;
                                    }
                                }
                                else if (name.Contains("Y"))
                                {
                                    if (Cal.CodeY[i] == OISCenter)
                                    {
                                        centerX = result[i].cx[j];
                                        centerY = result[i].cy[j];
                                        centerZ = result[i].cz[j];
                                        centertX = result[i].tx[j];
                                        centertY = result[i].ty[j];
                                        centertZ = result[i].tz[j];
                                        centerY1 = result[i].cy1[j];
                                        centerY2 = result[i].cy2[j];
                                        isCentered = true;
                                        break;
                                    }
                                }
                                else if (name.Contains("AF"))
                                {
                                    if (Cal.CodeZ[i] == AFCenter)
                                    {
                                        centerX = result[i].cx[j];
                                        centerY = result[i].cy[j];
                                        centerZ = result[i].cz[j];
                                        centertX = result[i].tx[j];
                                        centertY = result[i].ty[j];
                                        centertZ = result[i].tz[j];
                                        centerY1 = result[i].cy1[j];
                                        centerY2 = result[i].cy2[j];
                                        isCentered = true;
                                        break;
                                    }
                                }
                                else if (name.Contains("OIS SCAN"))
                                {
                                    if (Cal.CodeZ[i] == AFCenter)
                                    {
                                        centerX = result[i].cx[j];
                                        centerY = result[i].cy[j];
                                        centerZ = result[i].cz[j];
                                        centertX = result[i].tx[j];
                                        centertY = result[i].ty[j];
                                        centertZ = result[i].tz[j];
                                        centerY1 = result[i].cy1[j];
                                        centerY2 = result[i].cy2[j];
                                        isCentered = true;
                                        break;
                                    }
                                }
                            }
                        }

                        if (!isCentered)
                        {
                            AddLog(j, string.Format("Center Code Data Failed"));
                        }

                        //if (Option.FixedCenter)
                        //{
                        //    int centerPoint = 0;
                        //    for (int i = 0; i < fCount; i++)
                        //    {
                        //        if (name.Contains("X"))
                        //        {
                        //            centerPoint = HallParam[j].XHmid;
                        //        }
                        //        else if (name.Contains("Y"))
                        //        {
                        //            centerPoint = HallParam[j].YHmid;
                        //        }

                        //    }
                        //    centerX = result[centerPoint].cx[j]; centerY = result[centerPoint].cy[j];
                        //    centerY1 = result[centerPoint].cy1[j]; centerY2 = result[centerPoint].cy2[j];
                        //}
                        //else
                        //{
                        //}
                        for (int i = 0; i < fCount; i++)
                        {
                            Cal.StrokeX[i] = result[i].cx[j] - centerX;
                            Cal.StrokeY[i] = result[i].cy[j] - centerY;
                            Cal.StrokeZ[i] = result[i].cz[j] - centerZ;
                            Cal.StrokeY1[i] = result[i].cy1[j] - centerY1;
                            Cal.StrokeY2[i] = result[i].cy2[j] - centerY2;
                            Cal.TiltX[i] = result[i].tx[j] - centertX;
                            Cal.TiltY[i] = result[i].ty[j] - centertY;
                            Cal.TiltZ[i] = result[i].tz[j] - centertZ;
                        }
                    }
            }
            if (Option.SaveRawData)
            {
                string str = Convert.ToString(yield.LastSampleNum + 1);
                string dateDir = STATIC.CreateDateDir();
                dateDir += "DrivingData\\";
                if (!Directory.Exists(dateDir))
                    Directory.CreateDirectory(dateDir);


                //string timeDir = string.Format("{0}{1}{2}", dt.Hour, dt.Minute, dt.Second);
                string timeDir = $"{STATIC.LogDate.Hour}h{STATIC.LogDate.Minute}m{STATIC.LogDate.Second}s";


                for (int j = ch; j < ch + ChannelCnt; j++)
                {
                    if (!m_ChannelOn[j]) continue;
                    foreach (var Cal in CalList[j])
                        if (Cal.Name == name)
                        {
                            List<string> arry = new List<string>();

                            string path = "";
                            switch (name)
                            {
                                case "AF Scan":

                                    arry.Add("i,AF Code,X Code,Y1 Code,Y2 Code,X,Y,Z,TX,TY,TZ,Y1,Y2,Hall X,Hall Y1,Hall Y2,Hall AF,Current");
                                    for (int i = 0; i < fCount; i++)
                                    {
                                        path = string.Format(dateDir + "{0}_{1}_{2}_{3}.csv", name, m_StrIndex[j], InspCnt + 1, timeDir);
                                        string data = string.Format("{0},{1},{2},{3},{4},{5:0.000},{6:0.000},{7:0.000},{8:0.000},{9:0.000},{10:0.000},{11:0.000},{12:0.000},{13},{14},{15},{16},{17:0.000}", i, Cal.CodeZ[i], Condition.iAFCrossOffsetX, Condition.iAFCrossOffsetY, Condition.iAFCrossOffsetY,
                                            Cal.StrokeX[i], Cal.StrokeY[i], Cal.StrokeZ[i], Cal.TiltX[i], Cal.TiltY[i], Cal.TiltZ[i], Cal.StrokeY1[i], Cal.StrokeY2[i],
                                            Cal.HallX[i], Cal.HallY1[i], Cal.HallY2[i], Cal.HallZ[i], Cal.Current[i]);
                                        arry.Add(data);
                                        if (i == 0)
                                            AddLog(j, string.Format("Code AF\tStroke AF\tTx\tTy\tTz"));
                                        AddLog(j, string.Format("{0}\t{1:0.000}\t{2:0.000}\t{3:0.000}\t{4:0.000}", Cal.CodeZ[i], Cal.StrokeZ[i], Cal.TiltX[i], Cal.TiltY[i], Cal.TiltZ[i]));
                                    }
                                    break;
                                case "OIS X Scan":

                                    arry.Add("i,AF Code,X Code,Y1 Code,Y2 Code,X,Y,Z,TX,TY,TZ,Y1,Y2,Hall X,Hall Y1,Hall Y2,Hall AF,Current");
                                    for (int i = 0; i < fCount; i++)
                                    {
                                        path = string.Format(dateDir + "{0}_{1}_{2}_{3}.csv", name, m_StrIndex[j], InspCnt + 1, timeDir);
                                        string data = string.Format("{0},{1},{2},{3},{4},{5:0.000},{6:0.000},{7:0.000},{8:0.000},{9:0.000},{10:0.000},{11:0.000},{12:0.000},{13},{14},{15},{16},{17:0.000}", i, BestAFPos, Cal.CodeX[i], Condition.iXCrossOffset, Condition.iXCrossOffset,
                                            Cal.StrokeX[i], Cal.StrokeY[i], Cal.StrokeZ[i], Cal.TiltX[i], Cal.TiltY[i], Cal.TiltZ[i], Cal.StrokeY1[i], Cal.StrokeY2[i],
                                            Cal.HallX[i], Cal.HallY1[i], Cal.HallY2[i], Cal.HallZ[i], Cal.Current[i]);
                                        arry.Add(data);

                                        if (i == 0)
                                            AddLog(j, string.Format("Code X\tStroke X\tTx\tTy\tTz"));
                                        AddLog(j, string.Format("{0}\t{1:0.000}\t{2:0.000}\t{3:0.000}\t{4:0.000}", Cal.CodeX[i], Cal.StrokeX[i], Cal.TiltX[i], Cal.TiltY[i], Cal.TiltZ[i]));
                                    }

                                    AddLog(j, string.Format("Cross Y1 Hall Max {0:00} Y1 Hall Min {1:00}", Cal.HallY1.Max(), Cal.HallY1.Min()));
                                    AddLog(j, string.Format("Cross Y1 Hall Diff {0:00}", Math.Abs(Cal.HallY1.Max() - Cal.HallY1.Min())));
                                    AddLog(j, string.Format("Cross Y2 Hall Max {0:00} Y2 Hall Min {1:00}", Cal.HallY2.Max(), Cal.HallY2.Min()));
                                    AddLog(j, string.Format("Cross Y2 Hall Diff {0:00}", Math.Abs(Cal.HallY2.Max() - Cal.HallY2.Min())));

                                    AddLog(j, string.Format("Rotation Max {0:00} Min {1:00}", Cal.TiltZ.Max(), Cal.TiltZ.Min()));
                                    AddLog(j, string.Format("Rotation Diff {0:00}", Math.Abs(Cal.TiltZ.Max() - Cal.TiltZ.Min())));

                                    break;
                                case "OIS Y Scan":

                                    arry.Add("i,AF Code,X Code,Y1 Code,Y2 Code,X,Y,Z,TX,TY,TZ,Y1,Y2,Hall X,Hall Y1,Hall Y2,Hall AF,Current");
                                    for (int i = 0; i < fCount; i++)
                                    {
                                        path = string.Format(dateDir + "{0}_{1}_{2}_{3}.csv", name, m_StrIndex[j], InspCnt + 1, timeDir);
                                        string data = string.Format("{0},{1},{2},{3},{4},{5:0.000},{6:0.000},{7:0.000},{8:0.000},{9:0.000},{10:0.000},{11:0.000},{12:0.000},{13},{14},{15},{16},{17:0.000}", i, BestAFPos, Condition.iYCrossOffset, Cal.CodeY[i], Cal.CodeY[i],
                                               Cal.StrokeX[i], Cal.StrokeY[i], Cal.StrokeZ[i], Cal.TiltX[i], Cal.TiltY[i], Cal.TiltZ[i], Cal.StrokeY1[i], Cal.StrokeY2[i],
                                             Cal.HallX[i], Cal.HallY1[i], Cal.HallY2[i], Cal.HallZ[i], Cal.Current[i]);
                                        arry.Add(data);

                                        if (i == 0)
                                            AddLog(j, string.Format("Code Y1\tCode Y2\tStroke Y1\tStroke Y2\t\tTx\tTy\tTz"));

                                        AddLog(j, string.Format("{0}\t{1}\t{2:0.000}\t{3:0.000}\t{4:0.000}\t{5:0.000}\t{6:0.000}", Cal.CodeY[i], Cal.CodeY[i], Cal.StrokeY1[i], Cal.StrokeY2[i], Cal.TiltX[i], Cal.TiltY[i], Cal.TiltZ[i]));
                                    }

                                    AddLog(j, string.Format("Cross X Hall Max {0:00} X Hall Min {1:00}", Cal.HallY2.Max(), Cal.HallY2.Min()));
                                    AddLog(j, string.Format("Cross X Hall Diff {0:00}", Math.Abs(Cal.HallY2.Max() - Cal.HallY2.Min())));

                                    AddLog(j, string.Format("Rotation Max {0:00} Min {1:00}", Cal.TiltZ.Max(), Cal.TiltZ.Min()));
                                    AddLog(j, string.Format("Rotation Diff {0:00}", Math.Abs(Cal.TiltZ.Max() - Cal.TiltZ.Min())));

                                    break;

                            }
                            if (path != "") STATIC.SetTextLine(path, arry);
                        }
                }
            }

            for (int j = ch; j < ch + ChannelCnt; j++)
            {
                if (!m_ChannelOn[j]) continue;
                double maxtiltX = 0, maxtiltY = 0;
                double[] refArray = null;
                foreach (var Cal in CalList[j])
                    if (Cal.Name == name)
                    {
                        double forword = 0, backword = 0;
                        if (name.Contains("Linearity")) return;
                        if (name.Contains("AF"))
                        {
                            forword = PassFails[j].Results[(int)SpecItem.AF_Forwardstroke].Val = Cal.StrokeZ.Max(); //Cal.CalFwdStoke(Cal.CodeZ, Cal.StrokeZ);
                            backword = PassFails[j].Results[(int)SpecItem.AF_Backwardstroke].Val = Cal.StrokeZ.Min(); //Cal.CalBwdStoke(Cal.CodeZ, Cal.StrokeZ);
                            PassFails[j].Results[(int)SpecItem.AF_Ratedstroke].Val = Math.Abs(forword - backword);
                            //PassFails[j].Results[(int)SpecItem.AF_Sensitivity].Val = Cal.CalSensitivity(Cal.CodeZ, Cal.StrokeZ, Condition.AFSensMinRange, Condition.AFSensMaxRange,
                            //    Condition.AFSensMinStroke, Condition.AFSensMaxStroke, Condition.AFSensMode, Condition.iAFDrvCodeMin, Condition.iAFDrvCodeMax);

                            double sensTivity = PassFails[j].Results[(int)SpecItem.AF_Ratedstroke].Val / 4095;
                            PassFails[j].Results[(int)SpecItem.AF_Sensitivity].Val = sensTivity;

                            int TargetCode = (int)(Condition.AFTargetStroke / sensTivity); 

                            PassFails[j].Results[(int)SpecItem.AF_Linearity].Val = Cal.CalLinearity(Cal.CodeZ, Cal.StrokeZ, Condition.AFLinMinRange, Condition.AFLinMaxRange,
                                Condition.AFLinMinStroke, Condition.AFLinMaxStroke, TargetCode, Condition.AFLinMode, Condition.iAFDrvCodeMin, Condition.iAFDrvCodeMax);
                            PassFails[j].Results[(int)SpecItem.AF_Linearity1].Val = Cal.CalLinearity(Cal.CodeZ, Cal.StrokeZ, Condition.AFLinMinRange1, Condition.AFLinMaxRange1,
                                Condition.AFLinMinStroke1, Condition.AFLinMaxStroke1, TargetCode, Condition.AFLinMode1, Condition.iAFDrvCodeMin, Condition.iAFDrvCodeMax);
                            PassFails[j].Results[(int)SpecItem.AF_Linearity2].Val = Cal.CalLinearity(Cal.CodeZ, Cal.StrokeZ, Condition.AFLinMinRange2, Condition.AFLinMaxRange2,
                                Condition.AFLinMinStroke2, Condition.AFLinMaxStroke2, TargetCode, Condition.AFLinMode2, Condition.iAFDrvCodeMin, Condition.iAFDrvCodeMax);

                            PassFails[j].Results[(int)SpecItem.AF_Hysteresis].Val = Cal.CalHysteresis(Cal.CodeZ, Cal.StrokeZ, Condition.AFHysMinRange, Condition.AFHysMaxRange,
                                Condition.AFHysMinStroke, Condition.AFHysMaxStroke, TargetCode, Condition.AFHysMode, Condition.iAFDrvCodeMin, Condition.iAFDrvCodeMax);
                            PassFails[j].Results[(int)SpecItem.AF_Hysteresis1].Val = Cal.CalHysteresis(Cal.CodeZ, Cal.StrokeZ, Condition.AFHysMinRange1, Condition.AFHysMaxRange1,
                                Condition.AFHysMinStroke1, Condition.AFHysMaxStroke1, TargetCode, Condition.AFHysMode1, Condition.iAFDrvCodeMin, Condition.iAFDrvCodeMax);
                            PassFails[j].Results[(int)SpecItem.AF_Hysteresis2].Val = Cal.CalHysteresis(Cal.CodeZ, Cal.StrokeZ, Condition.AFHysMinRange2, Condition.AFHysMaxRange2,
                                Condition.AFHysMinStroke2, Condition.AFHysMaxStroke2, TargetCode, Condition.AFHysMode2, Condition.iAFDrvCodeMin, Condition.iAFDrvCodeMax);
                            double[] MtoM = Cal.CalCurrent(Cal.CodeZ, Cal.StrokeZ, Cal.Current, Condition.AFCurrMinRange, Condition.AFCurrMaxRange,
                                Condition.AFCurrMinStroke, Condition.AFCurrMaxStroke, TargetCode, Condition.AFCurrMode, Condition.iAFDrvCodeMin, Condition.iAFDrvCodeMax);

                            //    PassFails[j].Results[(int)SpecItem.AF_MaxCurrent].Val = MtoM[0]; //Cal.CalMaxCurrent(Cal.CodeZ, Cal.StrokeZ, Condition.iAFCodeRange, Condition.iAFStrokeRange);
                            //  PassFails[j].Results[(int)SpecItem.AF_MinCurrent].Val = MtoM[1];
                            //     PassFails[j].Results[(int)SpecItem.AF_HoldingCurrent].Val = Cal.CalHoldingCurrent(Cal.CodeZ, Cal.StrokeZ, Condition.iAFCodeRange, Condition.iAFStrokeRange);
                            PassFails[j].Results[(int)SpecItem.AF_CrosstalkX].Val = Cal.CalCrosstalkAF(Cal.CodeZ, Cal.StrokeZ, Cal.StrokeX, Condition.AFCrossMinRange, Condition.AFCrossMaxRange,
                                Condition.AFCrossMinStroke, Condition.AFCrossMaxStroke, TargetCode, Condition.AFCrossMode);
                            PassFails[j].Results[(int)SpecItem.AF_CrosstalkY].Val = Cal.CalCrosstalkAF(Cal.CodeZ, Cal.StrokeZ, Cal.StrokeY, Condition.AFCrossMinRange, Condition.AFCrossMaxRange,
                                Condition.AFCrossMinStroke, Condition.AFCrossMaxStroke, TargetCode, Condition.AFCrossMode);
                            PassFails[j].Results[(int)SpecItem.AF_CrosstalkX1].Val = Cal.CalCrosstalkAF(Cal.CodeZ, Cal.StrokeZ, Cal.StrokeX, Condition.AFCrossMinRange1, Condition.AFCrossMaxRange1,
                                Condition.AFCrossMinStroke1, Condition.AFCrossMaxStroke1, TargetCode, Condition.AFCrossMode1);
                            PassFails[j].Results[(int)SpecItem.AF_CrosstalkY1].Val = Cal.CalCrosstalkAF(Cal.CodeZ, Cal.StrokeZ, Cal.StrokeY, Condition.AFCrossMinRange1, Condition.AFCrossMaxRange1,
                                Condition.AFCrossMinStroke1, Condition.AFCrossMaxStroke1, TargetCode, Condition.AFCrossMode1);
                            PassFails[j].Results[(int)SpecItem.AF_CrosstalkX2].Val = Cal.CalCrosstalkAF(Cal.CodeZ, Cal.StrokeZ, Cal.StrokeX, Condition.AFCrossMinRange2, Condition.AFCrossMaxRange2,
                                Condition.AFCrossMinStroke2, Condition.AFCrossMaxStroke2, TargetCode, Condition.AFCrossMode2);
                            PassFails[j].Results[(int)SpecItem.AF_CrosstalkY2].Val = Cal.CalCrosstalkAF(Cal.CodeZ, Cal.StrokeZ, Cal.StrokeY, Condition.AFCrossMinRange2, Condition.AFCrossMaxRange2,
                                Condition.AFCrossMinStroke2, Condition.AFCrossMaxStroke2, TargetCode, Condition.AFCrossMode2);
                            //  PassFails[j].Results[(int)SpecItem.AF_CrosstalkR].Val = Cal.CalCrosstalkR(Cal.CodeZ, Cal.StrokeX, Cal.StrokeY, Condition.iAFCodeRange, Condition.iAFStrokeRange, AFCenter);
                            //    PassFails[j].Results[(int)SpecItem.AF_Rolling].Val = Cal.CalRolling(Cal.CodeZ, Cal.StrokeZ, Condition.iAFCodeRange, Condition.iAFStrokeRange, AFCenter);

                            //(double sqrT, double maxX, double maxY, double[] refArr) = Cal.CalTilt(Cal.CodeZ, Cal.TiltX, Cal.TiltY, Condition.TiltMinCode, Condition.TiltMaxCode, Condition.TiltRefCode, Condition.iAFDrvCodeMin, Condition.iAFDrvCodeMax);
                            (double sqrT, double maxX, double maxY, double[] refArr) = Cal.CalTilt(Cal.CodeZ,Cal.TiltX,Cal.TiltY,Condition.TiltMinCode,Condition.TiltMaxCode,
                            TargetCode, Condition.TiltRefCode,Condition.iAFDrvCodeMin,Condition.iAFDrvCodeMax,Condition.TiltMode);

                            maxtiltX = maxX; maxtiltY = maxY;
                            refArray = refArr;
                            PassFails[j].Results[(int)SpecItem.AF_Tilt].Val = sqrT;

                            //(double sqrT2, double maxX2, double maxY2, double[] refArr2) = Cal.CalTilt(Cal.CodeZ, Cal.TiltX, Cal.TiltY, Condition.TiltMinCode2, Condition.TiltMaxCode2, Condition.TiltRefCode2, Condition.iAFDrvCodeMin, Condition.iAFDrvCodeMax);
                            (double sqrT2, double maxX2, double maxY2, double[] refArr2) = Cal.CalTilt(Cal.CodeZ, Cal.TiltX, Cal.TiltY, Condition.TiltMinCode2, Condition.TiltMaxCode2,
                            TargetCode, Condition.TiltRefCode2, Condition.iAFDrvCodeMin, Condition.iAFDrvCodeMax, Condition.TiltMode2);


                            PassFails[j].Results[(int)SpecItem.AF_Tilt2].Val = sqrT2;

                            ShowDataResults(j, (int)SpecItem.AF_Ratedstroke, (int)SpecItem.AF_Ratedstroke, InspType.Normal, new double[] { });
                            ShowDataResults(j, (int)SpecItem.AF_Backwardstroke, (int)SpecItem.AF_Backwardstroke, InspType.OnlyMax, new double[] { });
                            ShowDataResults(j, (int)SpecItem.AF_Forwardstroke, (int)SpecItem.AF_Forwardstroke, InspType.OnlyMin, new double[] { });
                            ShowDataResults(j, (int)SpecItem.AF_Sensitivity, (int)SpecItem.AF_Sensitivity, InspType.OnlyMax, new double[] { });
                            ShowDataResults(j, (int)SpecItem.AF_Hysteresis, (int)SpecItem.AF_Hysteresis2, InspType.OnlyMax, new double[] { });
                            ShowDataResults(j, (int)SpecItem.AF_Linearity, (int)SpecItem.AF_Linearity2, InspType.OnlyMax, new double[] { });
                            ShowDataResults(j, (int)SpecItem.AF_Current, (int)SpecItem.AF_Current, InspType.MintoMax, MtoM);
                            ShowDataResults(j, (int)SpecItem.AF_CrosstalkX, (int)SpecItem.AF_CrosstalkY2, InspType.OnlyMax, new double[] { });
                            ///ShowDataResults(j, (int)SpecItem.AF_CrosstalkY, (int)SpecItem.AF_CrosstalkY, InspType.OnlyMax, new double[] { });
                            ShowDataResults(j, (int)SpecItem.AF_Tilt, (int)SpecItem.AF_Tilt, InspType.Normal, new double[] { });
                            ShowDataResults(j, (int)SpecItem.AF_Tilt2, (int)SpecItem.AF_Tilt2, InspType.Normal, new double[] { });
                            AFCurrentMinMax = MtoM.ToArray();





                        }
                        else if (name.Contains("OIS X"))
                        {
                            if (name.Contains("Mac"))
                            {
                                (double sqrT, double maxX, double maxY, double[] refArr) = Cal.CalTilt(Cal.CodeX, Cal.TiltY, Cal.TiltZ, 400, 7200, 4096, Condition.iXDrvCodeMin, Condition.iXDrvCodeMax);
                                maxtiltX = maxX; maxtiltY = maxY;
                                refArray = refArr;
                                PassFails[j].Results[(int)SpecItem.OISX_Tilt_AFMac].Val = sqrT;
                                ShowDataResults(j, (int)SpecItem.OISX_Tilt_AFMac, (int)SpecItem.OISX_Tilt_AFMac, InspType.Normal, new double[] { });
                            }
                            else
                            {
                                forword = PassFails[j].Results[(int)SpecItem.OISX_Forwardstroke].Val = Cal.StrokeX.Max();// Cal.CalFwdStoke(Cal.CodeX, Cal.StrokeX);
                                backword = PassFails[j].Results[(int)SpecItem.OISX_Backwardstroke].Val = Cal.StrokeX.Min();//Cal.CalBwdStoke(Cal.CodeX, Cal.StrokeX);
                                PassFails[j].Results[(int)SpecItem.OISX_Ratedstroke].Val = Math.Abs(forword - backword);

                                double sensTivity = PassFails[j].Results[(int)SpecItem.OISX_Ratedstroke].Val / 8191;
                                PassFails[j].Results[(int)SpecItem.OISX_Sensitivity].Val = sensTivity;

                                int TargetCode = (int)(Condition.OISXTargetStroke / sensTivity);

                                //PassFails[j].Results[(int)SpecItem.OISX_Sensitivity].Val = Cal.CalSensitivity(Cal.CodeX, Cal.StrokeX, Condition.XSensMinRange, Condition.XSensMaxRange,
                                //    Condition.XSensMinStroke, Condition.XSensMaxStroke, Condition.XSensMode, Condition.iXDrvCodeMin, Condition.iXDrvCodeMax);
                                PassFails[j].Results[(int)SpecItem.OISX_Linearity].Val = Cal.CalLinearity(Cal.CodeX, Cal.StrokeX, Condition.XLinMinRange, Condition.XLinMaxRange,
                                    Condition.XLinMinStroke, Condition.XLinMaxStroke, TargetCode, Condition.XLinMode, Condition.iXDrvCodeMin, Condition.iXDrvCodeMax);
                                PassFails[j].Results[(int)SpecItem.OISX_Linearity1].Val = Cal.CalLinearity(Cal.CodeX, Cal.StrokeX, Condition.XLinMinRange1, Condition.XLinMaxRange1,
                                    Condition.XLinMinStroke1, Condition.XLinMaxStroke1, TargetCode, Condition.XLinMode1, Condition.iXDrvCodeMin, Condition.iXDrvCodeMax);
                                PassFails[j].Results[(int)SpecItem.OISX_Linearity2].Val = Cal.CalLinearity(Cal.CodeX, Cal.StrokeX, Condition.XLinMinRange2, Condition.XLinMaxRange2,
                                    Condition.XLinMinStroke2, Condition.XLinMaxStroke2, TargetCode, Condition.XLinMode2, Condition.iXDrvCodeMin, Condition.iXDrvCodeMax);

                                PassFails[j].Results[(int)SpecItem.OISX_Hysteresis].Val = Cal.CalHysteresis(Cal.CodeX, Cal.StrokeX, Condition.XHysMinRange, Condition.XHysMaxRange,
                                    Condition.XHysMinStroke, Condition.XHysMaxStroke, TargetCode, Condition.XHysMode, Condition.iXDrvCodeMin, Condition.iXDrvCodeMax);
                                PassFails[j].Results[(int)SpecItem.OISX_Hysteresis1].Val = Cal.CalHysteresis(Cal.CodeX, Cal.StrokeX, Condition.XHysMinRange1, Condition.XHysMaxRange1,
                                    Condition.XHysMinStroke1, Condition.XHysMaxStroke1, TargetCode, Condition.XHysMode1, Condition.iXDrvCodeMin, Condition.iXDrvCodeMax);
                                PassFails[j].Results[(int)SpecItem.OISX_Hysteresis2].Val = Cal.CalHysteresis(Cal.CodeX, Cal.StrokeX, Condition.XHysMinRange2, Condition.XHysMaxRange2,
                                    Condition.XHysMinStroke2, Condition.XHysMaxStroke2, TargetCode, Condition.XHysMode2, Condition.iXDrvCodeMin, Condition.iXDrvCodeMax);

                                double[] MtoM = Cal.CalCurrent(Cal.CodeX, Cal.StrokeX, Cal.Current, Condition.XCurrMinRange, Condition.XCurrMaxRange,
                                  Condition.XCurrMinStroke, Condition.XCurrMaxStroke, TargetCode, Condition.XCurrMode, Condition.iXDrvCodeMin, Condition.iXDrvCodeMax);
                                //          PassFails[j].Results[(int)SpecItem.OISX_MaxCurrent].Val = MtoM[0]; //Cal.CalMaxCurrent(Cal.CodeX, Cal.StrokeX, Condition.iXCodeRange, Condition.iXStrokeRange);
                                //     PassFails[j].Results[(int)SpecItem.OISX_MinCurrent].Val = MtoM[1];
                                // PassFails[j].Results[(int)SpecItem.OISX_CenteringCurrent].Val = Cal.CalCenterCurrent(Cal.CodeX, Cal.StrokeX, Condition.iXCodeRange, Condition.iXCodeRange);
                                PassFails[j].Results[(int)SpecItem.OISX_CrosstalkFullY].Val = Cal.CalCrosstalk(Cal.CodeX, Cal.StrokeX, Cal.StrokeY, 0, 8191,
                                    Condition.XCrossMinStroke, Condition.XCrossMaxStroke, TargetCode, 0);
                                PassFails[j].Results[(int)SpecItem.OISX_CrosstalkY].Val = Cal.CalCrosstalk(Cal.CodeX, Cal.StrokeX, Cal.StrokeY, Condition.XCrossMinRange, Condition.XCrossMaxRange,
                                    Condition.XCrossMinStroke, Condition.XCrossMaxStroke, TargetCode, Condition.XCrossMode);
                                PassFails[j].Results[(int)SpecItem.OISX_CrosstalkY1].Val = Cal.CalCrosstalk(Cal.CodeX, Cal.StrokeX, Cal.StrokeY, Condition.XCrossMinRange1, Condition.XCrossMaxRange1,
                                    Condition.XCrossMinStroke1, Condition.XCrossMaxStroke1, TargetCode, Condition.XCrossMode1);
                                PassFails[j].Results[(int)SpecItem.OISX_CrosstalkY2].Val = Cal.CalCrosstalk(Cal.CodeX, Cal.StrokeX, Cal.StrokeY, Condition.XCrossMinRange2, Condition.XCrossMaxRange2,
                                    Condition.XCrossMinStroke2, Condition.XCrossMaxStroke2, TargetCode, Condition.XCrossMode2);

                                (double sqrT, double maxX, double maxY, double[] refArr) = Cal.CalTilt(Cal.CodeX, Cal.TiltY, Cal.TiltZ, 400, 7200, 4096, Condition.iXDrvCodeMin, Condition.iXDrvCodeMax);
                                maxtiltX = maxX; maxtiltY = maxY;
                                refArray = refArr;
                                PassFails[j].Results[(int)SpecItem.OISX_Tilt].Val = sqrT;

                                //  PassFails[j].Results[(int)SpecItem.OISX_CrosstalkY_dB].Val = xtalkRes[1];
                                //   PassFails[j].Results[(int)SpecItem.OISX_CrosstalkY_P2P].Val = xtalkRes[2];
                                //  PassFails[j].Results[(int)SpecItem.OISX_CrosstalkYP2P_dB].Val = xtalkRes[3];
                                //PassFails[j].Results[(int)SpecItem.OISX_CrosstalkZ].Val = Cal.CalCrosstalk(Cal.CodeX, Cal.StrokeZ, Condition.iXCodeRange, Condition.iXCodeRange);
                                //PassFails[j].Results[(int)SpecItem.OISX_CrosstalkR].Val = Cal.CalCrosstalkR(Cal.CodeX, Cal.StrokeY, Cal.StrokeZ, Condition.iXCodeRange, Condition.iXCodeRange);
                                // PassFails[j].Results[(int)SpecItem.OISX_Rolling].Val = Cal.CalRolling(Cal.CodeX, Cal.StrokeX, Condition.iXCodeRange, Condition.iXStrokeRange, OISCenter);


                                SlopeX = Cal.CalSlopeForOISShift(Cal.CodeX, Cal.StrokeX);

                                PassFails[j].Results[(int)SpecItem.x_HallDecenter].Val = (Math.Abs(forword) - Math.Abs(backword)) / 2.0;

                                ShowDataResults(j, (int)SpecItem.OISX_Ratedstroke, (int)SpecItem.OISX_Ratedstroke, InspType.Normal, new double[] { });
                                ShowDataResults(j, (int)SpecItem.OISX_Backwardstroke, (int)SpecItem.OISX_Backwardstroke, InspType.OnlyMax, new double[] { });
                                ShowDataResults(j, (int)SpecItem.OISX_Forwardstroke, (int)SpecItem.OISX_Forwardstroke, InspType.OnlyMin, new double[] { });
                                ShowDataResults(j, (int)SpecItem.OISX_Sensitivity, (int)SpecItem.OISX_Sensitivity, InspType.OnlyMax, new double[] { });
                                ShowDataResults(j, (int)SpecItem.OISX_Hysteresis, (int)SpecItem.OISX_Hysteresis2, InspType.OnlyMax, new double[] { });
                                ShowDataResults(j, (int)SpecItem.OISX_Linearity, (int)SpecItem.OISX_Linearity2, InspType.OnlyMax, new double[] { });
                                ShowDataResults(j, (int)SpecItem.OISX_Current, (int)SpecItem.OISX_Current, InspType.MintoMax, MtoM);
                                ShowDataResults(j, (int)SpecItem.OISX_CrosstalkFullY, (int)SpecItem.OISX_CrosstalkFullY, InspType.OnlyMax, new double[] { });
                                ShowDataResults(j, (int)SpecItem.OISX_CrosstalkY, (int)SpecItem.OISX_CrosstalkY2, InspType.OnlyMax, new double[] { });
                                ShowDataResults(j, (int)SpecItem.OISX_Tilt, (int)SpecItem.OISX_Tilt, InspType.Normal, new double[] { });
                                ShowDataResults(j, (int)SpecItem.x_HallDecenter, (int)SpecItem.x_HallDecenter, InspType.Normal, new double[] { });
                                OISXCurrentMinMax = MtoM.ToArray();
                            }


                        }
                        else if (name.Contains("OIS Y"))
                        {
                            if (name.Contains("Mac"))
                            {
                                (double sqrT, double maxX, double maxY, double[] refArr) = Cal.CalTilt(Cal.CodeY, Cal.TiltX, Cal.TiltZ, 400, 7200, 4096, Condition.iXDrvCodeMin, Condition.iXDrvCodeMax);
                                maxtiltX = maxX; maxtiltY = maxY;
                                refArray = refArr;
                                PassFails[j].Results[(int)SpecItem.OISY_Tilt_AFMac].Val = sqrT;
                                ShowDataResults(j, (int)SpecItem.OISY_Tilt_AFMac, (int)SpecItem.OISY_Tilt_AFMac, InspType.Normal, new double[] { });
                            }
                            else
                            {
                                forword = PassFails[j].Results[(int)SpecItem.OISY_Forwardstroke].Val = Cal.StrokeY.Max();// Cal.CalFwdStoke(Cal.CodeY1, Cal.StrokeY);
                                backword = PassFails[j].Results[(int)SpecItem.OISY_Backwardstroke].Val = Cal.StrokeY.Min(); // Cal.CalBwdStoke(Cal.CodeY1, Cal.StrokeY);
                                PassFails[j].Results[(int)SpecItem.OISY_Ratedstroke].Val = Math.Abs(forword - backword);

                                double sensTivity = PassFails[j].Results[(int)SpecItem.OISY_Ratedstroke].Val / 8191;
                                PassFails[j].Results[(int)SpecItem.OISY_Sensitivity].Val = sensTivity;

                                int TargetCode = (int)(Condition.OISYTargetStroke / sensTivity);

                                //PassFails[j].Results[(int)SpecItem.OISY_Sensitivity].Val = Cal.CalSensitivity(Cal.CodeY, Cal.StrokeY, Condition.YSensMinRange, Condition.YSensMaxRange,
                                //Condition.YSensMinStroke, Condition.YSensMaxStroke, Condition.YSensMode, Condition.iYDrvCodeMin, Condition.iYDrvCodeMax);
                                PassFails[j].Results[(int)SpecItem.OISY_Linearity].Val = Cal.CalLinearity(Cal.CodeY, Cal.StrokeY, Condition.YLinMinRange, Condition.YLinMaxRange,
                                    Condition.YLinMinStroke, Condition.YLinMaxStroke, TargetCode, Condition.YLinMode, Condition.iYDrvCodeMin, Condition.iYDrvCodeMax);
                                PassFails[j].Results[(int)SpecItem.OISY_Linearity1].Val = Cal.CalLinearity(Cal.CodeY, Cal.StrokeY, Condition.YLinMinRange1, Condition.YLinMaxRange1,
                                    Condition.YLinMinStroke1, Condition.YLinMaxStroke1, TargetCode, Condition.YLinMode1, Condition.iYDrvCodeMin, Condition.iYDrvCodeMax);
                                PassFails[j].Results[(int)SpecItem.OISY_Linearity2].Val = Cal.CalLinearity(Cal.CodeY, Cal.StrokeY, Condition.YLinMinRange2, Condition.YLinMaxRange2,
                                    Condition.YLinMinStroke2, Condition.YLinMaxStroke2, TargetCode, Condition.YLinMode2, Condition.iYDrvCodeMin, Condition.iYDrvCodeMax);

                                PassFails[j].Results[(int)SpecItem.OISY_Hysteresis].Val = Cal.CalHysteresis(Cal.CodeY, Cal.StrokeY, Condition.YHysMinRange, Condition.YHysMaxRange,
                                    Condition.YHysMinStroke, Condition.YHysMaxStroke, TargetCode, Condition.YHysMode, Condition.iYDrvCodeMin, Condition.iYDrvCodeMax);
                                PassFails[j].Results[(int)SpecItem.OISY_Hysteresis1].Val = Cal.CalHysteresis(Cal.CodeY, Cal.StrokeY, Condition.YHysMinRange1, Condition.YHysMaxRange1,
                                    Condition.YHysMinStroke1, Condition.YHysMaxStroke1, TargetCode, Condition.YHysMode1, Condition.iYDrvCodeMin, Condition.iYDrvCodeMax);
                                PassFails[j].Results[(int)SpecItem.OISY_Hysteresis2].Val = Cal.CalHysteresis(Cal.CodeY, Cal.StrokeY, Condition.YHysMinRange2, Condition.YHysMaxRange2,
                                    Condition.YHysMinStroke2, Condition.YHysMaxStroke2, TargetCode, Condition.YHysMode2, Condition.iYDrvCodeMin, Condition.iYDrvCodeMax);

                                double[] MtoM = Cal.CalCurrent(Cal.CodeY, Cal.StrokeY, Cal.Current, Condition.YCurrMinRange, Condition.YCurrMaxRange,
                                Condition.YCurrMinStroke, Condition.YCurrMaxStroke, TargetCode, Condition.YCurrMode, Condition.iYDrvCodeMin, Condition.iYDrvCodeMax);

                                // PassFails[j].Results[(int)SpecItem.OISY_MaxCurrent].Val = MtoM[0]; //Cal.CalMaxCurrent(Cal.CodeY1, Cal.StrokeY, Condition.iYCodeRange, Condition.iYStrokeRange);
                                //      PassFails[j].Results[(int)SpecItem.OISY_MinCurrent].Val = MtoM[1];
                                //   PassFails[j].Results[(int)SpecItem.OISY_CenteringCurrent].Val = Cal.CalCenterCurrent(Cal.CodeY1, Cal.StrokeY, Condition.iYCodeRange, Condition.iYStrokeRange);
                                //     PassFails[j].Results[(int)SpecItem.OISY_CrosstalkX].Val = Cal.CalCrosstalk(Cal.CodeY1, Cal.StrokeX, Condition.iYStrokeRange, Condition.iYStrokeRange);
                                PassFails[j].Results[(int)SpecItem.OISY_CrosstalkFullX].Val = Cal.CalCrosstalk(Cal.CodeY, Cal.StrokeY, Cal.StrokeX, 0, 8191,
                                    Condition.YCrossMinStroke, Condition.YCrossMaxStroke, TargetCode, 0);
                                PassFails[j].Results[(int)SpecItem.OISY_CrosstalkX].Val = Cal.CalCrosstalk(Cal.CodeY, Cal.StrokeY, Cal.StrokeX, Condition.YCrossMinRange, Condition.YCrossMaxRange,
                                    Condition.YCrossMinStroke, Condition.YCrossMaxStroke, TargetCode, Condition.YCrossMode);
                                PassFails[j].Results[(int)SpecItem.OISY_CrosstalkX1].Val = Cal.CalCrosstalk(Cal.CodeY, Cal.StrokeY, Cal.StrokeX, Condition.YCrossMinRange1, Condition.YCrossMaxRange1,
                                    Condition.YCrossMinStroke1, Condition.YCrossMaxStroke1, TargetCode, Condition.YCrossMode1);
                                PassFails[j].Results[(int)SpecItem.OISY_CrosstalkX2].Val = Cal.CalCrosstalk(Cal.CodeY, Cal.StrokeY, Cal.StrokeX, Condition.YCrossMinRange2, Condition.YCrossMaxRange2,
                                    Condition.YCrossMinStroke2, Condition.YCrossMaxStroke2, TargetCode, Condition.YCrossMode2);

                                (double sqrT, double maxX, double maxY, double[] refArr) = Cal.CalTilt(Cal.CodeY, Cal.TiltX, Cal.TiltZ, 400, 7200, 4096, Condition.iXDrvCodeMin, Condition.iXDrvCodeMax);
                                maxtiltX = maxX; maxtiltY = maxY;
                                refArray = refArr;
                                PassFails[j].Results[(int)SpecItem.OISY_Tilt].Val = sqrT;

                                //     PassFails[j].Results[(int)SpecItem.OISY_CrosstalkX_dB].Val = xtalkRes[1];
                                //     PassFails[j].Results[(int)SpecItem.OISY_CrosstalkX_P2P].Val = xtalkRes[2];
                                //    PassFails[j].Results[(int)SpecItem.OISY_CrosstalkXP2P_dB].Val = xtalkRes[3];

                                //PassFails[j].Results[(int)SpecItem.OISY_CrosstalkZ].Val = Cal.CalCrosstalk(Cal.CodeY1, Cal.StrokeZ, Condition.iYStrokeRange, Condition.iYStrokeRange);
                                //PassFails[j].Results[(int)SpecItem.OISY_CrosstalkR].Val = Cal.CalCrosstalkR(Cal.CodeY1, Cal.StrokeX, Cal.StrokeZ, Condition.iYStrokeRange, Condition.iYStrokeRange);
                                //      PassFails[j].Results[(int)SpecItem.OISY_Rolling].Val = Cal.CalRolling(Cal.CodeY, Cal.StrokeY, Condition.iYCodeRange, Condition.iYStrokeRange, OISCenter);
                                SlopeY = Cal.CalSlopeForOISShift(Cal.CodeY, Cal.StrokeY);
                                PassFails[j].Results[(int)SpecItem.y_HallDecenter].Val = (Math.Abs(forword) - Math.Abs(backword)) / 2.0;
                                ShowDataResults(j, (int)SpecItem.OISY_Ratedstroke, (int)SpecItem.OISY_Ratedstroke, InspType.Normal, new double[] { });
                                ShowDataResults(j, (int)SpecItem.OISY_Backwardstroke, (int)SpecItem.OISY_Backwardstroke, InspType.OnlyMax, new double[] { });
                                ShowDataResults(j, (int)SpecItem.OISY_Forwardstroke, (int)SpecItem.OISY_Forwardstroke, InspType.OnlyMin, new double[] { });
                                ShowDataResults(j, (int)SpecItem.OISY_Sensitivity, (int)SpecItem.OISY_Sensitivity, InspType.OnlyMax, new double[] { });
                                ShowDataResults(j, (int)SpecItem.OISY_Hysteresis, (int)SpecItem.OISY_Hysteresis2, InspType.OnlyMax, new double[] { });
                                ShowDataResults(j, (int)SpecItem.OISY_Linearity, (int)SpecItem.OISY_Linearity2, InspType.OnlyMax, new double[] { });
                                ShowDataResults(j, (int)SpecItem.OISY_Current, (int)SpecItem.OISY_Current, InspType.MintoMax, MtoM);
                                ShowDataResults(j, (int)SpecItem.OISY_CrosstalkFullX, (int)SpecItem.OISY_CrosstalkFullX, InspType.OnlyMax, new double[] { });
                                ShowDataResults(j, (int)SpecItem.OISY_CrosstalkX, (int)SpecItem.OISY_CrosstalkX2, InspType.OnlyMax, new double[] { });
                                ShowDataResults(j, (int)SpecItem.OISY_Tilt, (int)SpecItem.OISY_Tilt, InspType.Normal, new double[] { });
                                ShowDataResults(j, (int)SpecItem.y_HallDecenter, (int)SpecItem.y_HallDecenter, InspType.Normal, new double[] { });
                                OISYCurrentMinMax = MtoM.ToArray();
                            }

                        }
                        else if (name.Contains("Circle"))
                        {
                            //여기 처리해야됨.
                            IndexValue<int, double> maxVlaueX = new IndexValue<int, double>();
                            IndexValue<int, double> maxVlaueY = new IndexValue<int, double>();
                            IndexValue<int, double> resultValue = new IndexValue<int, double>();

                            var diffStrokeY = Cal.StrokeY.Select((mesureY, i) => Math.Abs(mesureY - Cal.CircleStrokeY[i])).ToList();
                            var diffStrokeX = Cal.StrokeX.Select((mesureX, i) => Math.Abs(mesureX - Cal.CircleStrokeX[i])).ToList();

                            var maxItemX = diffStrokeX.Select((value, index) => new { value, index })
                                                     .OrderByDescending(x => x.value)
                                                     .First();

                            var maxItemY = diffStrokeY.Select((value, index) => new { value, index })
                                                     .OrderByDescending(x => x.value)
                                                     .First();

                            maxVlaueX.Value = maxItemX.value;
                            maxVlaueX.Index = maxItemX.index;

                            maxVlaueY.Value = maxItemY.value;
                            maxVlaueY.Index = maxItemY.index;

                            if (maxVlaueX.Value > maxVlaueY.Value)
                            {
                                resultValue = maxVlaueX;
                            }
                            else
                            {
                                resultValue = maxVlaueY;
                            }


                            AddLog(j, $"--------------------DifferentStroke-----------------------");
                            for (int index = 0; index < diffStrokeY.Count; index++)
                            {
                                AddLog(j, $"X :{diffStrokeX[index]:F2} Y:{diffStrokeY[index]:F2}");
                            }

                            AddLog(j, $"----------------------------------------------------------");
                        }

                        int retryIndex = Rcp.RetryCnt.RetryOption.FindIndex(x => x.InspName == name);
                        int totalRetryCnt = 1;
                        if (retryIndex >= 0)
                        {
                            totalRetryCnt = Rcp.RetryCnt.RetryOption[retryIndex].Count + 1;
                        }

                        // Check if all test items passed (no failures recorded)
                        bool isPass = (PassFails[j].FirstFailIndex == 0);

                        // Check if this is the final retry attempt
                        bool isLastTry = (InspCnt >= totalRetryCnt - 1);

                        // Draw the chart if the test passed, or if it's the final attempt regardless of the result
                        if (isPass || isLastTry)
                        {
                            AddChart(j, name, null, null, maxtiltX, maxtiltY, refArray);
                        }
                    }
            }
            framCnt[port] = 0;
        }
        private void Process_ScanTimeTest(int port, string name)
        {
            try
            {
                framCnt[port] = 0;
                int ch = port * 2;
                int TagerCode = Condition.iAFStandbyCode;
                if (name.Contains("2"))
                {
                    TagerCode = Condition.iAFStandbyCode2;
                }

                double sensAf = PassFails[ch].Results[(int)SpecItem.AF_Sensitivity].Val;
                if (sensAf > 9999)
                {
                    AddLog(ch, string.Format($"AF Scan Not Drived =="));
                    IsScanError = true;
                    return;
                }
                AddLog(ch, string.Format($"StandbyCode {TagerCode}, Sensitivity {sensAf:F4}"));
                if (name.Contains("2"))
                {
                    TagerCode += StroketoCode(Condition.iAFTarget2, sensAf);
                    AddLog(ch, string.Format($"Target Stroke {Condition.iAFTarget2}, TagerCode {TagerCode}"));
                }
                else
                {
                    TagerCode += StroketoCode(Condition.iAFTarget, sensAf);
                    AddLog(ch, string.Format($"Target Stroke {Condition.iAFTarget}, TagerCode {TagerCode}"));
                }

                DrvIC.AFOnOff(ch, true);
                DrvIC.OISOn(ch, "X", false);
                DrvIC.OISOn(ch, "Y", false);

                Thread.Sleep(100);
                Stopwatch sw = new Stopwatch();
                sw.Reset(); sw.Start();
                //Time Grab ===============

                Task Func1 = null, Func2 = null;
                long startTime = 0;
                long endTime = 0;
                long lTimerFrequency = 0;
                SupremeTimer.QueryPerformanceCounter(ref startTime);
                SupremeTimer.QueryPerformanceCounter(ref endTime);
                SupremeTimer.QueryPerformanceFrequency(ref lTimerFrequency);

                double Ellapsed = 1000000 * (endTime - startTime) / (double)(lTimerFrequency);

                Func1 = new Task(() => {
                    IsScan[port] = true;
                    while (IsScan[port])
                    {
                        STATIC.fVision.m__G.oCam[port].GrabDWait(framCnt[port]);
                        for (int j = ch; j < ch + ChannelCnt; j++)
                        {
                            foreach (var Cal in CalList[j])
                                if (Cal.Name == name)
                                {
                                    SupremeTimer.QueryPerformanceCounter(ref endTime);
                                    SupremeTimer.QueryPerformanceFrequency(ref lTimerFrequency);
                                    Ellapsed = 1000 * (endTime - startTime) / (double)(lTimerFrequency); //  msec

                                    Cal.Time.Add(Ellapsed);
                                    Cal.StrokeX.Add(0);
                                    Cal.StrokeY.Add(0);
                                    Cal.StrokeZ.Add(0);
                                    Cal.StrokeY1.Add(0);
                                    Cal.StrokeY2.Add(0);
                                    Cal.TiltX.Add(0);
                                    Cal.TiltY.Add(0);
                                    Cal.TiltZ.Add(0);

                                }
                        }
                        for (int j = ch; j < ch + ChannelCnt; j++)
                        {
                            AddLog(j, string.Format("framCnt: {0}, Hall : {1} Time : {2:F2}", framCnt[port], DrvIC.ReadAFHall(j), Ellapsed));
                        }
                        framCnt[port]++;
                    }
                });
                Func1.Start();

                Func2 = new Task(() => {
                    foreach (var Cal in CalList[port])
                        if (Cal.Name == name)
                        {
                            for (int j = ch; j < ch + ChannelCnt; j++)
                            {
                                if (Cal.Name == name)
                                {
                                    if (name.Contains("2"))
                                    {
                                        DrvIC.AFMove(j, Condition.iAFStandbyCode2);
                                        AddLog(j, string.Format("AFMove StandbyCode : {0}", Condition.iAFStandbyCode2));
                                    }
                                    else
                                    {
                                        DrvIC.AFMove(j, Condition.iAFStandbyCode);
                                        AddLog(j, string.Format("AFMove StandbyCode : {0}", Condition.iAFStandbyCode));
                                    }
                                }
                            }

                            Wait(100);
                            //Thread.Sleep(100);
                            for (int j = ch; j < ch + ChannelCnt; j++)
                            {
                                if (Cal.Name == name)
                                {
                                    SupremeTimer.QueryPerformanceCounter(ref startTime);
                                    DrvIC.AFMove(j, TagerCode);
                                    AddLog(j, string.Format("AFMove TagerCode: {0}", TagerCode));
                                }
                            }

                            Wait(100);
                            //Thread.Sleep(100);
                        }
                    IsScan[port] = false;
                });
                Func2.Start();

                Task.WaitAll(Func1, Func2);
                
                sw.Stop();

                // FrmRate 표시 === 
                double frameRate = framCnt[port] / (double)sw.ElapsedMilliseconds * 1000;
                for (int j = ch; j < ch + ChannelCnt; j++)
                {
                    AddLog(j, string.Format("FrmRate == {0:F2} frame/sec", frameRate));
                }
                STATIC.fVision.m__G.oCam[port].CommonToReplayBuf(name, framCnt[port]);

                //for (int j = ch; j < ch + ChannelCnt; j++)
                //{
                //    DrvIC.OISOn(j, "AF", false);
                //    DrvIC.OISOn(j, "X", false);
                //    DrvIC.OISOn(j, "Y", false);
                //}
            }
            catch (Exception ex)
            {
                AddLog(0, ex.ToString());
            }
        }
        private void Process_CalcTimeTest(int port, string name, int InspCnt, int totalRetryCnt)
        {

            try
            {
                int ch = port * 2;

                for (int j = ch; j < ch + ChannelCnt; j++)
                {

                    AddLog(j, string.Format("{0} Driving Data>>", name));
                }
                List<FindResult> result = new List<FindResult>();

                for (int i = 0; i < framCnt[port]; i++)
                {
                    result.Add(new FindResult());
                    result[i] = STATIC.fVision.MeasureTxTyTz(i, name, true, false);/*.SaveImage(m__G,i,);*/
                }

                for (int j = ch; j < ch + ChannelCnt; j++)
                {
                    if (!m_ChannelOn[j]) continue;
                    foreach (var Cal in CalList[j])
                        if (Cal.Name == name)
                        {
                            double centerX = 0;
                            double centerY = 0;
                            double centerY1 = 0;
                            double centerY2 = 0;
                            double centerZ = 0;
                            double centertX = 0;
                            double centertY = 0;
                            double centertZ = 0;

                            centerX = result[2].cx[j];
                            centerY = result[2].cy[j];
                            centerZ = result[2].cz[j];
                            centertX = result[2].tx[j];
                            centertY = result[2].ty[j];
                            centertZ = result[2].tz[j];
                            centerY1 = result[2].cy1[j];
                            centerY2 = result[2].cy2[j];


                            for (int i = 0; i < framCnt[port]; i++)
                            {
                                Cal.StrokeX[i] = result[i].cx[j] - centerX;
                                Cal.StrokeX[i] = result[i].cy[j] - centerY;
                                Cal.StrokeZ[i] = result[i].cz[j] - centerZ;
                                Cal.StrokeY1[i] = result[i].cy1[j] - centerY1;
                                Cal.StrokeY2[i] = result[i].cy2[j] - centerY2;
                                Cal.TiltX[i] = result[i].tx[j] - centertX;
                                Cal.TiltY[i] = result[i].ty[j] - centertY;
                                Cal.TiltZ[i] = result[i].tz[j] - centertZ;
                            }
                        }
                }
                List<double> Time = new List<double>();
                List<double> Stroke = new List<double>();
                bool isStart = false;
                bool isFirstStart = false;
                double RefTime = 0;
                double RefStroke = 0;
                //int currentIndex = 0, FindIndex = 0, fillIndex = 0, fillCount = 0;
                //double scale = 1;
                foreach (var Cal in CalList[ch])
                {
                    if (Cal.Name == name)
                    {
                        switch (name)
                        {
                            case "AF Settling":
                            case "AF Settling2":
                                for (int i = 0; i < framCnt[port]; i++)
                                {
                                    if (i > 10 && Cal.Time[i] < 1) isStart = true;
                                    if (isStart)
                                    {
                                        if (!isFirstStart)
                                        {
                                            RefTime = Cal.Time[i];
                                            RefStroke = Cal.StrokeZ[i];
                                            isFirstStart = true;
                                        }
                                        Time.Add(Cal.Time[i]);
                                        Stroke.Add(Cal.StrokeZ[i]);
                                    }
                                }
                                break;
                        }
                    }
                }

                for (int i = 0; i < Time.Count; i++)
                {
                    Time[i] = Time[i] - RefTime;
                    Stroke[i] = Stroke[i] - RefStroke;
                    AddLog(ch, string.Format($"Time {Time[i]:F3}, Stroke {Stroke[i]:F3}"));
                }

                double TargetStable = GetStabilizedPosition(Stroke);
                AddLog(ch, string.Format($"Target Stable Stroke {TargetStable:F3}"));

                double getTime = GetSettlingTime(Time, Stroke, TargetStable, Condition.iAFSettlingCriteria);
                if (name.Contains("2"))
                {
                    getTime = GetSettlingTime(Time, Stroke, TargetStable, Condition.iAFSettlingCriteria2);
                }

                AddLog(ch, string.Format($"Get Stable Time {getTime:F3}"));

                if (name.Contains("2"))
                {
                    PassFails[ch].Results[(int)SpecItem.AF_SettillingTime2].Val = getTime;
                    ShowDataResults(ch, (int)SpecItem.AF_SettillingTime2, (int)SpecItem.AF_SettillingTime2, InspType.Normal, new double[] { });
                }
                else
                {
                    PassFails[ch].Results[(int)SpecItem.AF_SettillingTime].Val = getTime;
                    ShowDataResults(ch, (int)SpecItem.AF_SettillingTime, (int)SpecItem.AF_SettillingTime, InspType.Normal, new double[] { });
                }


                if (Option.SaveRawData)
                {
                    string str = Convert.ToString(yield.LastSampleNum + 1);
                    string dateDir = STATIC.CreateDateDir();
                    dateDir += "DrivingData\\";
                    if (!Directory.Exists(dateDir))
                        Directory.CreateDirectory(dateDir);

                    DateTime dt = DateTime.Now;
                    string timeDir = $"{STATIC.LogDate.Hour}h{STATIC.LogDate.Minute}m{STATIC.LogDate.Second}s";

                    for (int j = ch; j < ch + ChannelCnt; j++)
                    {
                        foreach (var Cal in CalList[j])
                            if (Cal.Name == name)
                            {
                                List<string> arry = new List<string>();

                                string path = "";
                                switch (name)
                                {
                                    case "AF Settling":
                                    case "AF Settling2":
                                        path = string.Format(dateDir + "{0}_{1}_{2}.csv", name, m_StrIndex[j], timeDir);
                                        arry.Add("i,AF Time,Z");

                                        for (int i = 0; i < Time.Count; i++)
                                        {
                                            string data = string.Format("{0},{1:0.000},{2:0.000}", i, Time[i], Stroke[i]);
                                            arry.Add(data);

                                        }
                                        //AddLog(j, lstr);
                                        break;
                                }
                                if (path != "") STATIC.SetTextLine(path, arry);
                            }
                    }
                }



                // Check if all test items passed (no failures recorded)
                bool isPass = (PassFails[ch].FirstFailIndex == 0);

                // Check if this is the final retry attempt
                bool isLastTry = (InspCnt >= totalRetryCnt - 1);

                // Draw the chart if the test passed, or if it's the final attempt regardless of the result
                if (isPass || isLastTry)
                {
                    if (Option.settlingGraphVisible)
                    {
                        AddChart(ch, name, Time.ToList(), Stroke.ToList());

                    }
                }
                framCnt[port] = 0;

            }
            catch (Exception ex)
            {
                PassFails[0].Results[(int)SpecItem.AF_SettillingTime].Val = 11.999;
                ShowDataResults(0, (int)SpecItem.AF_SettillingTime, (int)SpecItem.AF_SettillingTime, InspType.Normal, new double[] { });
                framCnt[port] = 0;
                AddLog(0, ex.ToString());
            }

        }
        public double GetSettlingTime(List<double> timeList, List<double> sList, double target, double tolerance = 3.0)
        {
            // 1. 데이터 개수 일치 확인 및 예외 처리
            if (timeList == null || sList == null || timeList.Count == 0 || timeList.Count != sList.Count)
            {
                return -1.0; // 에러 또는 데이터 없음
            }

            double lowerBound = target - tolerance;
            double upperBound = target + tolerance;

            // 2. 맨 끝(최종 시간)부터 역순으로 스캔
            for (int i = sList.Count - 1; i >= 0; i--)
            {
                double currentZ = sList[i];

                // 3. 현재 값이 오차 범위를 '벗어난' 지점 발견
                if (currentZ < lowerBound || currentZ > upperBound)
                {
                    // 만약 맨 마지막 데이터조차 오차 범위를 벗어나 있다면 "안정화 실패(Never Settled)"
                    if (i == sList.Count - 1)
                    {
                        return -1.0;
                    }

                    // 4. 벗어난 지점의 '바로 다음 지점(i + 1)'이 오차 범위 내로 완전히 진입한 시점
                    return timeList[i + 1];
                }
            }

            // 5. 처음(0번 인덱스)부터 끝까지 단 한 번도 범위를 벗어난 적이 없는 경우 (시작부터 안정화됨)
            return timeList[0];
        }
        public double GetStabilizedPosition(List<double> zList, int tailCount = 10)
        {
            if (zList == null || zList.Count == 0) return 0.0;

            // 데이터가 tailCount보다 적으면 전체의 평균을 냄
            int count = Math.Min(zList.Count, tailCount);
            double sum = 0.0;

            for (int i = zList.Count - count; i < zList.Count; i++)
            {
                sum += zList[i];
            }

            return sum / count;
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
                    if (errMsg[j] == "I2C Fail" || errMsg[j] == "Barcode Check" || errMsg[j] == "Socket Empty\r\nVision Check") { yield.TotlaTested--; continue; }

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

                    for (int i = (int)SpecItem.AF_NonEPAStroke; i < (int)SpecItem.Length; i++)
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
                                if (i == (int)SpecItem.AF_Current)
                                {
                                    log += $"{AFCurrentMinMax[1].ToString("F3")},";
                                    log += $"{AFCurrentMinMax[0].ToString("F3")},";
                                }
                                else if (i == (int)SpecItem.OISX_Current)
                                {
                                    log += $"{OISXCurrentMinMax[1].ToString("F3")},";
                                    log += $"{OISXCurrentMinMax[0].ToString("F3")},";
                                }

                                else if (i == (int)SpecItem.OISY_Current)
                                {
                                    log += $"{OISYCurrentMinMax[1].ToString("F3")},";
                                    log += $"{OISYCurrentMinMax[0].ToString("F3")},";

                                }
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
        private void Act_ScanCode(int port, string testItem, int InspCnt)
        {
            MakeWaveform(testItem);
            LEDs_All_On(port, true);
            Process_ScanCodeTest(port, testItem, InspCnt);
            LEDs_All_On(port, false);
            Process_CalcCodeTest(port, testItem, InspCnt);

        }
        bool IsScanError = false;
        private void Act_ScanTimeCode(int port, string testItem, int InspCnt)
        {
            int ch = port * 2;

            //int maxRetryCount = 10; // 최대 반복 횟수
            //int currentRetry = 0;   // 현재 시도 횟수

            //// bPass가 false(불합격)이고, 시도 횟수가 10번 미만인 동안 계속 반복
            //PassFails[ch].Results[(int)SpecItem.AF_SettillingTime].bPass = false;
            //while (!PassFails[ch].Results[(int)SpecItem.AF_SettillingTime].bPass && currentRetry < maxRetryCount)
            //{
            //    currentRetry++;

            //    AddLog(ch, $"AF Settling Time Cycle ({currentRetry}/{maxRetryCount})");
            int retryIndex = Rcp.RetryCnt.RetryOption.FindIndex(x => x.InspName == "AF Settling");
            int totalRetryCnt = 1;
            if (retryIndex >= 0)
            {
                totalRetryCnt = Rcp.RetryCnt.RetryOption[retryIndex].Count + 1;
            }

            AddLog(ch, $"AF Settling Time Cycle ({InspCnt}/{totalRetryCnt - 1})");

            LEDs_All_On(port, true);
            Process_ScanTimeTest(port, testItem);
            if (IsScanError) { IsScanError = false; return; }
            LEDs_All_On(port, false);
            Process_CalcTimeTest(port, testItem, InspCnt, totalRetryCnt);

            //    Thread.Sleep(100);
            //}
        }
        private void Act_ScanTimeCode2(int port, string testItem, int InspCnt)
        {
            int ch = port * 2;

            //int maxRetryCount = 10; // 최대 반복 횟수
            //int currentRetry = 0;   // 현재 시도 횟수

            // bPass가 false(불합격)이고, 시도 횟수가 10번 미만인 동안 계속 반복
            //PassFails[ch].Results[(int)SpecItem.AF_SettillingTime2].bPass = false;
            //while (!PassFails[ch].Results[(int)SpecItem.AF_SettillingTime2].bPass && currentRetry < maxRetryCount)
            //{
            //    currentRetry++;
            int retryIndex = Rcp.RetryCnt.RetryOption.FindIndex(x => x.InspName == "AF Settling2");
            int totalRetryCnt = 1;
            if (retryIndex >= 0)
            {
                totalRetryCnt = Rcp.RetryCnt.RetryOption[retryIndex].Count + 1;
            }
            AddLog(ch, $"AF Settling2 Time Cycle ({retryIndex}/{totalRetryCnt})");

            LEDs_All_On(port, true);
            Process_ScanTimeTest(port, testItem);
            if (IsScanError) { IsScanError = false; return; }
            LEDs_All_On(port, false);
            Process_CalcTimeTest(port, testItem, InspCnt, totalRetryCnt);


            //    Thread.Sleep(100);
            //}
        }
        private void Act_ScanTimeCode3(int port, string name, int InspCnt)
        {
            int ch = port * 2;

            LEDs_All_On(port, true);

            List<int> stanbyUm = new List<int>();
            List<int> targetUm = new List<int>();
            stanbyUm.Add(10);
            targetUm.Add(2);
            stanbyUm.Add(50);
            targetUm.Add(2);
            stanbyUm.Add(100);
            targetUm.Add(2);
            stanbyUm.Add(150);
            targetUm.Add(2);
            stanbyUm.Add(200);
            targetUm.Add(2);
            stanbyUm.Add(250);
            targetUm.Add(2);
            stanbyUm.Add(300);
            targetUm.Add(5);
            stanbyUm.Add(350);
            targetUm.Add(5);

            DrvIC.AFOnOff(ch, true);
            DrvIC.OISOn(ch, "X", false);
            DrvIC.OISOn(ch, "Y", false);
            Thread.Sleep(100);

            for (int k = 0; k < stanbyUm.Count; k++)
            {
                try
                {
                    for (int j = ch; j < ch + ChannelCnt; j++)
                    {
                        foreach (var Cal in CalList[j])
                            if (Cal.Name == name)
                            {
                                Cal.Clear();
                            }
                    }
                    framCnt[port] = 0;

                    double sensAf = PassFails[ch].Results[(int)SpecItem.AF_Sensitivity].Val;
                    if (sensAf > 9999)
                    {
                        AddLog(ch, string.Format($"AF Scan Not Drived =="));
                        IsScanError = true;
                        return;
                    }
                    int StanByCode = StroketoCode(stanbyUm[k], sensAf);
                    AddLog(ch, string.Format($"Inf Stroke {stanbyUm[k]}um, StandbyCode {StanByCode}, Sensitivity {sensAf:F4}" ));

                    for (int j = ch; j < ch + ChannelCnt; j++)
                    {
                        foreach (var Cal in CalList[j])
                            if (Cal.Name == name)
                            {
                                DrvIC.AFMove(j, StanByCode);
                            }
                    }
                    Thread.Sleep(200);

                    int TagerCode = StanByCode + StroketoCode(targetUm[k], sensAf);
                    AddLog(ch, string.Format($"Target Stroke {targetUm[k]} um , TagerCode {TagerCode}"));

                    Stopwatch sw = new Stopwatch();
                    sw.Reset(); sw.Start();
                    //Time Grab ===============

                    Task Func1 = null, Func2 = null;
                    long startTime = 0;
                    long endTime = 0;
                    long lTimerFrequency = 0;
                    SupremeTimer.QueryPerformanceCounter(ref startTime);
                    SupremeTimer.QueryPerformanceCounter(ref endTime);
                    SupremeTimer.QueryPerformanceFrequency(ref lTimerFrequency);

                    double Ellapsed = 1000000 * (endTime - startTime) / (double)(lTimerFrequency);



                    Func1 = new Task(() => {
                        IsScan[port] = true;
                        while (IsScan[port])
                        {
                            STATIC.fVision.m__G.oCam[port].GrabDWait(framCnt[port]);
                            for (int j = ch; j < ch + ChannelCnt; j++)
                            {
                                foreach (var Cal in CalList[j])
                                    if (Cal.Name == name)
                                    {
                                        SupremeTimer.QueryPerformanceCounter(ref endTime);
                                        SupremeTimer.QueryPerformanceFrequency(ref lTimerFrequency);
                                        Ellapsed = 1000 * (endTime - startTime) / (double)(lTimerFrequency); //  msec

                                        Cal.Time.Add(Ellapsed);
                                        Cal.StrokeX.Add(0);
                                        Cal.StrokeY.Add(0);
                                        Cal.StrokeZ.Add(0);
                                        Cal.StrokeY1.Add(0);
                                        Cal.StrokeY2.Add(0);
                                        Cal.TiltX.Add(0);
                                        Cal.TiltY.Add(0);
                                        Cal.TiltZ.Add(0);

                                    }
                            }
                            //for (int j = ch; j < ch + ChannelCnt; j++)
                            //{
                            //    AddLog(j, string.Format("framCnt: {0}, Hall : {1} Time : {2:F2}", framCnt[port], DrvIC.ReadAFHall(j), Ellapsed));
                            //}
                            framCnt[port]++;
                        }
                    });
                    Func1.Start();

                    Func2 = new Task(() => {
                        foreach (var Cal in CalList[port])
                            if (Cal.Name == name)
                            {
                                for (int j = ch; j < ch + ChannelCnt; j++)
                                {
                                    if (Cal.Name == name)
                                    {
                                        DrvIC.AFMove(j, StanByCode);
                                        AddLog(j, string.Format("AFMove StandbyCode : {0}", StanByCode));
                                    }
                                }

                                Wait(100);
                                //Thread.Sleep(100);
                                for (int j = ch; j < ch + ChannelCnt; j++)
                                {
                                    if (Cal.Name == name)
                                    {
                                        SupremeTimer.QueryPerformanceCounter(ref startTime);
                                        DrvIC.AFMove(j, TagerCode);
                                        AddLog(j, string.Format("AFMove TagerCode: {0}", TagerCode));
                                    }
                                }

                                Wait(100);
                                //Thread.Sleep(100);
                            }
                        IsScan[port] = false;
                    });
                    Func2.Start();

                    Task.WaitAll(Func1, Func2);

                    sw.Stop();

                    // FrmRate 표시 === 
                    double frameRate = framCnt[port] / (double)sw.ElapsedMilliseconds * 1000;
                    for (int j = ch; j < ch + ChannelCnt; j++)
                    {
                        AddLog(j, string.Format("FrmRate == {0:F2} frame/sec", frameRate));
                    }
                }
                catch (Exception ex)
                {
                    AddLog(0, ex.ToString());
                }

                try
                {
                    for (int j = ch; j < ch + ChannelCnt; j++)
                    {

                        AddLog(j, string.Format("{0} Driving Data>>", name));
                    }
                    List<FindResult> result = new List<FindResult>();

                    for (int i = 0; i < framCnt[port]; i++)
                    {
                        result.Add(new FindResult());
                        result[i] = STATIC.fVision.MeasureTxTyTz(i, name, true, false);/*.SaveImage(m__G,i,);*/
                    }

                    for (int j = ch; j < ch + ChannelCnt; j++)
                    {
                        if (!m_ChannelOn[j]) continue;
                        foreach (var Cal in CalList[j])
                            if (Cal.Name == name)
                            {
                                double centerX = 0;
                                double centerY = 0;
                                double centerY1 = 0;
                                double centerY2 = 0;
                                double centerZ = 0;
                                double centertX = 0;
                                double centertY = 0;
                                double centertZ = 0;

                                centerX = result[2].cx[j];
                                centerY = result[2].cy[j];
                                centerZ = result[2].cz[j];
                                centertX = result[2].tx[j];
                                centertY = result[2].ty[j];
                                centertZ = result[2].tz[j];
                                centerY1 = result[2].cy1[j];
                                centerY2 = result[2].cy2[j];


                                for (int i = 0; i < framCnt[port]; i++)
                                {
                                    Cal.StrokeX[i] = result[i].cx[j] - centerX;
                                    Cal.StrokeX[i] = result[i].cy[j] - centerY;
                                    Cal.StrokeZ[i] = result[i].cz[j] - centerZ;
                                    Cal.StrokeY1[i] = result[i].cy1[j] - centerY1;
                                    Cal.StrokeY2[i] = result[i].cy2[j] - centerY2;
                                    Cal.TiltX[i] = result[i].tx[j] - centertX;
                                    Cal.TiltY[i] = result[i].ty[j] - centertY;
                                    Cal.TiltZ[i] = result[i].tz[j] - centertZ;
                                }
                            }
                    }
                    List<double> Time = new List<double>();
                    List<double> Stroke = new List<double>();
                    bool isStart = false;
                    bool isFirstStart = false;
                    double RefTime = 0;
                    double RefStroke = 0;
                    //int currentIndex = 0, FindIndex = 0, fillIndex = 0, fillCount = 0;
                    //double scale = 1;
                    foreach (var Cal in CalList[ch])
                    {
                        if (Cal.Name == name)
                        {
                            for (int i = 0; i < framCnt[port]; i++)
                            {
                                if (i > 10 && Cal.Time[i] < 1) isStart = true;
                                if (isStart)
                                {
                                    if (!isFirstStart)
                                    {
                                        RefTime = Cal.Time[i];
                                        RefStroke = Cal.StrokeZ[i];
                                        isFirstStart = true;
                                    }
                                    Time.Add(Cal.Time[i]);
                                    Stroke.Add(Cal.StrokeZ[i]);
                                }
                            }
                        }
                    }

                    //for (int i = 0; i < Time.Count; i++)
                    //{
                    //    Time[i] = Time[i] - RefTime;
                    //    Stroke[i] = Stroke[i] - RefStroke;
                    //    AddLog(ch, string.Format($"Time {Time[i]:F3}, Stroke {Stroke[i]:F3}"));
                    //}

                    double TargetStable = GetStabilizedPosition(Stroke);
                    AddLog(ch, string.Format($"Target Stable Stroke {TargetStable:F3}"));

                    double getTime = GetSettlingTime(Time, Stroke, TargetStable, Condition.iAFSettlingCriteria);

                    AddLog(ch, string.Format($"Get Stable Time {getTime:F3}"));

                    if (Option.SaveRawData)
                    {
                        string str = Convert.ToString(yield.LastSampleNum + 1);
                        string dateDir = STATIC.CreateDateDir();
                        dateDir += "DrivingData\\";
                        if (!Directory.Exists(dateDir))
                            Directory.CreateDirectory(dateDir);

                        DateTime dt = DateTime.Now;
                        string timeDir = $"{STATIC.LogDate.Hour}h{STATIC.LogDate.Minute}m{STATIC.LogDate.Second}s";

                        for (int j = ch; j < ch + ChannelCnt; j++)
                        {
                            foreach (var Cal in CalList[j])
                                if (Cal.Name == name)
                                {
                                    List<string> arry = new List<string>();

                                    string path = "";
                                    path = string.Format(dateDir + "{0}_{1}_{2}_{3}.csv", name, k+1, m_StrIndex[j], timeDir);
                                    arry.Add("i,AF Time,Z");

                                    for (int i = 0; i < Time.Count; i++)
                                    {
                                        string data = string.Format("{0},{1:0.000},{2:0.000}", i, Time[i], Stroke[i]);
                                        arry.Add(data);

                                    }
                                    if (path != "") STATIC.SetTextLine(path, arry);
                                }
                        }
                    }
                    framCnt[port] = 0;

                }
                catch (Exception ex)
                {
                    framCnt[port] = 0;
                    AddLog(0, ex.ToString());
                }

                AddLog(ch, string.Format($"================ Cycle {k + 1} Complete ===================="));
            }


            LEDs_All_On(port, false);
        }

        private void PowerSequence(int port)
        {
            Dln.PowerOnOff(0, false);
            Process.Wait(300);
            Dln.PowerOnOff(0, true);
            Process.Wait(200);
            DrvIC.ICReset(0);
        }
        public FindResult Measure()
        {
            FindResult res = new FindResult();

            STATIC.fVision.m__G.oCam[0].Grab(0);
            res = STATIC.fVision.MeasureTxTyTz(0);
            return res;
        }

        public FindResult Measure1()
        {
            FindResult res = new FindResult();

            STATIC.fVision.m__G.oCam[0].Grab(1);
            res = STATIC.fVision.MeasureTxTyTz(1);
            return res;
        }
        #endregion
    }
}
