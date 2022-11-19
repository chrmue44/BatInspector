/********************************************************************************
 *               Author: Christian Müller
 *      Date of cration: 2021-08-10                                       
 *   Copyright (C) 2022: Christian Müller christian(at)chrmue(dot).de
 *
 *              Licence:
 * 
 * THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND,
 * EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
 ********************************************************************************/
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using libParser;
using libScripter;

namespace BatInspector
{

   public class Cols
   {
    public const string NAME = "name";
    public const string NR = "nr";
    public const string SAMPLERATE = "sampleRate";
    public const string FILE_LEN = "FileLen";
    public const string F_MAX_AMP = "freq_max_amp";
    public const string F_MIN = "freq_min";
    public const string F_MAX = "freq_max";
    public const string F_KNEE = "freq_knee";
    public const string DURATION = "duration";
    public const string START_TIME = "start";
    public const string SNR = "snr";
    public const string SPECIES = "Species";
    public const string SPECIES_MAN = "SpeciesMan";
    public const string PROBABILITY = "prob";
    public const string REMARKS = "remarks";
    public const string BANDWIDTH = "bandwidth";
    public const string F_START = "freq_start";
    public const string F_CENTER = "freq_center";
    public const string F_END = "freq_end";
    public const string FC = "fc";
    public const string F_BW_KNEE_FC = "freq_bw_knee_fc";
    public const string BIN_MAX_AMP = "bin_max_amp";
    public const string PC_F_MAX_AMP = "pc_freq_max_amp";
    public const string PC_F_MAX = "pc_freq_max";
    public const string PC_F_MIN = "pc_freq_min";
    public const string PC_KNEE = "pc_knee";
    public const string TEMP_BW_KNEE_FC = "temp_bw_knee_fc";
    public const string SLOPE = "slope";
    public const string KALMAN_SLOPE = "kalman_slope";
    public const string CURVE_NEW = "curve_neg";
    public const string CURVE_POS_START = "curve_pos_start";
    public const string CURVE_POS_END = "curve_pos_end";
    public const string MID_OFFSET = "mid_offset";
    public const string SMOTTHNESS = "smoothness";
    public const string REC_TIME = "recTime";
  }

  public class Analysis
  {

    Cols _cols;
    List<SpeciesInfos> _specList;
    object _fileLock = new object();
    List<AnalysisFile> _list;
    List<ReportItem> _report;
    Csv _csv;

    public List<AnalysisFile> Files { get { return _list; } }
    public Cols Cols { get { return _cols; } }

    public List<ReportItem> Report { get { return _report; } }

    public Analysis(List<SpeciesInfos> specList)
    {
      _list = new List<AnalysisFile>();
      _report = null;
      _cols = new Cols();
      _csv = new Csv();
      _specList = specList;
    }

    public AnalysisFile find(string name)
    {
      AnalysisFile retVal = null;
      foreach (AnalysisFile f in _list)
      {
        string fName = f.getString(Cols.NAME);
        if (fName.IndexOf(name) >= 0)
        {
          retVal = f;
          break;
        }
      }
      return retVal;
    }

