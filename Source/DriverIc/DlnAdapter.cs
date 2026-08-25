using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ4P
{
    public class DlnAdapter : IDlnInterface
    {
        private readonly DLN _dln;
        public DlnAdapter(DLN dln) { _dln = dln; }

        public bool IsRun { get => _dln.IsRun; set => _dln.IsRun = value; }
        public bool IsSafeOn { get => _dln.IsSafeOn; set => _dln.IsSafeOn = value; }
        public bool isMoving { get => _dln.isMoving; set => _dln.isMoving = value; }
        public bool m_bOccupied { get => _dln.m_bOccupied; set => _dln.m_bOccupied = value; }
        public uint PortCount => _dln.m_PortCount;

        public event EventHandler SwitchOn { add => _dln.SwitchOn += value; remove => _dln.SwitchOn -= value; }
        public event EventHandler SafetyOn { add => _dln.SafetyOn += value; remove => _dln.SafetyOn -= value; }

        public bool Init() => _dln.Init();
        public void SetError(string s) => _dln.SetError(s);
        public void PowerOnOff(int port, bool IsOn = true) => _dln.PowerOnOff(port, IsOn);
        public void LoadSocket() => _dln.LoadSocket();
        public void UnloadSocket() => _dln.UnloadSocket();
        public void CoverDn() => _dln.CoverDn();
        public void CoverUp() => _dln.CoverUp();
        public void SetSocketSensor(bool isOn) => _dln.SetSocketSensor(isOn);
        public bool GetGpioStatus(int input) => _dln.GetGpioStatus(input);

        // I2C Write 매핑
        public bool WriteByte(int ch, int slaveAddr, int memAddr, int memCnt, byte data) => _dln.WriteByte(ch, slaveAddr, memAddr, memCnt, data);
        public bool Write2Byte(int ch, int slaveAddr, int memAddr, int memCnt, ushort data) => _dln.Write2Byte(ch, slaveAddr, memAddr, memCnt, data);
        public bool Write2Byte(int ch, int slaveAddr, int memAddr, int memCnt, short data) => _dln.Write2Byte(ch, slaveAddr, memAddr, memCnt, data);
        public bool Write4Byte(int ch, int slaveAddr, int memAddr, int memCnt, uint data) => _dln.Write4Byte(ch, slaveAddr, memAddr, memCnt, data);
        public bool Write4Byte(int ch, int slaveAddr, int memAddr, int memCnt, int data) => _dln.Write4Byte(ch, slaveAddr, memAddr, memCnt, data);
        public bool WriteArray(int ch, int slaveAddr, int memAddr, byte[] data) => _dln.WriteArray(ch, slaveAddr, memAddr, data);

        // I2C Read 매핑
        public byte ReadByte(int ch, int slaveAddr, int memAddr, int memCnt) => _dln.ReadByte(ch, slaveAddr, memAddr, memCnt);
        public byte? ReadByteNull(int ch, int slaveAddr, int memAddr, int memCnt) => _dln.ReadByteNull(ch, slaveAddr, memAddr, memCnt);
        public ushort Read2Byte(int ch, int slaveAddr, int memAddr, int memCnt) => _dln.Read2Byte(ch, slaveAddr, memAddr, memCnt);
        public short Read2Byte_signed(int ch, int slaveAddr, int memAddr, int memCnt) => _dln.Read2Byte_signed(ch, slaveAddr, memAddr, memCnt);
        public uint Read4Byte(int ch, int slaveAddr, int memAddr, int memCnt) => _dln.Read4Byte(ch, slaveAddr, memAddr, memCnt);
        public int Read4Byte_signed(int ch, int slaveAddr, int memAddr, int memCnt) => _dln.Read4Byte_signed(ch, slaveAddr, memAddr, memCnt);
        public bool ReadArray(int ch, int slaveAddr, int memAddr, byte[] data) => _dln.ReadArray(ch, slaveAddr, memAddr, data);

        public double GetCurrent(int ch, int mode) => _dln.GetCurrent(ch, mode);
        public void SetLEDpower(int id, int value) => _dln.SetLEDpower(id, value);

        public byte[] RunInternalSequence(string cmd, byte[] payload = null)
        {
            return new byte[0]; // 내부 시퀀스는 DLN에서 직접 구현하지 않으므로 빈 배열 반환
        }
        public bool ChangeSlaveAddrUnified(int ch, byte origin, byte target, byte pinMode, bool isAF)
        {
            // (기존 ChangeSlaveAddrOIS 로직을 재활용)
            return true;
        }

        public void PeakDetector(int ADCNumber, PeakDetectState state)
        {
            _dln.PeakDetector(ADCNumber, state);
        }

        public bool WriteArray(int ch, int slaveAddr, byte[] data)
        {
            return _dln.WriteArray(ch, slaveAddr, data);
        }

        public bool ReadArray(int ch, int slaveAddr, byte[] data)
        {
            return _dln.ReadArray(ch, slaveAddr, data);
        }
    }
}
