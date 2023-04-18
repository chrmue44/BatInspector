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
using BatInspector.Properties;
using libParser;
using libScripter;
using Microsoft.VisualBasic.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows;
using System.Xml.Serialization;

namespace BatInspector
{
  public class PrjInfo
  {
    public string Name { get; set; }
    public string SrcDir { get;set; }
    public string DstDir { get;set; }
    public int MaxFileCnt { get; set; } 
    public double MaxFileLenSec { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string Weather { get; set; } 
    public string Landscape { get; set; } 
    public string GpxFile { get; set; }
  }


  public class Project
  {
    private BatExplorerProjectFile _batExplorerPrj;
    private string _prjFileName;
    private string _wavSubDir;
    private bool _ok;
    private List<string> _speciesList;
    private List<SpeciesInfos> _speciesInfo;
    private string _selectedDir;
    private Analysis _analysis;

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

    public string ReportName { get { return getReportName(_selectedDir); } }

    public string SummaryName { get { return getSummaryName(_selectedDir); } }

    BatSpeciesRegions _batSpecRegions;
    bool _changed = false;

    public Project(BatSpeciesRegions regions, List<SpeciesInfos> speciesInfo)
    {
      _batSpecRegions = regions;
      _speciesList = new List<string>();
      _speciesInfo = speciesInfo;
      _analysis = new Analysis(this);
    }


    public static string getReportName(string dir)
    {
      return dir +"/" + AppParams.Inst.Models[AppParams.Inst.SelectedModel].Dir + "/" + AppParams.PRJ_REPORT;
    }

    public static string getSummaryName(string dir)
    {
      return dir + "/" + AppParams.Inst.Models[AppParams.Inst.SelectedModel].Dir + "/" + AppParams.PRJ_SUMMARY;
    }

    /// <summary>
    /// checks wether a directory contains a project
    /// </summary>
    /// <param name="dir"></param>
    /// <returns></returns>
    public static string containsProject(DirectoryInfo dir)
    {
      try
      {
        string[] files = System.IO.Directory.GetFiles(dir.FullName, "*" + AppParams.EXT_PRJ,
                         System.IO.SearchOption.TopDirectoryOnly);
        if (files.Length > 0)
          return  files[0];
      }
      catch { }
      return "";
    }

    /// <summary>
    /// checks if a directory contains a report
    /// </summary>
    /// <param name="dir">directory info of directory to check</param>
    /// <param name="repName">naem of the report to search for</param>
    /// <returns></returns>
    public static string containsReport(DirectoryInfo dir, string repName)
    {
      string dirName = dir.FullName + "/" + AppParams.Inst.Models[AppParams.Inst.SelectedModel].Dir;
      if (Directory.Exists(dirName))
      {
        try
        {
          string[] files = System.IO.Directory.GetFiles(dirName, "*.*",
                           System.IO.SearchOption.TopDirectoryOnly);
          foreach (string file in files)
          {
            if (file.IndexOf(repName) >= 0)
              return file;
          }
        }
        catch { }
      }
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
      string sumName = Project.getSummaryName(dir.FullName);
      if(File.Exists(sumName))
      {
        Csv csv = new Csv();
        csv.read(sumName, ";", true);
        int col = csv.findInCol(Cols.SPECIES_MAN, "todo");
        if (col > 0)
          retVal = true;
      }
      else
      { 
        string fName = Project.getReportName(dir.FullName);
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
      }

      return retVal;
    }

