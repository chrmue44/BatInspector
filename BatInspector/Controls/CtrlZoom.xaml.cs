/********************************************************************************
 *               Author: Christian Müller
 *     Date of creation: 2023-08-18                                       
 *   Copyright (C) 2023: Christian Müller chrmue44(at)gmail(dot).com
 *
 *              Licence:  CC BY-NC 4.0 
 ********************************************************************************/

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using libParser;
using BatInspector.Properties;
using System;
using System.IO;
using System.Globalization;
using System.Collections.Generic;

namespace BatInspector.Controls
{
  /// <summary>
  /// Interaktionslogik für CtrlZoom.xaml
  /// </summary>
  public partial class CtrlZoom : UserControl
  {
    //  AnalysisFile _analysis;
    string _wavFilePath;
    ViewModel _model;
    int _stretch;
    int _oldCallIdx = -1;
    Image[] _playImgs;
    ctlWavFile _ctlWav = null;

    public CtrlZoom()
    {
      InitializeComponent();
      _playImgs = new Image[9];
      int size = 32;
      double op = 0.3;
      _playImgs[0] = new Image
      {
        Source = new BitmapImage(new Uri(@"pack://application:,,,/images/pause.png", UriKind.Absolute)),
        VerticalAlignment = VerticalAlignment.Center,
        Stretch = Stretch.Fill,
        Height = size,
        Width = size,
      };
      _playImgs[1] = new Image
      {
        Source = new BitmapImage(new Uri(@"pack://application:,,,/images/play-button.png", UriKind.Absolute)),
        VerticalAlignment = VerticalAlignment.Center,
        Stretch = Stretch.Fill,
        Height = size,
        Width = size
      };
      _playImgs[2] = new Image
      {
        Source = new BitmapImage(new Uri(@"pack://application:,,,/images/play-button-10x.png", UriKind.Absolute)),
        VerticalAlignment = VerticalAlignment.Center,
        Stretch = Stretch.Fill,
        Height = size,
        Width = size
      };
      _playImgs[3] = new Image
      {
        Source = new BitmapImage(new Uri(@"pack://application:,,,/images/play-button-20x.png", UriKind.Absolute)),
        VerticalAlignment = VerticalAlignment.Center,
        Stretch = Stretch.Fill,
        Height = 32,
        Width = 32
      };
      _playImgs[4] = new Image
      {
        Source = new BitmapImage(new Uri(@"pack://application:,,,/images/play-button.png", UriKind.Absolute)),
        VerticalAlignment = VerticalAlignment.Center,
        Stretch = Stretch.Fill,
        Height = size,
        Width = size,
        Opacity = op
      };
      _playImgs[5] = new Image
      {
        Source = new BitmapImage(new Uri(@"pack://application:,,,/images/play-button-10x.png", UriKind.Absolute)),
        VerticalAlignment = VerticalAlignment.Center,
        Stretch = Stretch.Fill,
        Height = size,
        Width = size,
        Opacity = op
      };
      _playImgs[6] = new Image
      {
        Source = new BitmapImage(new Uri(@"pack://application:,,,/images/play-button-20x.png", UriKind.Absolute)),
        VerticalAlignment = VerticalAlignment.Center,
        Stretch = Stretch.Fill,
        Height = 32,
        Width = 32,
        Opacity = op
      };
      _playImgs[7] = new Image
      {
        Source = new BitmapImage(new Uri(@"pack://application:,,,/images/play-het.png", UriKind.Absolute)),
        VerticalAlignment = VerticalAlignment.Center,
        Stretch = Stretch.Fill,
        Height = 32,
        Width = 32,
      };
      _playImgs[8] = new Image
      {
        Source = new BitmapImage(new Uri(@"pack://application:,,,/images/play-het.png", UriKind.Absolute)),
        VerticalAlignment = VerticalAlignment.Center,
        Stretch = Stretch.Fill,
        Height = 32,
        Width = 32,
        Opacity = op
      };
    }

