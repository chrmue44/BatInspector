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
      _ctlDeadTime.setup(BatInspector.Properties.MyResources.DeadTime, enDataType.DOUBLE, 2, wl, true);
      _ctlDeadTime.setValue(_rec.Control.DeadTime);
      _ctlGain.setup(BatInspector.Properties.MyResources.CtlZoomGain, 1, wl, wt);
      _ctlGain.setItems(new string[] { "48dB","58dB" });
      _ctlGain.SelectIndex = (int)_rec.Acquisition.Gain;
      _ctlPreTrigger.setup("Pre Trigger" + " [ms]", enDataType.DOUBLE, 2, wl, true);
      _ctlPreTrigger.setValue(_rec.Acquisition.PreTrigger);
      _ctlRecStart.init(new System.DateTime(), false, BatInspector.Properties.MyResources.FrmRecStartingTime, wl - 20);
      _ctlRecStart.Hour = _rec.Control.StartH;
      _ctlRecStart.Minute = _rec.Control.StartMin;
      _ctlRecStop.init(new System.DateTime(), false, BatInspector.Properties.MyResources.FrmRecStopTime, wl - 20);
      _ctlRecStop.Hour = _rec.Control.StopH;
      _ctlRecStop.Minute = _rec.Control.StopMin;
      _ctlRecTime.setup(BatInspector.Properties.MyResources.CtlZoomRecTime + " [s]", enDataType.DOUBLE, 2, wl, true);
      _ctlRecTime.setValue(_rec.Control.RecTime);
      _ctlSampleRate.setup(BatInspector.Properties.MyResources.SamplingRate, 2, wl, wt);
      _ctlSampleRate.setItems(new string[] { "19 kHz", "32 kHz", "44 kHz", "48 kHz", "192 kHz", "312 kHz", "384 kHz", "480 kHz" });
      _ctlSampleRate.SelectIndex = (int)_rec.Acquisition.SampleRate;
      _ctlTrigFreq.setup(BatInspector.Properties.MyResources.Frequency + " [Hz]", enDataType.DOUBLE, 1, wl, true);
      _ctlTrigFreq.setValue(_rec.Trigger.Frequency);
      _ctlTrigLength.setup(BatInspector.Properties.MyResources.FrmRecMinEventLength +" [ms]", enDataType.DOUBLE, 1, wl, true);
      _ctlTrigLength.setValue(_rec.Trigger.Length);
      _ctlTrigLevel.setup(BatInspector.Properties.MyResources.Level + " [dB]", enDataType.DOUBLE, 1, wl, true);
      _ctlTrigLevel.setValue(_rec.Trigger.Level);
      _ctlRecMode.setup(BatInspector.Properties.MyResources.FrmRecRecordingMode, 3, wl, wt);
      _ctlRecMode.setItems(new string[] { BatInspector.Properties.MyResources.Off, 
                                          BatInspector.Properties.MyResources.On,
                                          BatInspector.Properties.MyResources.FrmRecTimeControlled, 
                                          BatInspector.Properties.MyResources.FrmRecTwilight });
      _ctlRecMode.SelectIndex =(int)_rec.Control.Mode;
      _ctlTrigType.setup(BatInspector.Properties.MyResources.FrmRecTriggerType,4, wl, wt);
      _ctlTrigType.setItems(new string[] { BatInspector.Properties.MyResources.Level, 
                                           BatInspector.Properties.MyResources.Frequency, 
                                           BatInspector.Properties.MyResources.LevelFrequency });
      _ctlTrigType.SelectIndex =(int)_rec.Trigger.Type;
      _ctlLat.setup(BatInspector.Properties.MyResources.Latitude, enDataType.STRING, 5, wl, true);
      _ctlLat.setValue(_rec.General.Latitude.ToString());
      _ctlLon.setup(BatInspector.Properties.MyResources.Longitude, enDataType.STRING, 5, wl, true);
      _ctlLon.setValue(_rec.General.Longitude.ToString());
    }

    private void _btnRead_Click(object sender, RoutedEventArgs e)
    {

    }

    private void _btnWrite_Click(object sender, RoutedEventArgs e)
    {

    }

    private void _btnCancel_Click(object sender, RoutedEventArgs e)
    {
      this.Close();
    }
  }
}
