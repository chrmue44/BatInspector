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
using libParser;
using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Interop;

namespace BatInspector.Forms
{
  /// <summary>
  /// Interaction logic for FrmCreateXmlInfos.xaml
  /// </summary>
  public partial class FrmCreatePrj : Window
  {
    private ViewModel _model;
    private PrjInfo _info;
    private bool _inspect;
    private bool _isProjectFolder = false;

    public FrmCreatePrj(ViewModel model)
    {
      InitializeComponent();
      _model = model;
      _info = new PrjInfo();
      int widthLbl = 200;
      _ctlPrjName.setup(MyResources.frmCreatePrjName, Controls.enDataType.STRING, 0, widthLbl, true);
      _ctlLat.setup(MyResources.Latitude, Controls.enDataType.STRING, 0, widthLbl, true);
      _ctlLat.setValue("49° 46.002 N");
      _ctlLon.setup(MyResources.Longitude, Controls.enDataType.STRING, 0, widthLbl, true);
      _ctlLon.setValue("8° 38.032 E");
      _ctlSrcFolder.setup(MyResources.frmCreatePrjSrcFolder, widthLbl, true, "", setupStartEndTime);
      _ctlDstFolder.setup(MyResources.frmCreatePrjDstFolder, widthLbl, true);
      _ctlMaxFiles.setup(MyResources.frmCreatePrjMaxFiles, Controls.enDataType.INT, 0, widthLbl, true);
      _ctlMaxFiles.setValue(500);
      _ctlMaxFileLen.setup(MyResources.frmCreatePrjMaxFileLen, Controls.enDataType.DOUBLE, 1, widthLbl, true);
      _ctlMaxFileLen.setValue(5.0);
      _ctlPrjWeather.setup(MyResources.frmCreatePrjWeather, Controls.enDataType.STRING, 0, widthLbl, true);
      _ctlPrjWeather.setValue("");
      _ctlPrjLandscape.setup(MyResources.frmCreatePrjLandscape, Controls.enDataType.STRING, 0, widthLbl, true);
      _ctlPrjLandscape.setValue("");
      _ctlGpxFile.setup(MyResources.frmCreatePrjSelectGpxFile, widthLbl, false, "gpx Files (*.gpx)|*.gpx|All files(*.*)|*.*");
      _ctlGpxFile.IsEnabled = false;
      _rbGpxFile.IsChecked = false;
      _rbFixedPos.IsChecked = true;
      _cbOverwriteLoc.Visibility = Visibility.Hidden;
    }


    public bool parseLatitude(string coordStr, out double coord)
    {
      return parseGeoCoord(coordStr, out coord, "N", "S", 90);
    }


    public bool parseLongitude(string coordStr, out double coord)
    {
      return parseGeoCoord(coordStr, out coord, "E", "W", 180);
    }



    private void setupStartEndTime()
    {
      string[] files = Directory.GetFiles(_ctlSrcFolder.getValue(), "*.bpr");
      // folder contains project file
      if (files != null && files.Length > 0)
      {
        _isProjectFolder = true;
        _info.Name = Path.GetFileNameWithoutExtension(files[0]);
        _ctlPrjName.setValue(_info.Name);
      }
      else
      {
        _isProjectFolder = false;
        files = Directory.GetFiles(_ctlSrcFolder.getValue(), "*.wav");
        if (files != null && files.Length > 0)
        {
          DateTime start = ElekonInfoFile.getDateTimeFromFileName(files[0]);
          DateTime end = start.Date;
          end = end.AddDays(1);
          end = end.AddHours(6);
          _dtStart.init(start);
          _dtEnd.init(end);
        }
      }
      enableGuiElements();
    }

