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
    
   public const string COL_NAME = "name";
    public const string COL_NR = "nr";
    public const string COL_SAMPLERATE = "sampleRate";
    public const string COL_FILE_LEN = "FileLen";
    public const string COL_FREQ_MAX_AMP = "freq_max_amp";
    public const string COL_FREQ_MIN = "freq_min";
    public const string COL_FREQ_MAX = "freq_max";
    public const string COL_FREQ_KNEE = "freq_knee";
    public const string COL_DURATION = "duration";
    public const string COL_START_TIME = "start";
    public const string COL_SNR = "snr";
    public const string COL_SPECIES = "Species";
    public const string COL_SPECIES_MAN = "SpeciesMan";
    public const string COL_PROBABILITY = "prob";
    public const string COL_REMARKS = "remarks";

    int _colSampleRate = 0;
    int _colNr = 0;
    int _colFileLen = 0;
    int _colName = 0;
    int _colFreqMaxAmp = 0;
    int _colFreqMax = 0;
    int _colFreqMin = 0;
    int _colFreqKnee = 0;
    int _colDuration = 0;
    int _colStartTime = 0;
    int _colSpecies = 0;
    int _colSpeciesMan = 0;
    int _colProbability = 0;
    int _colSnr = 0;
    int _colRemarks= 0;

    List<AnalysisFile> _list;
    List<ReportItem> _report;

    public List<ReportItem> Report { get { return _report; } }

    public bool Changed
    {
      get
      {
        bool retVal = false;
        if (_list != null)
        {
          foreach (AnalysisFile f in _list)
          {
            retVal |= f.Changed;
          }
        }

        if (_report != null)
        {
          foreach (ReportItem r in _report)
          {
            retVal |= r.Changed;
          }
        }
        return retVal;
      }
    }

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
      _colNr = csv.findInRow(1, COL_NR);
      _colFreqMaxAmp = csv.findInRow(1, COL_FREQ_MAX_AMP);
      _colFreqMax = csv.findInRow(1, COL_FREQ_MAX);
      _colFreqMin = csv.findInRow(1, COL_FREQ_MIN);
      _colFreqKnee = csv.findInRow(1, COL_FREQ_KNEE);
      _colDuration = csv.findInRow(1, COL_DURATION);
      _colStartTime = csv.findInRow(1, COL_START_TIME);
      _colSpecies = csv.findInRow(1, COL_SPECIES);
      _colProbability = csv.findInRow(1, COL_PROBABILITY);
      _colSnr = csv.findInRow(1, COL_SNR);
      _colSpeciesMan = csv.findInRow(1, COL_SPECIES_MAN);
      _colRemarks = csv.findInRow(1, COL_REMARKS);

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
        if(_colSpeciesMan == 0)
        {
          int col = csv.ColCnt + 1;
          csv.insertCol(col, "todo");
          csv.setCell(1, col, COL_SPECIES_MAN);
          _colSpeciesMan = col;
          csv.save();
        }
        if (_colRemarks == 0)
        {
          int col = csv.ColCnt + 1;
          csv.insertCol(col, "");
          csv.setCell(1, col, COL_REMARKS);
          _colRemarks = col;
          csv.save();
        }
      }
      //@@@

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
        int nr = csv.getCellAsInt(row, _colNr);
        double fMin = csv.getCellAsDouble(row, _colFreqMin);
        double fMax = csv.getCellAsDouble(row, _colFreqMax);
        double fKnee = csv.getCellAsDouble(row, _colFreqKnee);
        double duration = csv.getCellAsDouble(row, _colDuration);
        string startTime = csv.getCell(row, _colStartTime);
        string species = csv.getCell(row, _colSpecies);
        double probability = csv.getCellAsDouble(row, _colProbability);
        double snr = csv.getCellAsDouble(row, _colSnr);
        string speciesMan = csv.getCell(row, _colSpeciesMan);
        file.Remarks = csv.getCell(row, _colRemarks);

        AnalysisCall call = new AnalysisCall(nr, fMaxAmp, fMin, fMax, fKnee, duration, startTime, species, 
                                             probability, speciesMan, snr);
        file.Calls.Add(call);

        ReportItem rItem = new ReportItem();
        rItem.FileName = fName;
        rItem.CallNr = callNr.ToString();
        if (callNr < 2)
          rItem.Remarks = file.Remarks;
        callNr++;
        rItem.FreqMin = (fMin / 1000).ToString("0.#");
        rItem.FreqMax = (fMax / 1000).ToString("0.#");
        rItem.FreqMaxAmp = (fMaxAmp/1000).ToString("0.#");
        rItem.Duration = duration.ToString("0.#");
        rItem.StartTime = startTime;
        rItem.SpeciesAuto = species;
        rItem.Probability = probability.ToString("0.###");
        rItem.Snr = snr.ToString();
        rItem.SpeciesMan = speciesMan;
        _report.Add(rItem);
      }
    }

    public void save(string fileName)
    {
      Csv csv = new Csv();
      csv.read(fileName);
      foreach(AnalysisFile f in _list)
      {
        foreach(AnalysisCall c in f.Calls)
        {
          int row = csv.findInCol(f.FileName, _colName, c.Nr.ToString(), _colNr);
          csv.setCell(row, _colSpeciesMan, c.SpeciesMan);
          if(c.Nr < 2)
            csv.setCell(row, _colRemarks, f.Remarks);
          c.resetChanged();
        }
      }
      csv.save();
      foreach(ReportItem r in _report)
      {
        r.resetChanged();
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
    bool _changed = false;
    string _remarks;
    public bool Changed { get; }
    public string FileName { get; set; }
    public string CallNr { get; set; }
    public string StartTime { get; set; }
    public string Duration { get; set; }
    public string FreqMin { get; set; }
    public string FreqMax { get; set; }
    public string FreqMaxAmp { get; set; }

    public string SpeciesAuto { get; set; }

    public string SpeciesMan { get; set; }
    public string Probability { get; set; }
    public string Snr { get; set; }
    public string Remarks { get { return _remarks; } set { _remarks = value; _changed = true; } }

    public void resetChanged()
    {
      _changed = false;
    }
  }

  public class AnalysisCall
  {

    bool _changed = false;
    string _speciesMan;
    public int Nr { get; }
    public double FreqMaxAmp { get; }
    public double FreqMin { get; }
    public double FreqMax { get; }
    public double FreqKnee { get; }
    public double Duration { get; }
    public String StartTime { get; }
    public double Probability { get; }

    public double Snr { get; }

    public string SpeciesAuto { get; }
    public string SpeciesMan { get { return _speciesMan; }  set { _speciesMan = value; _changed = true; } }

    public bool Changed { get { return _changed; } }

    public AnalysisCall(int nr, double freqMaxAmp, double freqMin, double freqMax, double freqKnee, double duration, string start, string speciesAuto, double probability, string speciesMan, double snr)
    {
      Nr = nr;
      FreqMaxAmp = freqMaxAmp;
      FreqMin = freqMin;
      FreqMax = freqMax;
      FreqKnee = freqKnee;
      Duration = duration;
      StartTime = start;
      SpeciesAuto = speciesAuto;
      Probability = probability;
      _speciesMan = speciesMan;
      Snr = snr;
      _changed = false;
    }

    public void resetChanged()
    {
      _changed = false;
    }
  }


  public class AnalysisFile
  {
    List<AnalysisCall> _calls;

    public string FileName { get; set; }
    public int SampleRate { get; set; }
    public double Duration { get; set; }
    public string Remarks { get; set; }

    public bool Changed 
    {
    get
      {
        bool retVal = false;
        foreach(AnalysisCall c in _calls)
        {
          retVal |= c.Changed;
        }
        return retVal;
      }
    }

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

    /// <summary>
    /// get start time[s] for call
    /// </summary>
    /// <param name="idx">index of call (0..n)</param>
    /// <returns>start time [s]</returns>
    public double getStartTime(int idx)
    {
      double retVal = 0;
      if ((idx >= 0) && (idx < _calls.Count))
      {
        int pos = _calls[idx].StartTime.LastIndexOf(':');
        if (pos >= 0)
        {
          string s = _calls[idx].StartTime.Substring(pos + 1);
          s = s.Replace(".", "");
          s = s.Replace(",", "");
          double.TryParse(s, out retVal);
          retVal /= 1000;
        }
      }
      return retVal;
    }

    public double getEndTime(int idx)
    {
      double retVal = 0;
      if ((idx >= 0) && (idx < _calls.Count))
      {
        retVal = getStartTime(idx);
        retVal += _calls[idx].Duration / 1000;
      }
      return retVal;
    }
  }
}
