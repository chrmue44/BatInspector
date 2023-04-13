/********************************************************************************
 *               Author: Christian Müller
 *      Date of cration: 2021-08-10                                       
 *   Copyright (C) 2022: Christian Müller christian(at)chrmue(dot).de
 *
 *              Licence:
 * 
 * THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND,
 * EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
 ********************************************************************************/

using BatInspector.Properties;
using System.Globalization;
using System.Threading;
using System.Windows;


namespace BatInspector.Forms
{
  /// <summary>
  /// Interaction logic for FrmCreateXmlInfos.xaml
  /// </summary>
  public partial class FrmCreatePrj : Window
  {
    private ViewModel _model;
    private PrjInfo _info;
    public FrmCreatePrj(ViewModel model)
    {
      InitializeComponent();
      _model = model;
      _info = new PrjInfo();
      int widthLbl = 200;
      _ctlPrjName.setup(MyResources.frmCreatePrjName, Controls.enDataType.STRING, 0, widthLbl, 150, true);
      _ctlLat.setup(MyResources.Latitude, Controls.enDataType.STRING, 0, widthLbl, 120, true);
      _ctlLon.setup(MyResources.Longitude, Controls.enDataType.STRING, 0, widthLbl, 120, true);
      _ctlSrcFolder.setup(MyResources.frmCreatePrjSrcFolder, widthLbl, true);
      _ctlDstFolder.setup(MyResources.frmCreatePrjDstFolder, widthLbl, true);
      _ctlMaxFiles.setup(MyResources.frmCreatePrjMaxFiles, Controls.enDataType.INT, 0, widthLbl, 80, true);
      _ctlMaxFiles.setValue(500);
      _ctlMaxFileLen.setup(MyResources.frmCreatePrjMaxFileLen, Controls.enDataType.DOUBLE, 1, widthLbl, 80, true);
      _ctlMaxFileLen.setValue(5.0);
      _ctlPrjWeather.setup(MyResources.frmCreatePrjWeather, Controls.enDataType.STRING, 0, widthLbl, 200, true);
      _ctlPrjLandscape.setup(MyResources.frmCreatePrjLandscape, Controls.enDataType.STRING,0, widthLbl, 200, true);
    }


    public bool parseLatitude(string coordStr, out double coord)
    {
      return parseGeoCoord(coordStr, out coord, "N", "S", 90);
    }

    public bool parseLongitude(string coordStr, out double coord)
    {
      return parseGeoCoord(coordStr, out coord, "E", "W", 180);
    }

    /// <summary>
    /// parse geographical coordinates, two formats are allowed:
    /// 1.: plain double e.g. 49.657489, -33.5679864
    /// 2.: degrees and minutes e.g. "49° 38.012 N", "8° 37.443 W"
    /// </summary>
    /// <param name="coordStr">coordinat as string</param>
    /// <param name="coord">output coordinate</param>
    /// <param name="hem1">hemesphere character positive direction</param>
    /// <param name="hem2">hemisphere character negative direction</param>
    /// <param name="maxDeg">max value for degrees</param>
    /// <returns></returns>
    bool parseGeoCoord(string coordStr, out double coord, string hem1, string hem2, int maxDeg)
    {
      bool retVal = true;
      bool ok = double.TryParse(coordStr,  NumberStyles.Any, CultureInfo.InvariantCulture, out coord);
      if (ok)
      {
        if ((coord < -maxDeg) || (coord > maxDeg))
          retVal = false;
      }
      else
      {
        int deg = 0;
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

        if (retVal)
        {
          string minStr = coordStr.Substring(pos + 1, pos2 - pos - 1);
          retVal = double.TryParse(minStr, NumberStyles.Any, CultureInfo.InvariantCulture, out double minval);
          if ((minval >= 60) || (minval < 0))
            retVal = false;
          if (retVal)
          {
            coord = deg + minval / 60;
            if (sign == 1)
              coord *= -1;
          }
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
          _info.Name = _ctlPrjName.getValue();
          _info.SrcDir = _ctlSrcFolder.getValue();
          _info.DstDir = _ctlDstFolder.getValue();
          _info.MaxFileCnt = _ctlMaxFiles.getIntValue();
          _info.MaxFileLenSec = _ctlMaxFileLen.getDoubleValue();
          _info.Weather = _ctlPrjWeather.getValue();
          _info.Landscape = _ctlPrjLandscape.getValue();
          _info.Latitude = lat;
          _info.Longitude = lon;
          Thread thr = new Thread(createProject);
          thr.Start();
        }
      }
    }

    private void createProject()
    {
      Project.createPrj(_info, _model.Regions, _model.SpeciesInfos);
    }

    private void _btnCancel_Click(object sender, RoutedEventArgs e)
    {
      this.Close();
    }
  }
}
