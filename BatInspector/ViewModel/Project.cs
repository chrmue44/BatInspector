/********************************************************************************
 *               Author: Christian Müller
 *      Date of cration: 2021-08-10                                       
 *   Copyright (C) 2022: christian Müller christian(at)chrmue(dot).de
 *
 *              Licence:
 * 
 * THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND,
 * EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
 ********************************************************************************/
using libParser;
using libScripter;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace BatInspector
{
  public class Project
  {
    BatExplorerProjectFile _batExplorerPrj;
    string _prjFileName;
    string _wavSubDir;
    bool _ok;

    public bool Ok {get {return _ok;} }

    public BatExplorerProjectFileRecordsRecord[] Records { get { return _batExplorerPrj.Records; } }
    public string Name { get{ return _prjFileName; } }
    public string WavSubDir { get { return _wavSubDir; } }


    public static bool containsProject(DirectoryInfo dir)
    {
      bool retVal = false;
      try
      {
        string[] files = System.IO.Directory.GetFiles(dir.FullName, "*.bpr",
                         System.IO.SearchOption.TopDirectoryOnly);
        if (files.Length > 0)
          retVal = true;
      }
      catch { }
      return retVal;
    }

    public static bool containsWavs(DirectoryInfo dir)
    {
      bool retVal = false;
      try
      {
        string[] files = System.IO.Directory.GetFiles(dir.FullName, "*.wav",
                         System.IO.SearchOption.TopDirectoryOnly);
        if (files.Length > 0)
          retVal = true;
      }
      catch { }
      return retVal;
    }

    public static bool evaluationDone(DirectoryInfo dir)
    {
      bool retVal = false;
      string fName = dir.FullName + "\\report.csv";
      if (File.Exists(fName))
      {
        Csv csv = new Csv();
        {
          csv.read(fName);
          int colSp = csv.findInRow(1, Cols.SPECIES_MAN);
          if (colSp > 0)
          {
            int row = csv.findInCol("todo", colSp);
            if (row == 0)
              retVal = true;
          }
        }
      }

      return retVal;
    }

    public void readPrjFile(string fName)
    {
      try
      {
        _prjFileName = fName;
        string xml = File.ReadAllText(fName);
        var serializer = new XmlSerializer(typeof(BatExplorerProjectFile));

        TextReader reader = new StringReader(xml);
        _batExplorerPrj = (BatExplorerProjectFile)serializer.Deserialize(reader);
        if (_batExplorerPrj.Type.IndexOf("Elekon") >= 0)
          _wavSubDir = "/Records/";
        else
          _wavSubDir = "/";
        _ok = true;
      }
      catch
      {
        DebugLog.log("error reading project file: " + fName, enLogType.ERROR);
        _ok = false;
      }
    }

    public void writePrjFile()
    {
      if (_batExplorerPrj != null)
      {
        var serializer = new XmlSerializer(typeof(BatExplorerProjectFile));
        TextWriter writer = new StreamWriter(_prjFileName);
        serializer.Serialize(writer, _batExplorerPrj);
        writer.Close();        
      }
    }

    public void removeFile(string wavName)
    {
      List<BatExplorerProjectFileRecordsRecord> list = _batExplorerPrj.Records.ToList();
      foreach (BatExplorerProjectFileRecordsRecord rec in list)
      {
        if (rec.File == wavName)
        {
          list.Remove(rec);
          break;
        }
      }
      _batExplorerPrj.Records = list.ToArray();
    }

    public BatExplorerProjectFileRecordsRecord find(string fileName)
    {
      BatExplorerProjectFileRecordsRecord retVal = null;
      foreach(BatExplorerProjectFileRecordsRecord r in _batExplorerPrj.Records)
      {
        if(fileName.Contains(r.File))
        {
          retVal = r;
          break;
        }
      }
      return retVal;
    }

    public void fillFromDirectory(DirectoryInfo dir)
    {
      try
      {
        string[] files = System.IO.Directory.GetFiles(dir.FullName, "*.wav",
                         System.IO.SearchOption.TopDirectoryOnly);
        _batExplorerPrj = new BatExplorerProjectFile("wavs", files.Length);
        _batExplorerPrj.Originator = "BatInspector";
        _batExplorerPrj.Type = "BatInspector";
        
        for (int i = 0; i < files.Length; i++)
        {
          string name = Path.GetFileName(files[i]);
          _batExplorerPrj.Records[i].File = name;
          name = Path.GetFileNameWithoutExtension(files[i]);
          _batExplorerPrj.Records[i].Name = name;
        }
        _wavSubDir = "/";
        _ok = true;
        _prjFileName = Path.GetFileName(dir.FullName);
        _batExplorerPrj.Name = _prjFileName;
        _prjFileName = dir.FullName + "/" + _prjFileName +".bpr";
        writePrjFile();
      }
      catch { }

    }
  }
}
