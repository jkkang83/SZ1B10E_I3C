using FZ4P;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;

namespace FZ4P
{
    public class Esp32WifiDevice : IDlnInterface
    {
        private readonly string _ip;
        private readonly int _port;
        private TcpClient _client;
        private NetworkStream _stream;
        private readonly object _commLock = new object();
        private readonly object _i2cLock = new object();

        public Esp32WifiDevice(string ipAddress, int port = 8080)
        {
            _ip = ipAddress;
            _port = port;
            // 만약 실패하더라도 객체 생성 자체가 죽지 않도록 내부에서 예외처리가 된 Init을 호출합니다.
            if (!Init())
            {
                // 연결 실패 시 로그만 남기고, 나중에 메인 UI에서 재시도할 수 있게 함
                STATIC.Process.AddLog(1, "[ESP32] Initial connection failed. Check WiFi or Power.");
            }
        }

        public bool IsRun { get; set; }
        public bool IsSafeOn { get; set; }
        public bool isMoving { get; set; }
        public bool m_bOccupied { get; set; }
        public uint PortCount => 1;

        public event EventHandler SwitchOn;
        public event EventHandler SafetyOn;

        #region TCP 통신 엔진 (Header 4byte + Contents)
        private byte[] SendAndReceive(string cmdId, byte[] payload = null, bool isInitCall = false)
        {
            int retry = 0;
            int maxRetry = isInitCall ? 0 : 1;

            while (retry <= maxRetry)
            {
                lock (_commLock)
                {
                    try
                    {
                        // 1. 연결 확인 (기존 로직 유지)
                        if (!isInitCall && (_client == null || !_client.Connected || _stream == null))
                        {
                            if (!Init()) { retry++; continue; }
                        }

                        // 2. 패킷 구성 최적화 (한 번의 할당으로 처리)
                        byte[] cmdBytes = System.Text.Encoding.ASCII.GetBytes(cmdId);
                        int payloadLen = payload?.Length ?? 0;
                        int totalContentLen = cmdBytes.Length + payloadLen;

                        // [핵심] 전체 패킷(헤더 4 + 본문)을 하나의 배열로 생성
                        byte[] sendBuffer = new byte[4 + totalContentLen];

                        // 헤더 채우기 (Little Endian)
                        Buffer.BlockCopy(BitConverter.GetBytes(totalContentLen), 0, sendBuffer, 0, 4);
                        // 커맨드 채우기
                        Buffer.BlockCopy(cmdBytes, 0, sendBuffer, 4, cmdBytes.Length);
                        // 페이로드 채우기
                        if (payloadLen > 0)
                            Buffer.BlockCopy(payload, 0, sendBuffer, 4 + cmdBytes.Length, payloadLen);

                        // [핵심] 단 한 번의 전송으로 Nagle 지연 방지
                        _stream.Write(sendBuffer, 0, sendBuffer.Length);
                        // _stream.Flush(); // 필요시 명시적 호출

                        // 3. 응답 읽기 (기존 로직이 이미 잘 짜여 있음)
                        byte[] resHeader = new byte[4];
                        if (!ReadExact(_stream, resHeader, 4)) throw new Exception("Header Read Fail");

                        int resLen = BitConverter.ToInt32(resHeader, 0);
                        if (resLen < 0 || resLen > 8192) throw new Exception($"Invalid len: {resLen}");

                        byte[] resContents = new byte[resLen];
                        if (resLen > 0)
                        {
                            if (!ReadExact(_stream, resContents, resLen)) throw new Exception("Content Read Fail");
                        }

                        return resContents;
                    }
                    catch (Exception ex)
                    {
                        if (isInitCall) throw; // Init 중 발생한 에러는 상위(Init함수 내부)로 던짐

                        retry++;
                        if (retry <= 1)
                        {
                            STATIC.Process.AddLog(0, $"[ESP32] Comm Error: {ex.Message}. Retrying...");
                            CloseConnection(); // 스트림과 클라이언트 정리
                            Thread.Sleep(500); // 칩 안정화 대기
                                               // continue를 통해 다시 while 루프 상단으로 가서 Init()부터 시작
                        }
                        else
                        {
                            SetError($"Comm Fail after retry: {ex.Message}");
                            return null;
                        }
                    }
                }
            }
            return null;
        }
        // 헬퍼 함수: 지정된 바이트를 다 읽을 때까지 대기
        private bool ReadExact(Stream stream, byte[] buffer, int length)
        {
            int totalRead = 0;
            while (totalRead < length)
            {
                int read = stream.Read(buffer, totalRead, length - totalRead);
                if (read <= 0) return false;
                totalRead += read;
            }
            return true;
        }
        #endregion

        #region GPIO & Power 제어 (기존 DLN 로직 이식)

