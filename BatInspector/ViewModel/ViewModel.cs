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
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Management;
using System.Windows.Markup;
using System.Windows.Media.Imaging;


namespace BatInspector
{
  public class SpeciesItem
  {
    public string Abbreviation { get; set; }
    public string Name { get; set; }
    public double CharFreqMin { get; set; }
    public double ChaFreqMax { get; set; }
    public double DurationMin { get; set; }
    public double DurationMax { get; set; }
    public double CallDistMin { get; set; }
    public double CallDistMax { get; set; }
  }


  public class ViewModel
  {

    string _selectedDir = "";
    Project _prj;
    string _version;
    ProcessRunner _proc;
    ZoomView _zoom;
    Filter _filter;
    ColorTable _colorTable;
    bool _extBusy = false;
    ScriptRunner _scripter = null;
    WavFile _wav;
    string _scriptName = "";
    ClassifierBarataud _clsBarataud;
    List<SpeciesInfos> _speciesInfos;
    SumReport _sumReport;
    BatSpeciesRegions _batSpecRegions;
    //ScatterDiagram _scatterDiagram;
    Forms.MainWindow _mainWin;
    List<BaseModel> _models;
    Query _query;
     
    
    public string SelectedDir { get { return _selectedDir; } }
    
    public ScriptRunner Scripter {  get { return _scripter; } }
    
    public string Version { get { return _version; } }
       
    public ClassifierBarataud Classifier { get { return _clsBarataud; } }
    
    public Project Prj { get { return _prj; } }
    
    public ZoomView ZoomView { get { return _zoom; } }
    
    public Filter Filter { get { return _filter; } }
    
    public ColorTable ColorTable { get { return _colorTable; } }
    
    public bool Busy { get { return isBusy(); } set { _extBusy = value; } }
    
    public WavFile WavFile { get { return _wav; } }
    
    public List<SpeciesInfos> SpeciesInfos { get { return _speciesInfos; } }
    
    public System.Windows.Input.Key LastKey { get; set; }
    
    public System.Windows.Input.Key KeyPressed { get; set; }

    public SumReport SumReport { get { return _sumReport; } }

    public BatSpeciesRegions Regions { get { return _batSpecRegions; } }

    public bool UpdateUi { get; set; }

    public bool ReloadPrj { get; set; }

    public Query Query { get { return _query; } set { _query = value; } }

    /// <summary>
    /// currently opened object (prj, query or null)
    /// </summary>
    public PrjBase CurrentlyOpen
    {
      get
      {
        if (_prj != null)
          return _prj;
        else if (_query != null)
          return _query;
        else
          return null;
      }
    }

    //  public ScatterDiagram ScatterDiagram { get { return _scatterDiagram; } set { _scatterDiagram = value; } }

    public ViewModel(Forms.MainWindow mainWin, string version)
    {
      if (!AppParams.IsInitialized)
        AppParams.load();
      _batSpecRegions = BatSpeciesRegions.load();
      _proc = new ProcessRunner();
      _mainWin = mainWin;
      _speciesInfos = BatInfo.load().Species;
      _version = version;
      _colorTable = new ColorTable(this);
      _colorTable.createColorLookupTable();
      _zoom = new ZoomView(_colorTable);
      _wav = new WavFile();
      _clsBarataud = new ClassifierBarataud(_batSpecRegions);
      _sumReport = new SumReport();
      _prj = new Project(_batSpecRegions, _speciesInfos);
      _models = new List<BaseModel>();
      _query = null;
      int index = 0;
      foreach(ModelItem m in AppParams.Inst.Models)
      {
        _models.Add(BaseModel.Create(index, m.ModelType));
        index++;
      }

      List<string> species = new List<string>();
      foreach (SpeciesInfos info in _speciesInfos)
        species.Add(info.Abbreviation);
      _filter = new Filter(species);
      initFilter();
      string scriptDir = Path.Combine(AppParams.AppDataPath, AppParams.Inst.ScriptDir);
      _scripter = new ScriptRunner(ref _proc, scriptDir, updateProgress, this);
      //_prj.Analysis.init(_prj.SpeciesInfos);
      UpdateUi = false;
    }

    public void updateReport()
    {
      if (File.Exists(Prj.ReportName))
        _prj.Analysis.read(Prj.ReportName);
    }


    public void initQuery(FileInfo file)
    {
      _prj = null;
      if (Query.isQuery(file))
      {
        _query = Query.readQueryFile(file.FullName, this);
        _selectedDir = Path.GetDirectoryName(file.FullName);
      }
      else
        _query = null;
    }

