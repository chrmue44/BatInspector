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
    const string _fName = "BatSpeciesRegions.json";
    [DataMember]
    List<ParRegion> Regions { get; set; }

    public BatSpeciesRegions()
    {
      init();
    }

    public static BatSpeciesRegions loadFrom(string fDir)
    {
      BatSpeciesRegions retVal = null;
      FileStream file = null;
      string fPath = Path.Combine(fDir,  _fName);
      try
      {
        DebugLog.log("try loading BatSpeciesRegions: " + fPath, enLogType.DEBUG);
        if (File.Exists(fPath))
        {
          using (file = new FileStream(fPath, FileMode.Open, FileAccess.Read))
          {
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(BatSpeciesRegions));
            retVal = (BatSpeciesRegions)ser.ReadObject(file);
            if (retVal == null)
              DebugLog.log("regions file not well formed!", enLogType.ERROR);
          }
        }
        else
        {
          DebugLog.log("BatSpeciesRegions does not exist, create new file: " + fPath, enLogType.DEBUG);
          retVal = new BatSpeciesRegions();
          retVal.init();
          if(!Directory.Exists(fDir))
            Directory.CreateDirectory(fDir);
          retVal.save(fDir);
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
      ParRegion r = new ParRegion()
      {
        Name = "Darmstadt-Dieburg",
        Species = new List<string>() { "MMYO", "MBEC", "MNAT", "MMYS", "MBRA", "MALC", "MDAU", "NNOC", "NLEI",
                    "ESER", "ENIL", "VMUR", "PPIP", "PPYG", "PNAT", "BBAR", "PAUR", "PAUS",
                    "Eptesicus", "Nyctalus", "Myotis", "Vespertilio", "Pipistrellus" },
        InformationSource = "http://www.adb.naturkunde-institut-langstadt.de/arten/flederm.htm",
        Location = new List<ParLocation>(){ new ParLocation(49.963175,8.563220),
                                            new ParLocation(49.727209,8.539645),
                                            new ParLocation(49.728493,8.788927),
                                            new ParLocation(49.839054,9.030958),
                                            new ParLocation(49.995876,9.010885)}


      };
      Regions.Add(r);

      r = new ParRegion()
      {
        Name = "Deutschland",
        Species = new List<string>() { "BBAR", "ENIL", "ESER", "HSAV", "MSCH", "MALC", "MBEC", "MBRA",
          "MDAS", "MDAU", "MEMA", "MMYO", "MMYS", "MNAT", "NLAS", "NLEI",
          "NNOC", "PKUH", "PAUR", "PAUS", "RFER", "RHIP", "PNAT", "PPIP",
          "PPYG", "TTEN", "VMUR", "Eptesicus", "Myotis", "Nyctalus",
          "Vespertilio", "Pipistrellus" },
        InformationSource = "https://de.wikipedia.org/wiki/Liste_von_Fledermausarten_in_Deutschland",
        Location = new List<ParLocation>() { new ParLocation(54.850456, 9.943414),
                                             new ParLocation(54.627120, 13.459099),
                                             new ParLocation(53.575474, 14.797689),
                                             new ParLocation(50.845796, 14.835755),
                                             new ParLocation(50.184498, 12.231023),
                                             new ParLocation(48.774997, 13.811179),
                                             new ParLocation(47.491914, 13.011019),
                                             new ParLocation(47.284410, 10.217837),
                                             new ParLocation(47.593611,  7.540160),
                                             new ParLocation(48.968411,  8.211553),
                                             new ParLocation(49.489325,  6.345152),
                                             new ParLocation(51.847398,  5.990045),
                                             new ParLocation(53.761120,  7.548357) }
      }; Regions.Add(r);

      r = new ParRegion()
      {
        Name = "Norway",
        Species = new List<string>() { "ENIL", "MBRA", "MDAU", "MMYS", "MNAT", "NNOC", "PNAT", "PPYG", "PAUR",
          "VMUR", "Eptesicus", "Myotis", "Nyctalus", "Vespertilio", "Pipistrellus" },
        InformationSource = "https://en.wikipedia.org/wiki/List_of_mammals_of_Norway",
        Location = new List<ParLocation>() { new ParLocation(58.03315047073381, 7.27678231645965),
                                             new ParLocation(59.64141461545502, 4.988287311243992),
                                             new ParLocation(62.10627175787673, 4.434672775646467),
                                             new ParLocation(71.72799336070543, 25.381615291901824),
                                             new ParLocation(69.79114374529098, 32.16566376889443),
                                             new ParLocation(68.50180138485125, 24.717826186995556),
                                             new ParLocation(68.08832864271665, 18.357576864552833),
                                             new ParLocation(64.1026052585473, 13.845134278292283),
                                             new ParLocation(58.88181859511196, 11.65020431757494) }

      }; Regions.Add(r);

      r = new ParRegion()
      {
        Name = "Wales",
        Species = new List<string>() { "BBAR", "ESER", "MALC", "MBRA", "MDAU", "MNAT", "MMYS", "NLEI",
          "NNOC", "PAUR", "PNAT", "PPIP", "PPYG", "RFER", "RHIP",
          "Eptesicus", "Myotis", "Nyctalus", "Pipistrellus" },
        InformationSource = "",
        Location = new List<ParLocation>() { new ParLocation(51.35333, -2.62814),
                                             new ParLocation(53.47805, -2.81731),
                                             new ParLocation(53.53009, -4.86630),
                                             new ParLocation(51.35333, -5.88538) }
      }; Regions.Add(r);

      r = new ParRegion()
      {
        Name = "United Kingdom",
        Species = new List<string>() { "BBAR", "ESER", "MALC", "MBEC", "MBRA", "MDAU", "MMYS", "MNAT", "NLEI",
          "NNOC", "PAUR", "PAUS", "PNAT", "PPIP", "PPYG", "RFER", "RHIP",
          "Eptesicus", "Myotis", "Nyctalus", "Vespertilio", "Pipistrellus" },
        InformationSource = "https://www.bats.org.uk/about-bats/what-are-bats/uk-bats",
        Location = new List<ParLocation>() { new ParLocation(51.033539, 1.113506),
          new ParLocation(52.873203, 1.533058),
          new ParLocation(59.262795, -2.593458),
          new ParLocation(57.512548, -6.764195),
          new ParLocation(53.319476, -4.496293),
          new ParLocation(50.016064, -5.487250),
          new ParLocation(50.733974, 0.240915)
        }
      }; Regions.Add(r);
    }

    public void save(string fDir)
    {
      string fPath = Path.Combine(fDir, _fName);
      try
      {
        using (StreamWriter file = new StreamWriter(fPath))
        {
          using (MemoryStream stream = new MemoryStream())
          {
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(BatSpeciesRegions));
            ser.WriteObject(stream, this);
            StreamReader sr = new StreamReader(stream);
            stream.Seek(0, SeekOrigin.Begin);
            string str = sr.ReadToEnd();
            file.Write(JsonHelper.FormatJson(str));
            file.Close();
          }
        }
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
          break;
        }
      }
      return retVal;
    }


    public bool IsInRegion(double lat, double lon, string regionName)
    {
      bool retVal = false;
      ParLocation l = new ParLocation(lat, lon);
      foreach (ParRegion r in Regions)
      {
        if ((r.Name == regionName) && inside(l, r.Location))
        {
          retVal = true;
          break;
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

    public bool occursAtLocation(string speciesAbrv, double lat, double lon)
    {
      bool retVal = false;
      ParRegion r = findRegion(lat, lon);
      if(r == null) 
        retVal = true;  // if region not specified always return true
      else
      {
        foreach(string spec in r.Species) 
        {
          if(spec.ToLower() ==  speciesAbrv.ToLower())
          {
            retVal = true; 
            break;
          }
        }
      }
      return retVal;
    }
  }
}