    public void setup(AnalysisFile analysis, string wavFilePath, ViewModel model, 
                       List<string> species, ctlWavFile ctlWav)
    {
      int lblWidth = 100;
      InitializeComponent();
      _model = model;
      _ctlWav = ctlWav;
      _imgFt.Source = (ctlWav != null) ? ctlWav._img.Source : null;
      _model.ZoomView.Analysis = analysis;
      _model.ZoomView.Cursor1.set(0, 0, false);
      _model.ZoomView.Cursor2.set(0, 0, false);

      _freq1.setup(MyResources.Frequency + " [kHz]:", enDataType.DOUBLE, 1, lblWidth);
      _time1.setup(MyResources.PointInTime + "[s]:", enDataType.DOUBLE, 3, lblWidth);
      _freq2.setup(MyResources.Frequency + " [kHz]:", enDataType.DOUBLE, 1, lblWidth);
      _time2.setup(MyResources.PointInTime + " [s]:", enDataType.DOUBLE, 3, lblWidth);

      lblWidth = 110;
      _sampleRate.setup(MyResources.SamplingRate + " [kHz]", enDataType.DOUBLE, 1, lblWidth);
      _duration.setup(MyResources.Duration + " [s]", enDataType.DOUBLE, 3, lblWidth);
      _deltaT.setup(MyResources.DeltaT + " [ms]:", enDataType.DOUBLE, 1, lblWidth);
      _wavFilePath = wavFilePath;
      
      string fName = System.IO.Path.GetFileName(analysis.Name);
      _tbWavName.Text = fName;
      string wavName = File.Exists(fName) ? fName : _wavFilePath + "/" + fName;

      _model.ZoomView.initWaterfallDiagram(wavName);
      _duration.setValue(_model.ZoomView.Waterfall.Duration);
      _sampleRate.setValue((double)_model.ZoomView.Waterfall.SamplingRate / 1000);
      //_ctlRange.setup(MyResources.CtlZoomRange + " [dB]:", enDataType.DOUBLE, 0, 100, 80, true, rangeChanged);
      //_ctlRange.setValue(AppParams.Inst.GradientRange);
      SizeChanged += ctrlZoom_SizeChanged;
      MouseDown += ctrlZoomMouseDown;

      lblWidth = 130;
      _ctlSelectCall.setup(MyResources.CtlWavCall + " Nr.", 0, 45, 50, ctlSelCallChanged);
      _ctlSelectCall2.setup(MyResources.CtlWavCall + " Nr.", 0, 65, 50, ctlSelCallChanged2);
      _ctlFMin.setup(MyResources.Fmin, enDataType.DOUBLE, 1, lblWidth);
      _ctlFMax.setup(MyResources.Fmax, enDataType.DOUBLE, 1, lblWidth);
      _ctlFMaxAmpl.setup(MyResources.fMaxAmpl, enDataType.DOUBLE, 1, lblWidth);
      _ctlDuration.setup(MyResources.Duration + " [ms]: ", enDataType.DOUBLE, 1, lblWidth);
      //_ctlSnr.setup(MyResources.Snr + ": ", enDataType.DOUBLE, 1, lblWidth);
      _ctlDist.setup(MyResources.CtlZoomDistToPrev + " [ms]: ", enDataType.DOUBLE, 1, lblWidth);

      lblWidth = 110;
      _ctlSpecAuto.setup(MyResources.CtlZoomSpeciesAuto, enDataType.STRING, 1, lblWidth);
      _ctlSpecMan.setup(MyResources.CtlZoomSpeciesMan, 0, lblWidth, 85, ctlSpecManChanged);
      _ctlSpecMan.setItems(species.ToArray());
      _ctlProbability.setup(BatInspector.Properties.MyResources.CtrlZoomProbability, enDataType.DOUBLE, 2, lblWidth);
      _gbMean.Visibility = Visibility.Collapsed;
      _ctlMeanCallMin.setup(MyResources.CtrlZoomFirst, 1, 40, 40, calcMeanValues);
      _ctlMeanCallMax.setup(MyResources.CtrlZoomLast, 1, 40, 40, calcMeanValues);
      _ctlMeanDist.setup(MyResources.CtlZoomDistToPrev, enDataType.DOUBLE, 1, 130);
      _ctlMeanDuration.setup(MyResources.Duration + " [ms]: ", enDataType.DOUBLE, 1, 130);
      _ctlMeanFMin.setup(MyResources.Fmin, enDataType.DOUBLE, 1, 130);
      _ctlMeanFMax.setup(MyResources.Fmax, enDataType.DOUBLE, 1, 130);
      _ctlMeanFMaxAmpl.setup(MyResources.fMaxAmpl, enDataType.DOUBLE, 1, 130);

      int wt = 140;
      int wv = 130;
      _ctlDateTime.setup(BatInspector.Properties.MyResources.CtlZoomRecTime, enDataType.STRING, 0, wt);
      _ctlDateTime.setValue(_model.ZoomView.FileInfo.DateTime);
      _ctlGpsPos.setup(BatInspector.Properties.MyResources.CtlZoomPos, enDataType.STRING, 0, wt);
      _ctlGpsPos.setValue(_model.ZoomView.FileInfo.GPS.Position);
      _ctlGain.setup(BatInspector.Properties.MyResources.CtlZoomGain, enDataType.STRING, 0, wt);
      _ctlGain.setValue(_model.ZoomView.FileInfo.Gain);
      _ctlTrigLevel.setup(BatInspector.Properties.MyResources.CtlZoomTrigLevel, enDataType.STRING, 0, wt);
      _ctlTrigLevel.setValue(_model.ZoomView.FileInfo.Trigger.Level);
      _ctlTrigFilter.setup(BatInspector.Properties.MyResources.CtlZoomTrigFilt, enDataType.STRING, 0, wt);
      _ctlTrigFilter.setValue(_model.ZoomView.FileInfo.Trigger.Filter);
      _ctlTrigFiltFreq.setup(BatInspector.Properties.MyResources.CtlZoomTrigFilttFreq, enDataType.STRING, 0, wt);
      _ctlTrigFiltFreq.setValue(_model.ZoomView.FileInfo.Trigger.Frequency);

      _ctlSpectrum.init(_model.ZoomView.Spectrum, _model.ZoomView.Waterfall.SamplingRate / 2000);

      string[] items = new string[_model.ZoomView.Analysis.Calls.Count];
      if (_model.ZoomView.Analysis.Calls.Count > 0)
      {
        setVisabilityCallData(true);
        for (int i = 0; i < _model.ZoomView.Analysis.Calls.Count; i++)
          items[i] = _model.ZoomView.Analysis.Calls[i].getString(Cols.NR);  // (i + 1).ToString();
        _ctlSelectCall.setItems(items);
        _ctlSelectCall2.setItems(items);
        _ctlMeanCallMin.setItems(items);
        _ctlMeanCallMax.setItems(items);
        setupCallData(0);
        _ctlMeanCallMin.setValue("1");
        _ctlMeanCallMax.setValue(_model.ZoomView.Analysis.Calls.Count.ToString());
        calcMeanValues(0, 0);

      }
      else
      {
        setVisabilityCallData(false);
      }

      update();
      _btnZoomTotal_Click(null, null);

      _ctlTimeMin.setup("tMin[s]", enDataType.DOUBLE, 3, 50);
      _ctlTimeMax.setup("tMax[s]", enDataType.DOUBLE, 3, 50);

      _cbMode.Items.Clear();
      _cbMode.Items.Add(BatInspector.Properties.MyResources.CtlWavCall);
      _cbMode.Items.Add("Cursor");
      _cbMode.SelectedIndex = 0;
      _oldCallIdx = -1;
      _tbFreqHET.Text = ((int)(AppParams.Inst.FrequencyHET / 1000)).ToString();
    }

    void setVisabilityCallData(bool on)
    {
      Visibility vis = on ? Visibility.Visible : Visibility.Hidden;
      _ctlSelectCall.Visibility = vis;
      _ctlSelectCall2.Visibility = vis;
      _ctlSpectrum.Visibility = vis;
      _ctlTimeMin.Visibility = vis;
      _ctlTimeMax.Visibility = vis;
      _btnNext.Visibility = vis;
      _btnPrev.Visibility = vis;
      _ctlFMax.Visibility = vis;
      _ctlFMaxAmpl.Visibility = vis;
      _ctlFMin.Visibility = vis;
      //_ctlSnr.Visibility = vis;
      _ctlDist.Visibility = vis;
      _ctlDuration.Visibility = vis;
    }

