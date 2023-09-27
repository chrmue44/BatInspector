/********************************************************************************
 *               Author: Christian Müller
 *     Date of creation: 2021-08-10                                       
 *   Copyright (C) 2023: Christian Müller chrmue44(at)gmail(dot).com
 *
 *              Licence:  CC BY-NC 4.0 
 ********************************************************************************/
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
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
    const int XT_TO_FT_RATIO = 5;
    string _wavName;
    SoundEdit _audio;
    bool _ok = false;
    List<double[]> _spec;
    ColorTable _colorTable;
    double _maxAmplitude;
    double _minAmplitude;
    WavFile _wav = null;
    
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
    public Waterfall(string wavName,  ColorTable colorTable)
    {
      _wavName = wavName;
      _colorTable = colorTable;
      _spec = new List<double[]>();
      _maxAmplitude = _minAmplitude;
      _audio = new SoundEdit(384000, (int)AppParams.Inst.FftWidth);
      if(FFT_W3)
      {
        BioAcoustics.initFft(AppParams.Inst.FftWidth, enWIN_TYPE.HANN);
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
        uint fftSize = AppParams.Inst.FftWidth;
        int idxStart = (int)(startTime * _audio.SamplingRate);
        if (idxStart > _audio.Samples.Length)
          idxStart = (int)_audio.Samples.Length;
        int idxEnd = (int)(EndTime * _audio.SamplingRate);
        if (idxEnd > _audio.Samples.Length)
          idxEnd = (int)_audio.Samples.Length;
        int step = (idxEnd - idxStart) /(int)width;
        if (step == 0)
          step = 1;
        if (step > fftSize)
          step = (int)fftSize;

        int max = (int)(idxEnd - idxStart) / (int)step;
        for (int i = 0; i < max; i++)
          _spec.Add(null);
/*        if (FFT_W3)   // not too helpul here
        { */
          for (int i = 0; i < max; i++)
          {
            int idx = idxStart + i * step;
            if (idx >= 0)
            {
              double[] sp = generateFft(idx, (int)fftSize, AppParams.Inst.FftWindow);
              _spec[i] = sp;
            }
          }
   /*     }
        else
        {
          Parallel.For(0, max, i =>
          {
            int idx = idxStart + i * step;
            if (idx >= 0)
            {
              double[] sp = generateFft(idx, (int)fftSize, AppParams.Inst.FftWindow);
              _spec[i] = sp;
            }
          });
        } */
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

    public void play_HET(double freq_HET, double tStart, double tEnd, double playPosition)
    {
      if (_wav == null)
        _wav = new WavFile();
      int iStart = Math.Max((int)(tStart * SamplingRate), 0);
      int iEnd = Math.Min((int)(tEnd * SamplingRate), _audio.Samples.Length);
      _wav.play_HET(1, _audio.SamplingRate, freq_HET, iStart, iEnd, _audio.Samples, null, playPosition);
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
      zeroPadding = (int)AppParams.Inst.FftWidth - length;
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
      int width = (int)AppParams.Inst.WaterfallWidth;
      int heightFt = (int)AppParams.Inst.WaterfallHeight;
      BitmapFast bmp = new BitmapFast(width, heightFt);

      if (_ok)
      {
        int fftSize = (int)AppParams.Inst.FftWidth;
        for (int x = 0; x < width; x++)
        {
          int idxSpec = (int)((double)_spec.Count / (double)width * (double)x);
          for (int y = 0; y < heightFt; y++)
          {
            if (_spec.Count > 0)
            {
              double f = (fMax - fMin) * y / heightFt + fMin;
              int idxFreq = (int)(f * 2000 / (double)_audio.SamplingRate * fftSize / 2);
              if (_spec[idxSpec] != null)
              {
                if (idxFreq >= _spec[idxSpec].Length)
                  idxFreq = _spec[idxSpec].Length - 1;
                double val = _spec[idxSpec][idxFreq];
                System.Drawing.Color col = _colorTable.getColor(val, _minAmplitude, _maxAmplitude);
                bmp.setPixel(x, heightFt - 1 - y, col);
              }
            }
          }
        }
      }
      return bmp.Bmp;
    }

    public Bitmap generateXtPicture(double aMin, double aMax, double tMin, double tMax)
    {
      int width = (int)AppParams.Inst.WaterfallWidth;
      int heightXt = (int)AppParams.Inst.WaterfallHeight / XT_TO_FT_RATIO;
      bool ovrdrive = _audio.findOverdrive(tMin, tMax);
      BitmapFast bmp = new BitmapFast(width, heightXt);
      for (int x = 0; x < width; x++)
        for (int y = 0; y < heightXt; y++)
          bmp.setPixel(x, y, AppParams.Inst.ColorXtBackground);

      if (_ok)
      {
        double samplesPerPixelf = this._audio.Samples.Length * (tMax - tMin) / this.Duration / width;
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
      return bmp.Bmp;
    }

    void drawLine(int x, int ymin, int ymax, BitmapFast bmp, Color color) 
    {
      for (int y = ymin; y <= ymax; y++)
        bmp.setPixel(x, y, color);
    }


  void plotAsBand(double aMin, double aMax,  int idxMin, int idxMax, BitmapFast bmp) 
    {
      int width = (int)AppParams.Inst.WaterfallWidth;
      int samplesPerPixel = (idxMax - idxMin) / width;
      int heightXt = (int)AppParams.Inst.WaterfallHeight / XT_TO_FT_RATIO;
      int ovrDriveCnt = 0;

      for (int x =0; x < width; x++) 
      {
        double min = 10;
        double max = -10;
        int idx = (idxMax - idxMin) * x / width + idxMin;
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
        int y1 = (int)((1 - (max - aMin) / (aMax - aMin)) * (heightXt-1));
        int y2 = (int)((1 - (min - aMin) / (aMax - aMin)) * (heightXt-1));
        Color color = AppParams.Inst.ColorXtLine;
        if (_audio.isOverdrive(idx))
          ovrDriveCnt = 3;
        if (ovrDriveCnt > 0)
        {
          color = Color.Red;
          ovrDriveCnt--;
        }
        drawLine(x, y1, y2, bmp, color);
      }
    }

    void plotAsSinglePixels(double aMin, double aMax, int idxMin, int idxMax, BitmapFast bmp)
    {
      int width = (int)AppParams.Inst.WaterfallWidth;
      int samplesPerPixel = (idxMax - idxMin) / width;
      int heightXt = (int)AppParams.Inst.WaterfallHeight / XT_TO_FT_RATIO;

      for (int x = 0; x < width; x++)
      {
        int idx = (idxMax - idxMin) * x / width + idxMin;
        int y1 = (int)((1 - (this._audio.Samples[idx] - aMin) / (aMax - aMin)) * (heightXt - 1));
        Color color = AppParams.Inst.ColorXtLine;
        if (_audio.isOverdrive(idx))
          color = Color.Red;
        bmp.setPixel(x, y1, color);
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
