/********************************************************************************
 *               Author: Christian Müller
 *     Date of creation: 2023-09-01                                       
 *   Copyright (C) 2023: Christian Müller chrmue44(at)gmail(dot).com
 *
 *              Licence:  CC BY-NC 4.0 
 ********************************************************************************/

using libParser;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

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
    public string Name { get;set; }

    [DataMember]
    public string InformationSource { get; set; }

    [DataMember]
    public List<ParLocation> Location { get; set; }

    [DataMember]
    public List<string> Species { get; set; }

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
    const string _fName = "dat\\BatSpeciesRegions.json";
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
      string fPath = Path.Combine(AppParams.AppDataPath,  _fName);
      try
      {
        DebugLog.log("try loading BatSpeciesRegions: " + fPath, enLogType.DEBUG);
        if (File.Exists(fPath))
        {
          file = new FileStream(fPath, FileMode.Open, FileAccess.Read);
          DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(BatSpeciesRegions));
          retVal = (BatSpeciesRegions)ser.ReadObject(file);
          if (retVal == null)
            DebugLog.log("regions file not well formed!", enLogType.ERROR);
        }
        else
        {
          DebugLog.log("BatSpeciesRegions does not exist, create new file: " + fPath, enLogType.DEBUG);
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
      string fPath = Path.Combine(AppParams.AppDataPath, _fName); try
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

    public ParRegion findRegion(double lat, double lon)
    {
      ParRegion retVal = null;
      foreach(ParRegion  r in Regions)
      {
        ParLocation l = new ParLocation(lat, lon);
        if (inside(l, r.Location))
        {
          retVal = r; 
        }
      }
      return retVal;
    }

    // Is p0 inside p?  Polygon 
    public static bool inside(ParLocation p0, List<ParLocation> p)
    {
      int n = p.Count;
      bool result = false;
      for (int i = 0; i < n; ++i)
      {
        int j = (i + 1) % n;
        if (
          // Does p0.y lies in half open y range of edge.
          // N.B., horizontal edges never contribute
          ((p[j].Lat <= p0.Lat && p0.Lat < p[i].Lat) ||
         (p[i].Lat <= p0.Lat && p0.Lat < p[j].Lat)) &&
       // is p to the left of edge?
       (p0.Lon < p[j].Lon + (p[i].Lon - p[j].Lon) * (p0.Lat - p[j].Lat) /
         (p[i].Lat - p[j].Lat))
       )
          result = !result;
      }
      return result;
    }

    /// <summary>
    /// calculates the distance of two points on the earth
    /// (as a perfect sphere)
    /// </summary>
    /// <param name="lat1">latitude point 1 [°]</param>
    /// <param name="lon1">longitude point 1 [°]</param>
    /// <param name="lat2">latitude point 2 [°]</param>
    /// <param name="lon2">longitude point 2 [°]</param>
    /// <returns>distance [km]</returns>
    public double distance(double lat1, double lon1, double lat2, double lon2)
    {
      lat1 = lat1 / 180 * Math.PI;
      lat2 = lat2 / 180 * Math.PI;
      lon1 = lon1 / 180 * Math.PI;
      lon2 = lon2 / 180 * Math.PI;
      double retVal = 6371 * Math.Acos(Math.Sin(lat1) * Math.Sin(lat2) + Math.Cos(lat1) * Math.Cos(lat2) * Math.Cos(lon2 - lon1));
      return retVal;
    }
  }
}
