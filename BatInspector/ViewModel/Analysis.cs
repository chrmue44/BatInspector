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
   /* const int COL_NAME = 1;
    const int COL_SAMPLERATE = 3;
    const int COL_FILE_LEN = 4;
    const int COL_FREQ_MAX_AMP = 5;
    const int COL_FREQ_MIN = 6;
    const int COL_FREQ_MAX = 7;
    const int COL_FREQ_KNEE = 8;
    const int COL_DURATION = 9;
    const int COL_START_TIME = 10;
    const int COL_SNR = 11;
    const int COL_SPECIES = 26; */
    
    const string COL_NAME = "name";
    const string COL_SAMPLERATE = "sampleRate";
    const string COL_FILE_LEN = "FileLen";
    const string COL_FREQ_MAX_AMP = "freq_max_amp";
    const string COL_FREQ_MIN = "freq_min";
    const string COL_FREQ_MAX = "freq_max";
    const string COL_FREQ_KNEE = "freq_knee";
    const string COL_DURATION = "duration";
    const string COL_START_TIME = "start";
    const string COL_SNR = "snr";
    const string COL_SPECIES = "Species";
    const string COL_PROBABILITY = "prob";

    int _colSampleRate = 0;
    int _colFileLen = 0;
    int _colName = 0;
    int _colFreqMaxAmp = 0;
    int _colFreqMax = 0;
    int _colFreqMin = 0;
    int _colFreqKnee = 0;
    int _colDuration = 0;
    int _colStartTime = 0;
    int _colSpecies = 0;
    int _colProbability = 0;
    int _colSnr = 0;

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

      _colSampleRate = csv.findInRow(1, COL_SAMPLERATE);
      _colFileLen = csv.findInRow(1, COL_FILE_LEN);
      _colName = csv.findInRow(1, COL_NAME);
      _colFreqMaxAmp = csv.findInRow(1, COL_FREQ_MAX_AMP);
      _colFreqMax = csv.findInRow(1, COL_FREQ_MAX);
      _colFreqMin = csv.findInRow(1, COL_FREQ_MIN);
      _colFreqKnee = csv.findInRow(1, COL_FREQ_KNEE);
      _colDuration = csv.findInRow(1, COL_DURATION);
      _colStartTime = csv.findInRow(1, COL_START_TIME);
      _colSpecies = csv.findInRow(1, COL_SPECIES);
      _colProbability = csv.findInRow(1, COL_PROBABILITY);
      _colSnr = csv.findInRow(1, COL_SNR);

      //@@@ temporary
      if (ret == 0)
      {
        if (_colSampleRate == 0)
        {
          _colSampleRate = 3;
          _colFileLen = 4;
          if (csv.getCellAsInt(2, 2) != 383500)
          {
            csv.insertCol(_colSampleRate, "383500");
            csv.insertCol(_colFileLen, "3.001");
          }
          csv.setCell(1, _colSampleRate, COL_SAMPLERATE);
          csv.setCell(1, _colFileLen, COL_FILE_LEN);
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
        string fName = csv.getCell(row, _colName);
        if (fName != lastFileName)
        {
          lastFileName = fName;
          file = new AnalysisFile(fName);
          _list.Add(file);
          callNr = 1;
        }
        file.SampleRate = csv.getCellAsInt(row, _colSampleRate);
        file.Duration = csv.getCellAsDouble(row, _colFileLen);
        double fMaxAmp = csv.getCellAsDouble(row, _colFreqMaxAmp);
        double fMin = csv.getCellAsDouble(row, _colFreqMin);
        double fMax = csv.getCellAsDouble(row, _colFreqMax);
        double fKnee = csv.getCellAsDouble(row, _colFreqKnee);
        double duration = csv.getCellAsDouble(row, _colDuration);
        string startTime = csv.getCell(row, _colStartTime);
        string species = csv.getCell(row, _colSpecies);
        double probability = csv.getCellAsDouble(row, _colProbability);
        double snr = csv.getCellAsDouble(row, _colSnr);

        AnalysisCall call = new AnalysisCall(fMaxAmp, fMin, fMax, fKnee, duration, startTime, species, probability, snr);
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
        rItem.Probability = probability.ToString("0.###");
        rItem.Snr = snr.ToString();
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
          row = report.findInCol(wavName, _colName);
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

    public string Probability { get; set; }
    public string Snr { get; set; }
  }

  public class AnalysisCall
  {
    public double FreqMaxAmp { get; }
    public double FreqMin { get; }
    public double FreqMax { get; }
    public double FreqKnee { get; }
    public double Duration { get; }
    public String StartTime { get; }
    public double Probability { get; }

    public double Snr { get; }

    public string Species { get; }
    public AnalysisCall(double freqMaxAmp, double freqMin, double freqMax, double freqKnee, double duration, string start, string species, double probability, double snr)
    {
      FreqMaxAmp = freqMaxAmp;
      FreqMin = freqMin;
      FreqMax = freqMax;
      FreqKnee = freqKnee;
      Duration = duration;
      StartTime = start;
      Species = species;
      Probability = probability;
      Snr = snr;
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
