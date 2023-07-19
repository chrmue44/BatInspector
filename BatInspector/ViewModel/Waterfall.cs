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
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using DSPLib;
using libParser;
using System.Threading.Tasks;
using System.Diagnostics;
using NAudio.Wave;

namespace BatInspector
{
  public class Waterfall
  {
    const bool FFT_W3 = false;  // FFT_W3:(no multithreading) 143 ms, DSPLib (with multithreading): 50ms

    string _wavName;
    UInt32 _fftSize;
  //  double[] _samples;
    SoundEdit _audio;
    bool _ok = false;
    List<double[]> _spec;
    //int _samplingRate;
    ColorTable _colorTable;
    double _maxAmplitude;
    double _minAmplitude;
    int _width;
    int _heightFt;
    int _heightXt;
    WavFile _wav = null;
    double[] _dummy;

    public double Duration 
    { 
      get
      {
        if (_audio != null)
          return (double)_audio.Samples.Length / _audio.SamplingRate;
        else
          return 0;
       }
    }
    public int SamplingRate { get { return _audio.SamplingRate; } }

    public SoundEdit Audio { get { return _audio; } }
    public List<double[]> Spec {  get { return _spec; } }
    public bool Ok { get { return _ok; } }

    public double Range 
    {
      get { return AppParams.Inst.GradientRange; }
      set
      {
        AppParams.Inst.GradientRange = value;
        calcMinAmplitude();
      }
    }
    public double MinAmplitude {  get { return _minAmplitude; } set { _minAmplitude = value; } }
    public double MaxAmplitude { get { return _maxAmplitude; } set { _maxAmplitude = value; } }

    public string WavName { get { return _wavName; } }
    public PlaybackState PlaybackState { get { return (_wav != null) ? _wav.PlaybackState : PlaybackState.Stopped; } }

    public double PlayPosition
    {  
      get { return (_wav != null) ? _wav.PlayPosition : 0.0; } 
    }
    public Waterfall(string wavName, UInt32 fftSize, int w, int h, ColorTable colorTable)
    {
      _width = w;
      _heightFt = h;
      _heightXt = h / 5;
      _wavName = wavName;
      _fftSize = fftSize;
      _colorTable = colorTable;
      _spec = new List<double[]>();
      _maxAmplitude = _minAmplitude;
      _audio = new SoundEdit(384000, (int)fftSize);
      if(FFT_W3)
      {
        BioAcoustics.initFft(fftSize, enWIN_TYPE.HANN);
      }
      if (File.Exists(_wavName))
      {
        _ok = _audio.readWav(wavName) == 0;
      }
      else
      {
        DebugLog.log("wav file '" + _wavName + "' does not exist!", enLogType.ERROR);
      }
    }

    /// <summary>
    /// generate a waterfall diagram
    /// </summary>
    /// <param name="startTime">start time [s] in wav file</param>
    /// <param name="EndTime">end time [s] in wav file</param>
    /// <param name="width">width of waterfall diagram in pixel</param>
    public void generateFtDiagram(double startTime, double EndTime, uint width)
    {
      if (_ok)
      {
        Stopwatch sw = Stopwatch.StartNew();
        _spec.Clear();
        int idxStart = (int)(startTime * _audio.SamplingRate);
        if (idxStart > _audio.Samples.Length)
          idxStart = (int)_audio.Samples.Length;
        int idxEnd = (int)(EndTime * _audio.SamplingRate);
        if (idxEnd > _audio.Samples.Length)
          idxEnd = (int)_audio.Samples.Length;
        int step = (idxEnd - idxStart) /(int)width;
        if (step == 0)
          step = 1;
        if (step > _fftSize)
          step = (int)_fftSize;

        int max = (int)(idxEnd - idxStart) / (int)step;
        for (int i = 0; i < max; i++)
          _spec.Add(null);
        if (FFT_W3)
        {
          for (int i = 0; i < max; i++)
          {
            int idx = idxStart + i * step;
            if (idx >= 0)
            {
              double[] sp = generateFft(idx, (int)_fftSize, AppParams.Inst.FftWindow);
              _spec[i] = sp;
            }
          }
        }
        else
        {
          Parallel.For(0, max, i =>
          {
            int idx = idxStart + i * step;
            if (idx >= 0)
            {
              double[] sp = generateFft(idx, (int)_fftSize, AppParams.Inst.FftWindow);
              _spec[i] = sp;
            }
          });
        }
        sw.Stop();
      }
      else
        DebugLog.log("generateDiagram(): WAV file is not open!", enLogType.ERROR);
    }