    public void initProject(DirectoryInfo dir)
    {
      if (Project.containsProject(dir) != "")
      {
        _selectedDir = dir.FullName + "/";
        string[] files = System.IO.Directory.GetFiles(dir.FullName, "*" + AppParams.EXT_PRJ,
                         System.IO.SearchOption.TopDirectoryOnly);
        if (_prj == null)
          _prj = new Project(_batSpecRegions, _speciesInfos);
        _prj.readPrjFile(files[0]);
        if (File.Exists(Prj.ReportName))
        {
          _prj.Analysis.read(_prj.ReportName);
          _prj.Analysis.openSummary(_prj.SummaryName, _prj.Notes);
        }
        else
          _prj.Analysis.init(_prj.SpeciesInfos);
        if (_prj.Ok &&_prj.Analysis.Report != null)
          checkProject();
        _scripter = new ScriptRunner(ref _proc, _selectedDir, updateProgress, this);
      }
      else if (Project.containsWavs(dir))
      {
        _prj = new Project(_batSpecRegions, _speciesInfos);
        _prj.fillFromDirectory(dir);
        _selectedDir = dir.FullName + "/";
        _scripter = new ScriptRunner(ref _proc, _selectedDir, updateProgress, this);
      }
      else
        _prj = null;
      if (_prj != null)
      {
        double maxFreq = 150;
        if ((_prj.Analysis != null) && (_prj.Analysis.Report != null) && (_prj.Analysis.Files.Count > 0))
          maxFreq = _prj.Analysis.Files[0].getInt(Cols.SAMPLERATE) / 2000;
      }
    }

    void updateProgress(int percent)
    {
      if (percent == 100)
        UpdateUi = true;
    }

    void checkProject()
    {
      bool ok = true;
      foreach(BatExplorerProjectFileRecordsRecord rec in _prj.Records)
      {
        AnalysisFile a = _prj.Analysis.find(rec.File);
        if (a == null)
        {
          DebugLog.log("mismatch Prj against Report, missing file " + rec.File + " in report", enLogType.ERROR);
          ok = false;
        }
      }
      foreach (AnalysisFile a in _prj.Analysis.Files)
      {
        BatExplorerProjectFileRecordsRecord r  = _prj.find(a.Name);
        if (r == null)
        {
          DebugLog.log("mismatch Prj against Report, missing file " + a.Name + " in project", enLogType.ERROR);
          ok = false;
        }
      }
      if (ok)
        DebugLog.log("report and project file are consistent", enLogType.DEBUG);
      else
        DebugLog.log("mismatch between project file and report, please check", enLogType.ERROR, true);
    }


    public void loadSettings()
    {
      AppParams.load();
      initFilter();
    }

    public void saveSettings()
    {
      AppParams.Inst.Filter.Clear();
      foreach(FilterItem fItem in _filter.Items)
      {
        FilterParams fPar = new FilterParams
        {
          Expression = fItem.Expression,
          Name = fItem.Name,
          isForAllCalls = fItem.IsForAllCalls
        };
        AppParams.Inst.Filter.Add(fPar);
      }
      AppParams.Inst.save();
    }

    public BitmapImage getFtImage(BatExplorerProjectFileRecordsRecord rec, out bool newImage, bool fromQuery)
    {
      string fullName = fromQuery ? Path.Combine(_selectedDir, rec.File) : Path.Combine(_selectedDir, _prj.WavSubDir, rec.File);
      string pngName = fullName.Replace(AppParams.EXT_WAV, AppParams.EXT_IMG);
      Bitmap bmp = null;
      BitmapImage bImg = null;
      newImage = false;
      if (File.Exists(pngName))
      {
        bmp = new Bitmap(pngName);
      }
      else
      {
        Waterfall wf = new Waterfall(fullName, _colorTable);
        if (wf.Ok)
        {
          wf.generateFtDiagram(0, (double)wf.Audio.Samples.Length / wf.SamplingRate, AppParams.Inst.WaterfallWidth);
          bmp = wf.generateFtPicture(0, wf.SamplingRate/2000);
          bmp.Save(pngName);
          newImage = true;
        }
        else
        {
           DebugLog.log("File '" + rec.File + "'does not exist, removed from project and report", enLogType.WARNING);
          if (_prj != null)
          {
            _prj.removeFile(rec.File);
            _prj.Analysis.removeFile(Prj.ReportName, rec.File);
          }
        }
      }
      if(bmp != null)
        bImg = Convert(bmp);
      return bImg;
    }

    public void executeCmd(string cmd)
    {
      _scripter.execCmd(cmd);
    }