        // 기존 DLN의 PowerOnOff는 9번 핀의 Direction을 제어함.
        // ESP32에서는 9번 핀에 연결된 전원 회로를 High/Low로 직접 제어한다고 매핑.
        public void PowerOnOff(int port, bool IsOn = true)
        {
            lock (_i2cLock)
            {
                STATIC.Process.AddLog(0, IsOn ? "Power On" : "Power Off");

                // 최신 핀 맵의 LDO ON/OFF(PB1-0)인 13번 핀을 제어합니다.
                SendAndReceive("O13", new byte[] { (byte)(IsOn ? 1 : 0) });
                //SendAndReceive("O13", new byte[] { (byte)(IsOn ? 0 : 1) });
            }
        }

        // 실린더/소켓 제어: 기존 DLN의 핀 조합 로직을 그대로 유지하며 통신만 변경
        public void LoadSocket()
        {
            lock (_i2cLock)
            {
                SendAndReceive("O25", new byte[] { 1 }); // Pins[10] = 1
                SendAndReceive("O32", new byte[] { 0 }); // Pins[20] = 0
            }
        }

        public void UnloadSocket()
        {
            lock (_i2cLock)
            {
                SendAndReceive("O25", new byte[] { 0 }); // Pins[10] = 0
                SendAndReceive("O32", new byte[] { 1 }); // Pins[20] = 1
            }
        }

        public void CoverDn()
        {
            lock (_i2cLock)
            {
                SendAndReceive("O33", new byte[] { 0 }); // Pins[11] = 1
                SendAndReceive("O26", new byte[] { 1 }); // Pins[21] = 0
            }
        }

        public void CoverUp()
        {
            lock (_i2cLock)
            {
                SendAndReceive("O33", new byte[] { 1 }); // Pins[11] = 0
                SendAndReceive("O26", new byte[] { 0 }); // Pins[21] = 1
            }
        }

        public bool GetGpioStatus(int input)
        {
            int realPin = input;

            // --- [DLN 핀 번호를 ESP32 실제 핀으로 컨버팅] ---
            // 기존 DLN 12번 (Load 센서) -> ESP32 실크 34번
            // 기존 DLN 13번 (Unload 센서) -> ESP32 실크 39번 (VN)
            if (input == 12) 
                realPin = 34;
            else if (input == 13) 
                realPin = 39;

            // 만약 SW0(PB0-0)가 기존에 8번이었다면 아래처럼 추가 매핑
            // else if (input == 8) realPin = 36; // VP

            var res = SendAndReceive($"I{realPin}");

            // 센서 특성(Active High/Low)에 따라 반전이 필요하면 여기서 처리 가능합니다.
            return res != null && res[0] != 1;
            //return res != null && res[0] == 1;
        }

        public void SetSocketSensor(bool isOn)
        {
            if (isOn)
            {
                // 기존 로직: 12, 13번 핀을 입력 모드로 설정 (ESP32 내부 설정 권장)
                STATIC.Process.AddLog(0, "Socket Sensor Set On");
            }
        }
        #endregion

        #region I2C 통신 (8bit / 16bit / 4Byte)

        public bool WriteByte(int ch, int slave, int addr, int cnt, byte data)
        {
            var res = SendAndReceive("W1", new byte[] { (byte)slave, (byte)addr, data, 0, 0 });
            // 응답이 있고, 그 값이 1(성공)일 때만 true
            return res != null && res.Length > 0 && res[0] == 1;
        }
        public byte ReadByte(int ch, int slave, int addr, int cnt)
        {
            var res = SendAndReceive("R1", new byte[] { (byte)slave, (byte)addr, 0, 0, 0 });
            // 장치가 없어서 null이 오거나 응답이 없으면 byte.MaxValue(255) 리턴
            if (res == null || res.Length == 0) return byte.MaxValue;
            return res[0];
        }

        public bool WriteArray(int ch, int slave, int addr, byte[] data)
        {
            List<byte> p = new List<byte> { (byte)slave, (byte)(addr & 0xFF), (byte)(addr >> 8), (byte)(data.Length & 0xFF), (byte)(data.Length >> 8) };
            p.AddRange(data);

            var res = SendAndReceive("WC", p.ToArray());

            // [수정] 응답이 있고, 첫 바이트가 1(성공)일 때만 true 리턴
            return res != null && res.Length > 0 && res[0] == 1;
        }

        public bool ReadArray(int ch, int slave, int addr, byte[] data)
        {
            byte[] p = { (byte)slave, (byte)(addr & 0xFF), (byte)(addr >> 8), (byte)(data.Length & 0xFF), (byte)(data.Length >> 8) };
            var res = SendAndReceive("RC", p);
            if (res != null) { Array.Copy(res, data, Math.Min(res.Length, data.Length)); return true; }
            return false;
        }