    /// <summary>
    /// create one or multiple projects from a directory containing WAV files
    /// </summary>
    /// <param name="info">parameters to specify project</param>
    /// <param name="regions"></param>
    /// <param name="speciesInfo"></param>
    public static void createPrj(PrjInfo info, BatSpeciesRegions regions, List<SpeciesInfos> speciesInfo)
    {
      try
      {
        DebugLog.log("start creating project(s): " + info.Name, enLogType.INFO);
        string[] files = Directory.GetFiles(info.SrcDir, "*.wav");
        if (files.Length > 0)
        {
          WavFile wavFile = new WavFile();
          wavFile.readFile(files[0]);
          uint sampleRate = wavFile.FormatChunk.Frequency;
          int maxFileLen = (int)((double)sampleRate * 2 * info.MaxFileLenSec + 2048);

          // 1st step split all 
          DebugLog.log("split files exceeding max. length", enLogType.INFO);
          foreach (string f in files)
          {
            FileInfo fileInfo = new FileInfo(f);
            if (fileInfo.Length > maxFileLen)
              Project.splitWav(f, info.MaxFileLenSec);
          }

          // 2nd step create projects
          files = Directory.GetFiles(info.SrcDir, "*.wav");
          double prjCntd = (double)files.Length / info.MaxFileCnt;
          int prjCnt = (int)prjCntd;
          if (prjCntd > prjCnt)
            prjCnt++;
          int fileCnt = files.Length / prjCnt;
          for (int p = 0; p < prjCnt; p++)
          {
            string dirName = info.Name;
            if(prjCnt > 1)
              dirName += "_" + p.ToString("D2");
            DebugLog.log("creating project " + dirName, enLogType.INFO);
            int iFirst = p * fileCnt;
            int iLast = (p + 1) * fileCnt - 1;
            char[] invalid = Path.GetInvalidPathChars();
            string fullDir = info.DstDir + "/" + dirName;
            string wavDir = fullDir + "/" + AppParams.DIR_WAVS;
            if(Directory.Exists(fullDir))
            {
              DebugLog.log("directory '" + fullDir + "' already exists, project creation aborted!", enLogType.ERROR);
              return;
            }
            else
              Directory.CreateDirectory(fullDir);
            if (!Directory.Exists(wavDir))
              Directory.CreateDirectory(wavDir);
            if (iLast > (files.Length - 1))
              fileCnt = files.Length - iFirst;
            string[] prjFiles = new string[fileCnt];
            Array.Copy(files, iFirst, prjFiles, 0, fileCnt);
            Utils.copyFiles(prjFiles, wavDir);
            Project prj = new Project(regions, speciesInfo);
            DirectoryInfo dir = new DirectoryInfo(fullDir);
            prj.fillFromDirectory(dir, "/" + AppParams.DIR_WAVS, info.Weather + "\n" + info.Landscape);
            prj.createXmlInfoFiles(info);
          }
        }
        else
          DebugLog.log("No WAV files in directory " + info.SrcDir, enLogType.ERROR);
      }
      catch (Exception e)
      {
        DebugLog.log("error creating Project " + info.Name + " " + e.ToString(), enLogType.ERROR);
      }
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
            Directory.Exists(_selectedDir + "/" + AppParams.DIR_WAVS))
        {
          _wavSubDir =AppParams.DIR_WAVS;
          initSpeciesList();
        }
        else
          _wavSubDir = "";
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
            DebugLog.log("error removing files from project: " + name + ", " + ex.ToString(), enLogType.ERROR);
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

    private static string[] splitWav(string fName, double size)
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
    /// <param name="dir">directory info of prj directory</param>
    /// <param name="wavSubDir">subdir with leading '/' for wav files</param>
    public void fillFromDirectory(DirectoryInfo dir, string wavSubDir = "/", string notes = "")
    {
      try
      {
        string[] files = System.IO.Directory.GetFiles(dir.FullName + wavSubDir, "*.wav",
                         System.IO.SearchOption.TopDirectoryOnly);
        List<BatExplorerProjectFileRecordsRecord> records = new List<BatExplorerProjectFileRecordsRecord>();
        _selectedDir = dir.FullName;
        for (int i = 0; i < files.Length; i++)
          addRecord(files[i], ref records);

        _batExplorerPrj = new BatExplorerProjectFile("wavs", records);
        _wavSubDir = wavSubDir;
        _ok = true;
        _prjFileName = Path.GetFileName(dir.FullName);
        _batExplorerPrj.Name = _prjFileName;
        _prjFileName = dir.FullName + "/" + _prjFileName +".bpr";
        _changed = true;
        Created = DateTime.Now.ToString(AppParams.GPX_DATETIME_FORMAT);
        Notes = notes;
        writePrjFile();
      }
      catch { }
    }

    public void createXmlInfoFiles(PrjInfo info)
    {
      bool replaceAll = false;
      gpx gpxFile = gpx.read(info.GpxFile);
      foreach (BatExplorerProjectFileRecordsRecord record in _batExplorerPrj.Records)
      {
        bool create = replaceAll;
        string fullName = _selectedDir + "/" +_wavSubDir + "/" + record.File;
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
          if (create)
          {
            DateTime time = ElekonInfoFile.getDateTimeFromFileName(fullName);
            double[] pos = new double[2];
            pos[0] = info.Latitude; 
            pos[1] = info.Longitude;
            if (gpxFile != null)
              pos = gpxFile.getPosition(time);
            ElekonInfoFile.create(fullName, pos[0], pos[1], time);
          }
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
        string fName =_selectedDir + "/" + _wavSubDir + "/" + _batExplorerPrj.Records[0].File.Replace(AppParams.EXT_WAV, AppParams.EXT_INFO);
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
