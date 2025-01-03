/********************************************************************************
 *               Author: Christian Müller
 *     Date of creation: 2021-08-10                                       
 *   Copyright (C) 2023: Christian Müller chrmue44(at)gmail(dot).com
 *
 *              Licence:  CC BY-NC 4.0 
 ********************************************************************************/
using BatInspector.Controls;
using BatInspector.Forms;
using libParser;
using libScripter;
using System;
using System.Collections.Generic;
using System.Deployment.Application;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;


namespace BatInspector
{
  public enum enAppState
  {
    IDLE,
    OPEN_PRJ,
    WAIT_FOR_GUI,
    AI_ANALYZE,
    IMPORT_PRJ,
    TOOL_RUNNING
  }

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

  public class ModelState
  {
    public string Msg { get; set; } = null;
    public enAppState State { get; set; } = enAppState.IDLE;
  }

  public class ViewModel
  {
    string _selectedDir = "";
    ProcessRunner _proc;
    ZoomView _zoom;
    Filter _filter;
    ColorTable _colorTable;
    bool _extBusy = false;
    ScriptRunner _scripter = null;
    WavFile _wav;
    ClassifierBarataud _clsBarataud;
    List<SpeciesInfos> _speciesInfos;
    SumReport _sumReport;
    BatSpeciesRegions _batSpecRegions;
    Forms.MainWindow _mainWin;
    List<BaseModel> _models;
    CtrlRecorder _recorder;
    Statistic _statistic;
    string _tempCmd;
    PrjView _view;
    ModelParams[] _defaultModelParams;
    dlgVoid _callBackEnd = null;

    public PrjView View { get{ return _view; } }

    public string SelectedDir { get { return _selectedDir; } }

    public ScriptRunner Scripter { get { return _scripter; } }

    public ClassifierBarataud Classifier { get { return _clsBarataud; } }

    public Project Prj { get { return _view.Prj; } }

    public ZoomView ZoomView { get { return _zoom; } }

    public Filter Filter { get { return _filter; } }

    public ColorTable ColorTable { get { return _colorTable; } }

    public bool Busy { get { return isBusy(); } set { _extBusy = value; } }

    public WavFile WavFile { get { return _wav; } }

    public List<SpeciesInfos> SpeciesInfos { get { return _speciesInfos; } }

    public System.Windows.Input.Key LastKey { get; set; }

    public SumReport SumReport { get { return _sumReport; } }

    public BatSpeciesRegions Regions { get { return _batSpecRegions; } }

    public bool UpdateUi { get; set; }

    public Query Query { get { return _view.Query; } set { _view.Query = value; } }

    public ModelState Status { get; set; }

    public Statistic Statistic { get { return _statistic; } }
    
    public CtrlRecorder Recorder { get { return _recorder; } }

    public ModelParams[] DefaultModelParams { get { return _defaultModelParams; } }


    /// <summary>
    /// currently opened object (prj, query or null)
    /// </summary>
    public PrjBase CurrentlyOpen
    {
      get
      {
        if ((_view.Prj != null) && (_view.Prj.Ok))
          return _view.Prj;
        else if (_view.Query != null)
          return _view.Query;
        else
          return null;
      }
    }