    public void play(int stretch, double tStart, double tEnd, double playPosition)
    {
      if (_wav == null)
        _wav = new WavFile();
      int iStart = Math.Max((int)(tStart *  SamplingRate), 0);
      int iEnd = Math.Min((int)(tEnd * SamplingRate), _audio.Samples.Length);
      _wav.play(1, _audio.SamplingRate / stretch, iStart, iEnd, _audio.Samples, null, playPosition * stretch);
    }

    public void pause()
    {
      if (_wav != null)
      {
        _wav.pause();
      }
    }

    public void stop()
    {
      if (_wav != null)
      {
        _wav.stop();
        _wav = null;
      }
    }


   

    //  http://web-tech.ga-usa.com/2012/05/creating-a-custom-hot-to-cold-temperature-color-gradient-for-use-with-rrdtool/index.html
 

    public double[] generateFft(int idx, int length, DSP.Window.Type window = DSP.Window.Type.Hanning)
    {
      bool logarithmic = AppParams.Inst.WaterfallLogarithmic;
      int zeroPadding = 0; // NOTE: Zero Padding
      if(idx + length > _audio.Samples.Length)
        length = _audio.Samples.Length - idx - 1;

      if (length <= 0)
      {
        DebugLog.log("unable to generate FFT with length " + length.ToString(), enLogType.ERROR);
        return new double[1];
      }
      zeroPadding = (int)_fftSize - length;
      double[] inputSignal = new double[length];
      Array.Copy(_audio.Samples, idx, inputSignal, 0, length);
      double[] lmSpectrum;
      double wScaleFactor = 1.0;
      if (FFT_W3)
      {
        lmSpectrum = BioAcoustics.calculateFft(inputSignal); 
      }
      else
      {
        // Apply window to the Input Data & calculate Scale Factor
        double[] wCoefs = DSP.Window.Coefficients(window, (uint)length);
        double[] wInputData = DSP.Math.Multiply(inputSignal, wCoefs);
        wScaleFactor = DSP.Window.ScaleFactor.Signal(wCoefs);

        // Instantiate & Initialize a new DFT
        FFT fft = new FFT();
        fft.Initialize((uint)length, (uint)zeroPadding); // NOTE: Zero Padding

        // Call the DFT and get the scaled spectrum back
        Complex[] cSpectrum = fft.Execute(wInputData);

        // Convert the complex spectrum to note: Magnitude Format
        lmSpectrum = DSPLib.DSP.ConvertComplex.ToMagnitude(cSpectrum);
      }
      for (int i = 0; i < lmSpectrum.Length; i++)
      {
        if (logarithmic)
        {
          lmSpectrum[i] = Math.Log(lmSpectrum[i]) * wScaleFactor;
          if (lmSpectrum[i] > _maxAmplitude)
          {
            _maxAmplitude = lmSpectrum[i];
            _minAmplitude = _maxAmplitude - AppParams.Inst.GradientRange;
          }
        }
        else
        {
          lmSpectrum[i] *= 100;
          if (lmSpectrum[i] > _maxAmplitude)
          {
            _maxAmplitude = lmSpectrum[i];
            _minAmplitude = 0;
          }
        }
      }
      calcMinAmplitude();
      // Properly scale the spectrum for the added window
//      lmSpectrum = DSP.Math.Multiply(lmSpectrum, wScaleFactor);

      return lmSpectrum;
    }

