/********************************************************************************
 *               Author: Christian Müller
 *     Date of creation: 2021-08-10                                       
 *   Copyright (C) 2023: Christian Müller chrmue44(at)gmail(dot).com
 *
 *              Licence:  CC BY-NC 4.0 
 ********************************************************************************/

//https://weblog.west-wind.com/posts/2020/Apr/06/Displaying-Nested-Child-Objects-in-the-Windows-Forms-Designer-Property-Grid

using libParser;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

using BatInspector.Properties;
using System.Configuration.Install;
using NAudio.SoundFont;
using System.Deployment.Application;
using System.Reflection;

namespace BatInspector
{
  public enum enCulture
  {
    de_DE,
    en_US,
  }

  public enum enModel
  {
    BAT_DETECT2 = 3,
    BATTY_BIRD_NET = 4,
    BIRDNET = 5
  };

  public enum enZoomType
  {
    LEFT,
    CENTER
  }

  [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Module | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Event | AttributeTargets.Interface | AttributeTargets.Parameter | AttributeTargets.Delegate | AttributeTargets.ReturnValue | AttributeTargets.GenericParameter)]
  class LocalizedDescriptionAttribute : DescriptionAttribute
  {
    static string Localize(string key)
    {
      return MyResources.ResourceManager.GetString(key, MyResources.Culture);
    }

    public LocalizedDescriptionAttribute(string key)
        : base(Localize(key))
    {
    }
  }

  [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Module | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Event | AttributeTargets.Interface | AttributeTargets.Parameter | AttributeTargets.Delegate | AttributeTargets.ReturnValue | AttributeTargets.GenericParameter)]
  class LocalizedCategoryAttribute : CategoryAttribute
  {
    static string Localize(string key)
    {
      return MyResources.ResourceManager.GetString(key, MyResources.Culture);
    }

    public LocalizedCategoryAttribute(string key)
        : base(Localize(key))
    {
    }
  }


  public class SpeciesInfoConfigurationTypeConverter : TypeConverter
  {
    public override bool GetPropertiesSupported(ITypeDescriptorContext context)
    {
      return true;
    }

    public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
    {
      return TypeDescriptor.GetProperties(typeof(SpeciesInfos));
    }
  }

  [TypeConverter(typeof(ExpandableObjectConverter))]
  [DataContract]
  public class FilterParams
  {
    [DataMember]
    [Description("name of the display filter")]
    public string Name { get; set; }

    [DataMember]
    [Description("logical expression for filter")]
    public string Expression { get; set; }

    [DataMember]
    [Description("only valid, whall ALL calls in one file apply to the logical expression")]
    public bool isForAllCalls { get; set; }

    public int Index { get; set; }
  }

  public class ColorItemConfigurationTypeConverter : TypeConverter
  {
    public override bool GetPropertiesSupported(ITypeDescriptorContext context)
    {
      return true;
    }

    public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
    {
      return TypeDescriptor.GetProperties(typeof(ColorItem));
    }
  }

  [TypeConverter(typeof(ColorItemConfigurationTypeConverter))]
  [DataContract]
  public class ColorItem
  {
    public ColorItem(int col, int val)
    {
      Color = col;
      Value = val;
    }
    [DataMember]
    public int Color { get; set; }
    [DataMember]
    public int Value { get; set; }
  }

  

  [TypeConverter(typeof(ExpandableObjectConverter))]
  [DataContract]
  public class AppParams
  {
    public const string SUM_REPORT = "sum_report.csv";    // report name for sumarized report
    public const string REPORT_DATE_FORMAT = "yyyy-MM-dd"; // date format for reports
    public const string REPORT_DATETIME_FORMAT = "yyyy-MM-dd hh:mm:ss"; // date format for reports
    public const string REPORT_DATETIME_FORMAT2 = "dd.MM.yyyy HH:mm:ss"; //Date time format for reports
    public const string REPORT_DATETIME_FORMAT3 = "yyyy-MM-ddThh:mm:ss"; //Date time format for reports
    public const string GPX_DATETIME_FORMAT = "yyyy-MM-ddTHH:mm:ssZ"; // date format for reports
    public const string GPX_DATETIME_FORMAT_MS = "yyyy-MM-ddTHH:mm:ss.fffZ"; // date format with ms for reports
    public const string PRJ_REPORT = "report.csv";        // report name for project report
    public const string PRJ_SUMMARY = "summary.csv";      // report name for project summar
    public const string DIR_WAVS = "Records";             // directory for WAV files
    public const string DIR_ORIG = "orig";                // sub directory to store original files
    public const string DIR_DEL = "del";                  // sub directory to save 'deleted' files
    public const string ANNOTATION_SUBDIR = "ann";        // subdirectory for annotations for specific models
    public const string EXT_WAV = ".wav";                 // file extension for wav files 
    public const string EXT_IMG = ".png";                 // file extension for image files of recordings
    public const string EXT_INFO = ".xml";                // file extension for info files in Elekon projects
    public const string EXT_PRJ = ".bpr";                 // file extension for Elekon project file
    public const string EXT_BATSPY = ".batspy";           // file extension for BatSpy project file
    public const string EXT_QUERY = ".qry";               // file extension for query files
    public const string EXT_GPX = ".gpx";                 // file extension for GPX files
    public const string EXT_KML = ".kml";                 // file extension for GPX files
    public const string EXT_TXT = ".txt";                 // file extension for TXT files
    public const string EXT_CSV = ".csv";                 // file extension for CSV files

    public const string CSV_SEPARATOR = ";";              // CSV separator
    public const string DIR_SCRIPT = "scripts";           // sub directory containig scripts
    public const string DIR_BAT_INFO = "batInfo";         // sub directory for user editable bat info files
    public const string BAT_INFO1_PDF = "/doc/Bestimmung-Fledermausrufe-Teil1.pdf"; // Information file about bat detection
    public const string BAT_INFO2_PDF = "/doc/Bestimmung-Fledermausrufe-Teil2.pdf"; // Information file about bat detection
    public const string HELP_FILE_DE = "/doc/BatInspector_DE.pdf"; // software manual
    public const string HELP_FILE_EN = "/doc/BatInspector_EN.pdf"; // software manual
    public const string PROG_DAT_DIR = "BatInspector";    // sub directory for program data
    public const string VAR_WRK_DIR = "WRK_DIR";            // variable name for work dir
    public const string VAR_DATA_PATH = "APP_DATA_PATH";    // variable name for application data path
    public const string CMD_REDUCE_NOISE = "reduce_noise.bat"; //command to reduce the background noise in WAV file

    public const int CTLWAV_WIDTH_BIRDS = 500;            // ctlWav with of info section for bird projects
    public const int CTLWAV_WIDTH_BATS = 360;             // ctlWav with of info section for bat projects

    const string FILE_NAME_SETTINGS = "BatInspectorSettings.json";
    const string _dataPath = "dataPath.txt";
    public const int MAX_FILES_PRJ_OVERVIEW = 1000;
    public const string PythonBin = "\"C:/Program Files/Python310/python.exe\"";
    public const int MAX_LOG_COUNT = 600;     // max. numbe rof log entries before creating a new log file
    public const int STATISTIC_CLASSES = 40;  // number of classes in statistic histograms
    public const int FFT_WIDTH = 1024;
    public const int NR_OF_TICKS = 9;         // number of ticks in zoom view (if changed, add/remove line(s) in zoom view)
    public const int CNT_WAV_CONTROLS = 16;   // max. number of wav file controls


    static AppParams _inst = null;

    bool _isInitialized = false;
    ScriptInventory _scriptInventory = null;
    static public  bool IsInitialized { get { return Inst._isInitialized; } }
   
    public static string AppVersion 
    {
      get
      {
        System.Version version;
        try
        {
          version = ApplicationDeployment.CurrentDeployment.CurrentVersion;
        }
        catch
        {
          version = Assembly.GetExecutingAssembly().GetName().Version;
        }

        return version.ToString();
      }
    }
    
    public static AppParams Inst 
    { 
      get 
      {
        if (_inst == null)
        {
          AppParams.load();
        }
        return _inst;
      }
    }

    static public string DriveLetter 
    {
      get
      {
        return Path.GetPathRoot(System.Reflection.Assembly.GetExecutingAssembly().Location);
      }
    }

    [Browsable(false)]
    public string FileName
    {
      get { return Path.Combine(AppParamsPath, FILE_NAME_SETTINGS); }
    }

    [DataMember]
    [LocalizedCategory("SetCatApplication")]
    [LocalizedDescription("SetDescRootPath")]
    [Browsable(false)]
    public string AppRootPath { get; set; } = "";


    public static String AppParamsPath { get; set; } = "";
    public static string AppDataPath { get; set; } = "";

    public static string LogDataPath { get; set; } = "";

    [DataMember]
    [Browsable(false)]
    public string Version { get; set; } = "";


    [DataMember]
    [LocalizedCategory("SetCatApplication")]
    [LocalizedDescription("SetDescExeEditor")]
    public string ExeEditor { get; set; } = "";

    [DataMember]
    [LocalizedCategory("SetCatApplication")]
    [LocalizedDescription("SetDescWavTool")]
    public string WavTool { get; set; } = "";

    [DataMember]
    [LocalizedCategory("SetCatPrjExplorer")]
    [LocalizedDescription("SetDescShowOnlyFiltered")]
    public bool ShowOnlyFilteredDirs { get; set; } = false;

    [DataMember]
    [LocalizedCategory("SetCatPrjExplorer")]
    [LocalizedDescription("SetDescDirFilter")]
    public List<string> DirFilter { get; set; } = new List<string>();

    [DataMember]
    [LocalizedDescription("SetDescLanguage")]
    [LocalizedCategory("SetCatApplication")]
    public enCulture Culture { get; set; } = enCulture.de_DE;

  /*  [DataMember]
    [LocalizedCategory("SetCatApplication")]
    [LocalizedDescription("SpecDescWidthWf")]
    public uint WaterfallHeight { get; set; } = 256;  */

    [DataMember]
    [LocalizedCategory("SetCatApplication")]
    [LocalizedDescription("SetDescHeightWf")]
    public uint WaterfallWidth { get; set; } = 1024;

  /*  [DataMember]
    [LocalizedCategory("SetCatApplication")]
    [LocalizedDescription("SetDescWidthFFT")]
    public uint FftWidth { get; set; } = 256; */

    [DataMember]
    [LocalizedCategory("SetCatZoom")]
    [LocalizedDescription("SetDescColorOfLine")]
    [Browsable(false)]
    public Color ColorXtLine { get; set; } = Color.Black;

    [DataMember]
    [LocalizedCategory("SetCatZoom")]
    [LocalizedDescription("SetDescFrequencyHET")]
    public double FrequencyHET { get; set; } = 40000;

    [DataMember]
    [LocalizedCategory("SetCatZoom")]
    [LocalizedDescription("SpecDescShowZoom")]
    public bool ZoomSeparateWin { get; set; } = false;

    [LocalizedCategory("SetCatZoom")]
    [LocalizedDescription("SetDescZoomLogarithmic")]
    public bool ZoomSpectrumLogarithmic { get; set; } = false;

    [DataMember]
    [LocalizedCategory("SetCatZoom")]
    [LocalizedDescription("SetDescLengthZoomMs")]
    public double ZoomOneCall { get; set; } = 100.0;

    [DataMember]
    [LocalizedCategory("SetCatZoom")]
    [LocalizedDescription("SetDescGridColor")]
    public Color GridColor { get; set; } = Color.Yellow;

    [DataMember]
    [LocalizedCategory("SetCatMainWindow")]
    [LocalizedDescription("SetDescWidthMainWin")]
    [Browsable(false)]
    public double MainWindowWidth { get; set; } = 1400;

    [DataMember]
    [LocalizedCategory("SetCatMainWindow")]
    [LocalizedDescription("SetDescHeightMainWin")]
    [Browsable(false)]
    public double MainWindowHeight { get; set; } = 900;

    [DataMember]
    [LocalizedCategory("SetCatMainWindow")]
    [LocalizedDescription("SetDescHeightLogCtrl")]
    [Browsable(false)]
    public double LogControlHeight { get; set; } = 150;

    [DataMember]
    [LocalizedCategory("SetCatMainWindow")]
    [LocalizedDescription("SetDescWidthFileSel")]
    [Browsable(false)]
    public double WidthFileSelector { get; set; } = 200;

    [DataMember]
    [LocalizedCategory("SetCatZoom")]
    [Description("width of main window [px]")]
    [Browsable(false)]
    public double ZoomWindowWidth { get; set; } = 1200;

    [DataMember]
    [LocalizedCategory("SetCatZoom")]
    [LocalizedDescription("SetDescHeightMainWin")]
    [Browsable(false)]
    public double ZoomWindowHeight { get; set; } = 900;

    [DataMember]
    [LocalizedCategory("SetCatZoom")]
    [LocalizedDescription("SetDescZoomType")]
    public enZoomType ZoomType { get; set; } = enZoomType.LEFT;

    [DataMember]
    [LocalizedCategory("SetCatApplication")]
    [LocalizedDescription("SerDescHideCallInfo")]
    public bool HideInfos { get; set; } = false;

    [DataMember]
    [LocalizedCategory("SetCatZoom")]
    [LocalizedDescription("SetDescColorBackgXTDiag")]
    [Browsable (false)]
    public Color ColorXtBackground { get; set; } = Color.LightGray;

    [DataMember]
    [LocalizedCategory("SetCatMainWindow")]
    [LocalizedDescription("SetDescMainWinPosX")]
    [Browsable(false)]
    public double MainWindowPosX { get; set; } = 0;

    [DataMember]
    [LocalizedCategory("SetCatMainWindow")]
    [LocalizedDescription("SetDescMainWinPosY")]
    [Browsable(false)]
    public double MainWindowPosY { get; set; } = 0;

    [DataMember]
    [LocalizedCategory("SetCatApplication")]
    [LocalizedDescription("SetDescRangeZoomWin")]
    public double GradientRange { get; set; } = 90;

    [DataMember]
    [Category("Filter")]
    [LocalizedDescription("SetDescFilter")]
    [Browsable(false)]
    public List<FilterParams> Filter { get; set; }

    [DataMember]
    [LocalizedDescription("SetDescMySqlConnect")]
    public string MySqlConnectStringBats { get; set; }

    [DataMember]
    [LocalizedDescription("SetDescMySqlConnect")]
    public string MySqlUser { get; set; }

    [DataMember]
    [LocalizedDescription("SetDescMySqlConnect")]
    public string MySqlServer { get; set; }

    [DataMember]
    [LocalizedDescription("SetDescMySqlConnect")]
    public string MySqlDbBats { get; set; }

    [DataMember]
    [LocalizedDescription("SetDescMySqlConnect")]
    public string MySqlDbBirds { get; set; }

    [DataMember]
    [LocalizedCategory("SetCatApplication")]
    [LocalizedDescription("SetDescWfLog")]
    public bool WaterfallLogarithmic { get; set; }

    [DataMember]
    [LocalizedCategory("SetCatModel"),
    LocalizedDescription("SetDescSamplingRate")]
    [Browsable(false)]
    public int SamplingRate { get; set; }

    [DataMember]
    [LocalizedCategory("SetCatModel"),
    LocalizedDescription("SetDescScriptAutoToMan")]
    public string ScriptCopyAutoToMan { get; set; }

    [DataMember]
    [LocalizedCategory("SetCatModel")]
    [Browsable(true)]
    public string ModelRootPath { get; set; } = "";

    [DataMember]
    [LocalizedCategory("SetCatModel"),
    LocalizedDescription("SetDescMultiModel")]
    public bool AllowMoreThanOneModel { get; set; } = false;

    [DataMember]
    [LocalizedCategory("SetCatModel"),
    LocalizedDescription("SetDescDefaultModel")]
    public enModel DefaultModel { get; set; } = enModel.BAT_DETECT2;

/*    [DataMember]
    [LocalizedCategory("SetCatModel")]
    [Browsable(false)]
    public List<ModelItem> Models { get; set; } = new List<ModelItem> { };
    */
    [DataMember]
    [Browsable(false)]
    public enWIN_TYPE FftWindow { get; set; }

    [DataMember]
    [LocalizedCategory("SetCatColorGradient"),
     LocalizedDescription("SpecDescColorRed"),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
     [Browsable(false)]
    public List<ColorItem> ColorGradientRed { get; set; } = new List<ColorItem>();

    [DataMember]
    [LocalizedCategory("SetCatColorGradient"),
    LocalizedDescription("SpecDescColorGreen"),
    DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    [Browsable(false)]
    public List<ColorItem> ColorGradientGreen { get; set; } = new List<ColorItem>();

    [DataMember]
    [LocalizedCategory("SetCatColorGradient"),
    LocalizedDescription("SpecDescColorBlue"),
    DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    [Browsable(false)]
    public List<ColorItem> ColorGradientBlue { get; set; } = new List<ColorItem>();

    [DataMember]
    [LocalizedCategory("SetCatPrediction"),
    LocalizedDescription("SpecDescCutCalls")]
    [Browsable(false)]

    public bool LogShowError { get; set; }

    [DataMember]
    [LocalizedCategory("SetCatLog"),
     LocalizedDescription("SpecDescShowWarning")]
    [Browsable(false)]
    public bool LogShowWarning { get; set; }
    
    [DataMember]
    [LocalizedCategory("SetCatLog"),
     LocalizedDescription("SpecDescShowInfo")]
    [Browsable(false)]
    public bool LogShowInfo { get; set; }
    
    [DataMember]
    [LocalizedCategory("SetCatLog"),
     LocalizedDescription("SpecDescShowDebug")]
    [Browsable(false)]
    public bool LogShowDebug { get; set; }

    [DataMember]
    [LocalizedCategory("SetCatApplication")]
    [LocalizedDescription("SerDescScriptInventory")]
    public string ScriptInventoryPath { get; set; }

    [DataMember]
    [LocalizedCategory("SetCatApplication")]
    [LocalizedDescription("SerDescBatInfo")]
    public string BatInfoPath { get; set; }

    [DataMember]
    public LocFileSettings LocFileSettings { get; set; }

    [DataMember]
    [Browsable(false)]
    public string ModelDefaultParamsFile { get; set; }

    [Browsable(false)]
    public ScriptInventory ScriptInventory { get { return _scriptInventory; } }
    public AppParams()
    {
      init();
    }

    private void updateFromOlderVersions()
    {
      if (string.IsNullOrEmpty(Version) || (Version != AppVersion))
      {
        if (string.IsNullOrEmpty(ModelDefaultParamsFile))
          ModelDefaultParamsFile = Path.Combine(AppDataPath, "dat", "default_model_params.xml");
      }
      int comparison = String.Compare(Version, "0.9.2.3", comparisonType: StringComparison.OrdinalIgnoreCase);
      if (string.IsNullOrEmpty(Version) || (comparison < 0))
      {
        double grad = GradientRange;
        grad = Math.Exp(grad);
        grad = Math.Log10(grad) * 10 + 20;
        GradientRange = grad;
      }

      Version = AppVersion;
    }

    public void init()
    {
      Version = AppVersion;
      AppRootPath = AppDomain.CurrentDomain.BaseDirectory;
      LogDataPath = Path.Combine(AppDataPath, "log");
      ScriptInventoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments),
                                         PROG_DAT_DIR, AppParams.DIR_SCRIPT);

      ExeEditor = "\"C:\\Windows\\Notepad.exe\"";
      WaterfallWidth = 1024;
      FftWindow = enWIN_TYPE.HANN;
      ColorXtLine = Color.Black;
      ColorXtBackground = Color.LightGray;
      MainWindowWidth = 1400;
      LogControlHeight = 150;
      WidthFileSelector = 150;
      MainWindowHeight = 900;
      ZoomWindowWidth = 1200;
      ZoomWindowHeight = 900;
      ZoomOneCall = 100;
      FrequencyHET = 40000;
      MainWindowPosX = 0;
      MainWindowPosY = 0;
      GradientRange = 90;
      HideInfos = false;
      WaterfallLogarithmic = true;
      Culture = enCulture.de_DE;
      initFilterParams();
      initColorGradient();
      ModelRootPath = Path.Combine(AppDataPath, "models");
      ModelDefaultParamsFile = Path.Combine(AppDataPath, "dat", "default_model_params.xml");
      DefaultModel = enModel.BAT_DETECT2;
      MySqlServer = "127.0.0.1";
      MySqlUser = "root";
      MySqlDbBats = "bat_calls";
      MySqlDbBirds = "birds";
      //  initModels();
      SamplingRate = 312500;
      ScriptCopyAutoToMan = "copyAutoToMan.scr";
      LogShowError = true;
      LogShowWarning = true;
      LogShowInfo = true;
      LogShowDebug = false;

      ShowOnlyFilteredDirs = false;
      DirFilter = new List<string>();
      for (int i = 0; i < 5; i++)
        DirFilter.Add("");
    }

    private void initFilterParams()
    {
      Filter = new List<FilterParams>();
      FilterParams p = new FilterParams();
      p.Name = "Example";
      p.Expression = "(SpeciesMan == \"PPIP\")";
      p.isForAllCalls = true;
      p.Index = 0;
      Filter.Add(p);
    }

    public void saveAs(string fName)
    {
      try
      {
        if (DirFilter == null)
          DirFilter = new List<string>();
        while (DirFilter.Count < 5)
          DirFilter.Add("");
        using (StreamWriter file = new StreamWriter(fName))
        {
          using (MemoryStream stream = new MemoryStream())
          {
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(AppParams));
            ser.WriteObject(stream, this);
            StreamReader sr = new StreamReader(stream);
            stream.Seek(0, SeekOrigin.Begin);
            string str = sr.ReadToEnd();
            file.Write(JsonHelper.FormatJson(str));
            file.Close();
            DebugLog.log("settings saved to '" + fName + "'", enLogType.INFO);
          }
        }
      }
      catch (Exception e)
      {
        DebugLog.log("failed to write config file for BatInspector" + fName + ": " + e.ToString(), enLogType.ERROR);
      }
    }

    public void save()
    {
      saveAs(FileName);
    }


    public static void loadFrom(string fPath)
    {
      AppParams retVal = null;
      FileStream file = null;
      try
      {
        DebugLog.log("try to load:" + fPath, enLogType.DEBUG);
        if (File.Exists(fPath))
        {
          using (file = new FileStream(fPath, FileMode.Open, FileAccess.Read))
          {
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(AppParams));
            retVal = (AppParams)ser.ReadObject(file);
            if (retVal == null)
              DebugLog.log("settings file not well formed!", enLogType.ERROR);
            if (retVal.ColorGradientBlue == null)
              retVal.initColorGradient();
            //          if (retVal.Models == null)
            //            retVal.initModels();
            DebugLog.log("successfully loaded", enLogType.DEBUG);
            retVal.AppRootPath = AppDomain.CurrentDomain.BaseDirectory;
            retVal.updateFromOlderVersions();
          }
        }
        else
        {
          DebugLog.log("load failed", enLogType.DEBUG);
          retVal = new AppParams();
          retVal.init();
          retVal.save();
        }
      }
      catch (Exception e)
      {
        DebugLog.log("failed to read config file : " + fPath + ": " + e.ToString(), enLogType.ERROR);
        retVal = null;
      }
      finally
      {
        if (file != null)
          file.Close();
      }
      if (string.IsNullOrEmpty(retVal.ScriptInventoryPath))
      {
        retVal.ScriptInventoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments),
                                         PROG_DAT_DIR, AppParams.DIR_SCRIPT);
      }
      if (string.IsNullOrEmpty(retVal.BatInfoPath))
      {
        retVal.BatInfoPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments),
                                         PROG_DAT_DIR, AppParams.DIR_BAT_INFO);
      }

      retVal._scriptInventory = ScriptInventory.loadFrom(retVal.ScriptInventoryPath, out bool firstLoadAfterInstall);

      if (firstLoadAfterInstall)
      {
        string srcPath = Path.Combine(AppDataPath, "setup");
        string dstPath = AppParams.Inst.BatInfoPath;
        BatInfo.copyInfoFileAfterSetup(srcPath, dstPath);
      }
      retVal.AppRootPath = replaceDriveLetter(retVal.AppRootPath);
    //  retVal.ModelRootPath = replaceDriveLetter(retVal.ModelRootPath);
      //retVal.SpeciesFile = replaceDriveLetter(retVal.SpeciesFile);
      LogDataPath = replaceDriveLetter(LogDataPath);
      AppDataPath = replaceDriveLetter(AppDataPath);
      DebugLog.log("root paths adapted to drive " + AppParams.DriveLetter, enLogType.DEBUG);
      retVal.check();
      //    retVal.adjustActivateBat();
      retVal._isInitialized = true;
      _inst = retVal;
    }

    private void check()
    {
      if ((FftWindow < 0) || (FftWindow > enWIN_TYPE.NONE))
        FftWindow = enWIN_TYPE.HANN;
    }

    static string replaceDriveLetter(string  path)
    {
      string driveStr = "C";
      int driveIdx = 0;
      if (path[0] == '"')
      {
        driveStr = "\"C";
        driveIdx = 1;
      }
      if(path.IndexOf(driveStr) < 0)
        path = path.Replace(path.Substring(driveIdx,1), DriveLetter.Substring(0,1));
      return path;
    }

    public static void load()
    {
      DebugLog.log("searching " + _dataPath + " in directory " + AppDomain.CurrentDomain.BaseDirectory, enLogType.DEBUG);
      AppParamsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments),
                                   PROG_DAT_DIR);
      if (File.Exists(_dataPath))
      {
        DebugLog.log(_dataPath + " found", enLogType.DEBUG);
        string path = File.ReadAllText(_dataPath);
        DebugLog.log("content " + _dataPath + ": " + path, enLogType.DEBUG);
        if (path == VAR_DATA_PATH)
        {
          AppDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                                     PROG_DAT_DIR);
        }
        else
        {
          AppDataPath = replaceDriveLetter(path);
          AppParamsPath = AppDataPath;
        }
      }
      else
      {
        AppDataPath = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
        DebugLog.log("datapath.txt NOT found", enLogType.INFO);
      }
      DebugLog.log("resulting data path: " + AppDataPath, enLogType.INFO);
      LogDataPath = Path.Combine(AppDataPath, "log");
      string fPath = Path.Combine(AppParamsPath, FILE_NAME_SETTINGS);
      loadFrom(fPath);
    }


    public void initColorGradient()
    {
      ColorGradientBlue = new List<ColorItem>
      {
        new ColorItem(0, 0),
        new ColorItem(100, 20),
        new ColorItem(0, 40),
        new ColorItem(0, 75),
        new ColorItem(40, 100)
      };
      ColorGradientGreen = new List<ColorItem>
      {
        new ColorItem(0, 0),
        new ColorItem(0, 20),
        new ColorItem(200, 60),
        new ColorItem(200, 75),
        new ColorItem(0, 100)
      };
      ColorGradientRed = new List<ColorItem>
      {
        new ColorItem(0, 0),
        new ColorItem(0, 20),
        new ColorItem(100, 60),
        new ColorItem(200, 75),
        new ColorItem(255, 100)
      };

      /* black - green
        ColorGradientBlue = new List<ColorItem>
        {
          new ColorItem(0, 0),
          new ColorItem(0, 30),
          new ColorItem(0, 70),
          new ColorItem(0, 75),
          new ColorItem(40, 100)
        };
        ColorGradientGreen = new List<ColorItem>
        {
          new ColorItem(0, 0),
          new ColorItem(70, 30),
          new ColorItem(200, 70),
          new ColorItem(200, 75),
          new ColorItem(0, 100)
        };
        ColorGradientRed = new List<ColorItem>
        {
          new ColorItem(0, 0),
          new ColorItem(0, 30),
          new ColorItem(200, 70),
          new ColorItem(200, 75),
          new ColorItem(255, 100)
        }; 
      */
    }

    /*    public void adjustActivateBat()
        {
          string str = "VIRTUAL_ENV=";
          string path =  ModelRootPath + "/_venv/Scripts/activate.bat";
          try
          {
            string activateBat = File.ReadAllText(path);
            int pos = activateBat.IndexOf(str) + str.Length;
            if (activateBat[pos + 1] != DriveLetter[0])
            {
              string newBat = activateBat.Substring(0, pos) + DriveLetter[0] + activateBat.Substring(pos + 1);
              File.WriteAllText(path, newBat);
              DebugLog.log("python scripts adapted to drive " + DriveLetter, enLogType.INFO);
            }
          }
          catch { }
        }*/
  } 
}
