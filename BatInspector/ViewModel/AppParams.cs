using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BatInspector
{
  [DataContract]
  public class SpeciesInfos
  {
    public SpeciesInfos(string abbr, string latin, string local, bool show)
    {
      Abbreviation = abbr;
      Latin = latin;
      Local = local;
      Show = show;
    }

    [DataMember]
    [Description("4 letter abbreviation for species name")]
    public string Abbreviation { get; set; }

    [DataMember]
    [Description("latin species name")]
    public string Latin { get; set; }

    [DataMember]
    [Description("local species name")]
    public string Local { get; set; }

    [DataMember]
    [Description("show species in comboboxes to select species")]
    public bool Show { get; set; }

  }
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

  [DataContract]
  public class ColorItem
  {
    public ColorItem(int col, int val)
    {
      Color = col;
      Value = val;
    }
    [DataMember]
    public int Color;
    [DataMember]
    public int Value;
  }

  [DataContract]
  public class AppParams
  {
    const string _fName = "BatInspectorSettings.json";

    [DataMember]
    [Category("Application")]
    [Description("width of waterfall diagram in px")]
    public int WaterfallHeight { get; set; } = 256;
    [DataMember]
    [Category("Application")]
    [Description("height of waterfall diagram in px")]
    public int WaterfallWidth { get; set; } = 512;
    [DataMember]
    [Category("Application")]
    [Description("width of FFT in points (MUST be a power of 2)")]
    public uint FftWidth { get; set; } = 512;
    [DataMember]
    [Category("Application")]
    [Description("Color of line in XT diagram")]
    public Color ColorXtLine { get; set; } = Color.Black;

    [DataMember]
    [Category("Application")]
    [Description("Color of Background in XT diagram")]
    public Color ColorXtBackground { get; set; } = Color.LightGray;

    [DataMember]
    [Category("Filter")]
    [Description("settings for display filter")]
    public List<FilterParams> Filter { get; set; }

    [DataMember]
    [Category("Bat Species")]
    [Description("settings for display filter")]
    public List<SpeciesInfos> Species { get; set; }

    [DataMember]
    [Category("ColorGradient")]
    [Description("definition of color gradient for color channel red")]
    public List<ColorItem> ColorGradientRed { get; set; }

    [DataMember]
    [Category("ColorGradient")]
    [Description("definition of color gradient for color channel green")]
    public List<ColorItem> ColorGradientGreen { get; set; }
    [DataMember]
    [Category("ColorGradient")]
    [Description("definition of color gradient for color channel green")]
    public List<ColorItem> ColorGradientBlue { get; set; }
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
      initFilterParams();
      initColorGradient();
      initSpeciesInfos();
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
      Species.Add(new SpeciesInfos("BBAR", "Barbastella barbastellus", "Mopsfledermaus", true));
      Species.Add(new SpeciesInfos("ESER", "Eptesicus Serotinus", "Breitflügelfledermaus", true));
      Species.Add(new SpeciesInfos("MBRA", "Myotis brandtii", "Große Bartfledermaus", true));
      Species.Add(new SpeciesInfos("MBEC", "Myotis bechsteinii", "Bechsteinfledermaus", true));
      Species.Add(new SpeciesInfos("MMYS", "Myotis mystacinus", "Kleine Bartfledermaus", true));
      Species.Add(new SpeciesInfos("MNAT", "Myotis nattereri", "Fransenfledermaus", true));
      Species.Add(new SpeciesInfos("MMYO", "Myotis myotis", "Großes Mausohr", true));
      Species.Add(new SpeciesInfos("MOXY", "Myotis oxygnatus", "Kleines Mausohr", true));
      Species.Add(new SpeciesInfos("MDAU", "Myotis daubentinii", "Wasserfledermaus", true));
      Species.Add(new SpeciesInfos("NNOC", "Nyctalus noctula", "Großer Abendsegler", true));
      Species.Add(new SpeciesInfos("NLEI", "Nyctalus leisleri", "Kleiner Abendsegler", true));
      Species.Add(new SpeciesInfos("PPIP", "Pipistrellus pipistrellus", "Zwergfledermaus", true));
      Species.Add(new SpeciesInfos("PPYP", "Pipistrellus pygmaeus", "Mückenfledermaus", true));
      Species.Add(new SpeciesInfos("PNAT", "Pipistrellus nathusii", "Rauhautfledermaus", true));


    }

    public void initColorGradient()
    {
      ColorGradientBlue = new List<ColorItem>();
      ColorGradientBlue.Add(new ColorItem(128, 0));
      ColorGradientBlue.Add(new ColorItem(255, 40));
      ColorGradientBlue.Add(new ColorItem(0, 70));
      ColorGradientBlue.Add(new ColorItem(0, 100));
      ColorGradientBlue.Add(new ColorItem(0, 100));
      ColorGradientGreen = new List<ColorItem>();
      ColorGradientGreen.Add(new ColorItem(0, 0));
      ColorGradientGreen.Add(new ColorItem(200, 40));
      ColorGradientGreen.Add(new ColorItem(255, 70));
      ColorGradientGreen.Add(new ColorItem(128, 75));
      ColorGradientGreen.Add(new ColorItem(0, 100));
      ColorGradientRed = new List<ColorItem>();
      ColorGradientRed.Add(new ColorItem(0, 0));
      ColorGradientRed.Add(new ColorItem(0, 40));
      ColorGradientRed.Add(new ColorItem(255, 70));
      ColorGradientRed.Add(new ColorItem(255, 75));
      ColorGradientRed.Add(new ColorItem(255, 100));
    }
  }
}
