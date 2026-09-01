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
        void AddSequence()
        {
            ItemList.Add(new ActItems() { Name = "FW Version Read", Func = Act_FW_Version_Read, IsMulti = true });
            ItemList.Add(new ActItems() { Name = "FW Download", Func = Act_FW_Download, IsMulti = true });
            ItemList.Add(new ActItems() { Name = "OIS HallCalibration", Func = Act_HallCalibration, IsMulti = true });
            ItemList.Add(new ActItems() { Name = "OIS Move Target", Func = Act_MoveTarget, IsMulti = true });
            ItemList.Add(new ActItems() { Name = "OIS Hall Deviation", Func = Act_HallDeviation, IsMulti = true });
        }

        void Act_FW_Version_Read(int ch, string testItem, int InspCnt)
        {
            uint fwver = 0;
            fwver = Dln.Read4Byte(ch, 0x5A, 0x1008, 2);
            AddLog(ch, string.Format("Read FW ver : {0} ", fwver));
            if (fwver == Condition.iFWversionCheck)
            {
                PassFails[0].Results[(int)SpecItem.FW_VERSION_READ].Val = 0;
                ShowDataResults(ch, (int)SpecItem.FW_VERSION_READ, (int)SpecItem.FW_VERSION_READ, InspType.OKNG, new double[] { });
                m_ChannelOn[ch] = true;
            }
            else
            {
                PassFails[0].Results[(int)SpecItem.FW_VERSION_READ].Val = 10;
                ShowDataResults(ch, (int)SpecItem.FW_VERSION_READ, (int)SpecItem.FW_VERSION_READ, InspType.OKNG, new double[] { });
                m_ChannelOn[ch] = false;
            }
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
                    FWUpdateResult = true;
                    //if (fwver == Condition.iFWversionCheck)
                    //{
                    //    AddLog(ch, string.Format("[PASS] Read FW ver : {0} ", fwver));
                    //    FWUpdateResult = true;
                    //}
                    //else
                    //{
                    //    fwver = Dln.Read4Byte(ch, 0x5A, 0x1008, 2);
                    //    AddLog(ch, string.Format("Retry FW Check"));

                    //    if (fwver != Condition.iFWversionCheck)
                    //    {
                    //        AddLog(ch, string.Format("[FAIL] Read FW ver : {0} ", fwver));
                    //        FWUpdateResult = false;
                    //    }
                    //    else
                    //    {
                    //        AddLog(ch, string.Format("[PASS] Read FW ver : {0} ", fwver));
                    //        FWUpdateResult = true;
                    //    }
                    //}

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
                m_ChannelOn[ch] = true;
            }
            else
            {
                PassFails[0].Results[(int)SpecItem.FW_DOWNLOAD].Val = 10;
                ShowDataResults(ch, (int)SpecItem.FW_DOWNLOAD, (int)SpecItem.FW_DOWNLOAD, InspType.OKNG, new double[] { });
                m_ChannelOn[ch] = false;
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
                FwCtrl |= 0x40;
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
        void Act_HallCalibration(int ch, string testItem, int InspCnt)
        {
            bool result = false;

            AddLog(ch, "Hall Calibration Start");

            try
            {
                if (Option.ResetEPA)
                    OISEPA_Reset(ch);

                result = HallCalibration(ch, true);

                ReadNVMHall(ch);

                if (result)
                    result = Store_OIS_CalData(ch);

                if (result)
                {
                    AddLog(ch, "Hall Calibration PASS");

                    PassFails[0].Results[(int)SpecItem.HALL_CALIBRATION].Val = 0;

                    ShowDataResults(
                        ch,
                        (int)SpecItem.HALL_CALIBRATION,
                        (int)SpecItem.HALL_CALIBRATION,
                        InspType.OKNG,
                        new double[] { });
                }
                else
                {
                    AddLog(ch, "Hall Calibration FAIL");

                    PassFails[0].Results[(int)SpecItem.HALL_CALIBRATION].Val = 10;

                    ShowDataResults(
                        ch,
                        (int)SpecItem.HALL_CALIBRATION,
                        (int)SpecItem.HALL_CALIBRATION,
                        InspType.OKNG,
                        new double[] { });
                }
            }
            catch (Exception ex)
            {
                AddLog(ch, ex.ToString());

                PassFails[0].Results[(int)SpecItem.HALL_CALIBRATION].Val = 10;

                ShowDataResults(
                    ch,
                    (int)SpecItem.HALL_CALIBRATION,
                    (int)SpecItem.HALL_CALIBRATION,
                    InspType.OKNG,
                    new double[] { });
            }
        }
        bool HallCalibration(int ch, bool isMOD)
        {
            bool isPass = false;

            try
            {
                if (isMOD)
                {
                    if (Condition.XOffsetInit != -1)
                    {
                        Dln.WriteByte(ch, 0x5A, 0x0922, 2, (byte)Condition.XOffsetInit);
                        AddLog(ch, $"Write X Offset Init : {Condition.XOffsetInit}");
                    }

                    if (Condition.YOffsetInit != -1)
                    {
                        Dln.WriteByte(ch, 0x5A, 0x0923, 2, (byte)Condition.YOffsetInit);
                        AddLog(ch, $"Write Y Offset Init : {Condition.YOffsetInit}");
                    }

                    if (Condition.XOffsetTH != -1)
                    {
                        Dln.WriteByte(ch, 0x5A, 0x0924, 2, (byte)Condition.XOffsetTH);
                        AddLog(ch, $"Write X Offset TH : {Condition.XOffsetTH}");
                    }

                    if (Condition.YOffsetTH != -1)
                    {
                        Dln.WriteByte(ch, 0x5A, 0x0925, 2, (byte)Condition.YOffsetTH);
                        AddLog(ch, $"Write Y Offset TH : {Condition.YOffsetTH}");
                    }
                }

                for (int i = 0; i < Condition.HallCalCount; i++)
                {
                    AddLog(ch, $"Hall Calibration Count : {i + 1}");

                    byte rdata = Dln.ReadByte(ch, 0x5A, 0x0922, 2);
                    AddLog(ch, $"Read X Offset Init : {rdata}");

                    rdata = Dln.ReadByte(ch, 0x5A, 0x0923, 2);
                    AddLog(ch, $"Read Y Offset Init : {rdata}");

                    rdata = Dln.ReadByte(ch, 0x5A, 0x0924, 2);
                    AddLog(ch, $"Read X Offset TH : {rdata}");

                    rdata = Dln.ReadByte(ch, 0x5A, 0x0925, 2);
                    AddLog(ch, $"Read Y Offset TH : {rdata}");

                    AddLog(ch, "OIS status check");

                    byte rcvData = Dln.ReadByte(ch, 0x5A, 0x0001, 2);

                    if (rcvData != 0x01)
                        Dln.WriteByte(ch, 0x5A, 0x0000, 2, 0x00);

                    AddLog(ch, "Hall calibration start");

                    Dln.WriteByte(ch, 0x5A, 0x0900, 2, 0x01);

                    AddLog(ch, "Check Hall Calibration Sequence");

                    Stopwatch st = new Stopwatch();
                    st.Start();

                    int repeatCnt = 1000;

                    do
                    {
                        if (repeatCnt <= 0)
                        {
                            AddLog(ch, "HallCal Abnormal Termination Error.");
                            break;
                        }

                        Thread.Sleep(50);

                        rcvData = Dln.ReadByte(ch, 0x5A, 0x0900, 2);
                        repeatCnt--;

                    } while ((rcvData & 0x01) == 0x01 && st.ElapsedMilliseconds < 10000);

                    st.Stop();

                    if ((rcvData & 0x01) == 0x01)
                    {
                        AddLog(ch, "Hall Calibration Timeout.");
                        continue;
                    }

                    AddLog(ch, "Result check");

                    ushort errData = Dln.Read2Byte(ch, 0x5A, 0x0004, 2);

                    if ((errData & (0x0004 | 0x0008)) != 0x0000)
                    {
                        AddLog(ch, $"Hall Calibration Error : 0x{errData:X4}");
                        isPass = false;
                        continue;
                    }

                    AddLog(ch, "Hall Calibration Pass.");
                    isPass = true;
                    break;
                }

                return isPass;
            }
            catch (Exception ex)
            {
                AddLog(ch, ex.ToString());
                return false;
            }
        }
        void ReadNVMHall(int ch)
        {
            try
            {
                AddLog(ch, ">> OIS Hall status check");

                byte rcvData = Dln.ReadByte(ch, 0x5A, 0x0001, 2);

                if (rcvData != 0x01)
                    Dln.WriteByte(ch, 0x5A, 0x0000, 2, 0x00);

                // X
                int xOffset = Dln.ReadByte(ch, 0x5A, 0x0406, 2);
                int xBias = Dln.ReadByte(ch, 0x5A, 0x0404, 2);
                int xMin = Dln.Read2Byte(ch, 0x5A, 0x0408, 2);
                int xMax = Dln.Read2Byte(ch, 0x5A, 0x040C, 2);
                int xMid = 16000;

                int xNonEpaMin = Dln.Read2Byte(ch, 0x5A, 0x0448, 2);
                int xNonEpaMax = Dln.Read2Byte(ch, 0x5A, 0x044C, 2);
                int xNonEpaMid = Dln.Read2Byte(ch, 0x5A, 0x0450, 2);

                int xGain = Dln.ReadByte(ch, 0x5A, 0x0402, 2);

                // Y
                int yOffset = Dln.ReadByte(ch, 0x5A, 0x0407, 2);
                int yBias = Dln.ReadByte(ch, 0x5A, 0x0405, 2);
                int yMin = Dln.Read2Byte(ch, 0x5A, 0x040A, 2);
                int yMax = Dln.Read2Byte(ch, 0x5A, 0x040E, 2);
                int yMid = 16000;

                int yNonEpaMin = Dln.Read2Byte(ch, 0x5A, 0x044A, 2);
                int yNonEpaMax = Dln.Read2Byte(ch, 0x5A, 0x044E, 2);
                int yNonEpaMid = Dln.Read2Byte(ch, 0x5A, 0x0452, 2);

                int yGain = Dln.ReadByte(ch, 0x5A, 0x0403, 2);

                int xHallRange = xNonEpaMax - xNonEpaMin;
                int yHallRange = yNonEpaMax - yNonEpaMin;

                AddLog(ch, "XH_Offset = " + xOffset.ToString());
                AddLog(ch, "XH_Bias = " + xBias.ToString());
                AddLog(ch, "XHmin = " + xMin.ToString());
                AddLog(ch, "XHmax = " + xMax.ToString());
                AddLog(ch, "XHmid = " + xMid.ToString());
                AddLog(ch, "NONEPA_XHmin = " + xNonEpaMin.ToString());
                AddLog(ch, "NONEPA_XHmax = " + xNonEpaMax.ToString());
                AddLog(ch, "NONEPA_XHmid = " + xNonEpaMid.ToString());
                AddLog(ch, "XH_Gain = " + xGain.ToString());

                AddLog(ch, "YH_Offset = " + yOffset.ToString());
                AddLog(ch, "YH_Bias = " + yBias.ToString());
                AddLog(ch, "YHmin = " + yMin.ToString());
                AddLog(ch, "YHmax = " + yMax.ToString());
                AddLog(ch, "YHmid = " + yMid.ToString());
                AddLog(ch, "NONEPA_YHmin = " + yNonEpaMin.ToString());
                AddLog(ch, "NONEPA_YHmax = " + yNonEpaMax.ToString());
                AddLog(ch, "NONEPA_YHmid = " + yNonEpaMid.ToString());
                AddLog(ch, "YH_Gain = " + yGain.ToString());

                AddLog(ch, "X Hall Range = " + xHallRange.ToString());
                AddLog(ch, "Y Hall Range = " + yHallRange.ToString());

                PassFails[0].Results[(int)SpecItem.OISX_HallRange].Val = xHallRange;
                PassFails[0].Results[(int)SpecItem.OISY_HallRange].Val = yHallRange;

                ShowDataResults(ch,
                    (int)SpecItem.OISX_HallRange,
                    (int)SpecItem.OISX_HallRange,
                    InspType.Normal,
                    new double[] { });

                ShowDataResults(ch,
                    (int)SpecItem.OISY_HallRange,
                    (int)SpecItem.OISY_HallRange,
                    InspType.Normal,
                    new double[] { });
            }
            catch (Exception ex)
            {
                AddLog(ch, ex.ToString());
            }
        }
        bool Store_OIS_CalData(int ch)
        {
            try
            {
                if (!Option.HallCalDataUpdate)
                {
                    AddLog(ch, "Skip Update HallCal Data!!");
                    return true;
                }

                AddLog(ch, "Update HallCal Data");

                int repeatCnt = 200;

                byte status = Dln.ReadByte(ch, 0x5A, 0x0001, 2);

                if (status != 0x01)
                    Dln.WriteByte(ch, 0x5A, 0x0000, 2, 0x00);

                ushort updateData = 0x0002 | 0x0004 | 0x0008 | 0x0020 | 0x0010;

                Dln.Write2Byte(ch, 0x5A, 0x0300, 2, updateData);

                Thread.Sleep(100);

                do
                {
                    if (repeatCnt <= 0)
                    {
                        AddLog(ch, "CalData Store Abnormal Termination Error.");
                        return false;
                    }

                    Thread.Sleep(50);

                    status = Dln.ReadByte(ch, 0x5A, 0x0300, 2);

                    repeatCnt--;

                } while ((status & updateData) == updateData);

                ushort errData = Dln.Read2Byte(ch, 0x5A, 0x0004, 2);

                if ((errData & 0x0040) != 0x0000)
                {
                    AddLog(ch, "Different INFORWRITE data on flash.");
                    return false;
                }

                AddLog(ch, "Flash success.");

                return true;
            }
            catch (Exception ex)
            {
                AddLog(ch, ex.ToString());
                return false;
            }
        }
        bool WriteFloat(int ch, int memAddr, float value)
        {
            byte[] data = BitConverter.GetBytes(value);
            return Dln.WriteArray(ch, 0x5A, memAddr, data);
        }
        void OISEPA_Reset(int ch)
        {
            try
            {
                if (!Option.ResetEPA)
                    return;

                AddLog(ch, "EPA Reset Start");

                byte status = Dln.ReadByte(ch, 0x5A, 0x0001, 2);

                if (status != 0x01)
                    Dln.WriteByte(ch, 0x5A, 0x0000, 2, 0x00);

                WriteFloat(ch, 0x041C, 0.0f);
                WriteFloat(ch, 0x0424, 0.0f);
                WriteFloat(ch, 0x0420, 0.0f);
                WriteFloat(ch, 0x0428, 0.0f);

                Dln.WriteByte(ch, 0x5A, 0x0418, 2, 0x01);

                int repeatCnt = 200;

                while (repeatCnt > 0)
                {
                    Thread.Sleep(10);

                    byte data = Dln.ReadByte(ch, 0x5A, 0x0418, 2);

                    if (data == 0x00)
                        break;

                    repeatCnt--;
                }

                if (repeatCnt <= 0)
                {
                    AddLog(ch, "EPA Reset Timeout.");
                    return;
                }

                if (!Store_OIS_EPAData(ch))
                {
                    AddLog(ch, "EPA Data Store Fail.");
                    return;
                }

                AddLog(ch, "EPA Reset Complete.");
            }
            catch (Exception ex)
            {
                AddLog(ch, ex.ToString());
            }
        }
        bool Store_OIS_EPAData(int ch)
        {
            try
            {
                int repeatCnt = 200;

                byte status = Dln.ReadByte(ch, 0x5A, 0x0001, 2);

                if (status != 0x01)
                    Dln.WriteByte(ch, 0x5A, 0x0000, 2, 0x00);

                ushort updateData = 0x0010;

                if (!Dln.Write2Byte(ch, 0x5A, 0x0300, 2, updateData))
                {
                    AddLog(ch, "EPA Data Store Write Fail.");
                    return false;
                }

                Thread.Sleep(100);

                do
                {
                    if (repeatCnt <= 0)
                    {
                        AddLog(ch, "EPA Data Store Abnormal Termination Error.");
                        return false;
                    }

                    Thread.Sleep(50);

                    status = Dln.ReadByte(ch, 0x5A, 0x0300, 2);
                    repeatCnt--;

                } while ((status & updateData) == updateData);

                ushort errData = Dln.Read2Byte(ch, 0x5A, 0x0004, 2);

                if ((errData & 0x0040) != 0x0000)
                {
                    AddLog(ch, "Different INFORWRITE data on flash.");
                    return false;
                }

                AddLog(ch, "EPA Flash success.");

                return true;
            }
            catch (Exception ex)
            {
                AddLog(ch, ex.ToString());
                return false;
            }
        }
        void Act_MoveTarget(int ch, string testItem, int InspCnt)
        {
            AddLog(ch, "OIS Move Target Start");

            if (!OISModeFixed(ch))
            {
                AddLog(ch, "OIS Fixed Mode Set Fail");
                return;
            }

            if (!OISOnOff(ch, true))
            {
                AddLog(ch, "OIS Servo On Fail");
                return;
            }

            AddLog(ch, "OIS Servo On");

            if (!OISMove(ch, "X", Condition.XMoveTarget))
            {
                AddLog(ch, "OIS X Move Fail");
                return;
            }

            Thread.Sleep(Condition.MoveHallDelay);

            ushort xHall = OISHallRead(ch, "X");
            AddLog(ch, $"X Target : {Condition.XMoveTarget}, X Hall : {xHall}");

            if (!OISMove(ch, "Y", Condition.YMoveTarget))
            {
                AddLog(ch, "OIS Y Move Fail");
                return;
            }

            Thread.Sleep(Condition.MoveHallDelay);

            ushort yHall = OISHallRead(ch, "Y");
            AddLog(ch, $"Y Target : {Condition.YMoveTarget}, Y Hall : {yHall}");

            AddLog(ch, "OIS Move Target Complete");
        }
        public bool OISOnOff(int ch, bool isOn)
        {
            bool status;
            if (isOn)
            {
                Dln.WriteByte(ch, 0x5A, 0x0000, 2, 0x01);
                status = Dln.WriteByte(ch, 0x5A, 0x0B00, 2, 0x01);
                AddLog(ch, "OIS Servo On");
            }
            else
            {
                status = Dln.WriteByte(ch, 0x5A, 0x0000, 2, 0x00);
                AddLog(ch, "OIS Servo Off");
            }

            return status;
        }
        public bool OISMove(int ch, string axis, int target)
        {
            if(axis == "X")
                return Dln.Write2Byte(ch, 0x5A, 0x0014, 2, (ushort)target);
            else if(axis == "Y")
                return Dln.Write2Byte(ch, 0x5A, 0x0016, 2, (ushort)target);
            else return false;
        }
        public ushort OISHallRead(int ch, string axis)
        {
            if (axis == "X")
                return Dln.Read2Byte(ch, 0x5A, 0x0B10, 2);
            else if (axis == "Y")
                return Dln.Read2Byte(ch, 0x5A, 0x0B12, 2);
            else return ushort.MaxValue;
        }
        public bool OISModeFixed(int ch)
        {
            return Dln.WriteByte(ch, 0x5A, 0x0002, 2, 0x0B);
        }
        void Act_HallDeviation(int ch, string testItem, int InspCnt)
        {
            AddLog(ch, "Hall Deviation Start");

            bool result = HallDeviation(
                ch,
                Condition.HallDEV_InitialDelay,
                Condition.HallDEV_Interval,
                Condition.HallDEV_sampling,
                Condition.HallDEV_TargetX,
                Condition.HallDEV_TargetY);

            if (!result)
            {
                AddLog(ch, "Hall Deviation Fail");
                return;
            }

            AddLog(ch, "Hall Deviation Complete");
        }
        bool HallDeviation(int ch, double InitDelay, double Interval, double sampling, double TargetX, double TargetY)
        {
            try
            {
                // Fixed Mode
                if (!OISModeFixed(ch))
                {
                    AddLog(ch, "OIS Fixed Mode Set Fail");
                    return false;
                }

                // Servo On
                if (!OISOnOff(ch, true))
                {
                    AddLog(ch, "OIS Servo On Fail");
                    return false;
                }

                AddLog(ch, "OIS Servo On");

                // Target Move
                if (!OISMove(ch, "X", (int)TargetX))
                {
                    AddLog(ch, "OIS X Move Fail");
                    return false;
                }

                if (!OISMove(ch, "Y", (int)TargetY))
                {
                    AddLog(ch, "OIS Y Move Fail");
                    return false;
                }

                AddLog(ch, "InitDelay = " + InitDelay + " ms");
                AddLog(ch, "Interval = " + Interval + " ms");
                AddLog(ch, "Sampling = " + sampling + " spl");
                AddLog(ch, "TargetX = " + TargetX + " code");
                AddLog(ch, "TargetY = " + TargetY + " code");

                Thread.Sleep((int)InitDelay);

                double hallMinX = 99999;
                double hallMaxX = 0;
                double hallMinY = 99999;
                double hallMaxY = 0;

                double sumX = 0;
                double sumY = 0;

                double sumSqrX = 0;
                double sumSqrY = 0;

                int sampleCount = (int)sampling;

                for (int i = 0; i < sampleCount; i++)
                {
                    Thread.Sleep((int)Interval);

                    double hallX = OISHallRead(ch, "X");
                    double hallY = OISHallRead(ch, "Y");

                    sumX += hallX;
                    sumY += hallY;

                    sumSqrX += hallX * hallX;
                    sumSqrY += hallY * hallY;

                    if (hallMinX > hallX) hallMinX = hallX;
                    if (hallMaxX < hallX) hallMaxX = hallX;

                    if (hallMinY > hallY) hallMinY = hallY;
                    if (hallMaxY < hallY) hallMaxY = hallY;
                }

                double avgX = sumX / sampleCount;
                double avgY = sumY / sampleCount;

                double avgSqrX = sumSqrX / sampleCount;
                double avgSqrY = sumSqrY / sampleCount;

                double deviationX = Math.Sqrt(Math.Abs(avgSqrX - avgX * avgX));
                double deviationY = Math.Sqrt(Math.Abs(avgSqrY - avgY * avgY));

                AddLog(ch, "Hall Average X = " + avgX.ToString("F3"));
                AddLog(ch, "Hall Average Y = " + avgY.ToString("F3"));

                AddLog(ch, "Hall Min X = " + hallMinX.ToString("F0"));
                AddLog(ch, "Hall Max X = " + hallMaxX.ToString("F0"));
                AddLog(ch, "Hall Min Y = " + hallMinY.ToString("F0"));
                AddLog(ch, "Hall Max Y = " + hallMaxY.ToString("F0"));

                AddLog(ch, "Hall DEV X = " + deviationX.ToString("F3"));
                AddLog(ch, "Hall DEV Y = " + deviationY.ToString("F3"));

                PassFails[0].Results[(int)SpecItem.OISX_HALLDEV].Val = deviationX;
                PassFails[0].Results[(int)SpecItem.OISY_HALLDEV].Val = deviationY;

                ShowDataResults(
                    ch,
                    (int)SpecItem.OISX_HALLDEV,
                    (int)SpecItem.OISY_HALLDEV,
                    InspType.Normal,
                    new double[] { });

                AddLog(ch, "Hall Deviation Test Finish");

                return true;
            }
            catch (Exception ex)
            {
                AddLog(ch, ex.ToString());
                return false;
            }
        }

        public static void Wait(int ms)
        {
            //       Thread.Sleep(ms);
            ms = ms * 1000;
            Stopwatch startNew = Stopwatch.StartNew();

            long usDelayTick = (ms * Stopwatch.Frequency) / 1000000;

            while (startNew.ElapsedTicks < usDelayTick) ;
        }

    }
}
