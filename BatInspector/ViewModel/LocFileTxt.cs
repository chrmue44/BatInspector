/********************************************************************************
 *               Author: Christian Müller
 *     Date of creation: 2023-09-01                                       
 *   Copyright (C) 2023: Christian Müller chrmue44(at)gmail(dot).com
 *
 *              Licence:  CC BY-NC 4.0 
 ********************************************************************************/

using libParser;
using libScripter;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Forms;

namespace BatInspector
{
  class LocTxtItem
  {
    public string Name { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }

    public string Date { get; set; }
    public string Time { get; set; }
  }

  public enum enLocFileMode
  {
    FILE_NAME = 0,
    TIME      = 1
  }

  [TypeConverter(typeof(ExpandableObjectConverter))]
  [DataContract]
  public class LocFileSettings
  {
    [DataMember]
    public int ColFilename { get; set; }
    [DataMember]
    public int ColNS { get; set; }
    [DataMember]
    public int ColWE { get; set; }
    [DataMember]
    public int ColLatitude { get; set; }
    [DataMember]
    public int ColLongititude { get; set; }
    [DataMember]
    public int ColTime { get; set; }
    [DataMember]
    public int ColDate { get; set; }
    [DataMember]
    public enLocFileMode Mode { get; set; }
    [DataMember]
    public char Delimiter { get; set; }
    public LocFileSettings()
    {
      ColFilename = 0;
      ColLatitude = 1;
      ColLongititude = 2;
      Mode = enLocFileMode.FILE_NAME;
      ColNS = 1;
      ColWE = 2;
      ColTime = -1;
      ColDate = -1;
      Delimiter = '\t';
    }

    public LocFileSettings(LocFileSettings l)
    {
      ColFilename = l.ColFilename;
      ColLatitude = l.ColLatitude;
      ColLongititude = l.ColLongititude;
      Mode = l.Mode;
      ColNS = l.ColNS;
      ColWE = l.ColWE;
      ColTime = l.ColTime;
      ColDate = l.ColDate;
      Delimiter = l.Delimiter;

    }
    public LocFileSettings(enLocFileMode m, int colFile, int colNS, int colLat, int colWE, int colLon, int colTime, int colDate, char delimiter) 
    { 
      ColFilename = colFile;
      ColLatitude = colLat;
      ColLongititude = colLon;
      Mode = m;
      ColNS = colNS;
      ColWE = colWE;
      ColTime = colTime;
      ColDate = colDate;
      Delimiter = delimiter;
    }

    public int getMaxColNr()
    {
      int retVal = 0;
      if (retVal < ColNS)
        retVal = ColNS;
      if (retVal < ColLatitude)
        retVal = ColLatitude;
      if(retVal < ColLongititude)
        retVal = ColLongititude;
      if(retVal < ColWE)
        retVal = ColWE;
      if(retVal < ColFilename)
        retVal = ColFilename;
      if(retVal < ColTime)
        retVal = ColTime;
      return retVal;
    }
  }

  class LocFileTxt
  {
    List<LocTxtItem> _list;
    LocFileSettings _pars;

    public LocFileTxt(LocFileSettings pars)
    {
      _list = new List<LocTxtItem>();
      _pars = pars;
    }

    public LocFileSettings Pars { get{ return _pars; } }
    public static LocFileTxt read(string filename, LocFileSettings pars) 
    {
      LocFileTxt retVal = new LocFileTxt(pars);
      if (File.Exists(filename))
      {
        string[] lines = File.ReadAllLines(filename);
        foreach (string line in lines) 
        {
          string[] token = line.Split(pars.Delimiter);
          if(token.Length > pars.getMaxColNr()) 
          {
            LocTxtItem item = new LocTxtItem();
            item.Name = token[pars.ColFilename];
            double sign = 0;
            if (token[pars.ColNS].IndexOf('N') >= 0)
            {
              token[pars.ColNS] = token[pars.ColNS].Replace("N", "").Trim();
              sign = 1.0;
            }
            else if (token[pars.ColNS].IndexOf('S') >= 0)
            {
              token[pars.ColNS] = token[pars.ColNS].Replace("S", "").Trim();
              sign = -1.0;
            }
            double.TryParse(token[pars.ColLatitude], NumberStyles.Any, CultureInfo.InvariantCulture, out double lat);
            item.Latitude = lat* sign;
            if (sign == 0.0)
              continue;
            sign = 0;
            if (token[pars.ColWE].IndexOf('E') >= 0)
            {
              token[pars.ColWE] = token[pars.ColWE].Replace("E", "").Trim();
              sign = 1.0;
            }
            else if (token[pars.ColWE].IndexOf('W') >= 0)
            {
              token[pars.ColWE] = token[pars.ColWE].Replace("W", "").Trim();
              sign = -1.0;
            }
            if (sign == 0.0)
              continue;

            double.TryParse(token[pars.ColLongititude], NumberStyles.Any, CultureInfo.InvariantCulture, out double lon);
            item.Longitude = lon * sign;
            if(pars.ColTime >= 0)
              item.Time = token[pars.ColTime];
            if (pars.ColDate >= 0)
              item.Date = token[pars.ColDate];
            retVal._list.Add(item);
          }
        }
      }
      return retVal;
    }

    public double[] getPosition(string fName, DateTime t)
    {
      double[] retVal = new double[2];
      retVal[0] = 0.0;
      retVal[1] = 0.0;
      switch (_pars.Mode)
      {
        case enLocFileMode.FILE_NAME:
          {
            string pointName = Path.GetFileNameWithoutExtension(fName).ToLower();
            for (int i = 0; i < this._list.Count; i++)
            {
              if (!string.IsNullOrEmpty(_list[i].Name) && (_list[i].Name.ToLower().IndexOf(pointName) >= 0))
              {
                retVal[0] = _list[i].Latitude;
                retVal[1] = _list[i].Longitude;
                break;
              }
            }
          }
          break;
        case enLocFileMode.TIME:
          {
            for (int i = 0; i < (this._list.Count - 1); i++)
            {
              bool ok = DateTime.TryParse(_list[i].Date, out DateTime d1);
              ok &= DateTime.TryParse(_list[i].Time, out DateTime ti1);
              if (ok)
              {
                d1 = d1.AddSeconds(ti1.Hour * 3600 + ti1.Minute * 60 + ti1.Second);
                ok = DateTime.TryParse(_list[i + 1].Date, out DateTime d2);
                ok &= DateTime.TryParse(_list[i + 1].Time, out DateTime ti2);
                if (ok)
                {
                  d2 = d2.AddSeconds(ti2.Hour * 3600 + ti2.Minute * 60 + ti2.Second);
                  DateTime t1 = d1; //.ToUniversalTime();
                  DateTime t2 = d2; //.ToUniversalTime();
                  if ((t1 <= t) && (t <= t2))
                  {
                    TimeSpan dt1 = t - t1;
                    TimeSpan dt2 = t2 - t;
                    double f = dt1.TotalSeconds / (dt1.TotalSeconds + dt2.TotalSeconds);
                    retVal[0] = (1 - f) * this._list[i].Latitude + f * this._list[i + 1].Latitude;
                    retVal[1] = (1 - f) * this._list[i].Longitude + f * this._list[i + 1].Longitude;
                    break;
                  }
                }
              }
            }
          }
          break;
      }
      return retVal;
    }
  }
}
