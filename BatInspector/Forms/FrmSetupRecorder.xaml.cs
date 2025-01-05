/********************************************************************************
 *               Author: Christian Müller
 *     Date of creation: 2023-09-01                                       
 *   Copyright (C) 2023: Christian Müller chrmue44(at)gmail(dot).com
 *
 *              Licence:  CC BY-NC 4.0 
 ********************************************************************************/

using System;
using System.Windows;
using System.Windows.Interop;
using BatInspector.Controls;
using libParser;
using System.Windows.Media;
using System.Collections.Generic;
using System.Windows.Controls;

namespace BatInspector.Forms
{
  /// <summary>
  /// Interaction logic for FrmSetupRecorder.xaml
  /// </summary>
  public partial class FrmSetupRecorder : Window
  {
    CtrlRecorder _rec;
    System.Windows.Threading.DispatcherTimer _timer;
    int _tickCnt = 0;
    bool _firmwareUpdate = false;

    public FrmSetupRecorder(CtrlRecorder recorder)
    {
      InitializeComponent();
      _rec = recorder;
      int wl = 90;
      _tbStatus.Foreground = System.Windows.Media.Brushes.Red;
      _ctlSwVersion.setup(BatInspector.Properties.MyResources.FrmRecSoftwareVersion, enDataType.STRING, 0, wl);
      _ctlSwVersion.setValue("");
      _ctlSerial.setup(BatInspector.Properties.MyResources.FrmRecSerialNr, enDataType.STRING, 0, wl);
      _ctlSerial.setValue("");
      _ctlMicInfo.setup(Properties.MyResources.Microphone, enDataType.STRING, 0, wl);
      _ctlMicInfo.setValue("");
      initParameterTab(150, 140);
      initStatusTab(180, 150);
      initSystemTab(130, 150);
      _timer = new System.Windows.Threading.DispatcherTimer();
      _timer.Tick += new EventHandler(timer_Tick);
      _timer.Interval = new TimeSpan(0, 0, 0, 0, 1000);
      _timer.Start();
      _cLog.CheckBoxesVisible = false;
      _cLog.setup(execCmd, true);
      enableButtons(false);
    }

    private void enableButtons(bool on)
    {
      _btnFwUpdate.IsEnabled = on;
      _btnRead.IsEnabled = on;
      _btnWrite.IsEnabled = on;
      _btnSetTime.IsEnabled = on;
      _btnSet.IsEnabled = on;
    }


    private void timer_Tick(object sender, EventArgs e)
    {
      if (_rec.IsConnected && !_firmwareUpdate)
        updateStatusControls();
      _tickCnt++;
    }


