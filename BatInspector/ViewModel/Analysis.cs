using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatInspector
{

  public class Analysis
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
    List<AnalysisFile> _list;

    public Analysis()
    {
      _list = new List<AnalysisFile>();
    }

    public void read(string fileName)
    {
      Csv csv = new Csv();
      int ret = csv.read(fileName);

      // @@@ temporary to migrate to new format
      if (ret == 0)
      {
        if (csv.getCellAsInt(2, COL_SAMPLERATE) != 383500)
        {
          csv.insertCol(COL_SAMPLERATE, "383500");
          csv.insertCol(COL_FILE_LEN, "3.001");
          csv.save();
        }
      }
      //@@@@

      _list.Clear();
      string lastFileName = "";
      AnalysisFile file = new AnalysisFile();

      for (int row = 2; row <= csv.RowCnt; row++)
      {
        string fName = csv.getCell(row, COL_NAME);
        if (fName != lastFileName)
        {
          lastFileName = fName;
          file = new AnalysisFile(fName);
          _list.Add(file);
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
    public AnalysisFile getAnalysis(string fileName)
    {
      AnalysisFile retVal = null;
      foreach (AnalysisFile f in _list)
      {
        if (f.FileName == fileName)
        {
          retVal = f;
          break;
        }
      }
      return retVal;
    }

    public void removeFile(string dir, string wavName)
    {
      AnalysisFile fileToDelete = null;
      foreach (AnalysisFile f in _list)
      {
        if (f.FileName == wavName)
        {
          fileToDelete = f;
          break;
        }
      }
      if (fileToDelete != null)
        _list.Remove(fileToDelete);

      string reportName = dir + "/report.csv";
      if (File.Exists(reportName))
      {
        Csv report = new Csv();
        report.read(reportName);
        int row = 0;
        do
        {
          row = report.findInCol(wavName, COL_NAME);
          if (row > 0)
          {
            report.removeRow(row);
          }
        } while (row > 0);
        report.save();
      }

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
