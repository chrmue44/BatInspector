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
  public class FilterParams
  {
    [DataMember]
    [Category("Filter")]
    [Description("name of the display filter")]
    public string Name { get; set; }

    [DataMember]
    [Category("Filter")]
    [Description("logical expression for filter")]
    public string Expression { get; set; }

    [DataMember]
    [Category("Filter")]
    [Description("only valid, whall ALL calls in one file apply to the logical expression")]
    public bool isForAllCalls { get; set; }

    public int Index { get; set; }
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
    [Category("Application")]
    [Description("settings for display filter")]
    public List<FilterParams> Filter { get; set; }

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
  }
}