    private void initParameterControls()
    {
      _ctlDeadTime.setValue(_rec.Control.DeadTime.Value);
      _ctlDeadTime.IsEnabled = true;
      _ctlGain.setItems(_rec.Acquisition.Gain.Items);
      _ctlGain.IsEnabled = true;
      _ctlGain.SelectIndex = (int)_rec.Acquisition.Gain.Value;
      _ctlPreTrigger.setValue(_rec.Acquisition.PreTrigger.Value);
      _ctlPreTrigger.IsEnabled = true;
      _ctlRecStartH.setValue((int)_rec.Control.StartH.Value);
      _ctlRecStartH.IsEnabled = true;
      _ctlRecStartMin.setValue((int)_rec.Control.StartMin.Value);
      _ctlRecStartMin.IsEnabled = true;
      _ctlRecStopH.setValue((int)_rec.Control.StopH.Value);
      _ctlRecStopH.IsEnabled = true;
      _ctlRecStopMin.setValue((int)_rec.Control.StopMin.Value);
      _ctlRecStopMin.IsEnabled = true;
      _ctlSampleRate.setItems(_rec.Acquisition.SampleRate.Items);
      _ctlSampleRate.SelectIndex = (int)_rec.Acquisition.SampleRate.Value;
      _ctlSampleRate.IsEnabled = true;
      _ctlTrigFiltType.setItems(_rec.Trigger.Filter.Items);
      _ctlTrigFiltType.SelectIndex = _rec.Trigger.Filter.Value;
      _ctlTrigFiltType.IsEnabled = true;
      _ctlTrigFreq.setValue(_rec.Trigger.Frequency.Value);
      _ctlTrigFreq.IsEnabled = true;
      _ctlTrigLength.setValue(_rec.Trigger.EventLength.Value);
      _ctlTrigLength.IsEnabled = true;
      _ctlTrigLevel.setValue(_rec.Trigger.Level.Value);
      _ctlTrigLevel.IsEnabled = true;
      _ctlRecordingTime.setValue(_rec.Control.RecTime.Value);
      _ctlRecordingTime.IsEnabled = true;
      _ctlRecFilterFreq.setValue(_rec.Acquisition.RecordingFilter.Value);
      _ctlRecFilterFreq.IsEnabled = true;
      _ctlRecFilterType.setItems(_rec.Acquisition.RecFiltType.Items);
      _ctlRecFilterType.SelectIndex = _rec.Acquisition.RecFiltType.Value;
      _ctlRecFilterType.IsEnabled = true;
      _ctlRecMode.setItems(_rec.Control.Mode.Items);
      _ctlRecMode.SelectIndex = (int)_rec.Control.Mode.Value;
      _ctlRecMode.IsEnabled = true;
      _ctlTrigType.setItems(_rec.Trigger.Type.Items);
      _ctlTrigType.SelectIndex = (int)_rec.Trigger.Type.Value;
      _ctlTrigType.IsEnabled = true;
      _ctlLanguage.setItems(_rec.General.Language.Items);
      _ctlLanguage.IsEnabled = true;
      _ctlLanguage.SelectIndex = _rec.General.Language.Value;
      _ctlBackLight.setValue((int)_rec.General.BackLightTime.Value);
      _ctlBackLight.IsEnabled = true;
      _ctlDisplayMode.setItems(_rec.General.DisplayMode.Items);
      _ctlDisplayMode.SelectIndex = _rec.General.DisplayMode.Value;
      _ctlDisplayMode.IsEnabled = true;
      _ctlPosMode.setItems(_rec.General.PositionMode.Items);
      _ctlPosMode.IsEnabled = true;
      _ctlPosMode.SelectIndex = _rec.General.PositionMode.Value;
      _ctlLat.setValue(Utils.LatitudeToString(_rec.General.Latitude.Value));
      _ctlLat.IsEnabled = true;
      _ctlLon.setValue(Utils.LongitudeToString(_rec.General.Longitude.Value));
      _ctlLon.IsEnabled = true;
      enableButtons(true);
    }


    private void initSystemTab(int wl, int wt)
    {
      _ctlPassWd.setup("Password", enDataType.STRING, 0, wl);
      _ctlPassWd.setValue("");
      _ctlSetSerial.setup("Set Serial Number", enDataType.STRING, 0, wl);
      _ctlSetSerial.setValue("");
      _ctlSetVoltFact.setup("Set Voltage", enDataType.DOUBLE, 3, wl);
      _ctlSetVoltFact.setValue(0.0);
      _ctlMicComment.setup("Comment", enDataType.STRING, 0, wl);
      _ctlMicId.setup("Id", enDataType.STRING, 0, wl);
      _ctlMicType.setup("Type", enDataType.STRING, 0, wl);
      _ctlMicFreqFile.setup(Properties.MyResources.FrequencyResponse, wl, false, "Text files(*.txt)|*.txt", showMicFreqResponse);
      _ctlMicFreqFile.IsEnabled = false;
      createFreqResponseGraph(_cnv, _cnv.Height, _cnv.Width);
    }


