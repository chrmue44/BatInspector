
using static System.Windows.Forms.AxHost;

namespace BatInspector
{ 
  public class Statistic
  {
    Histogram _fmin;
    Histogram _fmax;
    Histogram _fmaxAmp;
    Histogram _duration;
    Histogram _callDist;
    public Histogram Fmin { get { return _fmin; } }
    public Histogram Fmax { get { return _fmax; } }
    public Histogram Duration { get { return _duration; } }
    public Histogram CallDist { get { return _callDist;} }
    public Histogram FmaxAmp { get { return _fmaxAmp; } }

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
    }

    public void calcStatistic(FilterItem filterExp, Analysis analysis, Filter filter)
    {
      _fmin.init(_fmin.Min, _fmin.Max);
      _fmax.init(_fmax.Min, _fmax.Max);
      _fmaxAmp.init(_fmaxAmp.Min, _fmaxAmp.Max);
      _duration.init(_duration.Min, _duration.Max);
      _callDist.init(_callDist.Min, _callDist.Max);

      foreach (AnalysisFile f in analysis.Files)
      {
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
          }
        }
      }
    }
  }
}
