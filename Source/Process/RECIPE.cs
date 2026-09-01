using OpenCvSharp.Flann;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;
using System.Windows.Forms.VisualStyles;

namespace FZ4P
{
    public class Recipe
    {
        public CurrentPath Current { get; set; }
        public Condition Condition { get; set; }
        public FWSet FWSet { get; set; } = new FWSet();
        public XPidSet XPidSet { get; set; } = new XPidSet();
        public YPidSet YPidSet { get; set; } = new YPidSet();
        public CodeScript CodeScript { get; set; }
        public Spec Spec { get; set; }
        public Model Model { get; set; }
        public Option Option { get; set; }
        public List<PassFail> PassFails { get; set; }
        public TotalYield yield { get; set; }
        public TestTime tt { get; set; }
        public VisionFile vsFile { get; set; }
        public RetryCount RetryCnt { get; set; }
        public Password pw { get; set; }
        public List <NewYield> YieldItem { get; set; }
   

        public Recipe()
        {
            Current = new CurrentPath();
            if (File.Exists(STATIC.CurrentPath))
                Current = DataIO.DeserializeXMLFileToObject<CurrentPath>(STATIC.CurrentPath);

            if (!Directory.Exists(STATIC.RootDir)) Directory.CreateDirectory(STATIC.RootDir);
            if (!Directory.Exists(STATIC.DataDir)) Directory.CreateDirectory(STATIC.DataDir);
            if (!Directory.Exists(STATIC.RecipeDir)) Directory.CreateDirectory(STATIC.RecipeDir);
            if (!Directory.Exists(STATIC.SpecDir)) Directory.CreateDirectory(STATIC.SpecDir);
            if (!Directory.Exists(STATIC.PackageDir)) Directory.CreateDirectory(STATIC.PackageDir);
            if (!Directory.Exists(STATIC.ScannerDir)) Directory.CreateDirectory(STATIC.ScannerDir);
            if (!Directory.Exists(STATIC.FWDir)) Directory.CreateDirectory(STATIC.OISPIDDir);
            if (!Directory.Exists(STATIC.FWDir)) Directory.CreateDirectory(STATIC.FWDir);
            if (!Directory.Exists(STATIC.LogResultDir)) Directory.CreateDirectory(STATIC.LogResultDir);
            if (!Directory.Exists(STATIC.LogErrorImageDir)) Directory.CreateDirectory(STATIC.LogErrorImageDir);

            string res = string.Empty;
            res = STATIC.PKGRelease(STATIC.PackageDir, "*.rcp", STATIC.RecipeDir);
            if (res != string.Empty) Current.ConditionName = Path.GetFileName(res);
            else Current.ConditionName = "Default.rcp";
                res = STATIC.PKGRelease(STATIC.PackageDir, "*.spc", STATIC.SpecDir);
            if (res != string.Empty) Current.SpecName = Path.GetFileName(res);
            else Current.SpecName = "Default.spc";
            res = STATIC.PKGRelease(STATIC.PackageDir, "*.txt", STATIC.RootDir);

            string AFPIDpath = STATIC.PackageDir + "AFPID\\";
            if (!Directory.Exists(AFPIDpath)) Directory.CreateDirectory(AFPIDpath);
            string OISPIDpath = STATIC.PackageDir + "OISPID\\";
            if (!Directory.Exists(OISPIDpath)) Directory.CreateDirectory(OISPIDpath);

            res = STATIC.PKGRelease(STATIC.PackageDir+"AFPID\\", "*.txt", STATIC.FWDir);
            if (res != string.Empty) Current.FWPath = Path.GetFileName(res);
            res = STATIC.PKGRelease(STATIC.PackageDir + "OISPID\\", "*.txt", STATIC.OISPIDDir, "_X");
            if (res != string.Empty) Current.XPidPath = STATIC.OISPIDDir + Path.GetFileName(res);
            res = STATIC.PKGRelease(STATIC.PackageDir + "OISPID\\", "*.txt", STATIC.OISPIDDir, "_Y");
            if (res != string.Empty) Current.YPidPath = STATIC.OISPIDDir + Path.GetFileName(res);



            Current.SerializeToXMLFile(STATIC.CurrentPath);

            Condition = new Condition();
            if (File.Exists(STATIC.RecipeDir + Current.ConditionName))
                Condition = DataIO.DeserializeXMLFileToObject<Condition>(STATIC.RecipeDir + Current.ConditionName);

            Spec = new Spec();
            Spec.InitSpecList();
            if (File.Exists(STATIC.SpecDir + Current.SpecName))
            {
                Spec compare = new Spec();
                compare = DataIO.DeserializeXMLFileToObject<Spec>(STATIC.SpecDir + Current.SpecName);
                for (int i = 0; i < compare.specList.Count; i++)
                {
                    int index = Spec.specList.FindIndex(x => x.DisplayName == compare.specList[i].DisplayName);
                    if (index != -1)
                    {
                        Spec.specList[index].MinSpec = compare.specList[i].MinSpec;
                        Spec.specList[index].MaxSpec = compare.specList[i].MaxSpec;
                        Spec.specList[index].OnOff = compare.specList[i].OnOff;
                        Spec.specList[index].FailCnt = compare.specList[i].FailCnt;
                        //Spec.specList[index].InspectionType = compare.specList[i].InspectionType;
                    }
                }
            }

            if (File.Exists(Current.FWPath))
                FWSet.Read(Current.FWPath);
            if (File.Exists(Current.XPidPath))
                XPidSet.Read(Current.XPidPath);
            if (File.Exists(Current.YPidPath))
                YPidSet.Read(Current.YPidPath);

            //AFPidSet = new AFPidSet();
            //AfPidSet.Init(Current.AFPidPath, "PID\\");
            //XPidSet = new XPidSet();
            //XPidSet.Init(Current.XPidPath, "PID\\");
            //YPidSet = new YPidSet();
            //YPidSet.Init(Current.YPidPath, "PID\\");
            //CodeScript = new CodeScript();
            //CodeScript.Init(Current.CodeScriptPath, "PID\\");

            Model = new Model();

            Option = new Option();
            if(File.Exists(STATIC.OptionPath))
                Option = DataIO.DeserializeXMLFileToObject<Option>(STATIC.OptionPath);

            vsFile = new VisionFile();
            if (File.Exists(STATIC.VisionFileDir))
                vsFile = DataIO.DeserializeXMLFileToObject<VisionFile>(STATIC.VisionFileDir);
            else DataIO.SerializeToXMLFile(vsFile, STATIC.VisionFileDir);

            yield = new TotalYield();
            if (File.Exists(STATIC.YieldPath))
                yield = DataIO.DeserializeXMLFileToObject<TotalYield>(STATIC.YieldPath);

            YieldItem = new List<NewYield>();
            if (File.Exists(STATIC.YieldItemPath))
                YieldItem = DataIO.DeserializeXMLFileToObject<List<NewYield>>(STATIC.YieldItemPath);


            PassFails = new List<PassFail>();
            for (int i = 0; i < 2; i++)
            {
                PassFails.Add(new PassFail());
                for (int j = 0; j < (int)SpecItem.Length; j++) PassFails[i].Results.Add(new ResultItems());
            }
            tt = new TestTime();
            if (File.Exists(STATIC.TestTimeDir)) tt = DataIO.DeserializeXMLFileToObject<TestTime>(STATIC.TestTimeDir);
           
            RetryCnt = new RetryCount();

            pw = new Password();
            if (File.Exists(STATIC.PasswordDir))
                pw = DataIO.DeserializeXMLFileToObject<Password>(STATIC.PasswordDir);
        }
    }
    public class BaseRecipe
    {
        public List<object[]> Param = new List<object[]>();
        public string CurrentName { get; set; }
        public string FilePath { get; set; }
        public string[] ReadArry { get; set; }
        public bool bChange = false;
        public string InitDir { get; set; }
        public string Ext { get; set; }
        public virtual void Init(string current, string subDir)
        {
            if (!Directory.Exists(STATIC.BaseDir)) Directory.CreateDirectory(STATIC.BaseDir);
            InitDir = STATIC.BaseDir + subDir;
            Ext = Path.GetExtension(current);
            if (!Directory.Exists(InitDir)) Directory.CreateDirectory(InitDir);
            FilePath = CurrentName = current;

            //CurrentName = current;
            if (!File.Exists(FilePath)) Save(FilePath);

            Read(FilePath);
        }
        public virtual void Save(string filePath = "")
        {
        }
        public virtual void Read(string filePath = "")
        {
            if (!Directory.Exists(STATIC.RootDir)) Directory.CreateDirectory(STATIC.RootDir);
        }
        public virtual void SetParam()
        {
        }
        public virtual void SetParam(string key, string comment, object val)
        {
            for(int i = 0; i < Param.Count; i++)
            {
                if (Param[i][0].ToString() == key && Param[i][1].ToString() == comment)
                {
                    Param[i][2] = val;
                }
                if (Param[i][0].ToString() == key && comment == "")
                {
                    Param[i][1] = val;
                }
            }
        }
    }