    public ViewModel()
    {
      if (!AppParams.IsInitialized)
        AppParams.load();
      _batSpecRegions = BatSpeciesRegions.loadFrom(AppParams.Inst.BatInfoPath);
      _proc = new ProcessRunner();
      _speciesInfos = BatInfo.loadFrom(AppParams.Inst.BatInfoPath).Species;
      //   _speciesInfos.Add(new SpeciesInfos("?", "", "", false, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0));     // why here???
      //   _speciesInfos.Add(new SpeciesInfos("Social", "", "", false, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0));
      _colorTable = new ColorTable();
      _colorTable.createColorLookupTable();
      Status = new ModelState();
      _zoom = new ZoomView(_colorTable, _proc, Status);
      _wav = new WavFile();
      _clsBarataud = new ClassifierBarataud(_batSpecRegions);
      _sumReport = new SumReport();
      _models = new List<BaseModel>();
      _view = new PrjView();
      _view.Query = null;
      int index = 0;

      _defaultModelParams = BaseModel.readDefaultModelParams();
      foreach ( ModelParams m in _defaultModelParams)
      {
        _models.Add(BaseModel.Create(index, m.Type));
        index++;
      }
      _view.Prj = new Project(_batSpecRegions, _speciesInfos, true,
                 DefaultModelParams[getModelIndex(AppParams.Inst.DefaultModel)], DefaultModelParams.Length);

      List<string> species = new List<string>();
      foreach (SpeciesInfos info in _speciesInfos)
        species.Add(info.Abbreviation);
      species.Add("?");
      species.Add("Social");

      _filter = new Filter(species);
      initFilter();
      string scriptDir = AppParams.Inst.ScriptInventoryPath;
      _scripter = new ScriptRunner(ref _proc, scriptDir, updateProgress);
      //_prj.Analysis.init(_prj.SpeciesInfos);
      _recorder = new CtrlRecorder(_colorTable);
      _statistic = new Statistic(AppParams.STATISTIC_CLASSES);
      UpdateUi = false;
    }

    public void updateReport()
    {
      if ((Prj != null) && (Prj.Ok) && File.Exists(Prj.ReportName))
        _view.Prj.Analysis.read(Prj.ReportName, DefaultModelParams);
      else if ((Query != null) && File.Exists(Query.ReportName))
        _view.Query.Analysis.read(Query.ReportName, DefaultModelParams);

    }


    public void initQuery(FileInfo file)
    {
      if (Query.isQuery(file))
      {
        _view.Prj = null;
        _view.Query = Query.readQueryFile(file.FullName);
        _selectedDir = Path.GetDirectoryName(file.FullName);
      }
      else
        _view.Query = null;
    }

    public void initProject(DirectoryInfo dir, bool updateCtls)
    {
      if (Project.containsProject(dir) != "")
      {
        _view.Query = null;
        _selectedDir = dir.FullName;
        _view.Prj = new Project(_batSpecRegions, _speciesInfos, updateCtls, DefaultModelParams[getModelIndex(AppParams.Inst.DefaultModel)], DefaultModelParams.Length);
        _view.Prj.readPrjFile(_selectedDir, DefaultModelParams);
        if (File.Exists(Prj.ReportName))
        {
          _view.Prj.Analysis.read(Prj.ReportName, DefaultModelParams);
          _view.Prj.Analysis.openSummary(_view.Prj.SummaryName, _view.Prj.Notes);
          if ((_view.Prj.Analysis.Files.Count > 0) && _view.Prj.Analysis.Files[0].getDouble(Cols.TEMPERATURE) <= 0)
          {
            _view.Prj.Analysis.filloutTemperature(Path.Combine(_selectedDir, _view.Prj.WavSubDir));
            _view.Prj.Analysis.save(_view.Prj.ReportName, _view.Prj.Notes, _view.Prj.SummaryName);
          }
        }
        else
          _view.Prj.Analysis.init(_view.Prj.SpeciesInfos);
//        if (_prj.Ok && _prj.Analysis.Report != null)
//          checkProject();
        initScripter();
      }
      else if (Project.containsWavs(dir))
      {
        _view.Prj = new Project(_batSpecRegions, _speciesInfos, updateCtls, DefaultModelParams[getModelIndex(AppParams.Inst.DefaultModel)], DefaultModelParams.Length);
        _view.Prj.fillFromDirectory(dir);
        _selectedDir = dir.FullName;
        initScripter();
      }
      else
        _view.Prj = null;
    }

    void initScripter()
    {
      if (_scripter == null)
        _scripter = new ScriptRunner(ref _proc, _selectedDir, updateProgress);
      else
        _scripter.setWorkDir(_selectedDir);
    }

