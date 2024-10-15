/********************************************************************************
 *               Author: Christian Müller
 *     Date of creation: 2021-08-10                                       
 *   Copyright (C) 2023: Christian Müller chrmue44(at)gmail(dot).com
 *
 *              Licence:  CC BY-NC 4.0 
 ********************************************************************************/
using libParser;
using libScripter;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Threading;
using System.Windows;


namespace BatInspector
{

  public delegate void dlgShowActivityDiag(ActivityData data, string bmpName);
  public enum enPeriod
  {
    DAILY = 0,
    WEEKLY = 1,
    MONTHLY = 2
  }

  public class ReportListItem
  {
    public string ReportName { get; set; }
    public string SummaryName { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
  }

  [DataContract]
  public class SumReportItem
  {
    [DataMember]
    public DateTime Date { get; set; }
    [DataMember]
    public int Days { get; set; }
    [DataMember]
    public double Latitude { get; set; }
    [DataMember]
    public double Longitude { get; set; }
    [DataMember]
    public string Landscape { get; set; }
    [DataMember]
    public string Weather { get; set; }
    [DataMember]
    public double TempMin { get; set; }
    [DataMember]
    public double TempMax { get; set; }
    [DataMember]
    public double HumidityMin { get; set; }
    [DataMember]
    public double HumidityMax { get; set; }
    [DataMember]
    public List<SumItem> SpecList { get; set; }

    public SumReportItem(List<SpeciesInfos> species)
    {
      SpecList = new List<SumItem>();
      TempMin = 100;
      TempMax = -100;
      HumidityMin = 100;
      HumidityMax = 0;
      foreach (SpeciesInfos spec in species)
      {
        SumItem it = new SumItem(spec.Abbreviation, 0);
        SpecList.Add(it);
      }
    }
  }


  [DataContract]
  public class SumReportJson
  {
    [DataMember]
    public List<string> Species { get; set; }

    [DataMember]
    public List<SumReportItem> Days { get; set; }

    public SumReportJson()
    {
      Species = new List<string>();
      Days = new List<SumReportItem>();
    }

    public string getPerCentStr(string spec, int decimals)
    {
      double sumTotal = 0.0;
      double sumSpec = 0.0;
      foreach (SumReportItem it in Days)
      {
        foreach (SumItem s in it.SpecList)
        {
          if ((s.Species != "?") && (s.Species.ToLower() != "social"))
          {
            sumTotal += s.Count;
            if (s.Species.ToLower() == spec.ToLower())
              sumSpec += s.Count;
          }
        }
      }
      double perCent = sumSpec / sumTotal * 100;
      string formatStr = "0.";
      for (int i = 0; i < decimals; i++)
        formatStr += "#";
      string str = perCent.ToString(formatStr, CultureInfo.InvariantCulture);
      return str;
    }

    public string getSumStr(string spec)
    {
      int sumSpec = 0;
      foreach (SumReportItem it in Days)
      {
        foreach (SumItem s in it.SpecList)
        {
          if (s.Species.ToLower() == spec.ToLower())
            sumSpec += s.Count;
        }
      }
      return sumSpec.ToString();
    }

    public void save(string name)
    {
      try
      {
        StreamWriter file = new StreamWriter(name);
        MemoryStream stream = new MemoryStream();
        DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(SumReportJson));
        ser.WriteObject(stream, this);
        StreamReader sr = new StreamReader(stream);
        stream.Seek(0, SeekOrigin.Begin);
        string str = sr.ReadToEnd();
        file.Write(JsonHelper.FormatJson(str));
        file.Close();
        DebugLog.log("summary report (json) saved to '" + name + "'", enLogType.INFO);
      }
      catch (Exception e)
      {
        DebugLog.log("failed to write summary report file:" + name + ": " + e.ToString(), enLogType.ERROR);
      }

    }
  }

  [DataContract]
  public class SpeciesWebInfo
  {
    [DataMember]
    public string Name { get; set; }
    [DataMember]
    public string Comment { get; set; }
    [DataMember]
    public string Confusion { get; set; }

    [DataMember]
    public bool Show { get; set; }
    public SpeciesWebInfo(string name, string comment, string confusion)
    {
      Name = name;
      Comment = comment;
      Confusion = confusion;
      Show = false;
    }
  }

  [DataContract]
  public class WebReportDataJson
  {
    [DataMember]
    public string PageName { get; set; }
    [DataMember]
    public string LocationName { get; set; }
    [DataMember]
    public string Author { get; set; }

    [DataMember]
    public string Weather { get; set; }

    [DataMember]
    public string TimeSpan { get; set; }

    [DataMember]
    public string LocationDescription { get; set; }

    [DataMember]
    public string Template { get; set; }

    [DataMember]
    public string WavFolder { get; set; }

    [DataMember]
    public string Comment { get; set; }

    [DataMember]
    public string ImgLandscape { get; set; }

    [DataMember]
    public string ImgPortrait { get; set; }

    [DataMember]
    List<SpeciesWebInfo> Species { get; set; }

    public WebReportDataJson()
    {
      init();
    }

