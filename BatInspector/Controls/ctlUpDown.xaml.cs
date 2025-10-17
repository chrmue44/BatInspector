using System.Globalization;
using System.Windows;
using System.Windows.Controls;


namespace BatInspector.Controls
{
  public delegate void dlgUpDown(double value);

  /// <summary>
  /// Interaktionslogik für ctlUpDown.xaml
  /// </summary>
  public partial class ctlUpDown : UserControl
  {
    double _min = -100;
    double _max = 100;
    double _val = 0;
    double _step = 5;
    dlgUpDown _dlg;

    public ctlUpDown()
    {
      InitializeComponent();
    }

    public double Value { get { return _val; } }

    public void setup(double min, double max, double initVal, double step, dlgUpDown dlg)
    {
      _min = min;
      _max = max;
      _val = initVal;
      _step = step;
      _tbText.Text = _val.ToString();
      _dlg = dlg;
    }

    private void _btnUp_Click(object sender, RoutedEventArgs e)
    {
      bool ok = double.TryParse(_tbText.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out double val);
      if (ok)
      {
        val += _step;
        if (val > _max)
          val = _max;
        _val = val;
        _tbText.Text = val.ToString(CultureInfo.InvariantCulture);
        if (_dlg != null)
          _dlg(_val);
      }
    }

    private void _btnDown_Click(object sender, RoutedEventArgs e)
    {
      bool ok = double.TryParse(_tbText.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out double val);
      if (ok)
      {
        val -= _step;
        if (val < _min)
          val = _min;
        _val = val;
        _tbText.Text = val.ToString(CultureInfo.InvariantCulture);
        if (_dlg != null)
          _dlg(_val);
      }
    }
  }
}
