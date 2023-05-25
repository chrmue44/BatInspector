/********************************************************************************
 *               Author: Christian Müller
 *      Date of cration: 2023-05-25                                       
 *   Copyright (C) 2023: christian Müller chrmue44(at)gmail(dot).com
 *
 *              Licence:
 * 
 * THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND,
 * EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
 ********************************************************************************/

using System;

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
    double[] _originalSamples;
    int _samplingRate;

    public SoundEdit(int samplingRate, int size)
    {
      _size = size;
      _samplingRate = samplingRate;
      _samples = new double[size];
      _originalSamples= new double[size];
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

    public void FftForward()
    {
      _spectrum = BioAcoustics.calculateFftComplexOut(_samples);
    }

    public void copySamples(short[] samples)
    {
      _samples = new double[samples.Length];
      _originalSamples = new double[samples.Length];
      _size = samples.Length; 
      initFft();
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
      initFft();
      for (int i = 0; i < samples.Length; i++)
      {
        _samples[i] = samples[i];
        _originalSamples[i] = samples[i];
      }
    }

    public void FftBackward()
    {
      _samples = BioAcoustics.calculateFftReversed(_spectrum);
    }

    public int readWav(string name)
    {
      int retVal = 0;
      WavFile inFile = new WavFile();
      retVal = inFile.readFile(name, ref _samples);
      _samplingRate = (int)inFile.FormatChunk.Frequency;
      _originalSamples = new double[_samples.Length];
      _size = _samples.Length;
      Array.Copy(_samples, _originalSamples, _samples.Length);
      if (retVal == 0)
      {
        _samplingRate = (int)inFile.FormatChunk.Frequency;
        _size = inFile.AudioSamples.Length;
        initFft();
      }
      return retVal;
    }

    public void saveAs(string name)
    {
      WavFile outFile = new WavFile();
      outFile.createFile(1, _samplingRate, 0, Samples.Length - 1, Samples);
      outFile.saveFileAs(name);
    }

    public void bandpass(double fmin, double fmax)
    {
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

    public void undo()
    {
      for(int i = 0; i < _samples.Length; i++)
        _samples[i] = _originalSamples[i];
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
      BioAcoustics.initFft((uint)_size, enWIN_TYPE.NONE);
    }
  }
}
