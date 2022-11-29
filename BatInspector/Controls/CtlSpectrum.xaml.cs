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
  /// <summary>
  /// Interaktionslogik für CtlSpectrum.xaml
  /// </summary>
  public partial class CtlSpectrum : UserControl
  {
    bool _initFlag = false;
    Spectrum _spectrum;
    int _mode;
    double _fMin;
    double _fMax;
   
    public bool InitFlag { get { return _initFlag; } set { _initFlag = value; }  }
    
    public CtlSpectrum()
    {
      InitializeComponent();
    }

    public void init(Spectrum sp, double maxF)
    {
      _spectrum = sp;
      _spectrum.RulerDataF.setRange(0, maxF);
      _initFlag = true;
    }


    public void createFftImage(double[] samples, double tStart, double tEnd, double fMin, double fMax, int samplingRate, int mode, bool logarithmic)
    {
      _fMax = fMax;
      _fMin = fMin;
      _spectrum.create(samples, tStart, tEnd, samplingRate, logarithmic);
      _mode = mode;
      drawSpectrum(mode, fMin, fMax, logarithmic);
    }

    private void drawSpectrum(int mode, double fMin, double fMax, bool logarithmic)
    {
      _cvSpec.Children.Clear();
      if ((_spectrum != null) && (_spectrum.Amplitude != null))
      {
        double min = _spectrum.findMinAmplitude(logarithmic);
        double max = _spectrum.findMaxAmplitude(logarithmic);
        int w = (int)_cvSpec.ActualWidth;
        int h = (int)_cvSpec.ActualHeight;
        int n = h > 0 ? _spectrum.Amplitude.Length / h + 1 : 1;

        for (int y = h; y > 0; y--)
        {
          double f = fMin + (double)(h - y) / h * (fMax - fMin); 
          int i = (int)(_spectrum.Amplitude.Length * f /_spectrum.RulerDataF.Max);
          double a = _spectrum.getMeanAmpl(i, n, logarithmic);
          int x1 = w;
          int x2 = w - (int)(w * (a / (max - min)));
          if (mode == 0)
            CtrlZoom.createLine(_cvSpec, x1, y, x2, y, Brushes.Blue);
          else
            CtrlZoom.createLine(_cvSpec, x1, y, x2, y, Brushes.Cyan);

        }
      }
    }

    private void _cvSpec_MouseMove(object sender, MouseEventArgs e)
    {
      Point pos = e.GetPosition(this);
      double f = _spectrum.RulerDataF.Min + (pos.X / _cvSpec.ActualWidth) * (_spectrum.RulerDataF.Max - _spectrum.RulerDataF.Min);
      _cvSpec.ToolTip = f.ToString("#.#", CultureInfo.InvariantCulture) + "[kHz]";
    }

    private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
    {
      drawSpectrum(_mode, _fMin, _fMax, false);
    }

    public void drawCursor(int cursorNr, Cursor cursorI)
    {
      Line cursorL = _cursor1;
      if (cursorNr == 2)
        cursorL = _cursor2;
      cursorL.Visibility = cursorI.Visible ? Visibility.Visible : Visibility.Hidden;
      cursorL.X1 = 0;
      cursorL.X2 = ActualWidth;
      cursorL.Y1 = (1.0 - (cursorI.Freq / (_spectrum.RulerDataF.Max - _spectrum.RulerDataF.Min) - _spectrum.RulerDataF.Min)) * ActualHeight;
      cursorL.Y2 = cursorL.Y1;
    }
  }
}
