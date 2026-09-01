using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace FZ4P.DriverIc.Adapter
{
    /// <summary>
    /// RA8M1 I3C bridge adapter.
    ///
    /// TCP frame:
    ///   Request  : [BodyLength:4 LE][ASCII Command][Payload]
    ///   Response : [BodyLength:4 LE][Payload]
    ///
    /// Commands:
    ///   IN : Initialize
    ///   PG : Ping
    ///   VR : Version
    ///   WX : Register/Raw Write
    ///   RX : Register/Raw Read
    ///   WN : Register/Raw Write without response
    ///   DA : ENTDAA
    ///   DT : Device Table
    ///   BR : I3C Bus Reset
    ///   UA : Legacy slave address change
    ///
    /// WX/WN payload:
    ///   [ch:1][slave:1][memCnt:1][memAddr:4 LE][dataLen:2 LE][data:N]
    ///
    /// RX payload:
    ///   [ch:1][slave:1][memCnt:1][memAddr:4 LE][readLen:2 LE]
    ///
    /// RX response:
    ///   [status:1][data:N]
    /// </summary>
    public sealed class Ra8m1I3cAdapter : IDlnInterface, IDisposable
    {
        private const byte RESULT_OK = 1;
        private const int DEFAULT_PORT = 8080;
        private const int DEFAULT_CONNECT_TIMEOUT_MS = 2000;
        private const int DEFAULT_COMM_TIMEOUT_MS = 2000;
        private const int MAX_BODY_LENGTH = 1024 * 1024;

        private readonly string _ipAddress;
        private readonly int _port;
        private readonly int _connectTimeoutMs;
        private readonly int _communicationTimeoutMs;
        private readonly object _commLock = new object();

        private TcpClient _client;
        private NetworkStream _stream;
        private bool _disposed;

        public bool IsRun { get; set; }
        public bool IsSafeOn { get; set; }
        public bool isMoving { get; set; }
        public bool m_bOccupied { get; set; }
        public uint PortCount { get { return 1; } }

        public event EventHandler SwitchOn;
        public event EventHandler SafetyOn;

        public Ra8m1I3cAdapter(string ipAddress)
            : this(ipAddress, DEFAULT_PORT, DEFAULT_CONNECT_TIMEOUT_MS, DEFAULT_COMM_TIMEOUT_MS, false)
        {
        }

        public Ra8m1I3cAdapter(string ipAddress, int port)
            : this(ipAddress, port, DEFAULT_CONNECT_TIMEOUT_MS, DEFAULT_COMM_TIMEOUT_MS, false)
        {
        }

        public Ra8m1I3cAdapter(
            string ipAddress,
            int port,
            int connectTimeoutMs,
            int communicationTimeoutMs,
            bool autoInitialize = false)
        {
            if (string.IsNullOrWhiteSpace(ipAddress))
                throw new ArgumentException("IP address is empty.", nameof(ipAddress));

            _ipAddress = ipAddress;
            _port = port;
            _connectTimeoutMs = connectTimeoutMs;
            _communicationTimeoutMs = communicationTimeoutMs;

            if (autoInitialize)
                Init();
        }
        #region Legacy Compatibility

        public bool IsConnected
        {
            get
            {
                return _client != null &&
                       _client.Connected &&
                       _stream != null;
            }
        }


        // 기존 STATIC 코드 호환용.
        // 현재 실제 memCnt는 각 Read/Write 호출에서 직접 전달하므로
        // 내부 설정값은 사용하지 않는다.
        public void ConfigureTarget(int ch,
            int slaveAddr,
            int registerAddressLength)
        {
            // 현재 구조에서는 별도 설정 필요 없음.
        }


        // 기존 STATIC 코드 호환용.
        public bool RemoveRegisterAddressLengthConfiguration(
            int ch, int slaveAddr)
        {
            // 현재 구조에서는 별도 설정 필요 없음.
            return true;
        }


        // 기존 코드가 out 없이 호출하는 경우를 위한 overload.
        public bool RunEntdaa(int ch)
        {
            int deviceCount;

            return RunEntdaa(
                ch,
                out deviceCount);
        }

        #endregion
        public bool Init()
        {
            lock (_commLock)
            {
                try
                {
                    ThrowIfDisposed();

                    if (!ConnectEngine())
                        return false;

                    byte[] response = SendAndReceiveInternal("IN", null);

                    bool ok = response != null &&
                              response.Length > 0 &&
                              response[0] == RESULT_OK;

                    //IsRun = ok;

                    if (!ok)
                        CloseConnection();

                    return ok;
                }
                catch (Exception ex)
                {
                    //IsRun = false;
                    CloseConnection();
                    SetError("RA8M1 Init Fail : " + ex.Message);
                    return false;
                }
            }
        }

        public void Disconnect()
        {
            lock (_commLock)
            {
                CloseConnection();
                //IsRun = false;
            }
        }

        private bool ConnectEngine()
        {
            CloseConnection();

            TcpClient client = new TcpClient();
            client.NoDelay = true;

            try
            {
                IAsyncResult ar = client.BeginConnect(_ipAddress, _port, null, null);

                using (WaitHandle waitHandle = ar.AsyncWaitHandle)
                {
                    if (!waitHandle.WaitOne(_connectTimeoutMs))
                    {
                        client.Close();
                        return false;
                    }
                }

                client.EndConnect(ar);

                NetworkStream stream = client.GetStream();
                stream.ReadTimeout = _communicationTimeoutMs;
                stream.WriteTimeout = _communicationTimeoutMs;

                _client = client;
                _stream = stream;
                return true;
            }
            catch
            {
                try { client.Close(); } catch { }
                CloseConnection();
                return false;
            }
        }

        private void EnsureConnected()
        {
            if (_client != null && _client.Connected && _stream != null)
                return;

            if (!ConnectEngine())
                throw new IOException("RA8M1 TCP connection failed. " + _ipAddress + ":" + _port);
        }

        private void CloseConnection()
        {
            try { if (_stream != null) _stream.Close(); } catch { }
            try { if (_client != null) _client.Close(); } catch { }

            _stream = null;
            _client = null;
        }

        private byte[] SendAndReceive(string command, byte[] payload, bool retryOnTransportError)
        {
            lock (_commLock)
            {
                ThrowIfDisposed();

                try
                {
                    EnsureConnected();
                    return SendAndReceiveInternal(command, payload);
                }
                catch (Exception ex)
                {
                    CloseConnection();

                    if (retryOnTransportError)
                    {
                        try
                        {
                            EnsureConnected();
                            return SendAndReceiveInternal(command, payload);
                        }
                        catch (Exception retryEx)
                        {
                            CloseConnection();
                            SetError("RA8M1 TCP Fail : " + retryEx.Message);
                            return null;
                        }
                    }

                    SetError("RA8M1 TCP Fail : " + ex.Message);
                    return null;
                }
            }
        }

        private byte[] SendAndReceiveInternal(string command, byte[] payload)
        {
            byte[] packet = MakePacket(command, payload);
            _stream.Write(packet, 0, packet.Length);

            byte[] lengthBuffer = new byte[4];
            ReadExact(_stream, lengthBuffer, 4);

            int responseLength =
                lengthBuffer[0] |
                (lengthBuffer[1] << 8) |
                (lengthBuffer[2] << 16) |
                (lengthBuffer[3] << 24);

            if (responseLength < 0 || responseLength > MAX_BODY_LENGTH)
                throw new IOException("Invalid response length : " + responseLength);

            if (responseLength == 0)
                return new byte[0];

            byte[] response = new byte[responseLength];
            ReadExact(_stream, response, responseLength);
            return response;
        }

        private bool SendOnly(string command, byte[] payload)
        {
            lock (_commLock)
            {
                ThrowIfDisposed();

                try
                {
                    EnsureConnected();

                    byte[] packet = MakePacket(command, payload);
                    _stream.Write(packet, 0, packet.Length);
                    return true;
                }
                catch (Exception ex)
                {
                    CloseConnection();
                    SetError("RA8M1 SendOnly Fail : " + ex.Message);
                    return false;
                }
            }
        }

        private static byte[] MakePacket(string command, byte[] payload)
        {
            byte[] cmdBytes = Encoding.ASCII.GetBytes(command);
            int payloadLength = payload == null ? 0 : payload.Length;
            int bodyLength = cmdBytes.Length + payloadLength;

            byte[] packet = new byte[4 + bodyLength];

            packet[0] = (byte)(bodyLength & 0xFF);
            packet[1] = (byte)((bodyLength >> 8) & 0xFF);
            packet[2] = (byte)((bodyLength >> 16) & 0xFF);
            packet[3] = (byte)((bodyLength >> 24) & 0xFF);

            Buffer.BlockCopy(cmdBytes, 0, packet, 4, cmdBytes.Length);

            if (payloadLength > 0)
                Buffer.BlockCopy(payload, 0, packet, 4 + cmdBytes.Length, payloadLength);

            return packet;
        }

        private static void ReadExact(NetworkStream stream, byte[] buffer, int length)
        {
            int offset = 0;

            while (offset < length)
            {
                int read = stream.Read(buffer, offset, length - offset);

                if (read <= 0)
                    throw new IOException("TCP connection closed while receiving.");

                offset += read;
            }
        }

        private static byte[] MakeWritePayload(
            int ch,
            int slaveAddr,
            int memAddr,
            int memCnt,
            byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            byte[] payload = new byte[9 + data.Length];

            payload[0] = checked((byte)ch);
            payload[1] = checked((byte)slaveAddr);
            payload[2] = checked((byte)memCnt);

            payload[3] = (byte)(memAddr & 0xFF);
            payload[4] = (byte)((memAddr >> 8) & 0xFF);
            payload[5] = (byte)((memAddr >> 16) & 0xFF);
            payload[6] = (byte)((memAddr >> 24) & 0xFF);

            payload[7] = (byte)(data.Length & 0xFF);
            payload[8] = (byte)((data.Length >> 8) & 0xFF);

            if (data.Length > 0)
                Buffer.BlockCopy(data, 0, payload, 9, data.Length);

            return payload;
        }

        private static byte[] MakeReadPayload(
            int ch,
            int slaveAddr,
            int memAddr,
            int memCnt,
            int readLength)
        {
            return new byte[]
            {
                checked((byte)ch),
                checked((byte)slaveAddr),
                checked((byte)memCnt),

                (byte)(memAddr & 0xFF),
                (byte)((memAddr >> 8) & 0xFF),
                (byte)((memAddr >> 16) & 0xFF),
                (byte)((memAddr >> 24) & 0xFF),

                (byte)(readLength & 0xFF),
                (byte)((readLength >> 8) & 0xFF)
            };
        }

        private bool WriteRegister(
            int ch,
            int slaveAddr,
            int memAddr,
            int memCnt,
            byte[] data)
        {
            byte[] payload = MakeWritePayload(ch, slaveAddr, memAddr, memCnt, data);

            // Write는 중복 실행 방지를 위해 자동 retry 안 함.
            byte[] response = SendAndReceive("WX", payload, false);

            return response != null &&
                   response.Length > 0 &&
                   response[0] == RESULT_OK;
        }

        private byte[] ReadRegister(
            int ch,
            int slaveAddr,
            int memAddr,
            int memCnt,
            int length)
        {
            byte[] payload = MakeReadPayload(ch, slaveAddr, memAddr, memCnt, length);

            // Read는 transport 오류 시 1회 retry 허용.
            byte[] response = SendAndReceive("RX", payload, true);

            if (response == null ||
                response.Length != length + 1 ||
                response[0] != RESULT_OK)
            {
                return null;
            }

            byte[] result = new byte[length];

            if (length > 0)
                Buffer.BlockCopy(response, 1, result, 0, length);

            return result;
        }
        public bool ReInitTarget(int ch)
        {
            byte[] check = ReadRegister(ch, 0x5A, 0x1008, 2, 4);

            if (check != null && check.Length == 4)
                return true;

            Thread.Sleep(50);

            byte[] response = SendAndReceive("RI", null, false);

            return response != null &&
                   response.Length > 0 &&
                   response[0] == RESULT_OK;
        }
        public bool WriteByte(int ch, int slaveAddr, int memAddr, int memCnt, byte data)
        {
            return WriteRegister(
                ch,
                slaveAddr,
                memAddr,
                memCnt,
                new byte[] { data });
        }

        public bool Write2Byte(int ch, int slaveAddr, int memAddr, int memCnt, ushort data)
        {
            byte[] tmp =
            {
                (byte)(data & 0xFF),
                (byte)((data >> 8) & 0xFF)
            };

            return WriteRegister(ch, slaveAddr, memAddr, memCnt, tmp);
        }

        public bool Write2Byte(int ch, int slaveAddr, int memAddr, int memCnt, short data)
        {
            return Write2Byte(
                ch,
                slaveAddr,
                memAddr,
                memCnt,
                unchecked((ushort)data));
        }

        public bool Write4Byte(int ch, int slaveAddr, int memAddr, int memCnt, uint data)
        {
            byte[] tmp =
            {
                (byte)(data & 0xFF),
                (byte)((data >> 8) & 0xFF),
                (byte)((data >> 16) & 0xFF),
                (byte)((data >> 24) & 0xFF)
            };

            return WriteRegister(ch, slaveAddr, memAddr, memCnt, tmp);
        }

        public bool Write4Byte(int ch, int slaveAddr, int memAddr, int memCnt, int data)
        {
            return Write4Byte(
                ch,
                slaveAddr,
                memAddr,
                memCnt,
                unchecked((uint)data));
        }

        public bool WriteArray(int ch, int slaveAddr, int memAddr, byte[] data)
        {
            // 최신 DLN과 동일: memCnt = 2
            return WriteRegister(ch, slaveAddr, memAddr, 2, data);
        }

        public bool WriteArray(int ch, int slaveAddr, byte[] data)
        {
            // Register address 없는 raw transfer.
            return WriteRegister(ch, slaveAddr, 0, 0, data);
        }

        public bool WriteArray(
            int ch,
            int slaveAddr,
            int memAddr,
            int memCnt,
            byte[] data)
        {
            return WriteRegister(ch, slaveAddr, memAddr, memCnt, data);
        }

        public bool WriteArrayNoResponse(
            int ch,
            int slaveAddr,
            int memAddr,
            int memCnt,
            byte[] data)
        {
            byte[] payload = MakeWritePayload(ch, slaveAddr, memAddr, memCnt, data);
            return SendOnly("WN", payload);
        }

        public byte ReadByte(int ch, int slaveAddr, int memAddr, int memCnt)
        {
            byte[] data = ReadRegister(ch, slaveAddr, memAddr, memCnt, 1);
            return data == null ? byte.MaxValue : data[0];
        }

        public byte? ReadByteNull(int ch, int slaveAddr, int memAddr, int memCnt)
        {
            byte[] data = ReadRegister(ch, slaveAddr, memAddr, memCnt, 1);
            return data == null ? (byte?)null : data[0];
        }

        public ushort Read2Byte(int ch, int slaveAddr, int memAddr, int memCnt)
        {
            byte[] data = ReadRegister(ch, slaveAddr, memAddr, memCnt, 2);

            if (data == null)
                return ushort.MaxValue;

            return (ushort)(data[0] | (data[1] << 8));
        }

        public short Read2Byte_signed(int ch, int slaveAddr, int memAddr, int memCnt)
        {
            byte[] data = ReadRegister(ch, slaveAddr, memAddr, memCnt, 2);

            if (data == null)
                return short.MinValue;

            ushort raw = (ushort)(data[0] | (data[1] << 8));
            return unchecked((short)raw);
        }

        public uint Read4Byte(int ch, int slaveAddr, int memAddr, int memCnt)
        {
            byte[] data = ReadRegister(ch, slaveAddr, memAddr, memCnt, 4);

            if (data == null)
                return uint.MinValue;

            return
                ((uint)data[0]) |
                ((uint)data[1] << 8) |
                ((uint)data[2] << 16) |
                ((uint)data[3] << 24);
        }

        public int Read4Byte_signed(int ch, int slaveAddr, int memAddr, int memCnt)
        {
            byte[] data = ReadRegister(ch, slaveAddr, memAddr, memCnt, 4);

            if (data == null)
                return int.MinValue;

            uint raw =
                ((uint)data[0]) |
                ((uint)data[1] << 8) |
                ((uint)data[2] << 16) |
                ((uint)data[3] << 24);

            return unchecked((int)raw);
        }

        public bool ReadArray(int ch, int slaveAddr, int memAddr, byte[] data)
        {
            if (data == null)
                return false;

            // 최신 DLN과 동일: memCnt = 2
            return ReadArray(ch, slaveAddr, memAddr, 2, data);
        }

        public bool ReadArray(int ch, int slaveAddr, byte[] data)
        {
            if (data == null)
                return false;

            return ReadArray(ch, slaveAddr, 0, 0, data);
        }

        public bool ReadArray(
            int ch,
            int slaveAddr,
            int memAddr,
            int memCnt,
            byte[] data)
        {
            if (data == null)
                return false;

            byte[] response =
                ReadRegister(
                    ch,
                    slaveAddr,
                    memAddr,
                    memCnt,
                    data.Length);

            if (response == null ||
                response.Length != data.Length)
            {
                return false;
            }

            if (data.Length > 0)
                Buffer.BlockCopy(response, 0, data, 0, data.Length);

            return true;
        }

        public bool Ping()
        {
            byte[] response = SendAndReceive("PG", null, true);

            return response != null &&
                   response.Length > 0 &&
                   response[0] == RESULT_OK;
        }

        public string GetVersion()
        {
            byte[] response = SendAndReceive("VR", null, true);

            if (response == null ||
                response.Length == 0 ||
                response[0] != RESULT_OK)
            {
                return string.Empty;
            }

            if (response.Length >= 5)
            {
                return string.Format(
                    "Protocol {0}.{1}, Firmware {2}.{3}",
                    response[1],
                    response[2],
                    response[3],
                    response[4]);
            }

            return "OK";
        }

        public bool RunEntdaa(int ch, out int deviceCount)
        {
            deviceCount = 0;

            byte[] response =
                SendAndReceive(
                    "DA",
                    new byte[] { checked((byte)ch) },
                    false);

            if (response == null ||
                response.Length < 1 ||
                response[0] != RESULT_OK)
            {
                return false;
            }

            if (response.Length >= 2)
                deviceCount = response[1];

            return true;
        }

        public I3cDeviceInfo[] GetDeviceTable(int ch)
        {
            byte[] response =
                SendAndReceive(
                    "DT",
                    new byte[] { checked((byte)ch) },
                    true);

            if (response == null ||
                response.Length < 2 ||
                response[0] != RESULT_OK)
            {
                return new I3cDeviceInfo[0];
            }

            int count = response[1];
            const int recordSize = 10;

            if (response.Length < 2 + count * recordSize)
                return new I3cDeviceInfo[0];

            List<I3cDeviceInfo> list =
                new List<I3cDeviceInfo>();

            int offset = 2;

            for (int i = 0; i < count; i++)
            {
                ulong pid = 0;

                for (int j = 0; j < 6; j++)
                    pid = (pid << 8) | response[offset + 4 + j];

                list.Add(
                    new I3cDeviceInfo
                    {
                        DynamicAddress = response[offset + 0],
                        StaticAddress = response[offset + 1],
                        Bcr = response[offset + 2],
                        Dcr = response[offset + 3],
                        ProvisionalId = pid
                    });

                offset += recordSize;
            }

            return list.ToArray();
        }

        public bool ResetI3cBus(int ch)
        {
            byte[] response =
                SendAndReceive(
                    "BR",
                    new byte[] { checked((byte)ch) },
                    false);

            return response != null &&
                   response.Length > 0 &&
                   response[0] == RESULT_OK;
        }

        public sealed class I3cDeviceInfo
        {
            public byte DynamicAddress { get; internal set; }
            public byte StaticAddress { get; internal set; }
            public byte Bcr { get; internal set; }
            public byte Dcr { get; internal set; }
            public ulong ProvisionalId { get; internal set; }

            public override string ToString()
            {
                return string.Format(
                    "DA=0x{0:X2}, SA=0x{1:X2}, BCR=0x{2:X2}, DCR=0x{3:X2}, PID=0x{4:X12}",
                    DynamicAddress,
                    StaticAddress,
                    Bcr,
                    Dcr,
                    ProvisionalId);
            }
        }

        public byte[] RunInternalSequence(string cmd, byte[] payload = null)
        {
            if (string.IsNullOrEmpty(cmd))
                return null;

            return SendAndReceive(cmd, payload, false);
        }

        public bool ChangeSlaveAddrUnified(
            int ch,
            byte origin,
            byte target,
            byte pinMode,
            bool isAF)
        {
            byte[] payload =
            {
                checked((byte)ch),
                origin,
                target,
                pinMode,
                isAF ? (byte)1 : (byte)0
            };

            byte[] response =
                SendAndReceive(
                    "UA",
                    payload,
                    false);

            return response != null &&
                   response.Length > 0 &&
                   response[0] == RESULT_OK;
        }

        // GPIO / Light / Socket 미사용.
        public void PowerOnOff(int port, bool IsOn = true) { }
        public void LoadSocket() { }
        public void UnloadSocket() { }
        public void CoverDn() { }
        public void CoverUp() { }
        public void SetSocketSensor(bool isOn) { }
        public bool GetGpioStatus(int input) { return false; }
        public void PeakDetector(int ADCNumber, PeakDetectState state) { }
        public double GetCurrent(int ch, int mode) { return 0.0; }
        public void SetLEDpower(int id, int value) { }

        public void SetError(string s)
        {
            try
            {
                if (STATIC.Process != null)
                    STATIC.Process.AddLog(0, "[RA8M1-I3C] " + s);
            }
            catch
            {
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(Ra8m1I3cAdapter));
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            lock (_commLock)
            {
                if (_disposed)
                    return;

                _disposed = true;
                CloseConnection();
                //IsRun = false;
            }

            GC.SuppressFinalize(this);
        }
    }
}