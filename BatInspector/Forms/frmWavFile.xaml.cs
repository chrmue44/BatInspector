
using System.Windows;
using BatInspector.Controls;
using Microsoft.Win32;

namespace BatInspector.Forms
{
  /// <summary>
  /// Interaktionslogik für frmWavFile.xaml
  /// </summary>
  public partial class frmWavFile : Window
  {
    ViewModel _model;
    public frmWavFile(ViewModel model)
    {
      InitializeComponent();
      _model = model;
      _ctlFileName.setup("File name", Controls.enDataType.STRING, 0, 110, 400);
      _ctlBitsPerSample.setup("Bits per Chan", Controls.enDataType.INT, 0, 110);
      _ctlChannels.setup("Channel count.", Controls.enDataType.INT, 0, 110);
      _ctlSamplingRate.setup("Sampling rate", Controls.enDataType.UINT, 0, 110, 80, true, samplingRateChanged);

    }
  
    private void _btnOpen_Click(object sender, RoutedEventArgs e)
    {
      OpenFileDialog openFileDialog = new OpenFileDialog();
      openFileDialog.Filter = "WAV files (*.wav)|*.wav|All files (*.*)|*.*";
      if (openFileDialog.ShowDialog() == true)
      {
        _model.WavFile.readFile(openFileDialog.FileName);
        _ctlFileName.setValue(openFileDialog.FileName);
        _ctlBitsPerSample.setValue(_model.WavFile.BitsPerSample);
        _ctlChannels.setValue(_model.WavFile.Channels);
        _ctlSamplingRate.setValue(_model.WavFile.SamplingRate);
      }
    }

    private void _btnPlay_Click(object sender, RoutedEventArgs e)
    {
      _model.WavFile.play();
    }

    private void _btnSave_Click(object sender, RoutedEventArgs e)
    {
      _model.WavFile.saveFile();
    }

    private void samplingRateChanged(enDataType type, object val)
    {
      int sr = _ctlSamplingRate.getIntValue();
      _model.WavFile.SamplingRate = (uint)sr;
    }
  }
}