    public bool Changed
    {
      get
      {
        bool retVal = _csv.Changed;

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

    public void read(string fileName)
    {
      lock (_fileLock)
      {
        _csv = new Csv();
        int ret = _csv.read(fileName, ";", true);

        //_cols.init(csv);

        //@@@ temporary
        if (ret == 0)
        {
          if (_csv.getColNr(Cols.SAMPLERATE) < 1)
          {
            _csv.insertCol(2, "383500", Cols.SAMPLERATE);
            _csv.save();
          }
          if (_csv.getColNr(Cols.FILE_LEN) < 1)
          {
            _csv.insertCol(3, "3.001", Cols.FILE_LEN);
            _csv.save();
          }
          if (_csv.getColNr(Cols.SPECIES_MAN) < 1)
          {
            int col = _csv.ColCnt + 1;
            _csv.insertCol(col, "todo", Cols.SPECIES_MAN);
            _csv.save();
          }
          if (_csv.getColNr(Cols.REMARKS) < 1)
          {
            int col = _csv.ColCnt + 1;
            _csv.insertCol(col, "", Cols.REMARKS);
            _csv.save();
          }
        }
        if (_csv.getColNr(Cols.REC_TIME) < 1)
          filloutRecTime(_csv);

        _list.Clear();
        string lastFileName = "$$$";
        int callNr = 1;
        _report = new List<ReportItem>();
        double oldStartTime = 0.0;
        AnalysisFile file = null;
        for (int row = 2; row <= _csv.RowCnt; row++)
        {
          string fName = _csv.getCell(row, Cols.NAME);
          string timStr = _csv.getCell(row, Cols.REC_TIME);
          double startTime = _csv.getCellAsDouble(row, Cols.START_TIME);
          DateTime recTime = AnyType.getDate(timStr); 
          if (fName != lastFileName)
          {
            lastFileName = fName;
            file = new AnalysisFile(_csv, row, recTime);
            _list.Add(file);
            callNr = 1;
          }
         
          AnalysisCall call = new AnalysisCall(_csv, row);
          if(callNr == 1)
          {
            oldStartTime = 0;
            call.DistToPrev = 0;
          }
          else
          {
            call.DistToPrev = startTime - oldStartTime;
            oldStartTime = startTime;
          }
          bool isInList = SpeciesInfos.isInList(_specList, call.getString(Cols.SPECIES));
          file.addCall(call, isInList);

          ReportItem rItem = new ReportItem();
          rItem.FileName = fName;
          rItem.CallNr = callNr.ToString();
          if (callNr < 2)
            rItem.Remarks = file.getString(Cols.REMARKS);
          callNr++;
          rItem.FreqMin = (call.getDouble(Cols.F_MIN) / 1000).ToString("0.#", CultureInfo.InvariantCulture);
          rItem.FreqMax = (call.getDouble(Cols.F_MAX) / 1000).ToString("0.#", CultureInfo.InvariantCulture);
          rItem.FreqMaxAmp = (call.getDouble(Cols.F_MAX_AMP)/ 1000).ToString("0.#", CultureInfo.InvariantCulture);
          rItem.Duration = call.getDouble(Cols.DURATION).ToString("0.#", CultureInfo.InvariantCulture);
          rItem.StartTime = call.getString(Cols.START_TIME);
          rItem.SpeciesAuto = call.getString(Cols.SPECIES);
          rItem.Probability = call.getDouble(Cols.PROBABILITY).ToString("0.###", CultureInfo.InvariantCulture);
          rItem.Snr = call.getDouble(Cols.SNR).ToString();
          rItem.SpeciesMan = call.getString(Cols.SPECIES_MAN);
          _report.Add(rItem);
        }
        restChanged();
      }
    }


    public void save(string fileName)
    {
      lock (_fileLock)
      {
        _csv.save();
        if (_report != null)
        {
          foreach (ReportItem r in _report)
          {
            r.resetChanged();
          }
        }
      }
    }

    public AnalysisFile getAnalysis(string fileName)
    {
      AnalysisFile retVal = null;
      foreach (AnalysisFile f in _list)
      {
        string fName = f.getString(Cols.NAME);
        if (fName.IndexOf(fileName) >= 0)
        {
          retVal = f;
          break;
        }
      }
      return retVal;
    }


    public void removeFile(string reportName, string wavName, bool doReadSave = true, Csv report = null)
    {
      lock (_fileLock)
      {
        if(doReadSave)
          save(reportName);

        //remove from analysis file list
        AnalysisFile fileToDelete = AnalysisFile.find(_list, wavName);
        
        if (fileToDelete != null)
          _list.Remove(fileToDelete);

        // remove from report file
        if (File.Exists(reportName))
        {
          if (doReadSave)
          {
            report = new Csv();
            report.read(reportName,";",true);
          }
          bool res = removeWavFromReport(report, wavName);
          if(res && doReadSave)
            report.save();
        }
        else
        {
          DebugLog.log("Analysis::removeFile: report file: " + reportName + " not found", enLogType.ERROR);
        }

        // remove from report control
        if (_report != null)
        {
          List<ReportItem> list = new List<ReportItem>();
          foreach (ReportItem item in _report)
          {
            if (item.FileName == wavName)
              list.Add(item);
          }
          foreach (ReportItem item in list)
            _report.Remove(item);
        }
      }
    }

    public void checkConfidence(List<SpeciesInfos> species)
    {
      foreach (AnalysisFile f in _list)
        f.checkConfidence(species);
    }


    public bool removeWavFromReport(Csv report, string wavName)
    {
      int row = 0;
      bool retVal = false;
      do
      {
        row = report.findInCol(wavName, Cols.NAME, true);
        if (row > 0)
        {
          report.removeRow(row);
          retVal = true;
        }
      } while (row > 0);
      return retVal;
    }

    /// <summary>
    /// calculates the time difference bewtween to times
    /// </summary>
    /// <param name="time1"></param>
    /// <param name="time2"></param>
    /// <returns>time difference in ms</returns>
    double calcDistToPrev(string time1, string time2)
    {
      double retVal = 0.0;
      string[] split1 = time1.Split(':');
      string[] split2 = time2.Split(':');
      if ((split1.Length == 3) && (split2.Length == 3))
      {
        double t1 = 0.0;
        double.TryParse(split1[2], NumberStyles.Any, CultureInfo.InvariantCulture, out t1);
        double t2 = 0.0;
        double.TryParse(split2[2], NumberStyles.Any, CultureInfo.InvariantCulture, out t2);
        retVal = Math.Abs(t1 - t2) * 1000;
      }
      return retVal;
    }


    void filloutRecTime(Csv csv)
    {
      csv.insertCol(2, "", Cols.REC_TIME);
      string oldF = "$$$";
      DateTime time = new DateTime();
      string dateStr = "";

      Stopwatch sw = new Stopwatch();
      sw.Start();
      for (int r = 2; r <= csv.RowCnt; r++)
      {
        string fName = csv.getCell(r, Cols.NAME);
        if (fName != oldF)
        {
          string xmlName = fName.Replace(".wav", ".xml");
          BatRecord rec = ElekonInfoFile.read(xmlName);
          if (rec != null)
          {
            try
            {
              time = DateTime.ParseExact(rec.DateTime, "dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
            }
            catch { }
            dateStr = AnyType.getTimeString(time);
          }
          else
            DebugLog.log("error reading file: " + xmlName, enLogType.ERROR);
        }
        csv.setCell(r, Cols.REC_TIME, dateStr);
      }
      csv.save();
      sw.Stop();
      DebugLog.log("read out start time of recordings, exec time: " + sw.Elapsed.ToString(), enLogType.INFO);
    }

    void restChanged()
    {
      if (_report != null)
      {
        foreach (ReportItem r in _report)
        {
          r.resetChanged();
        }
      }

    }

  }

  public class ReportItem
  {
    bool _changed = false;
    string _remarks;
    public bool Changed { get { return _changed; } }
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
    public string Remarks
    {
      get { return _remarks; }
      set
      {
        _remarks = value;
        _changed = true;
      }
    }

    public void resetChanged()
    {
      _changed = false;
    }
  }

  public class AnalysisCall
  {
    Csv _csv;
    int _row;
   
    public double DistToPrev { get; set; }

    public AnalysisCall(Csv csv, int row)
    {
      _csv = csv;
      _row = row;
    }

    public double getDouble(string key)
    {
      return _csv.getCellAsDouble(_row, key);
    }

    public int getInt(string key)
    {
      return _csv.getCellAsInt(_row, key);  
    }

    public string getString(string key)
    {
      return _csv.getCell(_row, key);
    }

    public void setString(string key, string value)
    {
      _csv.setCell(_row, key, value);
    }


    public bool checkConfidence(List<SpeciesInfos> species)
    {
      bool retVal = false;
      string speciesAuto = getString(Cols.SPECIES);
      double duration = getDouble(Cols.DURATION);
      double fMin = getDouble(Cols.F_MIN);
      double fMax = getDouble(Cols.F_MAX);
      SpeciesInfos spec = SpeciesInfos.find(speciesAuto, species);
      string err = "";
      if(spec != null)
      {
        if (duration < spec.DurationMin)
          err = "Dmin";
        else if (duration > spec.DurationMax)
          err = "Dmax";
//        else if (this.FreqMaxAmp < (spec.FreqCharMin * 1000))
//          err = "FcMin";
//        else if (this.FreqMaxAmp > (spec.FreqCharMax * 1000))
//          err = "FcMax";
        else if (fMin < (spec.FreqMinMin * 1000))
          err = "FminMin";
        else if (fMin > (spec.FreqMinMax * 1000))
          err = "FminMax";
        else if (fMax < (spec.FreqMaxMin * 1000))
          err = "FminMin";
        //     else if (this.FreqMax > (spec.FreqMaxMax * 1000))
        //       err = "FmaxMax";
        if (err == "")
          retVal = true;
        else
        {
          speciesAuto = "??C95[" + err + "," + speciesAuto + "]";
          _csv.setCell(_row, Cols.SPECIES, speciesAuto);
        }
      }
      return retVal;
    }
  }


  public class AnalysisFile
  {
    List<AnalysisCall> _calls;

    int _startRow;

    Csv _csv;
    DateTime _recTime;

    public bool Selected { get; set; } = false;

    public DateTime RecTime { get;  }

    Dictionary<string, int> _specFound;

    public List<AnalysisCall> Calls { get { return _calls; } }


    // fake analysis for projects without report
    public AnalysisFile(string name, int sampleRate, double duration)
    {
      _calls = new List<AnalysisCall>();
      _specFound = new Dictionary<string, int>();
      string header = Cols.NAME + ";" + Cols.SAMPLERATE + ";" + Cols.DURATION;
      _csv = new Csv(header);
      _csv.addRow();
      _startRow = 2;
      _csv.setCell(_startRow, Cols.NAME, name);
      _csv.setCell(_startRow, Cols.DURATION, duration);
      _csv.setCell(_startRow, Cols.SAMPLERATE,sampleRate);
    }



    public AnalysisFile(Csv csv, int startRow, DateTime recTime)
    {
      _calls = new List<AnalysisCall>();
      _specFound = new Dictionary<string, int>();
      _startRow = startRow;
      _csv = csv;
      _recTime = recTime;
    }

    public void setString(string key, string value)
    {
       _csv.setCell(_startRow, key, value);
    }

    public string getString(string key)
    {
      return _csv.getCell(_startRow, key);
    }

    public double getDouble(string key)
    {
      return _csv.getCellAsDouble(_startRow, key);
    }

    public int getInt(string key)
    {
      return _csv.getCellAsInt(_startRow, key);
    }

    /// <summary>
    /// get start time[s] for call
    /// </summary>
    /// <param name="idx">index of call (0..n)</param>
    /// <returns>start time [s]</returns>
    public double getStartTime(int idx)
    {
      double retVal = 0;
      string startTime = _calls[idx].getString(Cols.START_TIME);
      if ((idx >= 0) && (idx < _calls.Count))
      {
        int pos = startTime.LastIndexOf(':');
        if (pos >= 0)
        {
          string s = startTime.Substring(pos + 1);
          s = s.Replace(".", "");
          s = s.Replace(",", "");
          double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture,  out retVal);
          retVal /= 1000;
        }
        else
        {
          double.TryParse(startTime, NumberStyles.Any, CultureInfo.InvariantCulture, out retVal);
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
        retVal += _calls[idx].getDouble(Cols.DURATION) / 1000;
      }
      return retVal;
    }

    public void checkConfidence(List<SpeciesInfos> species)
    {
      foreach (AnalysisCall c in _calls)
        c.checkConfidence(species);
    }

    static public AnalysisFile find(List<AnalysisFile> list, string fName)
    {
      AnalysisFile retVal = null;
      foreach(AnalysisFile f in list)
      {
        string fileName = f.getString(Cols.NAME);
        if(fileName.Contains(fName))
        {
          retVal = f;
          break;
        }
      }
      return retVal;
    }

    public void addCall(AnalysisCall call, bool isInList)
    {
      _calls.Add(call);
      if (isInList)
      {
        string spec = call.getString(Cols.SPECIES);
        if (_specFound.ContainsKey(spec))
          _specFound[spec] += 1;
        else
          _specFound.Add(spec, 1);
      }
    }

    public int getNrOfAutoSpecies()
    {
      return _specFound.Count;
    }

    public KeyValuePair<string,int> getSpecies(int rank)
    {
      KeyValuePair<string, int> retVal = new KeyValuePair<string, int>("????", 0);
      var myList = _specFound.ToList();
      myList.Sort((pair1, pair2) => pair1.Value.CompareTo(pair2.Value));
      int len = myList.Count;
      if (len >= rank)
        retVal = myList[len - rank];
      return retVal;
    }
  }
}
