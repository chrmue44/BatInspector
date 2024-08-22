using BatInspector.Properties;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System.Collections.Generic;



namespace BatInspector
{
  public class HeatMap
  {
    List<stAxisItem> _axisItems;
    PlotModel _plotModel;

    public HeatMap() 
    {
      _axisItems = new List<stAxisItem>();

    }

    public PlotModel PlotModel { get { return _plotModel; } set { _plotModel = value; } }


    public PlotModel createHeatMap(List<List<int>> hm, string title)
    {
      _plotModel = new PlotModel { Title = title };
      initPlotModel();
      double[,] data = toDoubleArray(hm, out double max);
      var heatMapSeries = new HeatMapSeries
      {
        X0 = 0,
        X1 = data.GetLength(0),
        Y0 = 0,
        Y1 = data.GetLength(1),
        Interpolate = false,
        RenderMethod = HeatMapRenderMethod.Bitmap,
        Data = data
      };
      _plotModel.Series.Add(heatMapSeries);
      return PlotModel;
    }


    private double[,] toDoubleArray(List<List<int>> hm, out double max)
    {
      int c = hm[0].Count;
      double[,] retVal = new double[hm.Count,c];
      max = 0;
      for (int i = 0; i < retVal.GetLength(0); i++)
      {
        for (int j = 0; j < c; j++)
        {
          retVal[i, j] = (double)hm[i][j];
          if (max < retVal[i, j])
            max = retVal[i, j];
        }
      }
      return retVal;
    }

    private void initPlotModel() 
    {
      _plotModel.Axes.Add(new LinearColorAxis
      {
        Palette = OxyPalettes.Cool(100)
      });
      
      LinearAxis xAxis = new LinearAxis
      {
        Position = AxisPosition.Bottom,
        Minimum = 0,
        Maximum = 200,
        Title = "Datum",
        Unit = "",
        MajorGridlineStyle = LineStyle.Solid,
        MajorGridlineColor = OxyColor.FromArgb(255, 220, 220, 220)
      };
      xAxis.MinimumMajorStep = 1;
      

      _plotModel.Axes.Add(xAxis);

      _plotModel.Axes.Add(new LinearAxis
      {
        Position = AxisPosition.Left,
        Minimum = 0,
        Maximum = 24,
        Title = "Uhrzeit",
        Unit = "",
        MajorGridlineStyle = LineStyle.Solid,
        MajorGridlineColor = OxyColor.FromArgb(255, 220, 220, 220)
      });
    }
  }
}
