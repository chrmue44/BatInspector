/********************************************************************************
 *               Author: Christian Müller
 *     Date of creation: 2023-10-26                                       
 *   Copyright (C) 2023: Christian Müller chrmue44(at)gmail(dot).com
 *
 *              Licence:  CC BY-NC 4.0 
 ********************************************************************************/
using libParser;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows.Controls;
using System.Xml;
using System.Xml.Serialization;


  public partial class kml
  {
  public double[] getPosition(string fName)
  {
    double[] retVal = new double[2];
    retVal[0] = 0.0;
    retVal[1] = 0.0;
    string pointName = Path.GetFileNameWithoutExtension(fName);
    for (int i = 0; i < this.Doc.Placemark.Length; i++)
    {
      if (!string.IsNullOrEmpty(Doc.Placemark[i].name) && (Doc.Placemark[i].name == pointName))
      {
        string[] coords = Doc.Placemark[i].Point.coordinates.Split(',');
        if (coords.Length == 3)
        {
          double.TryParse(coords[1], NumberStyles.Any, CultureInfo.InvariantCulture, out retVal[0]);
          double.TryParse(coords[0], NumberStyles.Any, CultureInfo.InvariantCulture, out retVal[1]);
        }
        else
          DebugLog.log("error reading coordinates from KML for file: " + fName, enLogType.ERROR);
        break;
      }
    }
    return retVal;
  }

  static public kml read(string fileName)
  {
    kml retVal = null;
    try
    {
      if (File.Exists(fileName))
      {
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.Load(fileName);
        retVal = createKml(xmlDoc);
      }
    }
    catch (Exception ex)
    {
      DebugLog.log("unable to open file: " + fileName + ", " + ex.ToString(), enLogType.ERROR);
    }
    return retVal;
  }

  static private kml createKml(XmlDocument doc)
  {
    kml retVal = new kml();
    retVal.Doc = new kmlDocument();
    string errMsg = "";

    try
    {
      if (doc.ChildNodes.Count < 1)
        return null;
      if (doc.ChildNodes[0].Name != "kml")
        return null;
      XmlNode n = doc.ChildNodes[0];
      if (n.ChildNodes.Count < 1)
        return null;
      n = doc.ChildNodes[0];
      if (n.ChildNodes[0].Name != "Document")
        return null;

      XmlNodeList list = n.ChildNodes[0].ChildNodes;
      List<kmlDocumentPlacemark> tmpList = new List<kmlDocumentPlacemark>();

      for (int i = 0; i < list.Count; i++)
      {
        if (list[i].Name == "Placemark")
        {
          XmlNode placeMark = list[i];
          kmlDocumentPlacemark p = new kmlDocumentPlacemark();
          for (int j = 0; j < placeMark.ChildNodes.Count; j++)
          {
            if (placeMark.ChildNodes[j].Name == "name")
              p.name = placeMark.ChildNodes[j].InnerText;
            else if (placeMark.ChildNodes[j].Name == "Point")
            {
              XmlNode point = placeMark.ChildNodes[j];
              if (point.ChildNodes.Count < 1)
                return null;
              if (point.ChildNodes[0].Name != "coordinates")
                return null;
              string coords = point.ChildNodes[0].InnerText;
              p.Point = new kmlDocumentPlacemarkPoint();
              p.Point.coordinates = coords;
            }
          }
          tmpList.Add(p);
        }
      }
      retVal.Doc.Placemark = tmpList.ToArray();
    }
    catch (Exception ex)
    {
      retVal = null;
      errMsg = ex.ToString();
    }
    if (retVal == null)
      DebugLog.log("error reading kml: " + errMsg, enLogType.ERROR);

    return retVal;

  }
}