        public uint Read4Byte(int ch, int slave, int addr, int cnt)
        {
            byte[] b = new byte[4];
            return ReadArray(ch, slave, addr, b) ? (uint)(b[0] << 24 | b[1] << 16 | b[2] << 8 | b[3]) : uint.MaxValue;
        }

        public bool Write4Byte(int ch, int slave, int addr, int cnt, uint data)
            => WriteArray(ch, slave, addr, new byte[] { (byte)(data >> 24), (byte)(data >> 16), (byte)(data >> 8), (byte)data });

        public double GetCurrent(int ch, int mode)
        {
            int slave = (mode == 0) ? 0x40 : 0x41;
            var res = SendAndReceive("R2", new byte[] { (byte)slave, 0x01, 0, 0, 0 });

            // [수정] 응답이 null이면(장치 없음) 0을 리턴
            if (res == null || res.Length < 2) return 0;

            return ((res[0] << 8 | res[1]) / 10.0 + 10);
        }

        public void SetLEDpower(int id, int value)
        {
            if (STATIC.LightDln != null)
            {
                STATIC.LightDln.SetLEDpower(id, value);
            }
        }
        #endregion
        public bool Init()
        {
            try
            {
                // 1. 디바운스 로직 초기화 (기존 동일)
                debounceLogics = new[] {
            new SignalDebounceLogic(),
            new SignalDebounceLogic()
        };

                foreach (var logic in debounceLogics)
                {
                    logic.SignalChanged += SignalChanged;
                }

                // 2. [중요] 물리적 TCP 소켓 연결을 여기서 먼저 수행
                // SendAndReceive 내부에서 Init을 부르는 무한루프를 끊기 위함입니다.
                if (!ConnectEngine())
                {
                    return false;
                }

                // 3. 무선 ESP32 핸드쉐이크
                // 세 번째 인자에 true를 전달하여 SendAndReceive 내부에서 다시 Init을 부르지 않게 합니다.
                var response = SendAndReceive("IN", null, true);

                // 4. 응답 검증 및 결과 리턴
                if (response != null && response.Length > 0 && response[0] == 1)
                {
                    STATIC.Process.AddLog(0, "[ESP32] IO Connection established successfully.");
                    StartSwWatcher();
                    return true;
                }

                SetError("ESP32 Handshake Failed (Invalid response)");
                return false;
            }
            catch (Exception ex)
            {
                SetError($"Init Exception: {ex.Message}");
                return false;
            }
        }

        // 저수준 소켓 연결 전용 함수 분리
        private bool ConnectEngine()
        {
            try
            {
                CloseConnection(); // 기존 쓰레기 연결 정리

                _client = new TcpClient();
                _client.NoDelay = true;
                var result = _client.BeginConnect(_ip, _port, null, null);

                if (!result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(2)))
                {
                    SetError("Connect Timeout");
                    return false;
                }

                _client.EndConnect(result);
                _stream = _client.GetStream();
                _stream.ReadTimeout = 2000;
                _stream.WriteTimeout = 2000;

                return true;
            }
            catch (Exception ex)
            {
                SetError($"ConnectEngine Fail: {ex.Message}");
                return false;
            }
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
        private Thread _swWatcher;
        private bool _lastSwState = false;
        private bool _isSwitchActive = false; // 기존 IsSwitch 역할

