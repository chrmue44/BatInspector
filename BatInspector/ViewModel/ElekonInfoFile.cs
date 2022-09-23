using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace BatInspector
{
  public class ElekonInfoFile
  {
    public static BatRecord read(string infoName)
    {
      BatRecord retVal = null;
      if (File.Exists(infoName))
      {
        string xml = File.ReadAllText(infoName);
        var serializer = new XmlSerializer(typeof(BatRecord));
        TextReader reader = new StringReader(xml);
        retVal = (BatRecord)serializer.Deserialize(reader);
      }
      else
        retVal = new BatRecord();
      initUninitializedValues(ref retVal);
      return retVal;
    }

    public static void parsePosition(BatRecord rec, out double lat, out double lon)
    {
      string[] pos = rec.GPS.Position.Split(' ');
      lat = 0.0;
      lon = 0.0;
      if (pos.Length == 2)
      {
        double.TryParse(pos[0], NumberStyles.Any, CultureInfo.InvariantCulture, out lat);
        double.TryParse(pos[1], NumberStyles.Any, CultureInfo.InvariantCulture, out lon);
      }
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
