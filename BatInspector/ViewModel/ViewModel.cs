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
    public const int OPT_INSPECT = 1;
    public const int OPT_CUT = 2;
    public const int OPT_PREPARE = 4;
    public const int OPT_PREDICT = 8;

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

    Forms.MainWindow _mainWin;
    public string WavFilePath { get { return _selectedDir + _prj.WavSubDir; } }
    public string PrjPath { get { return _selectedDir; } }

    public string Version { get { return _version; } }
    public Analysis Analysis { get { return _analysis; } }

    public Project Prj { get { return _prj; } }

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
      _analysis = new Analysis();
      _proc = new ProcessRunner();
      _mainWin = mainWin;
      _prj = new Project();
      _filter = new Filter();
      _settings = new AppParams();
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
        _scripter = new ScriptRunner(ref _proc, _selectedDir, null);
      }
      else if (Project.containsWavs(dir))
      {
        _prj = new Project();
        _prj.fillFromDirectory(dir);
        _selectedDir = dir.FullName + "/";
        _scripter = new ScriptRunner(ref _proc, _selectedDir, null);
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
        Waterfall wf = new Waterfall(_selectedDir + _prj.WavSubDir + rec.File, _settings.FftWidth, _settings.WaterfallWidth, _settings.WaterfallHeight, _settings, _colorTable);
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

    public void deleteFiles(List<string> files)
    {
      DebugLog.log("start deleting files", enLogType.INFO);
      foreach (string f in files)
        deleteFile(f);
      DebugLog.log(files.Count.ToString() + " files deleted", enLogType.INFO);
    }


    void deleteFile(string wavName)
    {
      if (_prj != null)
      {
        string dirName = _selectedDir + "/Records";
        string delName = wavName.Replace(".wav", ".*");
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
        
        string exeR = "\"C:/Program Files/R/R-4.2.0/bin/Rscript.exe\"";
        string reportName = _selectedDir + "report.csv";
        reportName = reportName.Replace("\\", "/");
        string specFile = "C:/Users/chrmu/bat/tierSta/species.csv";

        if ((options & OPT_INSPECT) != 0)
        {
          string scriptR = "C:/Users/chrmu/prj/BatInspector/R/features.R";
          string argsR = scriptR + " " + _selectedDir + "/Records " + reportName + " " + specFile + " 312500";
          DebugLog.log("starting evaluation of calls: " + exeR + " " + argsR, enLogType.INFO);
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
          retVal = _proc.LaunchCommandLineApp(exeR, null, _selectedDir, true, argsR, true, true);
        }

        if ((options & (OPT_CUT | OPT_PREPARE | OPT_PREDICT)) != 0)
        {
          string exePy = "\"C:/Program Files/Python310/python.exe\"";
          string datFile = _selectedDir + "/Xdata000.npy";
          string wrkDir = "C:/Users/chrmu/prj/BatInspector/py";
          string script = "C:/Users/chrmu/prj/BatInspector/py/batclass.py";
          string modDir = "C:/Users/chrmu/prj/BatInspector/mod_tsa";
          string args = script;
          prepareFolder();
          if ((options & OPT_CUT) != 0)
            args += " --cut";
          if ((options & OPT_PREPARE) != 0)
            args += " --prepPredict";
          if ((options & OPT_PREDICT) != 0)
            args += " --predict";
          args += " --csvcalls " + reportName +
               " --root " + modDir + " --specFile " + specFile +
               " --dataDir " + _selectedDir +
               " --data " + datFile;
          retVal = _proc.LaunchCommandLineApp(exePy, null, wrkDir, true, args, true, true);
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
    private void prepareFolder(bool delete = true)
    {
      createDir("bat", delete);
      createDir("wav", delete);
      createDir("dat", delete);
      createDir("log", delete);
      createDir("img", delete);
    }
  }

}
