using Dln;
using Dln.Gpio;
using FZ4P.Commons.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FZ4P
{
    public class DLN : IGPIOOutput
    {
        public Process Process { get { return STATIC.Process; } }
        public uint m_PortCount = 0;
        public List<Device> DLNdevice = new List<Device>();
        public Dln.I2cMaster.Port[] DLNi2c;
        public Dln.Gpio.Module[] DLNgpio;

        public event EventHandler SwitchOn = null;
        public event EventHandler SafetyOn = null;
        private bool isSafeOn = false;
        public bool IsRun = false;
        public bool isMoving = false;
        public bool IsSafeOn
        {
            get { return isSafeOn; }
            set { if (value != isSafeOn) { isSafeOn = value; SafetyOn?.Invoke(null, EventArgs.Empty); } }
        }

        private bool IsSwitch = false;
        public bool m_bOccupied = false;
        public bool[] IsLoad = new bool[2] { false, false };
        object I2cLock = new object();
        public DLN(int mode)
        {
            if (!Init(mode)) return;
        }

        public void SetError(string s)
        {
            if (STATIC.Rcp.Model.MCType == "Handler")
            {
                STATIC.TcpConn.SendMessage("Disconnected", STATIC.TcpConn.TCPCOnState);
                STATIC.TcpConn.disconnect();


                STATIC.fSystemLogView.TextView($"SendMessage : Disconnected");
            }

            Form f = Application.OpenForms["F_Main"];

            if (f != null)
            {
                //TODO : 임시 삭제
                //MessageBox.Show(f, s, "Error",
                //        MessageBoxButtons.OK, MessageBoxIcon.Error);
                //System.Diagnostics.Process.GetCurrentProcess().Kill();
            }
            else
            {
                // 메인폼을 못 찾았을 때 (owner 없이 표시)
                //MessageBox.Show(s, "Error",
                //    MessageBoxButtons.OK, MessageBoxIcon.Error);
                //System.Diagnostics.Process.GetCurrentProcess().Kill();
            }
        }

        public bool Init(int mode = 0)
        {
            try
            {
                if (DLNdevice.Count > 0)
                    DLNdevice.Clear();

                Library.Connect("localhost", Connection.DefaultPort);

                m_PortCount = Device.Count();

                if (m_PortCount == 0)
                {
                    //MessageBox.Show("--- No DLN-series adapters ---.", null, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
            catch /*(Exception ex)*/
            {
                //MessageBox.Show(ex.Message);
                return false;
            }

            for (int i = 0; i < m_PortCount; i++)
            {
                try
                {
                    DLNdevice.Add(Device.Open(i));
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Port " + i + " : " + ex.Message + "\n Re-Connect USB Cable!");    // disappeared
                    return false;
                }
            }

            DLNi2c = new Dln.I2cMaster.Port[m_PortCount];
            DLNgpio = new Dln.Gpio.Module[m_PortCount];

            for (int i = 0; i < m_PortCount; i++)
            {
                try
                {
                    if (DLNdevice[i].I2cMaster.Ports[0].Restrictions.MaxReplyCount != Restriction.NotSupported)
                        DLNdevice[i].I2cMaster.Ports[0].MaxReplyCount = 10;

                    if (DLNdevice[i].I2cMaster.Ports[0].Restrictions.Frequency == Restriction.MustBeDisabled)
                        DLNdevice[i].I2cMaster.Ports[0].Enabled = false;

                    DLNdevice[i].I2cMaster.Ports[0].Frequency = 400 * 1000;
                    DLNdevice[i].I2cMaster.Ports[0].Enabled = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Port " + i + " : " + ex.Message);
                    return false;
                }
            }
            if (mode == 1)
            {
                DLNi2c[0] = DLNdevice[0].I2cMaster.Ports[0];
                return true;
            }

            for (int i = 0; i < m_PortCount; i++)
            {
                // ID
                DLNdevice[i].Gpio.Pins[6].Enabled = true;
                DLNdevice[i].Gpio.Pins[6].Direction = 0;   //  0 ~ 15 : 0(in), 24 ~ 31 : 1(out)
                DLNdevice[i].Gpio.Pins[6].PulldownEnabled = true;
                DLNdevice[i].Gpio.Pins[7].Enabled = true;
                DLNdevice[i].Gpio.Pins[7].Direction = 0;   //  0 ~ 15 : 0(in), 24 ~ 31 : 1(out)
                DLNdevice[i].Gpio.Pins[7].PulldownEnabled = true;

                // 스위치
                DLNdevice[i].Gpio.Pins[8].Enabled = true;
                DLNdevice[i].Gpio.Pins[8].Direction = 0;   //  0 ~ 15 : 0(in), 24 ~ 31 : 1(out)
                DLNdevice[i].Gpio.Pins[8].PulldownEnabled = true;

                // 스위치
                DLNdevice[i].Gpio.Pins[(int)GPIONumber.SocketOutPut].Enabled = true;
                DLNdevice[i].Gpio.Pins[(int)GPIONumber.SocketOutPut].Direction = 0;   //  0 ~ 15 : 0(in), 24 ~ 31 : 1(out)
                DLNdevice[i].Gpio.Pins[(int)GPIONumber.SocketOutPut].PulldownEnabled = true;

                GPIOOutputSet(i, (int)GPIONumber.PeakDetector_DetectADC1);
                GPIOOutputSet(i, (int)GPIONumber.PeakDetector_ResetADC1);
                GPIOOutputSet(i, (int)GPIONumber.PeakDetector_DetectADC2);
                GPIOOutputSet(i, (int)GPIONumber.PeakDetector_ResetADC2);

                Thread.Sleep(100);

                int[] res = new int[2];
                res[0] = DLNdevice[i].Gpio.Pins[6].Value;
                int portID = 0;
                if (res[0] == 1)
                    portID++;

                res[1] = DLNdevice[i].Gpio.Pins[7].Value;
                if (res[1] == 1)
                    portID += 2;

                int portCount = DLNdevice[i].I2cMaster.Ports.Count;
                if (portCount == 0)
                {
                    MessageBox.Show("Current DLN-series adapter doesn't support I2C Master interface.", null, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
                DLNi2c[portID] = DLNdevice[i].I2cMaster.Ports[0];
                DLNgpio[portID] = DLNdevice[i].Gpio;
            }

            if (DLNgpio.Length > 2)
            {
                DLNgpio[2].Pins[9].Enabled = true;
                DLNgpio[2].Pins[9].Direction = 0;   //  0 ~ 15 : 0(in), 24 ~ 31 : 1(out)
                DLNgpio[2].Pins[9].OutputValue = 1;
                DLNgpio[2].Pins[9].PulldownEnabled = true;
            }

            //DLNgpio[1].Pins[9].Enabled = true;
            //DLNgpio[1].Pins[9].Direction = 0;   //  0 ~ 15 : 0(in), 24 ~ 31 : 1(out)
            //DLNgpio[1].Pins[9].OutputValue = 1;
            //DLNgpio[1].Pins[9].PulldownEnabled = true;


            //DLNgpio[1].Pins[8].ConditionMetThreadSafe += SWEventHandler;
            //DLNgpio[1].Pins[8].SetEventConfiguration(Dln.Gpio.EventType.LevelHigh, 50);

            //DLNgpio[1].Pins[10].Enabled = true;
            //DLNgpio[1].Pins[10].Direction = 1;   //  0 ~ 15 : 0(in), 24 ~ 31 : 1(out)

            //DLNgpio[1].Pins[20].Enabled = true;
            //DLNgpio[1].Pins[20].Direction = 1;   //  0 ~ 15 : 0(in), 24 ~ 31 : 1(out
            //                                     //  

            //DLNgpio[1].Pins[11].Enabled = true;
            //DLNgpio[1].Pins[11].Direction = 1;   //  0 ~ 15 : 0(in), 24 ~ 31 : 1(out)

            //DLNgpio[1].Pins[21].Enabled = true;
            //DLNgpio[1].Pins[21].Direction = 1;   //  0 ~ 15 : 0(in), 24 ~ 31 : 1(out)

            //DLNgpio[1].Pins[(int)GPIONumber.SocketOutPut].ConditionMetThreadSafe += SafeEventHandler;
            //DLNgpio[1].Pins[(int)GPIONumber.SocketOutPut].SetEventConfiguration(Dln.Gpio.EventType.LevelHigh, 50);


            //SetSocketSensor(STATIC.Rcp.Option.SocketSensorUse);

            debounceLogics = new[] { new SignalDebounceLogic(),
                                     new SignalDebounceLogic(),};

            foreach (var logic in debounceLogics)
            {
                logic.SignalChanged += SignalChanged;
            }
            return true;
        }
        private SignalDebounceLogic[] debounceLogics;
        public static bool SocIn = false;
        public static bool SocOut = false;


        private void GPIOOutputSet(int deviceNumber, int pinNumber)
        {
            DLNdevice[deviceNumber].Gpio.Pins[pinNumber].Enabled = true;
            DLNdevice[deviceNumber].Gpio.Pins[pinNumber].Direction = 1;   //  0 ~ 15 : 0(in), 24 ~ 31 : 1(out)
        }
        private void SignalChanged(object sender, DebounceEventArgs args)
        {
            if (!args.IsSuccess) return;

            switch (args.SignalType)
            {
                case TargetSignalType.Emergency:
                    if (args.SignalState)
                    {
                        //IsTEMG = args.SignalState;
                        //TEMGOn?.Invoke(null, EventArgs.Empty);
                    }
                    else
                    {
                        //IsTEMG = args.SignalState;
                        //TEMGOff?.Invoke(null, EventArgs.Empty);
                    }
                    break;
                case TargetSignalType.Start:
                    //시작 버튼이 라이징일때만 해야된다.
                    if (args.SignalState)
                    {
                        SwitchOn?.Invoke(null, EventArgs.Empty);
                    }
                    break;
                default: break;
            }
        }
        private void DLN_ConditionMetThreadSafe(object sender, Dln.Gpio.ConditionMetEventArgs e)
        {
            switch (e.Pin)
            {
                case 12:
                    if (e.Value == 1)
                        SocIn = true;
                    else SocIn = false;
                    break;
                case 13:
                    if (e.Value == 1)
                        SocOut = true;
                    else SocOut = false;
                    break;
            }
        }
        private void Debounce(int Index, TargetSignalType targetSignalType, bool initSignal)
        {
            if (!debounceLogics[Index].DebounceState)
            {
                debounceLogics[Index].SetDebounceMs(100)
                       .SetTagetSignalType(targetSignalType)
                       .SetInitState(initSignal);

                debounceLogics[Index].StartSignal();
                Debug.WriteLine("=====================");
                Debug.WriteLine($"Debounce On");
            }
            else
            {
                debounceLogics[Index].StopSignal();
                Debug.WriteLine($"Debounce Off");
                Debug.WriteLine("=====================");
            }
        }
        private void SWEventHandler(object sender, Dln.Gpio.ConditionMetEventArgs e)
        {
            if (e.Value == 1 && !IsSwitch)
            {
                DLNgpio[1].Pins[8].SetEventConfiguration(Dln.Gpio.EventType.LevelLow, 50);
                IsSwitch = true;
                Debounce(1, TargetSignalType.Start, false);
                //SwitchOn?.Invoke(null, EventArgs.Empty);
            }
            else if (e.Value == 0 && IsSwitch)
            {
                DLNgpio[1].Pins[8].SetEventConfiguration(Dln.Gpio.EventType.LevelHigh, 50);
                IsSwitch = false;
                Debounce(1, TargetSignalType.Start, true);
            }
        }

        private void SafeEventHandler(object sender, Dln.Gpio.ConditionMetEventArgs e)
        {
            if (e.Value == 1)
            {
                DLNgpio[1].Pins[(int)GPIONumber.SocketOutPut].SetEventConfiguration(Dln.Gpio.EventType.LevelLow, 50);
                IsSafeOn = false;
            }
            else if (e.Value == 0)
            {
                DLNgpio[1].Pins[(int)GPIONumber.SocketOutPut].SetEventConfiguration(Dln.Gpio.EventType.LevelHigh, 50);
                IsSafeOn = true;
            }
        }
        public void LoadSocket()
        {
            try
            {
                if (DLNgpio == null) return;

                lock (I2cLock)
                {
                    DLNgpio[1].Pins[10].OutputValue = 1;
                    DLNgpio[1].Pins[20].OutputValue = 0;
                }

            }
            catch
            {

                SetError("Load Socket I2C NG");
                //Form f = Application.OpenForms["F_Main"];
                //MessageBox.Show(f, "Load Socket I2C NG", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);//STATIC.Process.AddLog(0, $"I2C Disconnected");
                //System.Diagnostics.Process.GetCurrentProcess().Kill();
                //Init();
            }

        }
        public void UnloadSocket()
        {
            try
            {
                if (DLNgpio == null) return;

                lock (I2cLock)
                {
                    DLNgpio[1].Pins[10].OutputValue = 0;
                    DLNgpio[1].Pins[20].OutputValue = 1;
                }
            }
            catch
            {
                SetError("Unload Socket I2C NG");
            }


        }

        public void CoverDn()
        {
            try
            {
                if (DLNgpio == null) return;
                lock (I2cLock)
                {
                    DLNgpio[1].Pins[11].OutputValue = 1;
                    DLNgpio[1].Pins[21].OutputValue = 0;

                }
            }
            catch
            {
                SetError("Cover Dn I2C NG");

            }


        }
        public void CoverUp()
        {
            try
            {
                if (DLNgpio == null) return;
                lock (I2cLock)
                {
                    DLNgpio[1].Pins[11].OutputValue = 0;
                    DLNgpio[1].Pins[21].OutputValue = 1;
                }
            }
            catch
            {
                SetError("Cover Up NG");

            }


        }
        public void SetLEDpower(int id, int value)
        {
            byte bufferH = 0;
            byte[] bufferL = new byte[1];

            int lDACaddr = 0x4F;        // A0,A1상태에 따라 ID 변경, 지금은  A0,A1 pull up

            if (value > 1650)   // 소프트웨어 전압 기준 3.3V
            {
                value = 1650;
            }
            //if (value > 4095)
            //{
            //    value = 4095;
            //}


            //  기존 single channel dac code
            //   | XXXX | XXXX |  
            //   | XXXX | XXXX | XXXX | 0000 |
            //   | Address | CtrlByte | Value(12bit) |
            bufferH = (byte)(value / 16);
            bufferL[0] = (byte)(value << 4);

            //  기존 single channel dac code
            //bufferH = (byte)(value / 256);
            //bufferL[0] = (byte)(value % 256);


            byte[] left_side = { 0x10 };      //1
            byte[] left_center = { 0x12 };    //2
            byte[] right_side = { 0x14 };     //3
            byte[] right_center = { 0x16 };   //4


            int ch = 0;

            while (m_bOccupied)
            {
                Thread.Sleep(1);
            }
            m_bOccupied = true;
            if (DLNi2c == null) return;
            try
            {
                if (id == 1)
                {
                    byte[] datas = { left_side[0], bufferH, bufferL[0] };
                    lock (I2cLock) DLNi2c[ch]?.Write(lDACaddr, datas); // diolan(0,1기준) 1번에서  LED control
                }
                else if (id == 2)
                {
                    byte[] datas = { left_center[0], bufferH, bufferL[0] };
                    lock (I2cLock) DLNi2c[ch]?.Write(lDACaddr, datas); // diolan(0,1기준) 1번에서  LED control
                }
                else if (id == 3)
                {
                    byte[] datas = { right_side[0], bufferH, bufferL[0] };
                    lock (I2cLock) DLNi2c[ch]?.Write(lDACaddr, datas); // diolan(0,1기준) 1번에서  LED control
                }
                else if (id == 4)
                {
                    byte[] datas = { right_center[0], bufferH, bufferL[0] };
                    lock (I2cLock) DLNi2c[ch]?.Write(lDACaddr, datas); // diolan(0,1기준) 1번에서  LED control
                }
                m_bOccupied = false;
            }
            catch
            {
                Init();
                try
                {
                    if (id == 1)
                    {
                        byte[] datas = { left_side[0], bufferH, bufferL[0] };
                        lock (I2cLock) DLNi2c?[ch].Write(lDACaddr, datas); // diolan(0,1기준) 1번에서  LED control
                    }
                    else if (id == 2)
                    {
                        byte[] datas = { left_center[0], bufferH, bufferL[0] };
                        lock (I2cLock) DLNi2c?[ch].Write(lDACaddr, datas); // diolan(0,1기준) 1번에서  LED control
                    }
                    else if (id == 3)
                    {
                        byte[] datas = { right_side[0], bufferH, bufferL[0] };
                        lock (I2cLock) DLNi2c?[ch].Write(lDACaddr, datas); // diolan(0,1기준) 1번에서  LED control
                    }
                    else if (id == 4)
                    {
                        byte[] datas = { right_center[0], bufferH, bufferL[0] };
                        lock (I2cLock) DLNi2c?[ch].Write(lDACaddr, datas); // diolan(0,1기준) 1번에서  LED control
                    }
                }
                catch
                {
                    SetError("Fail to LED Power :: Please Check USB Cable");

                    m_bOccupied = false;
                }
                m_bOccupied = false;
            }
        }
        public void PowerOnOff(int port, bool IsOn = true)
        {
            try
            {
                if (STATIC.Process.IsVirtual) return;
                if (IsOn)
                {

                    STATIC.Process.AddLog(0, $"Power On");
                    if (DLNgpio.Length > 2) { lock (I2cLock) DLNgpio[2].Pins[9].Direction = 1; }
                    lock (I2cLock) DLNgpio[1].Pins[9].Direction = 1;
                }
                else
                {
                    STATIC.Process.AddLog(0, $"Power Off");
                    if (DLNgpio.Length > 2) { lock (I2cLock) DLNgpio[2].Pins[9].Direction = 0; }

                    lock (I2cLock) DLNgpio[1].Pins[9].Direction = 0;



                }
            }
            catch
            {
                SetError("Power On Off I2C NG");

            }

        }
        public double GetCurrent(int ch, int mode)
        {
            double res = 0;
            int RegAddr = 0x01;
            byte[] buffer2 = new byte[2];
            try
            {
                lock (I2cLock)
                {
                    if (mode == 0) { DLNi2c[ch + 1].Read(0x40, 1, RegAddr, buffer2); } // AF
                    else DLNi2c[ch].Read(0x41, 1, RegAddr, buffer2);
                }
                res = (buffer2[0] * 256 + buffer2[1]) / 10.0 + 10;
            }
            catch
            {
                STATIC.I2CFailcnt++;
                if (STATIC.I2CFailcnt > 20)
                {
                    SetError("Get Current NG");
                }
                return 0;
            }
            return res;
        }

        public bool WriteArray(int ch, int slaveAddr, int memAddr, byte[] data)
        {

            try
            {
                if (Process.IsVirtual) return true;
                lock (I2cLock)
                {
                    if (DLNi2c[ch] != null) DLNi2c[ch].Write(slaveAddr, 2, memAddr, data);
                }

                return true;
            }
            catch
            {
                if (STATIC.I2CFailcnt > 21)
                {
                    STATIC.I2CFailcnt = 0;
                    return false;
                }
                STATIC.I2CFailcnt++;
                if (STATIC.I2CFailcnt > 20)
                {
                    SetError("Dln WriteFail");
                }
                return false;
            }
        }
        public bool WriteArray(int ch, int slaveAddr, byte[] data)
        {

            try
            {
                if (Process.IsVirtual) return true;
                lock (I2cLock)
                {
                    if (DLNi2c[ch] != null) DLNi2c[ch].Write(slaveAddr, data);
                }

                return true;
            }
            catch
            {
                if (STATIC.I2CFailcnt > 21)
                {
                    STATIC.I2CFailcnt = 0;
                    return false;
                }
                STATIC.I2CFailcnt++;
                if (STATIC.I2CFailcnt > 20)
                {
                    SetError("Dln WriteFail");
                }
                return false;
            }
        }        

        public bool ReadArray(int ch, int slaveAddr, int memAddr, byte[] data)
        {

            try
            {
                if (Process.IsVirtual) return true;
                lock (I2cLock)
                {
                    if (DLNi2c[ch] != null) DLNi2c[ch].Read(slaveAddr, 2, memAddr, data);
                }

                return true;
            }
            catch
            {
                STATIC.I2CFailcnt++;
                if (STATIC.I2CFailcnt > 20)
                {
                    SetError("Dln ReadFail");

                }
                return false;
            }
        }
        public bool ReadArray(int ch, int slaveAddr, byte[] data)
        {

            try
            {
                if (Process.IsVirtual) return true;
                lock (I2cLock)
                {
                    if (DLNi2c[ch] != null) DLNi2c[ch].Read(slaveAddr, data);
                }

                return true;
            }
            catch
            {
                STATIC.I2CFailcnt++;
                if (STATIC.I2CFailcnt > 20)
                {
                    SetError("Dln ReadFail");

                }
                return false;
            }
        }

        public bool WriteByte(int ch, int slaveAddr, int memAddr, int memCnt, byte data)
        {
            byte[] tmp = new byte[] { data };

            try
            {
                if (Process.IsVirtual) return true;
                lock (I2cLock) { if (DLNi2c[ch] != null) DLNi2c[ch].Write(slaveAddr, memCnt, memAddr, tmp); }
                return true;
            }
            catch
            {
                STATIC.I2CFailcnt++;
                if (STATIC.I2CFailcnt > 20)
                {
                    SetError("Dln WriteFail");
                }
                return false;
            }
        }
        public bool Write2Byte(int ch, int slaveAddr, int memAddr, int memCnt, ushort data)
        {
            byte[] tmp = { 0, 0 };

            tmp[0] = (byte)(data & 0xff);
            tmp[1] = (byte)((data >> 8) & 0xff);

            try
            {
                if (Process.IsVirtual) return true;
                lock (I2cLock) { if (DLNi2c[ch] != null) DLNi2c[ch].Write(slaveAddr, memCnt, memAddr, tmp); }
                return true;
            }
            catch
            {
                STATIC.I2CFailcnt++;
                if (STATIC.I2CFailcnt > 20)
                {
                    SetError("Dln WriteFail");
                }
                return false;
            }
        }
        public bool Write2Byte(int ch, int slaveAddr, int memAddr, int memCnt, short data)
        {
            byte[] tmp = { 0, 0 };

            tmp[0] = (byte)(data & 0xff);
            tmp[1] = (byte)((data >> 8) & 0xff);

            try
            {
                if (Process.IsVirtual) return true;
                lock (I2cLock) { if (DLNi2c[ch] != null) DLNi2c[ch].Write(slaveAddr, memCnt, memAddr, tmp); }
                return true;
            }
            catch
            {
                STATIC.I2CFailcnt++;
                if (STATIC.I2CFailcnt > 20)
                {
                    SetError("Dln WriteFail");
                }
                return false;
            }
        }
        public bool Write4Byte(int ch, int slaveAddr, int memAddr, int memCnt, uint data)
        {
            byte[] tmp = { 0, 0, 0, 0 };

            tmp[0] = (byte)(data & 0xff);
            tmp[1] = (byte)((data >> 8) & 0xff);
            tmp[2] = (byte)((data >> 16) & 0xff);
            tmp[3] = (byte)((data >> 24) & 0xff); 
            
            try
            {
                if (Process.IsVirtual) return true;
                lock (I2cLock) { if (DLNi2c[ch] != null) DLNi2c[ch].Write(slaveAddr, memCnt, memAddr, tmp); }
                return true;
            }
            catch
            {
                STATIC.I2CFailcnt++;
                if (STATIC.I2CFailcnt > 20)
                {
                    SetError("Dln WriteFail");
                }
                return false;
            }
        }
        public bool Write4Byte(int ch, int slaveAddr, int memAddr, int memCnt, int data)
        {
            byte[] tmp = { 0, 0, 0, 0 };

            tmp[0] = (byte)(data & 0xff);
            tmp[1] = (byte)((data >> 8) & 0xff);
            tmp[2] = (byte)((data >> 16) & 0xff);
            tmp[3] = (byte)((data >> 24) & 0xff);

            try
            {
                if (Process.IsVirtual) return true;
                lock (I2cLock) { if (DLNi2c[ch] != null) DLNi2c[ch].Write(slaveAddr, memCnt, memAddr, tmp); }
                return true;
            }
            catch
            {
                STATIC.I2CFailcnt++;
                if (STATIC.I2CFailcnt > 20)
                {
                    SetError("Dln WriteFail");
                }
                return false;
            }
        }

        public byte? ReadByteNull(int ch, int slaveAddr, int memAddr, int memCnt)
        {
            byte[] tmp = new byte[1];
            try
            {
                if (Process.IsVirtual) return null;
                lock (I2cLock)
                {
                    if (DLNi2c[ch] != null) DLNi2c[ch].Read(slaveAddr, memCnt, memAddr, tmp);
                }
                return tmp[0];
            }
            catch
            {
                STATIC.I2CFailcnt++;
                if (STATIC.I2CFailcnt > 20)
                {
                    SetError("Dln ReadFail");

                }
                return null;
            }
        }
        public byte ReadByte(int ch, int slaveAddr, int memAddr, int memCnt)
        {
            byte[] tmp = new byte[1];
            try
            {
                if (Process.IsVirtual) return tmp[0];
                lock (I2cLock)
                {
                    if (DLNi2c[ch] != null) DLNi2c[ch].Read(slaveAddr, memCnt, memAddr, tmp);
                }
                return tmp[0];
            }
            catch
            {
                STATIC.I2CFailcnt++;
                if (STATIC.I2CFailcnt > 20)
                {
                    SetError("Dln ReadFail");

                }
                return byte.MaxValue;
            }
        }
        public ushort Read2Byte(int ch, int slaveAddr, int memAddr, int memCnt)
        {
            byte[] tmp = new byte[2];
            try
            {
                if (Process.IsVirtual) return tmp[0];
                lock (I2cLock)
                {
                    if (DLNi2c[ch] != null) DLNi2c[ch].Read(slaveAddr, memCnt, memAddr, tmp);
                }
                return (ushort)(tmp[0] + (tmp[1] << 8));
            }
            catch
            {
                STATIC.I2CFailcnt++;
                if (STATIC.I2CFailcnt > 20)
                {
                    SetError("Dln ReadFail");

                }
                return ushort.MaxValue;
            }
        }
        public short Read2Byte_signed(int ch, int slaveAddr, int memAddr, int memCnt)
        {
            byte[] tmp = new byte[2];
            try
            {
                if (Process.IsVirtual) return tmp[0];
                lock (I2cLock)
                {
                    if (DLNi2c[ch] != null) DLNi2c[ch].Read(slaveAddr, memCnt, memAddr, tmp);
                }
                return (short)(tmp[0] + (tmp[1] << 8));
            }
            catch
            {
                STATIC.I2CFailcnt++;
                if (STATIC.I2CFailcnt > 20)
                {
                    SetError("Dln ReadFail");

                }
                return short.MinValue;
            }
        }
        public uint Read4Byte(int ch, int slaveAddr, int memAddr, int memCnt)
        {
            byte[] tmp = new byte[4];
            try
            {
                if (Process.IsVirtual) return tmp[0];
                lock (I2cLock)
                {
                    if (DLNi2c[ch] != null) DLNi2c[ch].Read(slaveAddr, memCnt, memAddr, tmp);
                }
                return (uint)(tmp[0] | (tmp[1] << 8) | (tmp[2] << 16) | (tmp[3] << 24));
            }
            catch
            {
                STATIC.I2CFailcnt++;
                if (STATIC.I2CFailcnt > 20)
                {
                    SetError("Dln ReadFail");

                }
                return uint.MinValue;
            }
        }
        public int Read4Byte_signed(int ch, int slaveAddr, int memAddr, int memCnt)
        {
            byte[] tmp = new byte[4];
            try
            {
                if (Process.IsVirtual) return tmp[0];
                lock (I2cLock)
                {
                    if (DLNi2c[ch] != null) DLNi2c[ch].Read(slaveAddr, memCnt, memAddr, tmp);
                }
                return (int)(tmp[0] | (tmp[1] << 8) | (tmp[2] << 16) | (tmp[3] << 24));
            }
            catch
            {
                STATIC.I2CFailcnt++;
                if (STATIC.I2CFailcnt > 20)
                {
                    SetError("Dln ReadFail");

                }
                return int.MinValue;
            }
        }

        public void SetSocketSensor(bool isOn)
        {
            try
            {
                if (isOn)
                {
                    if (Process.IsVirtual) return;
                    lock (I2cLock)
                    {
                        DLNgpio[1].Pins[12].Enabled = true;
                        DLNgpio[1].Pins[12].Direction = 0;   //  0 ~ 15 : 0(in), 24 ~ 31 : 1(out)
                        DLNgpio[1].Pins[12].PulldownEnabled = true;
                        DLNgpio[1].Pins[13].Enabled = true;
                        DLNgpio[1].Pins[13].Direction = 0;   //  0 ~ 15 : 0(in), 24 ~ 31 : 1(out)
                        DLNgpio[1].Pins[13].PulldownEnabled = true;

                    }

                }

            }
            catch
            {
                SetError("Socket Sensor I2C NG");

            }
        }
        public bool GetGpioStatus(int input)
        {

            try
            {
                if (Process.IsVirtual) return true;
                lock (I2cLock)
                {
                    if (DLNgpio[1].Pins[input].Value == 1) return true;
                    else return false;
                }
            }
            catch
            {
                SetError("Socket Sensor I2C NG");

                return false;
            }

        }

        public void PeakDetector(int ADCNumber, PeakDetectState state)
        {
            try
            {
                if (Process.IsVirtual) return;
                lock (I2cLock)
                {
                    var targetPin = GetOutputPin(ADCNumber);

                    switch (state)
                    {
                        case PeakDetectState.Hold:
                            targetPin[0].OutputValue = 0;
                            targetPin[1].OutputValue = 0;
                            break;
                        case PeakDetectState.Detect:
                            targetPin[0].OutputValue = 0;
                            targetPin[1].OutputValue = 1;
                            break;
                        case PeakDetectState.Reset:
                            targetPin[0].OutputValue = 1;
                            targetPin[1].OutputValue = 1;
                            break;
                        default:
                            targetPin[0].OutputValue = 1;
                            targetPin[1].OutputValue = 1;
                            break;
                    }
                }
            }
            catch
            {
                SetError("Socket Sensor I2C NG");
                return;
            }
        }

        private List<Pin> GetOutputPin(int ADCNumber)
        {
            List<Pin> pins = new List<Pin>();

            if (ADCNumber == 0) //OIS PIN
            {
                pins.Add(DLNgpio[1].Pins[(int)GPIONumber.PeakDetector_DetectADC2]);
                pins.Add(DLNgpio[1].Pins[(int)GPIONumber.PeakDetector_ResetADC2]);
            }
            else                //AF PIN
            {
                pins.Add(DLNgpio[1].Pins[(int)GPIONumber.PeakDetector_DetectADC1]);
                pins.Add(DLNgpio[1].Pins[(int)GPIONumber.PeakDetector_ResetADC1]);
            }

            return pins;
        }
    }
}