    public int executeScript(string path, bool  initVars = true)
    {
      if (!System.IO.Path.IsPathRooted(path))
        _scriptName = System.IO.Path.Combine(AppParams.AppDataPath, AppParams.Inst.ScriptDir, path);
      else
        _scriptName = path;
      int retVal = _scripter.RunScript(_scriptName, true, initVars);
      return retVal;
    }

    public void cancelScript()
    {
      _scripter.cancelExecution();
      DebugLog.log("script execution cancelled manually", enLogType.INFO);
    }

    public void editScript(string path)
    {
      string exe = AppParams.Inst.ExeEditor;
      if (!System.IO.Path.IsPathRooted(path))
        _scriptName = System.IO.Path.Combine(AppParams.AppDataPath, path);
      else
        _scriptName = path;
      string args = _scriptName;
      _proc.launchCommandLineApp(exe, null, null, false, args, true, false);
    }

    public void deleteFiles(List<string> files)
    {
      DebugLog.log("start deleting files", enLogType.INFO);
      _prj.writePrjFile();

      foreach (string f in files)
        deleteFile(f);

      _prj.Analysis.save(SelectedDir, _prj.Notes);
      DebugLog.log(files.Count.ToString() + " files deleted", enLogType.INFO);
    }

    public void removeDeletedWavsFromReport(string reportName)
    {
      if (_prj.Analysis != null)
        _prj.Analysis.removeDeletedWavsFromReport(_prj);
    }

    void deleteFile(string wavName)
    {
      if (_prj != null)
      {
        string dirName = Path.Combine(_selectedDir, _prj.WavSubDir);
        string delName = System.IO.Path.GetFileName(wavName);
        delName = delName.Replace(AppParams.EXT_WAV, ".*");
        IEnumerable<string> delFiles = Directory.EnumerateFiles(dirName, delName);
        string destDir = Path.Combine(SelectedDir, AppParams.DIR_DEL);
        if(!Directory.Exists(destDir))
          Directory.CreateDirectory(destDir);
        foreach (string f in delFiles)
        {
          try
          { 
            File.Copy(f, destDir + "/" + System.IO.Path.GetFileName(f));
            File.Delete(f);
            DebugLog.log("delete file " + f, enLogType.DEBUG);
          }
          catch
          {
          }

        }

        _prj.removeFile(wavName);
        _prj.writePrjFile();
        _prj.Analysis.removeFile(_prj.ReportName, wavName);
      }
    }

    /// <summary>
    /// evaluation of bat species
    /// </summary>
    public int evaluate()
    {
      int retVal = 2;
      
      if(Prj != null)
      {
        for(int i = 0; i < AppParams.Inst.Models.Count; i++)
        {
          if ((i < _models.Count) && (AppParams.Inst.Models[i].Active == true))
          {
            retVal = _models[i].classify(Prj);
          }
        }
        ReloadPrj = true;
      }
      DebugLog.log("evaluation of species done", enLogType.INFO);
      return retVal;
    }

    public List<SpeciesItem> getPossibleSpecies(double charFreq, double duration, double callDist)
    {
      List<SpeciesItem> retVal = new List<SpeciesItem>();

      foreach(SpeciesInfos s in _speciesInfos)
      {
        if(
            (s.FreqCharMin <= charFreq) && (charFreq <= s.FreqCharMax) &&
            (s.DurationMin <= duration) && (duration <= s.DurationMax) &&
            (s.CallDistMin <= callDist) && (callDist <= s.CallDistMax) 
          )
        {
          SpeciesItem item = new SpeciesItem
          {
            Abbreviation = s.Abbreviation,
            Name = s.Local,
            CharFreqMin = s.FreqCharMin,
            ChaFreqMax = s.FreqCharMax,
            DurationMin = s.DurationMin,
            DurationMax = s.DurationMax,
            CallDistMin = s.CallDistMin,
            CallDistMax = s.CallDistMax
          };
          retVal.Add(item);
        }
      }
      return retVal;
    }

    //http://www.shujaat.net/2010/08/wpf-images-from-project-resource.html
    static public BitmapImage Convert(Bitmap value)
    {
      MemoryStream ms = new MemoryStream();
      value.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
      BitmapImage image = new BitmapImage();
      image.BeginInit();
      ms.Seek(0, SeekOrigin.Begin);
      image.StreamSource = ms;
      image.EndInit();

      return image;
    }

    private void initFilter()
    {
      _filter.Items.Clear();
      foreach (FilterParams p in AppParams.Inst.Filter)
      {
        FilterItem it = new FilterItem(_filter.Items.Count, p.Name, p.Expression, p.isForAllCalls);
        _filter.Items.Add(it);
      }
    }

