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

namespace BatInspector
{
  public class Waterfall
  {
    const bool FFT_W3 = false;

    string _wavName;
    UInt32 _fftSize;
    double[] _samples;
    bool _ok = false;
    List<double[]> _spec;
    int _samplingRate;
    ColorTable _colorTable;
    double _maxAmplitude;
    double _minAmplitude;
    int _width;
    int _heightFt;
    int _heightXt;
    AppParams _settings;
    WavFile _wav = null;

    public double Duration 
    { 
      get
      {
        if (_samples != null)
          return (double)_samples.Length / _samplingRate;
        else
          return 0;
       }
    }
    public int SamplingRate { get { return _samplingRate; } }
    public double[] Samples { get { return _samples; } }
    public List<double[]> Spec {  get { return _spec; } }
    public bool Ok { get { return _ok; } }

    public double Range 
    {
      get { return _settings.GradientRange; }
      set
      {
        _settings.GradientRange = value;
        calcMinAmplitude();
      }
    }
    public double MinAmplitude {  get { return _minAmplitude; } set { _minAmplitude = value; } }
    public double MaxAmplitude { get { return _maxAmplitude; } set { _maxAmplitude = value; } }

    public Waterfall(string wavName, UInt32 fftSize, int w, int h, ColorTable colorTable)
    {
      _width = w;
      _heightFt = h;
      _heightXt = h / 5;
      _wavName = wavName;
      _fftSize = fftSize;
      _colorTable = colorTable;
      double[] dummy;
      _spec = new List<double[]>();
      _maxAmplitude = _minAmplitude;
      if (File.Exists(_wavName))
      {
        _ok = openWav(wavName, out _samples, out dummy, out _samplingRate);
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
        _spec.Clear();
        int idxStart = (int)(startTime * _samplingRate);
        if (idxStart > _samples.Length)
          idxStart = (int)_samples.Length;
        int idxEnd = (int)(EndTime * _samplingRate);
        if (idxEnd > _samples.Length)
          idxEnd = (int)_samples.Length;
        int step = (idxEnd - idxStart) /(int)width;
        if (step == 0)
          step = 1;
        if (step > _fftSize)
          step = (int)_fftSize;

         int max = (int)(idxEnd - idxStart) / (int)step;
         for (int i = 0; i < max; i++)
           _spec.Add(null);
         Parallel.For(0, max, i =>
         {
           int idx = idxStart + i * step;
           if (idx >= 0)
           {
             double[] sp = generateFft(idx, (int)_fftSize, _settings.FftWindow);
             _spec[i] = sp;
           }
         });
      }
      else
        DebugLog.log("generateDiagram(): WAV file is not open!", enLogType.ERROR);

    }


    public void play(int stretch, double tStart, double tEnd)
    {
      _wav = new WavFile();
      int iStart = Math.Max((int)(tStart *  SamplingRate), 0);
      int iEnd = Math.Min((int)(tEnd * SamplingRate), Samples.Length);
      _wav.play(1, _samplingRate / stretch, iStart, iEnd, Samples);
      _wav = null;
    }

    public void stop()
    {
      if (_wav != null)
        _wav.stop();
    }


   

    //  http://web-tech.ga-usa.com/2012/05/creating-a-custom-hot-to-cold-temperature-color-gradient-for-use-with-rrdtool/index.html
 

