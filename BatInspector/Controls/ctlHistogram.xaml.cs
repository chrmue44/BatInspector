using libParser;
using System.Globalization;
using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;

namespace BatInspector.Controls
{
  /// <summary>
  /// Interaction logic for ctlHistogram.xaml
  /// </summary>
  public partial class ctlHistogram : UserControl
  {
    Histogram _histogram = null;
    Brush _brushBar = Brushes.Blue;
    int _borderBar = 1;
    double _rx_x = 33;  //x position x axis
    double _wb = 5;     // width of histogram bar
    double[] _ticksX = null;

    public ctlHistogram()
    {
      InitializeComponent();
      int lblW = 50;
      _ctlStdDev.setup(BatInspector.Properties.MyResources.StdDev, enDataType.DOUBLE, 2, lblW);
      _ctlMean.setup(BatInspector.Properties.MyResources.Mean, enDataType.DOUBLE, 2,   lblW);
      _ctlCnt.setup(BatInspector.Properties.MyResources.Count, enDataType.INT,2,lblW);
    }

    public void initHistogram(Histogram histogram, string title, double[] ticks = null)
    {
      _grp.Header = title;
      _histogram = histogram;
      _ticksX = ticks;
    }

    public void createHistogram()
    {
      if (_histogram != null)
      {
        double ry_x = 30;
        double ry_y = 0;
        double borderY = 95;
        double borderX = 50;
        _rx_x = 33;
        double rx_y = this.ActualHeight - borderY;
        double h_histo = this.ActualHeight - ry_y - borderY;
        double w_histo = this.ActualWidth - ry_x - borderX;
        _wb = w_histo / _histogram.Classes.Count;
        _cnv.Children.Clear();
         GraphHelper.createRulerY(_cnv, ry_x, ry_y, h_histo, 0, _histogram.MaxCount);
        if(_ticksX == null)
          GraphHelper.createRulerX(_cnv, _rx_x, rx_y, w_histo, _histogram.Min, _histogram.Max);
        else
          GraphHelper.createRulerX(_cnv, _rx_x, rx_y, w_histo, _histogram.Min, _histogram.Max, _ticksX);

        for (int i = 0; i < _histogram.Classes.Count; i++)
        {
          double hh = h_histo * _histogram.Classes[i] / _histogram.MaxCount;
          double xb = _rx_x + _wb * i;
          paintBar(xb, h_histo + ry_y, _wb, hh);
        }
        _ctlMean.setValue(_histogram.Mean);
        _ctlStdDev.setValue(_histogram.StdDev);
        _ctlCnt.setValue(_histogram.Count);
      }
    }

    private void paintBar(double x, double y, double width, double height)
    {
      GraphHelper.createBox(_cnv, x , y - height , x + width , y, _borderBar,Brushes.Blue, Brushes.LightBlue);
    }

    private void _cnv_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
    {
      try
      {
        Point p = e.GetPosition(_cnv);
        int classNr = (int)((p.X - _rx_x) / _wb) + 1;
        if (classNr < 1)
          classNr = 1;
        if(classNr > _histogram.Classes.Count)
          classNr = _histogram.Classes.Count;

        if (!_ftToolTip.IsOpen)
          _ftToolTip.IsOpen = true;

        _tbf.Text =  BatInspector.Properties.MyResources.Class + " " + classNr.ToString() + "/" + _histogram.Classes.Count.ToString() +"\n" +
        "min:" + _histogram.getClassMin(classNr - 1).ToString("#.#", CultureInfo.InvariantCulture) +
        ", max:" + _histogram.getClassMax(classNr - 1).ToString("#.#", CultureInfo.InvariantCulture) + "\n" +
        BatInspector.Properties.MyResources.Quantity + ": " + _histogram.Classes[classNr - 1].ToString();
        _ftToolTip.HorizontalOffset = p.X + 20;
        _ftToolTip.VerticalOffset = p.Y + 20;
        DebugLog.log("Histogram: mouse move", enLogType.DEBUG);
      }
      catch (Exception ex)
      {
        DebugLog.log("Histogram: mouse move failed: " + ex.ToString(), enLogType.ERROR);
      }

    }

    private void _cnv_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
    {
      _ftToolTip.IsOpen = false;
    }
  }
}
