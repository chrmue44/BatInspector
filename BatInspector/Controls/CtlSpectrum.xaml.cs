using BatInspector.Forms;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BatInspector.Controls
{

  public delegate int dlgSpectrumMode(int mode);

  /// <summary>
  /// Interaktionslogik für CtlSpectrum.xaml
  /// </summary>
  public partial class CtlSpectrum : UserControl
  {
    bool _initFlag = false;
    Spectrum _spectrum;
    dlgSpectrumMode _dlgSpecMode;
    public CtlSpectrum()
    {
      InitializeComponent();
    }

    public void init(Spectrum sp, double maxF, dlgSpectrumMode dlg)
    {
      _spectrum = sp;
      _dlgSpecMode = dlg;
      _spectrum.RulerDataF.setRange(0, maxF);
      initRulerF();
      _ctlTime.setup(BatInspector.Properties.MyResources.CtlSpectrumTimeSpan, enDataType.DOUBLE, 3, 50, 50, 20);
      _cbMode.Items.Clear();
      _cbMode.Items.Add("Call");
      _cbMode.Items.Add("Cursor");
      _initFlag = true;
      _cbMode.SelectedIndex = 0;
    }

    public void initRulerF()
    {
      if (Visibility == Visibility.Visible)
      {
        _cvRulerF.Children.Clear();
        CtrlZoom.createLine(_cvRulerF, 0, 3, _cvRulerF.ActualWidth, 3, Brushes.Black);
        RulerData rData = _spectrum.RulerDataF;
        for (int i = 0; i <= 10; i++)
        {
          CtrlZoom.createLine(_cvRulerF, _cvRulerF.ActualWidth * i / 10, 3, _cvRulerF.ActualWidth * i / 10, 10, Brushes.Black);
          string str = ((rData.Max - rData.Min) * i / 10 + rData.Min).ToString("0.0", CultureInfo.InvariantCulture);
          if ((i % 2) == 0)
            CtrlZoom.createText(_cvRulerF, _cvRulerF.ActualWidth * i / 10 - 20, 15, str, Colors.Black);
        }
      }
    }

    public void createFftImage(double[] samples, double tStart, double tEnd, int samplingRate)
    {
      _cvSpec.Children.Clear();
      _spectrum.create(samples, tStart, tEnd, samplingRate);
      _ctlTime.setMinValue(tStart);
      _ctlTime.setMaxValue(tEnd);
      double min = _spectrum.findMinAmplitude();
      double max = _spectrum.findMaxAmplitude();
      int w = (int)_cvSpec.ActualWidth;
      int h = (int)_cvSpec.ActualHeight;
      int n = _spectrum.Amplitude.Length / h + 1;

      for (int x = 0; x < w; x++)
      {
        int y1 = (int)(_cvSpec.ActualHeight);
        int i = (int)((double)_spectrum.Amplitude.Length / w * x * _spectrum.RulerDataF.Max/_spectrum.Fmax);
        double a =  _spectrum.getMeanAmpl(i, n);
        int y2 = h - (int)(a/(max - min) * h);
        if(_cbMode.SelectedIndex == 0)
          CtrlZoom.createLine(_cvSpec, x, y1, x , y2, Brushes.Blue);
        else
          CtrlZoom.createLine(_cvSpec, x, y1, x, y2, Brushes.Cyan);
      }
    }

    public void setTimeLimits(double tMin, double tMax)
    {
      _ctlTime.setMinValue(tMin);
      _ctlTime.setMaxValue(tMax);
    }

    private void _cbModeChanged(object sender, SelectionChangedEventArgs e)
    {
      if (!_initFlag)
      {
        int idx = _dlgSpecMode(_cbMode.SelectedIndex);
        _cbMode.SelectedIndex = idx;
      }
      else
        _initFlag = false;
    }

    private void _cvSpec_MouseMove(object sender, MouseEventArgs e)
    {
      Point pos = e.GetPosition(this);
      double f = _spectrum.RulerDataF.Min + (pos.X / _cvSpec.ActualWidth) * (_spectrum.RulerDataF.Max - _spectrum.RulerDataF.Min);
      _cvSpec.ToolTip = f.ToString("#.#", CultureInfo.InvariantCulture) + "[kHz]";
    }
  }
}