    public double[] generateFft(int idx, int length, DSP.Window.Type window = DSP.Window.Type.Hanning)
    {
      bool logarithmic = _settings.WaterfallLogarithmic;
      //   double amplitude = 1.0; double frequency = 20000.5;
      int zeroPadding = 0; // NOTE: Zero Padding
  //    _maxAmplitude = -100;
      if(idx + length > _samples.Length)
      {
        length = _samples.Length - idx - 1;
//        zeroPadding = _fftSize - length;
      }
      zeroPadding = (int)_fftSize - length;
      double[] inputSignal = new double[length];
      Array.Copy(_samples, idx, inputSignal, 0, length);
      double[] lmSpectrum;
      double wScaleFactor;
      if (FFT_W3)
      {
        wScaleFactor = 1;
        lmSpectrum = BioAcoustics.calculateFft((int)_fftSize, enWIN_TYPE.BLACKMAN_HARRIS_7, inputSignal);
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
            _minAmplitude = _maxAmplitude - _settings.GradientRange;
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
              int idxFreq =(int)( f * 2000 /(double) _samplingRate * _fftSize / 2);
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
          bmp.SetPixel(x, y, _settings.ColorXtBackground);

      if (_ok)
      {
        double samplesPerPixelf = this.Samples.Length * (tMax - tMin) / this.Duration / _width;
        int samplesPerPixel = (int)samplesPerPixelf;
        int idxMin = (int)(tMin / this.Duration * this.Samples.Length);
        int idxMax = (int)(tMax / this.Duration * this.Samples.Length);
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
        bmp.SetPixel(x, y, _settings.ColorXtLine);
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
          if ((idx < 0) || (idx >= this.Samples.Length))
            break;
          double val = this.Samples[idx++];
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
        int y1 = (int)((1 - (this.Samples[idx] - aMin) / (aMax - aMin)) * (_heightXt - 1));
        bmp.SetPixel(x, y1, _settings.ColorXtLine);
      }
    }

    void calcMinAmplitude()
    {
      if (_settings.WaterfallLogarithmic)
        _minAmplitude = _maxAmplitude - _settings.GradientRange;
      else
        _minAmplitude = _maxAmplitude / Math.Pow(10, _settings.GradientRange / 20);
    }

    // Returns left and right double arrays. 'right' will be null if sound is mono.
    public bool openWav(string filename, out double[] left, out double[] right, out int samplingRate)
    {
      left = null;
      right = null; 
      samplingRate = 0;

      byte[] wav = File.ReadAllBytes(filename);
      if (wav.Length < 256)
      {
        DebugLog.log("could not open WAV: " + filename, enLogType.ERROR);
        return false;
      }

      // Determine if mono or stereo
      int bitsPerSample = wav[34] + wav[35] * 0x100;
      int channels = wav[22];     // Forget byte 23 as 99.999% of WAVs are 1 or 2 channels
      samplingRate = wav[24] + wav[25] * 0x100 + wav[26] * 0x10000 + wav[27] * 0x1000000;
      int fmt = wav[20] + wav[21] * 0x100;

      // Get past all the other sub chunks to get to the data subchunk:
      int pos = 12;   // First Subchunk ID from 12 to 16

      // Keep iterating until we find the data chunk (i.e. 64 61 74 61 ...... (i.e. 100 97 116 97 in decimal))
      while (!(wav[pos] == 100 && wav[pos + 1] == 97 && wav[pos + 2] == 116 && wav[pos + 3] == 97))
      {
        pos += 4;
        int chunkSize = wav[pos] + wav[pos + 1] * 256 + wav[pos + 2] * 65536 + wav[pos + 3] * 16777216;
        pos += 4 + chunkSize;
      }
      pos += 8;

      // Pos is now positioned to start of actual sound data.
      int samples = (wav.Length - pos) * 8 / bitsPerSample;     // 2 bytes per sample (16 bit sound mono)
      if (channels == 2) samples /= 2;        // 4 bytes per sample (16 bit stereo)

      // Allocate memory (right will be null if only mono sound)
      left = new double[samples];
      if (channels == 2) right = new double[samples];
      else right = null;

      // Write to double array/s:
      int i = 0;
      //      while (pos < length)

      if (channels == 1)
      {
        if (bitsPerSample == 16)
        {
          while (i < left.Length)
          {
            left[i] = BitConverter.ToInt16(wav, pos) / 32768.0;
            pos += 2;
            i++;
          }
        }
        if (bitsPerSample == 32)
        {
          while (i < left.Length)
          {
            left[i] = BitConverter.ToInt32(wav, pos) / 32768.0 / 32768.0;
            pos += 4;
            i++;
          }
        }

      }
      else
      {
        while (i < left.Length)
        {
          left[i] = BitConverter.ToInt16(wav, pos) / 32768.0;
          pos += 2;
          right[i] = BitConverter.ToInt16(wav, pos) / 32768.0;
          pos += 2;
          i++;
        }
      }
      return true;
    }
  }
}
