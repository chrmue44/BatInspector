using BatInspector.Forms;
using System;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BatInspector
{
  /// <summary>
  /// Interaktionslogik für FrmZoom.xaml
  /// </summary>
  public partial class FrmZoom : Window
  {
    AnalysisFile _analysis;
    string _wavFilePath;
    Waterfall _wf;
    ViewModel _model;

    public FrmZoom(string name, AnalysisFile analysis, string wavFilePath, ViewModel model)
    {
      InitializeComponent();
      this.Title = name;
      _analysis = analysis;
      _model = model;

      _model.ZoomView.RulerDataA.setRange(-1, 1);
      _model.ZoomView.RulerDataT.setRange(0, _analysis.Duration);
      _model.ZoomView.RulerDataF.setRange(0, _analysis.SampleRate / 2000);

      initRulerT();
      initRulerA();
      initRulerF();

      _freq1.setup("Frequency [kHz]:", Forms.enDataType.DOUBLE, 1);
      _time1.setup("Time [s]:", Forms.enDataType.DOUBLE, 3);
      _freq2.setup("Frequency: [kHz]", Forms.enDataType.DOUBLE, 1);
      _time2.setup("Time [s]:", Forms.enDataType.DOUBLE, 3);
      _sampleRate.setup("SampleRate [kHz]", Forms.enDataType.DOUBLE, 1);
      _sampleRate.setValue((double)_analysis.SampleRate / 1000);
      _duration.setup("Duration [s]", Forms.enDataType.DOUBLE, 3);
      _duration.setValue(_analysis.Duration);
      _deltaT.setup("Delta T [ms]:", Forms.enDataType.DOUBLE, 0);
      _wavFilePath = wavFilePath;
      _wf = new Waterfall(_wavFilePath + analysis.FileName, 512, 512, 256, _model.Settings);
      _ctlRange.setup("Range [dB]:", Forms.enDataType.DOUBLE, 0, rangeChanged);
      _ctlRange.setValue(20.0);
      ContentRendered += FrmZoom_ContentRendered;
      SizeChanged += FrmZoom_SizeChanged;
      MouseDown += FrmZoomMouseDown;
    }

    public void rangeChanged(enDataType type, object val)
    {
      if (type == enDataType.DOUBLE)
      {
        double range = (double)val;
        _wf.Range = range;
        updateRuler();
        updateImage();
      }
      else
        DebugLog.log("wrong data type for 'Range'", enLogType.ERROR);
    }


    private void _btnIncRange_Click(object sender, RoutedEventArgs e)
    {
      _wf.Range += 1.0;
      _ctlRange.setValue(_wf.Range);
      updateRuler();
      updateImage();
    }

    private void _btnDecRange_Click(object sender, RoutedEventArgs e)
    {
      if (_wf.Range > 3)
      {
        _wf.Range -= 1.0;
        _ctlRange.setValue(_wf.Range);
        updateRuler();
        updateImage();
      }
    }
    private void FrmZoomMouseDown(object sender, MouseEventArgs e)
    {
      Point p = e.GetPosition(_imgFt);
      ZoomView z = _model.ZoomView;
      double f = (1.0 - (double)(p.Y) / ((double)_imgFt.ActualHeight)) * (z.RulerDataF.Max - z.RulerDataF.Min) + z.RulerDataF.Min;
      double t = (double)(p.X - _imgFt.Margin.Left) / (double)_imgFt.ActualWidth * (z.RulerDataT.Max - z.RulerDataT.Min) + z.RulerDataT.Min;
      if (e.LeftButton == MouseButtonState.Pressed)
      {
        if (z.RulerDataF.check(f) && z.RulerDataT.check(t))
        {
          _model.ZoomView.Cursor1.set(t, f, true);
          drawCursor(_cursorX1, _cursorY1, _model.ZoomView.Cursor1);
          setFileInformations();
        }
      }
      if (e.RightButton == MouseButtonState.Pressed)
      {
        if (z.RulerDataF.check(f) && z.RulerDataT.check(t))
        {
          _model.ZoomView.Cursor2.set(t, f, true);
          drawCursor(_cursorX2, _cursorY2, _model.ZoomView.Cursor2);
          setFileInformations();
        }
      }
    }

    public void setFileInformations()
    {
      ZoomView z = _model.ZoomView;
      _sampleRate.setValue((double)_analysis.SampleRate / 1000);
      _duration.setValue(_analysis.Duration);
      if (z.Cursor1.Visible)
      {
        _grpCursor1.Visibility = Visibility.Visible;
        _freq1.setValue(z.Cursor1.Freq);
        _time1.setValue(z.Cursor1.Time);
      }
      else
        _grpCursor1.Visibility = Visibility.Hidden;

      if (z.Cursor2.Visible)
      {
        _grpCursor2.Visibility = Visibility.Visible;
        _freq2.setValue(z.Cursor2.Freq);
        _time2.setValue(z.Cursor2.Time);
      }
      else
        _grpCursor2.Visibility = Visibility.Hidden;
      if (z.Cursor1.Visible && z.Cursor2.Visible)
      {
        _deltaT.setValue((z.Cursor2.Time - z.Cursor1.Time) * 1000);
        _deltaT.Visibility = Visibility.Visible;
      }
      else
        _deltaT.Visibility = Visibility.Hidden;
    }
    private void drawCursor(Line lx, Line ly, Cursor cursor)
    {
      lx.Visibility = cursor.Visible ? Visibility.Visible : Visibility.Hidden;
      ly.Visibility = cursor.Visible ? Visibility.Visible : Visibility.Hidden;
      int x = (int)(_imgFt.Margin.Left + (cursor.Time - _model.ZoomView.RulerDataT.Min) /
                       (_model.ZoomView.RulerDataT.Max - _model.ZoomView.RulerDataT.Min) * _imgFt.ActualWidth);
      int y = (int)(_imgFt.Margin.Top + (1.0 - (cursor.Freq - _model.ZoomView.RulerDataF.Min) / 
                      (_model.ZoomView.RulerDataF.Max - _model.ZoomView.RulerDataF.Min)) * _imgFt.ActualHeight);
      lx.X1 = x;
      lx.Y1 = _imgFt.Margin.Top;
      lx.X2 = x;
      lx.Y2 = _imgFt.ActualHeight + _imgFt.Margin.Top;
      ly.X1 = _imgFt.Margin.Left;
      ly.Y1 = y;
      ly.X2 = _imgFt.ActualWidth + _imgFt.Margin.Left;
      ly.Y2 = y;
    }

    private void FrmZoom_SizeChanged(object sender, SizeChangedEventArgs e)
    {
      initRulerF();
      initRulerT();
      initRulerA();
      drawCursor(_cursorX1, _cursorY1, _model.ZoomView.Cursor1);
      drawCursor(_cursorX2, _cursorY2, _model.ZoomView.Cursor2);
    }

    private void hideCursors()
    {
      _model.ZoomView.Cursor1.hide();
      _model.ZoomView.Cursor2.hide();
      _grpCursor1.Visibility = Visibility.Hidden;
      _grpCursor2.Visibility = Visibility.Hidden;
      _deltaT.Visibility = Visibility.Hidden;
      drawCursor(_cursorX1, _cursorY1, _model.ZoomView.Cursor1);
      drawCursor(_cursorX2, _cursorY2, _model.ZoomView.Cursor2);
    }

    private void FrmZoom_ContentRendered(object sender, EventArgs e)
    {
      _model.ZoomView.RulerDataA.setRange(-1, 1);
      _model.ZoomView.RulerDataT.setRange(0, _analysis.Duration);
      _model.ZoomView.RulerDataF.setRange(0, _analysis.SampleRate / 2000);

      initRulerF();
      initRulerT();
      initRulerA();
    }

    void initRulerA()
    {
      _rulerA.Children.Clear();
      createLine(_rulerA, _rulerA.ActualWidth - 3, _imgXt.Margin.Top,
                          _rulerA.ActualWidth - 3, _imgXt.ActualHeight + _imgXt.Margin.Top, Brushes.Black);
      int steps = 4;
      RulerData rData = _model.ZoomView.RulerDataA;
      for (int i = 0; i <= steps; i++)
      {
        double y = _imgXt.Margin.Top + _imgXt.ActualHeight * i / steps;
        createLine(_rulerA, _rulerA.ActualWidth - 3, y,
                            _rulerA.ActualWidth - 10, y, Brushes.Black);
        string str = ((rData.Max - rData.Min) * (steps - i) / steps + rData.Min).ToString("0.#");
        createText(_rulerA, _rulerA.ActualWidth - 35, y - 5, str, Colors.Black);
      }
    }
    void initRulerF()
    {
      _rulerF.Children.Clear();
      createLine(_rulerF, _rulerF.ActualWidth - 3, _imgFt.Margin.Top,
                          _rulerF.ActualWidth - 3, _imgFt.ActualHeight + _imgFt.Margin.Top, Brushes.Black);
      RulerData rData = _model.ZoomView.RulerDataF;
      for (int i = 0; i <= 10; i++)
      {
        double y = _imgFt.Margin.Top +_imgFt.ActualHeight * i / 10;
        createLine(_rulerF, _rulerF.ActualWidth - 3, y,
                            _rulerF.ActualWidth - 10, y, Brushes.Black);
        string str = ((rData.Max - rData.Min) * (10 - i) / 10 + rData.Min).ToString("0.#");
        createText(_rulerF, _rulerF.ActualWidth - 35, y - 5, str, Colors.Black);
      }
    }

    void initRulerT()
    {
      _rulerT.Children.Clear();
      createLine(_rulerT, 0, 3, _rulerT.ActualWidth, 3, Brushes.Black);
      RulerData rData = _model.ZoomView.RulerDataT;
      for (int i = 0; i <= 10; i++)
      {
        createLine(_rulerT, _rulerT.ActualWidth * i / 10, 3, _rulerT.ActualWidth * i / 10, 10, Brushes.Black);
        string str = ((rData.Max - rData.Min) *  i / 10 + rData.Min).ToString("0.###");
        createText(_rulerT, _rulerT.ActualWidth * i / 10 - 20, 15, str, Colors.Black);
      }
    }


    private void createLine(Canvas ca, double x1, double y1, double x2, double y2, Brush brush, int thickness = 1)
    {
      Line li = new Line();
      li.X1 = x1;
      li.X2 = x2;
      li.Y1 = y1;
      li.Y2 = y2;
      li.Stroke = brush;
      li.StrokeThickness = thickness;
      ca.Children.Add(li);
    }

    private void createText(Canvas can, double x, double y, string text, Color color)
    {
      TextBlock textBlock = new TextBlock();
      textBlock.Text = text;
      textBlock.Foreground = new SolidColorBrush(color);
      textBlock.TextAlignment = TextAlignment.Right;
      Canvas.SetLeft(textBlock, x);
      Canvas.SetTop(textBlock, y);
      can.Children.Add(textBlock);
    }

    private void _btnZoomCursor_Click(object sender, RoutedEventArgs e)
    {
      ZoomView z = _model.ZoomView;
      if (z.Cursor1.Visible && z.Cursor2.Visible)
      {
        _model.ZoomView.RulerDataF.setRange(z.Cursor1.Freq, z.Cursor2.Freq);
        _model.ZoomView.RulerDataT.setRange(z.Cursor1.Time, z.Cursor2.Time);
        hideCursors();
        createZoomImg();
      }
      else
      {
        MessageBox.Show("Zoom to cursor not possible, enable both cursors first!", "Information", MessageBoxButton.OK, MessageBoxImage.Warning);
      }
    }

    private void _btnZoomTotal_Click(object sender, RoutedEventArgs e)
    {
      _model.ZoomView.RulerDataF.setRange(0,_wf.SamplingRate / 2000);
      _model.ZoomView.RulerDataT.setRange( 0, _wf.Duration);
      hideCursors();
      createZoomImg();
    }

    private void _btnZoomInV_Click(object sender, RoutedEventArgs e)
    {
      _model.ZoomView.zoomInV();
      hideCursors();
      createZoomImg();
    }

    private void _btnZoomOutV_Click(object sender, RoutedEventArgs e)
    {
      _model.ZoomView.zoomOutV();
      hideCursors();
      createZoomImg();
    }

    private void _btnZoomInH_Click(object sender, RoutedEventArgs e)
    {
      _model.ZoomView.zoomInH();
      hideCursors();
      createZoomImg();
    }

    private void _btnZoomOutH_Click(object sender, RoutedEventArgs e)
    {
      _model.ZoomView.zoomOutH();
      hideCursors();
      createZoomImg();
    }

    private void _btnmoveLeft_Click(object sender, RoutedEventArgs e)
    {
      if(_model.ZoomView.moveLeft())
      { 
        hideCursors();
        createZoomImg();
      }
    }

    private void _btnmoveRight_Click(object sender, RoutedEventArgs e)
    {
      if(_model.ZoomView.moveRight(_wf.Duration))
      { 
        hideCursors();
        createZoomImg();
      }
    }

    private void _btnmoveUp_Click(object sender, RoutedEventArgs e)
    {
      if (_model.ZoomView.moveUp(_wf.SamplingRate / 2000))
      {
        hideCursors();
        createZoomImg();
      }
    }

    private void _btnmoveDown_Click(object sender, RoutedEventArgs e)
    {
      if (_model.ZoomView.moveDown(0))
      {
        hideCursors();
        createZoomImg();
      }
    }

    private void updateRuler()
    {
      _model.ZoomView.RulerDataT.limits(0, _wf.Duration);
      initRulerT();

      _model.ZoomView.RulerDataF.limits(0, _wf.SamplingRate / 2000);
      initRulerF();

      _model.ZoomView.RulerDataA.limits(-1, 1);
      initRulerF();
    }

    private void createZoomImg()
    {
      updateRuler();
      _wf.generateFtDiagram(_model.ZoomView.RulerDataT.Min, _model.ZoomView.RulerDataT.Max, _model.Settings.FftWidth);
      updateImage();
    }

    private void updateImage()
    {
      System.Drawing.Bitmap bmpFt = _wf.generateFtPicture(_model.ZoomView.RulerDataF.Min, _model.ZoomView.RulerDataF.Max);
      if (bmpFt != null)
      {
        BitmapImage bImg = ViewModel.Convert(bmpFt);
        _imgFt.Source = bImg;
      }
      System.Drawing.Bitmap bmpXt = _wf.generateXtPicture(_model.ZoomView.RulerDataA.Min, _model.ZoomView.RulerDataA.Max,
                                                          _model.ZoomView.RulerDataT.Min, _model.ZoomView.RulerDataT.Max);
      if (bmpXt != null)
      {
        BitmapImage bImg = ViewModel.Convert(bmpXt);
        _imgXt.Source = bImg;
      }
    }

    private void Window_KeyUp(object sender, KeyEventArgs e)
    {
      _model.KeyPressed = System.Windows.Input.Key.None;
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
      _model.KeyPressed = e.Key;
    }

    private void _btnPlay_1_Click(object sender, RoutedEventArgs e)
    {
      _wf.play(1, _model.ZoomView.RulerDataT.Min, _model.ZoomView.RulerDataT.Max);
    }
    private void _btnPlay_10_Click(object sender, RoutedEventArgs e)
    {
      _wf.play(10, _model.ZoomView.RulerDataT.Min, _model.ZoomView.RulerDataT.Max);
    }
    private void _btnPlay_20_Click(object sender, RoutedEventArgs e)
    {
      _wf.play(20, _model.ZoomView.RulerDataT.Min, _model.ZoomView.RulerDataT.Max);
    }
  }
}