    public class TestTime
    {
        public double CurrentST { get; set; } = 0;

        public double St { get; set; } = 0;
        public int Count { get; set; } = 0;
    }
    public class Option
    {
        [Option("HallCal DataUpdate")] public bool HallCalDataUpdate { get; set; }
        [Option("Reset EPA")] public bool ResetEPA { get; set; }
    }
    public class Condition
    {
        [Condition("ToDoList", "", "", "", "")] public List<string> ToDoList { get; set; } = new List<string>();
        [Condition("FW Ver Check", "FW Ver", "", "", "dec")] public int iFWversionCheck { get; set; } = 1;
        [Condition("Hall Cal", "Hall Cal Count", "Hall Calibration", "Hall Calibration MOD", "cnt")] public int HallCalCount { get; set; } = 1;
        [Condition("Hall Cal", "X offset Init", "Hall Calibration MOD", "", "dec")] public int XOffsetInit { get; set; } = -1;
        [Condition("Hall Cal", "X Offset TH", "Hall Calibration MOD", "", "dec")] public int XOffsetTH { get; set; } = -1;
        [Condition("Hall Cal", "Y Offset Init", "Hall Calibration MOD", "", "dec")] public int YOffsetInit { get; set; } = -1;
        [Condition("Hall Cal", "Y Offset TH", "Hall Calibration MOD", "", "dec")] public int YOffsetTH { get; set; } = -1;
        [Condition("Hall Move", "X Taeget", "Hall Move", "", "Code")] public int XMoveTarget { get; set; } = 4000;
        [Condition("Hall Move", "Y Taeget", "Hall Move", "", "Code")] public int YMoveTarget { get; set; } = 4000;
        [Condition("Hall Move", "Delay", "Delay", "", "ms")] public int MoveHallDelay { get; set; } = 30;
        [Condition("Hall Deviation", "Loop", "Hall Deviation", "", "cnt")] public int Retry_HallDeviation { get; set; } = 1;
        [Condition("Hall Deviation", "Initial Delay", "Hall Deviation", "", "msec")] public double HallDEV_InitialDelay { get; set; } = 1000;
        [Condition("Hall Deviation", "Read Interval", "Hall Deviation", "", "msec")] public double HallDEV_Interval { get; set; } = 10;
        [Condition("Hall Deviation", "Sampling", "Hall Deviation", "", "cnt")] public double HallDEV_sampling { get; set; } = 100;
        [Condition("Hall Deviation", "Target X", "Hall Deviation", "", "code")] public double HallDEV_TargetX { get; set; } = 16000;
        [Condition("Hall Deviation", "Target Y", "Hall Deviation", "", "code")] public double HallDEV_TargetY { get; set; } = 16000;
    }
    public class RetryCount
    {
        public List<Retry> RetryOption = new List<Retry>();

    }
    public class Retry
    {
        public string InspName { get; set; }
        public int Count { get; set; }
    }