    private void initParameterTab(int wl, int wt)
    {
      _ctlDeadTime.setup(BatInspector.Properties.MyResources.DeadTime, enDataType.DOUBLE, 2, wl, false, setDeadTime);
      _ctlGain.setup(BatInspector.Properties.MyResources.CtlZoomGain, 1, wl, wt, setGain, null, "", false);
      _ctlPreTrigger.setup("Pre Trigger" + " [ms]", enDataType.DOUBLE, 2, wl, false, setPreTrigger);
      _ctlRecStartH.setup(BatInspector.Properties.MyResources.frmRecStartingTimeH, enDataType.INT, 0, wl, false, setStartH);
      _ctlRecStartMin.setup(BatInspector.Properties.MyResources.frmRecStartingTimeMin, enDataType.INT, 0, wl, false, setStartMin);
      _ctlRecStopH.setup(BatInspector.Properties.MyResources.frmRecStopTimeH, enDataType.INT, 0, wl, false, setStopH);
      _ctlRecStopMin.setup(BatInspector.Properties.MyResources.frmRecStopTimeMin, enDataType.INT, 0, wl, false, setStopMin);
      _ctlSampleRate.setup(BatInspector.Properties.MyResources.SamplingRate, 2, wl, wt, setSampleRate);
      _ctlTrigFreq.setup(BatInspector.Properties.MyResources.Frequency + " [kHz]", enDataType.DOUBLE, 1, wl, false, setTrigFrequency);
      _ctlTrigFiltType.setup(BatInspector.Properties.MyResources.frmRecFilterType, 0, wl, wt, setFilterType, null, "", false);
      _ctlTrigLength.setup(BatInspector.Properties.MyResources.FrmRecMinEventLength + " [ms]", enDataType.DOUBLE, 1, wl, false, setTrigLength);
      _ctlTrigLevel.setup(BatInspector.Properties.MyResources.frmRecLevel + " [dB]", enDataType.DOUBLE, 1, wl, false, setTrigLevel);
      _ctlRecordingTime.setup(BatInspector.Properties.MyResources.frmRecRecordingTime, enDataType.DOUBLE, 1, wl, false, setRecTime);
      _ctlRecFilterType.setup(BatInspector.Properties.MyResources.frmRecFilterType, 0, wl, wt, setRecFilterType, null, "", false);
      _ctlRecFilterFreq.setup(BatInspector.Properties.MyResources.Frequency + " [kHz]", enDataType.DOUBLE, 1, wl, false, setRecFrequency);
      _ctlRecMode.setup(BatInspector.Properties.MyResources.FrmRecRecordingMode, 3, wl, wt, setRecMode, null, "", false);
      _ctlTrigType.setup(BatInspector.Properties.MyResources.FrmRecTriggerType, 4, wl, wt, setTrigType, null, "", false);
      _ctlLanguage.setup(BatInspector.Properties.MyResources.Language, 0, wl, wt, setLanguage, null, "", false);
      _ctlBackLight.setup(BatInspector.Properties.MyResources.FrmRecBacklightTime, enDataType.INT, 0, wl, true, setBacklightTime);
      _ctlDisplayMode.setup(BatInspector.Properties.MyResources.FrmSetupRecorderBrightness, 0, wl, wt, setDisplayMode);
      _ctlPosMode.setup(BatInspector.Properties.MyResources.FrmRecModePosition, 0, wl, wt, setPositionMode, null, "", false);
      _ctlLat.setup(BatInspector.Properties.MyResources.Latitude, enDataType.STRING, 5, wl, false, setLat);
      _ctlLon.setup(BatInspector.Properties.MyResources.Longitude, enDataType.STRING, 5, wl, false, setLon);
    }

    private void setStopMin(enDataType type, object val)
    {
      _rec.Control.StopMin.Value = _ctlRecStopMin.getIntValue();
    }

    private void setStopH(enDataType type, object val)
    {
      _rec.Control.StopH.Value = _ctlRecStopH.getIntValue();
    }

    private void setStartMin(enDataType type, object val)
    {
      _rec.Control.StartMin.Value = _ctlRecStartMin.getIntValue();
    }

    private void setStartH(enDataType type, object val)
    {
      _rec.Control.StartH.Value = _ctlRecStartH.getIntValue();
    }

    private void setLanguage(int index, string val)
    {
      _rec.General.Language.Value = _ctlLanguage.getSelectedIndex();
    }

    private void setBacklightTime(enDataType type, object val)
    {
      _rec.General.BackLightTime.Value = _ctlBackLight.getIntValue();
    }
    private void setDisplayMode(int idx, string val)
    {
      _rec.General.DisplayMode.Value = _ctlDisplayMode.getSelectedIndex();
    }

    private void setPositionMode(int index, string val)
    {
      _rec.General.PositionMode.Value = _ctlPosMode.getSelectedIndex();
    }

    private void setFilterType(int index, string val)
    {
      _rec.Trigger.Filter.Value = _ctlTrigFiltType.getSelectedIndex();
    }

    private void setRecFilterType(int index, string val)
    {
      _rec.Acquisition.RecFiltType.Value = _ctlRecFilterType.getSelectedIndex();
    }

    private void setLon(enDataType type, object val)
    {
      double lon;
      bool ok = Project.parseLongitude(_ctlLon.getValue(), out lon);
      if (ok)
        _rec.General.Longitude.Value = lon;
      else
        MessageBox.Show(BatInspector.Properties.MyResources.LongitudeFormatError + _ctlLat.getValue(),
                  BatInspector.Properties.MyResources.Error,
                  MessageBoxButton.OK, MessageBoxImage.Error);
    }

