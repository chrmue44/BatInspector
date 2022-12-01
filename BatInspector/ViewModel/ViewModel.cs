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
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
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
    public const int OPT_INSPECT = 0x01;
    public const int OPT_CUT = 0x02;
    public const int OPT_PREPARE = 0x04;
    public const int OPT_PREDICT1 = 0x08;
    public const int OPT_CONF95 = 0x10;
    public const int OPT_PREDICT2 = 0x40;
    public const int OPT_PREDICT3 = 0x80;
    public const int OPT_CLEANUP = 0x100;

    string _selectedDir = "";
    Project _prj;
    string _version;
    Analysis _analysis;
    ProcessRunner _proc;
    ZoomView _zoom;
    Filter _filter;
    AppParams _settings;
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
    int _evalOptions;
    Forms.MainWindow _mainWin;
    
    public string WavFilePath { get { return _selectedDir + _prj.WavSubDir; } }
    
    public string PrjPath { get { return _selectedDir; } }
    
    public string ScriptName { get { return _scriptName; } }
    
    public ScriptRunner Scripter {  get { return _scripter; } }
    
    public string Version { get { return _version; } }
    
    public Analysis Analysis { get { return _analysis; } }
    
    public ClassifierBarataud Classifier { get { return _clsBarataud; } }
    
    public Project Prj { get { return _prj; } }
    
    public ZoomView ZoomView { get { return _zoom; } }
    
    public Filter Filter { get { return _filter; } }
    
    public AppParams Settings { get { return _settings; }  }
    
    public ColorTable ColorTable { get { return _colorTable; } }
    
    public bool Busy { get { return isBusy(); } set { _extBusy = value; } }
    
    public WavFile WavFile { get { return _wav; } }
    
    public List<SpeciesInfos> SpeciesInfos { get { return _speciesInfos; } }
    
    public System.Windows.Input.Key LastKey { get; set; }
    
    public System.Windows.Input.Key KeyPressed { get; set; }

    public SumReport SumReport { get { return _sumReport; } }

    public BatSpeciesRegions Regions { get { return _batSpecRegions; } }

    public bool UpdateUi { get; set; }

  //  public ScatterDiagram ScatterDiagram { get { return _scatterDiagram; } set { _scatterDiagram = value; } }

    public ViewModel(Forms.MainWindow mainWin, string version)
    {
      _batSpecRegions = BatSpeciesRegions.load();
      _proc = new ProcessRunner();
      _mainWin = mainWin;
      _filter = new Filter();
      _settings = new AppParams();
      _speciesInfos = BatInfo.load().Species;
      _analysis = new Analysis(_speciesInfos);
      _version = version;
      _colorTable = new ColorTable(this);
      _colorTable.createColorLookupTable();
      _zoom = new ZoomView(_colorTable);
      _wav = new WavFile();
      _clsBarataud = new ClassifierBarataud(_batSpecRegions);
      _sumReport = new SumReport(_speciesInfos);
      _prj = new Project(_batSpecRegions, _speciesInfos);
      UpdateUi = false;
    }

    public void updateReport()
    {
      if (File.Exists(_selectedDir + AppParams.PRJ_REPORT))
        _analysis.read(_selectedDir + AppParams.PRJ_REPORT);
    }


    public void initProject(DirectoryInfo dir)
    {
      if (Project.containsProject(dir))
      {
        _selectedDir = dir.FullName + "/";
        if (File.Exists(_selectedDir + AppParams.PRJ_REPORT))
          _analysis.read(_selectedDir + AppParams.PRJ_REPORT);
        else
          _analysis = new Analysis(_speciesInfos);
        string[] files = System.IO.Directory.GetFiles(dir.FullName, "*.bpr",
                         System.IO.SearchOption.TopDirectoryOnly);
        if (_prj == null)
          _prj = new Project(_batSpecRegions, _speciesInfos);
        _prj.readPrjFile(files[0]);
        if (_analysis.Report != null)
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
        if ((_analysis != null) && (_analysis.Report != null) && (_analysis.Files.Count > 0))
          maxFreq = _analysis.Files[0].getInt(Cols.SAMPLERATE) / 2000;
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
        AnalysisFile a = _analysis.find(rec.File);
        if (a == null)
        {
          DebugLog.log("mismatch Prj against Report, missing file " + rec.File + " in report", enLogType.ERROR);
          ok = false;
        }
      }
      foreach (AnalysisFile a in _analysis.Files)
      {
        BatExplorerProjectFileRecordsRecord r  = _prj.find(a.Name);
        if (r == null)
        {
          DebugLog.log("mismatch Prj against Report, missing file " + a.Name + " in project", enLogType.ERROR);
          ok = false;
        }
      }
      if (ok)
        DebugLog.log("report.csv and project file are consistent", enLogType.INFO);
      else
        DebugLog.log("mismatch between project file and report, please check", enLogType.ERROR, true);
    }


    public void loadSettings()
    {
      _settings = AppParams.load();
      _filter.Items.Clear();
      foreach(FilterParams p in _settings.Filter)
      {
        FilterItem it = new FilterItem();
        it.Index = _filter.Items.Count;
        it.Name = p.Name;
        it.Expression = p.Expression;
        it.IsForAllCalls = p.isForAllCalls;
        _filter.Items.Add(it);
      }
      _scripter = new ScriptRunner(ref _proc, _settings.ScriptDir, updateProgress, this);
      _analysis = new Analysis(SpeciesInfos);
    }

    public void saveSettings()
    {
      _settings.Filter.Clear();
      foreach(FilterItem fItem in _filter.Items)
      {
        FilterParams fPar = new FilterParams();
        fPar.Expression = fItem.Expression;
        fPar.Name = fItem.Name;
        fPar.isForAllCalls = fItem.IsForAllCalls;
        _settings.Filter.Add(fPar);
      }
      _settings.save();
    }

    public BitmapImage getFtImage(BatExplorerProjectFileRecordsRecord rec, out bool newImage)
    {
      string fullName = _selectedDir + _prj.WavSubDir + rec.File;
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
        Waterfall wf = new Waterfall(_selectedDir + _prj.WavSubDir + rec.File, _settings.FftWidth,
                                     _settings.WaterfallWidth, _settings.WaterfallHeight, _settings, _colorTable);
        if (wf.Ok)
        {
          wf.generateFtDiagram(0, (double)wf.Samples.Length / wf.SamplingRate, _settings.FftWidth);
          bmp = wf.generateFtPicture(0, wf.SamplingRate/2000);
          bmp.Save(pngName);
          newImage = true;
        }
        else
        {
           DebugLog.log("File '" + rec.File + "'does not exist, removed from project and report", enLogType.WARNING);
          _prj.removeFile(rec.File);
          _analysis.removeFile(_selectedDir + "/" + AppParams.PRJ_REPORT, rec.File);
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
      int retVal = 1;
      _scriptName = path;
      retVal = _scripter.RunScript(path, true, initVars);
      return retVal;
    }

    public void cancelScript()
    {
      _scripter.cancelExecution();
      DebugLog.log("script execution cancelled manually", enLogType.INFO);
    }

    public void editScript(string name)
    {
      string exe = Settings.ExeEditor;
      _scriptName = name;
      string args = name;
      _proc.LaunchCommandLineApp(exe, null, null, false, args, true, false);
    }

    public void deleteFiles(List<string> files)
    {
      DebugLog.log("start deleting files", enLogType.INFO);
      _prj.writePrjFile();

      foreach (string f in files)
        deleteFile(f);

      _analysis.save(PrjPath + AppParams.PRJ_REPORT);
      DebugLog.log(files.Count.ToString() + " files deleted", enLogType.INFO);
    }

    public void removeDeletedWavsFromReport(string reportName)
    {
      if (_analysis != null)
        _analysis.removeDeletedWavsFromReport(_prj);
    }

    void deleteFile(string wavName)
    {
      if (_prj != null)
      {
        string dirName = _selectedDir + _prj.WavSubDir;
        string delName = Path.GetFileName(wavName);
        delName = delName.Replace(AppParams.EXT_WAV, ".*");
        IEnumerable<string> delFiles = Directory.EnumerateFiles(dirName, delName);
        string destDir = PrjPath + "/del";
        if(!Directory.Exists(destDir))
          Directory.CreateDirectory(destDir);
        foreach (string f in delFiles)
        {
          try
          { 
            File.Copy(f, destDir + "/" + Path.GetFileName(f));
            File.Delete(f);
            DebugLog.log("delete file " + f, enLogType.DEBUG);
          }
          catch
          {
          }

        }

        _prj.removeFile(wavName);
        _prj.writePrjFile();
        _analysis.removeFile(_selectedDir +"/" + AppParams.PRJ_REPORT, wavName);
      }
    }

    /// <summary>
    /// start evaluation of bat species
    /// </summary>
    public int startEvaluation(int options)
    {
      int retVal = 2;
      _evalOptions = options;
      
      if(Prj != null)
      {
        string reportName = _selectedDir + AppParams.PRJ_REPORT;
        reportName = reportName.Replace("\\", "/");
        addSpeciesColsToReport(reportName, _settings.SpeciesFile);
        string datFile = _selectedDir + "/Xdata000.npy";
        string wrkDir = "C:/Users/chrmu/prj/BatInspector/py";
        string args = _settings.PythonScript;

        if ((options & OPT_INSPECT) != 0)
        {
          //internal:
          string dir = _selectedDir + _prj.WavSubDir;
          string rep = _selectedDir + AppParams.PRJ_REPORT;
          rep = rep.Replace("\\", "/");
          if (File.Exists(rep))
            File.Delete(rep);
          BioAcoustics.analyzeFiles(rep, dir);
          _analysis = new Analysis(_speciesInfos);
          _analysis.read(rep);
        }

        if ((options & (OPT_CUT | OPT_PREPARE | OPT_PREDICT1)) != 0)
        {
          DebugLog.log("preparing files for species prediction", enLogType.INFO);
          prepareFolder();
          if ((options & OPT_CUT) != 0)
            args += " --cut --sampleRate " + _settings.SamplingRate.ToString();
          if ((options & OPT_PREPARE) != 0)
            args += " --prepPredict";
          if ((options & OPT_PREDICT1) != 0)
            args += " --predict";
          args += " --csvcalls " + reportName +
               " --root " + _settings.ModelDir + " --specFile " + _settings.SpeciesFile +
               " --dataDir " + _selectedDir +
               " --data " + datFile +
               " --model " + _settings.ModelType1.ToString() +
               " --predCol " + Cols.SPECIES;
          retVal = _proc.LaunchCommandLineApp(_settings.PythonBin, null, wrkDir, true, args, true, true);
        }

        if ((options & OPT_PREDICT2) != 0)
        {
          DebugLog.log("species prediction with model 2", enLogType.INFO);
          if ((options & OPT_PREDICT2) != 0)
            args += " --predict";
          args += " --csvcalls " + reportName +
               " --root " + _settings.ModelDir + " --specFile " + _settings.SpeciesFile +
               " --dataDir " + _selectedDir +
               " --data " + datFile +
               " --model " + _settings.ModelType2.ToString() +
               " --predCol " + Cols.SPECIES2;
          retVal = _proc.LaunchCommandLineApp(_settings.PythonBin, null, wrkDir, true, args, true, true);
        }

        if ((options & OPT_CONF95) != 0)
        {
          DebugLog.log("executing confidence test prediction", enLogType.INFO);
          _analysis.read(PrjPath + AppParams.PRJ_REPORT);
          _analysis.checkConfidence(_speciesInfos);
          _analysis.save(PrjPath + AppParams.PRJ_REPORT);
          _analysis.read(PrjPath + AppParams.PRJ_REPORT);
        }

        if ((options & OPT_CLEANUP) != 0)
        {
          DebugLog.log("cleaning up temporary files", enLogType.INFO);
          cleanupTempFiles();
        }
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
          SpeciesItem item = new SpeciesItem();
          item.Abbreviation = s.Abbreviation;
          item.Name = s.Local;
          item.CharFreqMin = s.FreqCharMin;
          item.ChaFreqMax = s.FreqCharMax;
          item.DurationMin = s.DurationMin;
          item.DurationMax = s.DurationMax;
          item.CallDistMin = s.CallDistMin;
          item.CallDistMax = s.CallDistMax;
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

    bool isBusy()
    {
      bool retVal = false;

      retVal = _proc.IsRunning | _extBusy;
      return retVal;
    }

    private void addSpeciesColsToReport(string report, string speciesFile)
    {
      Csv spec = new Csv();
      Csv rep = new Csv();
      spec.read(speciesFile, ";", true);
      rep.read(report, ";", true);
      if (rep.findInRow(1, "----") < 1)
      {
        int c = rep.ColCnt + 1;
        rep.insertCol(c, "", "----");
      }
      if (rep.findInRow(1, Cols.SPECIES) < 1)
      {
        int c = rep.ColCnt + 1;
        rep.insertCol(c, "", Cols.SPECIES);
      }
      if (rep.findInRow(1, Cols.SPECIES2) < 1)
      {
        int c = rep.findInRow(1, Cols.SPECIES) + 1;
        rep.insertCol(c, "", Cols.SPECIES2);
      }

      for (int r = 2; r <= spec.RowCnt; r++)
      {
        string species = spec.getCell(r, 1);
        if (rep.findInRow(1, species) < 1)
        {
          int c = rep.ColCnt + 1;
          rep.insertCol(c, "", species);
        }
      }
      rep.save();
    }

    private void createDir(string subDir, bool delete)
    {
      string dir = _selectedDir + "/" + subDir;
      if (!Directory.Exists(dir))
        Directory.CreateDirectory(dir);
      if (delete)
      {
        System.IO.DirectoryInfo di = new DirectoryInfo(dir);

        foreach (FileInfo file in di.GetFiles())
        {
          file.Delete();
        }
        foreach (DirectoryInfo d in di.GetDirectories())
        {
          d.Delete(true);
        }
      }
    }

    private void removeDir(string subDir)
    {
      string dir = _selectedDir + "/" + subDir;
      try
      {
        if (Directory.Exists(dir))
          Directory.Delete(dir, true);
      }
      catch (Exception ex)
      {
        DebugLog.log("problems deleting dir: " + dir + ", " + ex.ToString(), enLogType.ERROR);
      }
    }


    private void prepareFolder(bool delete = true)
    {
      createDir("bat", delete);
      createDir("wav", delete);
      createDir("dat", delete);
      createDir("log", delete);
      createDir("img", delete);
    }


    private void cleanupTempFiles()
    {
      removeDir("bat");
      removeDir("wav");
      removeDir("dat");
      removeDir("log");
      removeDir("img");
      var dir = new DirectoryInfo(_selectedDir);
      foreach (var file in dir.EnumerateFiles("*.npy"))
      {
        file.Delete();
      }
    }
  }
}