    public void update()
    {
      if (_model != null)
      {
        _model.ZoomView.RulerDataA.setRange(-1, 1);
        _model.ZoomView.RulerDataT.setRange(0, _model.ZoomView.Waterfall.Duration);
        _model.ZoomView.RulerDataF.setRange(0, _model.ZoomView.Waterfall.SamplingRate / 2000);
        _model.ZoomView.Spectrum.RulerDataF.setRange(0, _model.ZoomView.Waterfall.SamplingRate / 2000);

        initRulerF();
        initRulerT();
        initRulerA();

        _ctlSelectCall._cb.SelectedIndex = 0;
        _ctlSelectCall2._cb.SelectedIndex = 0;
      }
    }

    private void ctlSpecManChanged(int index, string val)
    {

      if (_model.ZoomView.Analysis != null)
      {
        if ((_model.ZoomView.SelectedCallIdx >= 0) && (_model.ZoomView.SelectedCallIdx < _model.ZoomView.Analysis.Calls.Count))
          _model.ZoomView.Analysis.Calls[_model.ZoomView.SelectedCallIdx].setString(Cols.SPECIES_MAN, val);
        else
          DebugLog.log("ctlZoom.ctlSpecManChanged(): index error", enLogType.ERROR);
      }
    }

    public void rangeChanged(enDataType type, object val)
    {
      if (type == enDataType.DOUBLE)
      {
        double range = (double)val;
        _model.ZoomView.Waterfall.Range = range;
        updateRuler();
        updateImage();
      }
      else
        DebugLog.log("wrong data type for 'Range'", enLogType.ERROR);
    }


    public void setTimeLimits(double tMin, double tMax)
    {
      _ctlTimeMin.setValue(tMin);
      _ctlTimeMax.setValue(tMax);
    }

    public void updateManSpecies()
    {
      _ctlSpecMan.setValue(_model.ZoomView.Analysis.Calls[_model.ZoomView.SelectedCallIdx].getString(Cols.SPECIES_MAN));
    }

