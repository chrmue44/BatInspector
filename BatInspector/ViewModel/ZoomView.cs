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
    }


    public RulerData RulerDataT { get { return _rulerDataT; } }
    public RulerData RulerDataF { get { return _rulerDataF; } }
    public RulerData RulerDataA { get { return _rulerDataA; } }
    public Cursor Cursor1 { get { return _cursor1; } }
    public Cursor Cursor2 { get { return _cursor2; } }
    public Spectrum Spectrum { get { return _spectrum; } }
    public BatRecord FileInfo {  get { return  _fileInfo; } }

    public Waterfall Waterfall {  get { return _wf; } }

    public void initWaterfallDiagram(string wavName, uint fftSize, int w, int h, AppParams settings)
    {
      _wf = new Waterfall(wavName, fftSize, w, h, settings, _colorTable);
      string infoName = wavName.Replace(".wav", ".xml");
      if (File.Exists(infoName))
      {
        string xml = File.ReadAllText(infoName);
        var serializer = new XmlSerializer(typeof(BatRecord));
        TextReader reader = new StringReader(xml);
        _fileInfo = (BatRecord)serializer.Deserialize(reader);
      }
      initUninitializedValues();
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

    //TODO: very ugly: find a better way to do this
    private void initUninitializedValues()
    {
      if (_fileInfo.DateTime == null)
        _fileInfo.DateTime = "";
      if (_fileInfo.Duration == null)
        _fileInfo.Duration = "";
      if (_fileInfo.FileName == null)
        _fileInfo.FileName = "";
      if (_fileInfo.Gain == null)
        _fileInfo.Gain = "";
      if (_fileInfo.GPS == null)
        _fileInfo.GPS = new BatRecordGPS();
      if (_fileInfo.GPS.Position == null)
        _fileInfo.GPS.Position = "";
      if (_fileInfo.InputFilter == null)
        _fileInfo.InputFilter = "";
      if (_fileInfo.PeakValue == null)
        _fileInfo.PeakValue = "";
      if (_fileInfo.Samplerate == null)
        _fileInfo.Samplerate = "";
      if (_fileInfo.Trigger == null)
        _fileInfo.Trigger = new BatRecordTrigger();
      if (_fileInfo.Trigger.Filter == null)
        _fileInfo.Trigger.Filter = "";
      if (_fileInfo.Trigger.Frequency == null)
        _fileInfo.Trigger.Frequency = "";
      if (_fileInfo.Trigger.Level == null)
        _fileInfo.Trigger.Level = "";
    }

  }
}