    public enum InspType
    {
        Normal, 
        OKNG,
        OnlyMin,
        OnlyMax,
        MintoMax,
        BothXYFail,
    }

  
    public enum SpecItem
    {
        [Spec("AF> FW Update", "any", InspType.OKNG, "FW Update")] FW_DOWNLOAD,
        [Spec("AF> FW Verison Read", "any", InspType.OKNG, "FW Version Read")] FW_VERSION_READ,
        [Spec("XY> HallCalibration", "", InspType.OKNG, "Hall Cal")] HALL_CALIBRATION,
        [Spec("X> Hall Range", "", InspType.Normal, "Hall Cal")] OISX_HallRange,
        [Spec("Y> Hall Range", "", InspType.Normal, "Hall Cal")] OISY_HallRange,
        [Spec("X> Hall Dev", "", InspType.Normal, "Hall Deviation")] OISX_HALLDEV,
        [Spec("Y> Hall Dev", "", InspType.Normal, "Hall Deviation")] OISY_HALLDEV,
        Length,
    };
   
    public class Spec
    {
        public List<SpecArray> specList { get; set; } = new List<SpecArray>();
        public void InitSpecList()
        {
            specList.Clear();
            for (int i = 0; i < (int)SpecItem.Length; i++)
            {
                SpecItem s = (SpecItem)i;
                specList.Add(new SpecArray());
              
                specList[i].Unit = DataIO.GetEnumArttribute<SpecAttribute>(s)?.Unit;
                specList[i].DisplayName = DataIO.GetEnumArttribute<SpecAttribute>(s)?.DisplayName;
                specList[i].InspectionType = (InspType)DataIO.GetEnumArttribute<SpecAttribute>(s)?.InspType;
                specList[i].Category = DataIO.GetEnumArttribute<SpecAttribute>(s)?.Category;
            }
        }

    }

