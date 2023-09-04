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
using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.IO;
using System.Linq;
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
    public bool LocSourceGpx { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public bool IsProjectFolder { get; set; } 

    public bool OverwriteLocation { get; set; }

  }


  public abstract class PrjBase
  {
    protected List<string> _speciesList;

    protected List<SpeciesInfos> _speciesInfo;

    protected Analysis _analysis;
    protected BatSpeciesRegions _batSpecRegions;

    public List<SpeciesInfos> SpeciesInfos { get { return _speciesInfo; } }
    public List<string> Species { get { return _speciesList; } }
    public Analysis Analysis { get { return _analysis; } }

    public PrjBase(List<SpeciesInfos> speciesInfo, BatSpeciesRegions batSpecRegions)
    {
      _speciesList = new List<string>();
      _speciesInfo = speciesInfo;

      _analysis = new Analysis(this.SpeciesInfos);
      _batSpecRegions = batSpecRegions;
    }

    public abstract string getFullFilePath(string path);
    public abstract BatExplorerProjectFileRecordsRecord[] getRecords(); 

    /// <summary>
    /// init the list of species valid for the project.
    /// The list is depending on the region where the first file of the project was recorded
    /// </summary>
    protected void initSpeciesList()
    {
      BatExplorerProjectFileRecordsRecord[] records = getRecords();
      if (records.Length > 0)
      {
        string fName = getFullFilePath(records[0].File);
        fName = fName.Replace(AppParams.EXT_WAV, AppParams.EXT_INFO);
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

  public class Project : PrjBase
  {
    private string _prjFileName;
    private string _wavSubDir;
    private bool _ok;
    private string _selectedDir;
    private BatExplorerProjectFile _batExplorerPrj;
    private bool _reloadInGui;

    public bool Ok { get { return _ok; } }

    public BatExplorerProjectFileRecordsRecord[] Records
    {
      get {
        if (_batExplorerPrj.Records != null)
          return _batExplorerPrj.Records;
        else
          return null;
      }
    }
    public string Name { get { return _prjFileName; } }
    public string WavSubDir { get { return _wavSubDir; } }
    public string Notes
    {
      get
      {
        return _batExplorerPrj != null ? _batExplorerPrj.Notes : "";
      }
      set
      {
        if (_batExplorerPrj != null)
        {
          _batExplorerPrj.Notes = value;
          _changed = true;
        }
      }
    }
    public string Created
    {
      get
      {
        if (_batExplorerPrj != null)
          return _batExplorerPrj.Created;
        else
          return "";
      }
      set { _batExplorerPrj.Created = value; } }
    public string PrjDir { get { return _selectedDir + "/"; } }


    public string ReportName { get { return getReportName(_selectedDir); } }

    public string SummaryName { get { return getSummaryName(_selectedDir); } }

    public bool ReloadInGui { get { return _reloadInGui; } set { _reloadInGui = value; } }


    bool _changed = false;

    public Project(BatSpeciesRegions regions, List<SpeciesInfos> speciesInfo, string wavSubDir = "")
    : base(speciesInfo, regions)
    {
      _wavSubDir = wavSubDir;
    }


    public static string getReportName(string dir)
    {
      return dir + "/" + AppParams.Inst.Models[AppParams.Inst.SelectedModel].Dir + "/" + AppParams.PRJ_REPORT;
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
          return files[0];
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
      if (File.Exists(sumName))
      {
        Csv csv = new Csv();
        csv.read(sumName, ";", true);
        int col = csv.findInCol("todo", Cols.SPECIES_MAN);
        retVal = (col == 0);
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
    /// returns the selected files according to search pattern and file creation time
    /// </summary>
    /// <param name="prjInfo"></param>
    /// <param name="searchPattern"></param>
    /// <returns>a list of file names</returns>
    private static string[] getSelectedFiles(PrjInfo prjInfo, string searchPattern)
    {
      string[] files = Directory.GetFiles(prjInfo.SrcDir, searchPattern);
      List<string> strings = new List<string>();
      foreach (string file in files)
      {
        DateTime fileTime = ElekonInfoFile.getDateTimeFromFileName(file);
        if ((prjInfo.StartTime <= fileTime) && (fileTime <= prjInfo.EndTime))
          strings.Add(file);
      }
      return strings.ToArray();
    }

    /// <summary>
    /// create one or multiple projects from a directory containing a project
    /// </summary>
    /// <param name="info">parameters to specify project</param>
    /// <param name="regions"></param>
    /// <param name="speciesInfo"></param>
    public static string[] splitPrj(PrjInfo info, BatSpeciesRegions regions, List<SpeciesInfos> speciesInfo)
    {
      string wavDir = Path.Combine(info.SrcDir, AppParams.DIR_WAVS);
      string wavSubDir = "";
      if (Directory.Exists(wavDir))
        wavSubDir = AppParams.DIR_WAVS;

      Project prjSrc = new Project(regions, speciesInfo, wavSubDir);
      string fName = Path.Combine(info.SrcDir, info.Name) + ".bpr";
      prjSrc.readPrjFile(fName);
      string[] notes = prjSrc.Notes.Split('\n');

      info.SrcDir = Path.Combine(info.SrcDir, prjSrc.WavSubDir);
      info.Landscape = notes[0];
      info.StartTime = new DateTime(1900, 1, 1);
      info.EndTime = new DateTime(2100, 1, 1);
      if (notes.Length > 1)
        info.Weather = notes[1];

      return createPrj(info, regions, speciesInfo);
    }

    /// <summary>
    /// create one or multiple projects from a directory containing WAV files
    /// </summary>
    /// <param name="info">parameters to specify project</param>
    /// <param name="regions"></param>
    /// <param name="speciesInfo"></param>
    public static string[] createPrj(PrjInfo info, BatSpeciesRegions regions, List<SpeciesInfos> speciesInfo)
    {
      List<string> retVal = new List<string>();
      try
      {
        DebugLog.log("start creating project(s): " + info.Name, enLogType.INFO);
        string[] files = getSelectedFiles(info, "._*.wav");
        gpx gpxFile = null;
        
        if(files.Length > 0)
        {
          MessageBoxResult res = MessageBox.Show(BatInspector.Properties.MyResources.msgDeleteIntermediate, 
                                                 BatInspector.Properties.MyResources.msgQuestion, 
                                                 MessageBoxButton.YesNo, MessageBoxImage.Question);
          if (res == MessageBoxResult.Yes)
          {
            foreach (string file in files)
              File.Delete(file);
          }
          else
          {
            DebugLog.log("project creation aborted", enLogType.INFO);
            return retVal.ToArray();
          }
        }

        files = getSelectedFiles(info, "*.wav");
        if(info.OverwriteLocation)
        {
          if (info.LocSourceGpx)
          {
            gpxFile = gpx.read(info.GpxFile);
            if (gpxFile == null)
            {
              DebugLog.log("gpx file not readable: " + info.GpxFile, enLogType.ERROR);
              return retVal.ToArray();
            }
          }
        }  
        
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
            {
              string[] names = Project.splitWav(f, info.MaxFileLenSec);
              if (info.IsProjectFolder)
                Project.createSplitXmls(f, names);
            }
          }

          // 2nd step create projects
          files = getSelectedFiles(info, "*.wav");
          double prjCntd = (double)files.Length / info.MaxFileCnt;
          int prjCnt = (int)prjCntd;
          if (prjCntd > prjCnt)
            prjCnt++;
          int fileCnt = files.Length / prjCnt;
          for (int p = 0; p < prjCnt; p++)
          {
            string dirName = info.Name;
            if (prjCnt > 1)
              dirName += "_" + p.ToString("D2");
            DebugLog.log("creating project " + dirName, enLogType.INFO);
            int iFirst = p * fileCnt;
            int iLast = (p + 1) * fileCnt - 1;
            char[] invalid = Path.GetInvalidPathChars();
            string fullDir = info.DstDir + "/" + dirName;
            string wavDir = fullDir + "/" + AppParams.DIR_WAVS;
            if (Directory.Exists(fullDir))
            {
              DebugLog.log("directory '" + fullDir + "' already exists, project creation aborted!", enLogType.ERROR);
              return retVal.ToArray();
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
            if (info.IsProjectFolder)
            {
              string[] xmlFiles = new string[prjFiles.Length];
              for (int i = 0; i < xmlFiles.Length; i++)
                xmlFiles[i] = prjFiles[i].Replace(".wav", ".xml");
              if (info.OverwriteLocation && info.LocSourceGpx)
                replaceLocations(xmlFiles, wavDir, gpxFile);
              else if (info.OverwriteLocation)
                replaceLocations(xmlFiles, wavDir, info.Latitude, info.Longitude);
              else
                Utils.copyFiles(xmlFiles, wavDir);
            }
            Project prj = new Project(regions, speciesInfo);
            DirectoryInfo dir = new DirectoryInfo(fullDir);
            prj.fillFromDirectory(dir, "/" + AppParams.DIR_WAVS, info.Landscape + "\n" + info.Weather);
            if (!info.IsProjectFolder)
              prj.createXmlInfoFiles(info);
            retVal.Add(dirName);
          }
        }
        else
          DebugLog.log("No WAV files in directory " + info.SrcDir, enLogType.ERROR);
      }
      catch (Exception e)
      {
        DebugLog.log("error creating Project " + info.Name + " " + e.ToString(), enLogType.ERROR);
      }
      return retVal.ToArray();
    }

    private static void replaceLocations(string[] xmlfiles, string wavDir, gpx gpxFile)
    {
      foreach(string fName in xmlfiles)
      { 
        BatRecord f = ElekonInfoFile.read(fName);
        double[] pos = gpxFile.getPosition(ElekonInfoFile.getDateTimeFromFileName(fName));
        f.GPS.Position = pos[0].ToString(CultureInfo.InvariantCulture) + " " + pos[1].ToString(CultureInfo.InvariantCulture);
        string dstName = Path.GetFileName(fName);
        dstName = Path.Combine(wavDir, dstName);
        ElekonInfoFile.write(dstName, f);
      }
    }

    private static void replaceLocations(string[] xmlfiles, string wavDir, double lat, double lon)
    {
      foreach (string fName in xmlfiles)
      {
        BatRecord f = ElekonInfoFile.read(fName);
        f.GPS.Position = lat.ToString(CultureInfo.InvariantCulture) + " " + lon.ToString(CultureInfo.InvariantCulture);
        string dstName = Path.GetFileName(fName);
        dstName = Path.Combine(wavDir, dstName);
        ElekonInfoFile.write(dstName, f);
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
          _wavSubDir = AppParams.DIR_WAVS;
        else
          _wavSubDir = "";
        initSpeciesList();
        _ok = true;
        _changed = false;
      }
      catch (Exception ex)
      {
        DebugLog.log("error reading project file: " + fName + " :" + ex.ToString(), enLogType.ERROR);
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
      string destDir = Path.Combine(this.PrjDir, AppParams.DIR_DEL);
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
          catch (Exception ex)
          {
            DebugLog.log("error removing files from project: " + name + ", " + ex.ToString(), enLogType.ERROR);
          }
        }
        else
          newList.Add(rec);
      }
      _batExplorerPrj.Records = newList.ToArray();
      writePrjFile();
      _reloadInGui = true;
    }

    /// <summary>
    /// find a file in the project
    /// </summary>
    /// <param name="fileName">name of the file (full path or just file name)</param>
    /// <returns>record containing the file information</returns>
    public BatExplorerProjectFileRecordsRecord find(string fileName)
    {
      BatExplorerProjectFileRecordsRecord retVal = null;
      foreach (BatExplorerProjectFileRecordsRecord r in _batExplorerPrj.Records)
      {
        if (fileName.Contains(r.File))
        {
          retVal = r;
          break;
        }
      }
      return retVal;
    }


    public void addFiles(string[] files, bool removeSrc = false)
    {
      List<BatExplorerProjectFileRecordsRecord> list = new List<BatExplorerProjectFileRecordsRecord>();
      foreach (BatExplorerProjectFileRecordsRecord rec in _batExplorerPrj.Records)
        list.Add(rec);
      foreach (string file in files)
      {
        addRecord(file, ref list);
        if (_analysis != null)
        {
          WavFile wFile = new WavFile();
          wFile.readFile(file);
          _analysis.addFile(ReportName, file, (int)wFile.FormatChunk.Frequency,
                            (double)wFile.AudioSamples.Length / wFile.FormatChunk.Frequency);
        }
      }

      _batExplorerPrj.Records = list.ToArray();
      Array.Sort(_batExplorerPrj.Records);

      string dstPath = Path.Combine(PrjDir, _wavSubDir);
      List<string> fileList = new List<string>();
      foreach (string file in files)
      {
        fileList.Add(file);
        string xmlFile = file.Replace(".wav", ".xml");
        if (File.Exists(xmlFile))
          fileList.Add(xmlFile);
        string pngFile = file.Replace(".wav", ".png");
        if (File.Exists(pngFile))
          fileList.Add(pngFile);
      }
      Utils.copyFiles(fileList.ToArray(), dstPath, removeSrc);
      writePrjFile();
      _analysis?.save(_selectedDir, Notes);
      _reloadInGui = true;
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



    private static void createSplitXmls(string fName, string[] newNames)
    {
      fName = fName.Replace(".wav", ".xml");
      foreach (string newName in newNames)
      {
        string xmlName = newName.Replace(".wav", ".xml");
        File.Copy(fName, xmlName, true);
      }
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
        for (int i = 0; i < cnt; i++)
        {
          int iStart = (int)((double)i * wav.FormatChunk.Frequency * size);
          int iEnd = iStart + (int)((double)wav.FormatChunk.Frequency * size) - 1;
          if (iEnd > (wav.AudioSamples.Length - 1))
            iEnd = wav.AudioSamples.Length - 1;
          short[] samples = new short[iEnd - iStart + 1];
          Array.Copy(wav.AudioSamples, iStart, samples, 0, iEnd - iStart + 1);
          WavFile newWav = new WavFile();
          newWav.createFile(wav.FormatChunk.Channels, (int)wav.FormatChunk.Frequency, 0, samples.Length - 1, samples);
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
        _prjFileName = dir.FullName + "/" + _prjFileName + ".bpr";
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
        string fullName = _selectedDir + "/" + _wavSubDir + "/" + record.File;
        if (File.Exists(fullName))
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

    public void recovery(bool delFiles, bool changedFiles)
    {
      if (changedFiles)
      {
        string srcDir = Path.Combine(PrjDir, AppParams.DIR_ORIG);
        string dstDir = Path.Combine(PrjDir, WavSubDir);
        DirectoryInfo dir = new DirectoryInfo(srcDir);
        foreach (FileInfo file in dir.GetFiles())
        {
          string dstFile = Path.Combine(dstDir, file.Name);
          if (File.Exists(dstFile))
            File.Delete(dstFile);
          File.Copy(file.FullName, dstFile);
          File.Delete(file.FullName);
        }
        DebugLog.log("original files of Prj '" + Name + "' recovered", enLogType.INFO);
      }
      if (delFiles)
      {
        string srcDir = Path.Combine(PrjDir, AppParams.DIR_DEL);
        DirectoryInfo dir = new DirectoryInfo(srcDir);
        List<string> files = new List<string>();
        foreach (FileInfo file in dir.GetFiles("*.wav"))
          files.Add(file.FullName);
        addFiles(files.ToArray(), true);
        DebugLog.log("deleted files of Prj '" + Name + "' recovered", enLogType.INFO);
      }
    }

    public override string getFullFilePath(string wavName)
    {
      string retVal = Path.Combine(_selectedDir, _wavSubDir, wavName);
      return retVal;
    }

    public override BatExplorerProjectFileRecordsRecord[] getRecords() 
    {
      return _batExplorerPrj.Records;
    }
  }
}
