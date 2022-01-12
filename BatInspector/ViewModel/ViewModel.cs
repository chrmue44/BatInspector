using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;

namespace BatInspector
{
  public class Cursor
  {
    public double Freq = 0;
    public double Time = 0;
    public bool Visible = false;
  }
   
  public class RulerData
  {
    public double Min = 0;
    public double Max = 100;
  }

  public class ViewModel
  {
    const int COL_NAME = 1;
    const int COL_SAMPLERATE = 3;
    const int COL_FILE_LEN = 4;
    const int COL_FREQ_MAX_AMP = 5;
    const int COL_FREQ_MIN = 6;
    const int COL_FREQ_MAX = 7;
    const int COL_FREQ_KNEE = 8;
    const int COL_DURATION = 9;
    const int COL_START_TIME = 10;
    const int COL_SNR = 11;

    string _selectedDir;
    BatExplorerProjectFile _batExplorerPrj;
    string _prjFileName;
    List<AnalysisFile> _analysis;
    ProcessRunner _proc;
    Forms.MainWindow _mainWin;
    public string WavFilePath { get { return _selectedDir + "Records/"; } }
    public string PrjPath { get { return _selectedDir; } }


    public BatExplorerProjectFile Prj { get { return _batExplorerPrj; } }

    public ViewModel(Forms.MainWindow mainWin)
    {
      _analysis = new List<AnalysisFile>();
      _proc = new ProcessRunner(DebugLog.log);
      _mainWin = mainWin;
    }

    public bool containsProject(DirectoryInfo dir)
    {
      bool retVal = false;
      string[] files = System.IO.Directory.GetFiles(dir.FullName, "*.bpr",
                       System.IO.SearchOption.TopDirectoryOnly);
      if (files.Length > 0)
        retVal = true;

      return retVal;
    }

    public void initProject(DirectoryInfo dir)
    {
      if(containsProject(dir))
      { 
        _selectedDir = dir.FullName + "/";
         if (File.Exists(_selectedDir + "report.csv"))
           readAnalysis(_selectedDir + "report.csv");
        string[] files = System.IO.Directory.GetFiles(dir.FullName, "*.bpr",
                         System.IO.SearchOption.TopDirectoryOnly);
        _prjFileName = files[0];
        readPrjFile(_prjFileName);
      }
      else
        _batExplorerPrj = null;
    }

    public BitmapImage getImage(BatExplorerProjectFileRecordsRecord rec, out bool newImage)
    {
      string fullName = _selectedDir + "Records/" + rec.File;
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
        Waterfall wf = new Waterfall(_selectedDir + "Records/" + rec.File, AppParams.FftWidth, AppParams.WaterfallWidth, AppParams.WaterfallHeight);
        if (wf.Ok)
        {
          wf.generateDiagram(0, (double)wf.Samples.Length / wf.SamplingRate, AppParams.FftWidth);
          bmp = wf.generatePicture(0, wf.SamplingRate/2000);
          bmp.Save(pngName);
          newImage = true;
        }
      }
      if(bmp != null)
        bImg = Convert(bmp);
      return bImg;
    }

    public void readPrjFile(string fName)
    {
      string xml = File.ReadAllText(fName);
      var serializer = new XmlSerializer(typeof(BatExplorerProjectFile));

      TextReader reader = new StringReader(xml);
      _batExplorerPrj = (BatExplorerProjectFile)serializer.Deserialize(reader);
    }

    public void writePrjFile()
    {
      if(_batExplorerPrj != null)
      {
        var serializer = new XmlSerializer(typeof(BatExplorerProjectFile));
        TextWriter writer = new StreamWriter(_prjFileName);
        serializer.Serialize(writer, _batExplorerPrj);
        writer.Close();
      }
    }