    private void setLat(enDataType type, object val)
    {
      double lat;
      bool ok = Project.parseLatitude(_ctlLat.getValue(), out lat);
      if (ok)
        _rec.General.Latitude.Value = lat;
      else
        MessageBox.Show(BatInspector.Properties.MyResources.LatitudeFormatError + _ctlLat.getValue(),
                  BatInspector.Properties.MyResources.Error,
                  MessageBoxButton.OK, MessageBoxImage.Error);
    }

    private void setTrigType(int index, string val)
    {
      _rec.Trigger.Type.Value = _ctlTrigType.getSelectedIndex();
    }

    private void setRecMode(int index, string val)
    {
      _rec.Control.Mode.Value = _ctlRecMode.getSelectedIndex();
    }

    private void setRecTime(enDataType type, object val)
    {
      _rec.Control.RecTime.Value = _ctlRecordingTime.getDoubleValue();
    }

    private void setTrigLevel(enDataType type, object val)
    {
      _rec.Trigger.Level.Value = _ctlTrigLevel.getDoubleValue();
    }

    private void setTrigLength(enDataType type, object val)
    {
      _rec.Trigger.EventLength.Value = _ctlTrigLength.getDoubleValue();
    }

    private void setTrigFrequency(enDataType type, object val)
    {
      _rec.Trigger.Frequency.Value = _ctlTrigFreq.getDoubleValue();
    }

    private void setRecFrequency(enDataType type, object val)
    {
      _rec.Acquisition.RecordingFilter.Value = _ctlRecFilterFreq.getDoubleValue();
    }

    private void setSampleRate(int index, string val)
    {
      _rec.Acquisition.SampleRate.Value = _ctlSampleRate.getSelectedIndex();
    }

    private void setPreTrigger(enDataType type, object val)
    {
      _rec.Acquisition.PreTrigger.Value = _ctlPreTrigger.getDoubleValue();
    }

    private void setDeadTime(enDataType type, object val)
    {
      _rec.Control.DeadTime.Value = _ctlDeadTime.getDoubleValue();
    }

    private void setGain(int index, string val)
    {
      _rec.Acquisition.Gain.Value = _ctlGain.getSelectedIndex();
      ;
    }

    private void initStatusTab(int wl, int wt)
    {
      _ctlBatVoltage.setup(BatInspector.Properties.MyResources.FrmRecBatteryVoltageV, enDataType.DOUBLE, 2, wl, false);
      _ctlBatVoltage.setValue(0.0);
      _ctlBatCapacity.setup(BatInspector.Properties.MyResources.FrmRecBatteryCapacity, enDataType.DOUBLE, 0, wl, false);
      _ctlBatCapacity.setValue(0.0);
      _ctlHumidity.setup(BatInspector.Properties.MyResources.FrmRecHumidity, enDataType.DOUBLE, 1, wl, false);
      _ctlHumidity.setValue(0.0);
      _ctlTemperature.setup(BatInspector.Properties.MyResources.FrmRecTemperature, enDataType.DOUBLE, 1, wl, false);
      _ctlTemperature.setValue(0.0);
      _ctlAudioBlocks.setup(BatInspector.Properties.MyResources.frmRecNrOfUsedAudioblocks, enDataType.INT, 0, wl, false);
      _ctlAudioBlocks.setValue(0);
      _ctlAudioLoadAvg.setup(BatInspector.Properties.MyResources.FrmRecAudioLoadAvg, enDataType.DOUBLE, 2, wl, false);
      _ctlAudioLoadAvg.setValue(0.0);
      _ctlAudioLoadMax.setup(BatInspector.Properties.MyResources.frmRecAudioLoadMax, enDataType.DOUBLE, 2, wl, false);
      _ctlAudioLoadMax.setValue(0.0);
      _ctlDiskFree.setup(BatInspector.Properties.MyResources.frmRecFreeDiskSpaceMB, enDataType.DOUBLE, 1, wl, false);
      _ctlDiskFree.setValue(0.0);
      _ctlLocation.setup(BatInspector.Properties.MyResources.FrmCreatePrjLLocation, enDataType.STRING, 0, wl, false);
      _ctlLocation.setValue("");
      _ctlGpsStatus.setup("GPS Status", 0, wl, wt, null, null, "", false);
      _ctlGpsStatus.setValue("");
      _ctlNrSatellites.setup(BatInspector.Properties.MyResources.frmRecNrOfSatellites, enDataType.INT, 0, wl, false);
      _ctlNrSatellites.setValue(0);
      _ctlHeight.setup(BatInspector.Properties.MyResources.Height, enDataType.DOUBLE, 1, wl, false);
      _ctlHeight.setValue(0.0);
      _ctlMainLoop.setup(BatInspector.Properties.MyResources.frmRecMainLoop, enDataType.INT, 0, wl, false);
      _ctlMainLoop.setValue(0);
      _ctlRecCount.setup(BatInspector.Properties.MyResources.frmRecNrRecordings, enDataType.INT, 0, wl, false);
      _ctlRecCount.setValue(0);
      _ctlRecState.setup(BatInspector.Properties.MyResources.Status, 0, wl, wt, null, null, "", false);
      _ctlDate.setup(BatInspector.Properties.MyResources.Date, enDataType.STRING, 0, wl, false);
      _ctlDate.setValue("");
      _ctlTime.setup(BatInspector.Properties.MyResources.Time, enDataType.STRING, 0, wl, false);
      _ctlTime.setValue("");
    }

