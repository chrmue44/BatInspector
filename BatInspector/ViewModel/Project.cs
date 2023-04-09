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
using System.Runtime.InteropServices.WindowsRuntime;
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
    Analysis _analysis;

    public bool Ok {get {return _ok;} }

    public BatExplorerProjectFileRecordsRecord[] Records { get { return _batExplorerPrj.Records; } }
    public string Name { get{ return _prjFileName; } }
    public string WavSubDir { get { return _wavSubDir; } }
    public string Notes { get { return _batExplorerPrj != null ?_batExplorerPrj.Notes : ""; } set { if(_batExplorerPrj!= null) _batExplorerPrj.Notes = value; } }
    public string Created { get { return _batExplorerPrj.Created; }  set { _batExplorerPrj.Created = value; } }
    public List<string> Species {  get { return _speciesList; } }
    public List<SpeciesInfos> SpeciesInfos { get { return _speciesInfo; } }
    public string PrjDir { get { return _selectedDir + "/"; } }

    public Analysis Analysis { get { return _analysis; }  }

    BatSpeciesRegions _batSpecRegions;
    bool _changed = false;

    public Project(BatSpeciesRegions regions, List<SpeciesInfos> speciesInfo)
    {
      _batSpecRegions = regions;
      _speciesList = new List<string>();
      _speciesInfo = speciesInfo;
      _analysis = new Analysis(speciesInfo);
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

    public void removeFilesNotInReport()
    {
      List<BatExplorerProjectFileRecordsRecord> newList = new List<BatExplorerProjectFileRecordsRecord>();
      string destDir = this.PrjDir + "/del";
      if (!Directory.Exists(destDir))
        Directory.CreateDirectory(destDir);

      foreach (BatExplorerProjectFileRecordsRecord rec in _batExplorerPrj.Records)
      {
        if (Analysis.find(rec.Name) == null)
        {
          string name = PrjDir + "/" + WavSubDir + "/" + rec.File;
          DebugLog.log("delete file " + name, enLogType.DEBUG);
          try
          {
            if (File.Exists(name))
            {
              File.Copy(name, destDir + "/" + Path.GetFileName(name));
              File.Delete(name);
            }
            name = name.Replace(".wav", ".xml");
            if (File.Exists(name))
            {
              File.Copy(name, destDir + "/" + Path.GetFileName(name));
              File.Delete(name);
            }
            name = name.Replace(".xml", ".png");
            if (File.Exists(name))
            {
              File.Copy(name, destDir + "/" + Path.GetFileName(name));
              File.Delete(name);
            }
              _changed = true;
          }
          catch(Exception ex) 
          {
              //TODO log
          }
        }
        else
          newList.Add(rec);
      }
      _batExplorerPrj.Records = newList.ToArray();
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


    private void addRecord(string filePath, ref List<BatExplorerProjectFileRecordsRecord> list)
    {
      BatExplorerProjectFileRecordsRecord rec = new BatExplorerProjectFileRecordsRecord();
      string name = Path.GetFileName(filePath);
      rec.File = name;
      name = Path.GetFileNameWithoutExtension(filePath);
      rec.Name = name;
      list.Add(rec);  
    }

    private string[] splitWav(string fName, double size)
    {
      WavFile wav = new WavFile();
      string[] retVal = null;
      try
      {
        wav.readFile(fName);
        double len = wav.AudioSamples.Length / wav.FormatChunk.Frequency / size;
        int cnt = (int)len;
        if ((double)cnt < len)
          cnt++;
        retVal = new string[cnt];
        for(int i = 0; i < cnt; i++)
        {
          int iStart = (int)((double)i * wav.FormatChunk.Frequency * size);
          int iEnd = iStart + (int)((double)wav.FormatChunk.Frequency * size) - 1;
          if (iEnd > (wav.AudioSamples.Length - 1))
            iEnd = wav.AudioSamples.Length - 1;
          short[] samples = new short[iEnd - iStart + 1];
          Array.Copy(wav.AudioSamples, iStart, samples, 0, iEnd - iStart + 1);
          WavFile newWav = new WavFile();
          newWav.createFile(wav.FormatChunk.Channels, (int)wav.FormatChunk.Frequency, 0, samples.Length-1, samples);
          string name = Path.GetDirectoryName(fName) + "/" +
                        Path.GetFileNameWithoutExtension(fName) + "_" + i.ToString("D3") + Path.GetExtension(fName);
          newWav.saveFileAs(name);
          retVal[i] = name;
        }
        File.Delete(fName);
      }
      catch (Exception ex) 
      {
        DebugLog.log("Error splitting wav file: " + fName + ":" + ex.ToString(), enLogType.ERROR);
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
        List<BatExplorerProjectFileRecordsRecord> records = new List<BatExplorerProjectFileRecordsRecord>();
        
        for (int i = 0; i < files.Length; i++)
        {
          FileInfo fi = new FileInfo(files[i]);
          if(fi.Length > 3800000)
          {
            string[] fNames = splitWav(files[i], 4.5);
            for(int j = 0; j < fNames.Length; j++)
            {
              addRecord(fNames[j], ref records);
            }
          }
          else
          {
            addRecord(files[i], ref records);
          }          
        }

        _batExplorerPrj = new BatExplorerProjectFile("wavs", records);
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
