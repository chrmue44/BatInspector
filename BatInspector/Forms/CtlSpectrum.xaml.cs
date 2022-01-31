using System;
using System.Collections.Generic;
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

namespace BatInspector.Forms
{
  /// <summary>
  /// Interaktionslogik für CtlSpectrum.xaml
  /// </summary>
  public partial class CtlSpectrum : UserControl
  {
    Spectrum _spectrum;
    public CtlSpectrum()
    {
      InitializeComponent();
    }

    public void init(Spectrum sp)
    {
      _spectrum = sp;
      initRulerF();
    }

    public void initRulerF()
    {
      _cvRulerF.Children.Clear();
      FrmZoom.createLine(_cvRulerF, 0, 3, _cvRulerF.ActualWidth, 3, Brushes.Black);
      RulerData rData = _spectrum.RulerDataF;
      for (int i = 0; i <= 10; i++)
      {
        FrmZoom.createLine(_cvRulerF, _cvRulerF.ActualWidth * i / 10, 3, _cvRulerF.ActualWidth * i / 10, 10, Brushes.Black);
        string str = ((rData.Max - rData.Min) * i / 10 + rData.Min).ToString("0.0");
        if((i % 2) == 0)
          FrmZoom.createText(_cvRulerF, _cvRulerF.ActualWidth * i / 10 - 20, 15, str, Colors.Black);
      }
    }

    public void createFftImage()
    {
      int w = (int)_cvSpec.ActualWidth;
      int h = (int)_cvSpec.ActualHeight;
      for (int i = 0; i < 20; i++)
      {
        int x = (int)(_cvSpec.ActualWidth / 19.0 * i);
        int y1 = (int)(_cvSpec.ActualHeight);
        int y2 = (int)(_cvSpec.ActualHeight - i*i / 4);
        FrmZoom.createLine(_cvSpec, x, y1,x , y2, Brushes.Blue);
      }
    }
  }
}
