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
    
    public string WavFilePath { get { return _selectedDir + "/" + _prj.WavSubDir; } }
    
    public string PrjPath { get { return _selectedDir; } }
    
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

  //  public ScatterDiagram ScatterDiagram { get { return _scatterDiagram; } set { _scatterDiagram = value; } }

    public ViewModel(Forms.MainWindow mainWin, string version)
    {
      _batSpecRegions = BatSpeciesRegions.load();
      _proc = new ProcessRunner();
      _mainWin = mainWin;
      _filter = new Filter();
      _speciesInfos = BatInfo.load().Species;
      _version = version;
      _colorTable = new ColorTable(this);
      _colorTable.createColorLookupTable();
      _zoom = new ZoomView(_colorTable);
      _wav = new WavFile();
      _clsBarataud = new ClassifierBarataud(_batSpecRegions);
      _sumReport = new SumReport(_speciesInfos);
      _prj = new Project(_batSpecRegions, _speciesInfos);
      _models = new List<BaseModel>();
      int index = 0;
      foreach(ModelItem m in AppParams.Inst.Models)
      {
        _models.Add(BaseModel.Create(index, m.ModelType));
        index++;
      }
      UpdateUi = false;
    }

    public void updateReport()
    {
      if (File.Exists(Prj.ReportName))
        _prj.Analysis.read(Prj.ReportName);
    }


    public void initProject(DirectoryInfo dir)
    {
      if (Project.containsProject(dir))
      {
        _selectedDir = dir.FullName + "/";
        string[] files = System.IO.Directory.GetFiles(dir.FullName, "*.bpr",
                         System.IO.SearchOption.TopDirectoryOnly);
        if (_prj == null)
          _prj = new Project(_batSpecRegions, _speciesInfos);
        _prj.readPrjFile(files[0]);
        if (File.Exists(Prj.ReportName))
        {
          _prj.Analysis.read(_prj.ReportName);
          _prj.Analysis.openSummary(_prj.SummaryName); ;
        }
        else
          _prj.Analysis.init(_prj);
        if (_prj.Analysis.Report != null)
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
        DebugLog.log("report and project file are consistent", enLogType.INFO);
      else
        DebugLog.log("mismatch between project file and report, please check", enLogType.ERROR, true);
    }


    public void loadSettings()
    {
      AppParams.load();
      _filter.Items.Clear();
      foreach(FilterParams p in AppParams.Inst.Filter)
      {
        FilterItem it = new FilterItem
        {
          Index = _filter.Items.Count,
          Name = p.Name,
          Expression = p.Expression,
          IsForAllCalls = p.isForAllCalls
        };
        _filter.Items.Add(it);
      }
      string scriptDir = AppParams.Inst.AppRootPath + "/" + AppParams.Inst.ScriptDir;
      _scripter = new ScriptRunner(ref _proc, scriptDir, updateProgress, this);
      _prj.Analysis.init(_prj);
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

    public BitmapImage getFtImage(BatExplorerProjectFileRecordsRecord rec, out bool newImage)
    {
      string fullName = _selectedDir + "/" + _prj.WavSubDir + "/" + rec.File;
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
        Waterfall wf = new Waterfall(_selectedDir + "/" + _prj.WavSubDir + "/" + rec.File, AppParams.Inst.FftWidth,
                                     AppParams.Inst.WaterfallWidth, AppParams.Inst.WaterfallHeight,  _colorTable);
        if (wf.Ok)
        {
          wf.generateFtDiagram(0, (double)wf.Samples.Length / wf.SamplingRate, AppParams.Inst.FftWidth);
          bmp = wf.generateFtPicture(0, wf.SamplingRate/2000);
          bmp.Save(pngName);
          newImage = true;
        }
        else
        {
           DebugLog.log("File '" + rec.File + "'does not exist, removed from project and report", enLogType.WARNING);
          _prj.removeFile(rec.File);
          _prj.Analysis.removeFile(Prj.ReportName, rec.File);
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
      _scriptName = path;
      int retVal = _scripter.RunScript(path, true, initVars);
      return retVal;
    }

    public void cancelScript()
    {
      _scripter.cancelExecution();
      DebugLog.log("script execution cancelled manually", enLogType.INFO);
    }

    public void editScript(string name)
    {
      string exe = AppParams.Inst.ExeEditor;
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

      _prj.Analysis.save(PrjPath);
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
        string dirName = _selectedDir + "/" + _prj.WavSubDir;
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
        _prj.Analysis.removeFile(_prj.ReportName, wavName);
      }
    }

    /// <summary>
    /// start evaluation of bat species
    /// </summary>
    public int startEvaluation()
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
      bool retVal = _proc.IsRunning | _extBusy;
      return retVal;
    }
  }
}
