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
using libParser;
using libScripter;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace BatInspector
{
  public enum enPeriod
  {
    DAILY = 0,
    WEEKLY = 1,
    MONTHLY = 2
  }

  public class SpecListItem
  {
    public string Species { get; }
    public int Count { get; set; } 

    public SpecListItem(string s, int c)
    {
      Species = s;
      Count = c;
    }

    public static SpecListItem find(string s, List<SpecListItem> list)
    {
      SpecListItem retVal = null;
      foreach(SpecListItem item in list) 
      {
        if (item.Species == s)
        {
          retVal = item;
          break;
        }
      }
      return retVal;
    }
  }

  public class ReportListItem
  {
    public string FileName { get; set; }
    public DateTime Date { get; set; }
  }

  public class SumReportItem
  {
    public DateTime Date { get; set; }
    public int Days { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string Landscape { get; set; }
    public string Weather { get; set; }
    public List<SpecListItem> SpecList { get; set; }

    public SumReportItem()
    {
      SpecList = new List<SpecListItem>();
    }
  }


  public class SumReport
  {
    private List<SpeciesInfos> _species;
    private Csv _rep;
    private string _rootDir;
    private DirectoryInfo _dirInfo;
    private List<ReportListItem> _reports;

    public SumReport(List<SpeciesInfos> species)
    {
      _species = species;
      _rep = new Csv(true);
      _reports = new List<ReportListItem>();
    }

    public void createReport(DateTime start, DateTime end, enPeriod period, string rootDir)
    {
      initDirTree(rootDir);
      _rep = new Csv(true);
      string[] header = { Cols.DATE, Cols.DAYS, Cols.LAT, Cols.LON, Cols.LANDSCAPE, Cols.WEATHER };
      _rep.initColNames(header, true);

      SumReportItem item;
      DateTime date = start;
      while (date < end)
      {
        int days = calcDays(date, end, period);        
        item = getSums(date, days);
        if(item.SpecList.Count > 0)
          addEntry(item);
        date = incrementDate(date, period);
      }

      if (Directory.Exists(rootDir))
      {
        string fileName = rootDir + "/" + AppParams.SUM_REPORT;
        _rep.saveAs(fileName);
      }
      else
        DebugLog.log("unable to save sum report, directory does not exist: " + rootDir, enLogType.ERROR);
    }

    public int calcDays(DateTime date, DateTime end, enPeriod period)
    {
      int days = 0;
      bool exit = false;
      int oldMonth = date.Month;
      while((date < end) && !exit)
      {
        days++;
        date = date.AddDays(1);
        switch(period)
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

    void initDirTree(string rootDir)
    {
      _rootDir = rootDir;
      _dirInfo = new DirectoryInfo(rootDir);
      foreach (DirectoryInfo subDir in _dirInfo.GetDirectories())
      {
        string prjFile = Project.containsReport(subDir, AppParams.PRJ_SUMMARY);
        if ( prjFile != "")
        {
          Csv csv = new Csv();
          csv.read(prjFile, AppParams.CSV_SEPARATOR, true);
          string dateStr = csv.getCell(2, Cols.DATE);
          DateTime date = new DateTime();
          try
          {
            date = DateTime.ParseExact(dateStr, AppParams.REPORT_DATE_FORMAT, CultureInfo.InvariantCulture);
          }
          catch
          {
            DebugLog.log("unrecognized date format in " + prjFile + ": " + dateStr, enLogType.ERROR);
          }
          ReportListItem rec = new ReportListItem
          {
            FileName = prjFile,
            Date = date
          };
          _reports.Add(rec);
        }
      }
    }

    
    /// <summary>
    /// add entry to sum report
    /// </summary>
    /// <param name="item">report item</param>
    private void addEntry(SumReportItem item)
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
      foreach(SpecListItem sc in item.SpecList)
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
    /// <returns></returns>
    private SumReportItem getSums(DateTime date, int days)
    {
      SumReportItem retVal = new SumReportItem();
      retVal.Date = date;
      DateTime end = date.AddDays(days);
      retVal.Days = days;
      List<SpecListItem> list = new List<SpecListItem>();
      double latitude = 0;
      double longitude = 0; ;
      int sumCnt = 0;

      foreach (ReportListItem rep in _reports)
      {
        if((rep.Date >= date) && (rep.Date < end))
        {
          sumCnt++;
          Csv summary = new Csv();
          summary.read(rep.FileName, ";", true);
          retVal.Weather = summary.getCell(2, Cols.WEATHER);
          retVal.Landscape = summary.getCell(2, Cols.LANDSCAPE);
          for (int row = 2; row <= summary.RowCnt; row++)
          {
            latitude += summary.getCellAsDouble(row, Cols.LAT);
            longitude += summary.getCellAsDouble(row, Cols.LON);
            string spec = summary.getCell(row, Cols.SPECIES_MAN);
            int count = summary.getCellAsInt(row, Cols.COUNT);  
            SpecListItem item = SpecListItem.find(spec, list);
            if (item == null)
            {
              item = new SpecListItem(spec, 0);
              list.Add(item);
            }
            item.Count += count;
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
  }
}