    public class SpecArray
    {
        public double MinSpec { get; set; } = 0;
        public double MaxSpec { get; set; } = 0;
        public bool OnOff { get; set; } = false;
        public string Category { get; set; }
        public string DisplayName { get; set; }
        public string Unit { get; set; }
        public int FailCnt { get; set; }

        public InspType InspectionType { get; set; }
    }

    public class TotalYield
    {
        public int LastSampleNum { get; set; }
        public int TotlaTested { get; set; }
        public int TotlaPassed { get; set; }
        public int TotlaFailed { get; set; }

    }
    public class ResultItems
    {
        public double Val = double.MaxValue;
        public bool bPass = true;
        public string msg = "";
    }
    public class PassFail
    {
        public int FirstFailIndex;
        public string FirstFail;    
        public string TotalTime;
        public List<ResultItems> Results = new List<ResultItems>();
    }

    public class ReCount
    {
        public string testName = string.Empty;
        public int Count = 0;
    }

    public class FWSet : BaseRecipe
    {
        public FWSet()
        {
            Param.Add(new object[] { "11", "2D" });
        }
        public override void Save(string filePath = "")
        {
            if (filePath != "") FilePath = filePath;
            StreamWriter sw = new StreamWriter(FilePath);
            sw.WriteLine("Addr\tData");
            for (int i = 0; i < Param.Count; i++)
            {
                string data = string.Format("{0}\t{1}", Param[i][0], Param[i][1]);
                sw.WriteLine(data);
            }
            sw.Close();

            Read();

            bChange = true;
        }
        public override void Read(string filePath = "")
        {
            if (filePath == "" || filePath == null) throw new ArgumentException($"파라미터의 값을 확인 바랍니다 [FilePaht : {filePath}]");
            
            FilePath = filePath;
            CurrentName = Path.GetFileName(FilePath);
            InitDir = Path.GetDirectoryName(FilePath);

            StreamReader sr = new StreamReader(FilePath);

            ReadArry = sr.ReadToEnd().Split('\r');

            Param.Clear();

            int Arryindex = 0;
            int Paramindex = 0;
            while (true)
            {
                if (Arryindex >= ReadArry.Length) break;
                if (ReadArry[Arryindex] == "\n") break;
                string[] arry = ReadArry[Arryindex].Split('\t');
                for (int i = 0; i < arry.Length; i++) arry[i] = arry[i].Trim();
                if (arry[0] == "Addr") { Arryindex++; continue; }
                Param.Add(new object[arry.Length]);
                for (int i = 0; i < arry.Length; i++)
                {
                    Param[Paramindex][i] = arry[i];
                }
                Arryindex++;
                Paramindex++;
            }
            sr.Close();
        }
    }
    public class XPidSet : BaseRecipe
    {
        public XPidSet()
        {
            Param.Add(new object[] { "10", "1E" });
        }
        public override void Save(string filePath = "")
        {
            if (filePath != "") FilePath = filePath;
            StreamWriter sw = new StreamWriter(FilePath);
            sw.WriteLine("Addr\tData");
            for (int i = 0; i < Param.Count; i++)
            {
                string data = string.Format("{0}\t{1}", Param[i][0], Param[i][1]);
                sw.WriteLine(data);
            }
            sw.Close();

            Read();

            bChange = true;
        }
        public override void Read(string filePath = "")
        {
            if (filePath != "")
            {
                FilePath = filePath;
                CurrentName = Path.GetFileName(FilePath);
                InitDir = Path.GetDirectoryName(FilePath);
            }
            StreamReader sr = new StreamReader(FilePath);

            ReadArry = sr.ReadToEnd().Split('\r');

            Param.Clear();

            int Arryindex = 0;
            int Paramindex = 0;
            while (true)
            {
                if (Arryindex >= ReadArry.Length) break;
                if (ReadArry[Arryindex] == "\n") break;
                string[] arry = ReadArry[Arryindex].Split('\t');
                for (int i = 0; i < arry.Length; i++) arry[i] = arry[i].Trim();
                if (arry[0] == "Addr") { Arryindex++; continue; }
                Param.Add(new object[arry.Length]);
                for (int i = 0; i < arry.Length; i++)
                {
                    Param[Paramindex][i] = arry[i];
                }

                Arryindex++;
                Paramindex++;
            }
            sr.Close();
        }
    }
    public class YPidSet : BaseRecipe
    {
        public YPidSet()
        {
            Param.Add(new object[] { "10", "14", "14" });
        }
        public override void Save(string filePath = "")
        {
            if (filePath != "") FilePath = filePath;
            StreamWriter sw = new StreamWriter(FilePath);
            sw.WriteLine("Addr\tY1Data\tY2Data");
            for (int i = 0; i < Param.Count; i++)
            {
                string data = string.Format("{0}\t{1}\t{1}", Param[i][0], Param[i][1], Param[i][2]);
                sw.WriteLine(data);
            }
            sw.Close();

            Read();

            bChange = true;
        }
        public override void Read(string filePath = "")
        {
            if (filePath != "")
            {
                FilePath = filePath;
                CurrentName = Path.GetFileName(FilePath);
                InitDir = Path.GetDirectoryName(FilePath);
            }
            StreamReader sr = new StreamReader(FilePath);

            ReadArry = sr.ReadToEnd().Split('\r');

            Param.Clear();

            int Arryindex = 0;
            int Paramindex = 0;
            while (true)
            {
                if (Arryindex >= ReadArry.Length) break;
                if (ReadArry[Arryindex] == "\n") break;
                string[] arry = ReadArry[Arryindex].Split('\t');
                for (int i = 0; i < arry.Length; i++) arry[i] = arry[i].Trim();
                if (arry[0] == "Addr") { Arryindex++; continue; }
                Param.Add(new object[arry.Length]);
                for (int i = 0; i < arry.Length; i++)
                {
                    Param[Paramindex][i] = arry[i];
                }
                Arryindex++;
                Paramindex++;
            }
            sr.Close();
        }
    }
    public class CodeScript : BaseRecipe
    {
        public CodeScript()
        {
            Param.Add(new object[] { "0", "0", "0", "0" });
        }
        public override void Save(string filePath = "")
        {
            if (filePath != "") FilePath = filePath;
            StreamWriter sw = new StreamWriter(FilePath);
            sw.WriteLine("Index\ttarget_X\ttarget_Y1\ttarget_Y2");
            for (int i = 0; i < Param.Count; i++)
            {
                string data = string.Format("{0}\t{1}\t{2}\t{3}", Param[i][0], Param[i][1], Param[i][2], Param[i][3]);
                sw.WriteLine(data);
            }
            sw.Close();

            Read();

            bChange = true;
        }
        public override void Read(string filePath = "")
        {
            if (filePath != "")
            {
                FilePath = filePath;
                CurrentName = Path.GetFileName(FilePath);
            }
            StreamReader sr = new StreamReader(FilePath);

            ReadArry = sr.ReadToEnd().Split('\r');

            Param.Clear();

            int Arryindex = 0;
            int Paramindex = 0;
            while (true)
            {
                if (ReadArry.Length <= Arryindex)
                    break;
                if (ReadArry[Arryindex] == "\n")
                    break;
                string[] arry = ReadArry[Arryindex].Split('\t');
                for (int i = 0; i < arry.Length; i++) arry[i] = arry[i].Trim();
                if (arry[0] == "Index") { Arryindex++; continue; }
                Param.Add(new object[arry.Length]);
                for (int i = 0; i < arry.Length; i++)
                {
                    Param[Paramindex][i] = arry[i];
                }
                Arryindex++;
                Paramindex++;
            }
            sr.Close();
        }
    }
    public class CurrentPath
    {