    public void deleteFile(string wavName)
    {
      if (_batExplorerPrj != null)
      {
        string dirName = _selectedDir + "/Records";
        string delName = wavName.Replace(".wav", ".*");
        foreach (string f in Directory.EnumerateFiles(dirName, delName))
        {
          File.Delete(f);
        }

        List<BatExplorerProjectFileRecordsRecord> list = _batExplorerPrj.Records.ToList();
        foreach (BatExplorerProjectFileRecordsRecord rec in list)
        {
          if (rec.File == wavName)
          {
            list.Remove(rec);
            break;
          }
        }
        _batExplorerPrj.Records = list.ToArray();
        writePrjFile();
        AnalysisFile fileToDelete = null;
        foreach (AnalysisFile f in _analysis)
        {
          if(f.FileName == wavName)
          {
            fileToDelete = f;
            break;
          }  
        }
        if (fileToDelete != null)
          _analysis.Remove(fileToDelete);

        string reportName = _selectedDir + "/report.csv";
        if (File.Exists(reportName))
        {
          Csv report = new Csv();
          report.read(reportName);
          int row = 0;
          do
          {
            row = report.findInCol(wavName, COL_NAME);
            if(row > 0)
            {
              report.removeRow(row);
            }
          } while (row > 0);
          report.save();
        }
      }
    }

    public AnalysisFile getAnalysis(string fileName)
    {
      AnalysisFile retVal = null;
      foreach(AnalysisFile f in _analysis)
      {
        if(f.FileName == fileName)
        {
          retVal = f;
          break;
        }
      }
      return retVal;
    }

    public void startEvaluation()
    {
      if(Prj != null)
      {
        string exe = "D:/bin/R-4.1.0/bin/Rscript.exe";
        string wrkDir = "D:/prj/bioacoustics";
        string args = "cm.R " + _selectedDir + "/Records " + _selectedDir + "/report.csv"; 
        _proc.LaunchCommandLineApp(exe, null, wrkDir, false, args, false, true);
      }
    }

    private void readAnalysis(string fileName)
    {
      Csv csv = new Csv();
      int ret = csv.read(fileName);

      // @@@ temporary to migrate to new format
      if(ret == 0)
      {
        if(csv.getCellAsInt(2, COL_SAMPLERATE) != 383500)
        {
          csv.insertCol(COL_SAMPLERATE, "383500");
          csv.insertCol(COL_FILE_LEN, "3.001");
          csv.save();
        }
      }
      //@@@@

      _analysis.Clear();
      string lastFileName = "";
      AnalysisFile file = new AnalysisFile();

      for (int row = 2; row <= csv.RowCnt; row++)
      {
        string fName = csv.getCell(row, COL_NAME);
        if (fName != lastFileName)
        {
          lastFileName = fName;
          file = new AnalysisFile(fName);
          _analysis.Add(file);
        }
        file.SampleRate = csv.getCellAsInt(row, COL_SAMPLERATE);
        file.Duration = csv.getCellAsDouble(row, COL_FILE_LEN);
        double fMaxAmp = csv.getCellAsDouble(row, COL_FREQ_MAX_AMP);
        double fMin = csv.getCellAsDouble(row, COL_FREQ_MIN);
        double fMax = csv.getCellAsDouble(row, COL_FREQ_MAX);
        double fKnee = csv.getCellAsDouble(row, COL_FREQ_KNEE);
        double duration = csv.getCellAsDouble(row, COL_DURATION);
        string startTime = csv.getCell(row, COL_START_TIME);
        AnalysisCall call = new AnalysisCall(fMaxAmp, fMin, fMax, fKnee, duration, startTime);
        file.Calls.Add(call);
      }
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
  }

  public class AnalysisCall
  {
    public double FreqMaxAmp { get; }
    public double FreqMin { get; }
    public double FreqMax { get; }
    public double FreqKnee { get; }
    public double Duration { get; }
    public String StartTime { get; }
    public AnalysisCall(double freqMaxAmp, double freqMin, double freqMax, double freqKnee, double duration, string start)
    {
      FreqMaxAmp = freqMaxAmp;
      FreqMin = freqMin;
      FreqMax = freqMax;
      FreqKnee = freqKnee;
      Duration = duration;
      StartTime = start;
    }
  }


  public class AnalysisFile
  {
    List<AnalysisCall> _calls;

    public string FileName { get; set; }
    public int SampleRate { get; set; }
    public double Duration { get; set; }

    public List<AnalysisCall> Calls { get { return _calls; } }

    public AnalysisFile()
    {
      _calls = new List<AnalysisCall>();
    }

    public AnalysisFile(string name)
    {
      FileName = name;
      _calls = new List<AnalysisCall>();
    }
  }

}