    void initSystemControls(string serial)
    {
      _ctlPassWd.IsEnabled = true;
      _ctlSetVoltFact.IsEnabled = true;
      _ctlSetSerial.IsEnabled = true;
      _ctlSetSerial.setValue(serial);
      _ctlMicFreqFile.IsEnabled = true;
      _ctlMicId.IsEnabled = true;
      _ctlMicInfo.IsEnabled = true;
      _ctlMicType.IsEnabled = true;
      _ctlMicComment.IsEnabled = true;
      readFreqResponse();
    }

    void initStatusControls()
    {
      _ctlRecState.setItems(_rec.Status.RecordingStatus.Items);
      _ctlGpsStatus.setItems(_rec.Status.Gps.Items);
    }

    private void updateStatusControls()
    {
      if ((_tickCnt % 10) == 0)
      {
        _ctlBatVoltage.setValue(_rec.Status.BattVoltage.Value);
        _ctlBatCapacity.setValue(_rec.Status.ChargeLevel.Value);
        _ctlAudioBlocks.setValue((int)_rec.Status.AudioBlocks.Value);
        _ctlAudioLoadAvg.setValue(_rec.Status.CpuLoadAvg.Value);
        _ctlAudioLoadMax.setValue(_rec.Status.CpuLoadMax.Value);
        _ctlDiskFree.setValue(_rec.Status.DiskSpace.Value);
        _ctlDate.setValue(_rec.Status.Date.Value);
        _ctlLocation.setValue(_rec.Status.Location.Value);
        _ctlHeight.setValue(_rec.Status.Height.Value);
        _ctlHumidity.setValue(_rec.Status.Humidity.Value);
        _ctlTemperature.setValue(_rec.Status.Temperature.Value);
        _ctlNrSatellites.setValue((int)_rec.Status.NrSatellites.Value);
      }
      _ctlMainLoop.setValue((int)_rec.Status.MainLoop.Value);
      _ctlRecCount.setValue((int)_rec.Status.RecCount.Value);
      _ctlTime.setValue(_rec.Status.Time.Value);
      _ctlRecState.SelectIndex = _rec.Status.RecordingStatus.Value;
      _ctlGpsStatus.SelectIndex = _rec.Status.Gps.Value;

      if (_cbUpdateFft.IsChecked == true)
        _img.Source = _rec.Status.getLiveFft();
    }

    private void _btnRead_Click(object sender, RoutedEventArgs e)
    {
      _rec.loadPars();
      initParameterControls();
    }

    private void _btnWrite_Click(object sender, RoutedEventArgs e)
    {
      _rec.savePars();
      DebugLog.log("Parameters written to EEPROM", enLogType.INFO);
    }

    private void _btnCancel_Click(object sender, RoutedEventArgs e)
    {
      _rec.disConnect();
      this.Close();
    }

    private void _btnConnect_Click(object sender, RoutedEventArgs e)
    {
      bool res = _rec.connect(out string version, out string serialNr);
      if (res)
      {
        _rec.getMicInfos(out string id, out string type, out string comment);
        _ctlSwVersion.setValue(version);
        _ctlSerial.setValue(serialNr);
        _ctlMicInfo.setValue(id);
        _ctlMicId.setValue(id);
        _ctlMicType.setValue(type);
        _ctlMicComment.setValue(comment);
        _tbStatus.Text = BatInspector.Properties.MyResources.FrmRecConnected;
        _tbStatus.Foreground = System.Windows.Media.Brushes.Green;
        initParameterControls();
        initStatusControls();
        initSystemControls(serialNr);
      }
    }