        public string ConditionName { get; set; } = "Default.txt";
        public string SpecName { get; set; } = "Default.txt";
        public string FWPath { get; set; } = "DefaultAF.txt";
        public string XPidPath { get; set; } = "DefaultX.txt";
        public string YPidPath { get; set; } = "DefaultY.txt";
        public string CodeScriptPath { get; set; } = "DefaultCodeScript.txt";
    }
    public class Model : BaseRecipe
    {
        public string MCNum;
        public string TesterNo;
        
        public string MCType;
        private string lotID;
        public string LotID
        {
            get { return lotID; }
            set
            {
                if (value != lotID)
                { lotID = value; IsLotChanged = true; }
                else IsLotChanged = false;
            }
        }
        public string OperatorName;

        public List<string> List = new List<string>();

        public List<string> MakerList = new List<string>();
      
      
        public List<string> SupplierList = new List<string>();
        public List<string> MCTypeList = new List<string>();


        public bool IsLotChanged = false;
        public event EventHandler Changed = null;

        public Model()
        {
            FilePath = STATIC.RootDir + "Model.txt";

            MCTypeList.Add("Normal");
            MCTypeList.Add("Master");
            MCTypeList.Add("Slave");
            MCTypeList.Add("Handler");

            Read();
        }
        public override void Read(string filePath = "")
        {
            base.Read();
            if (!File.Exists(FilePath))
            {
                List.Add("0");
                List.Add("0");
                List.Add("Normal");
               
                STATIC.SetTextLine(FilePath, List);
                SetParam();
            }
            else
            {
                List = STATIC.GetTextAll(FilePath);
                SetParam();
            }
        }
        public override void Save(string filePath = "")
        {
            List.Clear();
            List.Add(MCNum);
            List.Add(TesterNo);
            List.Add(MCType);


            STATIC.SetTextLine(FilePath, List);
        }

