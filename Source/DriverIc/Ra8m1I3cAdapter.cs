using FZ4P;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace FZ4P.DriverIc.Adapter
{
    /// <summary>
    /// RA8M1 Ethernet-to-I3C adapter.
    ///
    /// TCP frame (same outer framing as Esp32WifiDevice):
    ///   PC -> RA8M1 : [BodyLength:4, Little Endian][ASCII Command][Payload]
    ///   RA8M1 -> PC : [BodyLength:4, Little Endian][Response Body]
    ///
    /// Extended command protocol expected from the RA8M1 firmware:
    ///
    /// IN : Initialize/handshake
    ///      Response: [Status]
    ///
    /// WX : Register/raw write
    ///      Payload : [Channel:1][TargetAddr:1][MemCnt:1]
    ///                [MemAddr:4 LE][DataLength:2 LE][Data:N]
    ///      Response: [Status]
    ///
    /// RX : Register/raw read
    ///      Payload : [Channel:1][TargetAddr:1][MemCnt:1]
    ///                [MemAddr:4 LE][ReadLength:2 LE]
    ///      Response: [Status][Data:N]
    ///
    /// WN : No-response write
    ///      Payload is identical to WX. The firmware MUST NOT send a response.
    ///
    /// DA : Run ENTDAA
    ///      Payload : [Channel:1]
    ///      Response: [Status][DeviceCount:1]
    ///
    /// DT : Get I3C device table
    ///      Payload : [Channel:1]
    ///      Response: [Status][DeviceCount:1][DeviceRecord:10 * Count]
    ///      Record  : [DynamicAddr:1][StaticAddr:1][BCR:1][DCR:1][PID:6 BE]
    ///
    /// BR : Reset I3C bus
    ///      Payload : [Channel:1]
    ///      Response: [Status]
    ///
    /// VR : Get protocol/firmware version
    ///      Response: [Status][ProtocolMajor][ProtocolMinor][FwMajor][FwMinor]...
    ///
    /// PG : Ping
    ///      Response: [Status]
    ///
    /// UA : Existing unified legacy-address-change sequence
    ///      Payload : [Origin][Target][PinMode][IsAF]
    ///      Response: [Status][FoundAddress]
    ///
    /// Status value 1 means success. Other values are errors.
    /// </summary>
    public sealed class Ra8m1I3cAdapter : IDlnInterface, IDisposable
    {
        #region Nested types

        private enum Ra8m1Status : byte
        {
            Fail = 0,
            Ok = 1,
            Nack = 2,
            Timeout = 3,
            BusBusy = 4,
            InvalidParameter = 5,
            AddressNotFound = 6,
            Unsupported = 7,
            FspError = 8
        }

        /// <summary>
        /// One record returned by the DT command.
        /// </summary>
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
                    "Dynamic=0x{0:X2}, Static=0x{1:X2}, BCR=0x{2:X2}, DCR=0x{3:X2}, PID=0x{4:X12}",
                    DynamicAddress,
                    StaticAddress,
                    Bcr,
                    Dcr,
                    ProvisionalId);
            }
        }

        #endregion

        #region Constants

        private const string CommandInitialize = "IN";
        private const string CommandWrite = "WX";
        private const string CommandRead = "RX";
        private const string CommandWriteNoResponse = "WN";
        private const string CommandEntdaa = "DA";
        private const string CommandDeviceTable = "DT";
        private const string CommandBusReset = "BR";
        private const string CommandVersion = "VR";
        private const string CommandPing = "PG";
        private const string CommandChangeAddress = "UA";

        private const int ResponseHeaderLength = 4;
        private const int ReadWriteCommonPayloadLength = 9;
        private const int DeviceRecordLength = 10;
        private const int DefaultMaxResponseLength = 1024 * 1024;

        #endregion

        #region Fields

        private readonly string _ipAddress;
        private readonly int _port;
        private readonly int _connectTimeoutMs;
        private readonly int _communicationTimeoutMs;

        private readonly object _communicationLock = new object();
        private readonly object _targetConfigurationLock = new object();
        private readonly Dictionary<int, int> _registerAddressLengthByTarget =
            new Dictionary<int, int>();

        private TcpClient _client;
        private NetworkStream _stream;
        private bool _disposed;
        private int _defaultRegisterAddressLength;
        private string _lastError = string.Empty;
        private byte _lastStatus;

        #endregion

        #region Constructor

        /// <summary>
        /// Creates the adapter and performs IN handshake immediately.
        /// </summary>
        public Ra8m1I3cAdapter(string ipAddress, int port = 8080)
            : this(
                ipAddress,
                port,
                2000,
                2000,
                1,
                true)
        {
        }

        /// <summary>
        /// Creates the adapter with explicit communication options.
        /// Set autoInitialize to false when DeviceFactory will call Init() separately.
        /// </summary>
        public Ra8m1I3cAdapter(
            string ipAddress,
            int port,
            int connectTimeoutMs,
            int communicationTimeoutMs,
            int defaultRegisterAddressLength,
            bool autoInitialize)
        {
            if (string.IsNullOrWhiteSpace(ipAddress))
                throw new ArgumentException("IP address is required.", nameof(ipAddress));

            if (port < 1 || port > 65535)
                throw new ArgumentOutOfRangeException(nameof(port));

            if (connectTimeoutMs <= 0)
                throw new ArgumentOutOfRangeException(nameof(connectTimeoutMs));

            if (communicationTimeoutMs <= 0)
                throw new ArgumentOutOfRangeException(nameof(communicationTimeoutMs));

            ValidateRegisterAddressLength(defaultRegisterAddressLength, false);

            _ipAddress = ipAddress;
            _port = port;
            _connectTimeoutMs = connectTimeoutMs;
            _communicationTimeoutMs = communicationTimeoutMs;
            _defaultRegisterAddressLength = defaultRegisterAddressLength;

            // This test configuration does not use a safety GPIO.
            IsSafeOn = true;

            if (autoInitialize && !Init())
            {
                Log(0, "[RA8M1-I3C] Initial connection failed. It can be retried by calling Init().");
            }
        }

        #endregion

        #region IDlnInterface properties/events

        public bool IsRun { get; set; }
        public bool IsSafeOn { get; set; }
        public bool isMoving { get; set; }
        public bool m_bOccupied { get; set; }

        public uint PortCount
        {
            get { return 1; }
        }

        public bool IsConnected
        {
            get
            {
                lock (_communicationLock)
                {
                    return IsTransportReady();
                }
            }
        }

        public string LastError
        {
            get { return _lastError; }
        }

        public byte LastStatus
        {
            get { return _lastStatus; }
        }

        public int LastEntdaaDeviceCount { get; private set; }

        public int MaxResponseLength { get; set; } = DefaultMaxResponseLength;

        public int DefaultRegisterAddressLength
        {
            get { return _defaultRegisterAddressLength; }
            set
            {
                ValidateRegisterAddressLength(value, false);
                _defaultRegisterAddressLength = value;
            }
        }