    private void enableGuiElements()
    {
      _ctlPrjName.IsEnabled = !_isProjectFolder;
      _ctlPrjWeather.Visibility = !_isProjectFolder ? Visibility.Visible : Visibility.Hidden;
      _ctlPrjLandscape.Visibility = !_isProjectFolder ? Visibility.Visible : Visibility.Hidden;
      _dtStart.Visibility = !_isProjectFolder ? Visibility.Visible : Visibility.Hidden;
      _dtEnd.Visibility = !_isProjectFolder ? Visibility.Visible : Visibility.Hidden;

      _lblDateStart.Visibility = !_isProjectFolder ? Visibility.Visible : Visibility.Hidden;
      _lblDateEnd.Visibility = !_isProjectFolder ? Visibility.Visible : Visibility.Hidden;
      _cbOverwriteLoc.Visibility = _isProjectFolder ? Visibility.Visible : Visibility.Hidden;
      enableLocationItems(!_isProjectFolder || (_cbOverwriteLoc.IsChecked == true));
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
      bool ok = double.TryParse(coordStr, NumberStyles.Any, CultureInfo.InvariantCulture, out coord);
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
      _info.Name = _ctlPrjName.getValue();
      _info.SrcDir = _ctlSrcFolder.getValue();
      _info.DstDir = _ctlDstFolder.getValue();
      _info.MaxFileCnt = _ctlMaxFiles.getIntValue();
      _info.MaxFileLenSec = _ctlMaxFileLen.getDoubleValue();
      _info.OverwriteLocation = _cbOverwriteLoc.IsChecked == true;
      _info.GpxFile = _rbGpxFile.IsChecked == true ? _ctlGpxFile.getValue() : "";
      _info.LocSourceGpx = _rbGpxFile.IsChecked == true;
      _inspect = _cbEvalPrj.IsChecked == true;
      _info.IsProjectFolder = _isProjectFolder;
      bool ok = false;
      double lat = 0;
      double lon = 0;
      if ((!_info.LocSourceGpx && _info.OverwriteLocation) || !_isProjectFolder)
      {
        ok = parseLatitude(_ctlLat.getValue(), out lat);
        if (!ok)
          MessageBox.Show(BatInspector.Properties.MyResources.LatitudeFormatError + _ctlLat.getValue(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        else
        {
          ok = parseLongitude(_ctlLon.getValue(), out lon);
          if (!ok)
            MessageBox.Show(BatInspector.Properties.MyResources.LongitudeFormatError + _ctlLon.getValue(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
      }
      if (!ok)
        return;
      _info.Latitude = lat;
      _info.Longitude = lon;

      if (_isProjectFolder)
      {
        this.Visibility = Visibility.Hidden;
        Thread thr = new Thread(splitProject);
        thr.Start();
      }
      else
      {
        try
        {
          this.Visibility = Visibility.Hidden;
          _info.Weather = _ctlPrjWeather.getValue();
          _info.Landscape = _ctlPrjLandscape.getValue();
          _info.GpxFile = _rbGpxFile.IsChecked == true ? _ctlGpxFile.getValue() : "";
          _info.StartTime = _dtStart.DateTime;
          _info.EndTime = _dtEnd.DateTime;
          Thread thr = new Thread(createProject);
          thr.Start();
        }
        catch (Exception ex)
        {
          DebugLog.log("invalid project data, creation of project failed!: " + ex.ToString(), enLogType.ERROR);
        }
      }
    }

    private void splitProject()
    {
      string[] projects = Project.splitPrj(_info, _model.Regions, _model.SpeciesInfos);
      if (_inspect)
      {
        foreach (string prj in projects)
        {
          string prjPath = _info.DstDir + "/" + prj;
          DirectoryInfo dir = new DirectoryInfo(prjPath);
          _model.initProject(dir);
          _model.evaluate();
        }
      }
    }

    private void createProject()
    {
      string[] projects = Project.createPrj(_info, _model.Regions, _model.SpeciesInfos);
      if (_inspect)
      {
        foreach (string prj in projects)
        {
          string prjPath = _info.DstDir + "/" + prj;
          DirectoryInfo dir = new DirectoryInfo(prjPath);
          _model.initProject(dir);
          _model.evaluate();
        }
      }
    }


    private void _btnCancel_Click(object sender, RoutedEventArgs e)
    {
      this.Visibility = Visibility.Hidden;
    }


    private void btnRadioClick(object sender, RoutedEventArgs e)
    {
      _ctlGpxFile.IsEnabled = _rbFixedPos.IsChecked == false;
      _ctlLat.IsEnabled = _rbFixedPos.IsChecked == true;
      _ctlLon.IsEnabled = _rbFixedPos.IsChecked == true;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      winUtils.hideCloseButton(new WindowInteropHelper(this).Handle);
    }

    private void _cbOverwriteLoc_Click(object sender, RoutedEventArgs e)
    {
      enableLocationItems(_cbOverwriteLoc.IsChecked == true);
    }

    private void enableLocationItems(bool en)
    {
      _ctlGpxFile.IsEnabled = en & (_rbGpxFile.IsChecked == true);
      _ctlLat.IsEnabled = en & (_rbFixedPos.IsChecked == true);
      _ctlLon.IsEnabled = en & (_rbFixedPos.IsChecked == true);
      _rbGpxFile.IsEnabled = en;
      _rbFixedPos.IsEnabled = en;
    }
  }
}
