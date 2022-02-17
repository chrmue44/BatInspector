using BatInspector.Controls;
using BatInspector.Forms;
using System;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BatInspector.Forms
{
  /// <summary>
  /// Interaktionslogik für FrmZoom.xaml
  /// </summary>
  public partial class FrmZoom : Window
  {
    ViewModel _model;

    public System.Windows.Media.ImageSource ImgSource
    {
      set { _ctl._imgFt.Source = value; }
    }
    public FrmZoom(ViewModel model)
    {
      InitializeComponent();
      _model = model;
      ContentRendered += FrmZoom_ContentRendered;
    }

    public void setup(string name, AnalysisFile analysis, string wavFilePath, System.Windows.Media.ImageSource img)
    {
      _ctl.setup(analysis, wavFilePath, _model, img);
      this.Title = name;
    }




    private void FrmZoom_ContentRendered(object sender, EventArgs e)
    {
      _ctl.update();
    }

    private void Window_KeyUp(object sender, KeyEventArgs e)
    {
      _model.KeyPressed = System.Windows.Input.Key.None;
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
      _model.KeyPressed = e.Key;
    }

    private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
    {
      _model.Settings.ZoomWindowHeight = this.Height;
      _model.Settings.ZoomWindowWidth = this.Width;
    }
  }
}
