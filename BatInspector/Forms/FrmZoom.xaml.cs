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
    RulerData _rulerDataX;
    RulerData _rulerDataY;
    string _wavFilePath;
    Waterfall _wf;

    public FrmZoom(string name, AnalysisFile analysis, string wavFilePath)
    {
      InitializeComponent();
      this.Title = name;
      _analysis = analysis;
      _cursor1 = new Cursor();
      _cursor2 = new Cursor();
      _rulerDataX = new RulerData();
      _rulerDataY = new RulerData();
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
      Point p = e.GetPosition(_img);
      double f = (1.0 - (double)(p.Y) / ((double)_img.ActualHeight)) * (_rulerDataY.Max - _rulerDataY.Min) + _rulerDataY.Min;
      double t = (double)(p.X - _img.Margin.Left) / (double)_img.ActualWidth * (_rulerDataX.Max - _rulerDataX.Min) + _rulerDataX.Min;
      if (e.LeftButton == MouseButtonState.Pressed)
      {
        if ((f <= _rulerDataY.Max) && (f >= _rulerDataY.Min) && (t >= _rulerDataX.Min) && (t <= _rulerDataX.Max))
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
        if ((f <= _rulerDataY.Max) && (f >= _rulerDataY.Min) && (t >= _rulerDataX.Min) && (t <= _rulerDataX.Max))
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
      int x = (int)(_img.Margin.Left + (cursor.Time - _rulerDataX.Min) /(_rulerDataX.Max - _rulerDataX.Min) * _img.ActualWidth);
      int y = (int)(_img.Margin.Top + (1.0 - (cursor.Freq - _rulerDataY.Min) / (_rulerDataY.Max - _rulerDataY.Min)) * _img.ActualHeight);
      lx.X1 = x;
      lx.Y1 = _img.Margin.Top;
      lx.X2 = x;
      lx.Y2 = _img.ActualHeight + _img.Margin.Top;
      ly.X1 = _img.Margin.Left;
      ly.Y1 = y;
      ly.X2 = _img.ActualWidth + _img.Margin.Left;
      ly.Y2 = y;
    }

    private void FrmZoom_SizeChanged(object sender, SizeChangedEventArgs e)
    {
      initRulerY(_rulerDataY.Min, _rulerDataY.Max);
      initRulerX(_rulerDataX.Min, _rulerDataX.Max);
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
      initRulerY(0, _analysis.SampleRate/2000);
      initRulerX(0, _analysis.Duration);
    }

    void initRulerY(double min, double max)
    {
      _rulerY.Children.Clear();
      _rulerDataY.Min = min;
      _rulerDataY.Max = max;
      createLine(_rulerY, _rulerY.ActualWidth - 3, _img.Margin.Top,
                          _rulerY.ActualWidth - 3, _img.ActualHeight + _img.Margin.Top, Brushes.Black);
      for(int i = 0; i <= 10; i++)
      {
        double y = _img.Margin.Top +_img.ActualHeight * i / 10;
        createLine(_rulerY, _rulerY.ActualWidth - 3, y,
                            _rulerY.ActualWidth - 10, y, Brushes.Black);
        string str = ((max - min) * (10 - i) / 10 + min).ToString("0.#");
        createText(_rulerY, _rulerY.ActualWidth - 35, y - 5, str, Colors.Black);
      }
    }

    void initRulerX(double min, double max)
    {
      _rulerX.Children.Clear();
      _rulerDataX.Min = min;
      _rulerDataX.Max = max;
      createLine(_rulerX, 0, 3, _rulerX.ActualWidth, 3, Brushes.Black);
      for (int i = 0; i <= 10; i++)
      {
        createLine(_rulerX, _rulerX.ActualWidth * i / 10, 3, _rulerX.ActualWidth * i / 10, 10, Brushes.Black);
        string str = ((max - min) *  i / 10 + min).ToString("0.###");
        createText(_rulerX, _rulerX.ActualWidth * i / 10 - 20, 15, str, Colors.Black);
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
        _rulerDataY.Min = Math.Min(_cursor1.Freq, _cursor2.Freq);
        _rulerDataY.Max = Math.Max(_cursor1.Freq, _cursor2.Freq);
        _rulerDataX.Min = Math.Min(_cursor1.Time, _cursor2.Time);
        _rulerDataX.Max = Math.Max(_cursor1.Time, _cursor2.Time);
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
      _rulerDataY.Min = 0;
      _rulerDataY.Max = _wf.SamplingRate / 2000;
      _rulerDataX.Min = 0;
      _rulerDataX.Max = _wf.Duration;
      hideCursors();
      createZoomImg();
    }

    private void _btnZoomInV_Click(object sender, RoutedEventArgs e)
    {
      _rulerDataY.Max = (_rulerDataY.Max - _rulerDataY.Min) / 2 + _rulerDataY.Min;
      hideCursors();
      createZoomImg();
    }

    private void _btnZoomOutV_Click(object sender, RoutedEventArgs e)
    {
      _rulerDataY.Max = (_rulerDataY.Max - _rulerDataY.Min) * 2 + _rulerDataY.Min;
      hideCursors();
      createZoomImg();
    }

    private void _btnZoomInH_Click(object sender, RoutedEventArgs e)
    {
      _rulerDataX.Max = (_rulerDataX.Max - _rulerDataX.Min)/ 2 + _rulerDataX.Min;
      hideCursors();
      createZoomImg();
    }

    private void _btnZoomOutH_Click(object sender, RoutedEventArgs e)
    {
      _rulerDataX.Max = (_rulerDataX.Max - _rulerDataX.Min) * 2 + _rulerDataX.Min;
      hideCursors();
      createZoomImg();
    }

    private void _btnmoveLeft_Click(object sender, RoutedEventArgs e)
    {
      double width = _rulerDataX.Max - _rulerDataX.Min;
      if (_rulerDataX.Min >= width / 4)
      {
        hideCursors();
        _rulerDataX.Min -= width / 4;
        _rulerDataX.Max -= width / 4;
        createZoomImg();
      }
    }

    private void _btnmoveRight_Click(object sender, RoutedEventArgs e)
    {
      double width = _rulerDataX.Max - _rulerDataX.Min;
      if (_rulerDataX.Max <= _wf.Duration - width / 4)
      {
        hideCursors();
        _rulerDataX.Max += width / 4;
        _rulerDataX.Min += width / 4;
        createZoomImg();
      }
    }
    private void _btnmoveUp_Click(object sender, RoutedEventArgs e)
    {
      double height = _rulerDataY.Max - _rulerDataY.Min;
      if (_rulerDataY.Max <= _wf.SamplingRate/2000 -  height / 4)
      {
        hideCursors();
        _rulerDataY.Min += height/ 4;
        _rulerDataY.Max += height / 4;
        createZoomImg();
      }
    }

    private void _btnmoveDown_Click(object sender, RoutedEventArgs e)
    {
      double height = _rulerDataY.Max - _rulerDataY.Min;
      if (_rulerDataY.Min >= height / 4)
      {
        hideCursors();
        _rulerDataY.Min -= height / 4;
        _rulerDataY.Max -= height / 4;
        createZoomImg();
      }
    }
    private void createZoomImg()
    {
      if (_rulerDataX.Max > _wf.Duration)
        _rulerDataX.Max = _wf.Duration;
      if (_rulerDataX.Min < 0)
        _rulerDataX.Min = 0;
      initRulerX(_rulerDataX.Min, _rulerDataX.Max);

      if (_rulerDataY.Max > _wf.SamplingRate / 2000)
        _rulerDataY.Max = _wf.SamplingRate / 2000;
      if (_rulerDataY.Min < 0)
        _rulerDataY.Min = 0;
      initRulerY(_rulerDataY.Min, _rulerDataY.Max);

      _wf.generateDiagram(_rulerDataX.Min, _rulerDataX.Max, BatInspector.AppParams.FftWidth);
      updateImage();
    }
    private void updateImage()
    {
      System.Drawing.Bitmap bmp = _wf.generatePicture(_rulerDataY.Min, _rulerDataY.Max);
      if (bmp != null)
      {
        BitmapImage bImg = ViewModel.Convert(bmp);
        _img.Source = bImg;
      }
    }

 
  }
}
