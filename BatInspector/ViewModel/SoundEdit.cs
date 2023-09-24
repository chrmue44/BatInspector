/********************************************************************************
 *               Author: Christian Müller
 *     Date of creation: 2023-05-25                                       
 *   Copyright (C) 2023: Christian Müller chrmue44(at)gmail(dot).com
 *
 *              Licence:  CC BY-NC 4.0 
 ********************************************************************************/

using BatInspector.Forms;
using libParser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;

namespace BatInspector
{
  /// <summary>
  /// a class to store audio samples and manipulate them. The result can be stored in a WAV file
  /// </summary>
  public class SoundEdit
  {
    int _size;
    double[] _samples;
    double[] _spectrum;
    double[] _originalSamples = null;
    int _samplingRate;
    string _fName = "";
    bool _fftInitDone = false;
    List<Tuple<int, int>> _overdrive;

    public SoundEdit(int samplingRate = 384000, int size = 384000)
    {
      _size = size;
      _samplingRate = samplingRate;
      _samples = new double[size];
      _overdrive = new List<Tuple<int, int>>();
    }

    /// <summary>
    /// complex frequency spectrum
    /// </summary>
    public double[] Spectrum { get { return _spectrum; } set {  _spectrum = value; } }

    /// <summary>
    /// samples in time domain
    /// </summary>
    public double[] Samples { get { return _samples; }   }

    public int SamplingRate {  get{ return  _samplingRate; } }
    public void expandSamples(double[] samples, int size)
    {
      _samples = new double[size];
      _size = size;
      int iDst = 0;
      int iSrc = 0;
      do
      {
        _samples[iDst++] = samples[iSrc++];
        if(iSrc >= samples.Length)
          iSrc = 0;
      } while (iDst < samples.Length);
      initFft();
    }

    public void applyThresholdToSpectrum(double thresh)
    {
      for(int i = 0; i < _spectrum.Length; i++)
      {
        if (Math.Abs(_spectrum[i]) < thresh)
          _spectrum[i] = 0;
      }
    }

    public double getMaxPower()
    {
      double maxPower = 0;
      foreach(double power in _spectrum)
      {
        if(power > maxPower)
          maxPower = power;
      }
      return maxPower;
    }

    public void subtract(double[] spectrum)
    {
      for(int i = 0; i < spectrum.Length; i++) 
      {
        _spectrum[i] -= spectrum[i];
      }
    }

    public void removeSection(double tMin, double tMax)
    {
      int iStart = (int)(tMin * _samplingRate);
      if(iStart > _samples.Length - 1)
        iStart = _samples.Length - 1;
      int iEnd = (int)(tMax * _samplingRate);
      if (iEnd > _samples.Length - 1)
        iEnd = _samples.Length - 1;
      double[] s = new double[_samples.Length - (iEnd - iStart)];
      for(int i = 0; i < iStart; i++)
        s[i] = _samples[i];
      for (int i = iStart, j = iEnd; i < s.Length; i++, j++)
        s[i] = _samples[j];

      _samples = s;
    }

    public bool findOverdrive(double tMin, double tMax)
    {
      _overdrive.Clear();
      int cntOv = 0;
      int cntRange = 0;
      bool nonOverDrive = true;
      int start = 0;
      int end = 0;
      int iStart = (int)(tMin * _samplingRate);
      int iEnd = (int)(tMax * _samplingRate);
      if (iEnd > _samples.Length)
        iEnd = _samples.Length;
      int cntRangeMax = (int)(0.001 * _samplingRate);
      for(int i = iStart; i < iEnd; i++)
      {
        if ((_samples[i] >= 0.995) || (_samples[i] <= -0.995))
          cntOv++;
        else
        {
          cntOv = 0;
          cntRange++;
        }
        if (nonOverDrive)
        {
          if (cntOv > 0)
          {
            start = i;
            cntRange = 0;
            nonOverDrive = false;
          }
        }
        else
        {
          if (cntRange > cntRangeMax)
          {
            end = i;
            Tuple<int, int> t = new Tuple<int, int>(start, end);
            _overdrive.Add(t);
            nonOverDrive = true;
          }
        }
      }
      return _overdrive.Count > 0;
    }

    public bool isOverdrive(int idx)
    {
      foreach(Tuple<int, int> t in _overdrive)
      {
        if((t.Item1 <= idx) && (idx <= t.Item2))
          return true;
      }
      return false;
    }

    public void FftForward()
    {
      initFft();
      _spectrum = BioAcoustics.calculateFftComplexOut(_samples);
    }

    public void copySamples(short[] samples)
    {
      _samples = new double[samples.Length];
      _originalSamples = new double[samples.Length];
      _size = samples.Length; 
      _fftInitDone = false;
      for (int i = 0; i < samples.Length; i++)
      {
        _samples[i] = samples[i];
        _originalSamples[i] = samples[i];
      }
    }

    public void copySamples(double[] samples)
    {
      _samples = new double[samples.Length];
      _originalSamples = new double[samples.Length];
      _size = samples.Length;
      _fftInitDone = false;
      for (int i = 0; i < samples.Length; i++)
      {
        _samples[i] = samples[i];
        _originalSamples[i] = samples[i];
      }
    }

