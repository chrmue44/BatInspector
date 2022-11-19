using BatInspector.Properties;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;


namespace BatInspector
{

  public enum enScatterAxis
  {
    FreqMaxAmp,
    FreqMin,
    FreqMax,
    FreqKnee,
    Duration,
    DistToPrev,
    Fc
  }

  public struct stAxisItem
  {
    public stAxisItem(enScatterAxis name, string unit)
    {
      Name = name;
      Unit = unit;
    }

    public enScatterAxis Name;
    public string Unit;
  }
  

  public class ScatterDiagram
  {
    List<stAxisItem> _axisItems;
    PlotModel _plotModel;

    public PlotModel PlotModel { get { return _plotModel; }  set { _plotModel = value; } }
    public List<stAxisItem> AxisItems { get { return _axisItems; } }

    public ScatterDiagram()
    {
      _axisItems = new List<stAxisItem>();
      _plotModel = new PlotModel { Title = MyResources.frmMainScatterDiagram };
      initPlotModel();
  //    dummyDiag();
    }

    
    public void initPlotModel()
    {
      _axisItems.Clear();
      _axisItems.Add(new stAxisItem(enScatterAxis.FreqMaxAmp, "Hz"));
      _axisItems.Add(new stAxisItem(enScatterAxis.FreqMin, "Hz"));
      _axisItems.Add(new stAxisItem(enScatterAxis.FreqMax, "Hz"));
      _axisItems.Add(new stAxisItem(enScatterAxis.FreqKnee, "Hz"));
      _axisItems.Add(new stAxisItem(enScatterAxis.Fc, "Hz"));
      _axisItems.Add(new stAxisItem(enScatterAxis.Duration, "ms"));
      _axisItems.Add(new stAxisItem(enScatterAxis.DistToPrev, "ms"));
/*      _axisItems.Add(new stAxisItem("bandwidth", fMax, "kHz"));
      _axisItems.Add(new stAxisItem("freq_start", fMax, "kHz"));
      _axisItems.Add(new stAxisItem("freq_25", fMax, "kHz"));
      _axisItems.Add(new stAxisItem("freq_center", fMax, "kHz"));
      _axisItems.Add(new stAxisItem("freq_75", fMax, "kHz"));
      _axisItems.Add(new stAxisItem("freq_end", fMax, "kHz"));
      _axisItems.Add(new stAxisItem("fc", fMax, "kHz"));
      _axisItems.Add(new stAxisItem("freq_bw_knee_fc", fMax, "kHz"));
      _axisItems.Add(new stAxisItem("pc_freq_max_amp", fMax, "kHz"));
      _axisItems.Add(new stAxisItem("pc_freq_max", fMax, "kHz"));
      _axisItems.Add(new stAxisItem("pc_freq_min", fMax, "kHz"));
      _axisItems.Add(new stAxisItem("pc_knee", fMax, "kHz")); */
    }
    
    public void createScatterDiagram(stAxisItem xAxis, stAxisItem yAxis, ViewModel model, FilterItem filter, bool freezeAxes)
    { 
      ScatterSeries scatterSeries = new ScatterSeries { MarkerType = MarkerType.Square };
      if(!freezeAxes)
        _plotModel.Axes.Clear();
      _plotModel.Series.Clear();


      scatterSeries.TrackerFormatString = "\n{1}:{2:0.0} " + xAxis.Unit +",\n{3}:{4:0.0} "+ yAxis.Unit + "\n{Tag}";
      double color = 75;
      double size = 2;
      double x = 0;
      double y = 0;
      double xMax = 10;
      double yMax = 10;
      foreach (AnalysisFile f in model.Analysis.Files)
      {
        foreach(AnalysisCall c in f.Calls)
        {
          bool res = (filter == null) || model.Filter.apply(filter, c);
          if (res)
          {
            x = getAxisValue(xAxis.Name, c);
            if (x > xMax) xMax = x;
            y = getAxisValue(yAxis.Name, c);
            if (y > yMax) yMax = y;
            string str = f.getString(Cols.NAME) + "_" + c.getInt(Cols.NR).ToString();
            scatterSeries.Points.Add(new ScatterPoint(x, y, size, color, str));
          }
        }
      }
      if (!freezeAxes || (_plotModel.Axes.Count < 2))
      {
        _plotModel.Axes.Add(new LinearAxis
        {
          Position = AxisPosition.Bottom,
          Minimum = 0,
          Maximum = xMax,
          Title = xAxis.Name.ToString(),
          Unit = xAxis.Unit,
          MajorGridlineStyle = LineStyle.Solid,
          MajorGridlineColor = OxyColor.FromArgb(255, 220, 220, 220)
        });

        _plotModel.Axes.Add(new LinearAxis
        {
          Position = AxisPosition.Left,
          Minimum = 0,
          Maximum = yMax,
          Title = yAxis.Name.ToString(),
          Unit = yAxis.Unit,
          MajorGridlineStyle = LineStyle.Solid,
          MajorGridlineColor = OxyColor.FromArgb(255, 220, 220, 220)
        });
      }
      _plotModel.Series.Add(scatterSeries);
    }

    
    void dummyDiag()
    {
      ScatterSeries scatterSeries = new ScatterSeries { MarkerType = MarkerType.Square };
      _plotModel.Axes.Clear();
      _plotModel.Series.Clear();

      double color = 75;
      double size = 2;
      var r = new Random(314);
        for (int i = 0; i < 100; i++)
        {
          var x = r.NextDouble() * 10;
          var y = r.NextDouble() * 2 - 1;
          size = 2; // r.Next(5, 15);
          color = 75; // r.Next(100, 1000);
        scatterSeries.Points.Add(new ScatterPoint(x, y, size, color));
      }
      _plotModel.Series.Add(scatterSeries);

    } 
    

    public stAxisItem findAxisItem(string name)
    {
      stAxisItem retVal = _axisItems[0];
      foreach(stAxisItem it in _axisItems)
      {
        if (it.Name.ToString() == name)
          retVal = it;
      }
      return retVal;
    }

    double getAxisValue(enScatterAxis type, AnalysisCall call)
    {
      double retVal = 0;
      switch (type)
      {
        case enScatterAxis.FreqMaxAmp:
          retVal = call.getDouble (Cols.F_MAX_AMP);
          break;
        case enScatterAxis.FreqMin:
          retVal = call.getDouble(Cols.F_MIN);
          break;
        case enScatterAxis.FreqMax:
          retVal = call.getDouble(Cols.F_MAX);
          break;
        case enScatterAxis.FreqKnee:
          retVal = call.getDouble(Cols.F_KNEE);
          break;
        case enScatterAxis.Duration:
          retVal = call.getDouble(Cols.DURATION);
          break;
        case enScatterAxis.DistToPrev:
          retVal = call.DistToPrev;
          break;
        case enScatterAxis.Fc:
          retVal = call.getDouble(Cols.FC);
          break;
        default:
          retVal = 0;
          break;
      }
      return retVal;
    }
  }
}
