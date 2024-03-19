/********************************************************************************
 *               Author: Christian Müller
 *     Date of creation: 2021-08-10                                       
 *   Copyright (C) 2023: Christian Müller chrmue44(at)gmail(dot).com
 *
 *              Licence:  CC BY-NC 4.0 
 ********************************************************************************/

using BatInspector.Properties;
using libParser;
using System;
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
    private int _widthLbl;

    public FrmCreatePrj(ViewModel model)
    {
      InitializeComponent();
      _model = model;
      _info = new PrjInfo();
      _widthLbl = 200;
      _ctlPrjName.setup(MyResources.frmCreatePrjName, Controls.enDataType.STRING, 0, _widthLbl, true);
      _ctlLat.setup(MyResources.Latitude, Controls.enDataType.STRING, 0, _widthLbl, true);
      _ctlLat.setValue("49° 46.002 N");
      _ctlLon.setup(MyResources.Longitude, Controls.enDataType.STRING, 0, _widthLbl, true);
      _ctlLon.setValue("8° 38.032 E");
      _ctlSrcFolder.setup(MyResources.frmCreatePrjSrcFolder, _widthLbl, true, "", initDailogAfterSelectingSrc);
      _ctlDstFolder.setup(MyResources.frmCreatePrjDstFolder, _widthLbl, true);
      _ctlMaxFiles.setup(MyResources.frmCreatePrjMaxFiles, Controls.enDataType.INT, 0, _widthLbl, true);
      _ctlMaxFiles.setValue(1000);
      _ctlMaxFileLen.setup(MyResources.frmCreatePrjMaxFileLen, Controls.enDataType.DOUBLE, 1, _widthLbl, true);
      _ctlMaxFileLen.setValue(5.0);
      _ctlPrjWeather.setup(MyResources.frmCreatePrjWeather, Controls.enDataType.STRING, 0, _widthLbl, true);
      _ctlPrjWeather.setValue("");
      _ctlPrjLandscape.setup(MyResources.frmCreatePrjLandscape, Controls.enDataType.STRING, 0, _widthLbl, true);
      _ctlPrjLandscape.setValue("");
      _ctlGpxFile.setup(MyResources.frmCreatePrjSelectGpxFile, _widthLbl, false, "gpx Files (*.gpx)|*.gpx|All files(*.*)|*.*");
      _ctlGpxFile.IsEnabled = false;
      _rbGpxFile.IsChecked = false;
      _rbFixedPos.IsChecked = true;
      _cbOverwriteLoc.Visibility = Visibility.Hidden;
      setVisibilityTimeFilter();
    }

    public void init()
    {
      _ctlLat.setValue("");
      _ctlLon.setValue("");
      _cbEvalPrj.IsChecked = true;
    }



    private void setVisibilityTimeFilter()
    {
      Visibility vis = _cbTimeFilter.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
      _dtStart.Visibility = vis;
      _dtEnd.Visibility = vis;
      _lblDateStart.Visibility = vis;
      _lblDateEnd.Visibility = vis;
    }

    private void initDailogAfterSelectingSrc()
    {
      if (Directory.Exists(_ctlSrcFolder.getValue()))
      {
        string[] files = Directory.GetFiles(_ctlSrcFolder.getValue(), "*.bpr");
        // folder contains project file
        _cbTimeFilter.IsChecked = false;
        if (files != null && files.Length > 0)
        {
          _isProjectFolder = true;
          _info.Name = Path.GetFileNameWithoutExtension(files[0]);
          _ctlPrjName.setValue(_info.Name);
          _cbTimeFilter.IsEnabled = false;
        }
        else
        {
          _isProjectFolder = false;
          string[] kmlFiles = Directory.GetFiles(_ctlSrcFolder.getValue(), "*.kml");
          string[] gpxFiles = Directory.GetFiles(_ctlSrcFolder.getValue(), "*.gpx");
          string[] txtFiles = Directory.GetFiles(_ctlSrcFolder.getValue(), "*.txt");
          if (kmlFiles.Length > 0)
          {
            _ctlGpxFile.setValue(kmlFiles[0]);
            _rbKmlFile.IsChecked = true;
          }
          else if (gpxFiles.Length > 0)
          {
            _ctlGpxFile.setValue(gpxFiles[0]);
            _rbGpxFile.IsChecked = true;
          }
          else if (txtFiles.Length > 0)
          {
            _ctlGpxFile.setValue(txtFiles[0]);
            _rbTxtFile.IsChecked = true;
          }

          btnRadioClick(null, null);

          DirectoryInfo dir = new DirectoryInfo(_ctlSrcFolder.getValue());
          string prjName = dir.Name.Replace(',', '_').Replace(' ', '_');
          _ctlPrjName.setValue(prjName);

          _cbTimeFilter.IsEnabled = true;
          files = Directory.GetFiles(_ctlSrcFolder.getValue(), "*.wav");
          if (files != null && files.Length > 0)
          {
            if (_cbTimeFilter.IsChecked == true)
            {
              DateTime start = ElekonInfoFile.getDateTimeFromFileName(files[0]);
              DateTime end = start.Date;
              end = end.AddDays(1);
              end = end.AddHours(6);
              _dtStart.init(start);
              _dtEnd.init(end);
            }
            else
            {
              _dtStart.init(new DateTime(1950, 1, 1));
              _dtEnd.init(new DateTime(2099, 12, 31));
            }
          }
        }
        enableGuiElements();
      }
    }

    private void enableGuiElements()
    {
      _ctlPrjName.IsEnabled = !_isProjectFolder;
      _ctlPrjWeather.Visibility = !_isProjectFolder ? Visibility.Visible : Visibility.Hidden;
      _ctlPrjLandscape.Visibility = !_isProjectFolder ? Visibility.Visible : Visibility.Hidden;
      setVisibilityTimeFilter();
      _cbOverwriteLoc.Visibility = _isProjectFolder ? Visibility.Visible : Visibility.Hidden;
      _cbOverwriteLoc.IsChecked = !_isProjectFolder;
      enableLocationItems(!_isProjectFolder || (_cbOverwriteLoc.IsChecked == true));
    }

   
    private void _btnOk_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        _info.Name = _ctlPrjName.getValue();
        _info.SrcDir = _ctlSrcFolder.getValue();
        _info.DstDir = _ctlDstFolder.getValue();
        _info.MaxFileCnt = _ctlMaxFiles.getIntValue();
        _info.MaxFileLenSec = _ctlMaxFileLen.getDoubleValue();
        _info.OverwriteLocation = _cbOverwriteLoc.IsChecked == true;
        _info.GpxFile = (_rbGpxFile.IsChecked == true) || (_rbKmlFile.IsChecked == true)  || 
                        (_rbTxtFile.IsChecked == true) ? _ctlGpxFile.getValue() : "";
        _info.LocSourceGpx = _rbGpxFile.IsChecked == true;
        _info.LocSourceKml = _rbKmlFile.IsChecked == true;
        _info.LocSourceTxt = _rbTxtFile.IsChecked == true;
        _inspect = _cbEvalPrj.IsChecked == true;
        _info.IsProjectFolder = _isProjectFolder;
        bool ok = true;
        double lat = 0;
        double lon = 0;

        if (string.IsNullOrEmpty(_info.DstDir))
        {
          MessageBox.Show(BatInspector.Properties.MyResources.frmCreateDestFolderEmpty,
                          BatInspector.Properties.MyResources.Error,
                          MessageBoxButton.OK, MessageBoxImage.Error);
          ok = false;
        }
        else
        {
          if (!_info.LocSourceGpx && !_info.LocSourceKml && !_info.LocSourceTxt && _info.OverwriteLocation && !_isProjectFolder)
          {
            ok = Project.parseLatitude(_ctlLat.getValue(), out lat);
            if (!ok)
              MessageBox.Show(BatInspector.Properties.MyResources.LatitudeFormatError + _ctlLat.getValue(),
                              BatInspector.Properties.MyResources.Error,
                              MessageBoxButton.OK, MessageBoxImage.Error);
            else
            {
              ok = Project.parseLongitude(_ctlLon.getValue(), out lon);
              if (!ok)
                MessageBox.Show(BatInspector.Properties.MyResources.LongitudeFormatError + _ctlLon.getValue(),
                                BatInspector.Properties.MyResources.Error, 
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
          }
        }
        if (!ok)
          return;
        _model.Status.Msg = BatInspector.Properties.MyResources.FrmCreatePrjImportingProject;
        _model.Status.State = enAppState.IMPORT_PRJ;
        _info.Latitude = lat;
        _info.Longitude = lon;

        Thread thr = new Thread(createProject);
        if (!_isProjectFolder)
        {
          _info.Weather = _ctlPrjWeather.getValue();
          _info.Landscape = _ctlPrjLandscape.getValue();
          _info.GpxFile = (_rbGpxFile.IsChecked == true) || (_rbKmlFile.IsChecked == true)
            || (_rbTxtFile.IsChecked == true) ? _ctlGpxFile.getValue() : "";
          _info.StartTime = _dtStart.DateTime;
          _info.EndTime = _dtEnd.DateTime;
        }
        thr.Start();
        this.Visibility = Visibility.Hidden;
      }
      catch (Exception ex)
      {
        DebugLog.log("invalid project data, creation of project failed!: " + ex.ToString(), enLogType.ERROR);
      }
    }


    private void createProject()
    {
      try
      {
        _model.createProject(_info, _inspect);
      }
      catch(Exception ex)
      {
        DebugLog.log("error creating project: " + ex.ToString(), enLogType.ERROR);
        DebugLog.save();
      }
    }


    private void _btnCancel_Click(object sender, RoutedEventArgs e)
    {
      this.Visibility = Visibility.Hidden;
    }


    private void btnRadioClick(object sender, RoutedEventArgs e)
    {
      string oldVal = _ctlGpxFile.getValue();
      if(_rbGpxFile.IsChecked == true)
        _ctlGpxFile.setup(MyResources.frmCreatePrjSelectGpxFile, _widthLbl, false, "GPX Files (*.gpx)|*.gpx|All files(*.*)|*.*");
      if (_rbKmlFile.IsChecked == true)
        _ctlGpxFile.setup(MyResources.frmCreatePrjSelectKmlFile, _widthLbl, false, "KML Files (*.kml)|*.kml|All files(*.*)|*.*");
      if (_rbTxtFile.IsChecked == true)
        _ctlGpxFile.setup(MyResources.frmCreatePrjSelectTxtFile, _widthLbl, false, "TXT Files (*.txt)|*.txt|All files(*.*)|*.*");
      _ctlGpxFile.setValue(oldVal);
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
      _ctlGpxFile.IsEnabled = en & ((_rbGpxFile.IsChecked == true) | (_rbKmlFile.IsChecked == true));
      _ctlLat.IsEnabled = en & (_rbFixedPos.IsChecked == true);
      _ctlLon.IsEnabled = en & (_rbFixedPos.IsChecked == true);
      _rbGpxFile.IsEnabled = en;
      _rbKmlFile.IsEnabled = en;
      _rbFixedPos.IsEnabled = en;
    }

    private void _cbTimeFilter_Click(object sender, RoutedEventArgs e)
    {
      setVisibilityTimeFilter();
    }
  }
}
