using System.Windows.Controls;
using System.Windows.Media;


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
    public ctlHistogram()
    {
      InitializeComponent();
    }

    public void initHistogram(Histogram histogram, string title)
    {
      _grp.Header = title;
      _histogram = histogram;
    }

    public void createHistogram()
    {
      if (_histogram != null)
      {
        double ry_x = 30;
        double ry_y = 0;
        double rx_x = 33;
        double borderY = 78;
        double borderX = 43;
        double rx_y = this.ActualHeight - borderY;
        double h_histo = this.ActualHeight - ry_y - borderY;
        double w_histo = this.ActualWidth - ry_x - borderX;
        _cnv.Children.Clear();
        GraphHelper.createRulerY(_cnv, ry_x, ry_y, h_histo, 0, _histogram.MaxCount);
        GraphHelper.createRulerX(_cnv, rx_x, rx_y, w_histo, _histogram.Min, _histogram.Max);
        for (int i = 0; i < _histogram.Classes.Count; i++)
        {
          double hh = h_histo * _histogram.Classes[i] / _histogram.MaxCount;
          double wb = w_histo / _histogram.Classes.Count;
          double xb = rx_x + wb * i;
          paintBar(xb, h_histo + ry_y, wb, hh);
        }
      }
    }

    private void paintBar(double x, double y, double width, double height)
    {
      GraphHelper.createLine(_cnv, x, y, x, y - height, _brushBar, _borderBar);
      GraphHelper.createLine(_cnv, x, y, x + width, y, _brushBar, _borderBar);
      GraphHelper.createLine(_cnv, x + width - _borderBar, y, x + width - _borderBar, y -height, _brushBar, _borderBar);
      GraphHelper.createLine(_cnv, x , y - height , x + width, y -height , _brushBar, _borderBar);
      GraphHelper.createFill(_cnv, x + _borderBar, y - height + _borderBar, x + width - _borderBar, y - _borderBar, Brushes.LightBlue);
    }
  }
}
