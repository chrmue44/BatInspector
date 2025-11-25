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
      PrjMetaData.initUninitializedValues(ref retVal);
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

  }
}
