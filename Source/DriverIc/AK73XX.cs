using Dln;
using FZ4P.Commons.Enums;
using FZ4P.Commons.Helper;
using FZ4P.DriverIc.Interfaces;
using FZ4P.DriverIc.MoveHall.Context;
using FZ4P.DriverIc.ReadHall.Context;
using FZ4P.DriverIc.SlaveID.Context;
using Modules.Helper;
using OpenCvSharp.Dnn;
using OpenCvSharp.Flann;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace FZ4P
{
    public class AK73XX : IAK73XX, IDriverIC_AF_Function,IDriverIC_OIS_Function, IDriverIC_FRA_Function
    {
        
        public Process Process { get { return STATIC.Process; } }
        public Condition Condition { get { return STATIC.Rcp.Condition; } }
        public IDlnInterface Dln { get { return STATIC.Dln; } }

        public readonly StrategySlaveIDContext SelectSlaveID = null;
        public string Name { get; set; }
   
        public int AFOriginAddr { get; set; }
        public int XOriginAddr { get; set; }
        public int Y1OriginAddr { get; set; }
        public int Y2OriginAddr { get; set; }
        public int AF_Addr { get; set; }
        public int XSlaveAddr { get; set; }
        public int Y1SlaveAddr { get; set; }
        public int Y2SlaveAddr { get; set; }
        public int FRA_Addr { get; set; }

        public int FRA_AFSlaveAddr { get; set; }
        public int FRA_XSlaveAddr { get; set; }
        public int FRA_Y1SlaveAddr { get; set; }
        public int FRA_Y2SlaveAddr { get; set; }
        public int AF_MID_CODE { get; private set; } = 0;
        public int AF_MIN_CODE { get; private set; } = 0;
        public int AF_MAX_CODE { get; private set; } = 0;

        public int OIS_MID_CODE { get; private set; } = 0;
        public int OIS_MIN_CODE { get; private set; } = 0;
        public int OIS_MAX_CODE { get; private set; } = 0;

        public int AF_ADC_Addr { get; set; }
        public int OIS_ADC_Addr { get; set; }

        public AK73XX()
        {
            SelectSlaveID = new StrategySlaveIDContext();

            Name = "AK73XX";
            var slaveID = SelectSlaveID.GetSlaveID(ActuatorType.Type1C87);

            // AFOriginAddr = 0x0C;
            AFOriginAddr = slaveID.AFOriginAddr;

            XOriginAddr = slaveID.XOriginAddr;
            Y1OriginAddr = slaveID.Y1OriginAddr;
            Y2OriginAddr = slaveID.Y2OriginAddr;

            AF_Addr = slaveID.AF_Addr;
            XSlaveAddr = slaveID.XSlaveAddr;
            Y1SlaveAddr = slaveID.Y1SlaveAddr;
            Y2SlaveAddr = slaveID.Y2SlaveAddr;
            FRA_Addr = slaveID.FRA_Addr;

            // FRA_AFSlaveAddr = 0x50;
            FRA_AFSlaveAddr = slaveID.FRA_AFSlaveAddr;

            FRA_XSlaveAddr = slaveID.FRA_XSlaveAddr;
            FRA_Y1SlaveAddr = slaveID.FRA_Y1SlaveAddr; 
            FRA_Y2SlaveAddr = slaveID.FRA_Y2SlaveAddr;

            AF_ADC_Addr = slaveID.AF_ADC_Addr;
            OIS_ADC_Addr = slaveID.OIS_ADC_Addr;

            AF_MID_CODE = slaveID.AF_MID_CODE;
            AF_MAX_CODE = slaveID.AF_MAX_CODE;
            AF_MIN_CODE = slaveID.AF_MIN_CODE;

            OIS_MIN_CODE = slaveID.OIS_MIN_CODE;
            OIS_MID_CODE = slaveID.OIS_MID_CODE;
            OIS_MAX_CODE = slaveID.OIS_MAX_CODE;
        }


        #region AF Function
        public bool ChangeSlaveAddr(int ch)
        {
            byte[] addr = { 0x18, 0x1E, 0xE8, 0x98 }; byte icAddr_temp = 0xFF;
            byte temp;
            byte[] i2c_2nd = new byte[2];

            byte rbuf = Dln.ReadByte(ch, AF_Addr, 0x03, 1);
            if (rbuf == 0x1C)
            {
                Process.AddLog(ch, $"IC Address check OK");
                //return true;
            }
            for (int i = 0; i < 4; i++)
            {
                rbuf = Dln.ReadByte(ch, addr[i] >> 1, 0x03, 1);
                if (rbuf == 0x1C)
                {
                    icAddr_temp = (byte)(addr[i] >> 1);
                    break;
                }
            }
            if (icAddr_temp != 0xFF)
            {
                Dln.WriteByte(ch, icAddr_temp, 0x02, 1, 0x40);
                Process.Wait(5);
                Dln.WriteByte(ch, icAddr_temp, 0xAE, 1, 0x3B);
                Dln.WriteByte(ch, icAddr_temp, 0x0A, 1, 0x00);
                AF_Memory_Update(ch, 1);
                AF_Memory_Update(ch, 5);
                Dln.WriteByte(ch, AF_Addr, 0xAE, 1, 0x00);
                temp = Dln.ReadByte(ch, AF_Addr, 0x03, 1);

                if (temp == 0x1C) { Process.AddLog(ch, $"I2C address change from 0x{icAddr_temp.ToString("X2")} to 0x{(AF_Addr << 1).ToString("X2")}"); return true; }
                else { Process.AddLog(ch, $"I2C address change NG(fpga error)"); return false; }

            }
            else { Process.AddLog(ch, $"I2C address change NG(check error)"); return false; }

        }
        public bool AF_Memory_Update(int ch, int mode)
        {
            byte val = 0;
            ushort time = 0;
            byte check = 0xFF;
            switch (mode)
            {
                case 0: val = 0x00; time = 0; break;
                case 1: val = 0x01; time = 150; break;
                case 2: val = 0x02; time = 230; break;
                case 3: val = 0x04; time = 200; break;
                case 4: val = 0x08; time = 210; break;
                case 5: val = 0x10; time = 50; break;
                default: break;
            }
            for (int i = 0; i < 5; i++)
            {
                Dln.WriteByte(ch, AF_Addr, 0x03, 1, val);
                Process.Wait(time);
                check = (byte)(Dln.ReadByte(ch, AF_Addr, 0x4B, 1) & 0x04);
                if (check == 0x00) break;
            }
            if (check != 0x00) { Process.AddLog(ch, $"AF Memory Update NG"); return false; }
            return true;
        }
        public void AF_IC_Data(int ch)
        {
            int target_code = 0;
            int current_code = 0;
            int poscal = 0;
            int negcal = 0;
            int posvt = 0;
            int negvt = 0;

            Dln.WriteByte(ch, AF_Addr, 0xAE, 1, 0x3B);
            AF_Memory_Update(ch, 5);
            Dln.WriteByte(ch, AF_Addr, 0xAE, 1, 0x00);
            target_code = ((Dln.Read2Byte(ch, AF_Addr, 0x00, 1) >> 4) & 0x0FFF);
            current_code = ReadAFHall(ch);
            poscal = (Dln.Read2Byte_signed(ch, AF_Addr, 0x04, 1) >> 1) & 0x7FFF;
            negcal = (Dln.Read2Byte_signed(ch, AF_Addr, 0x06, 1) >> 1) & 0x7FFF;
            posvt = (Dln.Read2Byte_signed(ch, AF_Addr, 0xC0, 1) >> 6) & 0x03FF;
            negvt = (Dln.Read2Byte_signed(ch, AF_Addr, 0xC2, 1) >> 6) & 0x03FF;

            poscal = (Dln.Read2Byte_signed(ch, AF_Addr, 0x04, 1) >> 1) & 0x7FFF;
            negcal = (Dln.Read2Byte_signed(ch, AF_Addr, 0x06, 1) >> 1) & 0x7FFF;
            posvt = (Dln.Read2Byte_signed(ch, AF_Addr, 0xC0, 1) >> 6) & 0x03FF;
            negvt = (Dln.Read2Byte_signed(ch, AF_Addr, 0xC2, 1) >> 6) & 0x03FF;
            Process.AddLog(ch, $"tag : {target_code}, cur : {current_code}");
            Process.AddLog(ch, $"pcal : {poscal}, ncal : {negcal}");
            Process.AddLog(ch, $"pvt : {posvt}, nvt : {negvt}");
        }
        public bool ICReset(int ch)
        {
            byte[] rbuf = new byte[1];
            Dln.WriteArray(ch, AF_Addr, 0x02, new byte[] { 0x40 });
            Process.Wait(50);
            Dln.WriteArray(ch, AF_Addr, 0x03, new byte[] { 0x10 });
            Process.Wait(100);
            Dln.ReadArray(ch, AF_Addr, 0x4B, rbuf);
            if ((byte)(rbuf[0] & 0x04) != 0x00)
            {

                Process.AddLog(ch, "Store fail");
                return false;
            }
            Dln.WriteArray(ch, AF_Addr, 0x02, new byte[] { 0x00 });
            Dln.WriteArray(ch, AF_Addr, 0x00, new byte[] { 0x80, 0x00 });
            Process.Wait(50);

            Dln.WriteArray(ch, XSlaveAddr, 0x02, new byte[] { 0x40 });
            Dln.WriteArray(ch, Y1SlaveAddr, 0x02, new byte[] { 0x40 });
            return true;
        }
        public void OISReset(int ch)
        {
            Dln.WriteArray(ch, XSlaveAddr, 0x02, new byte[] { 0x40 });
            Dln.WriteArray(ch, Y1SlaveAddr, 0x02, new byte[] { 0x40 });
            Thread.Sleep(50);
        }
        public void AFSleep(int ch)
        {
            Dln.WriteByte(ch, AF_Addr, 0x02, 1, 0x20);
            Process.AddLog(ch, $"AF Sleep Mode");

        }
        public void AFOnOff(int ch, bool isOn)
        {
            if (isOn)
            {
                Dln.WriteByte(ch, AF_Addr, 0x02, 1, 0x00);
                Process.AddLog(ch, $"AF Servo On");
            }
            else
            {
                Dln.WriteByte(ch, AF_Addr, 0x02, 1, 0x40);
                Process.AddLog(ch, $"AF Servo Off");
            }
            Process.Wait(10);
        }
        public void AFMove(int ch, int code)
        {
            if ((code & 0x8000) != 0) code = 0;
            code = code & 0x0FFF;
            Dln.Write2Byte(ch, AF_Addr, 0x00, 1, (ushort)(code << 4));
        }

        public void RegisterZeroSet(int iCh, int TargetSlaveID, byte address)
        {
            Dln.WriteArray(iCh, TargetSlaveID, address,new byte[1] { 0x00 });
        }
        public int ReadAFHall(int ch)
        {
            int a = (Dln.Read2Byte(ch, AF_Addr, 0x84, 1) >> 4) & 0x0FFF;
            return a;
        }
        #endregion


        #region OIS Function

        public bool ChangeSlaveAddrOIS(int ch)
        {
            bool xChanged = false;
            bool y1Changed = false;

            //Write 시 실패 여부로 판단
            //if (!Dln.WriteArray(ch, Y1SlaveAddr, 0xAE, new byte[] { 0x3B })) y1Changed = false;
            //if (!Dln.WriteArray(ch, XSlaveAddr, 0xAE, new byte[] { 0x3B })) xChanged = false;

            if (xChanged)
                Process.AddLog(ch, string.Format("Already X Slave Address Changed.."));
            else
            {
                if (!Dln.WriteArray(ch, XOriginAddr, 0xAE, new byte[] { 0x3B })) return false;
                Process.AddLog(ch, string.Format("Setting Mode = Write Mem : 0x{0:X2} XData : 0x{1:X2}", 0xAE, 0x3B));

                if (!Dln.WriteArray(ch, XOriginAddr, 0x0B, new byte[] { 0x02 })) return false; // 02 : Normal, 04 : Reverse
                Process.AddLog(ch, string.Format("Set Pin Mode = Write Mem : 0x{0:X2} XData : 0x{1:X2}", 0x0B, 0x02));

                //if (!Dln.WriteArray(ch, XOriginAddr, 0x0A, new byte[] { 0x59 })) return false; // Setting Slave Address
                if (!Dln.WriteArray(ch, XOriginAddr, 0x0A, new byte[] { 0x0b })) return false; // Setting Slave Address 0x59 : 변경 0x0b : 그대로
                Process.AddLog(ch, string.Format("Setting Slave Address = Write Mem : 0x{0:X2} XData : 0x{1:X2}", 0x0A, 0x0b));
                Process.Wait(200);
                if (!Dln.WriteArray(ch, XSlaveAddr, 0x03, new byte[] { 0x01 })) return false; // Store Memory
                Process.Wait(150);

                Process.AddLog(ch, string.Format("Store Memory = Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x03, 0x01));
                Process.AddLog(ch, string.Format("X SlaveAddr Change FinIsh."));
            }

            if (y1Changed)
                Process.AddLog(ch, string.Format("Already Y Slave Address Changed.."));
            else
            {
                if (!Dln.WriteArray(ch, Y1OriginAddr, 0xAE, new byte[] { 0x3B })) return false;
                Process.AddLog(ch, string.Format("Setting Mode = Write Mem : 0x{0:X2} YData : 0x{1:X2}", 0xAE, 0x3B));

                if (!Dln.WriteArray(ch, Y1OriginAddr, 0x0B, new byte[] { 0x04 })) return false; // 02 : Normal, 04 : Reverse
                Process.AddLog(ch, string.Format("Set Pin Mode = Write Mem : 0x{0:X2} YData : 0x{1:X2}", 0x0B, 0x02));

                //if (!Dln.WriteArray(ch, Y1OriginAddr, 0x0A, new byte[] { 0x59 })) return false; // Setting Slave Address
                if (!Dln.WriteArray(ch, Y1OriginAddr, 0x0A, new byte[] { 0x0b })) return false; // Setting Slave Address
                Process.AddLog(ch, string.Format("Setting Slave Address = Write Mem : 0x{0:X2} YData : 0x{1:X2}", 0x0A, 0x0b));
                Process.Wait(200);
                if (!Dln.WriteArray(ch, Y1SlaveAddr, 0x03, new byte[] { 0x01 })) return false; // Store Memory
                Process.Wait(150);
                Process.AddLog(ch, string.Format("Store Memory = Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x03, 0x01));
                Process.AddLog(ch, string.Format("Y SlaveAddr Change FinIsh."));
            }

            return true;
        }
        public void AK7326_IC_Mode(int ch, int axis, byte mode)
        {
            byte option = 0, index;
            if (mode == 0) option = 0x40;
            else if (mode == 1) option = 0x00;
            else if (mode == 2) option = 0x40;
            else if (mode == 3) option = 0x00;
            int slaveaddr = axis == 0 ? XSlaveAddr : Y1SlaveAddr;
            string AxisStr = axis == 0 ? "OIS X" : "OIS Y";
            string modeStr = mode == 0 ? "Standby mode" : "Active mode";
            if(mode == 0 || mode == 1)
            {
                Dln.WriteArray(ch, slaveaddr, 0x02, new byte[] { option });
                Process.AddLog(ch, $"{AxisStr} {modeStr}");
            }
            else
            {
                Dln.WriteArray(ch, XSlaveAddr, 0x02, new byte[] { option });
                Dln.WriteArray(ch, Y1SlaveAddr, 0x02, new byte[] { option });
            }
            if (mode == 2) Process.AddLog(ch, "OIS Standby mode");
            if(mode == 3) Process.AddLog(ch, "OIS Active mode");
        }
        public void AK7326_IC_Data(int ch)
        {
            byte PIDVer, ProductID;
            int[] data = new int[2];

            byte[] rbuf = new byte[1];
            byte[] rbuf2 = new byte[2];
            Process.AddLog(ch, "=============== AK7326 IC Data ===============");
            for (int i = 0; i < 2; i++)
            {
                int slaveAddr = i == 0 ? XSlaveAddr : Y1SlaveAddr;
                AK7326_check_byte(ch, i, 0x00, 0x0F);
                AK7326_check_byte(ch, i, 0x10, 0x1F);
                AK7326_check_byte(ch, i, 0x20, 0x2F);
                AK7326_check_byte(ch, i, 0x30, 0x3F);
                AK7326_check_byte(ch, i, 0xE0, 0xEF);
                AK7326_check_byte(ch, i, 0xF0, 0xFF);

                Dln.ReadArray(ch, slaveAddr, 0x04, rbuf2);
                data[0] = ((rbuf2[0] << 8) + rbuf2[1]) >> 4;
                Dln.ReadArray(ch, slaveAddr, 0x06, rbuf2);
                data[1] = ((rbuf2[0] << 8) + rbuf2[1]) >> 4;
                Process.AddLog(ch, $"PCal : {data[0]}, Ncal : {data[1]}");
            }
           
        }
        public void AK7326_IC_reset(int ch)
        {
            Move(ch, "X", 4096);
            Move(ch, "Y", 4096);
            OISOn(ch, "X", true);
            OISOn(ch, "Y", true);

        }

        void AK7326_check_byte(int ch, int axis,  byte start, byte end)
        {
            int addr = 0; int index = 0;
            string s = string.Empty;
            byte[] rbuf = new byte[1];
            int slaveaddr = axis == 0 ? XSlaveAddr : Y1SlaveAddr;
            s += $"0x{start.ToString("X2")}~0x{end.ToString("X2")} : ";

            for (addr = start, index = 0; addr <= end; addr++, index++)
            {
                Dln.ReadArray(ch, slaveaddr, addr, rbuf);
                if ((index & 0x0003) == 0x0000)
                    s += " ";
                s += rbuf[0].ToString("X2");

            }
            Process.AddLog(ch, s);

        }
     
        public bool AK7326_memory_update(int ch, byte dir, int mode)
        {
            int index = 0;
            byte[] MemoryUpdataeAddr = new byte[] { 0x00, 0x01, 0x02, 0x04, 0x08, 0x10 };
            int[] MemoryUpdataeTime = new int[] { 0, 160, 270, 160, 100, 60 };
            int slaveaddr = dir == 0 ? XSlaveAddr : Y1SlaveAddr;
            bool res = false;
            byte val = 0;
            byte[] rbuf = new byte[1];
            switch (mode)
            {
                case 0:
                    for (index = 0; index < 5; index++)
                    {
                        Dln.WriteArray(ch, slaveaddr, 0x03, new byte[] { MemoryUpdataeAddr[index + 1] });
                        Process.Wait(MemoryUpdataeTime[index]);
                    }
                    for (index = 0; index < 5; index++)
                    {
                        Dln.ReadArray(ch, slaveaddr, 0x4B, rbuf);
                        val = (byte)(rbuf[0] & 0x04);
                   
                        if (val == 0x00)
                            break;
                    }
                    if ((index > 4))
                    {
                        Process.AddLog(ch, $"-- memory update NG (%c) -- {dir}");
                     
                        return false;
                    }

                    break;
                case 1:
                    Dln.WriteArray(ch, slaveaddr, 0x03, new byte[] { MemoryUpdataeAddr[5] });
                    Process.Wait(MemoryUpdataeTime[5]);
                    break;
                default:
                    break;
            }
            return true;
        }
        public void AK7326_PM_set_slave(int ch, int axis)
        {
            Dln.WriteArray(ch, FRA_Addr, 0x00, new byte[] { 0x01 }); Process.AddLog(ch, $"Write Mem : 0x{0x00:X2}, Data : 0x{0x01:X2}");
            Dln.WriteArray(ch, FRA_Addr, 0x00, new byte[] { 0x00 }); Process.AddLog(ch, $"Write Mem : 0x{0x00:X2}, Data : 0x{0x00:X2}");
            if (axis == 0) { Dln.WriteArray(ch, FRA_Addr, 0x6F, new byte[] { (byte)FRA_XSlaveAddr }); Process.AddLog(ch, $"Write Mem : 0x{0x6F:X2}, Data : 0x{FRA_XSlaveAddr:X2}"); }
            else if (axis == 1) { Dln.WriteArray(ch, FRA_Addr, 0x6F, new byte[] { (byte)FRA_Y1SlaveAddr }); Process.AddLog(ch, $"Write Mem : 0x{0x6F:X2}, Data : 0x{FRA_Y1SlaveAddr:X2}"); }
            else
            {
                Dln.WriteArray(ch, FRA_Addr, 0x6F, new byte[] { (byte)FRA_XSlaveAddr }); Process.AddLog(ch, $"Write Mem : 0x{0x6F:X2}, Data : 0x{FRA_XSlaveAddr:X2}");
                Dln.WriteArray(ch, FRA_Addr, 0x89, new byte[] { (byte)FRA_Y1SlaveAddr }); Process.AddLog(ch, $"Write Mem : 0x{0x89:X2}, Data : 0x{FRA_Y1SlaveAddr:X2}");
            }
        }
        public void OIS_drift_test_mode_init(int ch, bool status)
        {
            Move(ch, "X", 2048);
            Move(ch, "Y", 2048);
            OISOn(ch, "X", true);
            OISOn(ch, "Y", true);
            Process.Wait(100);
            if(status) { OISOn(ch, "X", false); OISOn(ch, "Y", false); }
            else { OISOn(ch, "X", true); OISOn(ch, "Y", true); }
            Process.Wait(100);
        }
        public void OIS_drift_test_mode_close(int ch, bool status)
        {
            if (status) { OISOn(ch, "X", false); OISOn(ch, "Y", false); }
        }
        public void AK7326_EEPROM_Writecheck(int ch, byte dir, byte address, byte value)
        {
            byte[] rbuf = new byte[1];
            byte data = 0;
            int slave = dir == 0 ? XSlaveAddr : Y1SlaveAddr;
            while (true)
            {
                Dln.WriteArray(ch, slave, 0xAE, new byte[] { 0x3B });
                Dln.WriteArray(ch, slave, address, new byte[] { value });
                Process.Wait(30);

                data++;
                Dln.ReadArray(ch, slave, 0x4B, rbuf);
                if ((rbuf[0] & 0x04) == 0x00)
                    break;
                if (data > 5)
                    break;
            }
            Dln.WriteArray(ch, slave, 0xAE, new byte[] { 0x00 });
      
        }

        public void OISOn(int ch, string name, bool isOn)
        {
            byte data = 0x00;

            if (name.Contains("X"))
            {
                if (isOn)
                {
                    Process.AddLog(ch, string.Format("OIS X On"));
                }
                else
                {
                    data = 0x40;
                    Process.AddLog(ch, string.Format("OIS X Off"));
                }

                if (!Dln.WriteArray(ch, XSlaveAddr, 0x02, new byte[] { data })) return;
                Process.AddLog(ch, string.Format("Write Mem : 0x{0:X2} XData : 0x{1:X2}", 0x02, data));
                Process.Wait(10);
            }
            else if (name.Contains("Y"))
            {
                if (isOn)
                {
                    Process.AddLog(ch, string.Format("OIS Y On"));
                }
                else
                {
                    data = 0x40;
                    Process.AddLog(ch, string.Format("OIS Y Off"));
                }

                if (!Dln.WriteArray(ch, Y1SlaveAddr, 0x02, new byte[] { data })) return;
                Process.AddLog(ch, string.Format("Write Mem : 0x{0:X2} Y1Data : 0x{1:X2}", 0x02, data));

                if (Y2SlaveAddr != 0x00)
                {
                    if (!Dln.WriteArray(ch, Y2SlaveAddr, 0x02, new byte[] { data })) return;
                    Process.AddLog(ch, string.Format("Write Mem : 0x{0:X2} Y2Data : 0x{1:X2}", 0x02, data));
                }
                Process.Wait(10);
            }


        }
        public bool Move(int ch, string name, int pos, bool openLoop = false)
        {
            var context = new MoveHallContext(IC_BITUSE.BIT_13, Dln);
            return context.Move(ch, name, pos, openLoop);
        }
        public int ReadHall(int ch, string name)
        {
            //int addr = 0x00;

            //if (name.Contains("X")) addr = XSlaveAddr;
            //else if (name.Contains("Y2")) addr = Y2SlaveAddr;
            //else if (name.Contains("Y1") || name.Contains("Y")) addr = Y1SlaveAddr;


            //byte[] data = new byte[2];

            //if (addr != 0x00) Dln.ReadArray(ch, addr, 0x84, data);
            //if (name == "Y2" && Y2SlaveAddr != 0x00) Dln.ReadArray(ch, addr, 0x84, data);


            //return ((data[0] << 8) + data[1]) >> 4;

            var context = new ReadHallContext(IC_BITUSE.BIT_13, Dln);
            return context.ReadHall(ch, name);
        }
        public int ReadHallOpenLoop(int ch, string name)
        {
            int addr = 0x00;

            if (name.Contains("X")) addr = XSlaveAddr;
            else if (name.Contains("Y2")) addr = Y2SlaveAddr;
            else if (name.Contains("Y1") || name.Contains("Y")) addr = Y1SlaveAddr;


            byte[] data = new byte[2];

            if (addr != 0x00) Dln.ReadArray(ch, addr, 0x80, data);
            if (name == "Y2" && Y2SlaveAddr != 0x00) Dln.ReadArray(ch, addr, 0x84, data);

            return ((data[0] << 8) + data[1]) >> 4;
        }
        #endregion






        public bool FRA_Single(int ch, string name, int amp, int mode, List<double> freq, ref List<double> gain, ref List<double> phase)
        {
            int addr;
            int sAddr;
            string axis;
            if (name.Contains("X"))
            {
                addr = FRA_XSlaveAddr;
                sAddr = XSlaveAddr;
                axis = "X";
            }
            else if (name.Contains("Y1"))
            {
                addr = FRA_Y1SlaveAddr;
                sAddr = Y1SlaveAddr;
                axis = "Y1";
            }
            else if (name.Contains("Y2"))
            {
                addr = FRA_Y2SlaveAddr;
                sAddr = Y2SlaveAddr;
                axis = "Y2";
            }
          
            else
                return false;

            if(addr != 0x00) SetSlaveAddr(ch, addr);
            byte[] data = new byte[1];

            if (!Dln.WriteArray(ch, sAddr, 0x02, new byte[] { 0x40 })) return false;
            Thread.Sleep(10);
            // Process.AddLog(ch, string.Format("Setting Mode = Write Mem : 0x{0:X2} {1}Data : 0x{2:X2}", 0xAE, axis, 0x3B));

            if (!Dln.WriteArray(ch, sAddr, 0xAE, new byte[] { 0x3B })) return false;
            Process.AddLog(ch, string.Format("Setting Mode = Write Mem : 0x{0:X2} {1}Data : 0x{2:X2}", 0xAE, axis, 0x3B));

            Dln.ReadArray(ch, sAddr, 0x4B, data);
            Process.AddLog(ch, string.Format("Read Mem : 0x{0:X2} Data : 0x{1:X2}", 0x4C, data[0]));


            if ((data[0] & 8) == 8)
            {
                if (!FRAModeDisable(ch)) return false;
            }

            if (!FRAModeEnable(ch)) return false;

            if (!Set_Amp(ch, amp)) return false;
            int oldfreq = (int)freq[0];
            for (int i = 0; i < freq.Count; i++)
            {
                if (!Set_Freq(ch, (int)freq[i])) return false;
                Thread.Sleep((int)(1000 / oldfreq + 5000 / freq[i] + 15));
                oldfreq = (int)freq[i];

                gain.Add(Get_Gain(ch));

                phase.Add(Get_Phase(ch, 0));

                Process.AddLog(ch, string.Format("{0} FRA Freq : {1} gain : {2:0.00} phase : {3:0.00}", axis, freq[i], gain[i], phase[i]));

                if (i > 0)
                {
                    if (mode == 0)
                    {
                        if (gain[i] * gain[i - 1] <= 0 && gain[i - 1] < 0) { Process.AddLog(ch, "Zero Cross Detected."); break; }

                    }
                    else if (mode == 1)
                    {
                        if (phase[i] * phase[i - 1] <= 0 && phase[i - 1] < 0) { Process.AddLog(ch, "Zero Cross Detected."); break; }
                    }
                }

            }

            if (!FRAModeDisable(ch)) return false;

            return true;
        }

        public bool SetSlaveAddr(int ch, int addr)
        {
            Process.AddLog(ch, string.Format("Set Slave Addr"));
            if (!Dln.WriteArray(ch, FRA_Addr, 0x00, new byte[] { 0x01 })) return false;
            Process.AddLog(ch, string.Format("Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x00, 0x01));
            if (!Dln.WriteArray(ch, FRA_Addr, 0x00, new byte[] { 0x00 })) return false;
            Process.AddLog(ch, string.Format("Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x00, 0x00));
            if (!Dln.WriteArray(ch, FRA_Addr, 0x6F, new byte[] { (byte)addr })) return false;
            Process.AddLog(ch, string.Format("Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x6F, addr));

            return true;
        }
        public bool FRAModeEnable(int ch)
        {
            Process.AddLog(ch, string.Format("FRA Mode Enable"));
            if (!Dln.WriteArray(ch, FRA_Addr, 0x56, new byte[] { 0x80 })) return false;
            Process.AddLog(ch, string.Format("Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x56, 0x80));
            if (!Dln.WriteArray(ch, FRA_Addr, 0xAC, new byte[] { 0x01 })) return false;
            Process.AddLog(ch, string.Format("Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0xAC, 0x01));
            Process.Wait(5);

            if (!Dln.WriteArray(ch, FRA_Addr, 0x54, new byte[] { 0x0F })) return false;
            Process.AddLog(ch, string.Format("Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x54, 0x0F));
            if (!Dln.WriteArray(ch, FRA_Addr, 0x55, new byte[] { 0x00 })) return false;
            Process.AddLog(ch, string.Format("Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0x55, 0x00));
            Process.Wait(5);

          
            if (!Dln.WriteArray(ch, FRA_Addr, 0xA8, new byte[] { 0xC5 })) return false;
            Process.AddLog(ch, string.Format("Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0xA8, 0xC5));
            Process.Wait(1000);

            return true;
        }
        public bool FRAModeDisable(int ch)
        {
            
            Process.AddLog(ch, string.Format("FRA Mode Disable"));
            if (!Dln.WriteArray(ch, FRA_Addr, 0xA8, new byte[] { 0x00 })) return false;
            Process.AddLog(ch, string.Format("Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0xA8, 0x00));
            if (!Dln.WriteArray(ch, FRA_Addr, 0xAF, new byte[] { 0xEE })) return false;
            Process.AddLog(ch, string.Format("Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0xAF, 0xEE));
            Process.Wait(5);

            if (!Dln.WriteArray(ch, FRA_Addr, 0xAC, new byte[] { 0x00 })) return false;
            Process.AddLog(ch, string.Format("Write Mem : 0x{0:X2} Data : 0x{1:X2}", 0xAC, 0x00));
            Process.Wait(15);


            return true;
        }
        public bool Set_Amp(int ch, int val)
        {
            int data = val << 6;

            if (!Dln.WriteArray(ch, FRA_Addr, 0x52, new byte[2] { (byte)(data >> 8), (byte)(data % 256) })) return false;
            //Process.AddLog(ch, string.Format("Write Mem : 0x{0:X2} Data : 0x{1:X4}", 0x52, data));

            return true;
        }
        public bool Set_Freq(int ch, int val)
        {
            int data = val << 1;

            if (!Dln.WriteArray(ch, FRA_Addr, 0x50, new byte[2] { (byte)(data >> 8), (byte)(data % 256) })) return false;

            
            Process.Wait(5000 / val + 10);
            //Process.AddLog(ch, string.Format("Write Mem : 0x{0:X2} Data : 0x{1:X4}", 0x50, data));

            return true;
        }
        public double Get_Gain(int ch)
        {
            byte[] data = new byte[3];
            Dln.ReadArray(ch, FRA_Addr, 0x94, data);
            double val = (data[0] << 16) + (data[1] << 8) + data[2];
            return Math.Log10(val / 65536) * 20;
        }
        public  double Get_Phase(int ch, int mode)
        {
            byte[] data = new byte[2];
            Dln.ReadArray(ch, FRA_Addr, 0x98, data);
            double val = (data[0] << 8) + data[1];
            val /= 128;
            if (val > 256)
                val -= 512;
            val = 180 + val;
            if(mode == 0)
            {
                if (val > 180) val = 360 - val;
                if (val < -180) val += 360;
            }
            else
            {
                if (val > 180) val = val - 360;
                if (val < -180) val += 360;
            }

                return val;
        }

        public void CurrentSetRegister(int ch, int iAxis)
        {
            int slaveID = 0;
            byte PointerRegister = 0x00;
            byte MSBbyte = 0x00;
            byte LSBbyte = 0x00;

            PointerRegister = BitDataHelper.SetBits(PointerRegister, (byte)POINTEREGISTER.CONFIGREGISTER);
            MSBbyte = BitDataHelper.SetBits(MSBbyte, (byte)ADS1013_MSB.OS_START);
            LSBbyte = BitDataHelper.SetBits(LSBbyte, (byte)ADS1013_LSB.SPS3300);

            if (iAxis == 0)      //OIS  
                slaveID = OIS_ADC_Addr;
            else
                slaveID = AF_ADC_Addr;

            Dln.WriteArray(ch, slaveID, new byte[] { PointerRegister, MSBbyte, LSBbyte });
            Thread.Sleep(20);

            PointerRegister = BitDataHelper.ClearBits(PointerRegister, (byte)POINTEREGISTER.CONFIGREGISTER);
            Dln.WriteArray(ch, slaveID, new byte[] { PointerRegister });
        }

        public short GetPeakCurrent(int ch, int iAxis)
        {
            int slaveID = 0;
            byte[] readByte = new byte[2] { 0x00, 0x00 };

            if (iAxis == 0)      //OIS  
                slaveID = OIS_ADC_Addr;
            else
                slaveID = AF_ADC_Addr;

            Dln.ReadArray(ch, slaveID, readByte);
            Thread.Sleep(1);

            var word = (short)((readByte[0] << 8) | readByte[1]);
            return (short)(word >> 4);
        }
    }
}
