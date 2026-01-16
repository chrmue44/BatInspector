using libParser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Documents;
using System.Xml.Serialization;

namespace BatInspector
{


  public class ElekonBra
  {
    static XmlSerializer _serializer = null;

    public static XmlSerializer getXmlSerializer()
    {
      if (_serializer == null)
        _serializer = new XmlSerializer(typeof(BatRecordBra));
      return _serializer;
    }

    public static BatRecordBra read(string braName)
    {
      BatRecordBra retVal;
      if (File.Exists(braName))
      {
        try
        {
          string xml = File.ReadAllText(braName);
          xml = xml.Replace("BatRecord", "BatRecordBra");
          TextReader reader = new StringReader(xml);
          retVal = (BatRecordBra)getXmlSerializer().Deserialize(reader);
        }
        catch (Exception ex)
        {
          DebugLog.log("error reading info file '" + braName + "': " + ex.ToString(), enLogType.ERROR);
          retVal = new BatRecordBra();
        }
      }
      else
        retVal = new BatRecordBra();
      return retVal;
    }

    public static Bd2AnnFile convertToBd2Ann(string braName, double timeOffs, double fMinOffs, string speciesName, string eventType)
    {
      Bd2AnnFile ann = new Bd2AnnFile();
      BatRecordBra rec = read(braName);
      if (rec != null)
      {
        string wavName = braName.Replace(".bra", ".wav");

        WavFile wav = new WavFile();
        wav.readFile(wavName);

        ann.annotated = true;
        ann.duration = (double)wav.AudioSamples.Length / wav.FormatChunk.Frequency;
        ann.id = Path.GetFileName(wavName);
        ann.notes = "";
        ann.time_exp = 1;

        List<Bd2Annatation> annCalls = new List<Bd2Annatation>();
        if (rec.Call != null)
        {
          foreach (BatRecordCall c in rec.Call)
          {
            Bd2Annatation a = new Bd2Annatation();
            a.Class = speciesName;
            a.Event = eventType;
            a.end_time = c.End + timeOffs;
            a.start_time = c.Begin + timeOffs;
            a.low_freq = c.MinFreq + fMinOffs;
            a.high_freq = c.MaxFreq;
            a.individual = "0";
            annCalls.Add(a);
          }
        }
        ann.Annatations = annCalls.ToArray();
      }
      return ann;
    }
  }
}
