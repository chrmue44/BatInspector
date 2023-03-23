using BatInspector.Properties;
using System.Globalization;
using System.Windows;


namespace BatInspector.Forms
{
  /// <summary>
  /// Interaction logic for FrmCreateXmlInfos.xaml
  /// </summary>
  public partial class FrmCreateXmlInfos : Window
  {
    ViewModel _model;
    public FrmCreateXmlInfos(ViewModel model)
    {
      InitializeComponent();
      _model = model;
      _ctlLat.setup(MyResources.Latitude, Controls.enDataType.STRING, 0, 120, 120, true);
      _ctlLon.setup(MyResources.Longitude, Controls.enDataType.STRING, 0, 120, 120, true);
    }


    public bool parseLatitude(string coordStr, out double coord)
    {
      return parseGeoCoord(coordStr, out coord, "N", "S", 90);
    }

    public bool parseLongitude(string coordStr, out double coord)
    {
      return parseGeoCoord(coordStr, out coord, "E", "W", 180);
    }

    bool parseGeoCoord(string coordStr, out double coord, string hem1, string hem2, int maxDeg)

    {
      bool retVal = true;
      int deg= 0;
      int sign = 0;
      int pos = coordStr.IndexOf("°");
      if (pos >= 0)
      {
        string degStr = coordStr.Substring(0, pos);
        retVal = int.TryParse(degStr, out deg);
      }
      else
        retVal = false;
      coord = 0;
      int pos2 = coordStr.IndexOf(hem1);
      if (pos2 < 0)
      {
        pos2 = coordStr.IndexOf(hem2);
        if (pos2 >= 0)
          sign = 1;
        else
          retVal = false;
      }

      if ((deg < -maxDeg) || (deg > maxDeg))
        retVal = false; 

      if(retVal)
      {
        string minStr = coordStr.Substring(pos + 1, pos2 - pos - 1);
        retVal = double.TryParse(minStr, NumberStyles.Any, CultureInfo.InvariantCulture, out double minval);
        if((minval >= 60) ||  (minval < 0))
          retVal = false;
        if(retVal)
        {
          coord = deg + minval / 60;
          if (sign == 1)
            coord *= -1;
        }
      }
      return retVal;
    }

    private void _btnOk_Click(object sender, RoutedEventArgs e)
    {
      bool ok = parseLatitude(_ctlLat.getValue(), out double lat);
      if (!ok)
        MessageBox.Show(BatInspector.Properties.MyResources.LatitudeFormatError + _ctlLat.getValue(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
      else
      {
        ok = parseLongitude(_ctlLon.getValue(), out double lon);
        if (!ok)
          MessageBox.Show(BatInspector.Properties.MyResources.LongitudeFormatError + _ctlLon.getValue(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        else
        {
          this.Close();
          _model.Prj.createXmlInfoFiles(lat, lon);
        }
      }
    }

    private void _btnCancel_Click(object sender, RoutedEventArgs e)
    {
      this.Close();
    }
  }
}
