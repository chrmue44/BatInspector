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
  [TypeConverter(typeof(SpeciesInfoConfigurationTypeConverter))]
  [DataContract]
  public class SpeciesInfos
  {
    public SpeciesInfos(string abbr, string latin, string local, bool show, double fcMin, double fcMax, double dMin, double dMax,
                        double fMinMin, double fMinMax, double fMaxMin, double fMaxMax)
    {
      Abbreviation = abbr;
      Latin = latin;
      Local = local;
      Show = show;
      FreqCharMin = fcMin;
      FreqCharMax = fcMax;
      DurationMin = dMin;
      DurationMax = dMax;
      FreqMinMin = fMinMin;
      FreqMinMax = fMinMax;
      FreqMaxMin = fMaxMin;
      FreqMaxMax = fMaxMax;
      ProofSpecies = "TODO";
      CharCalls = "TODO";
    }

    public static SpeciesInfos find(string abbreviation, List<SpeciesInfos> list)
    {
      SpeciesInfos retVal = null;
      foreach (SpeciesInfos s in list)
      {
        if (abbreviation.ToLower() == s.Abbreviation.ToLower())
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
    [Description("lowest minimum frequency [kHz]")]
    public double FreqMinMin { get; set; }

    [DataMember]
    [Description("highest minimum frequency [kHz]")]
    public double FreqMinMax { get; set; }

    [DataMember]
    [Description("lowest maximum frequency [kHz]")]
    public double FreqMaxMin { get; set; }

    [DataMember]
    [Description("highest minimum frequency [kHz]")]
    public double FreqMaxMax { get; set; }

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

    public static bool isInList(List<SpeciesInfos> list, string species)
    {
      bool retVal = false;
      foreach (SpeciesInfos s in list)
      {
        if ((species.ToUpper() == s.Abbreviation.ToUpper()) && s.Show)
        {
          retVal = true;
          break;
        }
      }
      return retVal;
    }
  }


  public class BatInfo
  {
    [DataMember]
    [LocalizedCategory("SetCatBatSpecies"),
    LocalizedDescription("SetDescBatSpecies"),
    DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]

    const string _fName = "dat/batinfo.json";
    public List<SpeciesInfos> Species { get; set; }

    public void save()
    {
      string fPath = Environment.CurrentDirectory + "/" + _fName;
      try
      {
        StreamWriter file = new StreamWriter(fPath);
        MemoryStream stream = new MemoryStream();
        DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(BatInfo));
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

    public static BatInfo load()
    {
      BatInfo retVal = null;
      FileStream file = null;
      string fPath = Environment.CurrentDirectory + "/" + _fName;
      try
      {
        if (File.Exists(fPath))
        {
          file = new FileStream(fPath, FileMode.Open);
          DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(BatInfo));
          retVal = (BatInfo)ser.ReadObject(file);
          if (retVal == null)
            DebugLog.log("settings file not well formed!", enLogType.ERROR);
        }
        else
        {
          retVal = new BatInfo();
          retVal.initSpeciesInfos();
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

    public void initSpeciesInfos()
    {
      Species = new List<SpeciesInfos>();
      Species.Add(new SpeciesInfos("BBAR", "Barbastella barbastellus", "Mopsfledermaus", true, 31, 42, 2, 5, 25, 30, 38, 48));
      Species.Add(new SpeciesInfos("ESER", "Eptesicus Serotinus", "Breitflügelfledermaus", true, 21, 25, 10, 18, 22, 27, 35, 60));
      Species.Add(new SpeciesInfos("MBRA", "Myotis brandtii", "Große Bartfledermaus", true, 37, 50, 4, 7, 23, 30, 65, 100));
      Species.Add(new SpeciesInfos("MBEC", "Myotis bechsteinii", "Bechsteinfledermaus", true, 38, 50, 2.5, 6, 25, 40, 80, 100));
      Species.Add(new SpeciesInfos("MMYS", "Myotis mystacinus", "Kleine Bartfledermaus", true, 40, 57, 3, 6, 28, 35, 65, 100));
      Species.Add(new SpeciesInfos("MNAT", "Myotis nattereri", "Fransenfledermaus", true, 28, 53, 2, 5, 12, 25, 80, 150));
      Species.Add(new SpeciesInfos("MMYO", "Myotis myotis", "Großes Mausohr", true, 27, 37, 5, 10, 21, 26, 50, 75));
      Species.Add(new SpeciesInfos("MOXY", "Myotis oxygnatus", "Kleines Mausohr", true, 28, 40, 5, 10, 21, 26, 50, 80));
      Species.Add(new SpeciesInfos("MDAU", "Myotis daubentinii", "Wasserfledermaus", true, 37, 55, 3, 7, 25, 40, 55, 95));
      Species.Add(new SpeciesInfos("NNOC", "Nyctalus noctula", "Großer Abendsegler", true, 17, 29, 5, 28, 16, 27, 20, 40));
      Species.Add(new SpeciesInfos("NLEI", "Nyctalus leisleri", "Kleiner Abendsegler", true, 21, 30, 5, 20, 22, 26, 25, 40));
      Species.Add(new SpeciesInfos("PPIP", "Pipistrellus pipistrellus", "Zwergfledermaus", true, 41, 52, 3, 10, 42, 51, 44, 70));
      Species.Add(new SpeciesInfos("PPYG", "Pipistrellus pygmaeus", "Mückenfledermaus", true, 50, 64, 3, 10, 51, 56, 60, 80));
      Species.Add(new SpeciesInfos("PNAT", "Pipistrellus nathusii", "Rauhautfledermaus", true, 42, 49, 4, 11, 35, 41, 36, 70));
    }



  }
}
