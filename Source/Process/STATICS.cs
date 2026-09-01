using FZ4P.BarcodeReader;
using FZ4P.BarcodeReader.CommandLine.Keyens;
using FZ4P.BarcodeReader.FileSelector;
using FZ4P.UI;
using FZ4P.DriverIc.Adapter;
using Modules.Communication.Context;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

namespace FZ4P
{
    public enum DeviceType
    {
        DLN,
        ESP32,
        I3C
    }

    public class DeviceConfig
    {
        public DeviceType SelectedType { get; set; } = DeviceType.DLN;

        // ESP32
        public string Esp32Ip { get; set; } = "192.168.1.101";
        public int Esp32Port { get; set; } = 8080;

        // RA8M1 Ethernet-to-I3C
        public string Ra8m1Ip { get; set; } = "192.168.0.10";
        public int Ra8m1Port { get; set; } = 8080;
        public int Ra8m1ConnectTimeoutMs { get; set; } = 2000;
        public int Ra8m1RequestTimeoutMs { get; set; } = 2000;

        // IDlnInterface의 WriteArray/ReadArray에는 memCnt가 없으므로
        // 등록되지 않은 I3C Target에 적용할 기본 Register Address 길이입니다.
        public int I3cDefaultRegisterAddressLength { get; set; } = 1;
    }

    public static class DeviceFactory
    {
        public static void CreateDevices(
            DeviceConfig config,
            out IDlnInterface mainDln,
            out IDlnInterface lightDln)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            mainDln = null;

            // 이번 I3C 테스트 장비에서는 조명을 사용하지 않습니다.
            // 기존 코드의 STATIC.LightDln 참조 호환성을 위해 필드는 유지하되
            // 실제 장치는 생성하지 않습니다.
            lightDln = null;

