/********************************************************************************
 *               Author: Christian Müller
 *     Date of creation: 2021-08-10                                       
 *   Copyright (C) 2023: Christian Müller chrmue44(at)gmail(dot).com
 *
 *              Licence:  CC BY-NC 4.0 
 ********************************************************************************/
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using BatInspector.Forms;
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
    public const string F_25 = "freq_25";
    public const string F_CENTER = "freq_center";
    public const string F_75 = "freq_75";
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
    public const string CURVE_NEG = "curve_neg";
    public const string CURVE_POS_START = "curve_pos_start";
    public const string CURVE_POS_END = "curve_pos_end";
    public const string MID_OFFSET = "mid_offset";
    public const string SMOTTHNESS = "smoothness";
    public const string REC_TIME = "recTime";
    public const string CALL_INTERVALL = "callInterval";
    public const string LAT = "lat";
    public const string LON = "lon";

    public const string COUNT = "Count";
    public const string DATE = "Date";
    public const string WEATHER = "Weather";
    public const string LANDSCAPE = "Landscape";

    public const string DAYS = "Days";
    public const string T18H = "18:00";
  }


    public class SumItem
  {
    public string Species { get; set; }
    public int Count { get; set; }

    public int[] CountTime { get; set; }

    public SumItem(string species, int count)
    {
      Species = species;
      Count = count;
      CountTime = new int[12];
      for(int i = 0; i < 12; i++)
        CountTime[i] = 0;
    }

    public static SumItem find(string species, List<SumItem> list)
    {
      foreach(SumItem item in list) 
      {
        if(item.Species == species) 
          return item;
      }
      return null;
    }
  }

  public class SumItemReport
  {
    SumItem _item;
    public SumItemReport(SumItem item) 
    {
      _item = item;
    }

    public string Species { get { return _item.Species; } }
    public int Count {  get { return _item.Count; } }
    public int _18h { get { return _item.CountTime[0]; } }
    public int _19h { get { return _item.CountTime[1]; } }
    public int _20h { get { return _item.CountTime[2]; } }
    public int _21h { get { return _item.CountTime[3]; } }
    public int _22h { get { return _item.CountTime[4]; } }
    public int _23h { get { return _item.CountTime[5]; } }
    public int _00h { get { return _item.CountTime[6]; } }
    public int _01h { get { return _item.CountTime[7]; } }
    public int _02h { get { return _item.CountTime[8]; } }
    public int _03h { get { return _item.CountTime[9]; } }
    public int _04h { get { return _item.CountTime[10]; } }
    public int _05h { get { return _item.CountTime[11]; } }
  }

  public delegate void DlgUpdateFile(string fName);

  public class Analysis
  {

    private Cols _cols;
    private object _fileLock = new object();
    private List<AnalysisFile> _list;
    private List<ReportItem> _report;
    private Csv _csv;
    private List<SumItem> _summary;
    private List<SpeciesInfos> _speciesInfos;
    private DlgUpdateFile _dlgUpdate;
 //   private string _notes;

    public List<AnalysisFile> Files { get { return _list; } }
    public Cols Cols { get { return _cols; } }

    public List<ReportItem> Report { get { return _report; } }

    public bool IsEmpty { get { return _list.Count == 0; } }

    public List<SumItemReport> Summary {
      get {
        if (_summary == null)
          return null;
        else
        {
          List<SumItemReport> l = new List<SumItemReport>();
          foreach (SumItem item in _summary)
          {
            SumItemReport it = new SumItemReport(item);
            l.Add(it);
          }
          return l;
        }
      } 
    }

    public Analysis(List<SpeciesInfos> specInfos, DlgUpdateFile dlgUpdate)
    {
      init(specInfos);
      _dlgUpdate = dlgUpdate;
    }

    public AnalysisFile find(string name)
    {
      AnalysisFile retVal = null;
      foreach (AnalysisFile f in _list)
      {
        if (f.Name.ToLower().IndexOf(name.ToLower()) >= 0)
        {
          retVal = f;
          break;
        }
      }
      return retVal;
    }

    public void addCsvReportRow(List<string> row)
    {
      _csv.addRow(row);
    }

    public void addReportItem(AnalysisFile file, AnalysisCall call)
    {
      if (_report != null)
      {
        ReportItem item = new ReportItem(file, call);
        _report.Add(item);
      }
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

    public void init(List<SpeciesInfos> specInfos)
    {
      _list = new List<AnalysisFile>();
      _report = null;
      _cols = new Cols();
      _csv = ModelBatDetect2.createReport(Cols.SPECIES);
      _speciesInfos = specInfos;
      _summary = new List<SumItem>();
    }

    public void read(string fileName)
    {
      init(_speciesInfos);
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
          if (_csv.getColNr(Cols.REC_TIME) < 1)
            filloutRecTime();
        }

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
            file = new AnalysisFile(_csv, fName, row, recTime);
            _list.Add(file);
            callNr = 1;
          }
         
          AnalysisCall call = new AnalysisCall(_csv, row, _dlgUpdate);
          if(callNr == 1)
          {
            oldStartTime = 0;
            call.DistToPrev = 0;
          }
          else
          {
            call.DistToPrev = (startTime - oldStartTime)*1000;
            oldStartTime = startTime;
          }
          bool isInList = SpeciesInfos.isInList(_speciesInfos, call.getString(Cols.SPECIES));
          file.addCall(call, isInList);

          ReportItem rItem = new ReportItem(file, call);
          _report.Add(rItem);
          callNr++;
        }
        resetChanged();
      }
    }


    public void save(string path, string notes)
    {
      lock (_fileLock)
      {
        if (_csv.FileName != "")
          _csv.save();
        else
          _csv.saveAs(path);
        if (_report != null)
        {
          foreach (ReportItem r in _report)
          {
            r.resetChanged();
          }
        }
        if (Report != null)
        {
          string sumName = _csv.FileName.Replace(AppParams.PRJ_REPORT, AppParams.PRJ_SUMMARY);
          createSummary(sumName, notes);
        }
      }
    }

    public void removeDeletedWavsFromReport(Project prj)
    {
      bool res = false;
      for (int r = 1; r <= _csv.RowCnt; r++)
      {
        string wavName = _csv.getCell(r, Cols.NAME);

        BatExplorerProjectFileRecordsRecord rec = prj.find(wavName);
        if (rec == null)
          res |= removeWavFromReport(wavName);
      }

      if (res)
        _csv.save();
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

    public void addFile(AnalysisFile file)
    {
      _list.Add(file);
    }

    public void addFile(string reportName, string wavName, int samplerate, double duration)
    {
      lock (_fileLock)
      {
        string wavFileName = Path.GetFileName(wavName);
        AnalysisFile f = new AnalysisFile(wavFileName, samplerate, duration);
        _list.Add(f);
        _csv.addRow();
        _csv.setCell(_csv.RowCnt, Cols.NAME, wavFileName);
        _csv.setCell(_csv.RowCnt, Cols.SAMPLERATE, samplerate);
        _csv.setCell(_csv.RowCnt, Cols.FILE_LEN, duration);
        _csv.setCell(_csv.RowCnt, Cols.NR, 1);
        if(_report != null)
        {
          ReportItem rep = new ReportItem();
          rep.FileName = wavFileName;
          rep.Duration = duration.ToString();
          _report.Add(rep);
        }
        
      }
    }


    public void removeFile(string reportName, string wavName)
    {
      lock (_fileLock)
      {
        AnalysisFile fileToDelete = AnalysisFile.find(_list, wavName);
        
        if (fileToDelete != null)
          _list.Remove(fileToDelete);

        // remove from report file
        if (File.Exists(reportName))
        {
          bool res = removeWavFromReport(wavName);
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


    public void calcProbabilityRatios(string speciesFile)
    {
      List<string> species = new List<string>();
      Csv spec = new Csv();
      spec.read(speciesFile, ";", true);
      for (int r = 2; r <= spec.RowCnt; r++)
        species.Add(spec.getCell(r, 1));

      foreach(AnalysisFile f in _list)
      {
        foreach(AnalysisCall c in f.Calls)
        {
          c.calcProbabilityRatio(species);
        }
      }
    }

    public void openSummary(string fileName, string notes)
    {
      if (!File.Exists(fileName))
        createSummary(fileName, notes);
      else
      {
        Csv sum = new Csv();
        sum.read(fileName, ";", true);
        _summary.Clear();
        for(int row = 2; row < sum.RowCnt; row++)
        {
          string species = sum.getCell(row, Cols.SPECIES_MAN);
          int count = sum.getCellAsInt(row, "Count");
          SumItem it = new SumItem(species, count);
          int startCol = sum.getColNr(Cols.T18H);
          int idx = 0;
          for(int c = startCol; c < startCol + 12; c++)
          {
            it.CountTime[idx] = sum.getCellAsInt(row, c);
            idx++;
          }
          _summary.Add(it);
        }
      }
    }


    public bool removeWavFromReport(string wavName)
    {
      int row = 0;
      bool retVal = false;
      do
      {
        row = _csv.findInCol(wavName, Cols.NAME, true);
        if (row > 0)
        {
          _csv.removeRow(row);
          retVal = true;
        }
      } while (row > 0);
      updateRowNumbers();
      return retVal;
    }


    public int getIndex(string fileName)
    {
      int retVal = -1;

      for (int i = 0;  i < Files.Count; i++)
      {
        string fName = Files[i].Name;
        if (fName.IndexOf(fileName) >= 0)
        {
          retVal = i;
          break;
        }
      }

      return retVal;
    }

    public void updateSpeciesCount()
    {
      foreach(AnalysisFile f in Files)
        f.updateFoundSpecies(_speciesInfos);
    }

    void updateRowNumbers()
    {
      foreach(AnalysisFile f in _list)
      {
        int row = _csv.findInCol(f.Name, Cols.NAME, true);
        if (row > 0)
          f.updateRow(row);
      }
    }


    public void createSummary(string fileName, string notes)
    {
      if (_list.Count > 0)
      {
        _summary.Clear();
        foreach (AnalysisFile f in _list)
        {
          foreach (AnalysisCall c in f.Calls)
          {
            SumItem it = SumItem.find(c.getString(Cols.SPECIES_MAN), _summary);
            if (it != null)
            {
              it.Count++;
            }
            else
            {
              it = new SumItem(c.getString(Cols.SPECIES_MAN), 1);
              _summary.Add(it);
            }
            int h = f.RecTime.TimeOfDay.Hours;
            h = (h < 18) ? h + 6 : h - 18;
            if ((h >= 0) && (h < it.CountTime.Length))
              it.CountTime[h]++;
          }
        }

        // create csv file
        Csv sum = new Csv();
        sum.addRow();
        string[] header = { Cols.DATE, Cols.LAT, Cols.LON, Cols.WEATHER, Cols.LANDSCAPE, Cols.SPECIES_MAN, Cols.COUNT, Cols.T18H, "19:00", "20:00", "21:00", "22:00", "23:00", "0:00", "1:00", "2:00", "3:00", "4:00", "5:00" };
        sum.initColNames(header, true);
        sum.addRow();
        int row = 2;
        sum.setCell(row, Cols.DATE, _list[0].RecTime.ToString(AppParams.REPORT_DATE_FORMAT));
        sum.setCell(row, Cols.LAT, _list[0].Calls[0].getDouble(Cols.LAT));
        sum.setCell(row, Cols.LON, _list[0].Calls[0].getDouble(Cols.LON));
        string[] note = notes.Split('\n');
        if (note.Length > 0)
          sum.setCell(row, Cols.WEATHER, note[0].Replace("\n",""));
        if (note.Length > 1)
          sum.setCell(row, Cols.LANDSCAPE, note[1].Replace("\n",""));

        int startTimeCol = sum.findInRow(1, Cols.T18H);
        if (startTimeCol > 0)
        {
          foreach (SumItem it in _summary)
          {
            sum.setCell(row, Cols.SPECIES_MAN, it.Species);
            sum.setCell(row, "Count", it.Count);
            sum.addRow();
            for (int c = 0; c < it.CountTime.Length; c++)
              sum.setCell(row, c + startTimeCol, it.CountTime[c]);
            row++;
          }
        }
        else
          DebugLog.log("Summary report: col '18:00' not found", enLogType.ERROR);
        sum.saveAs(fileName);
      }
    }

    void filloutRecTime()
    {
      _csv.insertCol(2, "", Cols.REC_TIME);
      string oldF = "$$$";
      string dateStr = "";

      Stopwatch sw = new Stopwatch();
      sw.Start();
      for (int r = 2; r <= _csv.RowCnt; r++)
      {
        string fName = _csv.getCell(r, Cols.NAME);
        if (fName != oldF)
        {
          string xmlName = fName.ToLower().Replace(AppParams.EXT_WAV, AppParams.EXT_INFO);
          BatRecord rec = ElekonInfoFile.read(xmlName);
          if (rec != null)
          {
            dateStr = rec.DateTime;
          }
          else
            DebugLog.log("error reading file: " + xmlName, enLogType.ERROR);
        }
        _csv.setCell(r, Cols.REC_TIME, dateStr);
      }
      _csv.save();
      sw.Stop();
      DebugLog.log("read out start time of recordings, exec time: " + sw.Elapsed.ToString(), enLogType.INFO);
    }

    void resetChanged()
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

    public ReportItem()
    {

    }

    public ReportItem(AnalysisFile file, AnalysisCall call)
    {
      FileName = file.Name;
      int callNr = call.getInt(Cols.NR);
      CallNr = callNr.ToString();
      if (callNr < 2)
        Remarks = file.getString(Cols.REMARKS);
      callNr++;
      FreqMin = (call.getDouble(Cols.F_MIN) / 1000).ToString("0.#", CultureInfo.InvariantCulture);
      FreqMax = (call.getDouble(Cols.F_MAX) / 1000).ToString("0.#", CultureInfo.InvariantCulture);
      FreqMaxAmp = (call.getDouble(Cols.F_MAX_AMP) / 1000).ToString("0.#", CultureInfo.InvariantCulture);
      Duration = call.getDouble(Cols.DURATION).ToString("0.#", CultureInfo.InvariantCulture);
      StartTime = call.getString(Cols.START_TIME);
      SpeciesAuto = call.getString(Cols.SPECIES);
      Probability = call.getDouble(Cols.PROBABILITY).ToString("0.###", CultureInfo.InvariantCulture);
      Latitude = call.getDouble(Cols.LAT).ToString("0.#######", CultureInfo.InvariantCulture);
      Longitude = call.getDouble(Cols.LON).ToString("0.#######", CultureInfo.InvariantCulture);
      Snr = call.getDouble(Cols.SNR).ToString();
      SpeciesMan = call.getString(Cols.SPECIES_MAN);
    }

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

    public string Latitude {get;set;}
    public string Longitude { get;set;}
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

    public static ReportItem find(string filename, List<ReportItem> list)
    {
      foreach(ReportItem r in list)
      {
        if(r.FileName == filename)
          return r;
      }
      return null;
    }
  }

  public class AnalysisCall
  {
    private Csv _csv;
    private int _row;
    private double _firstToSecond;
    private DlgUpdateFile _dlgUpdate;

   public double FirstToSecond { get { return _firstToSecond; } }
    
   /// <summary>
   /// distance to previous call [ms]
   /// </summary>
   public double DistToPrev { get; set; }

    public AnalysisCall(Csv csv, int row, DlgUpdateFile delegateUpdate)
    {
      _csv = csv;
      _row = row;
      _dlgUpdate = delegateUpdate;
    }

    public void updateRow(int row)
    {
      _row = row;
    }

    public List<string> getReportRow()
    {
      return _csv.getRow( _row );
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
      if (_dlgUpdate != null)
        _dlgUpdate(_csv.getCell(_row, Cols.NAME));
    }


    public bool checkConfidence(List<SpeciesInfos> species)
    {
      bool retVal = false;
      string speciesAuto = getString(Cols.SPECIES);
      double duration = getDouble(Cols.DURATION);
      double fMin = getDouble(Cols.F_MIN);
      double fMax = getDouble(Cols.F_MAX);
      SpeciesInfos spec = SpeciesInfos.findAbbreviation(speciesAuto, species);
      string err = "";
      if(spec != null)
      {
        if (duration < spec.DurationMin)
          err = "Dmin";
        else if (duration > spec.DurationMax)
          err = "Dmax";
        else if (fMin < (spec.FreqMinMin * 1000))
          err = "FminMin";
        else if (fMin > (spec.FreqMinMax * 1000))
          err = "FminMax";
        else if (fMax < (spec.FreqMaxMin * 1000))
          err = "FminMin";
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

    /// <summary>
    /// compare probability of detected species to the 2nd rank
    /// </summary>
    /// <param name="specList"></param>
    public void calcProbabilityRatio(List<string> specList)
    {
      double prob = getDouble(Cols.PROBABILITY);
      double prob2nd = 0.0;
      foreach(string spec in specList)  
      {
        char[] arr = new char[4];
        arr[0] = (char)(spec[0] & 0xDF);
        for(int i=1; i < 4; i++)
          arr[i] = (char)(spec[i] | 0x20);
        string specName = new string(arr);

        if (getString(Cols.SPECIES).ToUpper().IndexOf(specName.ToUpper()) < 0)
        {
          double pSpec = getDouble(specName);
          if (pSpec > prob2nd)
            prob2nd = pSpec;
        }
      }
      _firstToSecond = prob / prob2nd;
    }

    /// <summary>
    /// retrun the 5 characteristic frequency points
    /// </summary>
    /// <returns></returns>
    public double[] getFreqPoints()
    {
      double[] f = new double[5];
      f[0] = getDouble(Cols.F_START);
      f[1] = getDouble(Cols.F_25);
      f[2] = getDouble(Cols.F_CENTER);
      f[3] = getDouble(Cols.F_75);
      f[4] = getDouble(Cols.F_END);
      return f;
    }
  }


  public class AnalysisFile
  {
    private List<AnalysisCall> _calls;

    private int _startRow;

    private Csv _csv;
    private DateTime _recTime;
    private string _name;
    public bool Selected { get; set; } = false;

    public string Name { get { return _name; } set { _name = value; } }
    public DateTime RecTime { get { return _recTime; } set { _recTime = value; } }

    Dictionary<string, int> _specFound;

    public List<AnalysisCall> Calls { get { return _calls; } }


    // fake analysis for projects without report
    public AnalysisFile(string name, int sampleRate, double duration)
    {
      _calls = new List<AnalysisCall>();
      _specFound = new Dictionary<string, int>();
      string header = Cols.NAME + ";" + Cols.SAMPLERATE + ";" + Cols.DURATION + ";" + Cols.START_TIME;
      _csv = new Csv(header);
      _csv.addRow();
      _startRow = 2;
      _name = name;
      _csv.setCell(_startRow, Cols.DURATION, duration);
      _csv.setCell(_startRow, Cols.SAMPLERATE,sampleRate);
      _csv.setCell(_startRow, Cols.START_TIME, 0);
    }


    public AnalysisFile(Csv csv, string name, int startRow, DateTime recTime)
    {
      _name = name;
      _calls = new List<AnalysisCall>();
      _specFound = new Dictionary<string, int>();
      _startRow = startRow;
      _csv = csv;
      _recTime = recTime;
    }

    public void updateRow(int row)
    {
      _startRow = row;
      for(int i = 0; i < _calls.Count; i++)
        _calls[i].updateRow(row + i);
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
      if ((idx >= 0) && (idx < _calls.Count))
      {
        string startTime = _calls[idx].getString(Cols.START_TIME);
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

    public void updateFoundSpecies(List <SpeciesInfos> specList)
    {
      _specFound.Clear();
      foreach (AnalysisCall c in Calls)
      {
        string spec = c.getString(Cols.SPECIES);
        if (SpeciesInfos.isInList(specList, spec))
        {
          if (_specFound.ContainsKey(spec))
            _specFound[spec]++;
          else
            _specFound.Add(spec, 1);
        }
      }
    }


    static public AnalysisFile find(List<AnalysisFile> list, string fName)
    {
      AnalysisFile retVal = null;
      foreach(AnalysisFile f in list)
      {
        string fileName = f.getString(Cols.NAME);
        if(fileName.ToLower().Contains(fName.ToLower()))
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
