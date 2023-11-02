/********************************************************************************
 *               Author: Christian Müller
 *     Date of creation: 2023-09-01                                       
 *   Copyright (C) 2023: Christian Müller chrmue44(at)gmail(dot).com
 *
 *              Licence:  CC BY-NC 4.0 
 ********************************************************************************/

using libParser;
using System;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Xml.Serialization;

namespace BatInspector
{
  public class ElekonInfoFile
  {
    public const string DATE_FORMAT = "yyyy-MM-ddTHH:mm:ss";
    public static BatRecord read(string infoName)
    {
      BatRecord retVal;
      if (File.Exists(infoName))
      {
        try
        {
          string xml = File.ReadAllText(infoName);
          var serializer = new XmlSerializer(typeof(BatRecord));
          TextReader reader = new StringReader(xml);
          retVal = (BatRecord)serializer.Deserialize(reader);
        }
        catch (Exception ex) 
        {
          DebugLog.log("error reading info file '" + infoName +"': " + ex.ToString(), enLogType.ERROR);
          retVal = new BatRecord();
        }
      }
      else
        retVal = new BatRecord();
      initUninitializedValues(ref retVal);
      return retVal;
    }

    public static void write(string infoName, BatRecord record)
    {
      var serializer = new XmlSerializer(typeof(BatRecord));
      TextWriter writer = new StringWriter();
      serializer.Serialize(writer, record);
      File.WriteAllText(infoName, writer.ToString());
    }

    public static void create(string fileName, double lat, double lon, DateTime time)
    {
      BatRecord batRecord = new BatRecord();
      batRecord.FileName = Path.GetFileName(fileName);
      // string position = Utils.latToString(lat) + " " + Utils.lonToString(lon);
      string position = lat.ToString(CultureInfo.InvariantCulture) + " " + lon.ToString(CultureInfo.InvariantCulture);
      batRecord.GPS.Position = position;
      WavFile wavFile = new WavFile();
      wavFile.readFile(fileName);
      if ((wavFile.FormatChunk != null) && (wavFile.AudioSamples != null))
      {
        batRecord.Samplerate = wavFile.FormatChunk.Frequency.ToString() + " Hz";
        double duration = (double)wavFile.AudioSamples.Length / wavFile.FormatChunk.Frequency;
        batRecord.Duration = duration.ToString(CultureInfo.InvariantCulture) + " Sec";
        batRecord.DateTime = time.ToString(DATE_FORMAT);
        string infoName = fileName.ToLower().Replace(AppParams.EXT_WAV, AppParams.EXT_INFO);
        ElekonInfoFile.write(infoName, batRecord);
      }
      else
        DebugLog.log("error creating XML file for: " + fileName, enLogType.ERROR);        
    }

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

    public static DateTime getDateTimeFromFileName(string fileName)
    {
      DateTime creationTime = System.IO.File.GetLastWriteTime(fileName);

      string baseName = Path.GetFileNameWithoutExtension(fileName);
      string[] token = baseName.Split('_');
      if(token.Length > 1)
      {
        string dateStr = "";
        string timeStr = "";
        foreach (string t in token)
        {

          string str = Utils.removeNonNumerics(t);
          if(str.Length == 8)
          {
            try
            {
              DateTime tim = DateTime.ParseExact(str, "yyyyMMdd", CultureInfo.InvariantCulture);
              dateStr = str;
            }
            catch { }
          }
          if(str.Length == 6)
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
          creationTime = DateTime.ParseExact(dateStr, "yyyyMMdd HHmmss", CultureInfo.InvariantCulture);
        }
      }
      return creationTime;
    }

    public static string getDateString(DateTime date)
    {
      if (date == null)
      {
        date = new DateTime();
        DebugLog.log("erroneous time", enLogType.ERROR);
      }
      return AnyType.getTimeString(date).Replace("0d","");
    }


    public static DateTime parseDate(string datStr)
    {
      DateTime retVal = new DateTime();
      string[] str = datStr.Split(' ');
      if( str.Length == 1 )
         str = datStr.Split('T');

      if (str.Length == 2)
      {
        string[] date = str[0].Split('.');
        if (date.Length == 3)
        {
          string[] tim = str[1].Split(':');
          if(tim.Length == 3)
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
    private static void initUninitializedValues(ref BatRecord rec)
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
    }
  }
}
