/********************************************************************************
 *               Author: Christian Müller
 *     Date of creation: 2023-09-01                                       
 *   Copyright (C) 2023: Christian Müller chrmue44(at)gmail(dot).com
 *
 *              Licence:  CC BY-NC 4.0 
 ********************************************************************************/

using System.Windows;
using BatInspector.Controls;

namespace BatInspector.Forms
{
  /// <summary>
  /// Interaction logic for FrmSetupRecorder.xaml
  /// </summary>
  public partial class FrmSetupRecorder : Window
  {
    CtrlRecorder _rec;
    public FrmSetupRecorder(CtrlRecorder recorder)
    {
      InitializeComponent();
      _rec = recorder;
      int wl = 130;
      int wt = 150;
      _tbStatus.Foreground = System.Windows.Media.Brushes.Red;
      initParameterTab(wl, wt);
      initStatusTab(wl, wt);
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
      _ctlRecStart.Hour = (int)_rec.Control.StartH.Value;
      _ctlRecStart.Minute =(int)_rec.Control.StartMin.Value;
      _ctlRecStop.Hour = (int)_rec.Control.StopH.Value;
      _ctlRecStop.Minute = (int)_rec.Control.StopMin.Value;
      _ctlSampleRate.setItems(_rec.Acquisition.SampleRate.Items);
      _ctlSampleRate.SelectIndex = (int)_rec.Acquisition.SampleRate.Value;
      _ctlSampleRate.IsEnabled = true;
      _ctlTrigFreq.setValue(_rec.Trigger.Frequency.Value);
      _ctlTrigFreq.IsEnabled = true;
      _ctlTrigLength.setValue(_rec.Trigger.EventLength.Value);
      _ctlTrigLength.IsEnabled = true;
      _ctlTrigLevel.setValue(_rec.Trigger.Level.Value);
      _ctlTrigLevel.IsEnabled = true;
      _ctlRecordingTime.setValue(_rec.Control.RecTime.Value);
      _ctlRecordingTime.IsEnabled = true;
      _ctlRecMode.setItems(_rec.Control.Mode.Items);
      _ctlRecMode.SelectIndex = (int)_rec.Control.Mode.Value;
      _ctlRecMode.IsEnabled = true;
      _ctlTrigType.setItems(_rec.Trigger.Type.Items); 
      _ctlTrigType.SelectIndex = (int)_rec.Trigger.Type.Value;
      _ctlTrigType.IsEnabled = true;
      _ctlLat.setValue(_rec.General.Latitude.Value.ToString());
      _ctlLat.IsEnabled = true;
      _ctlLon.setValue(_rec.General.Longitude.Value.ToString());
      _ctlLon.IsEnabled = true;
    }

    private void initParameterTab(int wl, int wt)
    {
      _ctlDeadTime.setup(BatInspector.Properties.MyResources.DeadTime, enDataType.DOUBLE, 2, wl, false);
      _ctlGain.setup(BatInspector.Properties.MyResources.CtlZoomGain, 1, wl, wt, null, null, "", false);
      _ctlPreTrigger.setup("Pre Trigger" + " [ms]", enDataType.DOUBLE, 2, wl, false);
      _ctlRecStart.init(new System.DateTime(), false, BatInspector.Properties.MyResources.FrmRecStartingTime, wl);
      _ctlRecStop.init(new System.DateTime(), false, BatInspector.Properties.MyResources.FrmRecStopTime, wl);
      _ctlSampleRate.setup(BatInspector.Properties.MyResources.SamplingRate, 2, wl, wt);
      _ctlTrigFreq.setup(BatInspector.Properties.MyResources.Frequency + " [Hz]", enDataType.DOUBLE, 1, wl, false);
      _ctlTrigLength.setup(BatInspector.Properties.MyResources.FrmRecMinEventLength + " [ms]", enDataType.DOUBLE, 1, wl, false);
      _ctlTrigLevel.setup(BatInspector.Properties.MyResources.frmRecLevel + " [dB]", enDataType.DOUBLE, 1, wl, false);
      _ctlRecordingTime.setup(BatInspector.Properties.MyResources.frmRecRecordingTime, enDataType.DOUBLE, 1, wl, false);
      _ctlRecMode.setup(BatInspector.Properties.MyResources.FrmRecRecordingMode, 3, wl, wt, null, null, "", false);
      _ctlTrigType.setup(BatInspector.Properties.MyResources.FrmRecTriggerType, 4, wl, wt, null, null, "", false);
      _ctlLat.setup(BatInspector.Properties.MyResources.Latitude, enDataType.STRING, 5, wl, false);
      _ctlLon.setup(BatInspector.Properties.MyResources.Longitude, enDataType.STRING, 5, wl, false);
    }

    private void initStatusTab(int wl, int wt)
    {
      _ctlBatVoltage.setup(BatInspector.Properties.MyResources.FrmRecBatteryVoltageV, enDataType.DOUBLE, 2, wl, false);
      _ctlBatVoltage.setValue(0.0);
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
      _ctlNrSatellites.setup(BatInspector.Properties.MyResources.frmRecNrOfSatellites, enDataType.INT, 0, wl, false);
      _ctlNrSatellites.setValue(0);
      _ctlHeight.setup(BatInspector.Properties.MyResources.Height, enDataType.DOUBLE, 1, wl, false);
      _ctlHeight.setValue(0.0);
      _ctlMainLoop.setup(BatInspector.Properties.MyResources.frmRecMainLoop, enDataType.INT, 0, wl, false);
      _ctlMainLoop.setValue(0);
      _ctlRecCount.setup(BatInspector.Properties.MyResources.frmRecNrRecordings, enDataType.INT, 0, wl, false);
      _ctlRecCount.setValue(0);
      _ctlRecState.setup(BatInspector.Properties.MyResources.Status, enDataType.STRING, 0, wl, false);
    }

    private void updateStatusControls()
    {
      _ctlBatVoltage.setValue(0);
      _ctlHumidity.setValue(0);
      _ctlTemperature.setValue(0);
      _ctlAudioBlocks.setValue(0);
      _ctlAudioLoadAvg.setValue(0);
      _ctlAudioLoadMax.setValue(0);
      _ctlDiskFree.setValue(0);
      _ctlLocation.setValue("");
      _ctlNrSatellites.setValue(0);
      _ctlHeight.setValue(0);
      _ctlMainLoop.setValue(0);
      _ctlRecCount.setValue(0);
      _ctlRecState.setValue("");
    }

    private void _btnRead_Click(object sender, RoutedEventArgs e)
    {

    }

    private void _btnWrite_Click(object sender, RoutedEventArgs e)
    {

    }

    private void _btnCancel_Click(object sender, RoutedEventArgs e)
    {
      _rec.disConnect();
      this.Close();
    }

    private void _btnConnect_Click(object sender, RoutedEventArgs e)
    {
      bool res = _rec.connect();
      if (res)
      {
        _tbStatus.Text = BatInspector.Properties.MyResources.FrmRecConnected;
        _tbStatus.Foreground = System.Windows.Media.Brushes.Green;
        initParameterControls();
      }
    }
  }
}
