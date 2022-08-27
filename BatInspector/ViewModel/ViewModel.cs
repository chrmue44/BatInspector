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
    public const int OPT_RESAMPLE = 0x20;
    public const int OPT_PREDICT2 = 0x40;
    public const int OPT_PREDICT3 = 0x80;
    public const int OPT_CLEANUP = 0x100;

    string _selectedDir;
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

    Forms.MainWindow _mainWin;
    public string WavFilePath { get { return _selectedDir + _prj.WavSubDir; } }
    public string PrjPath { get { return _selectedDir; } }

    public string ScriptName { get { return _scriptName; } }
    public string Version { get { return _version; } }
    public Analysis Analysis { get { return _analysis; } }

    public Project Prj { get { return _prj; } }

    //public ProcessRunner ProcRunner { get { return _proc; } }

    public ZoomView ZoomView { get { return _zoom; } }

    public Filter Filter { get { return _filter; } }

    public AppParams Settings { get { return _settings; }  }

    public ColorTable ColorTable { get { return _colorTable; } }

    public bool Busy { get { return isBusy(); } set { _extBusy = value; } }

    public WavFile WavFile { get { return _wav; } }

    public System.Windows.Input.Key LastKey { get; set; }
    public System.Windows.Input.Key KeyPressed { get; set; }
    public ViewModel(Forms.MainWindow mainWin, string version)
    {
      _proc = new ProcessRunner();
      _mainWin = mainWin;
      _prj = new Project();
      _filter = new Filter();
      _settings = new AppParams();
      _analysis = new Analysis(Settings.Species);
      _version = version;
      _colorTable = new ColorTable(this);
      _colorTable.createColorLookupTable();
      _zoom = new ZoomView(_colorTable);
      _wav = new WavFile();
    }

    public void updateReport()
    {
      if (File.Exists(_selectedDir + "report.csv"))
        _analysis.read(_selectedDir + "report.csv");
    }

    public void initProject(DirectoryInfo dir)
    {
      if (Project.containsProject(dir))
      {
        _selectedDir = dir.FullName + "/";
        if (File.Exists(_selectedDir + "report.csv"))
          _analysis.read(_selectedDir + "report.csv");
        string[] files = System.IO.Directory.GetFiles(dir.FullName, "*.bpr",
                         System.IO.SearchOption.TopDirectoryOnly);
        if (_prj == null)
          _prj = new Project();
        _prj.readPrjFile(files[0]);
        _scripter = new ScriptRunner(ref _proc, _selectedDir, null, this);
      }
      else if (Project.containsWavs(dir))
      {
        _prj = new Project();
        _prj.fillFromDirectory(dir);
        _selectedDir = dir.FullName + "/";
        _scripter = new ScriptRunner(ref _proc, _selectedDir, null, this);
      }
      else
        _prj = null;
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
      _scripter = new ScriptRunner(ref _proc, _settings.ScriptDir, null, this);
      _analysis = new Analysis(Settings.Species);
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
      string pngName = fullName.Replace(".wav", ".png");
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
          _analysis.removeFile(_selectedDir + "/report.csv", rec.File);
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

    public int executeScript(string path)
    {
      int retVal = 1;
      _scriptName = path;
      retVal = _scripter.RunScript(path);
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
      foreach (string f in files)
        deleteFile(f);
      DebugLog.log(files.Count.ToString() + " files deleted", enLogType.INFO);
    }

    public void removeDeletedWavsFromReport(string reportName)
    {
      if (File.Exists(reportName))
      {
        List<string> wavToDel = new List<string>();

        Csv report = new Csv();
        report.read(reportName);
        for (int r = 1; r <= report.RowCnt; r++)
        {
          string wavName = report.getCell(r, Analysis.Cols.getCol(Cols.NAME));

          BatExplorerProjectFileRecordsRecord rec = _prj.find(wavName);
          if (rec == null)
            wavToDel.Add(wavName);
        }

        foreach (string f in wavToDel)
          Analysis.removeWavFromReport(report, f);

        report.save();
      }
    }

    void deleteFile(string wavName)
    {
      if (_prj != null)
      {
        string dirName = _selectedDir + _prj.WavSubDir;
        string delName = wavName.Replace(".wav", ".*");
        int pos = delName.LastIndexOf("/");
        int pos2 = delName.LastIndexOf("\\");
        if ((pos >= 0) && (pos > pos2))
          delName = delName.Substring(pos + 1);
        else if (pos2 >= 0)
          delName = delName.Substring(pos2 + 1);
        IEnumerable<string> delFiles = Directory.EnumerateFiles(dirName, delName);
        foreach (string f in delFiles)
        {
          File.Delete(f);
          DebugLog.log("delete file " + f, enLogType.DEBUG);
        }

        _prj.removeFile(wavName);
        _prj.writePrjFile();
        _analysis.removeFile(_selectedDir +"/report.csv", wavName);
      }
    }

    /// <summary>
    /// start evaluation of bat species
    /// </summary>
    public int startEvaluation(int options)
    {
      int retVal = 2;
      if(Prj != null)
      {
        string reportName = _selectedDir + "report.csv";
        reportName = reportName.Replace("\\", "/");

        if ((options & OPT_INSPECT) != 0)
        {
          string scriptR = _settings.RScript; // "C:/Users/chrmu/prj/BatInspector/R/features.R";
          string argsR = scriptR + " " + _selectedDir + _prj.WavSubDir + " " + reportName + " " + _settings.SpeciesFile + " 312500";
          DebugLog.log("starting evaluation of calls: " + _settings.Rbin + " " + argsR, enLogType.INFO);
          if (File.Exists(reportName))
          {
            DebugLog.log("backup file: " + reportName, enLogType.INFO);
            try
            {
              File.Delete(reportName + "_old");
              File.Copy(reportName, reportName + "_old");
              File.Delete(reportName);
            }
            catch { }
          }
          retVal = _proc.LaunchCommandLineApp(_settings.Rbin, null, _selectedDir, true, argsR, true, true);
        }

        if ((options & (OPT_CUT | OPT_PREPARE | OPT_PREDICT1 | OPT_RESAMPLE)) != 0)
        {
          string datFile = _selectedDir + "/Xdata000.npy";
          string wrkDir = "C:/Users/chrmu/prj/BatInspector/py";
          string args = _settings.PythonScript;
          DebugLog.log("preparing files for species prediction", enLogType.INFO);
          prepareFolder();
          if ((options & OPT_RESAMPLE) != 0)
            args += " --resample " + _selectedDir + _prj.WavSubDir + "*.wav" +
                    " --sampleRate " + _settings.SamplingRate.ToString();
          if ((options & OPT_CUT) != 0)
            args += " --cut";
          if ((options & OPT_PREPARE) != 0)
            args += " --prepPredict";
          if ((options & OPT_PREDICT1) != 0)
            args += " --predict";
          args += " --csvcalls " + reportName +
               " --root " + _settings.ModelDir + " --specFile " + _settings.SpeciesFile +
               " --dataDir " + _selectedDir +
               " --data " + datFile;
          retVal = _proc.LaunchCommandLineApp(_settings.PythonBin, null, wrkDir, true, args, true, true);
        }

        if((options & OPT_CONF95) != 0)
        {
          DebugLog.log("executing confidence test prediction", enLogType.INFO);
          _analysis.read(PrjPath + "report.csv");
          _analysis.checkConfidence(_settings.Species);
          _analysis.save(PrjPath + "report.csv");
          _analysis.read(PrjPath + "report.csv");
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

      foreach(SpeciesInfos s in _settings.Species)
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
    }
  }

}