    private bool tidyUpProject(DirectoryInfo dir, bool delWavs, bool pngs)
    {
      bool retVal = true;
      try
      {
        if (delWavs)
        {
          string delDir = Path.Combine(dir.FullName, AppParams.DIR_DEL);
          if (Directory.Exists(delDir))
          {
            Directory.Delete(delDir, true);
            DebugLog.log("deletes files permanently removed from project " + dir.Name, enLogType.INFO);
          }
        }
        if(pngs) 
        {
          FileInfo[] files = dir.GetFiles("*.png");
          foreach (FileInfo file in files)
            File.Delete(file.FullName);
          string dirRecords = Path.Combine(dir.FullName, AppParams.DIR_WAVS);
          DirectoryInfo dirWavs = new DirectoryInfo(dirRecords);
          files = dirWavs.GetFiles("*.png");
          foreach (FileInfo file in files)
            File.Delete(file.FullName);
          DebugLog.log("PNG files deleted in project " + dir.FullName, enLogType.INFO);
        }
      }
      catch
      {
        retVal = false;
      }
      return retVal;
    }

    private bool checkProject(DirectoryInfo dir, ref int wavSpace, ref int pngSpace)
    {
      bool retVal = true;
      try
      {
        string delDir = Path.Combine(dir.FullName, AppParams.DIR_DEL);
        if (Directory.Exists(delDir))
        {
          DirectoryInfo d = new DirectoryInfo(delDir);
          foreach(FileInfo file in d.GetFiles())
            wavSpace += (int)(file.Length / 1024);
        }

        FileInfo[] files = dir.GetFiles("*.png");
        foreach (FileInfo file in files)
          pngSpace += (int)(file.Length / 1024);
        string dirRecords = Path.Combine(dir.FullName, AppParams.DIR_WAVS);
        DirectoryInfo dirWavs = new DirectoryInfo(dirRecords);
        files = dirWavs.GetFiles("*.png");
        foreach (FileInfo file in files)
          pngSpace += (int)(file.Length / 1024);
      }
      catch
      {
        retVal = false;
      }
      return retVal;
    }
    private bool crawlTidyUp(DirectoryInfo dir, bool delWavs, bool pngs)
    {
      if (!dir.Exists)
        return false;
      DirectoryInfo[] dirs = dir.GetDirectories();
      bool ok = false;
      if (Project.containsProject(dir) != "")
        ok = tidyUpProject(dir, delWavs, pngs);
      else
      {
        foreach (DirectoryInfo d in dirs)
        {
          if (Project.containsProject(d) != "")
            ok = tidyUpProject(d, delWavs, pngs);
          else
            ok = crawlTidyUp(d, delWavs, pngs);
          if (!ok)
            break;
        }
      }
      return ok;
    }

    private bool crawlCheckSpace(DirectoryInfo dir, ref int wavSpace,  ref int pngSpace)
    {
      if (!dir.Exists)
        return false;

      DirectoryInfo[] dirs = dir.GetDirectories();
      
      bool ok = true;
      if (Project.containsProject(dir) != "")
        ok = checkProject(dir, ref wavSpace, ref pngSpace);
      else
      {
        foreach (DirectoryInfo d in dirs)
        {
          if (Project.containsProject(d) != "")
            ok = checkProject(d, ref wavSpace, ref pngSpace);
          else
            ok = crawlCheckSpace(d, ref wavSpace, ref pngSpace);
          if (!ok)
            break;
        }
      }
      return ok;
    }


    public void cleanup(string root, bool delWavs, bool logs, bool pngs)
    {
      if (Directory.Exists(root))
      {
        DirectoryInfo dir = new DirectoryInfo(root);
        crawlTidyUp(dir, delWavs, pngs);
      }

      if (Directory.Exists(AppParams.LogDataPath))
      {
        DirectoryInfo dir = new DirectoryInfo(AppParams.LogDataPath);
        foreach (FileInfo f in dir.GetFiles())
        {
          if (f.FullName != DebugLog.FileName)
            File.Delete(f.FullName);
        }
      }
    }

    public void checkMem(string root, out int wavSpace, out int logSpace, out int pngSpace)
    {
      wavSpace = 0;
      logSpace = 0;
      pngSpace = 0;
      if (Directory.Exists(root))
      {
        DirectoryInfo dir = new DirectoryInfo(root);
        crawlCheckSpace(dir, ref wavSpace, ref pngSpace);
      }

      if (Directory.Exists(AppParams.LogDataPath))
      {
        DirectoryInfo dir = new DirectoryInfo(AppParams.LogDataPath);
        foreach (FileInfo f in dir.GetFiles())
          logSpace += (int)(f.Length / 1024);
      }
    }


    bool isBusy()
    {
      bool retVal = _proc.IsRunning | _extBusy;
      return retVal;
    }
  }
}
