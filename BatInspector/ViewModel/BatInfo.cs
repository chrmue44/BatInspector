/********************************************************************************
 *               Author: Christian Müller
 *     Date of creation: 2023-09-01                                       
 *   Copyright (C) 2023: Christian Müller chrmue44(at)gmail(dot).com
 *
 *              Licence:  CC BY-NC 4.0 
 ********************************************************************************/

using BatInspector.Properties;
using libParser;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Navigation;


namespace BatInspector
{

  public enum enCallChar
  {
    CF = 0,
    FM = 1,
    FM_QCF = 2,
    FM_QCF_FM = 3,
    QCF = 4,
    QCF_FM = 5
  }

  public enum enIdentifiable
  {
    CHARACTERISTIC,
    PARTLY,
    NO
  }

  public enum enYesNoProperty
  {
    YES = 0,
    NO = 1,
    DONT_CARE = 2
  }


  public class CallData
  {
    public double FreqStart { get; set; }
    public double FreqEnd { get; set; }
    public double FreqChar { get; set; }
    public double Duration { get; set; }
    public double CallInterval { get; set; }
    public double FreqMk { get; set; }
    public enCallChar CallCharacteristic { get; set; }

    public enYesNoProperty HasCallTypeAandB { get; set; }
    public enYesNoProperty HasMyotisKink { get; set; }
    public enYesNoProperty HasKneeClearly { get; set; }   // Knie deutlich ausgeprägt

    public enYesNoProperty IsUniformFormFreqInt { get; set; }
    public enYesNoProperty HasUpwardHookAtEnd { get; set; }

    public enYesNoProperty HasNoCallChanges { get; set; }
    public enYesNoProperty IsConvex { get; set; }
    public enYesNoProperty HasStrongHarmonic { get; set; }

    public CallData()
    {
      FreqStart = -1;
      FreqEnd = -1;
      FreqChar = -1;
      Duration = -1;
      CallInterval = -1;
      FreqMk = -1;
      CallCharacteristic = enCallChar.QCF;
      HasCallTypeAandB = enYesNoProperty.DONT_CARE;
      HasMyotisKink = enYesNoProperty.DONT_CARE;
      HasKneeClearly = enYesNoProperty.DONT_CARE;
      IsUniformFormFreqInt = enYesNoProperty.DONT_CARE;
      HasUpwardHookAtEnd = enYesNoProperty.DONT_CARE;
      HasNoCallChanges = enYesNoProperty.DONT_CARE;
      IsConvex = enYesNoProperty.DONT_CARE;
      HasStrongHarmonic = enYesNoProperty.DONT_CARE;
    }

    public CallData(double freqStart, double freqEnd, double freqChar, double duration, double callInterval, double freqMk, enCallChar callChar)
    {
      FreqStart = freqStart;
      FreqEnd = freqEnd;
      FreqChar = freqChar;
      Duration = duration;
      CallInterval = callInterval;
      FreqMk = freqMk;
      CallCharacteristic = callChar;
      HasCallTypeAandB = enYesNoProperty.DONT_CARE;
      HasMyotisKink = enYesNoProperty.DONT_CARE;
      HasKneeClearly = enYesNoProperty.DONT_CARE;
      IsUniformFormFreqInt = enYesNoProperty.DONT_CARE;
      HasUpwardHookAtEnd = enYesNoProperty.DONT_CARE;
      HasNoCallChanges = enYesNoProperty.DONT_CARE;
      IsConvex = enYesNoProperty.DONT_CARE;
      HasStrongHarmonic = enYesNoProperty.DONT_CARE;
    }
  }



  [DataContract]
  public class ValRange
  {
    [DataMember]
    public double Min { get; set; }
    [DataMember]
    public double NormMin { get; set; }
    [DataMember]
    public double NormMax { get; set; }
    [DataMember]
    public double Max { get; set; }

    public ValRange()
    {
      Min = -1.0;
      Max = -1.0;
      NormMin = -1.0;
      NormMax = -1.0;
    }

    public ValRange(double min, double normMin, double normMax, double max)
    {
      Min = min;
      NormMin = normMin;
      NormMax = normMax;
      Max = max;
    }


  }

  [TypeConverter(typeof(SpeciesInfoConfigurationTypeConverter))]
  [DataContract]
  public class CheckData
  {
    public const int DURATION_SHORT = 0;
    public const int DURATION_MEDIUM = 1;
    public const int DURATION_LONG = 2;

    [DataMember]
    public ValRange FreqStart { get; set; } /// start frequency [kHz]
    [DataMember]
    public ValRange FreqEnd { get; set; } /// end frequency [kHz]
    [DataMember]
    public ValRange FreqChar { get; set; } /// end frequency [kHz]
    [DataMember]
    public ValRange Duration { get; set; } /// duration of call [ms]
    [DataMember]
    public ValRange FreqMk { get; set; }  /// >=0: frequency myotis knee [kHz]
    [DataMember]
    public enCallChar CallCharacteristic { get; set; }
    [DataMember]
    public enIdentifiable Identifiable { get; set; }
  }


