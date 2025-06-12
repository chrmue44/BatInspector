
using libScripter;
using System.Collections.Generic;
using System.Windows.Documents;
using static System.Windows.Forms.AxHost;

namespace BatInspector
{ 
  public class Statistic
  {
    const int CLASSES_PER_HOUR = 2;
//    public const int HOURS_PER_NIGHT = 11;
    const int TIME_CLASSES = CLASSES_PER_HOUR * 24;
    Histogram _fmin;
    Histogram _fmax;
    Histogram _fmaxAmp;
    Histogram _duration;
    Histogram _callDist;
    Histogram _recTime;

    string _fileExpression = "";
    public Histogram Fmin { get { return _fmin; } }
    public Histogram Fmax { get { return _fmax; } }
    public Histogram Duration { get { return _duration; } }
    public Histogram CallDist { get { return _callDist;} }
    public Histogram FmaxAmp { get { return _fmaxAmp; } }
    public Histogram RecTime { get{ return _recTime; } }

    public Statistic(int classes)
    {
      _fmin = new Histogram(classes);
      _fmin.init(0, 70);
      _fmax = new Histogram(classes);
      _fmax.init(20, 120);
      _duration = new Histogram(classes);
      _duration.init(0, 40);
      _callDist = new Histogram(classes);
      _callDist.init(0, 500);
      _fmaxAmp = new Histogram(classes);
      _fmaxAmp.init(0, 120);
      _recTime = new Histogram(TIME_CLASSES);
      _recTime.init(0, TIME_CLASSES - 1);
    }

    public void calcStatistic(FilterItem filterExp, Analysis analysis, Filter filter)
    {
      _fmin.init(_fmin.Min, _fmin.Max);
      _fmax.init(_fmax.Min, _fmax.Max);
      _fmaxAmp.init(_fmaxAmp.Min, _fmaxAmp.Max);
      _duration.init(_duration.Min, _duration.Max);
      _callDist.init(_callDist.Min, _callDist.Max);
      _recTime.init(_recTime.Min, _recTime.Max); 
      if(filterExp  != null) 
        _fileExpression = filterExp.Expression;
      else
        _fileExpression = "";


      foreach (AnalysisFile f in analysis.Files)
      {
        int hClass = getHourClass(f);
          
        foreach (AnalysisCall c in f.Calls)
        {
          bool res = (filterExp == null) || filter.apply(filterExp, c);
          if (res)
          {
            _fmin.add(c.getDouble(Cols.F_MIN)/1000);
            _fmax.add(c.getDouble(Cols.F_MAX)/1000);
            _fmaxAmp.add(c.getDouble(Cols.F_MAX_AMP)/1000);
            _duration.add(c.getDouble(Cols.DURATION));
            double callInterval = c.getDouble(Cols.CALL_INTERVALL);
            if (callInterval > 0)      // <= 0 means: no call interval detected
              _callDist.add(callInterval);
            _recTime.add(hClass);
          }
        }
      }
    }

    /// <summary>
    /// calculate the histogram class number from recording time. 
    /// Assumption is that 20:00 is the start of the frst class
    /// </summary>
    /// <param name="f">parameters of a recording file</param>
    /// <returns></returns>
    int getHourClass(AnalysisFile f) 
    {
      int h = f.RecTime.Hour;
      int m = f.RecTime.Minute;
      int hClass = h * CLASSES_PER_HOUR;
      hClass += m * CLASSES_PER_HOUR / 60;
      return hClass;
    }

    public void exportToCsv(string filename, string prjName) 
    {
      Csv csv = new Csv();
      int row = 1;

      csv.addRow();
      csv.setCell(row, 1, "Project");
      csv.setCell(row++, 2, prjName);
      csv.addRow();
      csv.setCell(row, 1, "Filter");
      csv.setCell(row++, 2, _fileExpression);

      csv.addRow();
      csv.setCell(row, 7, "Classes");
      row++;

      List<string> h = new List<string>() { "Parameter","Count","Min","Max","Mean","StdDev"};
      for(int i = 0; i < _fmin.Classes.Count;  i++) 
        h.Add((i+1).ToString());
      row++;
      csv.addRow(h);

      addHistogramToCsv(csv, row++, _fmin, "Fmin");
      addHistogramToCsv(csv, row++, _fmax, "Fmax");
      addHistogramToCsv(csv, row++, _fmaxAmp, "FmaxAmp");
      addHistogramToCsv(csv, row++, _duration, "Duration");
      addHistogramToCsv(csv, row++, _callDist, "CallInterval");

      csv.saveAs(filename);
    }


    public void addHistogramToCsv(Csv csv, int row, Histogram h, string name)
    {
      csv.addRow();
      int col = 1;
      csv.setCell(row, col++, name);
      csv.setCell(row, col++, h.Count);
      csv.setCell(row, col++, h.Min);
      csv.setCell(row, col++, h.Max);
      csv.setCell(row, col++, h.Mean);
      csv.setCell(row, col++, h.StdDev);
      for(int i = 0; i < h.Classes.Count; i++)
        csv.setCell(row, col++, h.Classes[i]);
    }
  }
}
