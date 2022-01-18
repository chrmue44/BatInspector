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
    const int COL_SPECIES = 26;

    List<AnalysisFile> _list;
    List<ReportItem> _report;

    public List<ReportItem> Report { get { return _report; } }
    public Analysis()
    {
      _list = new List<AnalysisFile>();
      _report = null;
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
      int callNr = 1;
      _report = new List<ReportItem>();

      for (int row = 2; row <= csv.RowCnt; row++)
      {
        string fName = csv.getCell(row, COL_NAME);
        if (fName != lastFileName)
        {
          lastFileName = fName;
          file = new AnalysisFile(fName);
          _list.Add(file);
          callNr = 1;
        }
        file.SampleRate = csv.getCellAsInt(row, COL_SAMPLERATE);
        file.Duration = csv.getCellAsDouble(row, COL_FILE_LEN);
        double fMaxAmp = csv.getCellAsDouble(row, COL_FREQ_MAX_AMP);
        double fMin = csv.getCellAsDouble(row, COL_FREQ_MIN);
        double fMax = csv.getCellAsDouble(row, COL_FREQ_MAX);
        double fKnee = csv.getCellAsDouble(row, COL_FREQ_KNEE);
        double duration = csv.getCellAsDouble(row, COL_DURATION);
        string startTime = csv.getCell(row, COL_START_TIME);
        string species = csv.getCell(row, COL_SPECIES);
        AnalysisCall call = new AnalysisCall(fMaxAmp, fMin, fMax, fKnee, duration, startTime, species);
        file.Calls.Add(call);

        ReportItem rItem = new ReportItem();
        rItem.FileName = fName;
        rItem.CallNr = callNr.ToString();
        callNr++;
        rItem.FreqMin = (fMin / 1000).ToString("0.#");
        rItem.FreqMax = (fMax / 1000).ToString("0.#");
        rItem.FreqMaxAmp = (fMaxAmp/1000).ToString("0.#");
        rItem.Duration = duration.ToString("0.#");
        rItem.StartTime = startTime;
        rItem.Species = species;
        _report.Add(rItem);
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
      //remove from analysis file list
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

      // remove from report file
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

      // remove from report control
      if(_report != null)
      {
        List<ReportItem> list = new List<ReportItem>();
        foreach(ReportItem item in _report)
        {
          if (item.FileName == wavName)
            list.Add(item);
        }
        foreach (ReportItem item in list)
          _report.Remove(item);
      }
    }
  }

  public class ReportItem
  {
    public string FileName { get; set; }
    public string CallNr { get; set; }
    public string StartTime { get; set; }
    public string Duration { get; set; }
    public string FreqMin { get; set; }
    public string FreqMax { get; set; }
    public string FreqMaxAmp { get; set; }

    public string Species { get; set; }

  }

  public class AnalysisCall
  {
    public double FreqMaxAmp { get; }
    public double FreqMin { get; }
    public double FreqMax { get; }
    public double FreqKnee { get; }
    public double Duration { get; }
    public String StartTime { get; }

    public string Species { get; }
    public AnalysisCall(double freqMaxAmp, double freqMin, double freqMax, double freqKnee, double duration, string start, string species)
    {
      FreqMaxAmp = freqMaxAmp;
      FreqMin = freqMin;
      FreqMax = freqMax;
      FreqKnee = freqKnee;
      Duration = duration;
      StartTime = start;
      Species = species;
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
