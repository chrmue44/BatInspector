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
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;

//using System.Windows.Forms;
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
    public string Notes { get; set; }
    public bool CorrectMic { get; set; } = false;
    public string GpxFile { get; set; }
    public bool LocSourceGpx { get; set; }
    public bool LocSourceKml { get; set; }
    public bool LocSourceTxt { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public bool IsProjectFolder { get; set; } 
    public bool OverwriteLocation { get; set; }
    public bool RemoveSource { get; set; }
    public string WavSubDir { get; set; } = AppParams.DIR_WAVS;
    public ModelParams ModelParams { get; set; }

    public string Location { get; set; }
    public string Creator { get; set; }
  }



  public abstract class PrjBase
  {
    protected List<string> _speciesList;

    protected Analysis[] _analysis;
    protected ModelParams _modelParams;

    public List<string> Species { get { return _speciesList; } }
    public Analysis Analysis { get { return _analysis[SelectedModelIndex]; } }
    public int SelectedModelIndex { get; set; } = 0;
    public bool IsBirdPrj {  get { return _modelParams.Type == enModel.BIRDNET; } }

    static protected readonly XmlSerializer PrjSerializer = new XmlSerializer(typeof(BatExplorerProjectFile));
    public PrjBase(bool updateCtls, ModelParams modelParams, int modelCount)
    {
      _speciesList = new List<string>();
      _analysis = new Analysis[modelCount];
      for (int i = 0; i < _analysis.Length; i++)
        _analysis[i] = new Analysis(updateCtls, enModel.BAT_DETECT2);
      _modelParams = modelParams;
    }

    public abstract string getFullFilePath(string path);
    public abstract PrjRecord[] getRecords();

    public ModelParams SelectedModelParams { get { return _modelParams; } set { _modelParams = value; } }

    public PrjRecord findRecord(string wavName)
    {
      PrjRecord retVal = null;
      PrjRecord[] records = getRecords();
      foreach(PrjRecord rec in records)
      {
        if(rec.File.ToLower().IndexOf(wavName.ToLower()) >= 0)
        {
          retVal = rec;
          break;
        }
      }
      return retVal;
    }

    /// <summary>
    /// init the list of species valid for the project.
    /// The list is depending on the region where the first file of the project was recorded
    /// </summary>
    protected void initSpeciesList()
    {
      PrjRecord[] records = getRecords();
      if (records.Length > 0)
      {
        string fName = getFullFilePath(records[0].File);
        fName = fName.ToLower().Replace(AppParams.EXT_WAV, AppParams.EXT_INFO);
        BatRecord info = ElekonInfoFile.read(fName);
        ElekonInfoFile.parsePosition(info, out double lat, out double lon);        
        _speciesList = createSpeciesList(lat,lon);
      }
    }

    public static List<string> createSpeciesList(double lat, double lon)
    {
      List<string> speciesList = new List<string>();
      ParRegion reg = App.Model.Regions.findRegion(lat, lon);
      if (reg != null)
      {
        foreach (string sp in reg.Species)
          speciesList.Add(sp);
      }
      else
      {
        foreach (SpeciesInfos sp in App.Model.SpeciesInfos)
          speciesList.Add(sp.Abbreviation);
      }
      speciesList.Add("?");
      speciesList.Add("Social");
      speciesList.Add("todo");
      return speciesList;
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
    private string _extension;
    bool _changed = false;

    public bool Ok { get { return _ok; } }

    public PrjRecord[] Records
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
      set 
      {
        if (_batExplorerPrj != null)
        { 
          _batExplorerPrj.Created = value;
          _changed = true;
        }
      }
    }

    public string CreateBy
    {
      get
      {
        if (_batExplorerPrj != null)
          return _batExplorerPrj.CreatedBy;
        else
          return "";
      }
      set 
      {
        if (_batExplorerPrj != null)
        {     
          _batExplorerPrj.CreatedBy = value;
          _changed = true;
        }
      }
    }

    public string Location
    {
      get
      {
        if (_batExplorerPrj != null)
          return _batExplorerPrj.Location;
        else
          return "";
      }
      set
      {
        if (_batExplorerPrj != null)
        {
          _batExplorerPrj.Location = value;
          _changed = true;
        }
      }
    }


    public string PrjDir { get { return _selectedDir; } }

    public ModelParams[] AvailableModelParams { get { return _batExplorerPrj.Models; } }

    public string ReportName { get { return getReportName(SelectedModelIndex); } }

    public string SummaryName { get { return getSummaryName(SelectedModelIndex); } }

    public bool ReloadInGui { get { return _reloadInGui; } set { _reloadInGui = value; } }

    public string PrjId 
    {
      get 
      {
        string retVal = "";
        if ((Analysis !=  null) && (Analysis.Files != null) && (Analysis.Files.Count  > 0))
        {
            string xmlName = Path.Combine(PrjDir,WavSubDir,
                                   Analysis.Files[0].Name.ToLower().Replace(AppParams.EXT_WAV, AppParams.EXT_INFO));
          BatRecord r = ElekonInfoFile.read(xmlName);
          bool ok = DateTime.TryParse(r.DateTime, out DateTime t);
          if (ok)
            retVal = $"{r.SN}_{t.Year}{t.Month.ToString("00")}{t.Day.ToString("00")}";
        }
        return retVal;
      } 
    }



    public string DeviceId
    {
      get
      {
        string retVal = "";
        if ((Analysis != null) && (Analysis.Files != null) && (Analysis.Files.Count > 0))
        {
          string xmlName = Path.Combine(PrjDir, WavSubDir,
                                 Analysis.Files[0].Name.ToLower().Replace(AppParams.EXT_WAV, AppParams.EXT_INFO));
          BatRecord r = ElekonInfoFile.read(xmlName);
          retVal = r.SN;
        }
        return retVal;
      }
    }

    public string MicId
    {
      get
      {
        if ((_batExplorerPrj != null) && (_batExplorerPrj.Microphone != null))
          return _batExplorerPrj.Microphone.Id;
        else
          return null;

      }
    }


    public FreqResponseRecord[] MicFreqResponse
    {
      get
      {
        if ((_batExplorerPrj != null) && (_batExplorerPrj.Microphone != null))
          return _batExplorerPrj.Microphone.FrequencyResponse;
        else
          return null;
      }
    }

    public Project(bool updateCtls, ModelParams modelParams, int modelCount, string wavSubDir = "")
    : base(updateCtls, modelParams, modelCount)
    {
      _wavSubDir = wavSubDir;
      _extension = AppParams.EXT_BATSPY;
    }

    public string getAnnotationDir()
    {
      return Path.Combine(PrjDir, _modelParams.SubDir, AppParams.ANNOTATION_SUBDIR);
    }

    public string getReportName(int modelIndex)
    {
      if ((modelIndex >= 0) && (modelIndex < AvailableModelParams.Length))
      {
        ModelParams mp = AvailableModelParams[modelIndex];
        if ((mp.Type == enModel.BAT_DETECT2) && (mp.DataSet == ModelBatDetect2.BD2_DEFAULT_MODEL))
          return Path.Combine(PrjDir, mp.SubDir, AppParams.PRJ_REPORT);
        else
          return Path.Combine(PrjDir, mp.SubDir, $"report_{mp.Name}_{mp.DataSet}.csv");
      }
      else
        DebugLog.log($"model index {modelIndex} out of range  ( 0.. {AvailableModelParams.Length - 1}", enLogType.ERROR);
      return "";
    }

    public static string getReportName(string dir, ModelParams mp, string dataSet)
    {
        if ((mp.Type == enModel.BAT_DETECT2) && (mp.DataSet == ModelBatDetect2.BD2_DEFAULT_MODEL))
          return Path.Combine(dir, mp.SubDir, AppParams.PRJ_REPORT);
        else
          return Path.Combine(dir, mp.SubDir, $"report_{mp.Name}_{dataSet}.csv");
    }

    public string getSummaryName(int modelIndex)
    {
      if ((modelIndex >= 0) && (modelIndex < AvailableModelParams.Length))
      {
        ModelParams mp = AvailableModelParams[modelIndex];
        if ((mp.Type == enModel.BAT_DETECT2) && (mp.DataSet == ModelBatDetect2.BD2_DEFAULT_MODEL))
          return Path.Combine(PrjDir, mp.SubDir, AppParams.PRJ_SUMMARY);
        else
          return Path.Combine(PrjDir, mp.SubDir, $"summary_{mp.Name}_{mp.DataSet}.csv");
      }
      else
        DebugLog.log($"model index {modelIndex} out of range  ( 0.. {AvailableModelParams.Length - 1}", enLogType.ERROR);
      return "";
    }
    public static string getSummaryName(string dir, ModelParams mp, string dataSet)
    {
      if ((mp.Type == enModel.BAT_DETECT2) && (mp.DataSet == ModelBatDetect2.BD2_DEFAULT_MODEL))
        return Path.Combine(dir, mp.SubDir, AppParams.PRJ_SUMMARY);
      else
        return Path.Combine(dir, mp.SubDir, $"summary_{mp.Name}_{dataSet}.csv");
    }

    public static string containsProject(string dir)
    {
      DirectoryInfo d = new DirectoryInfo(dir);
      return containsProject(d);
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
        else
        {
          files = System.IO.Directory.GetFiles(dir.FullName, "*" + AppParams.EXT_BATSPY,
                  System.IO.SearchOption.TopDirectoryOnly);

          if (files.Length > 0)
            return files[0];
        }
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
    public static string containsReport(DirectoryInfo dir, string repName, ModelParams modelParams)
    {
      string dirName = Path.Combine(dir.FullName, modelParams.SubDir); 
      if (Directory.Exists(dirName))
      {
        try
        {
          string[] files = System.IO.Directory.GetFiles(dirName, "*" + AppParams.EXT_CSV,
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
    public static bool evaluationDone(DirectoryInfo dir, ModelParams[] mp)
    {
      bool retVal = false;
      string sumName = "";
      string reportName = "";
      if (mp != null)
      {
        for (int i = 0; i < mp.Length; i++)
        {
          if (mp[i].Enabled)
          {
            sumName = getSummaryName(dir.FullName, mp[i], mp[i].DataSet);
            reportName = getReportName(dir.FullName, mp[i], mp[i].DataSet);
          }
        }
      }
      // legacy format
      else
      {
        sumName = Path.Combine(dir.FullName, "bd2", AppParams.SUM_REPORT);
        reportName = Path.Combine(dir.FullName, "bd2", AppParams.PRJ_REPORT);
      }

      if (File.Exists(sumName))
      {
        Csv csv = new Csv();
        csv.read(sumName, ";", true);
        int col = csv.findInCol("todo", Cols.SPECIES_MAN);
        retVal = (col == 0);
      }
      else if (File.Exists(reportName))
      {
        Csv csv = new Csv();
        {
          csv.read(reportName, ";", true);
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

    public void assignNewModelParams(ModelParams[] pars)
    {
      if(_batExplorerPrj != null)
        _batExplorerPrj.Models = pars;
    }

    public int applyFilter(Filter filter, FilterItem filterItem)
    {
      int retVal = 0;
      if(Analysis?.Files.Count > 0)
      {
        foreach(PrjRecord rec in Records)
        {
          AnalysisFile f = Analysis.find(rec.File);
          if (f != null)
          {
            rec.Selected = filter.apply(filterItem, f);
          }
          else
            rec.Selected = false;
        }
      }
      return retVal;
    }


    public void exportFiles(string outputDir, bool withXml = true, bool withPng = true)
    {
      int countWav = 0;
      if (Directory.Exists(outputDir))
      {
        DebugLog.log($"start files export from Project {Name}", enLogType.INFO);
        foreach (PrjRecord rec in Records)
        {
          if (rec.Selected)
          {
            string srcWavName = Path.Combine(PrjDir, WavSubDir, rec.File);
            string srcXmlName = srcWavName.ToLower().Replace("wav", "xml");
            string srcPngName = srcWavName.ToLower().Replace("wav", "png");
            string dstWavName = Path.Combine(outputDir, Path.GetFileName(rec.File));
            string dstXmlName = Path.Combine(outputDir, Path.GetFileNameWithoutExtension(rec.File)) + ".xml";
            string dstPngName = Path.Combine(outputDir, Path.GetFileNameWithoutExtension(rec.File)) + ".png";
            if (File.Exists(srcWavName))
            {
              File.Copy(srcWavName, dstWavName, true);
              countWav++;
            }
            if (withXml && File.Exists(srcXmlName))
              File.Copy(srcXmlName, dstXmlName, true);
            if (withPng && File.Exists(srcPngName))
              File.Copy(srcPngName, dstPngName, true);
          }
        }
        DebugLog.log($"{countWav} files exported from project {Name}", enLogType.INFO);
      }
      else
        DebugLog.log("could not find target directory " + outputDir, enLogType.ERROR);
    }


    public bool checkModelType()
    {
      bool retVal = (_batExplorerPrj.ProjectType == "Birds") && (AvailableModelParams[SelectedModelIndex].Type == enModel.BIRDNET) ||
                    (_batExplorerPrj.ProjectType != "Birds") && (AvailableModelParams[SelectedModelIndex].Type != enModel.BIRDNET);
      return retVal;
    }

    /// <summary>
    /// select the default model according to project type and location
    /// </summary>
    public void SelectDefaultModel()
    {
      if (_batExplorerPrj.ProjectType == "Birds")
      {
        _modelParams = ModelParams.GetModelParams(App.Model.getClassifier(enModel.BIRDNET).Name,
                                   App.Model.DefaultModelParams);
        _modelParams.DataSet = "WorldWide";
        SelectedModelIndex = App.Model.getModelIndex(enModel.BIRDNET);
      }
      else
      {
        double lat = 0;
        double lon = 0;
        if (Records.Length > 0)
        {
          string infoFileName = Records[0].File.ToLower().Replace(AppParams.EXT_WAV, AppParams.EXT_INFO);
          infoFileName = Path.Combine(_selectedDir, _wavSubDir, infoFileName);
          BatRecord r = ElekonInfoFile.read(infoFileName);
          ElekonInfoFile.parsePosition(r.GPS.Position, out lat, out lon);
        }
        _modelParams = ModelParams.GetModelParams(App.Model.getClassifier(enModel.BAT_DETECT2).Name,
                                   App.Model.DefaultModelParams);
        SelectedModelIndex = App.Model.getModelIndex(enModel.BAT_DETECT2);
        if (App.Model.Regions.IsInRegion(lat, lon, "Deutschland"))
          _modelParams.DataSet = BaseModel.BD2_MODEL_GERMAN;
        else if (App.Model.Regions.IsInRegion(lat, lon, "United Kingdom"))
          _modelParams.DataSet = BaseModel.BD2_MODEL_UK;
        else
          _modelParams.DataSet = BaseModel.BD2_MODEL_GERMAN;
      }
      foreach (ModelParams m in AvailableModelParams)
        m.Enabled = false;
      AvailableModelParams[SelectedModelIndex].Enabled = true;
    }


    /// <summary>
    /// returns the selected files according to search pattern and file creation time
    /// </summary>
    /// <param name="prjInfo"></param>
    /// <param name="searchPattern"></param>
    /// <returns>a list of file names</returns>
    private static string[] getSelectedFiles(PrjInfo prjInfo, string searchPattern)
    {
      string wavDir = prjInfo.IsProjectFolder ? Path.Combine(prjInfo.SrcDir, prjInfo.WavSubDir) : prjInfo.SrcDir;
      string[] files = Directory.GetFiles(wavDir, searchPattern);
      List<string> strings = new List<string>();
      foreach (string file in files)
      {
        DateTime fileTime = ElekonInfoFile.getDateTimeFromFileName(file);
        if ((prjInfo.StartTime <= fileTime) && (fileTime <= prjInfo.EndTime))
          strings.Add(file);
      }
      return strings.ToArray();
    }

 
    /*
    private static bool removeAppleTempFiles(PrjInfo info)
    {
      bool retVal = true;
      string[] files = getSelectedFiles(info, "._*.wav");
      if (files.Length > 0)
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
          retVal = false;
        }
      }
      return retVal;
    }
    */


    private static bool readGpxFile(PrjInfo info, out gpx gpxFile)
    {
      bool retVal = true;
      gpxFile = null;
      // read gpx file if needed
      if (info.OverwriteLocation)
      {
        if (info.LocSourceGpx)
        {
          gpxFile = gpx.read(info.GpxFile);
          if (gpxFile == null)
          {
            DebugLog.log("gpx file not readable: " + info.GpxFile, enLogType.ERROR);
            retVal = false;
          }
        }
      }
      return retVal;
    }

    private static bool readKmlFile(PrjInfo info, out kml kmlFile)
    {
      bool retVal = true;
      kmlFile = null;
      // read gpx file if needed
      if (info.OverwriteLocation)
      {
        if (info.LocSourceKml)
        {
          kmlFile = kml.read(info.GpxFile);
          if (kmlFile == null)
          {
            DebugLog.log("kml file not readable: " + info.GpxFile, enLogType.ERROR);
            retVal = false;
          }
        }
      }
      return retVal;
    }

    private static bool readLoctxtFile(PrjInfo info, LocFileSettings pars, out LocFileTxt txtFile)
    {
      bool retVal = true;
      txtFile = null;
      // read gpx file if needed
      if (info.OverwriteLocation)
      {
        if (info.LocSourceTxt)
        {
          txtFile = LocFileTxt.read(info.GpxFile, pars);
          if (txtFile == null)
          {
            DebugLog.log("txt location file not readable: " + info.GpxFile, enLogType.ERROR);
            retVal = false;
          }
        }
      }
      return retVal;

    }

    private static bool createPrjDirStructure(string prjDir, out string wavDir, string wavSubDir)
    {
      bool retVal = true;
      DebugLog.log("creating project " + prjDir, enLogType.INFO);
      if (string.IsNullOrEmpty(wavSubDir))
        wavDir = prjDir;
      else
        wavDir =Path.Combine(prjDir,wavSubDir);
      if (Directory.Exists(prjDir))
      {
        DebugLog.log("directory '" + prjDir + "' already exists, project creation of folder structure aborted!", enLogType.WARNING);
        retVal = false;
      }
      else
        Directory.CreateDirectory(prjDir);
      if (!Directory.Exists(wavDir))
        Directory.CreateDirectory(wavDir);
      return retVal;
    }


    private static string[] findSoundFiles(PrjInfo info, out string extension)
    {
      extension = "*.wav";
      string[] retVal = getSelectedFiles(info, extension);
      if(retVal.Length == 0)
      {
        extension = "*.raw";
        retVal = getSelectedFiles(info, extension);
      }
      return retVal;
    }

    private static void importRawFiles(string[] files, string wavDir, PrjInfo info)
    {
      RawImportParams pars = new RawImportParams()
      {
        SampleRate = 500000,
        Channels = 1,
        BitsPerSample = 16
      };

      foreach (string file in files)
      {
        string name = Path.GetFileName(file);
        string dest = Path.Combine(wavDir, name);
        dest = dest.Replace(".raw", AppParams.EXT_WAV);
        WavFile wavFile = new WavFile();
        wavFile.importRaw(file, pars);
        if (wavFile.AudioSamples.Length > 2048)
        {
          wavFile.saveFileAs(dest);
          DateTime time = ElekonInfoFile.getDateTimeFromFileName(file);
          double[] pos = new double[2];
          pos[0] = info.Latitude;
          pos[1] = info.Longitude;
          ElekonInfoFile.create(dest, pos[0], pos[1], time);
        }
        else
          DebugLog.log($"file {name} not imported because it is too short", enLogType.INFO);
      }
    }




    public static bool copyFromBatspy(PrjInfo info, ModelParams modelParams)
    {
      bool retVal = false;
      try
      {
        DebugLog.log("start copying project(s): " + info.Name, enLogType.INFO);
        string[] files = findSoundFiles(info, out string soundFileExtension);
        if (files.Length > 0)
        {
          DebugLog.log("copy wav files...", enLogType.INFO);
          string fullDir = Path.Combine(info.DstDir, info.Name);
          createPrjDirStructure(fullDir, out string wavDir, info.WavSubDir);
          Utils.copyFiles(files, wavDir, info.RemoveSource);
          string[] xmlFiles = getSelectedFiles(info, "*.xml");
          DebugLog.log("copy xml files...", enLogType.INFO);
          Utils.copyFiles(xmlFiles, wavDir, info.RemoveSource);

          files = System.IO.Directory.GetFiles(info.SrcDir, "*" + AppParams.EXT_BATSPY,
                 System.IO.SearchOption.TopDirectoryOnly);
          if (files.Length > 0)
          {
            string dstPrj = Path.Combine(fullDir, Path.GetFileName(files[0]));
            File.Copy(files[0], dstPrj, true);
          }

          Project prj = Project.createFrom(fullDir);
          prj.Location = info.Location;
          prj.CreateBy = info.Creator;
          prj.Notes = info.Notes;
          if (modelParams != null)
            prj._modelParams = modelParams;
          else
            prj.SelectDefaultModel();
          prj.writePrjFile();

          splitFiles(Path.Combine(info.DstDir, info.Name, info.WavSubDir), info);
          retVal = true;
        }
      }
      catch (Exception e)
      {
        DebugLog.log("error copying Project " + info.Name + " " + e.ToString(), enLogType.ERROR);
        retVal = false;
      }

      return retVal;
    }

    /// <summary>
    /// create one or multiple projects from a directory containing WAV files
    /// </summary>
    /// <param name="info">parameters to specify project</param>
    /// <param name="regions"></param>
    /// <param name="speciesInfo"></param>
    public static bool createPrjFromWavs(PrjInfo info, ModelParams modelParams)
    {
      Stopwatch t = new Stopwatch();
      t.Start();
      bool retVal = true;
      if ((info.MaxFileCnt == 0) || info.MaxFileLenSec == 0)
      {
        DebugLog.log("error creating project, maxFiles or maxFileLen == 0", enLogType.ERROR);
        return false;
      }
      try
      {
        DebugLog.log($"start creating project(s): {info.Name} at {t.Elapsed}", enLogType.INFO);
        string[] files;
        string soundFileExtension = "";
           
        files = findSoundFiles(info, out soundFileExtension);
        if (files.Length > 0)
        {
          bool ok = ElekonInfoFile.checkDateTimeInFileName(files[0]);
          if (!ok)
          {
            t.Stop();
            MessageBoxResult res = MessageBox.Show(BatInspector.Properties.MyResources.MsgDatTimeError,
                                                   BatInspector.Properties.MyResources.msgQuestion,
                                                   MessageBoxButton.OKCancel, MessageBoxImage.Question);
            t.Start();
            if (res != MessageBoxResult.OK)
              return false;
          }
        }        

        if (files.Length > 0)
        {
          // copy files to a single project at destination and create project
          DebugLog.log($"copy wav files... at {t.Elapsed}", enLogType.INFO);
          string fullDir = Path.Combine(info.DstDir, info.Name);
          createPrjDirStructure(fullDir, out string wavDir, info.WavSubDir);
          if (soundFileExtension == "*.wav")
            Utils.copyFiles(files, wavDir, info.RemoveSource);
          else
            importRawFiles(files, wavDir, info);
          string[] xmlFiles = getSelectedFiles(info, "*.xml");
          DebugLog.log($"copy xml files... at {t.Elapsed}", enLogType.INFO);
          Utils.copyFiles(xmlFiles, wavDir, info.RemoveSource);

          // create project
          Project prj = new Project(false, modelParams, App.Model.DefaultModelParams.Length);
          DirectoryInfo dir = new DirectoryInfo(fullDir);
          DebugLog.log($"creating project... at {t.Elapsed}", enLogType.INFO);
          prj.fillFromDirectory(dir, info.WavSubDir, info.Notes);

          // copy location files is present
          DebugLog.log($"copy location files... at {t.Elapsed}", enLogType.INFO);
          string[] gpxFiles = Directory.GetFiles(info.SrcDir, "*.gpx");
          Utils.copyFiles(gpxFiles, prj.PrjDir, info.RemoveSource);
          string[] kmlFiles = Directory.GetFiles(info.SrcDir, "*.kml");
          Utils.copyFiles(kmlFiles, prj.PrjDir, info.RemoveSource);
          string[] txtFiles = Directory.GetFiles(info.SrcDir, "*.txt");
          Utils.copyFiles(txtFiles, prj.PrjDir, info.RemoveSource);

          // for raws info files get created in importRawFiles()
          if (!info.IsProjectFolder && (soundFileExtension == "*.wav"))
          {
            DebugLog.log($"create xml info files... at {t.Elapsed}", enLogType.INFO);
            prj.createXmlInfoFiles(info);
          }
          // replace gpx locations
          xmlFiles = Directory.GetFiles(wavDir, "*.xml");
          if (info.OverwriteLocation && info.LocSourceGpx)
          {
            DebugLog.log($"replace locations from gpx file... at {t.Elapsed}", enLogType.INFO);
            bool ok = readGpxFile(info, out gpx gpxFile);
            if (ok)
              replaceLocations(xmlFiles, wavDir, gpxFile);
            else
              DebugLog.log("error reading GPX file, could not generate location information", enLogType.ERROR);
          }
          else if (info.OverwriteLocation && info.LocSourceKml)
          {
            DebugLog.log($"replace locations from kml file... at {t.Elapsed}", enLogType.INFO);
            bool ok = readKmlFile(info, out kml kmlFile);
            if (ok)
              replaceLocations(xmlFiles, wavDir, kmlFile);
            else
              DebugLog.log("error reading KML file, could not generate location information", enLogType.ERROR);
          }
          else if (info.OverwriteLocation && info.LocSourceTxt)
          {
            DebugLog.log($"replace locations from txt file... at {t.Elapsed}", enLogType.INFO);
            bool ok = readLoctxtFile(info, AppParams.Inst.LocFileSettings, out LocFileTxt txtFile);
            if (ok)
              replaceLocations(xmlFiles, wavDir, txtFile);
            else
              DebugLog.log("error reading TXT file, could not generate location information", enLogType.ERROR);
          }
          else if (info.OverwriteLocation)
          {
            DebugLog.log($"replace locations with fixed location... at {t.Elapsed}", enLogType.INFO);
            replaceLocations(xmlFiles, wavDir, info.Latitude, info.Longitude);
          }
          splitFiles(prj, info);
        }
        else
          DebugLog.log("No WAV files in directory " + info.SrcDir, enLogType.ERROR);
        // remove src project
        if (info.RemoveSource && (info.SrcDir != Path.Combine(info.DstDir, info.Name)))
          Directory.Delete(info.SrcDir, true);
        DebugLog.log($"creation of project done at {t.Elapsed}", enLogType.INFO);
      }
      catch (Exception e)
      {
        DebugLog.log("error creating Project " + info.Name + " " + e.ToString(), enLogType.ERROR);
        retVal = false; 
      }
      return retVal;
    }

    /// <summary>
    /// split all files in prj that are longer than the specified max. length
    /// </summary>
    /// <param name="prj"></param>
    /// <param name="info"></param>
    private static void splitFiles(Project prj, PrjInfo info)
    {
      DebugLog.log("split files exceeding max. length", enLogType.INFO);

      string prjWavDir = Path.Combine(prj.PrjDir, prj.WavSubDir);
      splitFiles(prjWavDir, info);
      prj.fillFromDirectory(new DirectoryInfo(prj.PrjDir), prj.WavSubDir, info.Notes);
    }

    private static void splitFiles(string prjWavDir, PrjInfo info)
    {
      string[] files = Directory.GetFiles(prjWavDir, "*" + AppParams.EXT_WAV);
      WavFile wavFile = new WavFile();
      wavFile.readFile(files[0]);
      uint sampleRate = wavFile.FormatChunk.Frequency;
      int maxFileLen = (int)((double)sampleRate * 2 * info.MaxFileLenSec + 2048);

      foreach (string f in files)
      {
        FileInfo fileInfo = new FileInfo(f);
        if (fileInfo.Length > maxFileLen)
        {
          string[] names = Project.splitWav(f, info.MaxFileLenSec);
          Project.createSplitXmls(f, names, info.MaxFileLenSec);
          string oldXml = f.Replace(AppParams.EXT_WAV, AppParams.EXT_INFO);
          if (File.Exists(oldXml))
            File.Delete(oldXml);
        }
      }

    }

    /// <summary>
    /// split a project into multiple projects
    /// </summary>
    /// <param name="prj">the project to split</param>
    public static List<string> splitProject(Project prj, int prjCnt, BatSpeciesRegions regions, ModelParams modelParams, int modelCount)
    {
      List<string> retVal = new List<string>();
      string wavSrc = Path.Combine(prj.PrjDir, prj.WavSubDir);
      string[] files = Directory.GetFiles(wavSrc, "*.wav");
      int fileCnt = files.Length / prjCnt;

      for (int p = 0; p < prjCnt; p++)
      { 
        int iFirst = p * fileCnt;
        if((p + 1) == prjCnt) 
          fileCnt = files.Length - iFirst;
        string dirName = prj.PrjDir + "_" + p.ToString("D2");
        bool ok = createPrjDirStructure(dirName, out string wavDir, prj.WavSubDir);
        if (!ok)
          return retVal;
        DebugLog.log("Creating project " + dirName, enLogType.INFO);

        string[] prjFiles = new string[fileCnt];
        Array.Copy(files, iFirst, prjFiles, 0, fileCnt);
        Utils.copyFiles(prjFiles, wavDir, true);

        string[] xmlFiles = new string[prjFiles.Length];
        for (int i = 0; i < xmlFiles.Length; i++)
          xmlFiles[i] = prjFiles[i].ToLower().Replace(AppParams.EXT_WAV, AppParams.EXT_INFO);
        Utils.copyFiles(xmlFiles, wavDir, true);

        string[] gpxFiles = Directory.GetFiles(prj.PrjDir, "*.gpx");
        Utils.copyFiles(gpxFiles, dirName);
        string[] kmlFiles = Directory.GetFiles(prj.PrjDir, "*.kml");
        Utils.copyFiles(kmlFiles, dirName);
        string[] txtFiles = Directory.GetFiles(prj.PrjDir, "*.txt");
        Utils.copyFiles(txtFiles, dirName);

        Project dstprj = new Project(false, modelParams, modelCount);
        dstprj.fillFromDirectory(new DirectoryInfo(dirName), AppParams.DIR_WAVS, prj.Notes);

        copyAnalysisPart(prj, dstprj);
        retVal.Add(dirName);
      }
      return retVal;
    }

    public static Project combineProjects(List<Project> prjs, string dir, string prjName)
    {
      if (prjs.Count == 0)
        return null;
      string destDir = Path.Combine(dir, prjName);
      bool ok = createPrjDirStructure(destDir, out string wavDir, prjs[0].WavSubDir);
      Project retVal = new Project(false, 
                                   prjs[0].SelectedModelParams, 
                                   prjs[0].AvailableModelParams.Length, prjs[0].WavSubDir);
      retVal._selectedDir = destDir;
      string[] report = new string[prjs[0].AvailableModelParams.Length];
      for (int i = 0; i < report.Length; i++)
        report[i] = "";

      foreach (Project prj in prjs)
      {
        DebugLog.log($"append project {prj.Name}", enLogType.INFO);
        if (retVal._batExplorerPrj == null)
          retVal._batExplorerPrj = prj._batExplorerPrj;
        else
          retVal._batExplorerPrj.Records = retVal._batExplorerPrj.Records.Concat(prj.Records).ToArray();
        string wavPath = Path.Combine(prj.PrjDir, prj.WavSubDir);
        string wavDest = Path.Combine(retVal.PrjDir, retVal.WavSubDir);
        Utils.CopyFolder(wavPath, wavDest);
        for(int i = 0; i < prjs[0].AvailableModelParams.Length; i++)
        {
          
          string modPath = Path.Combine(prj.PrjDir, prj.AvailableModelParams[i].SubDir);
          string modDest = Path.Combine(retVal.PrjDir, prj.AvailableModelParams[i].SubDir);
          if(Directory.Exists(modPath))
            Utils.CopyFolder(modPath, modDest);
          appendToReport(ref report[i], prj.getReportName(i));
        }
      }
      retVal._changed = true;
      retVal._prjFileName = Path.Combine(retVal.PrjDir, prjName + AppParams.EXT_BATSPY);
      retVal.writePrjFile();
      for (int i = 0; i < prjs[0].AvailableModelParams.Length; i++)
      {
        if(report[i] != "")
          File.WriteAllText(retVal.getReportName(i), report[i]);
      }
      retVal.Analysis.read(retVal.ReportName, retVal.AvailableModelParams);
      retVal.Analysis.createSummary(retVal.SummaryName, "");
      return retVal;
    }

    private static void appendToReport(ref string report, string reportName)
    {
      if (File.Exists(reportName))
      {
        if (report == "")
          report = File.ReadAllText(reportName);
        else
        {
          string[] lines = File.ReadAllLines(reportName);
          StringBuilder str = new StringBuilder();
          for (int i = 1; i < lines.Length; i++)
            str.AppendLine(lines[i]);
          report += str;
        }
      }
    }
    private static void copyAnalysisPart(Project prjSrc, Project prjDest)
    {
      foreach(PrjRecord rec in prjDest.Records)
      {
        AnalysisFile f = prjSrc._analysis[prjSrc.SelectedModelIndex].getAnalysis(rec.File);
        if(f != null)
        {
          prjDest.Analysis.addFile(f, true);
        }
      }
      string dir = Path.GetDirectoryName(prjDest.getReportName(prjDest.SelectedModelIndex));
      if (!Directory.Exists(dir))
        Directory.CreateDirectory(dir);
      prjDest.Analysis.save(prjDest.getReportName(prjDest.SelectedModelIndex), prjDest.Notes, prjDest.SummaryName);
    }

    private static void replaceLocations(string[] xmlfiles, string wavDir, gpx gpxFile)
    {
      DebugLog.log("replace locations from gpx file...", enLogType.INFO);
      foreach (string fName in xmlfiles)
      { 
        BatRecord f = ElekonInfoFile.read(fName);
        DateTime t = ElekonInfoFile.getDateTimeFromFileName(fName);
        double[] pos = gpxFile.getPosition(t);
        if ((pos == null) || (pos.Length < 2) || ((pos[0] == 0.0) && (pos[1] == 0.0)))
          DebugLog.log("no position found for " + fName + ", timestamp: " + t.ToString(), enLogType.ERROR);
        f.GPS.Position = pos[0].ToString(CultureInfo.InvariantCulture) + " " + pos[1].ToString(CultureInfo.InvariantCulture);
        string dstName = Path.GetFileName(fName);
        dstName = Path.Combine(wavDir, dstName);
        ElekonInfoFile.write(dstName, f);
      }
    }

    private static void replaceLocations(string[] xmlfiles, string wavDir, kml kmlFile)
    {
      DebugLog.log("replace locations from kml file...", enLogType.INFO);
      foreach (string fName in xmlfiles)
      {
        BatRecord f = ElekonInfoFile.read(fName);
		    double[] posOld = {90,0};
        double[] pos = kmlFile.getPosition(fName);
	      if(pos[0] < 1e-6)
	        pos = posOld;
        else
          posOld = pos;
        f.GPS.Position = pos[0].ToString(CultureInfo.InvariantCulture) + " " + pos[1].ToString(CultureInfo.InvariantCulture);
        string dstName = Path.GetFileName(fName);
        dstName = Path.Combine(wavDir, dstName);
        ElekonInfoFile.write(dstName, f);
      }
    }

    private static void replaceLocations(string[] xmlfiles, string wavDir, LocFileTxt txtFile)
    {
      DebugLog.log("replace locations from txt file...", enLogType.INFO);
      foreach (string fName in xmlfiles)
      {
        BatRecord f = ElekonInfoFile.read(fName);
        double[] posOld = { 90, 0 };
        DateTime t = AnyType.getDate(f.DateTime);
        double[] pos = txtFile.getPosition(fName, t);
        if (pos[0] < 1e-6)
          pos = posOld;
        else
          posOld = pos;
        f.GPS.Position = pos[0].ToString(CultureInfo.InvariantCulture) + " " + pos[1].ToString(CultureInfo.InvariantCulture);
        string dstName = Path.GetFileName(fName);
        dstName = Path.Combine(wavDir, dstName);
        ElekonInfoFile.write(dstName, f);
      }
    }

    private static void replaceLocations(string[] xmlfiles, string wavDir, double lat, double lon)
    {
      DebugLog.log("replace locations with fix location...", enLogType.INFO);
      foreach (string fName in xmlfiles)
      {
        BatRecord f = ElekonInfoFile.read(fName);
        f.GPS.Position = lat.ToString(CultureInfo.InvariantCulture) + " " + lon.ToString(CultureInfo.InvariantCulture);
        string dstName = Path.GetFileName(fName);
        dstName = Path.Combine(wavDir, dstName);
        ElekonInfoFile.write(dstName, f);
      }
    }

    public static ModelParams[] readModelParams(string prjFile)
    {
      ModelParams[] retVal = null;
      string xml = File.ReadAllText(prjFile);
      if (Path.GetExtension(prjFile) == AppParams.EXT_BATSPY)
      {
        TextReader reader = new StringReader(xml);
        BatExplorerProjectFile pf = (BatExplorerProjectFile)PrjSerializer.Deserialize(reader);
        retVal = pf.Models;
      }
      return retVal;
    }

    public static Project createFrom(string prjDir)
    {
      Project retVal = new Project(false,
                                   App.Model.DefaultModelParams[App.Model.getModelIndex(AppParams.Inst.DefaultModel)], 
                                   App.Model.DefaultModelParams.Length);
      retVal.readPrjFile(prjDir);
      retVal._selectedDir = prjDir;
      return retVal;
    }

    public void readPrjFile(string prjDir)
    {
      try
      {
        string[] files = System.IO.Directory.GetFiles(prjDir, "*" + AppParams.EXT_BATSPY,
                   System.IO.SearchOption.TopDirectoryOnly);
        if (files.Length == 0)
        {
          files =  System.IO.Directory.GetFiles(prjDir, "*" + AppParams.EXT_PRJ,
                     System.IO.SearchOption.TopDirectoryOnly);
          _extension = AppParams.EXT_PRJ;
        }
        if (files.Length > 0)
        {
          _prjFileName = files[0];
          _selectedDir = prjDir;
          string xml = File.ReadAllText(_prjFileName);
          if(_extension == AppParams.EXT_PRJ)
            _prjFileName = Path.Combine(Path.GetDirectoryName(_prjFileName), Path.GetFileNameWithoutExtension(_prjFileName) + AppParams.EXT_BATSPY);
          _speciesList.Clear();
          TextReader reader = new StringReader(xml);
          _batExplorerPrj = (BatExplorerProjectFile)PrjSerializer.Deserialize(reader);
          if (_batExplorerPrj.Created == null)
            _batExplorerPrj.Created = "";
          if (_batExplorerPrj.Notes == null)
            _batExplorerPrj.Notes = "";
          if (string.IsNullOrEmpty(_batExplorerPrj.CreatedBy))
            _batExplorerPrj.CreatedBy = "insert name of person";
          if (_batExplorerPrj.Models == null)
            _batExplorerPrj.Models = App.Model.DefaultModelParams;  
          if (Directory.Exists(Path.Combine(_selectedDir, AppParams.DIR_WAVS)))
            _wavSubDir = AppParams.DIR_WAVS;
          else
            _wavSubDir = "";

          _modelParams = setModelParams();
          for (int i = 0; i < AvailableModelParams.Length; i++)
          {
            if (AvailableModelParams[i].Enabled)
            {
              SelectedModelIndex = i;
              break;
            }
          }
          DirectoryInfo dir = new DirectoryInfo(Path.Combine(_selectedDir, _wavSubDir));
          FileInfo[] wavFiles = dir.GetFiles("*.wav");
          _batExplorerPrj.Records = new PrjRecord[wavFiles.Length];
          for (int i = 0; i < wavFiles.Length; i++)
          {
            _batExplorerPrj.Records[i] = new PrjRecord(wavFiles[i].Name);
          }

          initSpeciesList();
          _ok = true;
          _changed = false;
        }
      }
      catch (Exception ex)
      {
        DebugLog.log("error reading project file: " + prjDir + " :" + ex.ToString(), enLogType.ERROR);
        _ok = false;
      }
    }

    private ModelParams setModelParams()
    {
      ModelParams retVal = null;
      foreach(ModelParams m in _batExplorerPrj.Models)
      {
        if(m.Enabled)
        {
          retVal = m;
          break;
        }
      }
      if (retVal == null)
        DebugLog.log("open Project: no AI clasifier enabled!", enLogType.ERROR);
      return retVal;
    }

    public void writePrjFile()
    {
      if ((_batExplorerPrj != null) && _changed)
      {
        _batExplorerPrj.FileVersion = "3";
        TextWriter writer = new StreamWriter(_prjFileName);
        PrjSerializer.Serialize(writer, _batExplorerPrj);
        writer.Close();
        _changed = false;
        DebugLog.log("project '" + _prjFileName + "' saved", enLogType.INFO);
      }
    }

    public void removeFile(string wavName)
    {
      List<PrjRecord> list = _batExplorerPrj.Records.ToList();
      foreach (PrjRecord rec in list)
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
      List<PrjRecord> newList = new List<PrjRecord>();
      string destDir = Path.Combine(this.PrjDir, AppParams.DIR_DEL);
      if (!Directory.Exists(destDir))
        Directory.CreateDirectory(destDir);

      foreach (PrjRecord rec in _batExplorerPrj.Records)
      {
        if (Analysis.find(rec.File) == null)
        {
          string name = Path.Combine(PrjDir, WavSubDir, rec.File);
          DebugLog.log("delete file " + name, enLogType.DEBUG);
          try
          {
            lock (rec)
            {
              if (File.Exists(name))
                File.Move(name, Path.Combine(destDir, Path.GetFileName(name)));
              name = name.ToLower().Replace(AppParams.EXT_WAV, AppParams.EXT_INFO);
              if (File.Exists(name))
                File.Move(name, Path.Combine(destDir, Path.GetFileName(name)));
              name = name.Replace(AppParams.EXT_INFO, AppParams.EXT_IMG);
              if (File.Exists(name))
                File.Move(name, Path.Combine(destDir, Path.GetFileName(name)));
              _changed = true;
            }
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
    }


    /// <summary>
    /// find a file in the project
    /// </summary>
    /// <param name="fileName">name of the file (full path or just file name)</param>
    /// <returns>record containing the file information</returns>
    public PrjRecord find(string fileName)
    {
      PrjRecord retVal = null;
      if (fileName != null)
      {
        foreach (PrjRecord r in _batExplorerPrj.Records)
        {
          if (fileName.ToLower().Contains(r.File.ToLower()))
          {
            retVal = r;
            break;
          }
        }
      }
      return retVal;
    }


    public void addFiles(string[] files, bool removeSrc = false)
    {
      List<PrjRecord> list = new List<PrjRecord>();
      foreach (PrjRecord rec in _batExplorerPrj.Records)
        list.Add(rec);
      foreach (string file in files)
      {
        _changed = true;
        addRecord(file, ref list);
        if (_analysis[SelectedModelIndex]?.IsEmpty == false)
        {
          WavFile wFile = new WavFile();
          wFile.readFile(file);
          _analysis[SelectedModelIndex].addFile(ReportName, file, (int)wFile.FormatChunk.Frequency,
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
        string xmlFile = file.ToLower().Replace(AppParams.EXT_WAV, AppParams.EXT_INFO);
        if (File.Exists(xmlFile))
          fileList.Add(xmlFile);
      }
      Utils.copyFiles(fileList.ToArray(), dstPath, removeSrc);
      writePrjFile();
      if(_analysis[SelectedModelIndex]?.IsEmpty == false)
        _analysis[SelectedModelIndex]?.save(ReportName, Notes, SummaryName);
      _reloadInGui = true;
    }

    public void applyMicCorrection(double softening)
    {
      if (_batExplorerPrj.MicCorrected == false)
      {
        DebugLog.log("applying mic correction...", enLogType.INFO);
        foreach (PrjRecord record in _batExplorerPrj.Records)
        {
          string path = Path.Combine(_selectedDir, _wavSubDir, record.File);
          applyMicCorrection(path, softening);
        }
        _batExplorerPrj.MicCorrected = true;
        writePrjFile();
      }
      else
        DebugLog.log("mic correction already applied", enLogType.INFO);
    }


    private void applyMicCorrection(string wavFile, double softening)
    {
      try
      {
        SoundEdit result = new SoundEdit();
        result.readWav(wavFile);
        result.applyFreqResponse(_batExplorerPrj.Microphone.FrequencyResponse, softening);
        result.saveAs(wavFile, WavSubDir);
      }
      catch (Exception ex)
      {
        DebugLog.log($"error applying mic correction to {wavFile}, {ex.ToString()}", enLogType.ERROR);
      }
    }


    private void addRecord(string filePath, ref List<PrjRecord> list)
    {
      PrjRecord rec = new PrjRecord();
      string name = Path.GetFileName(filePath);
      rec.File = name;
//      name = Path.GetFileNameWithoutExtension(filePath);
//      rec.Name = name;
      list.Add(rec);
    }



    private static void createSplitXmls(string fName, string[] newNames, double maxLen)
    {
      fName = fName.ToLower().Replace(AppParams.EXT_WAV, AppParams.EXT_INFO);
      double deltaT = 0;
      DateTime t = new DateTime(1900,1,1);
      BatRecord rec = ElekonInfoFile.read(fName);
      foreach (string newName in newNames)
      {
        string xmlName = newName.ToLower().Replace(AppParams.EXT_WAV, AppParams.EXT_INFO);
        if(rec != null)
        {
          if(t.Year < 1910)
            t = AnyType.getDate(rec.DateTime);
          t =  t.AddMilliseconds(deltaT * 1000);
          deltaT += maxLen;
          rec.DateTime = ElekonInfoFile.getDateString(t);
          ElekonInfoFile.write(xmlName, rec);
        }
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
          newWav.createFile((int)wav.FormatChunk.Frequency, 0, samples.Length - 1, samples);
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
    public void fillFromDirectory(DirectoryInfo dir, string wavSubDir = "", string notes = "")
    {
      try
      {
        string dirName = Path.Combine(dir.FullName, wavSubDir);
        string[] files = System.IO.Directory.GetFiles(dirName, "*.wav",
                         System.IO.SearchOption.TopDirectoryOnly);
        List<PrjRecord> records = new List<PrjRecord>();
        _selectedDir = dir.FullName;
        for (int i = 0; i < files.Length; i++)
          addRecord(files[i], ref records);

        _batExplorerPrj = new BatExplorerProjectFile("wavs", records);
        _wavSubDir = wavSubDir;
        _ok = true;
        _prjFileName = Path.GetFileName(dir.FullName);
        _batExplorerPrj.Name = _prjFileName;
        _prjFileName = Path.Combine(dir.FullName , _prjFileName + AppParams.EXT_BATSPY);
        _changed = true;
        Created = DateTime.Now.ToString(AppParams.GPX_DATETIME_FORMAT);
        Notes = notes;
        writePrjFile();
      }
      catch (Exception ex)
      {
        DebugLog.log("error creating prject: " + ex.ToString(), enLogType.ERROR);
      }
    }

    public void createXmlInfoFiles(PrjInfo info)
    {
      bool replaceAll = false;
      DebugLog.log("creating xml info files...", enLogType.INFO);
      bool msgBoxShown = false;
      //gpx gpxFile = gpx.read(info.GpxFile);
      foreach (PrjRecord record in _batExplorerPrj.Records)
      {
        bool create = replaceAll;
        string fullName = Path.Combine(_selectedDir, _wavSubDir, record.File);
        if (File.Exists(fullName))
        {
          if (File.Exists(fullName.ToLower().Replace(AppParams.EXT_WAV, AppParams.EXT_INFO)))
          {
            if (!msgBoxShown)
            {
              MessageBoxResult res = MessageBox.Show(BatInspector.Properties.MyResources.ProjectmsgReplaceInfo,
                                                   BatInspector.Properties.MyResources.msgQuestion,
                                                   MessageBoxButton.YesNo, MessageBoxImage.Question);
              if (res == MessageBoxResult.Yes)
              {
                replaceAll = true;
                DebugLog.log("replacing existing xml files...", enLogType.INFO);
                create = true;
              }
              else
                DebugLog.log("copy existing xml files...", enLogType.INFO);

              msgBoxShown = true;
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
       //     if (gpxFile != null)
       //       pos = gpxFile.getPosition(time);
            ElekonInfoFile.create(fullName, pos[0], pos[1], time);
          }
        }
      }
    }

    public void recovery(bool delFiles, bool changedFiles)
    {
      if (changedFiles)
      {
        ColorTable colorTable = new ColorTable();
        colorTable.createColorLookupTable();

        string srcDir = Path.Combine(PrjDir, AppParams.DIR_ORIG);
        string dstDir = Path.Combine(PrjDir, WavSubDir);
        DirectoryInfo dir = new DirectoryInfo(srcDir);
        foreach (FileInfo file in dir.GetFiles())
        {
          string wavName = "";
          if (Path.GetExtension(file.FullName).ToLower() == AppParams.EXT_CSV)
          {
            wavName = Path.GetFileNameWithoutExtension(file.FullName) + AppParams.EXT_WAV;
            _analysis[SelectedModelIndex].undo(file.FullName);
            File.Delete(file.FullName);
            _analysis[SelectedModelIndex].save(ReportName, Notes, SummaryName);
            _analysis[SelectedModelIndex].read(ReportName, AvailableModelParams);
            _analysis[SelectedModelIndex].updateControls(wavName);
          }
          else
          {
            string dstFile = Path.Combine(dstDir, file.Name);
            if (File.Exists(dstFile))
              File.Delete(dstFile);
            File.Copy(file.FullName, dstFile);
            File.Delete(file.FullName);
            if (Path.GetExtension(file.FullName).ToLower() == AppParams.EXT_WAV)
            {
              string pngName = file.FullName.Replace(AppParams.EXT_WAV, AppParams.EXT_IMG);
              ViewModel.createPng(file.FullName, pngName, AppParams.FFT_WIDTH, colorTable);
            }
          }
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

    public override PrjRecord[] getRecords() 
    {
      return _batExplorerPrj.Records;
    }

    public static bool parseLatitude(string coordStr, out double coord)
    {
      return parseGeoCoord(coordStr, out coord, "N", "S", 90);
    }


    public static bool parseLongitude(string coordStr, out double coord)
    {
      return parseGeoCoord(coordStr, out coord, "E", "W", 180);
    }



    /// <summary>
    /// parse geographical coordinates, two formats are allowed:
    /// 1.: plain double e.g. 49.657489, -33.5679864
    /// 2.: degrees and minutes e.g. "49° 38.012 N", "8° 37.443 W"
    /// </summary>
    /// <param name="coordStr">coordinat as string</param>
    /// <param name="coord">output coordinate</param>
    /// <param name="hem1">hemesphere character positive direction</param>
    /// <param name="hem2">hemisphere character negative direction</param>
    /// <param name="maxDeg">max value for degrees</param>
    /// <returns></returns>
    static bool parseGeoCoord(string coordStr, out double coord, string hem1, string hem2, int maxDeg)
    {
      bool retVal = true;
      bool ok = double.TryParse(coordStr, NumberStyles.Any, CultureInfo.InvariantCulture, out coord);
      if (ok)
      {
        if ((coord < -maxDeg) || (coord > maxDeg))
          retVal = false;
      }
      else
      {
        int deg = 0;
        int sign = 0;
        int pos = coordStr.IndexOf("°");
        if (pos >= 0)
        {
          string degStr = coordStr.Substring(0, pos);
          retVal = int.TryParse(degStr, out deg);
        }
        else
          retVal = false;
        coord = 0;
        int pos2 = coordStr.IndexOf(hem1);
        if (pos2 < 0)
        {
          pos2 = coordStr.IndexOf(hem2);
          if (pos2 >= 0)
            sign = 1;
          else
            retVal = false;
        }

        if ((deg < -maxDeg) || (deg > maxDeg))
          retVal = false;

        if (retVal)
        {
          string minStr = coordStr.Substring(pos + 1, pos2 - pos - 1);
          retVal = double.TryParse(minStr, NumberStyles.Any, CultureInfo.InvariantCulture, out double minval);
          if ((minval >= 60) || (minval < 0))
            retVal = false;
          if (retVal)
          {
            coord = deg + minval / 60;
            if (sign == 1)
              coord *= -1;
          }
        }
      }
      return retVal;
    }
  }
}