    private void _btnSetTime_Click(object sender, RoutedEventArgs e)
    {
      _rec.Status.setTime();
      DebugLog.log("Current time sent to device", enLogType.INFO);
    }

    private void _btnFwUpdate_Click(object sender, RoutedEventArgs e)
    {
      System.Windows.Forms.OpenFileDialog dlg = new System.Windows.Forms.OpenFileDialog();
      string filter = "firmware files(*.hex)|*.hex";
      dlg.Filter = filter;
      System.Windows.Forms.DialogResult res = dlg.ShowDialog();
      if (res == System.Windows.Forms.DialogResult.OK)
      {
        _firmwareUpdate = true;
        BatSpy.uploadFirmware(dlg.FileName);
        _rec.disConnect();
        this.Close();
      }
    }

    private void execCmd(string cmd, dlgVoid callBack)
    {
      if (BatSpy.IsConnected)
      {
        string result = BatSpy.ExecuteCommand(cmd);
        _cLog.addTextLine(cmd, Brushes.Blue);
        _cLog.addTextLine(result.Replace("\n", " ").Replace("\r", ""), Brushes.Black);
        _cLog.activateCmd();
      }
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      winUtils.hideCloseButton(new WindowInteropHelper(this).Handle);
    }

    private void btnSet_Clicked(object sender, RoutedEventArgs e)
    {
      string passWd = _ctlPassWd.getValue();
      string newSerial = _ctlSetSerial.getValue();
      double voltage = _ctlSetVoltFact.getDoubleValue();
      bool ok = _rec.setSystemSettings(passWd, newSerial, voltage);

      string id = _ctlMicId.getValue();
      string type = _ctlMicType.getValue();
      string comment = _ctlMicComment.getValue();
      string freqRespFile = _ctlMicFreqFile.getValue();

      ok = _rec.setMicSettings(passWd, id, type, comment, freqRespFile);
      if (!ok)
        DebugLog.log("unable to set system parameters", enLogType.ERROR);
    }

    private void createFreqResponseGraph(Canvas c, double h, double w)
    {
      c.Children.Clear();
      GraphHelper.createRulerY(c, 20, 5, h-20, -10, 10, 4);
      double[] ticksY = { -5, 0, 5, 10 };
      GraphHelper.createGridY(c, 20, 5, w, h - 20,  -10, 10, ticksY);
      double[] ticksX = { 100, 1000, 10000, 100000 };
      GraphHelper.createRulerLogX(c, 20, h-20, w, 80, 200000, ticksX);
      GraphHelper.createText(c, -13, h - 40, "[dB]", Brushes.Black);
      double[] ticksGridX = { 100, 200, 300, 400, 500,600, 700, 800, 900,
                             1000, 2000, 3000, 4000, 5000, 6000, 7000, 8000, 9000, 10000,
                             20000, 30000, 40000, 50000, 60000, 70000, 80000, 90000, 100000, 200000 };
      GraphHelper.createGridLogX(c, 20, 5, w, h - 20, 80, 200000, ticksGridX);
      GraphHelper.createText(c, 50, h - 5, "[Hz]", Brushes.Black);
    }


    private void showMicFreqResponse()
    {
      string fileName = _ctlMicFreqFile.getValue();
      createFreqResponseGraph(_cnv, _cnv.Height, _cnv.Width);
      List<MicFreqItem> l = _rec.readFreqResponseFromFile(fileName);
      Point[] points = new Point[l.Count];
      for(int i = 0; i <  l.Count; i++) 
        points[i] = new Point(l[i].Frequency, l[i].Amplitude);
      GraphHelper.showGraphLog(_cnv, 20, 7, _cnv.Width - 20, _cnv.Height - 25, 80, 200000, -10, 10, points);
    }

    private void readFreqResponse()
    {
      createFreqResponseGraph(_cnv, _cnv.Height, _cnv.Width);
      List<MicFreqItem> l = _rec.readFreqResponseFromMic();
      Point[] points = new Point[l.Count];
      for (int i = 0; i < l.Count; i++)
        points[i] = new Point(l[i].Frequency, l[i].Amplitude);
      GraphHelper.showGraphLog(_cnv, 20, 7, _cnv.Width - 20, _cnv.Height - 25, 80, 200000, -10, 10, points);
    }
  }
}
