/********************************************************************************
 *               Author: Christian Müller
 *      Date of cration: 2021-08-10                                       
 *   Copyright (C) 2022: christian Müller christian(at)chrmue(dot).de
 *
 *              Licence:
 * 
 * THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND,
 * EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
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
using System.Resources;

namespace BatInspector
{

  public enum enCulture
  {
    de_DE,
    en_US,
  }

  public enum enModel
  {
    rnn6aModel,
    resNet34Model,
    BAT_DETECT2
  };

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

  [DataContract]
  public class ScriptItem
  {
    public ScriptItem(int index, string name, string description, bool isTool)
    {
      Index = index;
      Name = name;
      Description = description;
      IsTool = isTool;
    }

    [DataMember]
    public int Index { get; set; }

    [DataMember]
    public string Name { get; set; }

    [DataMember]
    public string Description { get; set; }

    [DataMember]
    public bool IsTool { get; set; }
  }

  [DataContract]
  public class ModelItem
  {
    [DataMember,
    Description("select type of model for prediction")]
    public enModel ModelType { get; set; }

    [DataMember]
    public bool Active { get; set; }

    [DataMember,
    LocalizedDescription("SetDescModDir")]
    public string Dir { get; set; }

    [DataMember,
    LocalizedDescription("SetDescLearningRate")]
    public double LearningRate { get; set; }

    [DataMember,
    LocalizedDescription("SetDescEpochs")]
    public int Epochs { get; set; }

    [DataMember,
    LocalizedDescription("SetDescScriptPredict")]
    public string Script { get; set; }

    [DataMember,
    LocalizedDescription("SetDescReportColumn")]
    public string ReportColumn { get; set; }

    [DataMember]
    public string ReportName { get; set; } 


  }


  [TypeConverter(typeof(ExpandableObjectConverter))]
  [DataContract]
  public class AppParams
  {
    public const string SUM_REPORT = "sum_report.csv";    // report name for sumarized report
    public const string PRJ_REPORT = "report.csv";        // report name for project report
    public const string PRJ_SUMMARY = "summary.csv";      // report name for project summary
    public const string ANNOTATION_SUBDIR = "ann";        // subdirectory for annotations for specific models
    public const string EXT_WAV = ".wav";                 // file extension for wav files 
    public const string EXT_IMG = ".png";                 // file extension for image files of recordings
    public const string EXT_INFO = ".xml";                // file extension for info files in Elekon projects
    public const string EXT_PRJ = ".bpr";                 // file extension for Elekon project file
    public const string CSV_SEPARATOR = ";";              // CSV separator
    public const string PATH_SCRIPT = "scripts";          // sub directory containig scripts
    public const string BAT_INFO1_PDF = "/doc/Bestimmung-Fledermausrufe-Teil1.pdf"; // Information file about bat detection
    public const string BAT_INFO2_PDF = "/doc/Bestimmung-Fledermausrufe-Teil2.pdf"; // Information file about bat detection
    public const string HELP_FILE = "/doc/BatInspector.pdf"; // software manual
    const string _fName = "BatInspectorSettings.json";

    static AppParams _inst = null;
    public static AppParams Inst 
    { 
      get 
      {
        if (_inst == null)
          AppParams.load();
        return _inst;
      }
    }

    [DataMember]
    [LocalizedCategory("SetCatApplication")]
    [LocalizedDescription("SetDescRootPath")]
    public string AppRootPath { get; set; } = "";

    [DataMember]
    [LocalizedCategory("SetCatApplication")]
    [LocalizedDescription("SetDescWidthFFT")]
    public string ScriptDir { get; set; } = "";

    [DataMember]
    [LocalizedCategory("SetCatApplication")]
    [LocalizedDescription("SetDescWidthFFT")]
    public string ExeEditor { get; set; } = "";

    [DataMember]
    [LocalizedDescription("SetDescLanguage")]
    [LocalizedCategory("SetCatApplication")]
    public enCulture Culture { get; set; } = enCulture.de_DE;

    [DataMember]
    [LocalizedCategory("SetCatApplication")]
    [LocalizedDescription("SpecDescWidthWf")]
    public int WaterfallHeight { get; set; } = 256;
    [DataMember]
    [LocalizedCategory("SetCatApplication")]
    [LocalizedDescription("SetDescHeightWf")]
    public int WaterfallWidth { get; set; } = 512;
    [DataMember]
    [LocalizedCategory("SetCatApplication")]
    [LocalizedDescription("SetDescWidthFFT")]
    public uint FftWidth { get; set; } = 512;
    [DataMember]
    [LocalizedCategory("SetCatApplication")]
    [LocalizedDescription("SetDescColorOfLine")]
    public Color ColorXtLine { get; set; } = Color.Black;

    [DataMember]
    [LocalizedCategory("SetCatApplication")]
    [LocalizedDescription("SpecDescShowZoom")]
    public bool ZoomSeparateWin { get; set; } = false;

    [LocalizedCategory("SetCatApplication")]
    [LocalizedDescription("SetDescZoomLogarithmic")]
    public bool ZoomSpectrumLogarithmic { get; set; } = false;

    [DataMember]
    [LocalizedCategory("SetCatApplication")]
    [LocalizedDescription("SetDescLengthZoomMs")]
    public double ZoomOneCall { get; set; } = 100.0;

    [DataMember]
    [LocalizedCategory("SetCatApplication")]
    [LocalizedDescription("SetDescWidthMainWin")]
    public double MainWindowWidth { get; set; } = 1400;

    [DataMember]
    [LocalizedCategory("SetCatApplication")]
    [LocalizedDescription("SetDescHeightMainWin")]
    public double MainWindowHeight { get; set; } = 900;

    [DataMember]
    [LocalizedCategory("SetCatApplication")]
    [LocalizedDescription("SetDescHeightLogCtrl")]
    public double LogControlHeight { get; set; } = 150;

    [DataMember]
    [LocalizedCategory("SetCatApplication")]
    [LocalizedDescription("SetDescWidthFileSel")]
    public double WidthFileSelector { get; set; } = 200;

    [DataMember]
    [LocalizedCategory("SetCatApplication")]
    [Description("width of main window [px]")]
    public double ZoomWindowWidth { get; set; } = 1200;

    [DataMember]
    [LocalizedCategory("SetCatApplication")]
    [LocalizedDescription("SetDescHeightMainWin")]
    public double ZoomWindowHeight { get; set; } = 900;

    [DataMember]
    [LocalizedCategory("SetCatApplication")]
    [LocalizedDescription("SerDescHideCallInfo")]
    public bool HideInfos { get; set; } = false;

    [DataMember]
    [LocalizedCategory("SetCatApplication")]
    [LocalizedDescription("SetDescColorBackgXTDiag")]
    public Color ColorXtBackground { get; set; } = Color.LightGray;

    [DataMember]
    [LocalizedCategory("SetCatApplication")]
    [LocalizedDescription("SetDescMainWinPosX")]
    public double MainWindowPosX { get; set; } = 0;

    [DataMember]
    [LocalizedCategory("SetCatApplication")]
    [LocalizedDescription("SetDescMainWinPosY")]
    public double MainWindowPosY { get; set; } = 0;

    [DataMember]
    [LocalizedCategory("SetCatApplication")]
    [LocalizedDescription("SetDescRangeZoomWin")]
    public double GradientRange { get; set; } = 15;

    [DataMember]
    [LocalizedCategory("SetCatApplication")]
    [LocalizedDescription("SetDescProb")]
    public double ProbabilityMin { get; set; } = 0.5;

    [DataMember]
    [Category("Filter")]
    [LocalizedDescription("SetDescFilter")]
    public List<FilterParams> Filter { get; set; }


    [DataMember]
    [LocalizedCategory("SetCatApplication")]
    [LocalizedDescription("SetDescWfLog")]
    public bool WaterfallLogarithmic { get; set; }


    [DataMember]
    [LocalizedCategory("SetCatApplication")]
    [Description("root directory for bat data")]
    public string RootDataDir { get; set; }


    [DataMember]
    [LocalizedCategory("SetCatScripting"),
    LocalizedDescription("SetDescSpeciesFile")]
    public string SpeciesFile { get; set; }

    [DataMember]
    [LocalizedCategory("SetCatScripting"),
    LocalizedDescription("SetDescPythonExe")]
    public string PythonBin { get; set; }

    [DataMember]
    [LocalizedCategory("SetCatScripting"),
    LocalizedDescription("SetDescSamplingRate")]
    public int SamplingRate { get; set; }

    [DataMember]
    [LocalizedCategory("SetCatScripting"),
    DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public List<ScriptItem> Scripts { get; set; } = new List<ScriptItem>();

    [DataMember]
    [LocalizedCategory("SetCatModel")]
    public string ModelRootPath { get; set; } = "";

    [DataMember]
    [LocalizedCategory("SetCatModel")]
    public List<ModelItem> Models { get; set; } = new List<ModelItem> { };

    [DataMember]
    public DSPLib.DSP.Window.Type FftWindow { get; set; }

    [DataMember]
    [LocalizedCategory("SetCatColorGradient"),
     LocalizedDescription("SpecDescColorRed"),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public List<ColorItem> ColorGradientRed { get; set; } = new List<ColorItem>();

    [DataMember]
    [LocalizedCategory("SetCatColorGradient"),
    LocalizedDescription("SpecDescColorGreen"),
    DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public List<ColorItem> ColorGradientGreen { get; set; } = new List<ColorItem>();

    [DataMember]
    [LocalizedCategory("SetCatColorGradient"),
    LocalizedDescription("SpecDescColorBlue"),
    DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public List<ColorItem> ColorGradientBlue { get; set; } = new List<ColorItem>();

    [DataMember]
    [LocalizedCategory("SetCatPrediction"),
    LocalizedDescription("SpecDescIdentify")]
    public bool PredIdentifyCalls { get; set; }

    [DataMember]
    [LocalizedCategory("SetCatPrediction"),
    LocalizedDescription("SpecDescCutCalls")]
    public bool PredCutCalls { get; set; }

    [DataMember]
    [LocalizedCategory("SetCatPrediction"),
    LocalizedDescription("SpecDescPrepData")]
    public bool PredPrepData { get; set; }

    [DataMember]
    [LocalizedCategory("SetCatPrediction"),
    LocalizedDescription("SpecDescPredict")]
    public bool PredPredict1 { get; set; }

    [DataMember]
    [LocalizedCategory("SetCatPrediction"),
    LocalizedDescription("SpecDescPredict")]
    public bool PredPredict2 { get; set; }

    [DataMember]
    [LocalizedCategory("SetCatPrediction"),
     LocalizedDescription("SpecDescPredict")]
    public bool PredPredict3 { get; set; }

    [DataMember]
    [LocalizedCategory("SetCatPrediction"),
     LocalizedDescription("SpecDescConfTest")]
    public bool PredConfTest { get; set; }

    [DataMember]
    [LocalizedCategory("SetCatPrediction"),
     LocalizedDescription("SpecDescDelTemp")]
    public bool PredDelTemp { get; set; }

    public AppParams()
    {
      init();
    }

    public void init()
    {
      AppRootPath = AppDomain.CurrentDomain.BaseDirectory;
      ScriptDir = "scripts";
      ExeEditor = "\"C:\\Program Files (x86)\\Notepad++\\notepad++.exe\"";
      WaterfallHeight = 256;
      WaterfallWidth = 512;
      FftWidth = 512;
      ColorXtLine = Color.Black;
      ColorXtBackground = Color.LightGray;
      MainWindowWidth = 1400;
      LogControlHeight = 150;
      WidthFileSelector = 150;
      MainWindowHeight = 900;
      ZoomWindowWidth = 1200;
      ZoomWindowHeight = 900;
      ZoomOneCall = 100;
      MainWindowPosX = 0;
      MainWindowPosY = 0;
      GradientRange = 15;
      ProbabilityMin = 0.5;
      HideInfos = false;
      WaterfallLogarithmic = true;
      Culture = enCulture.de_DE;
      initFilterParams();
      initColorGradient();
      initScripts();
      RootDataDir = "C:/users/chrmu/bat";
      SpeciesFile = "C:/Users/chrmu/bat/tierSta/species.csv";
      PythonBin = "\"C:/Program Files/Python310/python.exe\"";
      ModelRootPath = "C:/users/chrmu/prj/BatInspector/BatInspector/model";
      initModels();
      SamplingRate = 312500;

      PredIdentifyCalls = true;
      PredCutCalls = true;
      PredPrepData = true;
      PredPredict1 = true;
      PredPredict2 = false;
      PredPredict3 = false;
      PredConfTest = false;
      PredDelTemp = true;
    }

    private void initFilterParams()
    {
      Filter = new List<FilterParams>();
      FilterParams p = new FilterParams();
      p.Name = "Example";
      p.Expression = "(Species == \"PPIP\")";
      p.isForAllCalls = true;
      p.Index = 0;
      Filter.Add(p);
    }

    private void initScripts()
    {
      string strExeFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
      string strWorkPath = System.IO.Path.GetDirectoryName(strExeFilePath);
      strWorkPath += "/" + PATH_SCRIPT + "/";
      Scripts = new List<ScriptItem>();
      Scripts.Add(new ScriptItem(0, strWorkPath + "auto_to_man.scr",
                  "take over all unambiguously automatically recognized species", false));
      Scripts.Add(new ScriptItem(1, strWorkPath + "reset_man.scr",
                  "reset all manual species to 'todo'", false));
      Scripts.Add(new ScriptItem(2, strWorkPath + "junk.scr",
                  "select all recordings that seem to contain only junk", false));
    }

    private void initModels()
    {
      Models = new List<ModelItem>();
      Models.Add(new ModelItem());
      Models[0].Active = true;
      Models[0].Script = "bd2/run.bat";
      Models[0].Dir = "bd2";
      Models[0].Epochs = 0;
      Models[0].LearningRate = 0;
      Models[0].ReportColumn = "Species";
      Models[0].ReportName = "report.csv";
      Models.Add(new ModelItem());
      Models[1].Active = false;
      Models[1].Script = "py/run.bat";
      Models[1].Dir = "tsa";
      Models[1].Epochs = 30;
      Models[1].LearningRate = 0.00002;
      Models[1].ReportColumn = "Species";
      Models[1].ReportName = "report2.csv";

    }

    public void saveAs(string fName)
    {
      try
      {
        StreamWriter file = new StreamWriter(fName);
        MemoryStream stream = new MemoryStream();
        DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(AppParams));
        ser.WriteObject(stream, this);
        StreamReader sr = new StreamReader(stream);
        stream.Seek(0, SeekOrigin.Begin);
        string str = sr.ReadToEnd();
        file.Write(JsonHelper.FormatJson(str));
        file.Close();
      }
      catch (Exception e)
      {
        DebugLog.log("failed to write config file for BatInspector" + fName + ": " + e.ToString(), enLogType.ERROR);
      }
    }

    public void save()
    {
      string fPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\" + _fName;
      saveAs(fPath);
    }

    public static void loadFrom(string fPath)
    {
      AppParams retVal = null;
      FileStream file = null;
      try
      {
        if (File.Exists(fPath))
        {
          file = new FileStream(fPath, FileMode.Open);
          DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(AppParams));
          retVal = (AppParams)ser.ReadObject(file);
          if (retVal == null)
            DebugLog.log("settings file not well formed!", enLogType.ERROR);
          if (retVal.ColorGradientBlue == null)
            retVal.initColorGradient();
          if (retVal.Scripts == null)
            retVal.initScripts();
          if (retVal.Models == null)
            retVal.initModels();
        }
        else
        {
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
      _inst = retVal;

    }

    public static void load()
    {
      string fPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\" + _fName;
      loadFrom(fPath);
    }


    public void initColorGradient()
    {
      ColorGradientBlue = new List<ColorItem>();
      ColorGradientBlue.Add(new ColorItem(128, 0));
      ColorGradientBlue.Add(new ColorItem(255, 30));
      ColorGradientBlue.Add(new ColorItem(0, 70));
      ColorGradientBlue.Add(new ColorItem(0, 100));
      ColorGradientBlue.Add(new ColorItem(40, 100));
      ColorGradientGreen = new List<ColorItem>();
      ColorGradientGreen.Add(new ColorItem(0, 0));
      ColorGradientGreen.Add(new ColorItem(200, 30));
      ColorGradientGreen.Add(new ColorItem(255, 70));
      ColorGradientGreen.Add(new ColorItem(200, 75));
      ColorGradientGreen.Add(new ColorItem(0, 100));
      ColorGradientRed = new List<ColorItem>();
      ColorGradientRed.Add(new ColorItem(0, 0));
      ColorGradientRed.Add(new ColorItem(0, 30));
      ColorGradientRed.Add(new ColorItem(255, 70));
      ColorGradientRed.Add(new ColorItem(255, 75));
      ColorGradientRed.Add(new ColorItem(255, 100));
    }
  }
}
