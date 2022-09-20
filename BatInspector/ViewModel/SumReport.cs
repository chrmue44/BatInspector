using libParser;
using libScripter;
using System;
using System.Collections.Generic;
using System.IO;

namespace BatInspector
{
  public enum enPeriod
  {
    DAILY = 0,
    WEEKLY = 1,
    MONTHLY = 2
  }

  class SpecCnt
  {
    public string Species { get; }
    public int Count { get; } 

    public SpecCnt(string s, int c)
    {
      Species = s;
      Count = c;
    }
  }

  public class SumReport
  {
    const string COL_DATE = "Date";

    List<SpeciesInfos> _species;
    Csv _rep;

    public SumReport(List<SpeciesInfos> species)
    {
      _species = species;
      _rep = new Csv(true);
    }

    public void createReport(DateTime start, DateTime end, enPeriod period, string rootDir)
    {
      _rep.clear();
      _rep.addRow();
      int col = 1;
      _rep.insertCol(col++, COL_DATE);

      List<SpecCnt> list;
      DateTime date = start;
      while (date < end)
      {
        list = getSums(date);
        int days = calcDays(date, end, period);
        addEntry(date, days, list);
        date = incrementDate(date, period);
      }

      if (Directory.Exists(rootDir))
      {
        string fileName = rootDir + "/" + "sum_report.csv";
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

    void addEntry(DateTime date, int days, List<SpecCnt> list)
    {
      string dateStr = date.ToString("yyyy-MM-dd");
      _rep.addRow();
      int row = _rep.RowCnt;
      _rep.setCell(row, COL_DATE, dateStr);

      foreach(SpecCnt sc in list)
      {
        if(_rep.findInRow(1, sc.Species) == 0)
        {
          _rep.insertCol(_rep.ColCnt + 1, "0");
          _rep.setCell(1, sc.Species, sc.Species);
        }
        _rep.setCell(row, sc.Species, sc.Count);
      }
      
    }

    List<SpecCnt> getSums(DateTime date)
    {
      List<SpecCnt> retVal = new List<SpecCnt>();

      return retVal;
    }
  }
}
