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
using System.Runtime.Serialization;
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
    public const string SPEC_LATIN = "Species Latin";
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
    public const string TEMPERATURE = "temperature";  //report.csv
    public const string HUMIDITY = "humidity";        //report.csv
    public const string TEMP_MIN = "temp_min";        //summary.csv
    public const string TEMP_MAX = "temp_max";        //summary.csv
    public const string HUMID_MIN = "humid_min";      //report.csv
    public const string HUMID_MAX = "humid_max";      //report.csv

    public const string COUNT = "Count";
    public const string DATE = "Date";
    public const string WEATHER = "Weather";
    public const string LANDSCAPE = "Landscape";

    public const string DAYS = "Days";
    public const string T00H = "00:00";

    public const string PERF_ANN_FILE = "annFile";  //performance result: annotation file
    public const string PERF_DETECTED = "detected";  //performance result: bat call detected
    public const string PERF_CORRECT = "correct";  //performance result: bat call correctly detected
  }

  [DataContract]
  public class SumItem
  {
    [DataMember]
    public string Species { get; set; }
    [DataMember]
    public int Count { get; set; }

    [DataMember]
    public double TempMin { get; set; }
    [DataMember]
    public double TempMax { get; set; }
    [DataMember]
    public double HumidityMin { get; set; }
    [DataMember]
    public double HumidityMax { get; set; }

    [DataMember]
    public int[] CountTime { get; set; }

    public SumItem(string species, int count)
    {
      Species = species;
      Count = count;
      CountTime = new int[24];
      TempMin = 100;
      TempMax = -100;
      HumidityMax = 0;
      HumidityMin = 100;
      for(int i = 0; i < 24; i++)
        CountTime[i] = 0;
    }

    public static SumItem find(string species, List<SumItem> list, bool addIfMissing = false)
    {
      foreach(SumItem item in list) 
      {
        if(item.Species == species) 
          return item;
      }
      if (addIfMissing)
      {
        SumItem s = new SumItem(species, 0);
        list.Add(s);
        return s;
      }
      else
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
    public double TempMin {  get { return _item.TempMin; } }
    public double TempMax { get { return _item.TempMax; } }
    public double HumidityMin { get { return _item.HumidityMin; } }
    public double HumidityMax { get { return _item.HumidityMax; } }
    public int _00h { get { return _item.CountTime[0]; } }
    public int _01h { get { return _item.CountTime[1]; } }
    public int _02h { get { return _item.CountTime[2]; } }
    public int _03h { get { return _item.CountTime[3]; } }
    public int _04h { get { return _item.CountTime[4]; } }
    public int _05h { get { return _item.CountTime[5]; } }
    public int _06h { get { return _item.CountTime[6]; } }
    public int _07h { get { return _item.CountTime[7]; } }
    public int _08h { get { return _item.CountTime[8]; } }
    public int _09h { get { return _item.CountTime[9]; } }
    public int _10h { get { return _item.CountTime[10]; } }
    public int _11h { get { return _item.CountTime[11]; } }
    public int _12h { get { return _item.CountTime[12]; } }
    public int _13h { get { return _item.CountTime[13]; } }
    public int _14h { get { return _item.CountTime[14]; } }
    public int _15h { get { return _item.CountTime[15]; } }
    public int _16h { get { return _item.CountTime[16]; } }
    public int _17h { get { return _item.CountTime[17]; } }
    public int _18h { get { return _item.CountTime[18]; } }
    public int _19h { get { return _item.CountTime[19]; } }
    public int _20h { get { return _item.CountTime[20]; } }
    public int _21h { get { return _item.CountTime[21]; } }
    public int _22h { get { return _item.CountTime[22]; } }
    public int _23h { get { return _item.CountTime[23]; } }
  }

  public class Analysis
  {

    private Cols _cols;
    private object _fileLock = new object();
    private List<AnalysisFile> _list;
    private Csv _csv;
    private List<SumItem> _summary;
    private enModel _modelType;
    private bool _updateCtls = false;

    public List<AnalysisFile> Files { get { return _list; } }
    public Cols Cols { get { return _cols; } }

    public int CsvRowCount { get { return _csv == null ? 0 : _csv.RowCnt; } }
    public bool IsEmpty { get { return _list.Count == 0; } }

    public enModel ModelType { get { return _modelType; } }

    public Csv Csv { get { return _csv; } }

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

    public Analysis(bool updateCtls, enModel modelType)
    {
      _updateCtls = updateCtls;
      init();
      _modelType = modelType;
    }

    public AnalysisFile find(string name)
    {
      AnalysisFile retVal = null;
      if (!string.IsNullOrEmpty(name))
      {
        foreach (AnalysisFile f in _list)
        {
          if (f.Name.ToLower().IndexOf(name.ToLower()) >= 0)
          {
            retVal = f;
            break;
          }
        }
      }
      return retVal;
    }


    public void concat(Analysis a)
    {
      foreach(AnalysisFile f in a.Files)
      {
        _list.Add(f);
      }
    }

    public void addCsvReportRow(List<string> row)
    {
      _csv.addRow(row);
    }

    public List<string> getCsvRow(int row)
    {
      return _csv.getRow(row);
    }

    public bool Changed
    {
      get
      {
        bool retVal = _csv.Changed;
        return retVal;
      }
    }

    public void init()
    {
      _list = new List<AnalysisFile>();
      _cols = new Cols();
      _csv = ModelBatDetect2.createReport(Cols.SPECIES);
      _summary = new List<SumItem>();
    }

    public void read(string fileName, ModelParams[] modelParams)
    {
      init();
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
          if (_csv.getColNr(Cols.TEMPERATURE) < 1)
          {
            int colnr = _csv.getColNr(Cols.LON) + 1;
            _csv.insertCol(colnr, "", Cols.TEMPERATURE);
            _csv.insertCol(colnr + 1, "", Cols.HUMIDITY);
            _csv.save();
          }
        }

        for(int i = 0; i < modelParams.Length; i++)
        {
          if(fileName.IndexOf(modelParams[i].Name) >= 0)
          {
            _modelType = modelParams[i].Type;
            break;
          }
        }

        _list.Clear();
        string lastFileName = "$$$";
        int callNr = 1;
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
         
          AnalysisCall call = new AnalysisCall(_csv, row, _updateCtls);
         /* if(callNr == 1)
          {
            oldStartTime = 0;
            call.DistToPrev = 0;
          }
          else
          {
            call.DistToPrev = (startTime - oldStartTime)*1000;
            oldStartTime = startTime;
          }*/
          bool isInList = SpeciesInfos.isInList(App.Model.SpeciesInfos, call.getString(Cols.SPECIES));
          file.addCall(call, isInList);

          callNr++;
        }

        resetChanged();
      }
    }

    public void filloutTemperature(Project prj)
    {
      foreach(AnalysisFile f in _list)
      {
        BatRecord info = PrjMetaData.retrieveMetaData(prj, f.Name);
        info.Temparature = info.Temparature.Replace("°C", "");
        info.Humidity = info.Humidity.Replace("%","");
        double temperature = -20;
        double humidity = -1;
        if (info != null)
        {
          double.TryParse(info.Temparature, NumberStyles.Any, CultureInfo.InvariantCulture, out temperature);
          double.TryParse(info.Humidity, NumberStyles.Any, CultureInfo.InvariantCulture, out humidity);
          foreach (AnalysisCall c in f.Calls)
          {
            c.setString(Cols.TEMPERATURE, temperature.ToString("0.#", CultureInfo.InvariantCulture));
            c.setString(Cols.HUMIDITY, humidity.ToString("0.#", CultureInfo.InvariantCulture));
          }
        }
      }
      
    }

    public static void undo(string undoFileName, Csv csv)
    {
      Csv undoFile = new Csv();
      undoFile.read(undoFileName, ";", true);
      string wavFile = undoFile.getCell(2, Cols.NAME);
      if (undoFile.RowCnt > 1)
      {
        int row = csv.findInCol(wavFile, Cols.NAME);
        while (row > 0)
        {
          csv.removeRow(row);
          row = csv.findInCol(wavFile, Cols.NAME);
        }

        row = 2;
        while (row <= undoFile.RowCnt)
        {
          csv.addRow(undoFile.getRow(row));
          row++;
        }
      }
    }


    public void undo(string undoFileName)
    {
      undo(undoFileName, _csv);
    }



    public void save(string path, string notes, string sumName)
    {
      lock (_fileLock)
      {
        if (_csv.FileName != "")
          _csv.save();
        else
          _csv.saveAs(path);
        resetChanged();
        createSummary(sumName, notes);
      }
    }

    public void updateControls(string wavName)
    {
      if (_updateCtls)
      {
        App.MainWin.callbackUpdateAnalysis(wavName);
      }
    }

    public void removeDeletedWavsFromReport(Project prj)
    {
      bool res = false;
      for (int r = 1; r <= _csv.RowCnt; r++)
      {
        string wavName = _csv.getCell(r, Cols.NAME);

        PrjRecord rec = prj.find(wavName);
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
        if (fName.IndexOf(fileName.ToLower()) >= 0)
        {
          retVal = f;
          break;
        }
      }
      return retVal;
    }


    public void addFile(AnalysisFile file, bool addCsv = false)
    {
      _list.Add(file);
      if(addCsv)
      {
        foreach (AnalysisCall c in file.Calls)
        {
          _csv.addRow(c.getReportRow());
        }
      }
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
        int startCol = sum.getColNr(Cols.T00H);
        if (startCol < 0)
        {
          string bakFile = fileName + ".old";
          File.Copy(fileName, bakFile);
          File.Delete(fileName);
          createSummary(fileName, notes);
          DebugLog.log("found summary in old format, created a new one", enLogType.INFO);
        }
        else
        {
          for (int row = 2; row < sum.RowCnt; row++)
          {
            string species = sum.getCell(row, Cols.SPECIES_MAN);
            int count = sum.getCellAsInt(row, "Count");
            SumItem it = new SumItem(species, count);
            int idx = 0;
            it.TempMin = sum.getCellAsDouble(row, Cols.TEMP_MIN, true);
            it.TempMax = sum.getCellAsDouble(row, Cols.TEMP_MAX, true);
            it.HumidityMin = sum.getCellAsDouble(row, Cols.HUMID_MIN, true);
            it.HumidityMax = sum.getCellAsDouble(row, Cols.HUMID_MAX, true);
            for (int c = startCol; c < startCol + it.CountTime.Length; c++)
            {
              it.CountTime[idx] = sum.getCellAsInt(row, c);
              idx++;
            }
            _summary.Add(it);
          }
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
        if (fName.ToLower().IndexOf(fileName.ToLower()) >= 0)
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
        f.updateFoundSpecies(App.Model.SpeciesInfos);
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
      if (string.IsNullOrEmpty(fileName))
        return;
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
              double t = c.getDouble(Cols.TEMPERATURE);
              if (it.TempMin > t)
                it.TempMin = t;
              if(it.TempMax < t)
                it.TempMax = t;
              double hu = c.getDouble(Cols.HUMIDITY);
              if (it.HumidityMin > hu)
                it.HumidityMin = hu;
              if (it.HumidityMax < hu)
                it.HumidityMax = hu;
            }
            else
            {
              it = new SumItem(c.getString(Cols.SPECIES_MAN), 1);
              it.TempMin = c.getDouble(Cols.TEMPERATURE);
              it.TempMax = it.TempMin;
              it.HumidityMin = c.getDouble(Cols.HUMIDITY);
              it.HumidityMax = it.HumidityMin;
              _summary.Add(it);
            }
            int h = f.RecTime.TimeOfDay.Hours;
         //   h = (h < 18) ? h + 6 : h - 18;
            if ((h >= 0) && (h < it.CountTime.Length))
              it.CountTime[h]++;
          }
        }

        // create csv file
        Csv sum = new Csv();
        sum.addRow();
        string[] header = { Cols.DATE, Cols.LAT, Cols.LON, Cols.WEATHER, Cols.LANDSCAPE, Cols.TEMP_MIN, Cols.TEMP_MAX, Cols.HUMID_MIN, Cols.HUMID_MAX, Cols.SPECIES_MAN, Cols.COUNT, Cols.T00H, "01:00", "02:00", "03:00", "04:00", "05:00", "06:00", "07:00", "08:00",
                            "09:00", "10:00", "11:00", "12:00", "13:00", "14:00", "15:00", "16:00", "17:00", "18:00", "19:00", "20:00", "21:00", "22:00", "23:00" };
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

        int startTimeCol = sum.findInRow(1, Cols.T00H);
        if (startTimeCol > 0)
        {
          foreach (SumItem it in _summary)
          {
            sum.setCell(row, Cols.SPECIES_MAN, it.Species);
            sum.setCell(row, "Count", it.Count);
            sum.setCell(row, Cols.TEMP_MIN, it.TempMin);
            sum.setCell(row, Cols.TEMP_MAX, it.TempMax);
            sum.setCell(row, Cols.HUMID_MIN, it.HumidityMin);
            sum.setCell(row, Cols.HUMID_MAX, it.HumidityMax);
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
          BatRecord rec = PrjMetaData.retrieveMetaData(fName);
          if (rec != null)
          {
            dateStr = rec.DateTime;
          }
          else
            DebugLog.log($"error reading meta data for: {fName}", enLogType.ERROR);
        }
        _csv.setCell(r, Cols.REC_TIME, dateStr);
      }
      _csv.save();
      sw.Stop();
      DebugLog.log("read out start time of recordings, exec time: " + sw.Elapsed.ToString(), enLogType.INFO);
    }

    public void resetChanged()
    {
      foreach (AnalysisFile f in _list)
        f.resetChanged();
    }
  }


  public class AnalysisCall
  {
    private Csv _csv;
    private int _row;
    private double _firstToSecond;
    private bool _updateCtls = false;

    public double FirstToSecond { get { return _firstToSecond; } }
    
    /// <summary>
    /// distance to previous call [ms]
    /// </summary>
 //   public double DistToPrev { get; set; }

    public bool Changed { get; set; } = false;

    public int ReportRow { get { return _row; } set { _row = value; } }

    public AnalysisCall(Csv csv, int row, bool updateCtls)
    {
      _csv = csv;
      _row = row;
      _updateCtls = updateCtls;
    }

    public void updateRow(int row)
    {
      _row = row;
    }

    public List<string> getReportRow()
    {
      return _csv.getRow( _row );
    }

    public double getDouble(string key, bool disableErrorMsg = false)
    {
      return _csv.getCellAsDouble(_row, key, disableErrorMsg);
    }

    public DateTime getDateTime(string key, bool disableErrorMsg = false)
    {
      return _csv.getCellAsDate(_row, key);
    }

    public void setDouble(string key , double val)
    {
      _csv.setCell(_row, key, val);
      if (_updateCtls)
      {
        App.MainWin.callbackUpdateAnalysis(_csv.getCell(_row, Cols.NAME));
      }
    }

    public int getInt(string key)
    {
      return _csv.getCellAsInt(_row, key);  
    }

    public void setInt(string key, int val)
    {
      _csv.setCell(_row, key, val);
      if (_updateCtls)
      {
        App.MainWin.callbackUpdateAnalysis(_csv.getCell(_row, Cols.NAME));
      }
    }

    public string getString(string key)
    {
      return _csv.getCell(_row, key);
    }

    public void setString(string key, string value)
    {
      Changed = true;
      _csv.setCell(_row, key, value);
      if (_updateCtls)
      {
         App.MainWin.callbackUpdateAnalysis(_csv.getCell(_row, Cols.NAME));
      }
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
    private string _backup = null;

    public string Name { get { return _name; } set { _name = value; } }
    public DateTime RecTime { get { return _recTime; } }

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



    /// <summary>
    /// save Analysis of a single file as separate CSV
    /// </summary>
    /// <param name="filename"></param>
    public void saveAs(string filename)
    {
      _backup = filename;
      Csv csv = new Csv(_csv.getRowAsString(1));
      foreach(AnalysisCall call in _calls)
      {
        csv.addRow(_csv.getRow(call.ReportRow));
      }
      csv.saveAs(filename);
    }

    public void saveCsv()
    {
      if(_calls.Count > 0)
        _csv.save();
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

     public void removeCall(int i)
    {
      if((i >= 0) && (i < _calls.Count))
      {
        _csv.removeRow(_calls[i].ReportRow);
        _calls.RemoveAt(i);
        for (int j = i; j < _calls.Count; j++)
          _calls[j].ReportRow--;
      }
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
      if (idx < 0)
        idx = 0;
      if (idx < _calls.Count)
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

    public bool undo()
    {
      bool update = false;

      if (_backup != null)
      {
        update = true;
        Analysis.undo(_backup, _csv);
      }
      return update;
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

    public void resetChanged()
    {
      foreach (AnalysisCall c in _calls)
        c.Changed = false;
    }

    public int findCallIdx(int callNr)
    {
      int retVal = -1;
      for(int i = 0; i < _calls.Count; i++)
      {
        if(_calls[i].getInt(Cols.NR) == callNr)
        {
          retVal = i;
          break;
        }
      }
      return retVal;
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
