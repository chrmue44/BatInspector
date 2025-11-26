using libParser;
using NAudio.Wave;
using Org.BouncyCastle.Tls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatInspector
{
  
  public enum enMetaData
  {
    XML = 1,
    GUANO = 2,
  }

  public class PrjMetaData
  {
    public PrjMetaData() { }
    public BatRecord MetaData { get { return getMetaData(); } }

    public static void parsePosition(string gpsPos, out double lat, out double lon)
    {
      string[] pos = gpsPos.Split(' ');
      lat = 0.0;
      lon = 0.0;
      if (pos.Length == 2)
      {
        double.TryParse(pos[0], NumberStyles.Any, CultureInfo.InvariantCulture, out lat);
        double.TryParse(pos[1], NumberStyles.Any, CultureInfo.InvariantCulture, out lon);
      }
    }

    public static BatRecord retrieveMetaData(Project prj, string wavName)
    {
      string infoFileName = wavName.ToLower().Replace(AppParams.EXT_WAV, AppParams.EXT_INFO);
      infoFileName = Path.Combine(prj.PrjDir, prj.WavSubDir, infoFileName);
      BatRecord r = ElekonInfoFile.read(infoFileName);
      return r;
    }

    public static BatRecord retrieveMetaData(string fullWavPath)
    {
      string infoFileName = fullWavPath.ToLower().Replace(AppParams.EXT_WAV, AppParams.EXT_INFO);
      BatRecord r = ElekonInfoFile.read(infoFileName);
      return r;
    }

    public static BatRecord retrieveMetaData(string path, string wavName)
    {
      string infoFileName = wavName.ToLower().Replace(AppParams.EXT_WAV, AppParams.EXT_INFO);
      infoFileName = Path.Combine(path, infoFileName);
      BatRecord r = ElekonInfoFile.read(infoFileName);
      return r;
    }

    public static void createMetaData(string wavName, BatRecord rec, enMetaData metaData, int timeExFactor)
    {
      switch (metaData)
      {
        case enMetaData.GUANO:
          WavFile wav = new WavFile();
          wav.readFile(wavName);
          wav.addGuanoMetaData(rec, timeExFactor);
          wav.saveFile();
          break;
      }
    }


    public static void parsePosition(BatRecord rec, out double lat, out double lon)
    {
      parsePosition(rec.GPS.Position, out lat, out lon);
    }

    public static string formatPosition(string position, int decimals)
    {
      parsePosition(position, out double lat, out double lon);
      string fmt = "N" + decimals.ToString();
      string retVal = lat.ToString(fmt, CultureInfo.InvariantCulture) + " " +
                      lon.ToString(fmt, CultureInfo.InvariantCulture);
      return retVal;
    }

    public static bool checkDateTimeInFileName(string fileName)
    {
      DateTime? res = getDateTimeFromFileNameInternal(fileName);
      return (res != null);
    }

    public static DateTime getDateTimeFromFileName(string fileName)
    {
      DateTime? res = getDateTimeFromFileNameInternal(fileName);
      if (res != null)
        return (DateTime)res;
      else
        return System.IO.File.GetLastWriteTime(fileName);
    }

    public static void getPositionFromMetaData(string wavName, out double lat, out double lon)
    {
      string fName = wavName.ToLower().Replace(AppParams.EXT_WAV, AppParams.EXT_INFO);
      BatRecord info = ElekonInfoFile.read(fName);
      PrjMetaData.parsePosition(info, out lat, out lon);
    }

    public static void createSplitXmls(string fName, string[] newNames, double maxLen)
    {
      double deltaT = 0;
      DateTime t = new DateTime(1900, 1, 1);
      BatRecord rec = PrjMetaData.retrieveMetaData(fName);
      foreach (string newName in newNames)
      {
        string xmlName = newName.ToLower().Replace(AppParams.EXT_WAV, AppParams.EXT_INFO);
        if (rec != null)
        {
          if (t.Year < 1910)
            t = AnyType.getDate(rec.DateTime);
          t = t.AddMilliseconds(deltaT * 1000);
          deltaT += maxLen;
          rec.DateTime = PrjMetaData.getDateString(t);
          ElekonInfoFile.write(xmlName, rec);
        }
      }
    }

    private static DateTime? getDateTimeFromFileNameInternal(string fileName)
    {
      DateTime? retVal = null;

      string baseName = Path.GetFileNameWithoutExtension(fileName);
      char[] splitter = new char[] { '_', '-' };
      string[] token = baseName.Split(splitter);
      if (token.Length > 1)
      {
        string dateStr = "";
        string timeStr = "";
        foreach (string t in token)
        {
          string str = Utils.removeNonNumerics(t);
          if (str.Length == 8)
          {
            try
            {
              DateTime tim = DateTime.ParseExact(str, "yyyyMMdd", CultureInfo.InvariantCulture);
              dateStr = str;
            }
            catch { }
          }
          if (str.Length == 6)
          {
            try
            {
              DateTime tim = DateTime.ParseExact(str, "HHmmss", CultureInfo.InvariantCulture);
              timeStr = str;
            }
            catch { }
          }
        }
        if ((dateStr.Length > 1) && (timeStr.Length > 1))
        {
          dateStr += " " + timeStr;
          retVal = DateTime.ParseExact(dateStr, "yyyyMMdd HHmmss", CultureInfo.InvariantCulture);
        }
      }
      return retVal;
    }


    private static bool readGpxFile(PrjInfo info, out gpx gpxFile)
    {
      bool retVal = true;
      gpxFile = null;
      // read gpx file if needed
      if (info.OverwriteLocation)
      {
        if (info.LocSourceGpx)
        {
          gpxFile = gpx.read(info.GpxFile);
          if (gpxFile == null)
          {
            DebugLog.log("gpx file not readable: " + info.GpxFile, enLogType.ERROR);
            retVal = false;
          }
        }
      }
      return retVal;
    }

    private static bool readKmlFile(PrjInfo info, out kml kmlFile)
    {
      bool retVal = true;
      kmlFile = null;
      // read gpx file if needed
      if (info.OverwriteLocation)
      {
        if (info.LocSourceKml)
        {
          kmlFile = kml.read(info.GpxFile);
          if (kmlFile == null)
          {
            DebugLog.log("kml file not readable: " + info.GpxFile, enLogType.ERROR);
            retVal = false;
          }
        }
      }
      return retVal;
    }

    private static bool readLoctxtFile(PrjInfo info, LocFileSettings pars, out LocFileTxt txtFile)
    {
      bool retVal = true;
      txtFile = null;
      // read gpx file if needed
      if (info.OverwriteLocation)
      {
        if (info.LocSourceTxt)
        {
          txtFile = LocFileTxt.read(info.GpxFile, pars);
          if (txtFile == null)
          {
            DebugLog.log("txt location file not readable: " + info.GpxFile, enLogType.ERROR);
            retVal = false;
          }
        }
      }
      return retVal;
    }

    /// <summary>
    /// replace locations in xml files with locations from a gpx file. Assignments of location to the XML file is via time stamp
    /// </summary>
    /// <param name="xmlfiles">list if xml files</param>
    /// <param name="wavDir">directory containing the xml files</param>
    /// <param name="gpxFile">name of the gpx file with location information</param>
    private static void replaceLocationsInXmls(string[] xmlfiles, string wavDir, gpx gpxFile)
    {
      DebugLog.log("replace locations from gpx file...", enLogType.INFO);
      foreach (string fName in xmlfiles)
      {
        BatRecord f = ElekonInfoFile.read(fName);
        DateTime t = PrjMetaData.getDateTimeFromFileName(fName);
        double[] pos = gpxFile.getPosition(t);
        if ((pos == null) || (pos.Length < 2) || ((pos[0] == 0.0) && (pos[1] == 0.0)))
          DebugLog.log("no position found for " + fName + ", timestamp: " + t.ToString(), enLogType.ERROR);
        f.GPS.Position = pos[0].ToString(CultureInfo.InvariantCulture) + " " + pos[1].ToString(CultureInfo.InvariantCulture);
        string dstName = Path.GetFileName(fName);
        dstName = Path.Combine(wavDir, dstName);
        ElekonInfoFile.write(dstName, f);
      }
    }

    /// <summary>
    /// replace locations in xml files with locations from a text file. Assignments of location to the XML file is via time stamp
    /// </summary>
    /// <param name="xmlfiles">list if xml files</param>
    /// <param name="wavDir">directory containing the xml files</param>
    /// <param name="kmlFile">name of the kml file with location information</param>
    private static void replaceLocationsInXmls(string[] xmlfiles, string wavDir, kml kmlFile)
    {
      DebugLog.log("replace locations from kml file...", enLogType.INFO);
      foreach (string fName in xmlfiles)
      {
        BatRecord f = ElekonInfoFile.read(fName);
        double[] posOld = { 90, 0 };
        double[] pos = kmlFile.getPosition(fName);
        if (pos[0] < 1e-6)
          pos = posOld;
        else
          posOld = pos;
        f.GPS.Position = pos[0].ToString(CultureInfo.InvariantCulture) + " " + pos[1].ToString(CultureInfo.InvariantCulture);
        string dstName = Path.GetFileName(fName);
        dstName = Path.Combine(wavDir, dstName);
        ElekonInfoFile.write(dstName, f);
      }
    }

    /// <summary>
    /// replace locations in xml files with locations from a text file. Assignments of location to the XML file is via time stamp
    /// </summary>
    /// <param name="xmlfiles">list if xml files</param>
    /// <param name="wavDir">directory containing the xml files</param>
    /// <param name="txtFile">name of the text file with location information</param>
    private static void replaceLocationsInXmls(string[] xmlfiles, string wavDir, LocFileTxt txtFile)
    {
      DebugLog.log("replace locations from txt file...", enLogType.INFO);
      foreach (string fName in xmlfiles)
      {
        BatRecord f = ElekonInfoFile.read(fName);
        double[] posOld = { 90, 0 };
        DateTime t = AnyType.getDate(f.DateTime);
        double[] pos = txtFile.getPosition(fName, t);
        if (pos[0] < 1e-6)
          pos = posOld;
        else
          posOld = pos;
        f.GPS.Position = pos[0].ToString(CultureInfo.InvariantCulture) + " " + pos[1].ToString(CultureInfo.InvariantCulture);
        string dstName = Path.GetFileName(fName);
        dstName = Path.Combine(wavDir, dstName);
        ElekonInfoFile.write(dstName, f);
      }
    }

    /// <summary>
    /// replace location in XML files with a fixed location
    /// </summary>
    /// <param name="xmlfiles">list of xml files</param>
    /// <param name="wavDir">directory containing the xml files</param>
    /// <param name="lat">new latitude</param>
    /// <param name="lon">new longitude</param>
    private static void replaceLocationsInXmls(string[] xmlfiles, string wavDir, double lat, double lon)
    {
      DebugLog.log("replace locations with fix location...", enLogType.INFO);
      foreach (string fName in xmlfiles)
      {
        BatRecord f = ElekonInfoFile.read(fName);
        f.GPS.Position = lat.ToString(CultureInfo.InvariantCulture) + " " + lon.ToString(CultureInfo.InvariantCulture);
        string dstName = Path.GetFileName(fName);
        dstName = Path.Combine(wavDir, dstName);
        ElekonInfoFile.write(dstName, f);
      }
    }


    /// <summary>
    /// replace locations in all files in a given directory depending on the settings in the info struct
    /// </summary>
    /// <param name="info"></param>
    /// <param name="wavDir">full path of directory containing wavs and/or xmls</param>
    /// <param name="t"></param>
    public static void replaceLocations(PrjInfo info, string wavDir, Stopwatch t)
    {
      // replace gpx locations
      string[] xmlFiles = Directory.GetFiles(wavDir, "*.xml");
      if (info.OverwriteLocation && info.LocSourceGpx)
      {
        DebugLog.log($"replace locations from gpx file... at {t.Elapsed}", enLogType.INFO);
        bool ok = readGpxFile(info, out gpx gpxFile);
        if (ok)
          replaceLocationsInXmls(xmlFiles, wavDir, gpxFile);
        else
          DebugLog.log("error reading GPX file, could not generate location information", enLogType.ERROR);
      }
      else if (info.OverwriteLocation && info.LocSourceKml)
      {
        DebugLog.log($"replace locations from kml file... at {t.Elapsed}", enLogType.INFO);
        bool ok = readKmlFile(info, out kml kmlFile);
        if (ok)
          replaceLocationsInXmls(xmlFiles, wavDir, kmlFile);
        else
          DebugLog.log("error reading KML file, could not generate location information", enLogType.ERROR);
      }
      else if (info.OverwriteLocation && info.LocSourceTxt)
      {
        DebugLog.log($"replace locations from txt file... at {t.Elapsed}", enLogType.INFO);
        bool ok = readLoctxtFile(info, AppParams.Inst.LocFileSettings, out LocFileTxt txtFile);
        if (ok)
          replaceLocationsInXmls(xmlFiles, wavDir, txtFile);
        else
          DebugLog.log("error reading TXT file, could not generate location information", enLogType.ERROR);
      }
      else if (info.OverwriteLocation)
      {
        DebugLog.log($"replace locations with fixed location... at {t.Elapsed}", enLogType.INFO);
        replaceLocationsInXmls(xmlFiles, wavDir, info.Latitude, info.Longitude);
      }
    }

    public static string getDateString(DateTime date)
    {
      if (date == null)
      {
        date = new DateTime();
        DebugLog.log("erroneous time", enLogType.ERROR);
      }
      return AnyType.getTimeString(date).Replace("0d", "");
    }


    public static DateTime parseDate(string datStr)
    {
      DateTime retVal = new DateTime();
      string[] str = datStr.Split(' ');
      if (str.Length == 1)
        str = datStr.Split('T');

      if (str.Length == 2)
      {
        string[] date = str[0].Split('.');
        if (date.Length == 3)
        {
          string[] tim = str[1].Split(':');
          if (tim.Length == 3)
          {
            bool ok = int.TryParse(date[0], out int dd);
            ok &= int.TryParse(date[1], out int mm);
            ok &= int.TryParse(date[2], out int yyyy);
            ok &= int.TryParse(tim[0], out int hh);
            ok &= int.TryParse(tim[1], out int min);
            if (ok)
              retVal = new DateTime(yyyy, mm, dd, hh, min, 0);
          }
        }
      }
      return retVal;
    }


    //TODO: very ugly: find a better way to do this
    public static void initUninitializedValues(ref BatRecord rec)
    {
      if (rec.DateTime == null)
        rec.DateTime = "";
      if (rec.Duration == null)
        rec.Duration = "";
      if (rec.FileName == null)
        rec.FileName = "";
      if (rec.Gain == null)
        rec.Gain = "";
      if (rec.GPS == null)
        rec.GPS = new BatRecordGPS();
      if (rec.GPS.Position == null)
        rec.GPS.Position = "";
      if (rec.InputFilter == null)
        rec.InputFilter = "";
      if (rec.PeakValue == null)
        rec.PeakValue = "";
      if (rec.Samplerate == null)
        rec.Samplerate = "";
      if (rec.Trigger == null)
        rec.Trigger = new BatRecordTrigger();
      if (rec.Trigger.Filter == null)
        rec.Trigger.Filter = "";
      if (rec.Trigger.Frequency == null)
        rec.Trigger.Frequency = "";
      if (rec.Trigger.Level == null)
        rec.Trigger.Level = "";
      if (rec.Temparature == null)
        rec.Temparature = "";
      if (rec.Humidity == null)
        rec.Humidity = "";
    }



    private BatRecord getMetaData()
    {
      BatRecord record = new BatRecord();
      return record;
    }
  }
}
