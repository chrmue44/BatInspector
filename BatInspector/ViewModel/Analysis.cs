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

//    Dictionary<string, int> _cols = new Dictionary<string, int>();


  /*  public void init(Csv csv)
    {
      _cols.Clear();
      _cols.Add(SAMPLERATE, csv.findInRow(1, SAMPLERATE));
      _cols.Add(FILE_LEN, csv.findInRow(1, FILE_LEN));
      _cols.Add(NAME, csv.findInRow(1, NAME));
      _cols.Add(NR, csv.findInRow(1, NR));
      _cols.Add(F_MAX_AMP, csv.findInRow(1, F_MAX_AMP));
      _cols.Add(F_MAX, csv.findInRow(1, F_MAX));
      _cols.Add(F_MIN, csv.findInRow(1, F_MIN));
      _cols.Add(F_KNEE, csv.findInRow(1, F_KNEE));
      _cols.Add(DURATION, csv.findInRow(1, DURATION));
      _cols.Add(START_TIME, csv.findInRow(1, START_TIME));
      _cols.Add(SPECIES, csv.findInRow(1, SPECIES));
      _cols.Add(PROBABILITY, csv.findInRow(1, PROBABILITY));
      _cols.Add(SNR, csv.findInRow(1, SNR));
      _cols.Add(SPECIES_MAN, csv.findInRow(1, SPECIES_MAN));
      _cols.Add(REMARKS, csv.findInRow(1, REMARKS));
    }

    public int getCol(string id)
    {
      int retVal = 0;
      bool ok = _cols.TryGetValue(id, out retVal);
      if (!ok)
        DebugLog.log("unknown column id " + id, enLogType.ERROR);
      return retVal;
    } 

    public void Add(string id, int val)
    {
      try
      {
        _cols.Add(id, val);
      }
      catch 
      {
        DebugLog.log("id '" + id + "'already present in dictionary", enLogType.ERROR);
      }
    } */
  }

  public class Analysis
  {

    Cols _cols;
    List<SpeciesInfos> _specList;
    object _fileLock = new object();
    List<AnalysisFile> _list;
    List<ReportItem> _report;

    public List<AnalysisFile> Files { get { return _list; } }
    public Cols Cols { get { return _cols; } }

    public List<ReportItem> Report { get { return _report; } }

    public Analysis(List<SpeciesInfos> specList)
    {
      _list = new List<AnalysisFile>();
      _report = null;
      _cols = new Cols();
      _specList = specList;
    }

    public AnalysisFile find(string name)
    {
      AnalysisFile retVal = null;
      foreach (AnalysisFile f in _list)
      {
        if (f.FileName.IndexOf(name) >= 0)
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

    public void read(string fileName)
    {
      lock (_fileLock)
      {
        Csv csv = new Csv();
        int ret = csv.read(fileName, ";", true);
        //_cols.init(csv);

        //@@@ temporary
        if (ret == 0)
        {
          if (csv.getColNr(Cols.SAMPLERATE) < 1)
          {
            csv.insertCol(2, "383500", Cols.SAMPLERATE);
            csv.save();
          }
          if (csv.getColNr(Cols.FILE_LEN) < 1)
          {
            csv.insertCol(3, "3.001", Cols.FILE_LEN);
            csv.save();
          }
          if (csv.getColNr(Cols.SPECIES_MAN) < 1)
          {
            int col = csv.ColCnt + 1;
            csv.insertCol(col, "todo", Cols.SPECIES_MAN);
            csv.save();
          }
          if (csv.getColNr(Cols.REMARKS) < 1)
          {
            int col = csv.ColCnt + 1;
            csv.insertCol(col, "", Cols.REMARKS);
            csv.save();
          }
        }
        if (csv.getColNr(Cols.REC_TIME) < 1)
          filloutRecTime(csv);

        _list.Clear();
        string lastFileName = "$$$";
        int callNr = 1;
        _report = new List<ReportItem>();

        AnalysisFile file = null;
        for (int row = 2; row <= csv.RowCnt; row++)
        {
          string fName = csv.getCell(row, Cols.NAME);
          string timStr = csv.getCell(row, Cols.REC_TIME);
          DateTime recTime = AnyType.getDate(timStr); 
          if (fName != lastFileName)
          {
            lastFileName = fName;
            file = new AnalysisFile(fName);
            file.RecTime = recTime;
            _list.Add(file);
            file.Remarks = csv.getCell(row, Cols.REMARKS);
            callNr = 1;
          }
          file.SampleRate = csv.getCellAsInt(row, Cols.SAMPLERATE);
          file.Duration = csv.getCellAsDouble(row, Cols.FILE_LEN);
          double fMaxAmp = csv.getCellAsDouble(row, Cols.F_MAX_AMP);
          int nr = csv.getCellAsInt(row, Cols.NR);
          double fMin = csv.getCellAsDouble(row, Cols.F_MIN);
          double fMax = csv.getCellAsDouble(row, Cols.F_MAX);
          double fKnee = csv.getCellAsDouble(row, Cols.F_KNEE);
          double duration = csv.getCellAsDouble(row, Cols.DURATION);
          string startTime = csv.getCell(row, Cols.START_TIME);
          string species = csv.getCell(row, Cols.SPECIES);
          double probability = csv.getCellAsDouble(row, Cols.PROBABILITY);
          double snr = csv.getCellAsDouble(row, Cols.SNR);
          string speciesMan = csv.getCell(row, Cols.SPECIES_MAN);
          double distToPrev = (callNr == 1) ? 0.0 : calcDistToPrev(startTime, file.Calls[callNr - 2].StartTime);
          AnalysisCall call = new AnalysisCall(nr, fMaxAmp, fMin, fMax, fKnee, duration, startTime, species,
                                               probability, speciesMan, snr, distToPrev);

          bool isInList = SpeciesInfos.isInList(_specList, call.SpeciesAuto);
          file.addCall(call, isInList);

          ReportItem rItem = new ReportItem();
          rItem.FileName = fName;
          rItem.CallNr = callNr.ToString();
          if (callNr < 2)
            rItem.Remarks = file.Remarks;
          callNr++;
          rItem.FreqMin = (fMin / 1000).ToString("0.#", CultureInfo.InvariantCulture);
          rItem.FreqMax = (fMax / 1000).ToString("0.#", CultureInfo.InvariantCulture);
          rItem.FreqMaxAmp = (fMaxAmp / 1000).ToString("0.#", CultureInfo.InvariantCulture);
          rItem.Duration = duration.ToString("0.#", CultureInfo.InvariantCulture);
          rItem.StartTime = startTime;
          rItem.SpeciesAuto = species;
          rItem.Probability = probability.ToString("0.###", CultureInfo.InvariantCulture);
          rItem.Snr = snr.ToString();
          rItem.SpeciesMan = speciesMan;
          _report.Add(rItem);
        }
        restChanged();
      }
    }


    public void save(string fileName)
    {
      lock (_fileLock)
      {
        Csv csv = new Csv();
        csv.read(fileName);
        foreach (AnalysisFile f in _list)
        {
          foreach (AnalysisCall c in f.Calls)
          {
            int row = csv.findInCol(f.FileName, Cols.NAME, c.Nr.ToString(), Cols.NR);
            csv.setCell(row, Cols.SPECIES_MAN, c.SpeciesMan);
            csv.setCell(row, Cols.SPECIES, c.SpeciesAuto);
            if (c.Nr < 2)
              csv.setCell(row, Cols.REMARKS, f.Remarks);
            else
              csv.setCell(row, Cols.REMARKS, "");
            c.resetChanged();
          }
        }
        csv.save();
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
        if (f.FileName.IndexOf(fileName) >= 0)
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
            report.read(reportName);
          }
          removeWavFromReport(report, wavName);
          if(doReadSave)
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


    public void removeWavFromReport(Csv report, string wavName)
    {
      int row = 0;
      do
      {
        row = report.findInCol(wavName, Cols.NAME, true);
        if (row > 0)
        {
          report.removeRow(row);
        }
      } while (row > 0);
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
      foreach (AnalysisFile f in _list)
      {
        foreach (AnalysisCall c in f.Calls)
        {
          c.resetChanged();
        }
      }
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

    bool _changed = false;
    string _speciesMan;
    string _speciesAuto;
    public int Nr { get; }
    public double FreqMaxAmp { get; }
    public double FreqMin { get; }
    public double FreqMax { get; }
    public double FreqKnee { get; }
    public double Duration { get; }
    public double DistToPrev { get; }
    public String StartTime { get; }
    public double Probability { get; }

    public double Snr { get; }

    public string SpeciesAuto { get { return _speciesAuto; } set { _speciesAuto = value;  _changed = true; } }
    public string SpeciesMan { get { return _speciesMan; } set { _speciesMan = value; _changed = true; } }

    public bool Changed { get { return _changed; } }

    public AnalysisCall(int nr, double freqMaxAmp, double freqMin, double freqMax, double freqKnee, double duration, string start, string speciesAuto, double probability, string speciesMan, double snr, double distToPrev)
    {
      Nr = nr;
      FreqMaxAmp = freqMaxAmp;
      FreqMin = freqMin;
      FreqMax = freqMax;
      FreqKnee = freqKnee;
      Duration = duration;
      DistToPrev = distToPrev;
      StartTime = start;
      _speciesAuto = speciesAuto;
      Probability = probability;
      _speciesMan = speciesMan;
      Snr = snr;
      _changed = false;
    }

    public void resetChanged()
    {
      _changed = false;
    }

    public bool checkConfidence(List<SpeciesInfos> species)
    {
      bool retVal = false;
      SpeciesInfos spec = SpeciesInfos.find(SpeciesAuto, species);
      string err = "";
      if(spec != null)
      {
        if (this.Duration < spec.DurationMin)
          err = "Dmin";
        else if (this.Duration > spec.DurationMax)
          err = "Dmax";
//        else if (this.FreqMaxAmp < (spec.FreqCharMin * 1000))
//          err = "FcMin";
//        else if (this.FreqMaxAmp > (spec.FreqCharMax * 1000))
//          err = "FcMax";
        else if (this.FreqMin < (spec.FreqMinMin * 1000))
          err = "FminMin";
        else if (this.FreqMin > (spec.FreqMinMax * 1000))
          err = "FminMax";
        else if (this.FreqMax < (spec.FreqMaxMin * 1000))
          err = "FminMin";
   //     else if (this.FreqMax > (spec.FreqMaxMax * 1000))
   //       err = "FmaxMax";
        if (err == "")
           retVal = true;
        else
          this._speciesAuto = "??C95[" + err +"," + this.SpeciesAuto + "]";
      }
      return retVal;
    }
  }


  public class AnalysisFile
  {
    List<AnalysisCall> _calls;

    public string FileName { get; set; }
    public int SampleRate { get; set; }
    public double Duration { get; set; }
    public string Remarks { get; set; }
    public bool Selected { get; set; } = false;
    public DateTime RecTime { get; set; }

    public bool Changed
    {
      get
      {
        bool retVal = false;
        foreach (AnalysisCall c in _calls)
        {
          retVal |= c.Changed;
        }
        return retVal;
      }
    }

    Dictionary<string, int> _specFound;

    public List<AnalysisCall> Calls { get { return _calls; } }

   /* public AnalysisFile()
    {
      _calls = new List<AnalysisCall>();
      _specFound = new Dictionary<string, int>();
    } */

    public AnalysisFile(string name)
    {
      FileName = name;
      _calls = new List<AnalysisCall>();
      _specFound = new Dictionary<string, int>();
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
          double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture,  out retVal);
          retVal /= 1000;
        }
        else
        {
          double.TryParse(_calls[idx].StartTime, NumberStyles.Any, CultureInfo.InvariantCulture, out retVal);
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
        if(f.FileName.Contains(fName))
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
        if (_specFound.ContainsKey(call.SpeciesAuto))
          _specFound[call.SpeciesAuto] += 1;
        else
          _specFound.Add(call.SpeciesAuto, 1);
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