        // Init() 성공 시 또는 생성자에서 호출
        public void StartSwWatcher()
        {
            if (_swWatcher != null && _swWatcher.IsAlive) return;

            // [수정] 스레드 시작 전 현재 상태를 미리 읽어 초기화합니다.
            _lastSwState = GetGpioStatus(36);
            // 만약 Active High 스위치라면 초기 Active 상태도 맞춰줍니다.
            _isSwitchActive = _lastSwState;

            _swWatcher = new Thread(() =>
            {
                // _client가 살아있는 동안 계속 돌지만, 내부에서 연결 상태를 상시 체크
                while (true)
                {
                    try
                    {
                        // 연결이 끊겨있으면 1초 대기 후 다시 체크 (SendAndReceive가 살려낼 때까지 대기)
                        if (_client == null || !_client.Connected || _stream == null)
                        {
                            Thread.Sleep(1000);
                            continue;
                        }

                        bool currentState = GetGpioStatus(36);
                        if (currentState != _lastSwState)
                        {
                            _lastSwState = currentState;
                            if (currentState == true && !_isSwitchActive)
                            {
                                _isSwitchActive = true;
                                Debounce(1, TargetSignalType.Start, false);
                                SwitchOn?.Invoke(this, EventArgs.Empty);
                                STATIC.Process.AddLog(0, "[ESP32] Switch On Pressed");
                            }
                            else if (currentState == false && _isSwitchActive)
                            {
                                _isSwitchActive = false;
                                Debounce(1, TargetSignalType.Start, true);
                                STATIC.Process.AddLog(0, "[ESP32] Switch Off Released");
                            }
                        }
                        Thread.Sleep(100); // 부하 감소
                    }
                    catch
                    {
                        Thread.Sleep(1000); // 에러 발생 시 잠시 대기
                    }
                }
            });
            _swWatcher.IsBackground = true;
            _swWatcher.Start();
        }
        private SignalDebounceLogic[] debounceLogics;
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
        public void SetError(string s) => STATIC.Process.AddLog(0, $"[ESP32] {s}");
        public byte? ReadByteNull(int ch, int slave, int addr, int cnt) => ReadByte(ch, slave, addr, cnt);
        public ushort Read2Byte(int ch, int slave, int addr, int cnt) { var res = SendAndReceive("R2", new byte[] { (byte)slave, (byte)addr, 0, 0, 0 }); return res != null ? (ushort)(res[0] << 8 | res[1]) : ushort.MaxValue; }
        public bool Write2Byte(int ch, int slave, int addr, int cnt, ushort data) => SendAndReceive("W2", new byte[] { (byte)slave, (byte)addr, (byte)(data >> 8), (byte)data, 0 }) != null;
        public short Read2Byte_signed(int ch, int slave, int addr, int cnt) => (short)Read2Byte(ch, slave, addr, cnt);
        public int Read4Byte_signed(int ch, int slave, int addr, int cnt) => (int)Read4Byte(ch, slave, addr, cnt);
        public bool Write2Byte(int ch, int slave, int addr, int cnt, short data) => Write2Byte(ch, slave, addr, cnt, (ushort)data);
        public bool Write4Byte(int ch, int slave, int addr, int cnt, int data) => Write4Byte(ch, slave, addr, cnt, (uint)data);

        public byte[] RunInternalSequence(string cmd, byte[] payload = null)
        {
            lock (_commLock)
            {
                if (_stream == null) { if (!Init()) return null; }

                int originalTimeout = _stream.ReadTimeout;
                try
                {
                    switch (cmd)
                    {
                        default: _stream.ReadTimeout = 2000; break;
                    }

                    // 내부에서 자동으로 1회 재접속 시도함
                    return SendAndReceive(cmd, payload);
                }
                finally
                {
                    if (_stream != null) _stream.ReadTimeout = originalTimeout;
                }
            }
        }

        // CloseConnection 함수 내부의 변수명도 _client로 통일해주세요!
        private void CloseConnection()
        {
            try
            {
                _stream?.Close();
                _client?.Close(); // _tcpClient 아님
                _stream = null;
                _client = null;
            }
            catch { }
        }
        public bool ChangeSlaveAddrUnified(int ch, byte origin, byte target, byte pinMode, bool isAF)
        {
            // 1. ESP32 시퀀스 호출 (커맨드 "UA")
            byte[] payload = { origin, target, pinMode, (byte)(isAF ? 1 : 0) };

            // 통신 잠금 및 재접속 로직이 포함된 SendAndReceive 사용
            var res = SendAndReceive("UA", payload);

            if (res == null || res.Length < 2 || res[0] == 0)
            {
                SetError(isAF ? "I2C address change NG(check error)" : $"OIS SlaveAddr Change Failed.");
                return false;
            }

            byte foundAddr = res[1];

            // 2. 로그 출력 (기존 PC 로직과 동일하게 유지)
            if (isAF)
            {
                STATIC.Process.AddLog(ch, "IC Address check OK");
                STATIC.Process.AddLog(ch, $"I2C address change from 0x{foundAddr:X2} to 0x{(target << 1):X2}");
            }
            else
            {
                string label = (pinMode == 0x02) ? "X" : "Y";
                STATIC.Process.AddLog(ch, $"Setting Mode = Write Mem : 0xAE Data : 0x3B");
                STATIC.Process.AddLog(ch, $"Set Pin Mode = Write Mem : 0x0B Data : 0x{pinMode:X2}");
                STATIC.Process.AddLog(ch, $"Setting Slave Address = Write Mem : 0x0A Data : 0x{target:X2}");
                STATIC.Process.AddLog(ch, $"Store Memory = Write Mem : 0x03 Data : 0x01");
                STATIC.Process.AddLog(ch, $"{label} SlaveAddr Change Finished.");
            }

            return true;
        }

        public void PeakDetector(int ADCNumber, PeakDetectState state)
        {
            throw new NotImplementedException();
        }

        public bool WriteArray(int ch, int slaveAddr, byte[] data)
        {
            throw new NotImplementedException();
        }

        public bool ReadArray(int ch, int slaveAddr, byte[] data)
        {
            throw new NotImplementedException();
        }
    }
}