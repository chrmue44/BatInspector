/********************************************************************************
 *               Author: Christian Müller
 *      Date of cration: 2021-08-10                                       
 *   Copyright (C) 2022: Christian Müller christian(at)chrmue(dot).de
 *
 *              Licence:
 * 
 * THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND,
 * EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
 ********************************************************************************/
using libParser;
using libScripter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml.Serialization;

namespace BatInspector
{
  public class Project
  {
    BatExplorerProjectFile _batExplorerPrj;
    string _prjFileName;
    string _wavSubDir;
    bool _ok;
    List<string> _speciesList;
    List<SpeciesInfos> _speciesInfo;
    string _selectedDir;

    public bool Ok {get {return _ok;} }

    public BatExplorerProjectFileRecordsRecord[] Records { get { return _batExplorerPrj.Records; } }
    public string Name { get{ return _prjFileName; } }
    public string WavSubDir { get { return _wavSubDir; } }
    public string Notes { get { return _batExplorerPrj != null ?_batExplorerPrj.Notes : ""; } set { if(_batExplorerPrj!= null) _batExplorerPrj.Notes = value; } }
    public string Created { get { return _batExplorerPrj.Created; }  set { _batExplorerPrj.Created = value; } }
    public List<string> Species {  get { return _speciesList; } }
    BatSpeciesRegions _batSpecRegions;
    bool _changed = false;

    public Project(BatSpeciesRegions regions, List<SpeciesInfos> speciesInfo)
    {
      _batSpecRegions = regions;
      _speciesList = new List<string>();
      _speciesInfo = speciesInfo;
    }

    /// <summary>
    /// checks wether a directory contains a project
    /// </summary>
    /// <param name="dir"></param>
    /// <returns></returns>
    public static bool containsProject(DirectoryInfo dir)
    {
      try
      {
        string[] files = System.IO.Directory.GetFiles(dir.FullName, "*" + AppParams.EXT_PRJ,
                         System.IO.SearchOption.TopDirectoryOnly);
        if (files.Length > 0)
          return  true;
      }
      catch { }
      return false;
    }

    /// <summary>
    /// checks if a directory contains a report
    /// </summary>
    /// <param name="dir"></param>
    /// <returns></returns>
    public static string containsReport(DirectoryInfo dir)
    {
      try
      {
        string[] files = System.IO.Directory.GetFiles(dir.FullName, "*.*" ,
                         System.IO.SearchOption.TopDirectoryOnly);
        foreach(string file in files)
        {
          if (file.IndexOf(AppParams.PRJ_REPORT) >= 0)
            return file;
        }
      }
      catch { }
      return "";
    }

    /// <summary>
    /// checks wether a directory contains wav files
    /// </summary>
    /// <param name="dir"></param>
    /// <returns></returns>
    public static bool containsWavs(DirectoryInfo dir)
    {
      bool retVal = false;
      try
      {
        string[] files = System.IO.Directory.GetFiles(dir.FullName, "*" + AppParams.EXT_WAV,
                         System.IO.SearchOption.TopDirectoryOnly);
        if (files.Length > 0)
          retVal = true;
      }
      catch { }
      return retVal;
    }