            switch (config.SelectedType)
            {
                case DeviceType.DLN:
                    mainDln = new DlnAdapter(new DLN(0));
                    break;

                case DeviceType.ESP32:
                    mainDln = new Esp32WifiDevice(
                        config.Esp32Ip,
                        config.Esp32Port);
                    break;

                case DeviceType.I3C:
                    Ra8m1I3cAdapter i3cAdapter =
                        new Ra8m1I3cAdapter(
                            config.Ra8m1Ip,
                            config.Ra8m1Port,
                            config.Ra8m1ConnectTimeoutMs,
                            config.Ra8m1RequestTimeoutMs,
                            false);

                    // 객체는 연결 실패 시에도 유지
                    // 이후 STATIC.Dln.Init() 또는 Read/Write 호출 시 재접속 가능
                    i3cAdapter.Init();

                    mainDln = i3cAdapter;
                    break;

                default:
                    throw new InvalidOperationException(
                        "Unsupported hardware type: " +
                        config.SelectedType);
            }
        }
    }

    public static class STATIC
    {
        public static FVision fVision = new FVision();
        public static F_Manage fManage = new F_Manage();
        //public static F_Start fStart = new F_Start();
        public static HandlerConnection TcpConn = new HandlerConnection();
        public static int I2CFailcnt = 0;
        public static int I2CFailToDisonnectCount = 0;
        public static string SaveLogData = string.Empty;

        public static F_SystemLogView fSystemLogView = new F_SystemLogView();

        public static F_Manual fManual = new F_Manual();

        public enum STATE
        {
            Manage,
            Main,
            Vision,
        }
        private static int state = 0;
        public static int State
        {
            get { return state; }
            set { if (state != value) state = value; StateChange?.Invoke(null, EventArgs.Empty); }
        }

        public static event EventHandler StateChange = null;

        public static string BaseDir = "C:\\I3CTester\\";
        public static string RecipeDir = BaseDir + "Recipe\\";
        public static string SpecDir = BaseDir + "Spec\\";
        public static string RootDir = BaseDir + "\\DoNotTouch\\";
        public static string DataDir = BaseDir + "\\Data\\";
        public static string UserScriptDir = BaseDir + "\\DriverIC\\FW\\";
        public static string OptionPath = RootDir + "OptionState.txt";
        public static string YieldPath = RootDir + "Yield.txt";
        public static string YieldItemPath = RootDir + "YieldItem.txt";
        public static string CurrentPath = RootDir + "CurrPath.txt";
        public static string PackageDir = BaseDir + "Package\\";
        public static string TestTimeDir = RootDir + "TestTime.txt";
        public static string VisionFileDir = RootDir + "VisionFile.txt";
        public static string RetryCountDir = RootDir + "RetryCount.txt";
        public static string PasswordDir = RootDir + "PW.txt";
        public static string ScannerDir = RootDir + "ScannerType\\";
        public static string FWDir = BaseDir + "FW\\";
        public static string OISPIDDir = BaseDir + "OISPID\\";
        public static string LogResultDir = BaseDir + "Result\\";
        public static string LogErrorImageDir = LogResultDir + "Log\\";
        public static DateTime LogDate = new DateTime();
        public static string FailNumber = string.Empty;
        public static string ActID = string.Empty;

        public static string PKGRelease(string srcdir, string Ext, string destdir)
        {

            string[] Arr = Directory.GetFiles(srcdir, Ext);
            string destFile = string.Empty;
            for (int i = 0; i < Arr.Length; i++)
            {
                if (Arr[i].Contains("CurrentPath ") || Arr[i].Contains("MCInfo"))
                    continue;
                destFile = destdir + Arr[i].Substring(srcdir.Length);
                if (File.Exists(destFile))
                    File.Delete(destFile);
                File.Move(Arr[i], destFile);
            }
            return destFile;
        }
        public static string PKGRelease(string srcdir, string Ext, string destdir, string formatText)
        {

            string[] Arr = Directory.GetFiles(srcdir, Ext).Where(f => Path.GetFileName(f).Contains(formatText)).ToArray();
            string destFile = string.Empty;
            for (int i = 0; i < Arr.Length; i++)
            {
                if (Arr[i].Contains("CurrentPath ") || Arr[i].Contains("MCInfo"))
                    continue;
                destFile = destdir + Arr[i].Substring(srcdir.Length);
                if (File.Exists(destFile))
                    File.Delete(destFile);
                File.Move(Arr[i], destFile);
            }
            return destFile;
        }
        public static void SetTextLine(string path, List<string> list)
        {
            try
            {
                string FilePath = path;
                //if (!File.Exists(FilePath)) return;
                StreamWriter sw = new StreamWriter(FilePath);
                for (int i = 0; i < list.Count; i++)
                { sw.WriteLine(list[i]); }
                sw.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        public static List<string> GetTextAll(string path)
        {
            List<string> result = new List<string>();
            string FilePath = path;
            if (!File.Exists(FilePath)) return null;
            StreamReader sr = new StreamReader(FilePath);
            while (sr.Peek() >= 0)
            {
                result.Add(sr.ReadLine());
            }
            sr.Close();
            return result;
        }
        public static byte[] BinFileRead(string fileName)
        {
            byte[] reselt;
            if (fileName != "")
            {
                if (!File.Exists(fileName))
                {
                    return null;
                }
                BinaryReader binReader = new BinaryReader(File.Open(fileName, FileMode.Open));
                int count = (int)binReader.BaseStream.Length;
                reselt = binReader.ReadBytes(count);
                binReader.Close();
            }
            else
            {
                return null;
            }
            return reselt;
        }
        public static string OpenFile(string InitDir, string ext, bool save = false)
        {
            FileDialog op;
            if (save) op = new SaveFileDialog();
            else op = new OpenFileDialog();

            op.InitialDirectory = InitDir;
            if (ext != "") ext = ext.Remove(0, 1);
            op.Filter = "*." + ext + "|*." + ext;
            if (op.ShowDialog() == DialogResult.OK)
                return op.FileName;
            else return null;
        }
        public static string CreateDateDir()
        {
            DateTime dt = STATIC.LogDate;
            string dir = string.Format("{0}\\{1}\\{2}\\{3}\\", DataDir, dt.Year, dt.Month, dt.Day);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            return dir;
        }
        public static char GetEthernetIPv4()
        {
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                // Wi-Fi 제외 조건
                if (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
                    continue;

                // 비활성화된 NIC 제외
                if (ni.OperationalStatus != OperationalStatus.Up)
                    continue;

                // IPv4 검색
                foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
                {
                    if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                    {

                        string s = ip.Address.ToString();

                        return s[s.Length - 1];
                    }
                }
            }
            return '0';
        }



        public static Recipe Rcp = new Recipe();
        public static Process Process = new Process();

        // 상위 검사 코드는 하드웨어 종류와 관계없이 기존처럼 STATIC.Dln을 사용합니다.
        public static IDlnInterface Dln;

        // 이번 테스트 장비에서는 조명을 사용하지 않지만,
        // 기존 소스의 참조 호환성을 위해 필드는 유지합니다. 값은 항상 null입니다.
        public static IDlnInterface LightDln;

        public static DeviceConfig HardwareConfig = new DeviceConfig();
        public static string ConfigPath = RootDir + "HardwareConfig.xml";

        public static bool IsI3cMode
        {
            get
            {
                return HardwareConfig != null &&
                       HardwareConfig.SelectedType == DeviceType.I3C;
            }
        }

        /// <summary>
        /// I3C 전용 기능이 필요할 때만 사용합니다.
        /// 일반 Register Read/Write는 계속 STATIC.Dln을 사용하면 됩니다.
        /// </summary>
        public static Ra8m1I3cAdapter I3cAdapter
        {
            get { return Dln as Ra8m1I3cAdapter; }
        }

        public static void Initialize()
        {
            LoadHardwareConfig();
            ReleaseHardware();

            try
            {
                DeviceFactory.CreateDevices(
                    HardwareConfig,
                    out Dln,
                    out LightDln);

                if (Dln == null)
                    throw new InvalidOperationException(
                        "DeviceFactory returned a null main device.");

                if (Process != null)
                {
                    Process.AddLog(
                        0,
                        string.Format(
                            "[Hardware] Selected={0}",
                            HardwareConfig.SelectedType));

                    Ra8m1I3cAdapter i3c = I3cAdapter;
                    if (i3c != null)
                    {
                        Process.AddLog(
                            0,
                            string.Format(
                                "[RA8M1-I3C] {0}:{1}, Connected={2}",
                                HardwareConfig.Ra8m1Ip,
                                HardwareConfig.Ra8m1Port,
                                i3c.IsConnected));
                    }
                }
            }
            catch (Exception ex)
            {
                ReleaseHardware();

                if (Process != null)
                {
                    Process.AddLog(
                        0,
                        "[Hardware] Create failed: " + ex.Message);
                }
            }
        }

        private static void LoadHardwareConfig()
        {
            DeviceConfig loadedConfig = null;

            if (File.Exists(ConfigPath))
            {
                loadedConfig =
                    DataIO.DeserializeXMLFileToObject<DeviceConfig>(
                        ConfigPath);
            }

            HardwareConfig = loadedConfig ?? new DeviceConfig();
            NormalizeHardwareConfig(HardwareConfig);

            // 예전 XML에 신규 I3C 항목이 없었던 경우 기본값을 추가하고,
            // 더 이상 사용하지 않는 구형 항목은 제거한 형태로 다시 저장합니다.
            SaveHardwareConfig();
        }

        private static void NormalizeHardwareConfig(
            DeviceConfig config)
        {
            if (config == null)
                return;

            if (string.IsNullOrWhiteSpace(config.Esp32Ip))
                config.Esp32Ip = "192.168.1.101";

            if (config.Esp32Port < 1 ||
                config.Esp32Port > 65535)
            {
                config.Esp32Port = 8080;
            }

            if (string.IsNullOrWhiteSpace(config.Ra8m1Ip))
                config.Ra8m1Ip = "192.168.0.10";

            if (config.Ra8m1Port < 1 ||
                config.Ra8m1Port > 65535)
            {
                config.Ra8m1Port = 8080;
            }

            if (config.Ra8m1ConnectTimeoutMs <= 0)
                config.Ra8m1ConnectTimeoutMs = 2000;

            if (config.Ra8m1RequestTimeoutMs <= 0)
                config.Ra8m1RequestTimeoutMs = 2000;

            if (config.I3cDefaultRegisterAddressLength < 1 ||
                config.I3cDefaultRegisterAddressLength > 4)
            {
                config.I3cDefaultRegisterAddressLength = 1;
            }
        }

        public static bool SaveHardwareConfig()
        {
            if (HardwareConfig == null)
                HardwareConfig = new DeviceConfig();

            NormalizeHardwareConfig(HardwareConfig);

            return DataIO.SerializeToXMLFile(
                HardwareConfig,
                ConfigPath);
        }

        public static void ReleaseHardware()
        {
            IDlnInterface oldMain = Dln;
            IDlnInterface oldLight = LightDln;

            Dln = null;
            LightDln = null;

            DisposeDevice(oldMain);

            if (!object.ReferenceEquals(oldMain, oldLight))
                DisposeDevice(oldLight);
        }

        private static void DisposeDevice(
            IDlnInterface device)
        {
            IDisposable disposable = device as IDisposable;
            if (disposable == null)
                return;

            try
            {
                disposable.Dispose();
            }
            catch (Exception ex)
            {
                if (Process != null)
                {
                    Process.AddLog(
                        0,
                        "[Hardware] Dispose failed: " +
                        ex.Message);
                }
            }
        }

        /// <summary>
        /// 기존 IDlnInterface의 WriteArray/ReadArray에는 memCnt가 없으므로,
        /// Target별 Register Address 길이를 등록합니다.
        /// WriteByte/ReadByte 계열은 호출 인자의 memCnt를 그대로 사용합니다.
        /// </summary>
        public static bool ConfigureI3cTarget(
            int ch,
            int slaveAddr,
            int registerAddressLength)
        {
            Ra8m1I3cAdapter adapter = I3cAdapter;
            if (adapter == null)
                return false;

            try
            {
                adapter.ConfigureTarget(
                    ch,
                    slaveAddr,
                    registerAddressLength);

                return true;
            }
            catch (Exception ex)
            {
                if (Process != null)
                {
                    Process.AddLog(
                        ch,
                        "[RA8M1-I3C] ConfigureTarget failed: " +
                        ex.Message);
                }

                return false;
            }
        }

        public static bool RemoveI3cTargetConfiguration(
            int ch,
            int slaveAddr)
        {
            Ra8m1I3cAdapter adapter = I3cAdapter;
            if (adapter == null)
                return false;

            try
            {
                return adapter.RemoveRegisterAddressLengthConfiguration(
                    ch,
                    slaveAddr);
            }
            catch (Exception ex)
            {
                if (Process != null)
                {
                    Process.AddLog(
                        ch,
                        "[RA8M1-I3C] Remove target configuration failed: " +
                        ex.Message);
                }

                return false;
            }
        }

        /// <summary>
        /// DUT가 장착되고 전원이 인가된 뒤 호출합니다.
        /// </summary>
        public static bool RunI3cEntdaa(
            int ch = 0)
        {
            Ra8m1I3cAdapter adapter = I3cAdapter;
            if (adapter == null)
                return false;

            try
            {
                return adapter.RunEntdaa(ch);
            }
            catch (Exception ex)
            {
                if (Process != null)
                {
                    Process.AddLog(
                        ch,
                        "[RA8M1-I3C] ENTDAA failed: " +
                        ex.Message);
                }

                return false;
            }
        }

        public static bool RunI3cEntdaa(
            int ch,
            out int deviceCount)
        {
            deviceCount = 0;

            Ra8m1I3cAdapter adapter = I3cAdapter;
            if (adapter == null)
                return false;

            try
            {
                return adapter.RunEntdaa(
                    ch,
                    out deviceCount);
            }
            catch (Exception ex)
            {
                if (Process != null)
                {
                    Process.AddLog(
                        ch,
                        "[RA8M1-I3C] ENTDAA failed: " +
                        ex.Message);
                }

                return false;
            }
        }

        public static Ra8m1I3cAdapter.I3cDeviceInfo[]
            GetI3cDeviceTable(
                int ch = 0)
        {
            Ra8m1I3cAdapter adapter = I3cAdapter;
            if (adapter == null)
            {
                return new Ra8m1I3cAdapter.I3cDeviceInfo[0];
            }

            try
            {
                return adapter.GetDeviceTable(ch);
            }
            catch (Exception ex)
            {
                if (Process != null)
                {
                    Process.AddLog(
                        ch,
                        "[RA8M1-I3C] GetDeviceTable failed: " +
                        ex.Message);
                }

                return new Ra8m1I3cAdapter.I3cDeviceInfo[0];
            }
        }

        public static bool ResetI3cBus(
            int ch = 0)
        {
            Ra8m1I3cAdapter adapter = I3cAdapter;
            if (adapter == null)
                return false;

            try
            {
                return adapter.ResetI3cBus(ch);
            }
            catch (Exception ex)
            {
                if (Process != null)
                {
                    Process.AddLog(
                        ch,
                        "[RA8M1-I3C] Bus reset failed: " +
                        ex.Message);
                }

                return false;
            }
        }

        public static bool PingI3c()
        {
            Ra8m1I3cAdapter adapter = I3cAdapter;
            if (adapter == null)
                return false;

            try
            {
                return adapter.Ping();
            }
            catch (Exception ex)
            {
                if (Process != null)
                {
                    Process.AddLog(
                        0,
                        "[RA8M1-I3C] Ping failed: " +
                        ex.Message);
                }

                return false;
            }
        }

        public static string GetI3cVersion()
        {
            Ra8m1I3cAdapter adapter = I3cAdapter;
            if (adapter == null)
                return string.Empty;

            try
            {
                return adapter.GetVersion();
            }
            catch (Exception ex)
            {
                if (Process != null)
                {
                    Process.AddLog(
                        0,
                        "[RA8M1-I3C] Version read failed: " +
                        ex.Message);
                }

                return string.Empty;
            }
        }

        public static bool WriteI3cArrayNoResponse(
            int ch,
            int slaveAddr,
            int memAddr,
            int memCnt,
            byte[] data)
        {
            Ra8m1I3cAdapter adapter = I3cAdapter;
            if (adapter == null)
                return false;

            try
            {
                return adapter.WriteArrayNoResponse(
                    ch,
                    slaveAddr,
                    memAddr,
                    memCnt,
                    data);
            }
            catch (Exception ex)
            {
                if (Process != null)
                {
                    Process.AddLog(
                        ch,
                        "[RA8M1-I3C] No-response write failed: " +
                        ex.Message);
                }

                return false;
            }
        }

        public static AK73XX DrvIC = new AK73XX();

        public static Scanner Scanner = new Scanner(new SR_X300());
        public static CommunicationTypeSelector ScannerParamSelector = new CommunicationTypeSelector(ScannerDir);
    }
    public static class DataIO
    {
        public static string SerializeToXML<T>(this T toSerialize)
        {
            try
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
                XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                using (var ms = new MemoryStream())
                {
                    using (var xw = XmlWriter.Create(ms, new XmlWriterSettings()
                    {
                        Encoding = new UTF8Encoding(false),
                        Indent = true,
                    }))
                    {
                        xmlSerializer.Serialize(xw, toSerialize, ns);
                        return Encoding.UTF8.GetString(ms.ToArray());
                    }
                }
            }
            catch
            { return string.Empty; }

        }
        public static bool SerializeToXMLFile<T>(this T toSerialize, string FileName) where T : class, new()
        {
            try
            {
                string dir = Path.GetDirectoryName(FileName);
                try { Directory.CreateDirectory(dir); }
                catch
                { return false; }
                string backFile = Path.ChangeExtension(FileName, ".bak");
                if (File.Exists(backFile))
                    File.Delete(backFile);
                try { File.WriteAllText(backFile, toSerialize.SerializeToXML<T>()); }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                    return false;
                }
                FileInfo info = new FileInfo(backFile);
                if (info.Length == 0)
                { return false; }

                if (File.Exists(FileName))
                    File.Delete(FileName);
                File.Move(backFile, FileName);
                return true;
            }
            catch { return false; }
        }
        public static object Deserialize<T>(this string toDeserialize) where T : class, new()
        {
            try
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
                using (StringReader txtReader = new StringReader(toDeserialize))
                {
                    return xmlSerializer.Deserialize(txtReader);
                }
            }
            catch
            { return default(T); }
        }
        public static T DeserializeXMLFileToObject<T>(string FileName) where T : class, new()
        {
            try
            {
                string xml = File.ReadAllText(FileName);
                return xml.Deserialize<T>() as T;
            }
            catch
            {
                return default(T);
            }
        }

        public static T GetEnumArttribute<T>(Enum val) where T : Attribute
        {
            Type enumT = val.GetType();
            string enumName = Enum.GetName(enumT, val);
            if (enumName != null)
            {
                FieldInfo finfo = enumT.GetField(enumName);
                if (finfo != null)
                {
                    T attri = (T)Attribute.GetCustomAttribute(finfo, typeof(T));
                    return attri;
                }
            }

            return null;
        }
        public static T GetCustomAttribute<T>(PropertyDescriptor p) where T : Attribute
        {
            T attri = (T)p.Attributes[typeof(T)];
            return attri;

        }
    }
}