    public Bitmap generateFtPicture(double fMin, double fMax)
    {
      Bitmap bmp = new Bitmap(_width, _heightFt);
      if (_ok)
      {
        for (int x = 0; x < _width; x++)
        {
          int idxSpec = (int)((double)_spec.Count / (double)_width * (double)x);
          for (int y = 0; y < _heightFt; y++)
          {
            if (_spec.Count > 0)
            {
           //   int idxFreq2 = (int)((double)_spec[0].Length / (double)_height * (double)y);
              double f = (fMax - fMin) * y / _heightFt + fMin;
              int idxFreq =(int)( f * 2000 /(double) _audio.SamplingRate * _fftSize / 2);
              if (_spec[idxSpec] != null)
              {
                if (idxFreq >= _spec[idxSpec].Length)
                  idxFreq = _spec[idxSpec].Length - 1;
                double val = _spec[idxSpec][idxFreq];
                Color col = _colorTable.getColor(val, _minAmplitude, _maxAmplitude);
                bmp.SetPixel(x, _heightFt - 1 - y, col);
              }
            }
          }
        }
      }
      return bmp;
    }

    public Bitmap generateXtPicture(double aMin, double aMax, double tMin, double tMax)
    {
      Bitmap bmp = new Bitmap(_width, _heightXt);
      for (int x = 0; x < _width; x++)
        for (int y = 0; y < _heightXt; y++)
          bmp.SetPixel(x, y, AppParams.Inst.ColorXtBackground);

      if (_ok)
      {
        double samplesPerPixelf = this._audio.Samples.Length * (tMax - tMin) / this.Duration / _width;
        int samplesPerPixel = (int)samplesPerPixelf;
        int idxMin = (int)(tMin / this.Duration * this._audio.Samples.Length);
        int idxMax = (int)(tMax / this.Duration * this._audio.Samples.Length);
        if (samplesPerPixelf > 1.0)
        {
          //          m_isMinMax = true;
          plotAsBand(aMin, aMax, idxMin, idxMax, bmp);
        }
        else
        {
          plotAsSinglePixels(aMin, aMax, idxMin, idxMax, bmp);
  //        m_isMinMax = false;
        }

      }
      return bmp;
    }

    void drawLine(int x, int ymin, int ymax, Bitmap bmp) 
    {
      for (int y = ymin; y <= ymax; y++)
        bmp.SetPixel(x, y, AppParams.Inst.ColorXtLine);
    }


  void plotAsBand(double aMin, double aMax,  int idxMin, int idxMax, Bitmap bmp) 
    {
      int samplesPerPixel = (idxMax - idxMin) / _width;
      for(int x =0; x < _width; x++) 
      {
        double min = 10;
        double max = -10;
        int idx = (idxMax - idxMin) * x / _width + idxMin;
        for (int j = 0; j < samplesPerPixel; j++)
        {
          if ((idx < 0) || (idx >= this._audio.Samples.Length))
            break;
          double val = this._audio.Samples[idx++];
          if (min > val)
            min = val;
          if (max < val)
            max = val;
        }
        if (max > aMax)
          max = aMax;
        if (min < aMin)
          min = aMin;
        int y1 = (int)((1 - (max - aMin) / (aMax - aMin)) * (_heightXt-1));
        int y2 = (int)((1 - (min - aMin) / (aMax - aMin)) * (_heightXt-1));
        drawLine(x, y1, y2, bmp);
      }
    }

    void plotAsSinglePixels(double aMin, double aMax, int idxMin, int idxMax, Bitmap bmp)
    {
      int samplesPerPixel = (idxMax - idxMin) / _width;
      for (int x = 0; x < _width; x++)
      {
        int idx = (idxMax - idxMin) * x / _width + idxMin;
        int y1 = (int)((1 - (this._audio.Samples[idx] - aMin) / (aMax - aMin)) * (_heightXt - 1));
        bmp.SetPixel(x, y1, AppParams.Inst.ColorXtLine);
      }
    }

    void calcMinAmplitude()
    {
      if (AppParams.Inst.WaterfallLogarithmic)
        _minAmplitude = _maxAmplitude - AppParams.Inst.GradientRange;
      else
        _minAmplitude = _maxAmplitude / Math.Pow(10, AppParams.Inst.GradientRange / 20);
    }
  }
}