    private void _btnIncRange_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        _model.ZoomView.Waterfall.Range += 1.0;
        //_ctlRange.setValue(_model.ZoomView.Waterfall.Range);
        updateRuler();
        updateImage();
        DebugLog.log("Zoom:BTN 'increase range' clicked", enLogType.DEBUG);
      }
      catch (Exception ex)
      {
        DebugLog.log("Zoom:BTN 'increase range' failed: " + ex.ToString(), enLogType.ERROR);
      }
    }

    private void _btnDecRange_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        if (_model.ZoomView.Waterfall.Range > 3)
        {
          _model.ZoomView.Waterfall.Range -= 1.0;
          //_ctlRange.setValue(_model.ZoomView.Waterfall.Range);
          updateRuler();
          updateImage();
        }
        DebugLog.log("Zoom:BTN 'decrease range' clicked", enLogType.DEBUG);
      }
      catch (Exception ex)
      {
        DebugLog.log("Zoom:BTN 'decrease range' failed: " + ex.ToString(), enLogType.ERROR);
      }
    }

    private void ctrlZoomMouseDown(object sender, MouseEventArgs e)
    {
      try
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
            drawCursor(1);
            setFileInformations();
          }
        }
        if (e.RightButton == MouseButtonState.Pressed)
        {
          if (z.RulerDataF.check(f) && z.RulerDataT.check(t))
          {
            _model.ZoomView.Cursor2.set(t, f, true);
            drawCursor(2);
            setFileInformations();
          }
        }
        DebugLog.log("Zoom: 'Mouse down'", enLogType.DEBUG);
      }
      catch (Exception ex)
      {
        DebugLog.log("Zoom: 'Mouse down' failed: " + ex.ToString(), enLogType.ERROR);
      }
    }

    public void setFileInformations()
    {
      ZoomView z = _model.ZoomView;
      _sampleRate.setValue((double)_model.ZoomView.Waterfall.SamplingRate / 1000);
      _duration.setValue(_model.ZoomView.Analysis.getDouble(Cols.DURATION));
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


    private void drawCursor(int cursorNr)
    {
      Line lx = _cursorX1;
      Line ly = _cursorY1;
      Line la = _cursorA1;
      Cursor cursor = _model.ZoomView.Cursor1;
      if (cursorNr == 2)
      {
        lx = _cursorX2;
        ly = _cursorY2;
        la = _cursorA2;
        cursor = _model.ZoomView.Cursor2;
      }

      lx.Visibility = cursor.Visible ? Visibility.Visible : Visibility.Hidden;
      ly.Visibility = cursor.Visible ? Visibility.Visible : Visibility.Hidden;
      la.Visibility = cursor.Visible ? Visibility.Visible : Visibility.Hidden;

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

      la.X1 = x;
      la.Y1 = _imgXt.Margin.Top;
      la.X2 = x;
      la.Y2 = _imgXt.ActualHeight + _imgXt.Margin.Top;

      _ctlSpectrum.drawCursor(cursorNr, cursor);
    }

    private void ctrlZoom_SizeChanged(object sender, SizeChangedEventArgs e)
    {
      initRulerF();
      initRulerT();
      initRulerA();
      drawCursor(1);
      drawCursor(2);
    }

    private void hideCursors()
    {
      _model.ZoomView.Cursor1.hide();
      _model.ZoomView.Cursor2.hide();
      _grpCursor1.Visibility = Visibility.Hidden;
      _grpCursor2.Visibility = Visibility.Hidden;
      _deltaT.Visibility = Visibility.Hidden;
      drawCursor(1);
      drawCursor(2);
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
      }
      double y0 = _imgXt.Margin.Top + _imgXt.ActualHeight * 1 / 2;
      createText(_rulerA, _rulerA.ActualWidth - 40, y0 - 5, "0.0", Colors.Black);
      createText(_rulerA, _rulerA.ActualWidth - 40, _imgXt.Margin.Top - 5, rData.Max.ToString("0.##", CultureInfo.InvariantCulture), Colors.Black);
    }
    void initRulerF()
    {
      _rulerF.Children.Clear();
      int nrTicks = 9;
      double[] fTicks = ZoomView.createTicks(nrTicks, _model.ZoomView.RulerDataF);
      nrTicks = fTicks.Length;
      createLine(_rulerF, _rulerF.ActualWidth - 3, _imgFt.Margin.Top,
                          _rulerF.ActualWidth - 3, _imgFt.ActualHeight + _imgFt.Margin.Top, Brushes.Black);
      RulerData rData = _model.ZoomView.RulerDataF;
      double span = _model.ZoomView.RulerDataF.Max - _model.ZoomView.RulerDataF.Min;
      double min = _model.ZoomView.RulerDataF.Min;
      for (int i = 0; i < nrTicks; i++)
      {
        double y = _imgFt.ActualHeight - (_imgFt.Margin.Top + (fTicks[i] - min) / span * _imgFt.ActualHeight);
        createLine(_rulerF, _rulerF.ActualWidth - 3, y,
                            _rulerF.ActualWidth - nrTicks, y, Brushes.Black);
        string str = fTicks[i].ToString("0.#", CultureInfo.InvariantCulture);
        createText(_rulerF, _rulerF.ActualWidth - 37, y - 9, str, Colors.Black);
      }
    }

    void initRulerT()
    {
      _rulerT.Children.Clear();
      int nrTicks = 9;
      double[] tTicks = ZoomView.createTicks(nrTicks, _model.ZoomView.RulerDataT);
      nrTicks = tTicks.Length;
      createLine(_rulerT, 0, 3, _rulerT.ActualWidth, 3, Brushes.Black);
      RulerData rData = _model.ZoomView.RulerDataT;
      double span = _model.ZoomView.RulerDataT.Max - _model.ZoomView.RulerDataT.Min;
      double min = _model.ZoomView.RulerDataT.Min;
      for (int i = 0; i < nrTicks; i++)
      {
        double x = _rulerT.ActualWidth * (tTicks[i] - min) / span;
        createLine(_rulerT, x, 3, x, 10, Brushes.Black);
        string str = tTicks[i].ToString("0.###", CultureInfo.InvariantCulture);
        createText(_rulerT, x - 20, 15, str, Colors.Black);
      }
    }


    public static void createLine(Canvas ca, double x1, double y1, double x2, double y2, Brush brush, int thickness = 1)
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

    public static void createText(Canvas can, double x, double y, string text, Color color)
    {
      TextBlock textBlock = new TextBlock();
      textBlock.Text = text;
      textBlock.Foreground = new SolidColorBrush(color);
      textBlock.TextAlignment = TextAlignment.Right;
      Canvas.SetLeft(textBlock, x);
      Canvas.SetTop(textBlock, y);
      can.Children.Add(textBlock);
    }

    public void tick(double ms)
    {
      if ((_model != null) && (_model.ZoomView != null))
      {
        if (_model.ZoomView.Waterfall.PlaybackState == NAudio.Wave.PlaybackState.Playing)
        {
          double val = _model.ZoomView.Waterfall.PlayPosition / (_model.ZoomView.RulerDataT.Max - _model.ZoomView.RulerDataT.Min) / Math.Abs(_stretch) * 100;
          _slider.Value = val;
        }
        else if (_model.ZoomView.Waterfall.PlaybackState != NAudio.Wave.PlaybackState.Paused)
        {
          _slider.Value = 0;
          _btnPlay_20.IsEnabled = true;
          _btnPlay_20.Content = _playImgs[3];
          _btnPlay_10.IsEnabled = true;
          _btnPlay_10.Content = _playImgs[2];
          _btnPlay_1.IsEnabled = true;
          _btnPlay_1.Content = _playImgs[1];
          _btnPlay_HET.IsEnabled = true;
          _btnPlay_HET.Content = _playImgs[7];
        }
        if (_model.ZoomView.RefreshZoomImg)
        {
          createZoomImg();
          _model.ZoomView.RefreshZoomImg = false;
        }

      }
      else
      {
        _slider.Value = 0;
        _btnPlay_20.IsEnabled = true;
        _btnPlay_20.Content = _playImgs[3];
        _btnPlay_10.IsEnabled = true;
        _btnPlay_10.Content = _playImgs[2];
        _btnPlay_1.IsEnabled = true;
        _btnPlay_1.Content = _playImgs[1];
        _btnPlay_HET.IsEnabled = true;
        _btnPlay_HET.Content = _playImgs[7];
      }
    }

    private void _btnZoomCursor_Click(object sender, RoutedEventArgs e)
    {
      try
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
          MessageBox.Show(MyResources.msgZoomNotPossible, MyResources.msgInformation, MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        DebugLog.log("ZoomBtn: 'zoom cursor' clicked", enLogType.DEBUG);
      }
      catch (Exception ex)
      {
        DebugLog.log("ZoomBtn: 'zoom cursor' failed: " + ex.ToString(), enLogType.ERROR);
      }
    }

    private void _btnZoomTotal_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        _model.ZoomView.RulerDataF.setRange(0, _model.ZoomView.Waterfall.SamplingRate / 2000);
        _model.ZoomView.RulerDataT.setRange(0, _model.ZoomView.Waterfall.Duration);
        hideCursors();
        createZoomImg();
        DebugLog.log("ZoomBtn: 'zoom total' clicked", enLogType.DEBUG);
      }
      catch (Exception ex)
      {
        DebugLog.log("ZoomBtn: 'zoom total' failed: " + ex.ToString(), enLogType.ERROR);
      }
    }

    private void _btnZoomInV_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        _model.ZoomView.zoomInV();
        hideCursors();
        createZoomImg();
        DebugLog.log("ZoomBtn: 'zoom in V' clicked", enLogType.DEBUG);
      }
      catch (Exception ex)
      {
        DebugLog.log("ZoomBtn: 'zoom in V' failed: " + ex.ToString(), enLogType.ERROR);
      }
    }

    private void _btnZoomOutV_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        _model.ZoomView.zoomOutV();
        hideCursors();
        createZoomImg();
        DebugLog.log("ZoomBtn: 'zoom out V' clicked", enLogType.DEBUG);
      }
      catch (Exception ex)
      {
        DebugLog.log("ZoomBtn: 'zoom out V' failed: " + ex.ToString(), enLogType.ERROR);
      }
    }

    private void _btnZoomInH_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        _model.ZoomView.zoomInH();
        hideCursors();
        createZoomImg();
        DebugLog.log("ZoomBtn: 'zoom in H' clicked", enLogType.DEBUG);
      }
      catch (Exception ex)
      {
        DebugLog.log("ZoomBtn: 'zoom in H' failed: " + ex.ToString(), enLogType.ERROR);
      }
    }

    private void _btnZoomOutH_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        _model.ZoomView.zoomOutH();
        hideCursors();
        createZoomImg();
        DebugLog.log("ZoomBtn: 'zoom out H' clicked", enLogType.DEBUG);
      }
      catch (Exception ex)
      {
        DebugLog.log("ZoomBtn: 'zoom out H' failed: " + ex.ToString(), enLogType.ERROR);
      }
    }

    private void _btnmoveLeft_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        if (_model.ZoomView.moveLeft())
        {
          hideCursors();
          createZoomImg();
        }
        DebugLog.log("ZoomBtn: 'move left' clicked", enLogType.DEBUG);
      }
      catch (Exception ex)
      {
        DebugLog.log("ZoomBtn: 'move left' failed: " + ex.ToString(), enLogType.ERROR);
      }
    }

    private void _btnmoveRight_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        if (_model.ZoomView.moveRight(_model.ZoomView.Waterfall.Duration))
        {
          hideCursors();
          createZoomImg();
        }
        DebugLog.log("ZoomBtn: 'move right' clicked", enLogType.DEBUG);
      }
      catch (Exception ex)
      {
        DebugLog.log("ZoomBtn: 'move right' failed: " + ex.ToString(), enLogType.ERROR);
      }
    }

    private void _btnmoveUp_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        if (_model.ZoomView.moveUp(_model.ZoomView.Waterfall.SamplingRate / 2000))
        {
          hideCursors();
          createZoomImg();
        }
        DebugLog.log("ZoomBtn: 'move up' clicked", enLogType.DEBUG);
      }
      catch (Exception ex)
      {
        DebugLog.log("ZoomBtn: 'move up' failed: " + ex.ToString(), enLogType.ERROR);
      }
    }

    private void _btnmoveDown_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        if (_model.ZoomView.moveDown(0))
        {
          hideCursors();
          createZoomImg();
        }
        DebugLog.log("ZoomBtn: 'move down' clicked", enLogType.DEBUG);
      }
      catch (Exception ex)
      {
        DebugLog.log("ZoomBtn: 'move down' failed: " + ex.ToString(), enLogType.ERROR);
      }
    }

    private void _cbModeChanged(object sender, SelectionChangedEventArgs e)
    {
      if (!_ctlSpectrum.InitFlag)
      {
        int idx = changeSpectrumMode(_cbMode.SelectedIndex);
        _cbMode.SelectedIndex = idx;
      }
      else
        _ctlSpectrum.InitFlag = false;
    }

    private void updateRuler()
    {
      _model.ZoomView.RulerDataT.limits(0, _model.ZoomView.Waterfall.Duration);
      initRulerT();

      _model.ZoomView.RulerDataF.limits(0, _model.ZoomView.Waterfall.SamplingRate / 2000);
      initRulerF();

      _model.ZoomView.RulerDataA.limits(-1, 1);
      initRulerA();
    }

    private void createZoomImg()
    {
      updateRuler();
      double tStart = _model.ZoomView.RulerDataT.Min;
      double tEnd = _model.ZoomView.RulerDataT.Max;
      double fMin = _model.ZoomView.RulerDataF.Min;
      double fMax = _model.ZoomView.RulerDataF.Max;
      int samplingRate = _model.ZoomView.Waterfall.SamplingRate;
      double tStartCall = _model.ZoomView.Analysis.getStartTime(_oldCallIdx);
      double tEndCall = _model.ZoomView.Analysis.getEndTime(_oldCallIdx);

      if (((tEndCall - tStartCall) > 0) && ((tEndCall - tStartCall) < 0.2))
        _ctlSpectrum.createFftImage(_model.ZoomView.Waterfall.Audio.Samples, tStartCall, tEndCall, fMin, fMax, samplingRate, _cbMode.SelectedIndex, AppParams.Inst.ZoomSpectrumLogarithmic);

      double dt = (double)AppParams.Inst.FftWidth / samplingRate;
      _model.ZoomView.Waterfall.generateFtDiagram(tStart - dt, tEnd - dt, AppParams.Inst.WaterfallWidth);
      updateImage();
    }

    private void updateImage()
    {
      System.Drawing.Bitmap bmpFt = _model.ZoomView.Waterfall.generateFtPicture(_model.ZoomView.RulerDataF.Min, _model.ZoomView.RulerDataF.Max);
      if (bmpFt != null)
      {
        BitmapImage bImg = ViewModel.Convert(bmpFt);
        _imgFt.Source = bImg;
      }
      updateXtImage();
    }

    private void updateXtImage()
    {
      System.Drawing.Bitmap bmpXt = _model.ZoomView.Waterfall.generateXtPicture(_model.ZoomView.RulerDataA.Min, _model.ZoomView.RulerDataA.Max,
                                                          _model.ZoomView.RulerDataT.Min, _model.ZoomView.RulerDataT.Max);
      if (bmpXt != null)
      {
        BitmapImage bImg = ViewModel.Convert(bmpXt);
        _imgXt.Source = bImg;
      }
    }

    private void _btnPlay_1_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        DebugLog.log("ZoomBtn: play 1' clicked", enLogType.DEBUG);
        if (_model.ZoomView.Waterfall.PlaybackState == NAudio.Wave.PlaybackState.Playing)
        {
          _btnPlay_1.Content = _playImgs[1];
          _model.ZoomView.Waterfall.pause();
        }
        else
        {
          play(1);
          _btnPlay_20.IsEnabled = false;
          _btnPlay_20.Content = _playImgs[6];
          _btnPlay_10.IsEnabled = false;
          _btnPlay_10.Content = _playImgs[5];
          _btnPlay_1.Content = _playImgs[0];
          _btnPlay_HET.IsEnabled = false;
          _btnPlay_HET.Content = _playImgs[8];

        }
      }
      catch (Exception ex)
      {
        DebugLog.log("ZoomBtn: 'play 1' failed: " + ex.ToString(), enLogType.ERROR);
      }
    }

    /*
    private void _btnPause_Click(object sender, RoutedEventArgs e)
    {
      _model.ZoomView.Waterfall.pause();
    }
    */
    private void play(int stretch)
    {
      _stretch = stretch;
      double pos = _slider.Value / 100.0 * _model.ZoomView.Waterfall.Duration;
      if (stretch < 0)
        _model.ZoomView.Waterfall.play_HET(AppParams.Inst.FrequencyHET, 
                                           _model.ZoomView.RulerDataT.Min, _model.ZoomView.RulerDataT.Max, pos);
      else
      _model.ZoomView.Waterfall.play(_stretch, _model.ZoomView.RulerDataT.Min, _model.ZoomView.RulerDataT.Max, pos);
    }

    private void _btnPlay_10_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        DebugLog.log("ZoomBtn: 'play 10' clicked", enLogType.DEBUG);
        if (_model.ZoomView.Waterfall.PlaybackState == NAudio.Wave.PlaybackState.Playing)
        {
          _btnPlay_10.Content = _playImgs[2];
          _model.ZoomView.Waterfall.pause();
        }
        else
        {
          play(10);
          _btnPlay_1.IsEnabled = false;
          _btnPlay_1.Content = _playImgs[4];
          _btnPlay_20.IsEnabled = false;
          _btnPlay_20.Content = _playImgs[6];
          _btnPlay_HET.IsEnabled = false;
          _btnPlay_HET.Content = _playImgs[8];
          _btnPlay_10.Content = _playImgs[0];
        }
      }
      catch (Exception ex)
      {
        DebugLog.log("ZoomBtn: 'play 10' failed: " + ex.ToString(), enLogType.ERROR);
      }
    }

    private void _btnPlay_20_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        DebugLog.log("ZoomView:BTN 'play 20' clicked", enLogType.DEBUG);
        if (_model.ZoomView.Waterfall.PlaybackState == NAudio.Wave.PlaybackState.Playing)
        {
          _btnPlay_20.Content = _playImgs[3];
          _model.ZoomView.Waterfall.pause();
        }
        else
        {
          play(20);
          _btnPlay_20.Content = _playImgs[0];
          _btnPlay_1.IsEnabled = false;
          _btnPlay_1.Content = _playImgs[4];
          _btnPlay_10.IsEnabled = false;
          _btnPlay_10.Content = _playImgs[5];
          _btnPlay_HET.IsEnabled = false;
          _btnPlay_HET.Content = _playImgs[8];
        }
      }
      catch (Exception ex)
      {
        DebugLog.log("ZoomBtn: 'play 20' failed: " + ex.ToString(), enLogType.ERROR);
      }
    }


    private void _btnPlay_HET_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        DebugLog.log("ZoomView:BTN 'play HET' clicked", enLogType.DEBUG);
        if (_model.ZoomView.Waterfall.PlaybackState == NAudio.Wave.PlaybackState.Playing)
        {
          _btnPlay_HET.Content = _playImgs[3];
          _model.ZoomView.Waterfall.pause();
        }
        else
        {
          play(-1);
          _btnPlay_HET.Content = _playImgs[0];
          _btnPlay_1.IsEnabled = false;
          _btnPlay_1.Content = _playImgs[4];
          _btnPlay_10.IsEnabled = false;
          _btnPlay_10.Content = _playImgs[5];
          _btnPlay_20.IsEnabled = false;
          _btnPlay_20.Content = _playImgs[6];
        }
      }
      catch (Exception ex)
      {
        DebugLog.log("ZoomBtn: 'play HET' failed: " + ex.ToString(), enLogType.ERROR);
      }
    }

    private void _imgFt_MouseMove(object sender, MouseEventArgs e)
    {
      try
      {
        Point p = e.GetPosition(_imgFt);
        double f = _model.ZoomView.RulerDataF.Min +
                   (_imgFt.ActualHeight - p.Y) / _imgFt.ActualHeight * (_model.ZoomView.RulerDataF.Max - _model.ZoomView.RulerDataF.Min);
        double t = _model.ZoomView.RulerDataT.Min +
        p.X / _imgFt.ActualWidth * (_model.ZoomView.RulerDataT.Max - _model.ZoomView.RulerDataT.Min);
        _imgFt.ToolTip = f.ToString("#.#", CultureInfo.InvariantCulture) + "[kHz]/" +
        t.ToString("#.###" + "[s]", CultureInfo.InvariantCulture);
        DebugLog.log("ZoomBtn: image Ft mouse move", enLogType.DEBUG);
      }
      catch (Exception ex)
      {
        DebugLog.log("ZoomBtn: image Ft mouse move failed: " + ex.ToString(), enLogType.ERROR);
      }
    }

    private void _btnStop_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        _model.ZoomView.Waterfall.stop();
        _slider.Value = 0;
        _btnPlay_20.IsEnabled = true;
        _btnPlay_10.IsEnabled = true;
        _btnPlay_1.IsEnabled = true;
        DebugLog.log("ZoomBtn: 'stop' clicked", enLogType.DEBUG);
      }
      catch (Exception ex)
      {
        DebugLog.log("ZoomBtn: 'stop' failed: " + ex.ToString(), enLogType.ERROR);
      }
    }

    private void ctlSelCallChanged(int index, string val)
    {
      try
      {
        int.TryParse(val, out int Val);
        int idx = _ctlSelectCall.getSelectedIndex();
        if ((idx != _oldCallIdx) && (idx >= 0))
        {
          changeCall(idx);
        }
        DebugLog.log("ZoomBtn: 'select call' changed", enLogType.DEBUG);
      }
      catch (Exception ex)
      {
        DebugLog.log("ZoomBtn: 'select call' failed: " + ex.ToString(), enLogType.ERROR);
      }
    }

    private void ctlSelCallChanged2(int index, string val)
    {
      try
      {
        int.TryParse(val, out int Val);
        int idx = _ctlSelectCall2.getSelectedIndex();
        if ((idx != _oldCallIdx) && (idx >= 0))
        {
          changeCall(idx);
        }
        DebugLog.log("ZoomBtn: 'select call' changed", enLogType.DEBUG);
      }
      catch (Exception ex)
      {
        DebugLog.log("ZoomBtn: 'select call' failed: " + ex.ToString(), enLogType.ERROR);
      }
    }

    public void changeCall(int idx)
    {
      _model.ZoomView.SelectedCallIdx = idx;
      string callNr = _model.ZoomView.Analysis.Calls[idx].getString(Cols.NR);
      _ctlSelectCall.setValue(callNr);
      _ctlSelectCall2.setValue(callNr);
      _oldCallIdx = idx;
      setupCallData(idx);
      double tStart = _model.ZoomView.Analysis.getStartTime(idx);
      double tEnd = _model.ZoomView.Analysis.getEndTime(idx);
      _ctlTimeMin.setValue(tStart);
      _ctlTimeMax.setValue(tEnd);
      int samplingRate = _model.ZoomView.Waterfall.SamplingRate;
      //_ctlSpectrum.createFftImage(_model.ZoomView.Waterfall.Samples, tStart, tEnd, samplingRate,_cbMode.SelectedIndex);
      _model.ZoomView.RulerDataF.setRange(0, samplingRate / 2000);
      double pre = 0.01;
      _model.ZoomView.RulerDataT.setRange(tStart - pre, tStart + AppParams.Inst.ZoomOneCall / 1000.0 - pre);
      hideCursors();
      createZoomImg();
      _cbZoomAmpl_Click(null, null);
    }
    public void calcMeanValues(int idx, object val)
    {
      int.TryParse(_ctlMeanCallMin.getValue(), out int min);
      min--;
      int.TryParse(_ctlMeanCallMax.getValue(), out int max);
      max--;
      if ((min >= 0) && (max < _model.ZoomView.Analysis.Calls.Count))
      {
        double fMin = 0;
        double fMax = 0;
        double fMaxAmpl = 0;
        double duration = 0;
        double callDist = 0;
        int count = max - min + 1;
        int countDistPrev = count;
        if (min == 0)
          countDistPrev = count - 1;
        {
          for (int i = min; i <= max; i++)
          {
            fMin += _model.ZoomView.Analysis.Calls[i].getDouble(Cols.F_MIN) / count;
            fMax += _model.ZoomView.Analysis.Calls[i].getDouble(Cols.F_MAX) / count;
            fMaxAmpl += _model.ZoomView.Analysis.Calls[i].getDouble(Cols.F_MAX_AMP) / count;
            duration += _model.ZoomView.Analysis.Calls[i].getDouble(Cols.DURATION) / count;
            callDist += _model.ZoomView.Analysis.Calls[i].DistToPrev / countDistPrev;
          }
          _ctlMeanDist.setValue(callDist);
          _ctlMeanFMax.setValue(fMax / 1000);
          _ctlMeanFMin.setValue(fMin / 1000);
          _ctlMeanFMaxAmpl.setValue(fMaxAmpl / 1000);
          _ctlMeanDuration.setValue(duration);
        }
      }
    }

    private void setupCallData(int idx)
    {
      if (idx < _model.ZoomView.Analysis.Calls.Count)
      {
        AnalysisCall call = _model.ZoomView.Analysis.Calls[idx];
        _ctlFMin.setValue(call.getDouble(Cols.F_MIN) / 1000);
        _ctlFMax.setValue(call.getDouble(Cols.F_MAX) / 1000);
        _ctlFMaxAmpl.setValue(call.getDouble(Cols.F_MAX_AMP) / 1000);
        _ctlDuration.setValue(call.getDouble(Cols.DURATION));
        _ctlDist.setValue(call.DistToPrev);
        _ctlSpecAuto.setValue(call.getString(Cols.SPECIES));
        _ctlProbability.setValue(call.getDouble(Cols.PROBABILITY));
        _ctlSpecMan.setValue(call.getString(Cols.SPECIES_MAN));
      }
    }

    private int changeSpectrumMode(int mode)
    {
      int retVal = 0;
      ZoomView z = _model.ZoomView;
      double tStart = _model.ZoomView.Analysis.getStartTime(_oldCallIdx);
      double tEnd = _model.ZoomView.Analysis.getEndTime(_oldCallIdx);
      double fMin = _model.ZoomView.RulerDataF.Min;
      double fMax = _model.ZoomView.RulerDataF.Max;
      switch (mode)
      {
        case 0:
          {
            _ctlTimeMin.setValue(tStart);
            _ctlTimeMax.setValue(tEnd);
            _ctlSpectrum.createFftImage(_model.ZoomView.Waterfall.Audio.Samples, tStart, tEnd, fMin, fMax,
                                        _model.ZoomView.Waterfall.SamplingRate, _cbMode.SelectedIndex, AppParams.Inst.ZoomSpectrumLogarithmic);
          }
          break;

        case 1:
          if (z.Cursor1.Visible && z.Cursor2.Visible)
          {
            _ctlTimeMin.setValue(z.Cursor1.Time);
            _ctlTimeMax.setValue(z.Cursor2.Time);
            _ctlSpectrum.createFftImage(_model.ZoomView.Waterfall.Audio.Samples, z.Cursor1.Time, z.Cursor2.Time, fMin, fMax,
                                        _model.ZoomView.Waterfall.SamplingRate, _cbMode.SelectedIndex, AppParams.Inst.ZoomSpectrumLogarithmic);
            retVal = 1;
          }
          else
            MessageBox.Show(MyResources.msgZoomNotPossible, MyResources.msgInformation, MessageBoxButton.OK, MessageBoxImage.Warning);
          break;
      }
      return retVal;
    }

    private void _btnPrev_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        int idx = _oldCallIdx - 1;
        if ((idx >= 0) && (idx < _model.ZoomView.Analysis.Calls.Count))
        {
          changeCall(idx);
          string callNr = _model.ZoomView.Analysis.Calls[idx].getString(Cols.NR);
          _ctlSelectCall.setValue(callNr);
          _ctlSelectCall2.setValue(callNr);
        }
        DebugLog.log("ZoomBtn: 'previous' clicked", enLogType.DEBUG);
      }
      catch (Exception ex)
      {
        DebugLog.log("ZoomBtn: 'previous' failed: " + ex.ToString(), enLogType.ERROR);
      }
    }

    private void _btnNext_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        int idx = _oldCallIdx + 1;
        if ((idx >= 0) && (idx < _model.ZoomView.Analysis.Calls.Count))
        {
          changeCall(idx);
          string callNr = _model.ZoomView.Analysis.Calls[idx].getString(Cols.NR);
          _ctlSelectCall.setValue(callNr);
          _ctlSelectCall2.setValue(callNr);
        }
        DebugLog.log("ZoomBtn: 'next' clicked", enLogType.DEBUG);
      }
      catch (Exception ex)
      {
        DebugLog.log("ZoomBtn: 'next' failed: " + ex.ToString(), enLogType.ERROR);
      }
    }

    private void _cbZoomAmpl_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        if (_cbZoomAmpl.IsChecked == true)
          _model.ZoomView.findMaxAmplitude();
        else
          _model.ZoomView.RulerDataA.setRange(-1.0, 1.0);
        updateXtImage();
        initRulerA();
        DebugLog.log("ZoomBtn: 'Zoom Amplitude' clicked", enLogType.DEBUG);
      }
      catch (Exception ex)
      {
        DebugLog.log("ZoomBtn: 'Toom Amplitude: " + ex.ToString(), enLogType.ERROR);
      }

    }

    private void _imgXt_MouseMove(object sender, MouseEventArgs e)
    {
      try
      {
        Point pos = e.GetPosition(_imgXt);
        double t = _model.ZoomView.RulerDataT.Min +
        pos.X / _imgXt.ActualWidth * (_model.ZoomView.RulerDataT.Max - _model.ZoomView.RulerDataT.Min);
        _imgXt.ToolTip = t.ToString("#.###" + "[s]", CultureInfo.InvariantCulture);
        DebugLog.log("Zoom: image Xt 'mouse move'", enLogType.DEBUG);
      }
      catch (Exception ex)
      {
        DebugLog.log("Zoom: image Xt 'mouse move' failed: " + ex.ToString(), enLogType.ERROR);
      }
    }

    private void _btnSaveAs_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        System.Windows.Forms.SaveFileDialog dlg = new System.Windows.Forms.SaveFileDialog();
        dlg.Filter = "WAV files (*.wav)|*.wav";
        System.Windows.Forms.DialogResult res = dlg.ShowDialog();
        if (res == System.Windows.Forms.DialogResult.OK)
        {
          WavFile wav = new WavFile();
          int iStart = (int)(_model.ZoomView.RulerDataT.Min * _model.ZoomView.Waterfall.SamplingRate);
          int iEnd = (int)(_model.ZoomView.RulerDataT.Max * _model.ZoomView.Waterfall.SamplingRate);
          wav.createFile(1, _model.ZoomView.Waterfall.SamplingRate, iStart, iEnd, _model.ZoomView.Waterfall.Audio.Samples);
          wav.saveFileAs(dlg.FileName);
        }
        DebugLog.log("Zoom:Btn 'save As' clicked", enLogType.DEBUG);
      }
      catch (Exception ex)
      {
        DebugLog.log("Zoom:Btn 'save As' failed: " + ex.ToString(), enLogType.ERROR);
      }
    }

    private void _btnSave_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        _model.ZoomView.Waterfall.Audio.saveAs(_model.ZoomView.Waterfall.WavName, 
                                               _model.Prj.WavSubDir);
        string pngName = _model.ZoomView.Waterfall.WavName.ToLower().Replace(AppParams.EXT_WAV, AppParams.EXT_IMG);
        if (_ctlWav != null)
        {
          // delete ixisting PNG to force creation of a new one
          if (File.Exists(pngName))
            File.Delete(pngName);
          _ctlWav._img.Source = _model.getFtImage(_model.ZoomView.Waterfall.WavName);
        }
        DebugLog.log("Zoom:Btn 'save' clicked", enLogType.DEBUG);
      }
      catch (Exception ex)
      {
        DebugLog.log("Zoom:Btn 'save' failed: " + ex.ToString(), enLogType.ERROR);
      }
    }


    private void _btnBandpass_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        if ((_model.ZoomView.Cursor1.Visible) && (_model.ZoomView.Cursor2.Visible))
        {
          ZoomView.saveWavBackup(_model.ZoomView.Waterfall.WavName, _model.Prj.WavSubDir);
          double fMin = _model.ZoomView.Cursor1.Freq * 1000;
          double fMax = _model.ZoomView.Cursor2.Freq * 1000;
          _model.ZoomView.applyBandpass(fMin, fMax);
          hideCursors();
          createZoomImg();
        }
        else
          MessageBox.Show(MyResources.msgZoomNotPossible, MyResources.msgInformation, MessageBoxButton.OK, MessageBoxImage.Warning);
        DebugLog.log("Zoom:Btn 'Bandpass' clicked", enLogType.DEBUG);
      }
      catch (Exception ex)
      {
        DebugLog.log("Zoom:Btn 'Bandpass' failed: " + ex.ToString(), enLogType.ERROR);
      }
    }

    private void _btnReduceNoise_Click(object sender, RoutedEventArgs e)
    {
        DebugLog.log("Zoom:Btn 'ReduceNoise' clicked", enLogType.DEBUG);
        _model.ZoomView.reduceNoise();
    }

    private void _btnCutOut_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        if ((_model.ZoomView.Cursor1.Visible) && (_model.ZoomView.Cursor2.Visible))
        {
          ZoomView.saveWavBackup(_model.ZoomView.Waterfall.WavName, _model.Prj.WavSubDir);
          double tMin = _model.ZoomView.Cursor1.Time;
          double tMax = _model.ZoomView.Cursor2.Time;
          _model.ZoomView.removeSection(tMin, tMax);
          hideCursors();
          createZoomImg();
          update();
        }
        else
          MessageBox.Show(MyResources.msgZoomNotPossible, MyResources.msgInformation, MessageBoxButton.OK, MessageBoxImage.Warning);
        DebugLog.log("Zoom:Btn 'Cutout' clicked", enLogType.DEBUG);
      }
      catch (Exception ex)
      {
        DebugLog.log("Zoom:Btn 'Cutout' failed: " + ex.ToString(), enLogType.ERROR);
      }

    }

    private void _btnNormalize_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        _model.ZoomView.normalize();
        createZoomImg();
        DebugLog.log("Zoom:Btn 'Normalize' clicked", enLogType.DEBUG);
      }
      catch (Exception ex)
      {
        DebugLog.log("Zoom:Btn 'Normalize' failed: " + ex.ToString(), enLogType.ERROR);
      }
    }

    private void _btnUndo_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        _model.ZoomView.undoChanges();
        createZoomImg();
        DebugLog.log("Zoom:Btn 'Undo' clicked", enLogType.DEBUG);
      }
      catch (Exception ex)
      {
        DebugLog.log("Zoom:Btn 'und' failed: " + ex.ToString(), enLogType.ERROR);
      }
    }

    private void _tbFreHET_TextChanged(object sender, TextChangedEventArgs e)
    {
      int fHet = 0;
      bool ok = int.TryParse(_tbFreqHET.Text, out fHet);
      if(ok && _model.ZoomView.Waterfall != null)
      {
        double f = fHet * 1000;
        if (fHet < _model.ZoomView.Waterfall.SamplingRate / 2)
          AppParams.Inst.FrequencyHET = f;
      }
    }
  }
}