        public override void SetParam()
        {
            base.SetParam();
            int index = 0;
            MCNum = List[index++];
            TesterNo = List[index++];
            MCType = List[index++];

        }
        public void LotChanged()
        {
            Changed?.Invoke(null, EventArgs.Empty);
        }
    }
    public class VisionFile
    {
        public int RawGain { get; set; } = 40;
        public double Gamma { get; set; } = 0.85;
        public int Exposure { get; set; } = 73;
        public int EdgeBand { get; set; } = 9;
        public double LEDCurrentL { get; set; } = 2.05;
        public double LEDCurrentR { get; set; } = 1.9;
    }
   

    public class NewYield
    {
        public string ItemName { get; set; }
        public int FailCnt { get; set; }
    }

    public class Password
    {
        public string PW { get; set; } = "0";
        public string PW2 { get; set; } = "semco";
    }

  

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class OptionAttribute : Attribute
    {
        public string DisplayName { get; set; }
        public OptionAttribute(string des)
        {
            DisplayName = des;
        }
    }
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class ConditionAttribute : Attribute
    {
        public string Category { get; set; }
        public string DisplayName { get; set; }
        public string ToDo1 { get; set; }
        public string ToDo2 { get; set; }
        public string Unit { get; set; }
        public ConditionAttribute(string des, string des2, string des3, string des4, string des5)
        {
            Category = des;
            DisplayName = des2;
            ToDo1 = des3;
            ToDo2 = des4;
            Unit = des5;

        }
    }
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class SpecAttribute : Attribute
    {
        public string Category { get; set; }
        public string DisplayName { get; set; }
        public string Unit { get; set; }
        public InspType InspType { get; set; }
        public SpecAttribute(string des, string des2, InspType type, string des3)
        {
            Category = des3;
            DisplayName = des;
            Unit = des2;
            InspType = type;
        }
    }
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class CommonAttribute : Attribute
    {
        public string Category { get; set; }
        public string DisplayName { get; set; }
        public CommonAttribute(string des, string des2)
        {
            Category = des;
            DisplayName = des2;
        }
    }

}
