/********************************************************************************
 *               Author: Christian Müller
 *     Date of creation: 2023-09-01                                       
 *   Copyright (C) 2023: Christian Müller chrmue44(at)gmail(dot).com
 *
 *              Licence:  CC BY-NC 4.0 
 ********************************************************************************/

using libParser;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows.Controls;
using System.Windows.Documents;

namespace BatInspector
{
  class LocTxtItem
  {
    public string Name { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
  }

  class LocFileTxt
  {
    List<LocTxtItem> _list;

    public LocFileTxt()
    {
      _list = new List<LocTxtItem>();
    }
    public static LocFileTxt read(string filename) 
    {
      LocFileTxt retVal = new LocFileTxt();
      if (File.Exists(filename))
      {
        string[] lines = File.ReadAllLines(filename);
        foreach (string line in lines) 
        {
          string[] token = line.Split('\t');
          if(token.Length == 3) 
          {
            LocTxtItem item = new LocTxtItem();
            item.Name = token[0];
            double sign = 0;
            if (token[1].IndexOf('N') > 0)
            {
              token[1] = token[1].Replace("N", "").Trim();
              sign = 1.0;
            }
            else if (token[1].IndexOf('S') > 0)
            {
              token[1] = token[1].Replace("S", "").Trim();
              sign = -1.0;
            }
            double.TryParse(token[1], NumberStyles.Any, CultureInfo.InvariantCulture, out double lat);
            item.Latitude = lat* sign;
            if (sign == 0.0)
              continue;
            sign = 0;
            if (token[2].IndexOf('E') > 0)
            {
              token[2] = token[2].Replace("E", "").Trim();
              sign = 1.0;
            }
            else if (token[2].IndexOf('W') > 0)
            {
              token[2] = token[2].Replace("W", "").Trim();
              sign = -1.0;
            }
            if (sign == 0.0)
              continue;

            double.TryParse(token[2], NumberStyles.Any, CultureInfo.InvariantCulture, out double lon);
            item.Longitude = lon * sign;
            retVal._list.Add(item);
          }
        }
      }
      return retVal;
    }

    public double[] getPosition(string fName)
    {
      double[] retVal = new double[2];
      retVal[0] = 0.0;
      retVal[1] = 0.0;
      string pointName = Path.GetFileNameWithoutExtension(fName);
      for (int i = 0; i < this._list.Count; i++)
      {
        if (!string.IsNullOrEmpty(_list[i].Name) && (_list[i].Name.ToLower().IndexOf(Path.GetFileNameWithoutExtension(fName)) >= 0))
        {
          retVal[0] = _list[i].Latitude;
          retVal[1] = _list[i].Longitude;
          break;
        }
      }
      return retVal;
    }
  }
}