#pragma warning disable 0067
        // GPIO is not used by this RA8M1 test equipment, so these events are not raised.
        public event EventHandler SwitchOn;
        public event EventHandler SafetyOn;
#pragma warning restore 0067

        #endregion

        #region Initialization / connection

        public bool Init()
        {
            ThrowIfDisposed();

            if (IsVirtualMode())
            {
                IsRun = true;
                ClearLastError();
                return true;
            }

            lock (_communicationLock)
            {
                try
                {
                    CloseConnectionCore();

                    if (!ConnectEngineCore())
                    {
                        IsRun = false;
                        return false;
                    }

                    byte[] response = SendAndReceiveCore(
                        CommandInitialize,
                        null,
                        true,
                        false);

                    if (!IsSuccessResponse(response))
                    {
                        IsRun = false;
                        CloseConnectionCore();
                        SetError("RA8M1 handshake failed.");
                        return false;
                    }

                    IsRun = true;
                    RegisterCommunicationSuccess();
                    Log(0, "[RA8M1-I3C] Connection established.");
                    return true;
                }
                catch (Exception ex)
                {
                    IsRun = false;
                    CloseConnectionCore();
                    RegisterCommunicationFailure("Init", ex);
                    return false;
                }
            }
        }

        public void Disconnect()
        {
            lock (_communicationLock)
            {
                CloseConnectionCore();
            }
        }

        private bool ConnectEngineCore()
        {
            try
            {
                CloseConnectionCore();

                _client = new TcpClient();
                _client.NoDelay = true;
                _client.Client.SetSocketOption(
                    SocketOptionLevel.Socket,
                    SocketOptionName.KeepAlive,
                    true);

                IAsyncResult asyncResult = _client.BeginConnect(
                    _ipAddress,
                    _port,
                    null,
                    null);

                WaitHandle waitHandle = asyncResult.AsyncWaitHandle;
                try
                {
                    if (!waitHandle.WaitOne(_connectTimeoutMs, false))
                    {
                        SetError(string.Format(
                            "Connect timeout: {0}:{1}",
                            _ipAddress,
                            _port));
                        CloseConnectionCore();
                        return false;
                    }

                    _client.EndConnect(asyncResult);
                }
                finally
                {
                    waitHandle.Close();
                }

                _stream = _client.GetStream();
                _stream.ReadTimeout = _communicationTimeoutMs;
                _stream.WriteTimeout = _communicationTimeoutMs;

                return true;
            }
            catch (Exception ex)
            {
                CloseConnectionCore();
                SetError("ConnectEngine failed: " + ex.Message);
                return false;
            }
        }

        private bool EnsureConnectedCore()
        {
            if (IsTransportReady())
                return true;

            return Init();
        }

        private bool IsTransportReady()
        {
            return _client != null &&
                   _client.Connected &&
                   _stream != null;
        }

        private void CloseConnectionCore()
        {
            try
            {
                if (_stream != null)
                    _stream.Close();
            }
            catch
            {
            }

            try
            {
                if (_client != null)
                    _client.Close();
            }
            catch
            {
            }

            _stream = null;
            _client = null;
            IsRun = false;
        }

        #endregion

        #region TCP engine

        /// <summary>
        /// Sends one command and reads one length-prefixed response.
        /// allowRetry should normally be true only for idempotent read/query commands.
        /// </summary>
        private byte[] SendAndReceive(
            string command,
            byte[] payload,
            bool allowRetry)
        {
            ThrowIfDisposed();

            int attemptCount = allowRetry ? 2 : 1;

            for (int attempt = 0; attempt < attemptCount; attempt++)
            {
                lock (_communicationLock)
                {
                    try
                    {
                        if (!EnsureConnectedCore())
                            throw new IOException("RA8M1 is not connected.");

                        return SendAndReceiveCore(
                            command,
                            payload,
                            false,
                            allowRetry);
                    }
                    catch (Exception ex)
                    {
                        CloseConnectionCore();

                        bool hasNextAttempt = attempt + 1 < attemptCount;
                        if (!hasNextAttempt)
                        {
                            RegisterCommunicationFailure(command, ex);
                            return null;
                        }

                        Log(0, string.Format(
                            "[RA8M1-I3C] {0} communication error: {1}. Reconnecting once.",
                            command,
                            ex.Message));
                    }
                }

                Thread.Sleep(100);
            }

            return null;
        }

        /// <summary>
        /// Caller must hold _communicationLock.
        /// isInitCall prevents recursive reconnect/Init processing.
        /// </summary>
        private byte[] SendAndReceiveCore(
            string command,
            byte[] payload,
            bool isInitCall,
            bool allowRetry)
        {
            ValidateCommand(command);

            if (!isInitCall && !IsTransportReady())
                throw new IOException("TCP transport is not connected.");

            byte[] packet = BuildPacket(command, payload);
            _stream.Write(packet, 0, packet.Length);

            byte[] responseHeader = new byte[ResponseHeaderLength];
            if (!ReadExact(_stream, responseHeader, responseHeader.Length))
                throw new EndOfStreamException("Response header read failed.");

            int responseLength = ReadInt32LittleEndian(responseHeader, 0);
            if (responseLength < 0 || responseLength > MaxResponseLength)
            {
                throw new InvalidDataException(
                    "Invalid response length: " + responseLength);
            }

            byte[] response = new byte[responseLength];
            if (responseLength > 0 &&
                !ReadExact(_stream, response, responseLength))
            {
                throw new EndOfStreamException("Response body read failed.");
            }

            return response;
        }

        /// <summary>
        /// Sends WN and intentionally reads no response.
        /// Never retries automatically because duplicate writes may be unsafe.
        /// </summary>
        private bool SendOnly(string command, byte[] payload)
        {
            ThrowIfDisposed();

            lock (_communicationLock)
            {
                try
                {
                    if (!EnsureConnectedCore())
                        throw new IOException("RA8M1 is not connected.");

                    byte[] packet = BuildPacket(command, payload);
                    _stream.Write(packet, 0, packet.Length);
                    return true;
                }
                catch (Exception ex)
                {
                    CloseConnectionCore();
                    RegisterCommunicationFailure(command, ex);
                    return false;
                }
            }
        }

        private static byte[] BuildPacket(string command, byte[] payload)
        {
            ValidateCommand(command);

            byte[] commandBytes = Encoding.ASCII.GetBytes(command);
            int payloadLength = payload == null ? 0 : payload.Length;
            int bodyLength = checked(commandBytes.Length + payloadLength);

            byte[] packet = new byte[ResponseHeaderLength + bodyLength];
            WriteInt32LittleEndian(packet, 0, bodyLength);

            Buffer.BlockCopy(
                commandBytes,
                0,
                packet,
                ResponseHeaderLength,
                commandBytes.Length);

            if (payloadLength > 0)
            {
                Buffer.BlockCopy(
                    payload,
                    0,
                    packet,
                    ResponseHeaderLength + commandBytes.Length,
                    payloadLength);
            }

            return packet;
        }

        private static bool ReadExact(Stream stream, byte[] buffer, int length)
        {
            int totalRead = 0;

            while (totalRead < length)
            {
                int read = stream.Read(
                    buffer,
                    totalRead,
                    length - totalRead);

                if (read <= 0)
                    return false;

                totalRead += read;
            }

            return true;
        }

        private static void ValidateCommand(string command)
        {
            if (string.IsNullOrEmpty(command))
                throw new ArgumentException("Command is required.", nameof(command));

            for (int i = 0; i < command.Length; i++)
            {
                if (command[i] > 0x7F)
                    throw new ArgumentException("Command must be ASCII.", nameof(command));
            }
        }

        #endregion

        #region Target configuration

        /// <summary>
        /// Configures the register-address length used by the legacy WriteArray/ReadArray
        /// overloads that do not contain memCnt in IDlnInterface.
        /// </summary>
        public void ConfigureRegisterAddressLength(
            int ch,
            int slaveAddr,
            int registerAddressLength)
        {
            ValidateByteRange(ch, nameof(ch));
            ValidateByteRange(slaveAddr, nameof(slaveAddr));
            ValidateRegisterAddressLength(registerAddressLength, false);

            int key = MakeTargetKey(ch, slaveAddr);

            lock (_targetConfigurationLock)
            {
                _registerAddressLengthByTarget[key] = registerAddressLength;
            }
        }

        /// <summary>
        /// Compatibility alias used by earlier integration examples.
        /// </summary>
        public void ConfigureTarget(
            int ch,
            int slaveAddr,
            int registerAddressLength)
        {
            ConfigureRegisterAddressLength(
                ch,
                slaveAddr,
                registerAddressLength);
        }

        public bool RemoveRegisterAddressLengthConfiguration(
            int ch,
            int slaveAddr)
        {
            ValidateByteRange(ch, nameof(ch));
            ValidateByteRange(slaveAddr, nameof(slaveAddr));

            int key = MakeTargetKey(ch, slaveAddr);

            lock (_targetConfigurationLock)
            {
                return _registerAddressLengthByTarget.Remove(key);
            }
        }

        private int GetRegisterAddressLength(int ch, int slaveAddr)
        {
            int key = MakeTargetKey(ch, slaveAddr);

            lock (_targetConfigurationLock)
            {
                int value;
                if (_registerAddressLengthByTarget.TryGetValue(key, out value))
                    return value;
            }

            return DefaultRegisterAddressLength;
        }

        private static int MakeTargetKey(int ch, int slaveAddr)
        {
            return ((ch & 0xFF) << 8) | (slaveAddr & 0xFF);
        }

        #endregion

        #region Common register transfer

        private bool WriteRegister(
            int ch,
            int slaveAddr,
            int memAddr,
            int memCnt,
            byte[] data)
        {
            if (IsVirtualMode())
                return true;

            try
            {
                byte[] payload = MakeWritePayload(
                    ch,
                    slaveAddr,
                    memAddr,
                    memCnt,
                    data);

                // Do not automatically resend a write after an ambiguous TCP failure.
                byte[] response = SendAndReceive(
                    CommandWrite,
                    payload,
                    false);

                if (!IsSuccessResponse(response))
                {
                    RegisterProtocolFailure(CommandWrite, response);
                    return false;
                }

                RegisterCommunicationSuccess();
                return true;
            }
            catch (Exception ex)
            {
                RegisterCommunicationFailure(CommandWrite, ex);
                return false;
            }
        }

        private byte[] ReadRegister(
            int ch,
            int slaveAddr,
            int memAddr,
            int memCnt,
            int readLength)
        {
            if (IsVirtualMode())
                return new byte[readLength];

            try
            {
                byte[] payload = MakeReadPayload(
                    ch,
                    slaveAddr,
                    memAddr,
                    memCnt,
                    readLength);

                // A read is idempotent, so one reconnect/retry is permitted.
                byte[] response = SendAndReceive(
                    CommandRead,
                    payload,
                    true);

                if (response == null || response.Length < 1)
                {
                    RegisterProtocolFailure(CommandRead, response);
                    return null;
                }

                _lastStatus = response[0];
                if (response[0] != (byte)Ra8m1Status.Ok)
                {
                    RegisterProtocolFailure(CommandRead, response);
                    return null;
                }

                if (response.Length != readLength + 1)
                {
                    SetError(string.Format(
                        "RX response length mismatch. Expected={0}, Actual={1}",
                        readLength + 1,
                        response.Length));
                    return null;
                }

                byte[] result = new byte[readLength];
                if (readLength > 0)
                {
                    Buffer.BlockCopy(
                        response,
                        1,
                        result,
                        0,
                        readLength);
                }

                RegisterCommunicationSuccess();
                return result;
            }
            catch (Exception ex)
            {
                RegisterCommunicationFailure(CommandRead, ex);
                return null;
            }
        }

        private static byte[] MakeWritePayload(
            int ch,
            int slaveAddr,
            int memAddr,
            int memCnt,
            byte[] data)
        {
            ValidateTransferArguments(
                ch,
                slaveAddr,
                memAddr,
                memCnt,
                data == null ? -1 : data.Length);

            if (data == null)
                throw new ArgumentNullException(nameof(data));

            byte[] payload = new byte[ReadWriteCommonPayloadLength + data.Length];

            payload[0] = (byte)ch;
            payload[1] = (byte)slaveAddr;
            payload[2] = (byte)memCnt;

            WriteInt32LittleEndian(payload, 3, memAddr);
            WriteUInt16LittleEndian(payload, 7, (ushort)data.Length);

            if (data.Length > 0)
            {
                Buffer.BlockCopy(
                    data,
                    0,
                    payload,
                    ReadWriteCommonPayloadLength,
                    data.Length);
            }

            return payload;
        }

        private static byte[] MakeReadPayload(
            int ch,
            int slaveAddr,
            int memAddr,
            int memCnt,
            int readLength)
        {
            ValidateTransferArguments(
                ch,
                slaveAddr,
                memAddr,
                memCnt,
                readLength);

            byte[] payload = new byte[ReadWriteCommonPayloadLength];

            payload[0] = (byte)ch;
            payload[1] = (byte)slaveAddr;
            payload[2] = (byte)memCnt;

            WriteInt32LittleEndian(payload, 3, memAddr);
            WriteUInt16LittleEndian(payload, 7, (ushort)readLength);

            return payload;
        }

        private static void ValidateTransferArguments(
            int ch,
            int slaveAddr,
            int memAddr,
            int memCnt,
            int dataLength)
        {
            ValidateByteRange(ch, nameof(ch));
            ValidateByteRange(slaveAddr, nameof(slaveAddr));
            ValidateRegisterAddressLength(memCnt, true);

            if (memAddr < 0)
                throw new ArgumentOutOfRangeException(nameof(memAddr));

            if (dataLength < 0 || dataLength > ushort.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(dataLength));
        }

        #endregion

        #region IDlnInterface write methods

        public bool WriteByte(
            int ch,
            int slaveAddr,
            int memAddr,
            int memCnt,
            byte data)
        {
            return WriteRegister(
                ch,
                slaveAddr,
                memAddr,
                memCnt,
                new byte[] { data });
        }

        public bool Write2Byte(
            int ch,
            int slaveAddr,
            int memAddr,
            int memCnt,
            ushort data)
        {
            return WriteRegister(
                ch,
                slaveAddr,
                memAddr,
                memCnt,
                new byte[]
                {
                    (byte)((data >> 8) & 0xFF),
                    (byte)(data & 0xFF)
                });
        }

        public bool Write2Byte(
            int ch,
            int slaveAddr,
            int memAddr,
            int memCnt,
            short data)
        {
            return Write2Byte(
                ch,
                slaveAddr,
                memAddr,
                memCnt,
                unchecked((ushort)data));
        }

        public bool Write4Byte(
            int ch,
            int slaveAddr,
            int memAddr,
            int memCnt,
            uint data)
        {
            return WriteRegister(
                ch,
                slaveAddr,
                memAddr,
                memCnt,
                new byte[]
                {
                    (byte)((data >> 24) & 0xFF),
                    (byte)((data >> 16) & 0xFF),
                    (byte)((data >> 8) & 0xFF),
                    (byte)(data & 0xFF)
                });
        }

        public bool Write4Byte(
            int ch,
            int slaveAddr,
            int memAddr,
            int memCnt,
            int data)
        {
            return Write4Byte(
                ch,
                slaveAddr,
                memAddr,
                memCnt,
                unchecked((uint)data));
        }

        /// <summary>
        /// IDlnInterface compatibility overload. memCnt is obtained from the target map.
        /// </summary>
        public bool WriteArray(
            int ch,
            int slaveAddr,
            int memAddr,
            byte[] data)
        {
            return WriteRegister(
                ch,
                slaveAddr,
                memAddr,
                GetRegisterAddressLength(ch, slaveAddr),
                data);
        }

        /// <summary>
        /// Extended overload for new code that can specify memCnt explicitly.
        /// </summary>
        public bool WriteArray(
            int ch,
            int slaveAddr,
            int memAddr,
            int memCnt,
            byte[] data)
        {
            return WriteRegister(
                ch,
                slaveAddr,
                memAddr,
                memCnt,
                data);
        }

        /// <summary>
        /// Raw private transfer without a register-address prefix.
        /// </summary>
        public bool WriteArray(
            int ch,
            int slaveAddr,
            byte[] data)
        {
            return WriteRegister(
                ch,
                slaveAddr,
                0,
                0,
                data);
        }

        /// <summary>
        /// Sends WN. The RA8M1 firmware must not send any response for this command.
        /// </summary>
        public bool WriteArrayNoResponse(
            int ch,
            int slaveAddr,
            int memAddr,
            int memCnt,
            byte[] data)
        {
            if (IsVirtualMode())
                return true;

            try
            {
                byte[] payload = MakeWritePayload(
                    ch,
                    slaveAddr,
                    memAddr,
                    memCnt,
                    data);

                bool sent = SendOnly(
                    CommandWriteNoResponse,
                    payload);

                if (sent)
                    RegisterCommunicationSuccess();

                return sent;
            }
            catch (Exception ex)
            {
                RegisterCommunicationFailure(CommandWriteNoResponse, ex);
                return false;
            }
        }

        #endregion

        #region IDlnInterface read methods

        public byte ReadByte(
            int ch,
            int slaveAddr,
            int memAddr,
            int memCnt)
        {
            byte[] data = ReadRegister(
                ch,
                slaveAddr,
                memAddr,
                memCnt,
                1);

            return data == null || data.Length != 1
                ? byte.MaxValue
                : data[0];
        }

        public byte? ReadByteNull(
            int ch,
            int slaveAddr,
            int memAddr,
            int memCnt)
        {
            if (IsVirtualMode())
                return null;

            byte[] data = ReadRegister(
                ch,
                slaveAddr,
                memAddr,
                memCnt,
                1);

            if (data == null || data.Length != 1)
                return null;

            return data[0];
        }

        public ushort Read2Byte(
            int ch,
            int slaveAddr,
            int memAddr,
            int memCnt)
        {
            byte[] data = ReadRegister(
                ch,
                slaveAddr,
                memAddr,
                memCnt,
                2);

            if (data == null || data.Length != 2)
                return ushort.MaxValue;

            return (ushort)((data[0] << 8) | data[1]);
        }

        public short Read2Byte_signed(
            int ch,
            int slaveAddr,
            int memAddr,
            int memCnt)
        {
            byte[] data = ReadRegister(
                ch,
                slaveAddr,
                memAddr,
                memCnt,
                2);

            if (data == null || data.Length != 2)
                return short.MinValue;

            ushort unsignedValue = (ushort)((data[0] << 8) | data[1]);
            return unchecked((short)unsignedValue);
        }

        public uint Read4Byte(
            int ch,
            int slaveAddr,
            int memAddr,
            int memCnt)
        {
            byte[] data = ReadRegister(
                ch,
                slaveAddr,
                memAddr,
                memCnt,
                4);

            if (data == null || data.Length != 4)
                return uint.MaxValue;

            return ((uint)data[0] << 24) |
                   ((uint)data[1] << 16) |
                   ((uint)data[2] << 8) |
                   data[3];
        }

        public int Read4Byte_signed(
            int ch,
            int slaveAddr,
            int memAddr,
            int memCnt)
        {
            byte[] data = ReadRegister(
                ch,
                slaveAddr,
                memAddr,
                memCnt,
                4);

            if (data == null || data.Length != 4)
                return int.MinValue;

            uint unsignedValue = ((uint)data[0] << 24) |
                                 ((uint)data[1] << 16) |
                                 ((uint)data[2] << 8) |
                                 data[3];

            return unchecked((int)unsignedValue);
        }

        /// <summary>
        /// IDlnInterface compatibility overload. memCnt is obtained from the target map.
        /// </summary>
        public bool ReadArray(
            int ch,
            int slaveAddr,
            int memAddr,
            byte[] data)
        {
            return ReadArray(
                ch,
                slaveAddr,
                memAddr,
                GetRegisterAddressLength(ch, slaveAddr),
                data);
        }

        /// <summary>
        /// Extended overload for new code that can specify memCnt explicitly.
        /// </summary>
        public bool ReadArray(
            int ch,
            int slaveAddr,
            int memAddr,
            int memCnt,
            byte[] data)
        {
            if (data == null)
                return false;

            byte[] result = ReadRegister(
                ch,
                slaveAddr,
                memAddr,
                memCnt,
                data.Length);

            if (result == null || result.Length != data.Length)
                return false;

            if (data.Length > 0)
            {
                Buffer.BlockCopy(
                    result,
                    0,
                    data,
                    0,
                    data.Length);
            }

            return true;
        }

        /// <summary>
        /// Raw private transfer without a register-address prefix.
        /// </summary>
        public bool ReadArray(
            int ch,
            int slaveAddr,
            byte[] data)
        {
            return ReadArray(
                ch,
                slaveAddr,
                0,
                0,
                data);
        }

        #endregion

        #region I3C-specific commands

        public bool Ping()
        {
            if (IsVirtualMode())
                return true;

            byte[] response = SendAndReceive(
                CommandPing,
                null,
                true);

            bool success = IsSuccessResponse(response);
            if (success)
                RegisterCommunicationSuccess();
            else
                RegisterProtocolFailure(CommandPing, response);

            return success;
        }

        public bool RunEntdaa(int ch = 0)
        {
            if (IsVirtualMode())
            {
                LastEntdaaDeviceCount = 0;
                return true;
            }

            ValidateByteRange(ch, nameof(ch));

            byte[] response = SendAndReceive(
                CommandEntdaa,
                new byte[] { (byte)ch },
                false);

            if (!IsSuccessResponse(response))
            {
                LastEntdaaDeviceCount = 0;
                RegisterProtocolFailure(CommandEntdaa, response);
                return false;
            }

            LastEntdaaDeviceCount = response.Length >= 2
                ? response[1]
                : 0;

            RegisterCommunicationSuccess();
            return true;
        }

        public bool RunEntdaa(int ch, out int deviceCount)
        {
            bool result = RunEntdaa(ch);
            deviceCount = LastEntdaaDeviceCount;
            return result;
        }

        public I3cDeviceInfo[] GetDeviceTable(int ch = 0)
        {
            if (IsVirtualMode())
                return new I3cDeviceInfo[0];

            ValidateByteRange(ch, nameof(ch));

            byte[] response = SendAndReceive(
                CommandDeviceTable,
                new byte[] { (byte)ch },
                true);

            if (!IsSuccessResponse(response) || response.Length < 2)
            {
                RegisterProtocolFailure(CommandDeviceTable, response);
                return new I3cDeviceInfo[0];
            }

            int deviceCount = response[1];
            int expectedLength = 2 + deviceCount * DeviceRecordLength;

            if (response.Length != expectedLength)
            {
                SetError(string.Format(
                    "DT response length mismatch. Expected={0}, Actual={1}",
                    expectedLength,
                    response.Length));
                return new I3cDeviceInfo[0];
            }

            I3cDeviceInfo[] devices = new I3cDeviceInfo[deviceCount];
            int offset = 2;

            for (int i = 0; i < deviceCount; i++)
            {
                ulong provisionalId = 0;
                for (int pidIndex = 0; pidIndex < 6; pidIndex++)
                {
                    provisionalId =
                        (provisionalId << 8) |
                        response[offset + 4 + pidIndex];
                }

                devices[i] = new I3cDeviceInfo
                {
                    DynamicAddress = response[offset],
                    StaticAddress = response[offset + 1],
                    Bcr = response[offset + 2],
                    Dcr = response[offset + 3],
                    ProvisionalId = provisionalId
                };

                offset += DeviceRecordLength;
            }

            RegisterCommunicationSuccess();
            return devices;
        }

        public bool ResetI3cBus(int ch = 0)
        {
            if (IsVirtualMode())
                return true;

            ValidateByteRange(ch, nameof(ch));

            byte[] response = SendAndReceive(
                CommandBusReset,
                new byte[] { (byte)ch },
                false);

            bool success = IsSuccessResponse(response);
            if (success)
                RegisterCommunicationSuccess();
            else
                RegisterProtocolFailure(CommandBusReset, response);

            return success;
        }

        public byte[] GetVersionRaw()
        {
            if (IsVirtualMode())
                return new byte[] { (byte)Ra8m1Status.Ok, 0, 0, 0, 0 };

            byte[] response = SendAndReceive(
                CommandVersion,
                null,
                true);

            if (!IsSuccessResponse(response))
            {
                RegisterProtocolFailure(CommandVersion, response);
                return null;
            }

            RegisterCommunicationSuccess();
            return response;
        }

        public string GetVersion()
        {
            byte[] response = GetVersionRaw();
            if (response == null || response.Length < 1)
                return string.Empty;

            if (response.Length >= 5)
            {
                return string.Format(
                    "Protocol {0}.{1}, Firmware {2}.{3}",
                    response[1],
                    response[2],
                    response[3],
                    response[4]);
            }

            return "RA8M1 I3C";
        }

        #endregion

        #region Other IDlnInterface methods

        public byte[] RunInternalSequence(
            string cmd,
            byte[] payload = null)
        {
            if (IsVirtualMode())
                return new byte[0];

            // The side effects of an arbitrary internal command are unknown,
            // so it is not automatically resent after an ambiguous failure.
            return SendAndReceive(cmd, payload, false);
        }

        public bool ChangeSlaveAddrUnified(
            int ch,
            byte origin,
            byte target,
            byte pinMode,
            bool isAF)
        {
            if (IsVirtualMode())
                return true;

            ValidateByteRange(ch, nameof(ch));

            byte[] payload =
            {
                origin,
                target,
                pinMode,
                (byte)(isAF ? 1 : 0)
            };

            byte[] response = SendAndReceive(
                CommandChangeAddress,
                payload,
                false);

            if (!IsSuccessResponse(response) || response.Length < 2)
            {
                RegisterProtocolFailure(CommandChangeAddress, response);
                SetError(isAF
                    ? "AF legacy address change failed."
                    : "OIS legacy address change failed.");
                return false;
            }

            byte foundAddress = response[1];

            if (isAF)
            {
                Log(ch, "IC Address check OK");
                Log(ch, string.Format(
                    "I3C/I2C address change from 0x{0:X2} to 0x{1:X2}",
                    foundAddress,
                    target));
            }
            else
            {
                string label = pinMode == 0x02 ? "X" : "Y";
                Log(ch, string.Format(
                    "{0} legacy slave-address change finished: 0x{1:X2}",
                    label,
                    target));
            }

            RegisterCommunicationSuccess();
            return true;
        }

        // The current RA8M1 test equipment does not use GPIO, sockets,
        // cover control, power control, current measurement, LED or peak detector.
        public void PowerOnOff(int port, bool IsOn = true)
        {
        }

        public void LoadSocket()
        {
        }

        public void UnloadSocket()
        {
        }

        public void CoverDn()
        {
        }

        public void CoverUp()
        {
        }

        public void SetSocketSensor(bool isOn)
        {
        }

        public bool GetGpioStatus(int input)
        {
            return false;
        }

        public void PeakDetector(
            int ADCNumber,
            PeakDetectState state)
        {
        }

        public double GetCurrent(int ch, int mode)
        {
            return 0.0;
        }

        public void SetLEDpower(int id, int value)
        {
        }

        #endregion

        #region Error/log helpers

        public void SetError(string s)
        {
            _lastError = s ?? string.Empty;
            Log(0, "[RA8M1-I3C] " + _lastError);
        }

        private bool IsSuccessResponse(byte[] response)
        {
            if (response == null || response.Length < 1)
                return false;

            _lastStatus = response[0];
            return response[0] == (byte)Ra8m1Status.Ok;
        }

        private void RegisterProtocolFailure(
            string command,
            byte[] response)
        {
            if (response == null || response.Length == 0)
            {
                SetError(command + " failed: no response.");
                IncrementI2cFailureCount();
                return;
            }

            _lastStatus = response[0];
            SetError(string.Format(
                "{0} failed. Status={1} ({2})",
                command,
                response[0],
                GetStatusName(response[0])));
            IncrementI2cFailureCount();
        }

        private void RegisterCommunicationFailure(
            string operation,
            Exception ex)
        {
            SetError(string.Format(
                "{0} communication failed: {1}",
                operation,
                ex == null ? "Unknown error" : ex.Message));
            IncrementI2cFailureCount();
        }

        private static string GetStatusName(byte status)
        {
            if (Enum.IsDefined(typeof(Ra8m1Status), status))
                return ((Ra8m1Status)status).ToString();

            return "Unknown";
        }

        private void RegisterCommunicationSuccess()
        {
            ClearLastError();

            try
            {
                STATIC.I2CFailcnt = 0;
            }
            catch
            {
            }
        }

        private void IncrementI2cFailureCount()
        {
            try
            {
                STATIC.I2CFailcnt++;
            }
            catch
            {
            }
        }

        private void ClearLastError()
        {
            _lastError = string.Empty;
        }

        private static void Log(int channel, string message)
        {
            try
            {
                if (STATIC.Process != null)
                    STATIC.Process.AddLog(channel, message);
            }
            catch
            {
            }
        }

        private static bool IsVirtualMode()
        {
            try
            {
                return STATIC.Process != null &&
                       STATIC.Process.IsVirtual;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Binary helpers / validation

        private static void WriteInt32LittleEndian(
            byte[] buffer,
            int offset,
            int value)
        {
            buffer[offset] = (byte)(value & 0xFF);
            buffer[offset + 1] = (byte)((value >> 8) & 0xFF);
            buffer[offset + 2] = (byte)((value >> 16) & 0xFF);
            buffer[offset + 3] = (byte)((value >> 24) & 0xFF);
        }

        private static int ReadInt32LittleEndian(
            byte[] buffer,
            int offset)
        {
            return buffer[offset] |
                   (buffer[offset + 1] << 8) |
                   (buffer[offset + 2] << 16) |
                   (buffer[offset + 3] << 24);
        }

        private static void WriteUInt16LittleEndian(
            byte[] buffer,
            int offset,
            ushort value)
        {
            buffer[offset] = (byte)(value & 0xFF);
            buffer[offset + 1] = (byte)((value >> 8) & 0xFF);
        }

        private static void ValidateByteRange(
            int value,
            string parameterName)
        {
            if (value < byte.MinValue || value > byte.MaxValue)
                throw new ArgumentOutOfRangeException(parameterName);
        }

        private static void ValidateRegisterAddressLength(
            int memCnt,
            bool allowZero)
        {
            int minimum = allowZero ? 0 : 1;

            if (memCnt < minimum || memCnt > 4)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(memCnt),
                    "Register-address length must be between " +
                    minimum + " and 4 bytes.");
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(Ra8m1I3cAdapter));
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            if (_disposed)
                return;

            lock (_communicationLock)
            {
                if (_disposed)
                    return;

                CloseConnectionCore();
                _disposed = true;
            }

            GC.SuppressFinalize(this);
        }

        #endregion
    }
}