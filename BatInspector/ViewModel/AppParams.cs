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
    resNet34Model
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

  [TypeConverter(typeof(SpeciesInfoConfigurationTypeConverter))]
  [DataContract]
  public class SpeciesInfos
  {
    public SpeciesInfos(string abbr, string latin, string local, bool show, double fcMin, double fcMax, double dMin, double dMax)
    {
      Abbreviation = abbr;
      Latin = latin;
      Local = local;
      Show = show;
      FreqCharMin = fcMin;
      FreqCharMax = fcMax;
      DurationMin = dMin;
      DurationMax = dMax;
      ProofSpecies = "TODO";
      CharCalls = "TODO";
    }

    public static SpeciesInfos find(string abbreviation, List<SpeciesInfos> list)
    {
      SpeciesInfos retVal = null;
      foreach(SpeciesInfos s in list)
      {
        if(abbreviation.ToLower() == s.Abbreviation.ToLower())
        {
          retVal = s;
          break;
        }
      }
      return retVal;
    }

    [DataMember]
    [LocalizedDescription("SpecDescAbbr")]
    public string Abbreviation { get; set; }

    [DataMember]
    [LocalizedDescription("SpecDescLatinSpec")]
    public string Latin { get; set; }

    [DataMember]
    [Description("local species name")]
    public string Local { get; set; }

    [DataMember]
    [LocalizedDescription("SpecDescShowSpec")]
    public bool Show { get; set; }

    [DataMember]
    [Description("minimal characteristic frequency [kHz]")]
    public double FreqCharMin { get; set; }

    [DataMember]
    [Description("maximal characteristic frequency [kHz]")]
    public double FreqCharMax { get; set; }

    [DataMember]
    [LocalizedDescription("SpecDescMinCall")]
    public double DurationMin { get; set; }

    [DataMember]
    [LocalizedDescription("SpecDescMaxCall")]
    public double DurationMax { get; set; }

    [DataMember]
    [LocalizedDescription("SpecDescMinCallDist")]
    public double CallDistMin { get; set; }

    [DataMember]
    [LocalizedDescription("SpecDescMaxCallDist")]
    public double CallDistMax { get; set; }

    [DataMember]
    [Description("proof of species")]
    public string ProofSpecies { get; set; }

    [DataMember]
    [Description("Characteristic calls")]
    public string CharCalls { get; set; }

    [DataMember]
    [Description("Habitat")]
    public string Habitat { get; set; }

    [DataMember]
    [LocalizedDescription("SpecDescWav")]
    public string WavExample { get; set; }
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
    const string _fName = "BatInspectorSettings.json";

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
    [LocalizedCategory("SetCatBatSpecies"),
     LocalizedDescription("SetDescBatSpecies"),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public List<SpeciesInfos> Species { get; set; }

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
    LocalizedDescription("SpecDescRexe")]
    public string Rbin { get; set; }

    [DataMember]
    [LocalizedCategory("SetCatScripting"),
    LocalizedDescription("SetDescScriptProp")]
    public string RScript { get; set; }

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
    LocalizedDescription("SetDescScriptPredict")]
    public string PythonScript { get; set; }

    [DataMember]
    [LocalizedCategory("SetCatScripting"),
    LocalizedDescription("SetDescModDir")]
    public string ModelDir { get; set; }

    [DataMember]
    [LocalizedCategory("SetCatScripting"),
    LocalizedDescription("SetDescSamplingRate")]
    public int SamplingRate { get; set; }

    [DataMember]
    [LocalizedCategory("SetCatScripting"),
    Description("select type of model for prediction")]
    public enModel ModelType { get; set; }

    [DataMember]
    [LocalizedCategory("SetCatApplication")]
//    [LocalizedDescription("SetDescWfLog")]
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



    public AppParams()
    {
      init();
    }

    public void init()
    {
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
      initSpeciesInfos();
      RootDataDir = "C:/users/chrmu/bat";
      Rbin = "\"C:/Program Files/R/R-4.2.0/bin/Rscript.exe\"";
      RScript = "C:/Users/chrmu/prj/BatInspector/R/features.R";
      SpeciesFile = "C:/Users/chrmu/bat/tierSta/species.csv";
      PythonBin = "\"C:/Program Files/Python310/python.exe\"";
      PythonScript = "C:/Users/chrmu/prj/BatInspector/py/batclass.py";
      ModelDir = "C:/Users/chrmu/prj/BatInspector/mod_tsa";

      SamplingRate = 312500;
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

    public void save()
    {
      string fPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\" + _fName;
      try
      {
        StreamWriter file = new StreamWriter(fPath);
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
        DebugLog.log("failed to write config file for BatInspector" + fPath + ": " + e.ToString(), enLogType.ERROR);
      }
    }

    public static AppParams load()
    {
      AppParams retVal = null;
      FileStream file = null;
      string fPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\" + _fName;
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

      return retVal;
    }

    private void initSpeciesInfos()
    {
      Species = new List<SpeciesInfos>();
      Species.Add(new SpeciesInfos("BBAR", "Barbastella barbastellus", "Mopsfledermaus", true, 31,42,2,5));
      Species.Add(new SpeciesInfos("ESER", "Eptesicus Serotinus", "Breitflügelfledermaus", true,21,25,10,18));
      Species.Add(new SpeciesInfos("MBRA", "Myotis brandtii", "Große Bartfledermaus", true, 37,50,4,7));
      Species.Add(new SpeciesInfos("MBEC", "Myotis bechsteinii", "Bechsteinfledermaus", true, 38,50,2.5,6));
      Species.Add(new SpeciesInfos("MMYS", "Myotis mystacinus", "Kleine Bartfledermaus", true, 40,57,3,6));
      Species.Add(new SpeciesInfos("MNAT", "Myotis nattereri", "Fransenfledermaus", true,28,53,2,5));
      Species.Add(new SpeciesInfos("MMYO", "Myotis myotis", "Großes Mausohr", true, 27,37,5,10));
      Species.Add(new SpeciesInfos("MOXY", "Myotis oxygnatus", "Kleines Mausohr", true,28,40,5,10));
      Species.Add(new SpeciesInfos("MDAU", "Myotis daubentinii", "Wasserfledermaus", true,37,55,3,7));
      Species.Add(new SpeciesInfos("NNOC", "Nyctalus noctula", "Großer Abendsegler", true, 17, 29, 5, 28));
      Species.Add(new SpeciesInfos("NLEI", "Nyctalus leisleri", "Kleiner Abendsegler", true, 21, 30, 5, 20));
      Species.Add(new SpeciesInfos("PPIP", "Pipistrellus pipistrellus", "Zwergfledermaus", true, 41, 52, 3, 10));
      Species.Add(new SpeciesInfos("PPYG", "Pipistrellus pygmaeus", "Mückenfledermaus", true, 50, 64, 3, 10 ));
      Species.Add(new SpeciesInfos("PNAT", "Pipistrellus nathusii", "Rauhautfledermaus", true, 42, 49, 4, 11));


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
