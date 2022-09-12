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
    public bool inside(ParLocation p0, List<ParLocation> p)
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
  }
}
