/********************************************************************************
 *               Author: Christian Müller
 *      Date of cration: 2023-04-17                                       
 *   Copyright (C) 2023: christian Müller christian(at)chrmue(dot).de
 *
 *              Licence:
 * 
 * THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND,
 * EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
 ********************************************************************************/

using libParser;
using System;
using System.IO;
using System.Xml.Serialization;
using BatInspector;
using System.Globalization;


public partial class gpx
{
  public double[] getPosition(DateTime t)
  {
    double[] retVal = new double[2];
    retVal[0] = 0.0;
    retVal[1] = 0.0;
    for(int i = 0; i < this.trk.trkseg.Length; i++)
    {
      if(i < (trk.trkseg.Length - 1))
      {
        DateTime t1 = getTime(i);
        DateTime t2 = getTime(i + 1); 
        if((t1 <= t) && (t <= t2))
        {
          TimeSpan dt1 = t - t1;
          TimeSpan dt2 = t2 - t;
          double f = dt1.TotalSeconds / (dt1.TotalSeconds + dt2.TotalSeconds);
          double.TryParse(trk.trkseg[i].lat, NumberStyles.Any, CultureInfo.InvariantCulture, out double lat1);
          double.TryParse(trk.trkseg[i].lon, NumberStyles.Any, CultureInfo.InvariantCulture, out double lon1);
          double.TryParse(trk.trkseg[i + 1].lat, NumberStyles.Any, CultureInfo.InvariantCulture, out double lat2);
          double.TryParse(trk.trkseg[i + 1].lon, NumberStyles.Any, CultureInfo.InvariantCulture, out double lon2);
          retVal[0] = f * lat1 + (1 - f)* lat2;
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
    DateTime retVal = DateTime.ParseExact(str, AppParams.GPX_DATETIME_FORMAT, CultureInfo.InvariantCulture);
    return retVal;
  }


  static public gpx read(string fileName)
  {
    gpx retVal = null;
    try
    {
      if (File.Exists(fileName))
      {
        StreamReader reader = new StreamReader(fileName);
        reader.ReadToEnd();
        XmlSerializer ser = new XmlSerializer(typeof(gpx));
        retVal = (gpx)ser.Deserialize(reader);
      }
    }
    catch(Exception ex) 
    {
      DebugLog.log("unable to open file: " + fileName + ", " + ex.ToString(), enLogType.ERROR);
    }
    return retVal;  
  }
}
