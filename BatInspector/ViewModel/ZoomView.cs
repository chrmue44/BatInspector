/********************************************************************************
 *               Author: Christian Müller
 *      Date of cration: 2021-08-10                                       
 *   Copyright (C) 2022: christian Müller christian(at)chrmue(dot).de
 *
 *              Licence:
 * 
 * THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND,
 * EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
 ********************************************************************************/

using System;
using System.IO;
using System.Windows.Input;
using System.Xml.Serialization;

namespace BatInspector
{
  public class RulerData
  {
    double _min = 0;
    double _max = 100;
    public double Min { get { return _min; } }
    public double Max { get { return _max; } }

    public bool check(double x)
    {
      return ((Min <= x) && (x <= Max));
    }

    public void setRange(double min, double max)
    {
      _min = Math.Min(min, max);
      _max = Math.Max(min, max);
    }

    public void inc(double percent, double max)
    {
      double delta = _max - Min;
      _min += delta * percent;
      _max += delta * percent;
      if(Max > max)
      {
        _max = max;
        _min = Max - delta;
      }
    }
    public void decr(double percent, double min)
    {
      double delta = _max - Min;
      _min -= delta * percent;
      _max -= delta * percent;
      if (Min < min)
      {
        _min = min;
        _max = _min + delta;
      }
    }

    public void limits(double min, double max)
    {
      if (_max > max)
        _max = max;
      if (_min < min)
        _min = min;
    }
  }

  public class Cursor
  {
    double _freq = 0;
    double _time = 0;
    bool _visible = false;

    public double Freq { get { return _freq; } }
    public double Time { get { return _time; } }
    public bool Visible { get { return _visible; } }

    public void set(double t, double f, bool visible)
    {
      _freq = f;
      _time = t;
      _visible = visible;
    }

    public void hide()
    {
      _visible = false;
    }
  }

  public class ZoomView
  {
    RulerData _rulerDataT;
    RulerData _rulerDataF;
    RulerData _rulerDataA;
    Cursor _cursor1;
    Cursor _cursor2;
    Spectrum _spectrum;
    Waterfall _wf = null;
    ColorTable _colorTable;
    BatRecord _fileInfo;
    AnalysisFile _analysis;


    public ZoomView(ColorTable colorTable)
    {
      _colorTable = colorTable;
      _rulerDataT = new RulerData();
      _rulerDataF = new RulerData();
      _rulerDataA = new RulerData();
      _cursor1 = new Cursor();
      _cursor2 = new Cursor();
      _spectrum = new Spectrum();
      _fileInfo = new BatRecord();
      _analysis = null;
    }


    public RulerData RulerDataT { get { return _rulerDataT; } }
    public RulerData RulerDataF { get { return _rulerDataF; } }
    public RulerData RulerDataA { get { return _rulerDataA; } }
    public Cursor Cursor1 { get { return _cursor1; } }
    public Cursor Cursor2 { get { return _cursor2; } }
    public Spectrum Spectrum { get { return _spectrum; } }
    public BatRecord FileInfo {  get { return  _fileInfo; } }

    public Waterfall Waterfall {  get { return _wf; } }

    public AnalysisFile Analysis { get { return _analysis; } set { _analysis = value; } }

    public int SelectedCallIdx { get; set; }

    public void initWaterfallDiagram(string wavName, uint fftSize, int w, int h)
    {
      _wf = new Waterfall(wavName, fftSize, w, h, _colorTable);
      string infoName = wavName.Replace(AppParams.EXT_WAV, AppParams.EXT_INFO);
      _fileInfo = ElekonInfoFile.read(infoName);
    }

    public void zoomInV()
    {
      double max = (_rulerDataF.Max - _rulerDataF.Min) / 2 + _rulerDataF.Min;
      double min = _rulerDataF.Min;
      _rulerDataF.setRange(min, max);
    }
    public void zoomOutV()
    {
      double max = (_rulerDataF.Max - _rulerDataF.Min) * 2 + _rulerDataF.Min;
      double min = _rulerDataF.Min;
      _rulerDataF.setRange(min, max);
    }

    public void zoomInH()
    {
      double max = (_rulerDataT.Max - _rulerDataT.Min) / 2 + _rulerDataT.Min;
      double min = _rulerDataT.Min;
      _rulerDataT.setRange(min, max);
    }
    public void zoomOutH()
    {
      double max = (_rulerDataT.Max - _rulerDataT.Min) * 2 + _rulerDataT.Min;
      double min = _rulerDataT.Min;
      _rulerDataT.setRange(min, max);
    }

    public bool moveLeft()
    {
      bool retVal = false;
      if (_rulerDataT.Min > 0)
      {
        retVal = true;
        _rulerDataT.decr(0.25, 0);
      }
      return retVal;
    }

    public bool moveRight(double max)
    {
      bool retVal = false;
      if (_rulerDataT.Max < max)
      {
        retVal = true;
        _rulerDataT.inc(0.25, max);
      }
      return retVal;
    }

    public bool moveUp(double max)
    {
      bool retVal = false;
      if (_rulerDataF.Max < max)
      {
        retVal = true;
        _rulerDataF.inc(0.25, max);
      }
      return retVal;
    }

    public bool moveDown(double min)
    {
      bool retVal = false;
      if (_rulerDataF.Min > min)
      {
        retVal = true;
        _rulerDataF.decr(0.25, min);
      }
      return retVal;
    }



    public void findMaxAmplitude()
    {
      if(_wf != null)
      {
        int iMin = (int)(_rulerDataT.Min * _wf.SamplingRate);
        int iMax = (int)(_rulerDataT.Max * _wf.SamplingRate);
        double max = 0;
        for(int i = iMin; i < iMax; i++)
        {
          double abs = Math.Abs(_wf.Audio.Samples[i]);
          if (max < abs)
            max = abs;
        }
        _rulerDataA.setRange(-max, max);
      }
    }

    public void applyBandpass(double fMin, double fMax)
    {
      _wf.Audio.FftForward();
      _wf.Audio.bandpass(fMin, fMax);
      _wf.Audio.FftBackward();
    }

    public void undoChanges()
    {
      _wf.Audio.undo();
    }

    public void denoise()
    {
        _wf.Audio.FftForward();
        _wf.Audio.reduceNoise(-30);
        _wf.Audio.FftBackward();
    }
  }
}