/********************************************************************************
 *               Author: Christian Müller
 *     Date of creation: 2023-04-17                                       
 *   Copyright (C) 2023: Christian Müller chrmue44(at)gmail(dot).com
 *
 *              Licence:  CC BY-NC 4.0 
 ********************************************************************************/

using libParser;
using System;
using System.IO;
using System.Xml.Serialization;
using BatInspector;
using System.Globalization;
using System.Xml;
using System.Runtime.InteropServices.WindowsRuntime;


public partial class gpx
{
  public double[] getPosition(DateTime t)
  {
    double[] retVal = new double[2];
    retVal[0] = 0.0;
    retVal[1] = 0.0;
    for (int i = 0; i < this.trk.trkseg.Length; i++)
    {
      if (i < (trk.trkseg.Length - 1))
      {
        DateTime t1 = getTime(i).ToUniversalTime();
        DateTime t2 = getTime(i + 1).ToUniversalTime();
        if ((t1 <= t) && (t <= t2))
        {
          TimeSpan dt1 = t - t1;
          TimeSpan dt2 = t2 - t;
          double f = dt1.TotalSeconds / (dt1.TotalSeconds + dt2.TotalSeconds);
          double.TryParse(trk.trkseg[i].lat, NumberStyles.Any, CultureInfo.InvariantCulture, out double lat1);
          double.TryParse(trk.trkseg[i].lon, NumberStyles.Any, CultureInfo.InvariantCulture, out double lon1);
          double.TryParse(trk.trkseg[i + 1].lat, NumberStyles.Any, CultureInfo.InvariantCulture, out double lat2);
          double.TryParse(trk.trkseg[i + 1].lon, NumberStyles.Any, CultureInfo.InvariantCulture, out double lon2);
          retVal[0] = f * lat1 + (1 - f) * lat2;
          retVal[1] = f * lon1 + (1 - f) * lon2;
          break;
        }
      }
    }
    return retVal;
  }


  private DateTime getTime(int i)
  {
    string str = trk.trkseg[i].time;
    DateTime retVal = new DateTime(0);
    string[] formats = { AppParams.GPX_DATETIME_FORMAT, AppParams.GPX_DATETIME_FORMAT_MS };
    bool ok = DateTime.TryParseExact(str, formats, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out retVal);
    if (!ok)
      DebugLog.log("could not parse time stamp in GPX: file: " + str, enLogType.ERROR);
    return retVal;
  }


  static public gpx read(string fileName)
  {
    gpx retVal = null;
    try
    {
      if (File.Exists(fileName))
      {
        //does not work with newer garmin files:
        //StreamReader reader = new StreamReader(fileName);
        //reader.ReadToEnd();
        //XmlSerializer ser = new XmlSerializer(typeof(gpx));

        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.Load(fileName);
        retVal = createGpx(xmlDoc);
      }
    }
    catch (Exception ex)
    {
      DebugLog.log("unable to open file: " + fileName + ", " + ex.ToString(), enLogType.ERROR);
    }
    return retVal;
  }

  static gpx createGpx(XmlDocument doc)
  {
    gpx retVal = new gpx();
    try
    {
      retVal.trk = new gpxTrk();

      //    XmlNodeList nl = doc.SelectNodes("gpx/trk/trkseg");  // does not work, why??

      // the ugly way work:
      if (doc.ChildNodes.Count < 2)
        return null;
      if (doc.ChildNodes[1].Name != "gpx")
        return null;
      XmlNode n = doc.ChildNodes[1];
      if (n.ChildNodes.Count < 2)
        return null;
      n = doc.ChildNodes[1];
      if (n.ChildNodes[1].Name != "trk")
        return null;
      n = n.ChildNodes[1];
      if (n.ChildNodes.Count < 3)
        return null;
      if (n.ChildNodes[2].Name != "trkseg")
        return null;
      n = n.ChildNodes[2];

      XmlNodeList list = n.ChildNodes;
      retVal.trk.trkseg = new gpxTrkTrkpt[list.Count];

      for (int i = 0; i < list.Count; i++)
      {
        string latStr = list[i].Attributes["lat"].Value;
        string lonStr = list[i].Attributes["lon"].Value;
        string ele = list[i].ChildNodes[0].InnerText;
        n = list[i].ChildNodes[1];
        string timeStr = n.FirstChild.Value;
        retVal.trk.trkseg[i] = new gpxTrkTrkpt();
        retVal.trk.trkseg[i].ele = ele;
        retVal.trk.trkseg[i].lat = latStr;
        retVal.trk.trkseg[i].lon = lonStr;
        retVal.trk.trkseg[i].time = timeStr;
      }
    }
    catch (Exception ex)
    {
      DebugLog.log("error reading gpx file: " + ex.ToString(), enLogType.ERROR);
      retVal = null;
    }
    return retVal;
  }
}
