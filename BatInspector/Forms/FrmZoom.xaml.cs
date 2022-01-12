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
    Cursor _cursor1;
    Cursor _cursor2;
    RulerData _rulerDataT;
    RulerData _rulerDataF;
    RulerData _rulerDataA;
    string _wavFilePath;
    Waterfall _wf;

    public FrmZoom(string name, AnalysisFile analysis, string wavFilePath)
    {
      InitializeComponent();
      this.Title = name;
      _analysis = analysis;
      _cursor1 = new Cursor();
      _cursor2 = new Cursor();
      _rulerDataT = new RulerData();
      _rulerDataF = new RulerData();
      _rulerDataA = new RulerData();

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
      _wf = new Waterfall(_wavFilePath + analysis.FileName, 512, 512, 256);
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
        updateImage();
      }
      else
        DebugLog.log("wrong data type for 'Range'", enLogType.ERROR);
    }

    private void FrmZoomMouseDown(object sender, MouseEventArgs e)
    {
      Point p = e.GetPosition(_imgFt);
      double f = (1.0 - (double)(p.Y) / ((double)_imgFt.ActualHeight)) * (_rulerDataF.Max - _rulerDataF.Min) + _rulerDataF.Min;
      double t = (double)(p.X - _imgFt.Margin.Left) / (double)_imgFt.ActualWidth * (_rulerDataT.Max - _rulerDataT.Min) + _rulerDataT.Min;
      if (e.LeftButton == MouseButtonState.Pressed)
      {
        if ((f <= _rulerDataF.Max) && (f >= _rulerDataF.Min) && (t >= _rulerDataT.Min) && (t <= _rulerDataT.Max))
        {
          _cursor1.Visible = true;
          _cursor1.Freq = f;
          _cursor1.Time = t;
          drawCursor(_cursorX1, _cursorY1, _cursor1);
          setFileInformations();
        }
      }
      if (e.RightButton == MouseButtonState.Pressed)
      {
        if ((f <= _rulerDataF.Max) && (f >= _rulerDataF.Min) && (t >= _rulerDataT.Min) && (t <= _rulerDataT.Max))
        {
          _cursor2.Visible = true;
          _cursor2.Freq = f;
          _cursor2.Time = t;
          drawCursor(_cursorX2, _cursorY2, _cursor2);
          setFileInformations();
        }
      }
    }

    public void setFileInformations()
    {
      _sampleRate.setValue((double)_analysis.SampleRate / 1000);
      _duration.setValue(_analysis.Duration);
      if (_cursor1.Visible)
      {
        _grpCursor1.Visibility = Visibility.Visible;
        _freq1.setValue(_cursor1.Freq);
        _time1.setValue(_cursor1.Time);
      }
      else
        _grpCursor1.Visibility = Visibility.Hidden;

      if (_cursor2.Visible)
      {
        _grpCursor2.Visibility = Visibility.Visible;
        _freq2.setValue(_cursor2.Freq);
        _time2.setValue(_cursor2.Time);
      }
      else
        _grpCursor2.Visibility = Visibility.Hidden;
      if (_cursor1.Visible && _cursor2.Visible)
      {
        _deltaT.setValue((_cursor2.Time - _cursor1.Time) * 1000);
        _deltaT.Visibility = Visibility.Visible;
      }
      else
        _deltaT.Visibility = Visibility.Hidden;
    }
    private void drawCursor(Line lx, Line ly, Cursor cursor)
    {
      lx.Visibility = cursor.Visible ? Visibility.Visible : Visibility.Hidden;
      ly.Visibility = cursor.Visible ? Visibility.Visible : Visibility.Hidden;
      int x = (int)(_imgFt.Margin.Left + (cursor.Time - _rulerDataT.Min) /(_rulerDataT.Max - _rulerDataT.Min) * _imgFt.ActualWidth);
      int y = (int)(_imgFt.Margin.Top + (1.0 - (cursor.Freq - _rulerDataF.Min) / (_rulerDataF.Max - _rulerDataF.Min)) * _imgFt.ActualHeight);
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
      initRulerF(_rulerDataF.Min, _rulerDataF.Max);
      initRulerT(_rulerDataT.Min, _rulerDataT.Max);
      initRulerA(_rulerDataA.Min, _rulerDataA.Max);
      drawCursor(_cursorX1, _cursorY1, _cursor1);
      drawCursor(_cursorX2, _cursorY2, _cursor2);
    }

    private void hideCursors()
    {
      _cursor1.Visible = false;
      _cursor2.Visible = false;
      _grpCursor1.Visibility = Visibility.Hidden;
      _grpCursor2.Visibility = Visibility.Hidden;
      _deltaT.Visibility = Visibility.Hidden;
      drawCursor(_cursorX1, _cursorY1, _cursor1);
      drawCursor(_cursorX2, _cursorY2, _cursor2);
    }

    private void FrmZoom_ContentRendered(object sender, EventArgs e)
    {
      initRulerF(0, _analysis.SampleRate/2000);
      initRulerT(0, _analysis.Duration);
      initRulerA(-1, 1);
    }

    void initRulerA(double min, double max)
    {
      _rulerA.Children.Clear();
      _rulerDataA.Min = min;
      _rulerDataA.Max = max;
      createLine(_rulerA, _rulerA.ActualWidth - 3, _imgXt.Margin.Top,
                          _rulerA.ActualWidth - 3, _imgXt.ActualHeight + _imgXt.Margin.Top, Brushes.Black);
      int steps = 4;
      for (int i = 0; i <= steps; i++)
      {
        double y = _imgXt.Margin.Top + _imgXt.ActualHeight * i / steps;
        createLine(_rulerA, _rulerA.ActualWidth - 3, y,
                            _rulerA.ActualWidth - 10, y, Brushes.Black);
        string str = ((max - min) * (steps - i) / steps + min).ToString("0.#");
        createText(_rulerA, _rulerA.ActualWidth - 35, y - 5, str, Colors.Black);
      }
    }
    void initRulerF(double min, double max)
    {
      _rulerF.Children.Clear();
      _rulerDataF.Min = min;
      _rulerDataF.Max = max;
      createLine(_rulerF, _rulerF.ActualWidth - 3, _imgFt.Margin.Top,
                          _rulerF.ActualWidth - 3, _imgFt.ActualHeight + _imgFt.Margin.Top, Brushes.Black);
      for(int i = 0; i <= 10; i++)
      {
        double y = _imgFt.Margin.Top +_imgFt.ActualHeight * i / 10;
        createLine(_rulerF, _rulerF.ActualWidth - 3, y,
                            _rulerF.ActualWidth - 10, y, Brushes.Black);
        string str = ((max - min) * (10 - i) / 10 + min).ToString("0.#");
        createText(_rulerF, _rulerF.ActualWidth - 35, y - 5, str, Colors.Black);
      }
    }

    void initRulerT(double min, double max)
    {
      _rulerT.Children.Clear();
      _rulerDataT.Min = min;
      _rulerDataT.Max = max;
      createLine(_rulerT, 0, 3, _rulerT.ActualWidth, 3, Brushes.Black);
      for (int i = 0; i <= 10; i++)
      {
        createLine(_rulerT, _rulerT.ActualWidth * i / 10, 3, _rulerT.ActualWidth * i / 10, 10, Brushes.Black);
        string str = ((max - min) *  i / 10 + min).ToString("0.###");
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
      if (_cursor1.Visible && _cursor2.Visible)
      {
        _rulerDataF.Min = Math.Min(_cursor1.Freq, _cursor2.Freq);
        _rulerDataF.Max = Math.Max(_cursor1.Freq, _cursor2.Freq);
        _rulerDataT.Min = Math.Min(_cursor1.Time, _cursor2.Time);
        _rulerDataT.Max = Math.Max(_cursor1.Time, _cursor2.Time);
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
      _rulerDataF.Min = 0;
      _rulerDataF.Max = _wf.SamplingRate / 2000;
      _rulerDataT.Min = 0;
      _rulerDataT.Max = _wf.Duration;
      hideCursors();
      createZoomImg();
    }

    private void _btnZoomInV_Click(object sender, RoutedEventArgs e)
    {
      _rulerDataF.Max = (_rulerDataF.Max - _rulerDataF.Min) / 2 + _rulerDataF.Min;
      hideCursors();
      createZoomImg();
    }

    private void _btnZoomOutV_Click(object sender, RoutedEventArgs e)
    {
      _rulerDataF.Max = (_rulerDataF.Max - _rulerDataF.Min) * 2 + _rulerDataF.Min;
      hideCursors();
      createZoomImg();
    }

    private void _btnZoomInH_Click(object sender, RoutedEventArgs e)
    {
      _rulerDataT.Max = (_rulerDataT.Max - _rulerDataT.Min)/ 2 + _rulerDataT.Min;
      hideCursors();
      createZoomImg();
    }

    private void _btnZoomOutH_Click(object sender, RoutedEventArgs e)
    {
      _rulerDataT.Max = (_rulerDataT.Max - _rulerDataT.Min) * 2 + _rulerDataT.Min;
      hideCursors();
      createZoomImg();
    }

    private void _btnmoveLeft_Click(object sender, RoutedEventArgs e)
    {
      double width = _rulerDataT.Max - _rulerDataT.Min;
      if (_rulerDataT.Min >= width / 4)
      {
        hideCursors();
        _rulerDataT.Min -= width / 4;
        _rulerDataT.Max -= width / 4;
        createZoomImg();
      }
    }

    private void _btnmoveRight_Click(object sender, RoutedEventArgs e)
    {
      double width = _rulerDataT.Max - _rulerDataT.Min;
      if (_rulerDataT.Max <= _wf.Duration - width / 4)
      {
        hideCursors();
        _rulerDataT.Max += width / 4;
        _rulerDataT.Min += width / 4;
        createZoomImg();
      }
    }
    private void _btnmoveUp_Click(object sender, RoutedEventArgs e)
    {
      double height = _rulerDataF.Max - _rulerDataF.Min;
      if (_rulerDataF.Max <= _wf.SamplingRate/2000 -  height / 4)
      {
        hideCursors();
        _rulerDataF.Min += height/ 4;
        _rulerDataF.Max += height / 4;
        createZoomImg();
      }
    }

    private void _btnmoveDown_Click(object sender, RoutedEventArgs e)
    {
      double height = _rulerDataF.Max - _rulerDataF.Min;
      if (_rulerDataF.Min >= height / 4)
      {
        hideCursors();
        _rulerDataF.Min -= height / 4;
        _rulerDataF.Max -= height / 4;
        createZoomImg();
      }
    }
    private void createZoomImg()
    {
      if (_rulerDataT.Max > _wf.Duration)
        _rulerDataT.Max = _wf.Duration;
      if (_rulerDataT.Min < 0)
        _rulerDataT.Min = 0;
      initRulerT(_rulerDataT.Min, _rulerDataT.Max);

      if (_rulerDataF.Max > _wf.SamplingRate / 2000)
        _rulerDataF.Max = _wf.SamplingRate / 2000;
      if (_rulerDataF.Min < 0)
        _rulerDataF.Min = 0;
      initRulerF(_rulerDataF.Min, _rulerDataF.Max);

      _wf.generateFtDiagram(_rulerDataT.Min, _rulerDataT.Max, BatInspector.AppParams.FftWidth);
      updateImage();
    }
    private void updateImage()
    {
      System.Drawing.Bitmap bmpFt = _wf.generateFtPicture(_rulerDataF.Min, _rulerDataF.Max);
      if (bmpFt != null)
      {
        BitmapImage bImg = ViewModel.Convert(bmpFt);
        _imgFt.Source = bImg;
      }
      System.Drawing.Bitmap bmpXt = _wf.generateXtPicture(_rulerDataA.Min, _rulerDataA.Max);
      if (bmpXt != null)
      {
        BitmapImage bImg = ViewModel.Convert(bmpXt);
        _imgXt.Source = bImg;
      }
    }


  }
}