    private void init()
    {
      PageName = "name of web page";
      LocationName = "location name of recording";
      Author = "insert name";
      Weather = "describe weather conditions";
      TimeSpan = "Timespan of recordings";
      LocationDescription = "brief description of recording location";
      Template = "name of used template file";
      Comment = "insert comment about the recording";
      WavFolder = "";
      Species = new List<SpeciesWebInfo>();
      Species.Add(new SpeciesWebInfo("BBAR", "", "Typ A: charakteristisch\r\nTyp B: MDAU (sozial)"));
      Species.Add(new SpeciesWebInfo("ENIL", "", ""));
      Species.Add(new SpeciesWebInfo("ESER", "", "qcf: VMUR, NLEI\r\nfm-qcf: VMUR, NLEI, NNOC, ENIL\r\nfm: VMUR, NLEI, NNOC, ENIL"));
      Species.Add(new SpeciesWebInfo("MALC", "", ""));
      Species.Add(new SpeciesWebInfo("MBEC", "", ""));
      Species.Add(new SpeciesWebInfo("MBRA", "", ""));
      Species.Add(new SpeciesWebInfo("MDAS", "", ""));
      Species.Add(new SpeciesWebInfo("MDAU", "", "kurze Rufe ( < 3,5 ms) : MDAS, MMYS, MMYO\r\nmittellange Rufe (3,5 - 6 ms): MDAS, MBART, MBEC, MMYO"));
      Species.Add(new SpeciesWebInfo("MEMA", "", "kurz(1.8 - 3.5 ms): MBART, MBEC\r\nmittellang (3.5 - 6 ms): MALC, MBART, MBEC "));
      Species.Add(new SpeciesWebInfo("MDAS", "", ""));
      Species.Add(new SpeciesWebInfo("MMYO", "", "kurze Rufe (2,5 - 3,5 ms): MDAU\r\nmittellange Rufe ^(3,5 - 6): MDAU, MDAS, MNAT"));
      Species.Add(new SpeciesWebInfo("MMYS", "", "MDAU, MDAS, MALC, MBEC, MEMA"));
      Species.Add(new SpeciesWebInfo("MNAT", "", "kurze Rufe (<3,5ms): keine\r\nmittellang: (3,5 -  6ms); MMYO\r\nlang: MBEC, MMYO"));
      Species.Add(new SpeciesWebInfo("MOXY", "", ""));
      Species.Add(new SpeciesWebInfo("MSCH", "", ""));
      Species.Add(new SpeciesWebInfo("NLAS", "", ""));
      Species.Add(new SpeciesWebInfo("NLEI", "", "qcf: VMUR, NNOC\r\nfm-qcf: VMUR, NNOC, ESER (nicht bestimmbar)\r\nfm:  VMUR, NNOC, ESER, ENIL (nicht bestimmbar)"));
      Species.Add(new SpeciesWebInfo("NNOC", "", "qcf: VMUR, NLEI\r\nfm-qcf: VMUR, NLEI, ESER (teils bestimmbar)\r\nfm:  VMUR, NLEI, ESER, ENIL (nicht bestimmbar)"));
      Species.Add(new SpeciesWebInfo("PKUH", "", "PKUH (Ortungsrufe identisch), PPIP, HSAV"));
      Species.Add(new SpeciesWebInfo("PNAT", "", ""));
      Species.Add(new SpeciesWebInfo("PPIP", "", "PNAT, PKUH, PPYG"));
      Species.Add(new SpeciesWebInfo("PKUH", "", ""));
      Species.Add(new SpeciesWebInfo("PPYG", "", "PPIP"));
      Species.Add(new SpeciesWebInfo("PAUR", "", "BBAR, Nahordnung Nyctaloid"));
      Species.Add(new SpeciesWebInfo("PAUS", "", "BBAR, Nahordnung Nyctaloid"));
      Species.Add(new SpeciesWebInfo("PPYG", "", "PPIP"));
      Species.Add(new SpeciesWebInfo("RFER", "", ""));
      Species.Add(new SpeciesWebInfo("RHIP", "", ""));
      Species.Add(new SpeciesWebInfo("VMUR", "", ""));
      Species.Add(new SpeciesWebInfo("Nyctalus", "", ""));
      Species.Add(new SpeciesWebInfo("Myotis", "", ""));
      Species.Add(new SpeciesWebInfo("Pipistrellus", "", ""));
      Species.Add(new SpeciesWebInfo("Plecotus", "", ""));
    }

