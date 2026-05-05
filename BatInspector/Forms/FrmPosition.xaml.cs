using BatInspector.Properties;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows;


namespace BatInspector.Forms
{
  /// <summary>
  /// Interaction logic for FrmPosition.xaml
  /// </summary>
  public partial class FrmPosition : Window
  {
    ExpressionGenerator _exp;  

    public FrmPosition(ExpressionGenerator exp, string title)
    {
      InitializeComponent();
      Title = title;
      setup();
      _exp = exp;
    }

    private void setup()
    {
      int  wLbl = 150;
      _ctlLat.setup(MyResources.FrmPosLatitude, Controls.enDataType.DOUBLE, 6, wLbl, true);
      _ctlLon.setup(MyResources.FrmPosLongitude, Controls.enDataType.DOUBLE, 6, wLbl, true);
      _ctlDist.setup(MyResources.FrmPosDistance, Controls.enDataType.DOUBLE, 1, wLbl, true);
      if (Title == MyResources.ExpGenIsNear)
      {
        _ctlDist.Visibility = Visibility.Visible;
        _tbHint.Text = MyResources.FrmPosHintIsNear;
      }
      else
      {
        _tbHint.Text = MyResources.FrmPosHintCalcDistance;
        _ctlDist.Visibility = Visibility.Collapsed;
      }
    }

    private void _bntOk_Click(object sender, RoutedEventArgs e)
    {
      if (Title == MyResources.ExpGenIsNear)
      {
        _exp.GeoExpression = $"isNear({DBBAT.LAT}, {DBBAT.LON}, {_ctlLat.getDoubleValue().ToString(CultureInfo.InvariantCulture)}, " +
                             $"{_ctlLon.getDoubleValue().ToString(CultureInfo.InvariantCulture)}, {_ctlDist.getDoubleValue().ToString(CultureInfo.InvariantCulture)})";
      }
      else
      { 
        _exp.GeoExpression = $"calcDistance({DBBAT.LAT}, {DBBAT.LON}, {_ctlLat.getDoubleValue().ToString(CultureInfo.InvariantCulture)}, " +
                             $"{_ctlLon.getDoubleValue().ToString(CultureInfo.InvariantCulture)})";
      }
      this.DialogResult = true;
      this.Close();
    }
  }
}
