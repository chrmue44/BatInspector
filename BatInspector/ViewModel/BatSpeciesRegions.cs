using libParser;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace BatInspector
{
  [TypeConverter(typeof(ColorItemConfigurationTypeConverter))]
  [DataContract]
  public class ParLocation
  {
    [DataMember]
    public double Lat { get; set; }

    [DataMember]
    public double Lon { get; set; }

    public ParLocation(double lat, double lon)
    {
      Lat = lat;
      Lon = lon;
    }
  }

  [TypeConverter(typeof(ColorItemConfigurationTypeConverter))]
  [DataContract]
  public class ParRegion
  {
    [DataMember]
    string Name { get;set; }

    [DataMember]
    string InformationSource { get; set; }

    [DataMember]
    List<ParLocation> Location { get; set; }

    [DataMember]
    List<string> Species { get; set; }

    public ParRegion()
    {
      Location = new List<ParLocation>();
      Species = new List<string>();
      InformationSource = "";
      Name = "";
    }
  }


  [TypeConverter(typeof(ExpandableObjectConverter))]
  [DataContract]
  public class BatSpeciesRegions
  {
    const string _fName = "BatSpeciesRegions.json";
    [DataMember]
    List<ParRegion> Regions { get; set; }

    public BatSpeciesRegions()
    {
      init();
    }

    public static BatSpeciesRegions load()
    {
      BatSpeciesRegions retVal = null;
      FileStream file = null;
      string fPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\" + _fName;
      try
      {
        if (File.Exists(fPath))
        {
          file = new FileStream(fPath, FileMode.Open);
          DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(BatSpeciesRegions));
          retVal = (BatSpeciesRegions)ser.ReadObject(file);
          if (retVal == null)
            DebugLog.log("regions file not well formed!", enLogType.ERROR);
        }
        else
        {
          retVal = new BatSpeciesRegions();
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

    public void init()
    {
      Regions = new List<ParRegion>();
    }

    public void save()
    {
      string fPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\" + _fName;
      try
      {
        StreamWriter file = new StreamWriter(fPath);
        MemoryStream stream = new MemoryStream();
        DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(BatSpeciesRegions));
        ser.WriteObject(stream, this);
        StreamReader sr = new StreamReader(stream);
        stream.Seek(0, SeekOrigin.Begin);
        string str = sr.ReadToEnd();
        file.Write(JsonHelper.FormatJson(str));
        file.Close();
      }
      catch (Exception e)
      {
        DebugLog.log("failed to write regions file for BatInspector" + fPath + ": " + e.ToString(), enLogType.ERROR);
      }
    }

  }
}