    public SpeciesWebInfo findSpecies(string spec)
    {
      SpeciesWebInfo retVal = null;
      foreach (SpeciesWebInfo s in Species)
      {
        if (spec == s.Name)
        {
          retVal = s;
          break;
        }
      }
      return retVal;
    }
    public void save(string name)
    {
      try
      {
        StreamWriter file = new StreamWriter(name);
        MemoryStream stream = new MemoryStream();
        DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(WebReportDataJson));
        ser.WriteObject(stream, this);
        StreamReader sr = new StreamReader(stream);
        stream.Seek(0, SeekOrigin.Begin);
        string str = sr.ReadToEnd();
        file.Write(JsonHelper.FormatJson(str));
        file.Close();
        DebugLog.log("Web report entries saved to '" + name + "'", enLogType.INFO);
      }
      catch (Exception e)
      {
        DebugLog.log("failed to write web report data file:" + name + ": " + e.ToString(), enLogType.ERROR);
      }
    }
    public static WebReportDataJson load(string name)
    {
      WebReportDataJson retVal = null;
      FileStream file = null;
      try
      {
        DebugLog.log("try to load:" + name, enLogType.DEBUG);
        if (File.Exists(name))
        {
          file = new FileStream(name, FileMode.Open, FileAccess.Read);
          DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(WebReportDataJson));
          retVal = (WebReportDataJson)ser.ReadObject(file);
          if (retVal == null)
            DebugLog.log("web report entry file not well formed!", enLogType.ERROR);
        }
        else
        {
          DebugLog.log("load failed", enLogType.DEBUG);
          retVal = new WebReportDataJson();
          retVal.init();
          retVal.save(name);
        }
      }
      catch (Exception e)
      {
        DebugLog.log("failed to read config file : " + name + ": " + e.ToString(), enLogType.ERROR);
        retVal = null;
      }
      finally
      {
        if (file != null)
          file.Close();
      }
      return retVal;
    }
  }

  public class DailyActivity
  {
    public DailyActivity(DateTime date, int ticksPerHour) 
    {
      int cnt = 24 * ticksPerHour;
      Counter = new List<int>(cnt);
      for (int i = 0; i < cnt; i++)
        Counter.Add(0);
      Date = date;
      TotalCalls = 0;
      Latitude = 0;
      Longitude = 0;
      MaxCount = 0;
    }

    public List<int> Counter { get; }
    public DateTime Date { get; }

    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public int TotalCalls { get; set; }
    public int MaxCount { get; set; }
  }

  public class ActivityData
  {
    public ActivityData(int ticksPerHour)
    {
      Days = new List<DailyActivity>();
      TicksPerHour = ticksPerHour;
      TotalCalls = 0;
      DaysWithData = 0;
    }
    public List<DailyActivity> Days { get; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public int TicksPerHour { get; }
    public DateTime StartDate{ get; set; }  
    public DateTime EndDate { get; set; }
    public int DaysWithData { get; set; }
    public int TotalCalls { get; set; }
    /// <summary>
    /// maximum count value for class
    /// </summary>
    public int MaxCount { get; set; }
    public void Add(DailyActivity activity)
    {
      Days.Add(activity);
    }

    public int find(DateTime day)
    {
      int retVal = -1;
      for(int i = 0; i <  Days.Count; i++)
      {
        if(day.Date == Days[i].Date.Date)
        {
          retVal = i;
          break;
        }
      }
      return retVal;
    }

    public float calcMeanValue()
    {
      float sum = 0;
      int countEvents = 0;
      float meanValue = 0;
      for (int i = 0; i < Days.Count; i++)
      {
        for (int j = 0; j < Days[i].Counter.Count; j++)
        {
          if (Days[i].Counter[j] > 0)
          {
            sum += Days[i].Counter[j];
            countEvents++;
          }
        }
      }
      if (countEvents == 0)
        countEvents++;
      meanValue = sum / (float)countEvents;
      return meanValue;
    }
  }

  /// <summary>
  /// a class to create a sumarized report over multiple projects for a specified time period
  /// </summary>
  public class SumReport
  {
    private Csv _rep;
    private string _rootDir;
    private DirectoryInfo _dirInfo;
    private List<ReportListItem> _reports;
    private List<SumItem> _totalSum;
    private ViewModel _model;

    private DateTime _start;
    private DateTime _end;
    private enPeriod _period;
    private string _dstDir;
    private string _expression;
    private dlgShowActivityDiag _showActivityData = null;
    private string _bmpName;
    private ModelParams _modelParams;
    public SumReport(ViewModel model)
    {
      _model = model;
      _rep = new Csv(true);
      _reports = new List<ReportListItem>();
      _totalSum = new List<SumItem>();
    }

    /// <summary>
    /// main function to create a summarized report in csv format
    /// </summary>
    /// <param name="start">start time</param>
    /// <param name="end">end time</param>
    /// <param name="period">granularity of time</param>
    /// <param name="rootDir">root dir to start search for projects</param>
    public void createCsvReport(DateTime start, DateTime end, enPeriod period, string rootDir, string dstDir, string reportName,
                             List<SpeciesInfos> species, string expression)
    {
      initDirTree(rootDir, "summary");
      _rep = new Csv(true);
      string[] header = { Cols.DATE, Cols.DAYS, Cols.LAT, Cols.LON, Cols.LANDSCAPE, Cols.WEATHER, Cols.TEMP_MIN, Cols.TEMP_MAX, Cols.HUMID_MIN, Cols.HUMID_MAX };
      _rep.initColNames(header, true);
      _totalSum.Clear();
      foreach (SpeciesInfos sp in species)
        _totalSum.Add(new SumItem(sp.Abbreviation, 0));

      SumReportItem item;
      DateTime date = start;
      while (date < end)
      {
        int days = calcDays(date, end, period);
        item = getSums(date, days, species, expression);

        int nrCalls = 0;
        foreach (SumItem it in item.SpecList)
        {
          SumItem t = SumItem.find(it.Species, _totalSum, true);
          t.Count += it.Count;
          nrCalls += it.Count;
        }
        if (nrCalls > 0)
          addEntryToCsvReport(item);

        date = incrementDate(date, period);
      }

      _rep.addRow();
      _rep.addRow();
      _rep.setCell(_rep.RowCnt, Cols.DATE, "Sum Total");
      foreach (SumItem it in _totalSum)
      {
        if (it.Count <= 0)
          _rep.removeCol(it.Species);
        else
          _rep.setCell(_rep.RowCnt, it.Species, it.Count);
      }

      if (Directory.Exists(dstDir))
      {
        string fileName = Path.Combine(dstDir, reportName);
        _rep.saveAs(fileName);
      }
      else

        DebugLog.log("unable to save sum report, directory does not exist: " + rootDir, enLogType.ERROR);
    }

    void createActivityDiagSync()
    {
      DebugLog.log("start creating activity diagram...", enLogType.INFO);
      initDirTree(_rootDir, "report");

      DateTime date = _start;
      int minsPerPoint = 5;
      int ticksPerHour = 60 / minsPerPoint;
      bool addLine = false;
      ActivityData retVal = new ActivityData(ticksPerHour);
      retVal.StartDate = _start.AddDays(-1); ;
      retVal.EndDate = _end;
      double latitude = 0;
      double longitude = 0;
      while (date < _end)
      {
        DailyActivity dailyActivity = new DailyActivity(date,ticksPerHour);
        int days = calcDays(date, _end, _period);
        bool cont = gatherDailyActivity(date, days, _expression, dailyActivity, out bool addedLine);
        if(dailyActivity.TotalCalls > 0)
        {
          retVal.DaysWithData++;
          latitude += dailyActivity.Latitude;
          longitude += dailyActivity.Longitude;
          retVal.TotalCalls += dailyActivity.TotalCalls;
          if (dailyActivity.MaxCount > retVal.MaxCount)
            retVal.MaxCount = dailyActivity.MaxCount;
        }
        // after first line was found continue to add lines, even without having data
        if (addedLine)
          addLine = true;
        date = incrementDate(date, _period);
        if (addLine)
          retVal.Add(dailyActivity);
      }
      if (retVal.DaysWithData > 0)
      {
        retVal.Latitude = latitude / retVal.DaysWithData;
        retVal.Longitude = longitude / retVal.DaysWithData;
      }
      if(_showActivityData != null)
        _showActivityData(retVal, Path.Combine(_dstDir,_bmpName));
    }

    public void createActivityDiagAsync(DateTime start, DateTime end, enPeriod period, string rootDir, string dstDir, ModelParams modelPars, string expression, string bmpName, dlgShowActivityDiag dlgShowHeatMap)
    {
      _start = start;
      _end = end;
      _period = period;
      _rootDir = rootDir;
      _dstDir = dstDir;
      _expression = expression;
      _showActivityData = dlgShowHeatMap;
      _bmpName = bmpName;
      _modelParams = modelPars;

      Thread t = new Thread(createActivityDiagSync);
      t.Start();

    }


    public SumReportJson createWebReport(DateTime start, DateTime end, enPeriod period, string rootDir, string dstDir, string reportName,
                         List<SpeciesInfos> species, string expression)
    {
      SumReportJson retVal = new SumReportJson();
      initDirTree(rootDir, "report");

      DateTime date = start;
      while (date < end)
      {
        int days = calcDays(date, end, period);
        SumReportItem item = getSums(date, days, species, expression);

        int nrCalls = 0;
        foreach (SumItem it in item.SpecList)
        {
          nrCalls += it.Count;
        }
        if (nrCalls > 0)
          retVal.Days.Add(item);

        date = incrementDate(date, period);
      }
      retVal.Species = getSpeciesInReport(retVal.Days);
      return retVal;
    }

    public int calcDays(DateTime date, DateTime end, enPeriod period)
    {
      int days = 0;
      bool exit = false;
      int oldMonth = date.Month;
      while ((date < end) && !exit)
      {
        days++;
        date = date.AddDays(1);
        switch (period)
        {
          case enPeriod.DAILY:
            if (days >= 1)
              exit = true;
            break;
          case enPeriod.WEEKLY:
            if (days >= 7)
              exit = true;
            break;

          case enPeriod.MONTHLY:
            if (oldMonth != date.Month)
              exit = true;
            break;
        }
      }
      return days;
    }

    public DateTime incrementDate(DateTime date, enPeriod period)
    {
      switch (period)
      {
        case enPeriod.DAILY:
          date = date.AddDays(1);
          break;
        case enPeriod.WEEKLY:
          date = date.AddDays(7);
          break;

        case enPeriod.MONTHLY:
          {
            int oldMonth = date.Month;
            while (oldMonth == date.Month)
              date = date.AddDays(1);
          }
          break;
      }
      return date;
    }


    void crawlDirTree(DirectoryInfo dir, string reportName)
    {
      foreach (DirectoryInfo subDir in dir.GetDirectories())
      {
        crawlDirTree(subDir, reportName);
        string prjFile = Project.containsReport(subDir, reportName, _modelParams);
        if (prjFile != "")
        {
          Csv csv = new Csv();
          csv.read(prjFile, AppParams.CSV_SEPARATOR, true);
          findStartEnd(csv, out string startDateStr, out string endDateStr);
          DateTime startDate = new DateTime();
          DateTime endDate = new DateTime();
          bool ok_s = false;
          bool ok_e = false;
          try
          {
            ok_s = DateTime.TryParseExact(startDateStr, AppParams.REPORT_DATETIME_FORMAT, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out startDate);
            if(!ok_s)
              ok_s= DateTime.TryParseExact(startDateStr, AppParams.REPORT_DATETIME_FORMAT2, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out startDate);
            if (!ok_s)
              ok_s = DateTime.TryParseExact(startDateStr, AppParams.REPORT_DATETIME_FORMAT3, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out startDate);

            ok_e = DateTime.TryParseExact(endDateStr, AppParams.REPORT_DATETIME_FORMAT, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out endDate);
            if(!ok_e)
              ok_e = DateTime.TryParseExact(endDateStr, AppParams.REPORT_DATETIME_FORMAT2, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out endDate);
            if (!ok_e)
              ok_e = DateTime.TryParseExact(endDateStr, AppParams.REPORT_DATETIME_FORMAT3, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out endDate);
          }
          catch
          {
            ok_s = false;
          }
          if(!ok_s || !ok_e)
            DebugLog.log($"unrecognized date format in {prjFile}:  {startDateStr} and/or {endDateStr}", enLogType.ERROR);

          ReportListItem rec = new ReportListItem
          {
            ReportName = prjFile,
            SummaryName = Path.Combine(Path.GetDirectoryName(prjFile), AppParams.PRJ_SUMMARY),
            StartDate = startDate,
            EndDate = endDate
          };
          _reports.Add(rec);
        }
      }
    }

    private void findStartEnd(Csv csv, out string start, out string end)
    {
      DateTime dStart =  DateTime.MaxValue;
      DateTime dEnd = DateTime.MinValue;
      for(int row = 2; row <= csv.RowCnt; row++)
      {
        DateTime d = csv.getCellAsDate(row, Cols.REC_TIME);
        if (d < dStart)
          dStart = d;
        if (d > dEnd)
          dEnd = d;
      }
      start = dStart.ToString(AppParams.REPORT_DATETIME_FORMAT2);
      end = dEnd.ToString(AppParams.REPORT_DATETIME_FORMAT2);
    }

    private bool gatherDailyActivity(DateTime start, int days, string expression, DailyActivity dailyActivity, out bool addedLine)
    {
      bool retVal = true;
      addedLine = false;
      DateTime end = start.AddDays(days);
      dailyActivity.TotalCalls = 0;
      foreach (ReportListItem rep in _reports)
      {
        if ((rep.StartDate >= start) && (rep.StartDate < end) || (rep.EndDate >= start) && (rep.EndDate < end))
        {

          Analysis analysis = new Analysis(_model.SpeciesInfos, null, enModel.BAT_DETECT2);
          analysis.read(rep.ReportName, _model.DefaultModelParams);
          FilterItem filter = new FilterItem(-1, "query", expression.Replace('\n', ' '), false);

          if (analysis.Files.Count == 0)
          {
            MessageBoxResult res = MessageBox.Show(BatInspector.Properties.MyResources.QueryReportMissing, BatInspector.Properties.MyResources.Warning, MessageBoxButton.YesNo, MessageBoxImage.Warning);
            retVal = res == MessageBoxResult.Yes;
          }
          else
          {
            foreach (AnalysisFile file in analysis.Files)
            {
              DateTime t = file.RecTime;
              if (!((t.Date >= start) && (t < end)))
                continue;
              if ((dailyActivity.Latitude == 0) && (dailyActivity.Longitude == 0))
              {
                dailyActivity.Latitude = file.getDouble(Cols.LAT);
                dailyActivity.Longitude = file.getDouble(Cols.LON);
              }
              foreach (AnalysisCall call in file.Calls)
              {
                bool match = _model.Filter.apply(filter, call,
                              file.getString(Cols.REMARKS),
                              AnyType.getTimeString(file.RecTime), out retVal);
                if (!retVal)
                {
                  DebugLog.log("error parsing query expression: " + expression, enLogType.ERROR);
                  break;
                }
                if (match)
                {
                  int ticksPerHour = dailyActivity.Counter.Count / 24;
                  int idx = file.RecTime.Hour * ticksPerHour + file.RecTime.Minute * ticksPerHour / 60;
                  if (idx < dailyActivity.Counter.Count)
                  {
                    dailyActivity.Counter[idx]++;
                    if(dailyActivity.Counter[idx] > dailyActivity.MaxCount)
                      dailyActivity.MaxCount = dailyActivity.Counter[idx];
                    dailyActivity.TotalCalls++;
                  }
                  else
                    DebugLog.log("SumReport::createHeatMapLine, index error", enLogType.ERROR);
                }
              }
              if (!retVal)
                break;
            }
            addedLine = true;
            DebugLog.log($"evaluating report {rep.ReportName}  nr of calls: {dailyActivity.TotalCalls}", enLogType.INFO);
          }
        }
      }
      return retVal;
    }


    void initDirTree(string rootDir, string reportName)
    {
      _rootDir = rootDir;
      _dirInfo = new DirectoryInfo(rootDir);
      _reports.Clear();
      crawlDirTree(_dirInfo, reportName);
    }


    /// <summary>
    /// add entry to sum report
    /// </summary>
    /// <param name="item">report item</param>
    private void addEntryToCsvReport(SumReportItem item)
    {
      string dateStr = item.Date.ToString(AppParams.REPORT_DATE_FORMAT);
      _rep.addRow();
      int row = _rep.RowCnt;
      _rep.setCell(row, Cols.DATE, dateStr);
      _rep.setCell(row, Cols.DAYS, item.Days);
      _rep.setCell(row, Cols.LAT, item.Latitude);
      _rep.setCell(row, Cols.LON, item.Longitude);
      _rep.setCell(row, Cols.LANDSCAPE, item.Landscape);
      _rep.setCell(row, Cols.WEATHER, item.Weather);
      _rep.setCell(row, Cols.TEMP_MIN, item.TempMin);
      _rep.setCell(row, Cols.TEMP_MAX, item.TempMax);
      _rep.setCell(row, Cols.HUMID_MIN, item.HumidityMin);
      _rep.setCell(row, Cols.HUMID_MAX, item.HumidityMax);
      //      int startColTime = _rep.findInRow(1, Cols.T18H);
      foreach (SumItem sc in item.SpecList)
      {
        if (sc.Species != "")
        {
          if (_rep.findInRow(1, sc.Species) == 0)
            _rep.insertCol(_rep.ColCnt + 1, "0", sc.Species);
          _rep.setCell(row, sc.Species, sc.Count);
        }
      }
    }


    /// <summary>
    /// calculate sums for all species in the specified period
    /// </summary>
    /// <param name="date"></param>
    /// <param name="days"></param>
    /// <param name="expression">filter expression</param>
    /// <returns></returns>
    private SumReportItem getSums(DateTime date, int days, List<SpeciesInfos> species, string expression)
    {
      SumReportItem retVal = new SumReportItem(species);
      retVal.Date = date;
      DateTime end = date.AddDays(days);
      retVal.Days = days;
      List<SumItem> list = new List<SumItem>();
      foreach (SpeciesInfos speciesInfo in species)
        list.Add(new SumItem(speciesInfo.Abbreviation, 0));
      double latitude = 0;
      double longitude = 0; ;
      int sumCnt = 0;
      FilterItem filter = new FilterItem(-1, "query", expression.Replace('\n', ' '), false);

      foreach (ReportListItem rep in _reports)
      {
        if ((rep.StartDate >= date) && (rep.StartDate < end) || (rep.EndDate >= date) && (rep.EndDate < end))
        {
          Analysis analysis = new Analysis(_model.SpeciesInfos, null, enModel.BAT_DETECT2);
          analysis.read(rep.ReportName, _model.DefaultModelParams);
          if (analysis.Files.Count > 0)
          {
            foreach (AnalysisFile file in analysis.Files)
            {
              sumCnt++;
              latitude += file.getDouble(Cols.LAT);
              longitude += file.getDouble(Cols.LON);
              DateTime t = file.RecTime;
              if (!((t.Date >= date) && (t < end)))
                continue;
              foreach (AnalysisCall call in file.Calls)
              {
                bool match = _model.Filter.apply(filter, call,
                                 file.getString(Cols.REMARKS),
                                 AnyType.getTimeString(file.RecTime), out bool ok);
                if (!ok)
                {
                  DebugLog.log("error parsing query expression: " + expression, enLogType.ERROR);
                  break;
                }
                if (match)
                {
                  string spec = call.getString(Cols.SPECIES_MAN);
                  SumItem item = SumItem.find(spec, list, true);
                  item.Count++;
                  double temp = call.getDouble(Cols.TEMPERATURE);
                  if (temp < item.TempMin)
                    item.TempMin = temp;
                  if (temp < retVal.TempMin)
                    retVal.TempMin = temp;
                  if (temp > item.TempMax)
                    item.TempMax = temp;
                  if (temp > retVal.TempMax)
                    retVal.TempMax = temp;
                  double humid = call.getDouble(Cols.HUMIDITY);
                  if (humid < item.HumidityMin)
                    item.HumidityMin = humid;
                  if (humid < retVal.HumidityMin)
                    retVal.HumidityMin = humid;
                  if (humid > retVal.HumidityMax)
                    retVal.HumidityMax = humid;
                }
              }
            }
          }
        }
      }
      latitude /= sumCnt;
      longitude /= sumCnt;
      retVal.Latitude = latitude;
      retVal.Longitude = longitude;
      retVal.SpecList = list;
      return retVal;
    }

      /*    private SumReportItem getSums(DateTime date, int days, List<SpeciesInfos> species)
          {
            SumReportItem retVal = new SumReportItem(species);
            retVal.Date = date;
            DateTime end = date.AddDays(days);
            retVal.Days = days;
            List<SumItem> list = new List<SumItem>();
            foreach (SpeciesInfos speciesInfo in species)
              list.Add(new SumItem(speciesInfo.Abbreviation, 0));
            double latitude = 0;
            double longitude = 0; ;
            int sumCnt = 0;

            foreach (ReportListItem rep in _reports)
            {
              if ((rep.StartDate >= date) && (rep.StartDate < end) || (rep.EndDate >= date) && (rep.EndDate < end))
              {
                sumCnt++;
                Csv summary = new Csv();
                summary.read(rep.SummaryName, ";", true);
                int startColTime = summary.findInRow(1, Cols.T18H);
                retVal.Weather = summary.getCell(2, Cols.WEATHER).Replace(',', ' ');
                retVal.Landscape = summary.getCell(2, Cols.LANDSCAPE).Replace(',', ' ');
                for (int row = 2; row <= summary.RowCnt; row++)
                {
                  DateTime t = summary.getCellAsDate(row, Cols.DATE);
                  if ((t >= date) && (t < end))
                  {
                    latitude += summary.getCellAsDouble(row, Cols.LAT);
                    longitude += summary.getCellAsDouble(row, Cols.LON);
                    string spec = summary.getCell(row, Cols.SPECIES_MAN);
                    int count = summary.getCellAsInt(row, Cols.COUNT);
                    SumItem item = SumItem.find(spec, list, true);
                    item.Count += count;
                    if (retVal.TempMin > summary.getCellAsDouble(row, Cols.TEMP_MIN))
                      retVal.TempMin = summary.getCellAsDouble(row, Cols.TEMP_MIN);
                    if (retVal.TempMax < summary.getCellAsDouble(row, Cols.TEMP_MAX))
                      retVal.TempMax = summary.getCellAsDouble(row, Cols.TEMP_MAX);
                    if (retVal.HumidityMin > summary.getCellAsDouble(row, Cols.HUMID_MIN))
                      retVal.HumidityMin = summary.getCellAsDouble(row, Cols.HUMID_MIN);
                    if (retVal.HumidityMax < summary.getCellAsDouble(row, Cols.HUMID_MAX))
                      retVal.HumidityMax = summary.getCellAsDouble(row, Cols.HUMID_MAX);
                    for (int i = 0; i < item.CountTime.Length; i++)
                    {
                      int cnt = summary.getCellAsInt(row, startColTime + i);
                      item.CountTime[i] += cnt;
                    }
                  }
                }
              }
            }

            latitude /= sumCnt;
            longitude /= sumCnt;
            retVal.Latitude = latitude;
            retVal.Longitude = longitude;
            retVal.SpecList = list;
            return retVal;
          } */

      static int findInList(string s, List<string> list)
    {
      int retVal = -1;
      for (int i = 0; i < list.Count; i++)
      {
        if (list[i] == s)
        {
          retVal = i;
          break;
        }
      }
      return retVal;
    }

    public static List<string> getSpeciesInReport(List<SumReportItem> list)
    {
      List<string> retVal = new List<string>();
      foreach (SumReportItem item in list)
      {
        foreach (SumItem spec in item.SpecList)
        {
          if ((findInList(spec.Species, retVal) < 0) && (spec.Count > 0))
            retVal.Add(spec.Species);
        }
      }
      retVal.Sort();
      return retVal;
    }


    public void createWebPage(SumReportJson rep, string formDataName, List<SpeciesInfos> speciesInfo, string outputName)
    {
      WebReportDataJson formData = WebReportDataJson.load(formDataName);
      if (formData != null)
      {
        try
        {
          TextFile output = new TextFile();
          output.read(formData.Template);

          // head
          output.Replace("%YEAR%", rep.Days[0].Date.Year.ToString());
          output.Replace("%PAGE_NAME%", formData.PageName);
          output.Replace("%AUTHOR%", formData.Author);
          output.Replace("%LOCATION_DESCRIPTION%", formData.LocationDescription);
          output.Replace("%LOCATION_NAME%", formData.LocationName);
          output.Replace("%WEATHER%", formData.Weather);
          output.Replace("%REMARKS%", formData.Comment);
          output.Replace("%IMG_PORTRAIT%", formData.ImgPortrait);
          output.Replace("%IMG_LANDSCAPE%", formData.ImgLandscape);
          if (string.IsNullOrEmpty(formData.TimeSpan))
          {
            DateTime start = new DateTime(2100, 1, 1);
            DateTime end = new DateTime(1900, 1, 1);
            foreach (SumReportItem it in rep.Days)
            {
              if (it.Date < start)
                start = it.Date;
              if (it.Date > end)
                end = it.Date;
            }
            string str = start.ToString("dd.MM.yyyy") + " - " + end.ToString("dd.MM.yyyy") +
                         ", " + rep.Days.Count.ToString() + " Nächte";
            output.Replace("%TIME_SPAN%", str);
          }
          else
            output.Replace("%TIME_SPAN%", formData.TimeSpan);


          // list of detected species
          int i = output.findLine("%SPEC_ABR%");
          if (i >= 0)
          {
            string templateLine = output.getLine(i);
            output.removeLine(i);
            int lineNr = i;
            foreach (string spec in rep.Species)
            {
              if ((spec != "?") && (spec.ToLower() != "social"))
              {
                string line = templateLine;
                SpeciesInfos info = SpeciesInfos.findAbbreviation(spec, speciesInfo);
                if (info != null)
                {
                  line = line.Replace("%SPEC_ABR%", spec);
                  line = line.Replace("%SPEC_LAT%", info.Latin);
                  line = line.Replace("%SPEC_LOC%", info.Local);
                  line = line.Replace("%PERCENT%", rep.getPerCentStr(spec, 1));
                  SpeciesWebInfo webInfo = formData.findSpecies(spec);
                  if (webInfo != null)
                  {
                    line = line.Replace("%COMMENT%", webInfo.Comment);
                    line = line.Replace("%CONFUSION%", formData.findSpecies(spec).Confusion);
                  }
                  output.insert(lineNr, line);
                  lineNr++;
                }
              }
            }
          }

          // list of document recordings
          i = output.findLine("%WAV_SPEC_ABR%");
          if (i >= 0)
          {
            string templateLine = output.getLine(i);
            output.removeLine(i);
            int lineNr = i;
            for (int j = 1; j < rep.Species.Count; j++)
            {
              string line = templateLine;
              string pngFile = findFile(rep.Species[j], formData.WavFolder, AppParams.EXT_IMG);
              string wavFile = findFile(rep.Species[j], formData.WavFolder, AppParams.EXT_WAV);
              if ((pngFile.Length > 1) && (wavFile.Length > 1))
              {
                line = line.Replace("%WAV_SPEC_ABR%", SpeciesInfos.findAbbreviation(rep.Species[j], speciesInfo).Local);
                line = line.Replace("%PNG_NAME%", pngFile); //SpeciesInfos.findAbbreviation(spec, speciesInfo).Local
                line = line.Replace("%WAV_NAME%", wavFile);
                output.insert(lineNr, line);
                lineNr++;
              }
            }
          }

          // header for list of recording days
          i = output.findLine("%REP_SPEC%");
          if (i >= 0)
          {
            string line = output.getLine(i);
            rep.Species.Sort();
            line = line.Replace("%REP_SPEC%", rep.Species[0]);
            for (int j = 1; j < rep.Species.Count; j++)
              line += "**" + rep.Species[j] + "** |";
            output.setLine(i, line);
          }

          // list of recording days
          i = output.findLine("%REP_DATE%");
          if (i >= 0)
          {
            string templateLine = output.getLine(i);
            output.removeLine(i);
            int lineNr = i;
            foreach (SumReportItem it in rep.Days)
            {
              string line = templateLine;
              line = line.Replace("%REP_DATE%", it.Date.ToString("dd.MM.yyyy"));
              line = line.Replace("%REP_LAT%", it.Latitude.ToString("#.#####"));
              line = line.Replace("%REP_LON%", it.Longitude.ToString("#.#####"));
              line = line.Replace("%REP_TEMP%", it.TempMin.ToString("#.#") + " ... " + it.TempMax.ToString("#.#"));
              line = line.Replace("%REP_HUMID%", it.HumidityMin.ToString("#.#") + " ... " + it.HumidityMax.ToString("#.#"));
              SumItem sumIt = null;
              while ((sumIt == null) && (rep.Species.Count > 0))
              {
                sumIt = SumItem.find(rep.Species[0], it.SpecList);
                if(sumIt == null)
                  rep.Species.RemoveAt(0);
              }
              line = line.Replace("%REP_CNT%", sumIt.Count.ToString());
              for (int j = 1; j < rep.Species.Count; j++)
              {
                sumIt = SumItem.find(rep.Species[j], it.SpecList);
                if (sumIt != null)
                  line += " " + sumIt.Count.ToString() + " |";
              }
              output.insert(lineNr, line);
              lineNr++;
            }
          }

          // sums of all species
          i = output.findLine("%REP_SUM%");
          if (i >= 0)
          {
            string line = output.getLine(i);
            line = line.Replace("%REP_SUM%", rep.getSumStr(rep.Species[0]));
            for (int j = 1; j < rep.Species.Count; j++)
              line += " " + rep.getSumStr(rep.Species[j]) + " |";
            output.setLine(i, line);
          }


          output.saveAs(outputName);
        }
        catch (Exception ex)
        {
          DebugLog.log("failed to create web report: " + ex.ToString(), enLogType.ERROR);
        }
      }
      else
        DebugLog.log("could not load file '" + formDataName + "'", enLogType.ERROR);
    }

    private string findFile(string part, string folder, string extension)
    {
      string retVal = "";
      DirectoryInfo d = new DirectoryInfo(folder);
      FileInfo[] files = d.GetFiles("*" + extension);
      foreach (FileInfo file in files) 
      {
        if (Path.GetFileName(file.Name).ToLower().IndexOf(part.ToLower()) >= 0)
        {
          retVal = file.Name;
          break;
        }
      }
      return retVal;
    }
  }

  public class TextFile
  {
    List<string> _lines;
    string _name = "";

    public void read(string name)
    {
      _name = name;
      _lines = File.ReadAllLines(name).ToList<string>();
    }

    public void save()
    {
      File.WriteAllLines(_name, _lines.ToArray());
    }

    public void saveAs(string name)
    {
      File.WriteAllLines(name, _lines.ToArray());
    }

    public string getLine(int i)
    {
      string retVal = "";
      if ((i >= 0) && (i < _lines.Count))
        retVal = _lines[i];
      return retVal;
    }

    public void setLine(int i, string str)
    {
      if ((i >= 0) && (i < _lines.Count))
        _lines[i] = str;
    }

    public int findLine(string str)
    {
      int retVal = -1;
      for (int i = 0; i < _lines.Count; i++)
      {
        if (_lines[i].IndexOf(str) >= 0)
        {
          retVal = i;
          break;
        }
      }
      return retVal;
    }

    public void insert(int i, string line)
    {
      _lines.Insert(i, line);
    }

    public void removeLine(int i)
    {
      if ((i >= 0) && (i < _lines.Count))
        _lines.RemoveAt(i);
    }

    public void Replace(string oldStr, string newStr)
    {
      for (int i = 0; i < _lines.Count; i++)
      {
        _lines[i] = _lines[i].Replace(oldStr, newStr);
      }
    }
  }
}
