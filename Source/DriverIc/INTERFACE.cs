using Dln;
using FZ4P.Commons.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FZ4P
{
    public enum PeakDetectState
    {
        Hold = 0,
        Detect = 1,
        Reset = 2,
        InputFollowing = 3,
    }
    public interface IGPIOOutput
    {
        void PowerOnOff(int port, bool IsOn = true);

        // 소켓 및 커버
        void LoadSocket();
        void UnloadSocket();
        void CoverDn();
        void CoverUp();
        void SetSocketSensor(bool isOn);
        bool GetGpioStatus(int input);
        void PeakDetector(int ADCNumber, PeakDetectState state);
    }
    public interface IDlnInterface : IGPIOOutput
    {
        // 필드 및 속성
        bool IsRun { get; set; }
        bool IsSafeOn { get; set; }
        bool isMoving { get; set; }
        bool m_bOccupied { get; set; }
        uint PortCount { get; }

        // 이벤트
        event EventHandler SwitchOn;
        event EventHandler SafetyOn;

        // 기본 제어
        bool Init();
        void SetError(string s);

        // I2C Write 계열 (누락된 모든 타입 추가)
        bool WriteByte(int ch, int slaveAddr, int memAddr, int memCnt, byte data);
        bool Write2Byte(int ch, int slaveAddr, int memAddr, int memCnt, ushort data);
        bool Write2Byte(int ch, int slaveAddr, int memAddr, int memCnt, short data);
        bool Write4Byte(int ch, int slaveAddr, int memAddr, int memCnt, uint data);
        bool Write4Byte(int ch, int slaveAddr, int memAddr, int memCnt, int data);
        bool WriteArray(int ch, int slaveAddr, int memAddr, byte[] data);
        bool WriteArray(int ch, int slaveAddr, byte[] data);

        // I2C Read 계열 (누락된 모든 타입 추가)
        byte ReadByte(int ch, int slaveAddr, int memAddr, int memCnt);
        byte? ReadByteNull(int ch, int slaveAddr, int memAddr, int memCnt);
        ushort Read2Byte(int ch, int slaveAddr, int memAddr, int memCnt);
        short Read2Byte_signed(int ch, int slaveAddr, int memAddr, int memCnt);
        uint Read4Byte(int ch, int slaveAddr, int memAddr, int memCnt);
        int Read4Byte_signed(int ch, int slaveAddr, int memAddr, int memCnt);
        bool ReadArray(int ch, int slaveAddr, int memAddr, byte[] data);
        bool ReadArray(int ch, int slaveAddr, byte[] data);

        // 기타
        double GetCurrent(int ch, int mode);
        void SetLEDpower(int id, int value);

        byte[] RunInternalSequence(string cmd, byte[] payload = null);
        bool ChangeSlaveAddrUnified(int ch, byte origin, byte target, byte pinMode, bool isAF);
    }
}