    void updateProgress(int percent)
    {
      if (percent == 100)
        UpdateUi = true;
    }

    void checkProject()
    {
      bool ok = true;
      foreach (PrjRecord rec in _view.Prj.Records)
      {
        AnalysisFile a = _view.Prj.Analysis.find(rec.File);
        if (a == null)
        {
          DebugLog.log("mismatch Prj against Report, missing file " + rec.File + " in report", enLogType.ERROR);
          ok = false;
        }
      }
      foreach (AnalysisFile a in _view.Prj.Analysis.Files)
      {
        PrjRecord r = _view.Prj.find(a.Name);
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
      foreach (FilterItem fItem in _filter.Items)
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

    public void createPngIfMissing(PrjRecord rec, bool fromQuery)
    {
      string fullName = fromQuery ? Path.Combine(_selectedDir, rec.File) : Path.Combine(_selectedDir, _view.Prj.WavSubDir, rec.File);
      string pngName = fullName.ToLower().Replace(AppParams.EXT_WAV, AppParams.EXT_IMG);
      if (!File.Exists(pngName))
      {
        createPng(fullName, pngName,AppParams.FFT_WIDTH, _colorTable);
      }
    }


    public static void createPng(string wavName, string pngName, int fftWidth, ColorTable colorTable)
    {
      Waterfall wf = new Waterfall(wavName,  colorTable, fftWidth);
      if (wf.Ok)
      {
        wf.generateFtDiagram(0, (double)wf.Audio.Samples.Length / wf.SamplingRate, AppParams.Inst.WaterfallWidth);
        using (Bitmap bmp = wf.generateFtPicture(0, wf.Duration, 0, wf.SamplingRate / 2000))
        {
          if (File.Exists(pngName))
            File.Delete(pngName);
          bmp.Save(pngName);
        }
      }
      else
        DebugLog.log("could not create PNG for " + wavName, enLogType.WARNING);
    }

    
  

    /// <summary>
    /// execute command in separate thread
    /// </summary>
    /// <param name="cmd"></param>
    public void executeCmd(string cmd, dlgVoid callBackEnd)
    {
      _tempCmd = cmd;
      _callBackEnd = callBackEnd;
      Thread thr = new Thread(threadExecCmd);
      thr.Start();
    }

    public void threadExecCmd()
    {
      _scripter.execCmd(_tempCmd);
      if(_callBackEnd != null)
      {
        _callBackEnd();
        _callBackEnd = null;
      }
    }

    public void cancelScript()
    {
      _scripter.cancelExecution();
      DebugLog.log("script execution cancelled manually", enLogType.INFO);
    }

    public void editScript(string path)
    {
      string exe = AppParams.Inst.ExeEditor;
      string scriptName;
      if (!System.IO.Path.IsPathRooted(path))
        scriptName = System.IO.Path.Combine(AppParams.Inst.ScriptInventoryPath, path);
      else
        scriptName = path;
      string args = scriptName;
      _proc.launchCommandLineApp(exe, null, null, false, args, true, false);
    }
 
    public void deleteFiles(List<string> files)
    {
      DebugLog.log("start deleting files", enLogType.INFO);
      _view.Prj.writePrjFile();

      foreach (string f in files)
        deleteFile(f);

      _view.Prj.Analysis.save(_view.Prj.ReportName, _view.Prj.Notes, _view.Prj.SummaryName);
      DebugLog.log(files.Count.ToString() + " files deleted", enLogType.INFO);
    }

    public void removeDeletedWavsFromReport(string reportName)
    {
      if (_view.Prj.Analysis != null)
        _view.Prj.Analysis.removeDeletedWavsFromReport(_view.Prj);
    }

    void deleteFile(string wavName)
    {
      if (_view.Prj != null)
      {
        string dirName = Path.Combine(_selectedDir, _view.Prj.WavSubDir);
        string delName = System.IO.Path.GetFileName(wavName);
        delName = delName.ToLower().Replace(AppParams.EXT_WAV, ".*");
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

        _view.Prj.removeFile(wavName);
        _view.Prj.writePrjFile();
        if(_view.Prj.Analysis.IsEmpty == false)
          _view.Prj.Analysis.removeFile(_view.Prj.ReportName, wavName);
      }
    }

    public void stopEvaluation()
    {
      if ((Prj != null) && (Prj.Ok))
      {
        if (AppParams.Inst.AllowMoreThanOneModel)
        {
          for (int i = 0; i < Prj.AvailableModelParams.Length; i++)
          {
            if ((i < _models.Count) && (Prj.AvailableModelParams[i].Enabled == true))
            {
              _models[i].stopClassification();
              DebugLog.log("evaluation of species stopped", enLogType.INFO);
            }
          }
        }
        else
          _models[Prj.SelectedModelIndex].stopClassification();
      }
    }

    public int getModelIndex(enModel modType)
    {
      int retVal = -1;
      for(int i = 0; i < _models.Count; i++)
      {
        if(modType == _models[i].Type)
        {
          retVal = i;
          break;
        }
      }
      return retVal;
    }

    public int getModelIndex(string modelName)
    {
      int retVal = -1;
      for (int i = 0; i < _models.Count; i++)
      {
        if (modelName == _models[i].Name)
        {
          retVal = i;
          break;
        }
      }
      return retVal;
    }
    /// <summary>
    /// evaluation of bat species
    /// </summary>
    public int evaluate(bool cli)
    {
      int retVal = 2;
      if((Prj != null) && Prj.Ok)
      {
        if (AppParams.Inst.AllowMoreThanOneModel)
        {
          for (int i = 0; i < Prj.AvailableModelParams.Length; i++)
          {
            if ((i < _models.Count) && (Prj.AvailableModelParams[i].Enabled == true))
            {
              retVal = _models[i].classify(Prj, false, cli);
            }
          }
        }
        else
        {
          bool removeEmptyFiles = true;
          for(int i = 0; i < Prj.AvailableModelParams.Length; i++)
          {
            string report = Prj.getReportName(i);
            if (File.Exists(report))
              removeEmptyFiles = false;
          }
          retVal = _models[Prj.SelectedModelIndex].classify(Prj, removeEmptyFiles, cli);
        }
        _view.Prj.writePrjFile();
      }
      DebugLog.log("evaluation of species done", enLogType.INFO);
      return retVal;
    }

    public BaseModel getClassifier(enModel type)
    {
      BaseModel retVal = null;
      foreach(BaseModel m in _models)
      {
        if(type == m.Type)
        {
          retVal = m;
          break;
        }
      }
      return retVal;
    }

/*
    public ModelParams[] getDefaultModelParams()
    {
      ModelParams[] retVal = new ModelParams[AppParams.Inst.Models.Count];
      for (int i = 0; i < AppParams.Inst.Models.Count; i++)
      {
        BaseModel c = getClassifier(AppParams.Inst.Models[i].ModelType);
        retVal[i] = new ModelParams()
        {
          Name = c.Name,
          Type = c.Type,
          Parameters = c.getDefaultModelParams(),
          DataSet = BaseModel.getDataSetItems(c.Type)[0],
          Enabled = (i == 0)
        };
      }
      return retVal;
    }
    */

    public int createReport(Project prj)
    {
      int retVal = 0;
      if (prj != null)
      {
        if (AppParams.Inst.AllowMoreThanOneModel)
        {
          for (int i = 0; i < prj.AvailableModelParams.Length; i++)
          {
            if ((i < _models.Count) && (prj.AvailableModelParams[i].Enabled == true))
            {
              retVal = _models[i].createReport(prj);
            }
          }
        }
        else
          retVal = _models[prj.SelectedModelIndex].createReport(prj);
      }
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
    /*
        //http://www.shujaat.net/2010/08/wpf-images-from-project-resource.html
        //has suspicious memory problems
        static public BitmapImage ConvertOld(Bitmap value)
        {
          MemoryStream ms = new MemoryStream();
          value.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
          BitmapImage image = new BitmapImage();
          image.BeginInit();
          ms.Seek(0, SeekOrigin.Begin);
          image.StreamSource = ms;
          image.EndInit();

          return image;
        }
    */

    /*
    [System.Runtime.InteropServices.DllImport("gdi32.dll")]
    public static extern bool DeleteObject(IntPtr hObject);

    //https://stackoverflow.com/questions/6484357/converting-bitmapimage-to-bitmap-and-vice-versa
    static public BitmapImage Convert(Bitmap bitmap)
    {
      IntPtr hBitmap = bitmap.GetHbitmap();
      BitmapImage retval;

      try
      {
        retval = (BitmapImage)Imaging.CreateBitmapSourceFromHBitmap(
                     hBitmap,
                     IntPtr.Zero,
                     Int32Rect.Empty,
                     BitmapSizeOptions.FromEmptyOptions());
      }
      finally
      {
        DeleteObject(hBitmap);
      }
      return retval;
    }*/




    private void initFilter()
    {
      _filter.Items.Clear();
      foreach (FilterParams p in AppParams.Inst.Filter)
      {
        FilterItem it = new FilterItem(_filter.Items.Count, p.Name, p.Expression, p.isForAllCalls);
        _filter.Items.Add(it);
      }
    }

    private bool tidyUpProject(DirectoryInfo dir, bool delWavs, bool pngs, bool delOrig, bool delAnn)
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
        if (delOrig)
        {
          string origDir = Path.Combine(dir.FullName, AppParams.DIR_ORIG);
          if (Directory.Exists(origDir))
          {
            Directory.Delete(origDir, true);
            DebugLog.log("original files permanently removed from project " + dir.Name, enLogType.INFO);
          }
        }
        if (delAnn)
        {
          Project prj = new Project(Regions, SpeciesInfos, false, DefaultModelParams[0], DefaultModelParams.Length);
          prj.readPrjFile(dir.FullName, DefaultModelParams);
          string annDir = prj.getAnnotationDir();
          if (Directory.Exists(annDir))
          {
            Directory.Delete(annDir, true);
            DebugLog.log("AI annotation files permanently removed from project " + dir.Name, enLogType.INFO);
          }
        }
        if (pngs) 
        {
          FileInfo[] files = dir.GetFiles("*.png");
          foreach (FileInfo file in files)
            File.Delete(file.FullName);
          string dirRecords = Path.Combine(dir.FullName, AppParams.DIR_WAVS);
          if (!Directory.Exists(dirRecords))
            dirRecords = dir.FullName;
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

    private bool checkProjectMem(DirectoryInfo dir, ref int wavSpace, ref int pngSpace, ref int origSpace, ref int annSpace)
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

        string origDir = Path.Combine(dir.FullName, AppParams.DIR_ORIG);
        if (Directory.Exists(origDir))
        {
          DirectoryInfo d = new DirectoryInfo(origDir);
          foreach (FileInfo file in d.GetFiles())
            origSpace += (int)(file.Length / 1024);
        }
        Project prj = new Project(Regions, SpeciesInfos, false, DefaultModelParams[0], DefaultModelParams.Length);
        prj.readPrjFile(dir.FullName, DefaultModelParams);
        string annDir = prj.getAnnotationDir();
        if (Directory.Exists(annDir))
        {
          DirectoryInfo d = new DirectoryInfo(annDir);
          foreach (FileInfo file in d.GetFiles())
            annSpace += (int)(file.Length / 1024);
        }

        FileInfo[] files = dir.GetFiles("*.png");
        foreach (FileInfo file in files)
          pngSpace += (int)(file.Length / 1024);
        string dirRecords = Path.Combine(dir.FullName, AppParams.DIR_WAVS);
        if (!Directory.Exists(dirRecords))
          dirRecords = dir.FullName;

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
    private bool crawlTidyUp(DirectoryInfo dir, bool delWavs, bool pngs, bool delOrig, bool delAnn)
    {
      if (!dir.Exists)
        return false;
      DirectoryInfo[] dirs = dir.GetDirectories();
      bool ok = true;
      if (Project.containsProject(dir) != "")
        ok = tidyUpProject(dir, delWavs, pngs, delOrig, delAnn);
      else
      {
        foreach (DirectoryInfo d in dirs)
        {
          if (Project.containsProject(d) != "")
            ok = tidyUpProject(d, delWavs, pngs, delOrig, delAnn);
          else
            ok = crawlTidyUp(d, delWavs, pngs, delOrig, delAnn);
          if (!ok)
            break;
        }
      }
      return ok;
    }

    private bool crawlCheckSpace(DirectoryInfo dir, ref int wavSpace,  ref int pngSpace, ref int origSpace, ref int annSpace)
    {
      if (!dir.Exists)
        return false;

      DirectoryInfo[] dirs = dir.GetDirectories();
      
      bool ok = true;
      if (Project.containsProject(dir) != "")
        ok = checkProjectMem(dir, ref wavSpace, ref pngSpace, ref origSpace, ref annSpace);
      else
      {
        foreach (DirectoryInfo d in dirs)
        {
          if (Project.containsProject(d) != "")
            ok = checkProjectMem(d, ref wavSpace, ref pngSpace, ref origSpace, ref annSpace);
          else
            ok = crawlCheckSpace(d, ref wavSpace, ref pngSpace, ref origSpace, ref annSpace);
          if (!ok)
            break;
        }
      }
      return ok;
    }


    public void cleanup(string root, bool delWavs, bool logs, bool pngs, bool delOrig, bool delAnn)
    {
      if (Directory.Exists(root))
      {
        DirectoryInfo dir = new DirectoryInfo(root);
        crawlTidyUp(dir, delWavs, pngs, delOrig, delAnn);
      }

      if (logs && Directory.Exists(AppParams.LogDataPath))
      {
        DirectoryInfo dir = new DirectoryInfo(AppParams.LogDataPath);
        foreach (FileInfo f in dir.GetFiles())
        {
          if (f.FullName != DebugLog.FileName)
            File.Delete(f.FullName);
        }
      }
    }

    public void checkMem(string root, out int wavSpace, out int logSpace, out int pngSpace, out int origSpace, out int annSpace)
    {
      wavSpace = 0;
      logSpace = 0;
      pngSpace = 0;
      origSpace = 0;
      annSpace = 0;
      if (Directory.Exists(root))
      {
        DirectoryInfo dir = new DirectoryInfo(root);
        crawlCheckSpace(dir, ref wavSpace, ref pngSpace, ref origSpace, ref annSpace);
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

    public void createProject(PrjInfo info, bool inspect, bool cli)
    {
      bool ok = Project.createPrjFromWavs(info, Regions, SpeciesInfos, info.ModelParams, DefaultModelParams);
      if (!ok)
      {
        App.Model.Status.State = enAppState.IDLE;
        return;
      }
      string prjPath = Path.Combine(info.DstDir, info.Name);
      DirectoryInfo dir = new DirectoryInfo(prjPath);
      initProject(dir, false);
      if (inspect)
      {
        Status.Msg = BatInspector.Properties.MyResources.MainWindowMsgClassification;
        evaluate(cli);
        initProject(dir, false);  // re init project to take deleted files into account
      }
      if (Prj.Records.Length > info.MaxFileCnt)
      {
        double prjCnt = (double)Prj.Records.Length / info.MaxFileCnt;
        if (prjCnt > (int)prjCnt)
          prjCnt += 1;
        List<string> prjs = Project.splitProject(Prj, (int)prjCnt, Regions, info.ModelParams,  DefaultModelParams.Length);
        initProject(new DirectoryInfo(prjs[0]), false);
        // remove src project
        Directory.Delete(prjPath, true);
      }
      if (!cli)
        Prj.ReloadInGui = true;
    }
  }
}