  [TypeConverter(typeof(SpeciesInfoConfigurationTypeConverter))]
  [DataContract]
  public class SpeciesInfos
  {
    public SpeciesInfos(string abbr, string latin, string local, int page, string notDist, bool show, double fcMin, double fcMax, double dMin, double dMax,
                        double fMinMin, double fMinMax, double fMaxMin, double fMaxMax, double distMin, double distMax,
                        CheckData[] check = null, string wavExample = null)
    {
      Abbreviation = abbr;
      Latin = latin;
      Local = local;
      NotDistinguishableFrom = notDist;
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
      CheckData = check;
      PageNr = page;
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
      if (retVal == null)
      {
        if (abbreviation == "Myotis")
          retVal = new SpeciesInfos("Myotis", "Myotis", "Myotis", 1,"", false, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
        else if (abbreviation == "Plecotus")
          retVal = new SpeciesInfos("Plecotus", "Plecotus", "Plecotus",1, "", false, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
        else if (abbreviation == "Pipistrellus")
          retVal = new SpeciesInfos("Pipistrellus", "Pipistrellus", "Pipistrellus",1, "", false, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
        else if (abbreviation == "Nyctaloid")
          retVal = new SpeciesInfos("Nyctaloid", "Nyctaloid", "Nyctaloid",1, "", false, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
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

    public string getGenus()
    {
      string[] names = Latin.Split(' ');
      if (names.Length > 0)
        return names[0];
      else
        return "unknown genus";
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
    [Description("page in PDF description")]
    public int PageNr { get; set; }

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
    [Description("Species of confusion")]
    public string ConfusionSpec { get; set; } = "";

    [DataMember]
    [Description("species (abbr) not distinguishable from")]
    public string NotDistinguishableFrom { get; set; } = "";

    [DataMember]
    [LocalizedDescription("SpecDescWav")]
    public string WavExample { get; set; }

    [DataMember]
    [LocalizedDescription("SpecDescWav")]
    public CheckData[] CheckData { get; set; }

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
        using (StreamWriter file = new StreamWriter(fPath))
        {
          using (MemoryStream stream = new MemoryStream())
          {
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(BatInfo));
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
        DebugLog.log("failed to write config file for BatInspector" + fPath + ": " + e.ToString(), enLogType.ERROR);
      }
    }

    public static void copyInfoFileAfterSetup(string srcDir, string dstDir)
    {
      string srcFileName = Path.Combine(srcDir, _fName);
      string dstFileName = Path.Combine(dstDir, _fName);
      try
      {
        if (File.Exists(dstFileName))
        {
          if (File.GetLastWriteTime(dstFileName) < File.GetLastWriteTime(srcFileName))
          {
            MessageBoxResult res = MessageBox.Show(MyResources.msgNewVerBatInfo,
               BatInspector.Properties.MyResources.msgQuestion, MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (res == MessageBoxResult.Yes)
              File.Copy(srcFileName, dstFileName, true);
          }
        }
        else
          File.Copy(srcFileName, dstFileName, true);
      }
      catch (Exception ex)
      {
        DebugLog.log($"unable to copy {srcFileName} : {ex.ToString()}", enLogType.ERROR);
      }
    }

    public static BatInfo loadFrom(string fDir)
    {
      BatInfo retVal = null;
      FileStream file = null;
      string fPath = Path.Combine(fDir, _fName);
      try
      {
        DebugLog.log("try loading BatInfo: " + fPath, enLogType.DEBUG);
        if (File.Exists(fPath))
        {
          using (file = new FileStream(fPath, FileMode.Open, FileAccess.Read))
          {
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(BatInfo));
            retVal = (BatInfo)ser.ReadObject(file);
            if (retVal == null)
              DebugLog.log("settings file not well formed!", enLogType.ERROR);
          }
        }
        else
        {
          DebugLog.log("BatInfo does not exist, create new file: " + fPath, enLogType.DEBUG);
          if (!Directory.Exists(fDir))
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
      //Data from "Bayrisches Landesamt für Umwelt"
      CheckData[] cBbar = {new CheckData() { FreqStart = new   ValRange(33.0, 33.0, 36.0, 39.0 ),
                                             FreqEnd = new ValRange(27.0, 27.0 ,30.0, 32.0 ),
                                             Duration = new ValRange(2.0, 2.0, 3.5, 5.0 ),
                                             CallCharacteristic = enCallChar.FM,
                                             Identifiable = enIdentifiable.CHARACTERISTIC},
                          new CheckData() { FreqStart = new ValRange(40.0, 43.0, 46.0, 50.0 ),
                                             FreqEnd = new ValRange(29.0, 30.0, 35.0, 37.0 ),
                                             Duration = new ValRange(2.0, 2.0, 6.0,10.0 ),
                                             CallCharacteristic = enCallChar.FM,
                                             Identifiable = enIdentifiable.CHARACTERISTIC},
                          new CheckData() { FreqStart = new ValRange(42.0, 46.0, 55.0, 55.0 ),
                                             FreqEnd = new ValRange(20.0, 25.0, 30.0, 30.0 ),
                                             Duration = new ValRange(2.0, 2.0, 5.0,6.0 ),
                                             CallCharacteristic = enCallChar.FM,
                                             Identifiable = enIdentifiable.PARTLY}
                          };

      CheckData[] cEnil = {new CheckData() { FreqChar= new ValRange(25.0, 26.0, 30.0, 30.0 ),
                                             Duration = new ValRange(10.0, 10.0, 22.0,25.0 ),
                                             CallCharacteristic = enCallChar.QCF,
                                             Identifiable = enIdentifiable.CHARACTERISTIC},
                           new CheckData() { FreqChar= new ValRange(26.0, 26.0, 33.0, 33.0 ),
                                             Duration = new ValRange(4.0, 5.0, 19.0,19.0 ),
                                             CallCharacteristic = enCallChar.FM_QCF,
                                             Identifiable = enIdentifiable.CHARACTERISTIC},
                           new CheckData() { FreqMk= new ValRange(26.0, 26.0, 35.0,35.0 ),
                                             Duration = new ValRange(3.0, 3.5, 6.5,7.0 ),
                                             CallCharacteristic = enCallChar.FM,
                                             Identifiable = enIdentifiable.NO}
                          };

      CheckData[] cEser = {new CheckData() { FreqChar= new ValRange(21.0, 21.0, 25.0, 26.0),
                                             Duration = new ValRange(10.0, 10.0, 16.0, 18.0),
                                             CallCharacteristic = enCallChar.QCF,
                                             Identifiable = enIdentifiable.CHARACTERISTIC},
                           new CheckData() { FreqChar= new ValRange(22.0,22.0, 31.0, 31.0),
                                             Duration = new ValRange(4.0, 4.0,16.0, 18.0 ),
                                             CallCharacteristic = enCallChar.FM_QCF,
                                             Identifiable = enIdentifiable.CHARACTERISTIC},
                           new CheckData() { FreqChar= new ValRange(26.0, 26.0, 30.8, 31.0 ),
                                             Duration = new ValRange(4.0, 4.2,16.0, 18.0 ),
                                             CallCharacteristic = enCallChar.FM_QCF,
                                             Identifiable = enIdentifiable.NO},
                           new CheckData() { FreqMk= new ValRange(25.0, 26.0, 34.0, 34.0 ),
                                             Duration = new ValRange(3.0, 3.0, 7.0, 7.0 ),
                                             CallCharacteristic = enCallChar.FM,
                                             Identifiable = enIdentifiable.NO}
                          };

      CheckData[] cHsav = {new CheckData() { FreqChar= new ValRange(30.0, 31.0, 35.0, 36.0 ),
                                             Duration = new ValRange(8.0, 8.0, 15.0,17.0 ),
                                             CallCharacteristic = enCallChar.QCF,
                                             Identifiable = enIdentifiable.CHARACTERISTIC},
                           new CheckData() { FreqChar= new ValRange(31.0, 31.1, 38.0, 42.0 ),
                                             Duration = new ValRange(4.0, 5.0, 13.8,14.0 ),
                                             CallCharacteristic = enCallChar.FM_QCF,
                                             Identifiable = enIdentifiable.CHARACTERISTIC},
                           new CheckData() { FreqChar= new ValRange(34.0, 34.0, 40.9,41.0 ),
                                             Duration = new ValRange(3.0, 3.0, 5.0, 7.0 ),
                                             CallCharacteristic = enCallChar.FM,
                                             Identifiable = enIdentifiable.NO},
                          };

      CheckData[] cMdau = {new CheckData() { FreqStart = new ValRange(73.0, 82.0, 100.0, 101.0),
                                             FreqEnd = new ValRange(22.0, 25.0, 32.0,36.0 ),
                                             FreqMk = new ValRange(35.0, 40.0, 45.9, 46.0 ),
                                             Duration = new ValRange(1.5, 1.5, 3.5, 3.5 ),
                                             CallCharacteristic = enCallChar.FM,
                                             Identifiable = enIdentifiable.PARTLY},
                           new CheckData() { FreqStart = new ValRange(70.0, 82.0, 89.0, 103.0 ),
                                             FreqEnd = new ValRange(20.0, 26.0, 30.0, 38.0 ),
                                             FreqMk = new ValRange(34.0, 40.0, 44.0, 48.0 ),
                                             Duration = new ValRange(3.5, 3.5, 6.0, 6.0 ),
                                             CallCharacteristic = enCallChar.FM,
                                             Identifiable = enIdentifiable.PARTLY },
                           new CheckData() { FreqStart = new ValRange(60.0, 80.0, 91.0, 91.0 ),
                                             FreqEnd = new ValRange(20.0, 25.0, 37.0,37.0 ),
                                             FreqMk = new ValRange(33.0, 34.0, 38, 43.0),
                                             Duration = new ValRange(6.0, 6.0, 8.0,8.0 ),
                                             CallCharacteristic = enCallChar.FM,
                                             Identifiable = enIdentifiable.PARTLY }
          };
      CheckData[] cMdas = {new CheckData() { FreqStart = new ValRange(70.0, 75.0, 90.0, 93.0 ),
                                             FreqEnd = new ValRange(25.0, 26.0, 30.0, 30.0 ),
                                             FreqMk = new ValRange(32.0, 32.0, 36.0, 36.0 ),
                                             Duration = new ValRange(2.4, 2.4, 3.5, 3.5 ),
                                             CallCharacteristic = enCallChar.FM,
                                             Identifiable = enIdentifiable.PARTLY },
                           new CheckData() { FreqStart = new ValRange(65.0, 72.0, 97.0, 103.0 ),
                                             FreqEnd = new ValRange(21.0, 22.0, 32.0, 32.0 ),
                                             FreqMk = new ValRange(28.0, 32.0, 37.0, 37.0 ),
                                             Duration = new ValRange(3.5, 3.5, 6.0, 6.0 ),
                                             CallCharacteristic = enCallChar.FM,
                                             Identifiable = enIdentifiable.PARTLY },
                           new CheckData() { FreqStart = new ValRange(66.0, 66.0, 95.0, 101.0 ),
                                             FreqEnd = new ValRange(23.0, 24.0, 31.0, 31.0 ),
                                             FreqMk = new ValRange(27.0, 30.0, 36.0, 36.0 ),
                                             Duration = new ValRange(6.0, 6.0, 8.0, 8.0 ),
                                             CallCharacteristic = enCallChar.FM,
                                             Identifiable = enIdentifiable.NO },
                           new CheckData() { FreqStart = new ValRange(38.0, 41.0, 78.0, 86.0 ),
                                             FreqEnd = new ValRange(23.0, 25.0, 33.0, 33.0 ),
                                             FreqMk = new ValRange(29.0, 30.0, 36.0, 37.0 ),
                                             Duration = new ValRange(8.0, 8.0, 23.0, 23.0 ),
                                             CallCharacteristic = enCallChar.FM,
                                             Identifiable = enIdentifiable.CHARACTERISTIC }
          };
      CheckData[] cMalc = {new CheckData() { FreqStart = new ValRange(93.0, 100.0, 129.0, 135.0 ),
                                             FreqEnd = new ValRange(35.0, 40.0, 47.0, 52.0 ),
                                             FreqMk = new ValRange(43.0, 45.0, 55.0,56.0 ),
                                             Duration = new ValRange(1.4, 1.4, 3.5,3.5 ),
                                             CallCharacteristic = enCallChar.FM,
                                             Identifiable = enIdentifiable.CHARACTERISTIC },
                           new CheckData() { FreqStart = new ValRange(88.0, 94.0, 120.0, 123.0 ),
                                             FreqEnd = new ValRange(36.0, 38.0, 46.0, 48.0 ),
                                             FreqMk = new ValRange(43.0, 45.0, 50.0, 52.0 ),
                                             Duration = new ValRange(3.5, 3.5, 5.0,5.5 ),
                                             CallCharacteristic = enCallChar.FM,
                                             Identifiable = enIdentifiable.CHARACTERISTIC }
       };

      CheckData[] cMmys = {new CheckData() { FreqStart = new ValRange(90.0, 105.0, 128.0, 140.0 ),
                                             FreqEnd = new ValRange(23.0, 25.0, 36.0, 39.0 ),
                                             FreqMk = new ValRange(35.0, 43.0, 46.0, 48.0 ),
                                             Duration = new ValRange(1.5, 1.5, 3.5, 3.5 ),
                                             CallCharacteristic = enCallChar.FM,
                                             Identifiable = enIdentifiable.PARTLY },
                           new CheckData() { FreqStart = new ValRange(72.0, 86.0, 115.0, 122.0 ),
                                             FreqEnd = new ValRange(20.0, 25.0, 34.0, 37.0 ),
                                             FreqMk = new ValRange(33.0, 35.0, 42.0, 44.0 ),
                                             Duration = new ValRange(3.5, 3.5, 6.0, 6.0),
                                             CallCharacteristic = enCallChar.FM,
                                             Identifiable = enIdentifiable.PARTLY },
                           new CheckData() { FreqStart = new ValRange(70.0, 75.0, 88.0, 88.0 ),
                                             FreqEnd = new ValRange(27.0, 27.0, 31.0, 31.0 ),
                                             FreqMk = new ValRange(33.0, 33.0, 39.0, 39.0 ),
                                             Duration = new ValRange(6.0, 6.0, 8.0,8.0 ),
                                             CallCharacteristic = enCallChar.FM,
                                             Identifiable = enIdentifiable.NO }
      };
      CheckData[] cMbec = {new CheckData() { FreqStart = new ValRange(114.0, 125.0, 140.0, 150.0 ),
                                             FreqEnd = new ValRange(24.0, 28.0, 34.0, 36.0 ),
                                             FreqMk = new ValRange(37.0, 38.0, 45.0, 45.0 ),
                                             Duration = new ValRange(1.5, 1.5, 3.5, 3.5 ),
                                             CallCharacteristic = enCallChar.FM,
                                             Identifiable = enIdentifiable.PARTLY },
                           new CheckData() { FreqStart = new ValRange(99.0, 110.0, 130.0, 140.0 ),
                                             FreqEnd = new ValRange(21.0, 23.0, 27.0, 32.0 ),
                                             FreqMk = new ValRange(35.0, 38.0, 43.0, 46.0 ),
                                             Duration = new ValRange(3.5, 3.5, 6.0, 6.0 ),
                                             CallCharacteristic = enCallChar.FM,
                                             Identifiable = enIdentifiable.PARTLY },
                           new CheckData() { FreqStart = new ValRange(100.0, 106.0, 116.0, 116.0 ),
                                             FreqEnd = new ValRange(20.0, 23.0, 30.0, 30.0 ),
                                             FreqMk = new ValRange(33.0, 34.0, 38.0, 42.0 ),
                                             Duration = new ValRange(6.0, 6.0,9.1,9.1 ),
                                             CallCharacteristic = enCallChar.FM,
                                             Identifiable = enIdentifiable.PARTLY }
      };
      CheckData[] cMema = {new CheckData() { FreqStart = new ValRange(123.0, 130.0, 160.0, 175.0 ),
                                             FreqEnd = new ValRange(28.0, 33.0, 42.0, 45.0 ),
                                             FreqMk = new ValRange(45.0, 45.0, 50.0, 50.0 ),
                                             Duration = new ValRange(1.8, 1.8, 3.5, 3.5 ),
                                             CallCharacteristic = enCallChar.FM,
                                             Identifiable = enIdentifiable.PARTLY },
                           new CheckData() { FreqStart = new ValRange(97.0, 108.0, 150.0, 158.0 ),
                                             FreqEnd = new ValRange(30.0, 30.0, 45.0, 45.0 ),
                                             FreqMk = new ValRange(44.0, 44.0, 52.0, 60.0 ),
                                             Duration = new ValRange(3.5, 3.5, 6.0, 6.0 ),
                                             CallCharacteristic = enCallChar.FM,
                                             Identifiable = enIdentifiable.PARTLY }
      };
      CheckData[] cMmyo = {new CheckData() { FreqStart = new ValRange(90.0, 94.0, 110.0, 120.0),
                                             FreqEnd = new ValRange(17.0, 20.0, 27.0, 32.0 ),
                                             Duration = new ValRange(2.5, 2.5, 3.5, 3.5 ),
                                             CallCharacteristic = enCallChar.FM,
                                             Identifiable = enIdentifiable.CHARACTERISTIC },
                           new CheckData() { FreqStart = new ValRange(70.0, 90.0, 105.0, 120.0 ),
                                             FreqEnd = new ValRange(16.0, 23.0, 25.0, 27.0 ),
                                             FreqMk = new ValRange(27.0, 27.0, 35.0, 38.0 ),
                                             Duration = new ValRange(3.5, 3.5, 6.0, 6.0 ),
                                             CallCharacteristic = enCallChar.FM,
                                             Identifiable = enIdentifiable.CHARACTERISTIC },
                           new CheckData() { FreqStart = new ValRange(52.0, 70.0, 90.0, 110.0),
                                             FreqEnd = new ValRange(17.0, 17.0,25.0, 27.0),
                                             FreqMk = new ValRange(27.0, 27.0, 30.0, 32.0 ),
                                             Duration = new ValRange(6.0, 6.0, 12.0, 12.0 ),
                                             CallCharacteristic = enCallChar.FM,
                                             Identifiable = enIdentifiable.CHARACTERISTIC }
      };
      CheckData[] cMnat = {new CheckData() { FreqStart = new ValRange(120.0, 135.0, 150.0, 185.0 ),
                                             FreqEnd = new ValRange(10.0, 14.0, 23.0, 23.0 ),
                                             Duration = new ValRange(2.0, 2.0, 3.5, 3.5 ),
                                             CallCharacteristic = enCallChar.FM,
                                             Identifiable = enIdentifiable.CHARACTERISTIC  },
                           new CheckData() { FreqStart = new ValRange(102.0, 105.0, 120.0, 135.0 ),
                                             FreqEnd = new ValRange(14.0, 14.0, 23.0, 23.0 ),
                                             FreqMk = new ValRange(23.0,  23.0, 37.0, 41.0 ),
                                             Duration = new ValRange(3.5, 3.5, 6.0, 6.0 ),
                                             CallCharacteristic = enCallChar.FM,
                                             Identifiable = enIdentifiable.CHARACTERISTIC  },
                           new CheckData() { FreqStart = new ValRange(100.0, 109.0, 131.0, 131.0 ),
                                             FreqEnd = new ValRange(14.0, 19.0, 25.0, 27.0 ),
                                             FreqMk = new ValRange(25.0, 25.0, 40.0, 44.0 ),
                                             Duration = new ValRange(6.0, 6.0, 8.0, 8.0 ),
                                             CallCharacteristic = enCallChar.FM,
                                             Identifiable = enIdentifiable.PARTLY }
                          };

      CheckData[] cNlei = {new CheckData() { FreqChar= new ValRange(21.0, 21.0, 27.0, 27.0),
                                             Duration = new ValRange(6.0, 8.0, 20.0, 20.0),
                                             CallCharacteristic = enCallChar.CF,
                                             Identifiable = enIdentifiable.CHARACTERISTIC},
                           new CheckData() { FreqChar= new ValRange(21.0, 21.0, 27.0, 27.0),
                                             Duration = new ValRange(6.0, 8.0, 20.0, 20.0),
                                             CallCharacteristic = enCallChar.QCF,
                                             Identifiable = enIdentifiable.CHARACTERISTIC},
                           new CheckData() { FreqChar= new ValRange(24.0, 25.0, 29.0, 31.0 ),
                                             Duration = new ValRange(4.0, 5.0, 16.0, 18.0 ),
                                             CallCharacteristic = enCallChar.FM_QCF,
                                             Identifiable = enIdentifiable.NO},
                           new CheckData() { FreqMk= new ValRange(26.0, 28.0, 32.0, 32.0 ),
                                             Duration = new ValRange(3.0, 3.0, 5.0, 6.0),                                         CallCharacteristic = enCallChar.FM,
                                             Identifiable = enIdentifiable.NO}
                          };

      CheckData[] cNnoc = {new CheckData() { FreqChar= new ValRange(16.0, 17.0, 22.0, 23.0 ),
                                             Duration = new ValRange(10.0, 15.0, 30.0, 30.0 ),
                                             CallCharacteristic = enCallChar.QCF,
                                             Identifiable = enIdentifiable.CHARACTERISTIC},
                           new CheckData() { FreqChar= new ValRange(16.0, 17.0, 22.0, 23.0 ),
                                             Duration = new ValRange(10.0, 15.0, 30.0, 30.0 ),
                                             CallCharacteristic = enCallChar.CF,
                                             Identifiable = enIdentifiable.CHARACTERISTIC},
                           new CheckData() { FreqChar= new ValRange(19.0, 20.0, 29.0,30.0 ),
                                             Duration = new ValRange(6.0, 6.0, 20.0, 20.0 ),
                                             CallCharacteristic = enCallChar.FM_QCF,
                                             Identifiable = enIdentifiable.PARTLY},
                           new CheckData() { FreqMk= new ValRange(24.0, 26.0, 30.0, 30.0 ),
                                             Duration = new ValRange(3.0, 3.0, 7.0, 7.0 ),
                                             CallCharacteristic = enCallChar.FM,
                                             Identifiable = enIdentifiable.NO}
                          };

      CheckData[] cPaur = {new CheckData() { FreqStart = new ValRange(35.0, 35.0, 60.0, 60.0 ),
                                             FreqMk = new ValRange(11.0, 20.0, 35.0, 35.0 ),
                                             Duration = new ValRange(2.0, 2.0, 8.0, 8.0 ),
                                             CallCharacteristic = enCallChar.FM,
                                             Identifiable = enIdentifiable.CHARACTERISTIC }
                          };

      CheckData[] cPnat = {new CheckData() { FreqChar = new ValRange(34.0, 35.0, 41.0, 42.0 ),
                                             Duration = new ValRange(5.0, 5.0, 10.0, 12.0 ),
                                             CallCharacteristic = enCallChar.QCF,
                                             Identifiable = enIdentifiable.CHARACTERISTIC},
                           new CheckData() { FreqChar = new ValRange(34.0, 35.0, 41.0, 42.0 ),
                                             Duration = new ValRange(5.0, 5.0, 10.0, 12.0 ),
                                             CallCharacteristic = enCallChar.CF,
                                             Identifiable = enIdentifiable.CHARACTERISTIC},
                           new CheckData() { FreqChar = new ValRange(35.0, 35.0, 45.0, 45.0 ),
                                             Duration = new ValRange(3.0, 4.0, 11.0, 11.0 ),
                                             CallCharacteristic = enCallChar.FM_QCF,
                                             Identifiable = enIdentifiable.CHARACTERISTIC},
                           new CheckData() { FreqMk = new ValRange(36.0, 36.0, 46.0, 46.0 ),
                                             Duration = new ValRange(3.0, 3.0, 5.0, 6.0 ),
                                             CallCharacteristic = enCallChar.FM,
                                             Identifiable = enIdentifiable.NO},
                          };

      CheckData[] cPpip = {new CheckData() { FreqChar = new ValRange(40.1, 40.1, 50.0, 50.0 ),
                                             Duration = new ValRange(5.0, 5.0, 10.0, 11.0 ),
                                             CallCharacteristic = enCallChar.QCF,
                                             Identifiable = enIdentifiable.CHARACTERISTIC},
                           new CheckData() { FreqChar = new ValRange(40.1, 40.1, 50.0, 50.0 ),
                                             Duration = new ValRange(5.0, 5.0, 10.0, 11.0 ),
                                             CallCharacteristic = enCallChar.CF,
                                             Identifiable = enIdentifiable.CHARACTERISTIC},
                           new CheckData() { FreqChar = new ValRange(41.0, 42.0, 54.0, 54.0 ),
                                             Duration = new ValRange(3.0, 3.0, 10.0, 10.0 ),
                                             CallCharacteristic = enCallChar.FM_QCF,
                                             Identifiable = enIdentifiable.CHARACTERISTIC},
                           new CheckData() { FreqMk = new ValRange(44.0, 45.0, 55.0, 55.0 ),
                                             Duration = new ValRange(3.0, 3.0, 5.0, 6.0 ),
                                             CallCharacteristic = enCallChar.FM,
                                             Identifiable = enIdentifiable.NO},
                          };

      CheckData[] cPpyg = {new CheckData() { FreqChar = new ValRange(49.0, 50.0, 56.0, 60.0 ),
                                             Duration = new ValRange(4.0, 5.0, 8.0, 10.0 ),
                                             CallCharacteristic = enCallChar.QCF,
                                             Identifiable = enIdentifiable.CHARACTERISTIC},
                           new CheckData() { FreqChar = new ValRange(49.0, 50.0, 56.0, 60.0 ),
                                             Duration = new ValRange(4.0, 5.0, 8.0, 10.0 ),
                                             CallCharacteristic = enCallChar.CF,
                                             Identifiable = enIdentifiable.CHARACTERISTIC},
                           new CheckData() { FreqChar = new ValRange(50.0, 51.0, 64.0, 68.0 ),
                                             Duration = new ValRange(3.0, 3.0, 10.0, 10.0 ),
                                             CallCharacteristic = enCallChar.FM_QCF,
                                             Identifiable = enIdentifiable.CHARACTERISTIC},
                           new CheckData() { FreqMk = new ValRange(56.0, 56.0, 68.0, 68.0 ),
                                             Duration = new ValRange(3.0, 3.0, 5.0, 7.0 ),
                                             CallCharacteristic = enCallChar.QCF,
                                             Identifiable = enIdentifiable.NO},
                          };

      CheckData[] cRfer = {new CheckData() { FreqChar = new ValRange(77.0, 78.0, 83.0, 86.0 ),
                                             Duration = new ValRange(16.0, 30.0, 60.0, 75.0 ),
                                             CallCharacteristic = enCallChar.FM_QCF_FM,
                                             Identifiable = enIdentifiable.CHARACTERISTIC}
                          };
      CheckData[] cRhip = {new CheckData() { FreqChar = new ValRange(100.0, 105.0, 114.0, 116.0 ),
                                             Duration = new ValRange(16.0, 20.0, 60.0, 75.0 ),
                                             CallCharacteristic = enCallChar.FM_QCF_FM,
                                             Identifiable = enIdentifiable.CHARACTERISTIC }
                          };

      CheckData[] cVmur = {new CheckData() { FreqChar = new ValRange(21.0, 25.0, 26.0, 26.0 ),
                                             Duration = new ValRange(10.0, 12.0, 26.0, 30.0 ),
                                             CallCharacteristic = enCallChar.CF,
                                             Identifiable = enIdentifiable.PARTLY},
                           new CheckData() { FreqChar = new ValRange(21.0, 25.0, 26.0, 26.0 ),
                                             Duration = new ValRange(10.0, 12.0, 26.0, 30.0 ),
                                             CallCharacteristic = enCallChar.QCF,
                                             Identifiable = enIdentifiable.PARTLY},
                           new CheckData() { FreqChar = new ValRange(22.0, 23.0, 30.0, 30.0 ),
                                             Duration = new ValRange(4.0, 4.0, 14.0, 16.0 ),
                                             CallCharacteristic = enCallChar.FM_QCF,
                                             Identifiable = enIdentifiable.NO },
                           new CheckData() { FreqMk = new ValRange(23.0, 25.0, 30.0, 30.0 ),
                                             Duration = new ValRange(3.0, 3.0, 5.0, 7.0 ),
                                             CallCharacteristic = enCallChar.FM,
                                             Identifiable = enIdentifiable.NO }
                          };


      Species = new List<SpeciesInfos>();
      Species.Add(new SpeciesInfos("BBAR", "Barbastella barbastellus", "Mopsfledermaus", 73, "", true, 31, 42, 2, 5, 25, 30, 38, 48, 50, 75, cBbar, "dat/Bbar_13.wav"));
      Species.Add(new SpeciesInfos("ENIL", "Eptesicus nilssonii", "Nordfledermaus", 47, "", true, 26, 31, 8, 19, 26, 29, 35, 45, 120, 220, cEnil, "dat/Eptesicus_nilssonii_Ski0125_S2_From0192948ms_To0203771ms.wav"));
      Species.Add(new SpeciesInfos("ESER", "Eptesicus Serotinus", "Breitflügelfledermaus", 43, "", true, 21, 25, 10, 18, 22, 27, 35, 60, 130, 180, cEser, "dat/Eser_Ski0113_S2_From2329314ms_To2354627ms.wav"));
      Species.Add(new SpeciesInfos("HSAV", "Hypsugo savii", "Alpenfledermaus", 51, "", true, 31, 36, 6, 12, 31, 36, 35, 50, 180, 370, cHsav));
      Species.Add(new SpeciesInfos("MALC", "Myotis alcathoe", "Nymphenfledermaus", 19, "", true, 48, 65, 2, 4, 41, 47, 100, 130, 50, 90, cMalc));
      Species.Add(new SpeciesInfos("MBEC", "Myotis bechsteinii", "Bechsteinfledermaus", 28, "", true, 38, 50, 2.5, 6, 25, 40, 80, 100, 90, 110, cMbec, "dat/Myotis_bechsteinii_Rid_6059.wav"));
      Species.Add(new SpeciesInfos("MBRA", "Myotis brandtii", "Große Bartfledermaus", 23, "MMYS", true, 37, 50, 4, 7, 23, 30, 65, 100, 80, 110, cMmys, "dat/Myotis_brandtii_Ski0120_S2_From2264955ms_To2272950ms.wav"));
      Species.Add(new SpeciesInfos("MDAS", "Myotis dasycneme", "Teichfledermaus", 15, "", true, 36, 42, 5, 9, 25, 35, 65, 85, 80, 120, cMdas));
      Species.Add(new SpeciesInfos("MDAU", "Myotis daubentonii", "Wasserfledermaus", 11, "", true, 37, 55, 3, 7, 25, 40, 55, 95, 65, 95, cMdau, "dat/Mdau_Ski0111_S2_From2386127ms_To2412446ms.wav"));
      Species.Add(new SpeciesInfos("MEMA", "Myotis emarginatus", "Wimperfledermaus", 32, "", true, 48, 65, 1.5, 4, 30, 40, 90, 140, 40, 90, cMema));
      Species.Add(new SpeciesInfos("MMYO", "Myotis myotis", "Großes Mausohr", 36, "", true, 27, 37, 5, 10, 21, 26, 50, 75, 90, 160, cMmyo, "dat/Mmyo_Ski0112_S1_From0748143ms_To0763637ms.wav"));
      Species.Add(new SpeciesInfos("MMYS", "Myotis mystacinus", "Kleine Bartfledermaus", 23, "MBRA", true, 40, 57, 3, 6, 28, 35, 65, 100, 70, 90, cMmys, "dat/Myotis_mystacinus_Ski0126_S2_From0066286ms_To0091143ms.wav"));
      Species.Add(new SpeciesInfos("MNAT", "Myotis nattereri", "Fransenfledermaus", 40, "", true, 28, 53, 2, 5, 12, 25, 80, 150, 75, 110, cMnat));
      Species.Add(new SpeciesInfos("MOXY", "Myotis oxygnatus", "Kleines Mausohr", 1, "", true, 28, 40, 5, 10, 21, 26, 50, 80, 90, 160, null));
      Species.Add(new SpeciesInfos("MSCH", "Miniopterus schreibersii", "Langflügelfledermaus", 1,"", true, 49, 55, 6, 15, 49, 55, 55, 80, 65, 140, null));
      Species.Add(new SpeciesInfos("NLAS", "Nyctalus lasiopterus", "Riesenabendsegler", 1,"", true, 14.5, 23, 12, 28, 14, 20, 15, 25, 160, 820, null));
      Species.Add(new SpeciesInfos("NLEI", "Nyctalus leisleri", "Kleiner Abendsegler", 36, "", true, 21, 30, 5, 20, 22, 26, 25, 40, 200, 400, cNlei, "dat/Nlei_Ski0112_S2_From0359779ms_To0397522ms.wav"));
      Species.Add(new SpeciesInfos("NNOC", "Nyctalus noctula", "Großer Abendsegler", 32, "", true, 17, 29, 5, 28, 16, 27, 20, 40, 250, 300, cNnoc, "dat/Nyctalus_noctula_Rid_5953.wav"));
      Species.Add(new SpeciesInfos("PKUH", "Pipistrellus kuhlii", "Weißrandfledermaus", 55,"PNAT", true, 35, 42, 5, 12, 35, 42, 35, 60, 90, 130, cPnat));
      Species.Add(new SpeciesInfos("PNAT", "Pipistrellus nathusii", "Rauhautfledermaus", 55, "PKUH", true, 35, 42, 4, 11, 35, 41, 36, 70, 100, 130, cPnat, "dat/Pnat_Ski0121_S1_From1276579ms_To1294660ms.wav"));
      Species.Add(new SpeciesInfos("PPIP", "Pipistrellus pipistrellus", "Zwergfledermaus", 61, "", true, 41, 52, 3, 10, 42, 51, 44, 70, 75, 130, cPpip, "dat/Ppip_Ski0112_S1_From0107809ms_To0159033ms.wav"));
      Species.Add(new SpeciesInfos("PPYG", "Pipistrellus pygmaeus", "Mückenfledermaus", 65,"", true, 50, 64, 3, 10, 51, 56, 60, 80, 65, 95, cPpyg, "dat/Pipistrellus_pygmaeus_Rid_6162.wav"));
      Species.Add(new SpeciesInfos("PAUR", "Plecotus auritus", "Braunes Langohr", 69, "PAUS", true, 22, 40, 2, 5, 18, 23, 45, 60, 40, 120, cPaur, "dat/Plecotus_spec_Ski0126_S2_From0311629ms_To0329446ms.wav"));
      Species.Add(new SpeciesInfos("PAUS", "Plecotus austriacus", "Graues Langohr", 69, "PAUR", true, 22, 32, 2, 6, 20, 25, 40, 50, 60, 150, cPaur, "dat/Plecotus_spec_Ski0126_S2_From0311629ms_To0329446ms.wav"));
      Species.Add(new SpeciesInfos("RFER", "Rhinolophus ferrumequinum", "Große Hufeisennase", 77, "", true, 77, 86, 35, 75, 50, 78, 77, 86, 80, 120, cRfer));
      Species.Add(new SpeciesInfos("RHIP", "Rhinolophus hipposideros", "Kleine Hufeisennase", 79, "", true, 100, 116, 16, 75, 83, 100, 100, 116, 60, 100, cRhip));
      Species.Add(new SpeciesInfos("VMUR", "Vespertilio murinus", "Zweifarbfledermaus", 40, "", true, 22, 27, 10, 21, 21, 24, 30, 45, 75, 130, cVmur, "dat/Vespertilio_murinus_Ski0150_S2_From0877624ms_To0904583ms.wav"));
    }


    static private DocHelperRtf buildResults(List<CheckResult> results, double lat, double lon, bool localNames, RequestNavigateEventHandler evHandler)
    {
      DocHelperRtf doc = new  DocHelperRtf();

      // sum up test results
      foreach (CheckResult r in results)
      {
        string id = "";
        SolidColorBrush color;
        switch (r.Identifiable)
        {
          case enIdentifiable.CHARACTERISTIC:
            id = MyResources.BatInfoChar;
            color = new SolidColorBrush(Colors.LightGreen);
            break;
          case enIdentifiable.PARTLY:
            id = MyResources.BatInfoPartly;
            color = new SolidColorBrush(Colors.Yellow);
            break;
          default:
          case enIdentifiable.NO:
            color = new SolidColorBrush(Colors.Orange);
            id = MyResources.BatInfoNoIdent;
            break;
        }
        string reg = App.Model.Regions.occursAtLocation(r.Species.Abbreviation, lat, lon) ? "" : $"({MyResources.NotRegional})";
        string spec = localNames ? r.Species.Local : r.Species.Abbreviation;
        doc.addText("\n");
        doc.addText(spec, true, false, new SolidColorBrush(Colors.Black), color);
        if (r.Unambiguous)
          doc.addText($": {reg} {MyResources.Unambiguous} {r.AdditionalInfo} -- {MyResources.BatInfo_buildResults_See}: ");
        else
          doc.addText($": {reg} {id}, {r.AdditionalInfo} -- {MyResources.BatInfo_buildResults_See}: ");
        doc.addHyperlink("PDF", AppDomain.CurrentDomain.BaseDirectory + r.PdfName + $"#page={r.PageNr}", evHandler);
      }

      doc.addText("\n");
      if (results.Count == 0)
        // nothing found
        doc.addText(BatInspector.Properties.MyResources.msgNoMatchingCriteria);
      else
      {
        if (results.Count > 1)
        {
          List<string> listGenus = new List<string>();
          List<SpeciesInfos> listSpecies = new List<SpeciesInfos>();
          doc.addText("===>\n");

          // try to find similar species
          bool stop = false;
          if ((results.Count == 2) && (results[0].Species.Abbreviation == results[1].Species.NotDistinguishableFrom))
          {
            if (localNames)
              doc.addText($"{results[0].Species.Local}/{results[1].Species.Local}", true);
            else
              doc.addText($"{results[0].Species.Abbreviation}/{results[1].Species.Abbreviation}", true);
            stop = true;
          }

          // if not similar or more than two, try to reduce to genus
          if (!stop)
          {
            foreach (CheckResult r in results)
            {
              string genus = listGenus.Find(x => x == r.Species.getGenus());
              if ((genus == null) && App.Model.Regions.occursAtLocation(r.Species.Abbreviation, lat, lon))
                listGenus.Add(r.Species.getGenus());
              SpeciesInfos spec = listSpecies.Find(x => x.Abbreviation == r.Species.Abbreviation);
              if ((spec == null) && App.Model.Regions.occursAtLocation(r.Species.Abbreviation, lat, lon))
                listSpecies.Add(r.Species);
            }
            if (listSpecies.Count == 1)
            {
              if (localNames)
                doc.addText(listSpecies[0].Local, true);
              else
                doc.addText(listSpecies[0].Abbreviation, true);
            }
            else
            {
              if (listGenus.Count == 1)
                doc.addText(listGenus[0], true);
              else
              {
                if ((listGenus.Find(x => x == "Nyctalus") != null) || ((listGenus.Find(x => x == "Eptesicus") != null)))
                  doc.addText("Nyctaloid", true);
                else
                  doc.addText("?");
              }
            }
          }
        }
      }
      return doc;
    }


    static private bool checkParameter(double val, ValRange range, string name, string unit, ref string info, bool verbose)
    {
      bool retVal = false;
      if ((range == null) || (range.Min < 0))
        return true;

      if ((val >= range.NormMin) && (val <= range.NormMax))
      {
        retVal = true;
        if (verbose)
          info += $"{range.Min} {unit} < {name} < {range.Max} {unit}, ";
        else
          info += name + " OK, ";
      }
      else if ((val >= range.Min) && (val <= range.NormMin))
      {
        retVal = true;
        if (verbose)
          info += $"{range.Min} {unit} < {name} ({MyResources.BatInfoLowerLimit}) < {range.Max} {unit}, ";
        else
          info += name + $" OK ({MyResources.BatInfoLowerLimit}), ";
      }
      else if ((val >= range.NormMax) && (val <= range.Max))
      {
        retVal = true;
        if (verbose)
          info += $"{range.Min} {unit} < {name} ({MyResources.BatInfoUpperLimit}) < {range.Max} {unit} , ";
        else
          info += name + $" OK ({MyResources.BatInfoUpperLimit}), ";
      }
      return retVal;
    }


    class CheckResult
    {
      public SpeciesInfos Species { get; set; }
      public enIdentifiable Identifiable { get; set; }
      public bool Unambiguous { get; set; }
      public string AdditionalInfo { get; set; }
      public string PdfName { get;set; }
      public int PageNr { get; set; }

    }


    static public FlowDocument checkBatSpecies(CallData call, List<SpeciesInfos> species, double lat, double lon, bool verbose, bool includeUnidentifiable, bool localNames, RequestNavigateEventHandler evHandler)
    {      
      List<CheckResult> results = new List<CheckResult>();
      foreach (SpeciesInfos s in species)
      {
        if (s.CheckData != null)
        {
          string addInfo = "";
          foreach (CheckData check in s.CheckData)
          {
            bool ok = true;
            if (!includeUnidentifiable && check.Identifiable == enIdentifiable.NO)
              continue;

            if (check.CallCharacteristic != call.CallCharacteristic)
              continue;

            string info = call.CallCharacteristic.ToString() + ": ";
            ok &= checkParameter(call.FreqChar, check.FreqChar, "Fc", "kHz", ref info, verbose);
            ok &= checkParameter(call.FreqStart, check.FreqStart, "Fstart", "kHz", ref info, verbose);
            ok &= checkParameter(call.FreqEnd, check.FreqEnd, "Fend", "kHz",ref info, verbose);
            ok &= checkParameter(call.FreqMk, check.FreqMk, "Fmk","kHz", ref info, verbose);
            ok &= checkParameter(call.Duration, check.Duration, "D", "ms", ref info, verbose);
            if (ok)
            {
              CheckResult res = new CheckResult();
              res.Species = s;
              res.Identifiable = check.Identifiable;
              res.Unambiguous = checkUniqueness(s.Abbreviation, call, ref addInfo);
              addInfo += " " + info;
              res.AdditionalInfo = addInfo;
              res.PdfName = s.getGenus() == "Myotis" ? AppParams.BAT_INFO2_PDF : AppParams.BAT_INFO1_PDF;
              res.PageNr = s.PageNr;
              results.Add(res);
              break;
            }
          }
        }
      }
      DocHelperRtf doc = buildResults(results, lat, lon, localNames, evHandler);
      return doc.Doc;
    }

    /*
    int checkBatSpecies(AnalysisFile analysis, string fileName, int callIdx)
    {

      int retVal = -1;
      / *
        SoundEdit file = new SoundEdit(analysis.getInt(Cols.SAMPLERATE));
        WavFile wav = new WavFile();
        int iStart = (int)(App.Model.ZoomView.RulerDataT.Min * App.Model.ZoomView.Waterfall.SamplingRate);
        int iEnd = (int)(App.Model.ZoomView.RulerDataT.Max * App.Model.ZoomView.Waterfall.SamplingRate);
        wav.createFile(1, App.Model.ZoomView.Waterfall.SamplingRate, iStart, iEnd, App.Model.ZoomView.Waterfall.Audio.Samples);
        wav.saveFileAs(dlg.FileName); file.
        file.readFile(fileName);
        if ((callIdx >= 0) && (callIdx < analysis.Calls.Count))
        {
          double tStart = analysis.Calls[callIdx].getDouble(Cols.START_TIME);
          double tEnd = tStart + analysis.Calls[callIdx].getDouble(Cols.DURATION) / / 1000.0;

        }
        else
          retVal = -2;


       * /
      return retVal;
    }
    */

    static bool checkUniqueness(string spec, CallData d, ref string addInfo)
    {
      bool retVal = false;
      switch (spec)
      {
        case "BBAR":
          retVal = (d.HasCallTypeAandB == enYesNoProperty.YES);
          if (retVal)
            addInfo += "{call A & B}";
          break;

        case "ENIL":
          {
            bool ok1 = (
                       (d.CallCharacteristic == enCallChar.QCF) &&
                       (d.FreqEnd > 27.0) &&
                       (d.HasNoCallChanges == enYesNoProperty.YES)
                     );
            if (ok1)
              addInfo += "{QCF & (Fend < 27 kHz) & no call change";
            bool ok2 = (
                       (d.CallCharacteristic == enCallChar.FM_QCF) &&
                       (d.FreqEnd >= 30.0)
                     );
            if (ok2)
              addInfo += "{ FM_QCF & Fend > 30 kHz}";
            retVal = ok1 || ok2;
          }
          break;

        case "ESER":
          {
            bool ok1 = (
                       (d.CallCharacteristic == enCallChar.QCF) &&
                       (d.CallInterval >= 200.0) && (d.CallInterval <= 400.0) &&
                       (d.IsUniformFormFreqInt == enYesNoProperty.YES) &&
                       (d.FreqEnd >= 21.0) && (d.FreqEnd <= 25.0)
                     );
            if (ok1)
              addInfo += "{QCF & (200 ms < callI < 400 ms) & uniform calls & (21 kHz <= Fend <= 26 kHz)}";
            bool ok2 = (
                       (d.CallCharacteristic == enCallChar.FM_QCF) &&
                       (d.CallInterval >= 100.0) && (d.CallInterval <= 300.0) &&
                       (d.FreqEnd <= 26.0) &&
                       (d.HasUpwardHookAtEnd == enYesNoProperty.YES)
                     );
            if (ok2)
              addInfo += "{FM_QCF & (100 ms < callI < 300 ms) & Fend <=26 kHz & upwardHook}";
            retVal = ok1 || ok2;
          }
          break;

        case "HSAV":
          {
            bool ok1 = (
                       (d.CallCharacteristic == enCallChar.QCF) &&
                       (d.FreqChar <= 34.0)
                     );
            if (ok1)
              addInfo += "{QCF & Fc <= 34 kHz}";
            bool ok2 = (
                       (d.CallCharacteristic == enCallChar.FM_QCF) &&
                       (d.FreqChar <= 35.0)
                     );
            if (ok2)
              addInfo += "{FM_QCF & Fc <= 35 kHz}";
            retVal = ok1 || ok2;
          }
          break;

        case "MBEC":
          {
            bool ok1 = (
                       (d.Duration >= 1.5) && (d.Duration <= 3.5) &&
                       (d.FreqStart > 140.0) &&
                       (d.HasMyotisKink == enYesNoProperty.NO) &&
                       (d.FreqEnd <= 36.0)
                     );
            if (ok1)
              addInfo += "{(1.5 ms < D < 3.5 ms) & Fstart > 140 kHz & no mk & Fend <= 36 kHz}";
            bool ok2 = (
                       (d.Duration > 3.5) && (d.Duration <= 6) &&
                       (d.FreqStart > 122.0) && (d.FreqStart < 140.0)
                     );
            if (ok2)
              addInfo += "{(3.5 ms < D <= 6 ms) & (122 kHz < Fstart < 140 kHz}";
            retVal = ok1 || ok2;
          }
          break;

        case "MBRA":
        case "MMYS":
          {
            bool ok1 = (
                       (d.Duration >= 1.5) && (d.Duration <= 3.5) &&
                       (d.HasMyotisKink == enYesNoProperty.YES) &&
                       (d.FreqMk >= 36.0) && (d.FreqMk <= 44) &&
                       ((d.FreqStart > 100.0) || ((d.FreqStart - d.FreqEnd) > 75.0)) &&
                       (d.HasKneeClearly == enYesNoProperty.YES)
                     );
            if (ok1)
              addInfo += "{(1.5 ms < D < 3.5 ms) & has mk & (36 kHz <= Fmk <= 44 kHz) & (bw > 75 kHz)}";
            bool ok2 = (
                       (d.Duration > 3.5) && (d.Duration <= 6.0) &&
                       (d.HasMyotisKink == enYesNoProperty.YES) &&
                       (d.FreqMk >= 37.0)
                     );
            if (ok2)
              addInfo += "{(D > 3.5 ms) & has mk & (Fmk >= 37 kHz)}";
            retVal = ok1 || ok2;
          }
          break;

        case "MALC":
          retVal = (d.FreqStart < 123.0) && (d.FreqMk > 45) && (d.FreqEnd > 40.0);
          if (retVal)
            addInfo += "{Fstart < 123 kHz & Fmk > 45 kHz & Fend > 40 kHz}";
          break;

        case "MDAS":
          {
            bool ok1 = (
                       (d.Duration <= 3.5) &&
                       (d.HasMyotisKink == enYesNoProperty.YES) &&
                       (d.FreqMk < 35.0) &&
                       ((d.FreqStart - d.FreqEnd) <= 65.0) || (d.FreqStart < 90.0)
                     );
            if (ok1)
              addInfo += "{calld < 3.5 ms & has mk & Fmk < 35 kHz & bw <=65 kHz}";
            bool ok2 = (
                       (d.Duration > 3.5) && (d.Duration < 6.0) &&
                       (d.HasMyotisKink == enYesNoProperty.YES) &&
                       (d.FreqMk < 33.0) && (d.FreqStart < 103.0)
                     );
            if (ok2)
              addInfo += "{calld > 3.5 ms & has mk & Fmk < 33 kHz & Fstart < 103.0 kHz}";
            retVal = ok1 || ok2;
          }
          break;

        case "MDAU":
          retVal = (d.HasMyotisKink == enYesNoProperty.YES) &&
                   (d.FreqMk > 36.0) &&
                   ((d.FreqStart - d.FreqEnd) <= 66.0) || (d.FreqStart < 90.0);
          if (retVal)
            addInfo += "{has mk & bw <= 66 kHz & Fstart < 90 kHz}";
          break;

        case "MEMA":
          retVal =
          (
            (d.HasMyotisKink == enYesNoProperty.NO) &&
            (d.Duration < 6.0) &&
            (d.FreqStart > 150.0)
          );
          if (retVal)
            addInfo += "{has mk & D < 6 ms & Fstart > 150 kHz}";
          /*  TODO  ||
          (
            ((d.HasMyotisKink == enYesNoProperty.YES) || (d.HasKneeClearly == enYesNoProperty.YES)) &&
            (

          )*/

          break;

        case "MMYO":
          {
            bool ok1 = (
              (d.HasMyotisKink == enYesNoProperty.NO) &&
              (d.Duration >= 2.5) && (d.Duration <= 3.5) &&
              (d.FreqStart < 120.0) && (d.FreqEnd > 23.0)
            );
            if (ok1)
              addInfo += "{has mk & (2.5 ms < D < 3.5 ms) & Fstart < 120 kHz & Fend > 23 kHz}";
            bool ok2 = (
              (d.IsConvex == enYesNoProperty.YES) &&
              (d.Duration > 3.5) && (d.Duration <= 6.0)
            );
            if (ok2)
              addInfo += "{is convex & (3.5 ms < D < 6 ms)}";
            bool ok3 = (
              (d.IsConvex == enYesNoProperty.NO) &&
              (d.Duration > 3.5) && (d.Duration <= 6.0) &&
              (d.FreqStart > 120.0) &&
              (d.FreqEnd < 16.0)
            );
            if (ok3)
              addInfo += "{not convex & (3.5 ms < D < 6 ms) & Fstart > 120 kHz & Fend < 16 kHz}";
            bool ok4 =
            (
              (d.Duration > 6.0) && (d.Duration <= 8.0) &&
              (
                (d.FreqStart > 120.0) ||
                (d.FreqEnd < 17.0)
              )
            );
            if (ok4)
              addInfo += "{(6 ms < D < 8 ms) & Fstart > 120 kHz & Fend > 17 kHz}";
            retVal = ok1 || ok2 || ok3 || ok4;
          }
          break;

        case "MNAT":
          retVal =
          (
            (d.HasMyotisKink == enYesNoProperty.NO) &&
            (d.FreqStart > 120.0) && (d.FreqEnd <= 20.0)
          );
          if (retVal)
            addInfo += "{has mk & Fstart > 120 kHz & Fend < 20 kHz}";
          break;

        case "NLEI":
          retVal = (d.FreqChar > 23.0) && (d.CallCharacteristic == enCallChar.QCF);
          if (retVal)
            addInfo += "{QCF & Fc > 23 kHz}";
          break;

        case "NNOC":
          retVal = d.FreqChar < 21.0;
          if (retVal)
            addInfo += "{Fc < 21 kHz}";
          break;

        case "PAUS":
        case "PAUR":
          retVal = (d.CallCharacteristic == enCallChar.FM) && (d.IsUniformFormFreqInt == enYesNoProperty.YES);
          if (retVal)
            addInfo += "{FM & uniform calls}";
          break;

        case "PNAT":
        case "PKUH":
          {
            bool ok1 =
            (
              (d.CallCharacteristic == enCallChar.QCF) &&
              (d.FreqChar > 36.0) && (d.FreqChar <= 40.0)
            );
            if (ok1)
              addInfo += "{QCF & (36 kHz < Fc <= 40 kHz)}";
            bool ok2 = (
              (d.CallCharacteristic == enCallChar.FM_QCF) &&
              (d.Duration >= 7.0) &&
              (d.FreqChar >= 37.0) && (d.FreqChar <= 40.0)
            );
            if (ok2)
              addInfo += "{FM_QCF & D > 7 ms & (37 kHz <= Fc <= 40 kHz}";
            retVal = ok1 || ok2;
          }
          break;

        case "PPIP":
          {
            bool ok1 =
            (
            (d.CallCharacteristic == enCallChar.QCF) &&
            (d.FreqChar > 42.0) && (d.FreqChar <= 50.0)
            );
            if (ok1)
              addInfo += "{QCF & (42 kHz < Fc <= 50 kHz}";
            bool ok2 = (
              (d.CallCharacteristic == enCallChar.FM_QCF) &&
              (d.Duration >= 4.0) &&
              (d.FreqChar >= 43.0) && (d.FreqChar < 50.0)
            );
            if (ok2)
              addInfo += "{FM_QCF & (43 kHz < Fc <= 50 kHz}";
            retVal = ok1 || ok2;
          }
          break;

        case "PPYG":
          {
            bool ok1 =
          (
            (d.CallCharacteristic == enCallChar.QCF) &&
            (d.FreqChar > 51.0)
          );
            if (ok1)
              addInfo += "{QCF & Fc > 51 kHz}";
            bool ok2 = (
              (d.CallCharacteristic == enCallChar.FM_QCF) &&
            (d.Duration < 4.0) &&
            (d.FreqChar > 55.0)
          );
            if (ok2)
              addInfo += "{FM_QCF & D < 4 ms & Fc > 55 kHz}";
            bool ok3 = (
              (d.CallCharacteristic == enCallChar.FM_QCF) &&
              (d.Duration >= 4.0) &&
              (d.FreqChar > 53.0)
            );
            if (ok3)
              addInfo += "{FM_QCF & D > 4 ms & Fc > 53 kHz}";
            retVal = ok1 || ok2 || ok3;
          }
          break;

        case "VMUR":
          retVal = false;
          break;
      }
      return retVal;
    }
  }
}