    /// <summary>
    /// checks wether the manual evaluation of calls is completed (no more todo entries) or not
    /// </summary>
    /// <param name="dir"></param>
    /// <returns></returns>
    public static bool evaluationDone(DirectoryInfo dir)
    {
      bool retVal = false;
      string fName = dir.FullName + "/" + AppParams.PRJ_REPORT;
      if (File.Exists(fName))
      {
        Csv csv = new Csv();
        {
          csv.read(fName, ";", true);
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
        _selectedDir = Path.GetDirectoryName(fName);
        string xml = File.ReadAllText(fName);
        var serializer = new XmlSerializer(typeof(BatExplorerProjectFile));
        _speciesList.Clear();
        TextReader reader = new StringReader(xml);
        _batExplorerPrj = (BatExplorerProjectFile)serializer.Deserialize(reader);
        if (_batExplorerPrj.Created == null)
          _batExplorerPrj.Created = "";
        if (_batExplorerPrj.Notes == null)
          _batExplorerPrj.Notes = "";
        if (/*(_batExplorerPrj.Type != null) && (_batExplorerPrj.Type.IndexOf("Elekon") >= 0) || */
            Directory.Exists(_selectedDir + "/Records"))
        {
          _wavSubDir = "/Records/";
          initSpeciesList();
        }
        else
          _wavSubDir = "/";
        _ok = true;
        _changed = false;
      }
      catch (Exception ex)
      {
        DebugLog.log("error reading project file: " + fName + " :" + ex.ToString() , enLogType.ERROR);
        _ok = false;
      }
    }

    public void writePrjFile()
    {
      if ((_batExplorerPrj != null) && _changed)
      {
        _batExplorerPrj.FileVersion = "3";
        var serializer = new XmlSerializer(typeof(BatExplorerProjectFile));
        TextWriter writer = new StreamWriter(_prjFileName);
        serializer.Serialize(writer, _batExplorerPrj);
        writer.Close();
        _changed = false;
        DebugLog.log("project '" + _prjFileName + "' saved", enLogType.INFO);
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
          _changed = true;
          break;
        }
      }
      _batExplorerPrj.Records = list.ToArray();
    }

    /// <summary>
    /// find a file in the project
    /// </summary>
    /// <param name="fileName">name of the file (full path or just file name)</param>
    /// <returns>record containing the file information</returns>
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

    /// <summary>
    /// create an elekon compatible project from a directory that contains wav files
    /// </summary>
    /// <param name="dir"></param>
    public void fillFromDirectory(DirectoryInfo dir)
    {
      try
      {
        string[] files = System.IO.Directory.GetFiles(dir.FullName, "*.wav",
                         System.IO.SearchOption.TopDirectoryOnly);
        _batExplorerPrj = new BatExplorerProjectFile("wavs", files.Length);
      //  _batExplorerPrj.Originator = "BatInspector";
//        _batExplorerPrj.Type = "BatInspector";
        
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
        _changed = true;
        writePrjFile();
      }
      catch { }
    }

    public void createXmlInfoFiles(double lat, double lon)
    {
      bool replaceAll = false;
      foreach (BatExplorerProjectFileRecordsRecord record in _batExplorerPrj.Records)
      {
        bool create = replaceAll;
        string fullName = _selectedDir + _wavSubDir + record.File;
        if(File.Exists(fullName))
        {
          if (!replaceAll && File.Exists(fullName.Replace(".wav", ".xml")))
          {
            MessageBoxResult res = MessageBox.Show("Replace all existing info files in this project?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (res == MessageBoxResult.Yes)
            {
              replaceAll = true;
              create = true;
            }
          }
          else
            create = true;
          if(create)
            ElekonInfoFile.create(fullName, lat, lon);
        }
      }
    }

    /// <summary>
    /// init the list of species valid for the project.
    /// The list is depending on the region where the first file of the project was recorded
    /// </summary>
    void initSpeciesList()
    {
      if(_batExplorerPrj.Records.Length > 0)
      {
        string fName =_selectedDir + _wavSubDir + _batExplorerPrj.Records[0].File.Replace(AppParams.EXT_WAV, AppParams.EXT_INFO);
        BatRecord info = ElekonInfoFile.read(fName);
        ElekonInfoFile.parsePosition(info, out double lat, out double lon);
        ParRegion reg = _batSpecRegions.findRegion(lat, lon);
        _speciesList = new List<string>();
        if (reg != null)
        {
          foreach (string sp in reg.Species)
            _speciesList.Add(sp);
        }
        else
        {
          foreach (SpeciesInfos sp in _speciesInfo)
            _speciesList.Add(sp.Abbreviation);
        }
        _speciesList.Add("----");
        _speciesList.Add("?");
        _speciesList.Add("todo");
      }
    }
  }
}