    public void FftBackward()
    {
      initFft();  
      _samples = BioAcoustics.calculateFftReversed(_spectrum);
    }

    /// <summary>
    /// read content of WAV file
    /// </summary>
    /// <param name="name">name of the wav file</param>
    /// <param name="keepOriginal">keep content of _originalSamples</param>
    /// <returns></returns>
    public int readWav(string name, bool keepOriginal = false)
    {
      int retVal = 0;
      WavFile inFile = new WavFile();
      retVal = inFile.readFile(name, ref _samples);
      _samplingRate = (int)inFile.FormatChunk.Frequency;
      _size = _samples.Length;
      if ((_originalSamples == null) || !keepOriginal)
      {
        _originalSamples = new double[_samples.Length];
        Array.Copy(_samples, _originalSamples, _samples.Length);
      }
      if (retVal == 0)
      {
        _fName = name;
        _samplingRate = (int)inFile.FormatChunk.Frequency;
        _size = inFile.AudioSamples.Length;
        _fftInitDone = false;
      }
      return retVal;
    }

    /// <summary>
    /// save wav file. If the name equals to the original name a backup in the sub folder 
    /// 'orig' of the related project will be created
    /// </summary>
    /// <param name="name">name of the wav file</param>
    /// <param name="wavSubDir">sub directory containing wav files</param>
    public void saveAs(string name, string wavSubDir)
    {
      try
      {
        // backup in folder del if possible
        if ((_fName != "") && (name == _fName))
        {
          ZoomView.saveWavBackup(name, wavSubDir);
          // force new generation of png by deleting it
          string pngName = _fName.Replace(".wav", ".png");
          if (File.Exists(pngName))
            File.Delete(pngName);
        }
        WavFile outFile = new WavFile();
        outFile.createFile(1, _samplingRate, 0, Samples.Length - 1, Samples);
        outFile.saveFileAs(name);
      }
      catch
      {
        DebugLog.log("unable to save file: " + name, enLogType.ERROR);
      }
    }

    public void normalize()
    {
      double max = 0;
      foreach(double sp in _samples)
      {
        double s = Math.Abs(sp);
        if (s > max)
          max = s;
      }
      double fact = 0.95 / max;
      for (int i = 0; i < _samples.Length; i++)
        _samples[i] *= fact;
    }


    public void bandpass(double fmin, double fmax)
    {
      if (fmin > fmax)
      {
        double v;
        v = fmin;
        fmin = fmax;
        fmax = v; 
      }
      int iFmin = (int)(_spectrum.Length * fmin / _samplingRate);
      int iFmax = (int)(_spectrum.Length * fmax / _samplingRate);
      for (int i = 0; i < iFmin; i++)
      {
        _spectrum[i] = 0;
        _spectrum[_spectrum.Length - i - 1] = 0;
      }
      for (int i = iFmax; i < _spectrum.Length/2; i++)
      {
        _spectrum[i] = 0;
        _spectrum[_spectrum.Length - i - 1] = 0;
      }
    }

    /// <summary>
    /// noise reduction (does not work as expected)
    /// </summary>
    /// <param name="threshold"></param>
    public void reduceNoise(double threshold)
    {
      List<double> lst = new List<double>(_spectrum);

      findMaxPower(out double maxP, out double maxF);
      double MaxDb = 20* Math.Log10(maxP); 

      double limitDb = MaxDb + threshold;
      double limit = Math.Pow(10, limitDb / 20);

      limit = limit * limit;
      for (int i = 2; i < _spectrum.Length; i+=2)
      {
        double p = _spectrum[i] * _spectrum[i] + _spectrum[i+1] + _spectrum[i+1];
        if (p < limit)
        {
          _spectrum[i] = 0;
          _spectrum[i + 1] = 0;
        }
        else
        {
          _spectrum[i] = _spectrum[i];
        }
      }
    }

    public void undo()
    {
      if (_originalSamples != null)
      {
        _samples = new double[_originalSamples.Length];
        Array.Copy(_originalSamples, _samples, _originalSamples.Length);
      }
    }

    public void applyHeterodynModulation(double freq)
    {
      for(int i = 0; i < _samples.Length; i++)
      {
        double s = Math.Sin(2*Math.PI*i/_samplingRate * freq);
        _samples[i] *= s;
      }
    }

    void initFft()
    {
      if (!_fftInitDone)
      {
        BioAcoustics.initFft((uint)_size, enWIN_TYPE.NONE);
        _fftInitDone = true;
      }
    }

    void findMaxPower(out double p, out double f)
    {
      p = 0;
      f = 0;
      for(int i = 2; i < _spectrum.Length; i+=2)
      {
        double p1 = _spectrum[i]*_spectrum[i] + _spectrum[i+1] * _spectrum[i+1];
        if(p1 > p)
        {
          p = p1;
          f = i;
        }
      }
      p = Math.Sqrt(p);
      f = _samplingRate / 2 * f / _spectrum.Length;
    }
  }
}
