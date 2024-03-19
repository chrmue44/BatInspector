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
  [TypeConverter(typeof(SpeciesInfoConfigurationTypeConverter))]
  [DataContract]
  public class SpeciesInfos
  {
    public SpeciesInfos(string abbr, string latin, string local, bool show, double fcMin, double fcMax, double dMin, double dMax,
                        double fMinMin, double fMinMax, double fMaxMin, double fMaxMax, double distMin, double distMax, string wavExample = null)
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
      CallDistMin = distMin;
      CallDistMax = distMax;
      ProofSpecies = "TODO";
      CharCalls = "TODO";
      WavExample = wavExample;
    }

    public static SpeciesInfos findAbbreviation(string abbreviation, List<SpeciesInfos> list)
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

    public static SpeciesInfos findLatin(string latin, List<SpeciesInfos> list)
    {
      SpeciesInfos retVal = null;
      foreach (SpeciesInfos s in list)
      {
        if (latin.ToLower() == s.Latin.ToLower())
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

    const string _fName = "batinfo.json";
    public List<SpeciesInfos> Species { get; set; }

    public void save(string fDir)
    {
      string fPath = Path.Combine(fDir, _fName);
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

    public static BatInfo loadFrom (string fDir)
    {
      BatInfo retVal = null;
      FileStream file = null;
      string fPath = Path.Combine(fDir, _fName);
      try
      {
        DebugLog.log("try loading BatInfo: " + fPath, enLogType.DEBUG);
        if (File.Exists(fPath))
        {
          file = new FileStream(fPath, FileMode.Open, FileAccess.Read);
          DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(BatInfo));
          retVal = (BatInfo)ser.ReadObject(file);
          if (retVal == null)
            DebugLog.log("settings file not well formed!", enLogType.ERROR);
        }
        else
        {
          DebugLog.log("BatInfo does not exist, create new file: " + fPath, enLogType.DEBUG);
          if(!Directory.Exists(fDir))
            Directory.CreateDirectory(fDir);
          retVal = new BatInfo();
          retVal.initSpeciesInfos();
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

    public void initSpeciesInfos()
    {
      Species = new List<SpeciesInfos>();
      Species.Add(new SpeciesInfos("BBAR", "Barbastella barbastellus", "Mopsfledermaus", true, 31, 42, 2, 5, 25, 30, 38, 48, 50, 75, "dat/Bbar_13.wav"));
      Species.Add(new SpeciesInfos("ENIL", "Eptesicus nilsonii", "Nordfledermaus", true, 26, 31, 8, 19, 26, 29, 35, 45, 120, 220, "dat/Eptesicus_nilssonii_Ski0125_S2_From0192948ms_To0203771ms.wav")); 
      Species.Add(new SpeciesInfos("ESER", "Eptesicus Serotinus", "Breitflügelfledermaus", true, 21, 25, 10, 18, 22, 27, 35, 60, 130, 180, "dat/Eser_Ski0113_S2_From2329314ms_To2354627ms.wav"));
      Species.Add(new SpeciesInfos("MALC", "Myotis alcathoe", "Nymphenfledermaus", true, 48, 65, 2, 4, 41, 47, 100, 130, 50, 90)); 
      Species.Add(new SpeciesInfos("MBEC", "Myotis bechsteinii", "Bechsteinfledermaus", true, 38, 50, 2.5, 6, 25, 40, 80, 100, 90, 110, "dat/Myotis_bechsteinii_Rid_6059.wav"));
      Species.Add(new SpeciesInfos("MBRA", "Myotis brandtii", "Große Bartfledermaus", true, 37, 50, 4, 7, 23, 30, 65, 100, 80, 110, "dat/Myotis_brandtii_Ski0120_S2_From2264955ms_To2272950ms.wav"));
      Species.Add(new SpeciesInfos("MDAS", "Myotis dasycneme", "Teichfledermaus", true, 36, 42, 5, 9, 25, 35, 65, 85, 80, 120));
      Species.Add(new SpeciesInfos("MDAU", "Myotis daubentonii", "Wasserfledermaus", true, 37, 55, 3, 7, 25, 40, 55, 95, 65, 95, "dat/Mdau_Ski0111_S2_From2386127ms_To2412446ms.wav"));
      Species.Add(new SpeciesInfos("MEMA", "Myotis emarginatus", "Wimperfledermaus", true, 48, 65, 1.5, 4, 30, 40, 90, 140, 40, 90));
      Species.Add(new SpeciesInfos("MMYO", "Myotis myotis", "Großes Mausohr", true, 27, 37, 5, 10, 21, 26, 50, 75, 90, 160, "dat/Mmyo_Ski0112_S1_From0748143ms_To0763637ms.wav"));
      Species.Add(new SpeciesInfos("MMYS", "Myotis mystacinus", "Kleine Bartfledermaus", true, 40, 57, 3, 6, 28, 35, 65, 100, 70, 90, "dat/Myotis_mystacinus_Ski0126_S2_From0066286ms_To0091143ms.wav"));
      Species.Add(new SpeciesInfos("MNAT", "Myotis nattereri", "Fransenfledermaus", true, 28, 53, 2, 5, 12, 25, 80, 150, 75, 110, "dat/Myotis_mystacinus_Ski0126_S2_From0066286ms_To0091143ms.wav"));
      Species.Add(new SpeciesInfos("MOXY", "Myotis oxygnatus", "Kleines Mausohr", true, 28, 40, 5, 10, 21, 26, 50, 80, 90, 160));
      Species.Add(new SpeciesInfos("MSCH", "Miniopterus schreibersii", "Langflügelfledermaus", true, 49, 55, 6, 15, 49, 55, 55, 80, 65, 140));
      Species.Add(new SpeciesInfos("NLAS", "Nyctalus lasiopterus", "Riesenabendsegler", true, 14.5, 23, 12, 28, 14, 20, 15, 25, 160, 820));
      Species.Add(new SpeciesInfos("NLEI", "Nyctalus leisleri", "Kleiner Abendsegler", true, 21, 30, 5, 20, 22, 26, 25, 40, 200, 400, "dat/Nlei_Ski0112_S2_From0359779ms_To0397522ms.wav"));
      Species.Add(new SpeciesInfos("NNOC", "Nyctalus noctula", "Großer Abendsegler", true, 17, 29, 5, 28, 16, 27, 20, 40, 250, 300, "dat/Nyctalus_noctula_Rid_5953.wav"));
      Species.Add(new SpeciesInfos("PKUH", "Pipistrellus kuhlii", "Weißrandfledermaus", true, 35, 42, 5, 12, 35, 42, 35, 60, 90, 130));
      Species.Add(new SpeciesInfos("PNAT", "Pipistrellus nathusii", "Rauhautfledermaus", true, 35, 42, 4, 11, 35, 41, 36, 70, 100, 130, "dat/Pnat_Ski0121_S1_From1276579ms_To1294660ms.wav"));
      Species.Add(new SpeciesInfos("PPIP", "Pipistrellus pipistrellus", "Zwergfledermaus", true, 41, 52, 3, 10, 42, 51, 44, 70, 75, 130, "dat/Ppip_Ski0112_S1_From0107809ms_To0159033ms.wav"));
      Species.Add(new SpeciesInfos("PPYG", "Pipistrellus pygmaeus", "Mückenfledermaus", true, 50, 64, 3, 10, 51, 56, 60, 80, 65, 95, "dat/Pipistrellus_pygmaeus_Rid_6162.wav"));
      Species.Add(new SpeciesInfos("PAUR", "Plecotus auritus", "Braunes Langohr", true, 22, 40, 2, 5, 18, 23, 45, 60, 40, 120, "dat/Plecotus_spec_Ski0126_S2_From0311629ms_To0329446ms.wav"));
      Species.Add(new SpeciesInfos("PAUS", "Plecotus austriacus", "Graues Langohr", true, 22, 32, 2, 6, 20, 25, 40, 50, 60, 150, "dat/Plecotus_spec_Ski0126_S2_From0311629ms_To0329446ms.wav"));
      Species.Add(new SpeciesInfos("RFER", "Rhinolophus ferrumequinum", "Große Hufeisennase", true, 77, 86, 35, 75, 50, 78, 77, 86, 80, 120));
      Species.Add(new SpeciesInfos("RHIP", "Rhinolophus hipposideros", "Kleine Hufeisennase", true, 100, 116, 16, 75, 83, 100, 100, 116,60, 100));
      Species.Add(new SpeciesInfos("VMUR", "Vespertilio murinus", "Zweifarbfledermaus", true, 22, 27, 10, 21, 21, 24, 30, 45, 75, 130, "dat/Vespertilio_murinus_Ski0150_S2_From0877624ms_To0904583ms.wav"));
    }
  }
}
