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
using DSPLib;
using System;
using System.Numerics;

namespace BatInspector
{
  public class Spectrum
  {
    const bool FFT_W3 = false;

    double[] _samples;
    RulerData _rulerDataF;
    uint _fftSize;
    double[] _ampl;
    double _fMax;

    public RulerData RulerDataF { get { return _rulerDataF; } }
    public double[] Amplitude {  get { return _ampl; } }

    public double Fmax { get { return _fMax; } }

    public Spectrum()
    {
      _rulerDataF = new RulerData();
    }

    public void create(double[] samples, double tStart, double tEnd, int samplingRate)
    {
      uint idxStart = (uint)(tStart * samplingRate);
      uint idxEnd = (uint)(tEnd *samplingRate);
      _fMax = ((double)samplingRate )/ 2000.0;
      _samples = samples;
      uint len = idxEnd - idxStart;
      _fftSize = 256;
      while (_fftSize < len)
        _fftSize <<= 1;
      _ampl = generateFft(idxStart, len, DSP.Window.Type.Hanning);
    }


    double[] generateFft(UInt32 idx, UInt32 length, DSP.Window.Type window = DSP.Window.Type.Hanning)
    {
      //   double amplitude = 1.0; double frequency = 20000.5;
      UInt32 zeroPadding = 0; // NOTE: Zero Padding
      if (idx + length > _samples.Length)
      {
        length = (UInt32)_samples.Length - idx - 1;
        //        zeroPadding = _fftSize - length;
      }
      zeroPadding = _fftSize - length;
      double[] inputSignal = new double[_fftSize];
      Array.Copy(_samples, idx, inputSignal, 0, length);
      double[] lmSpectrum;
      if (FFT_W3)
      {
        for (uint i = length; i < _fftSize; i++)
          inputSignal[i] = 0;
        lmSpectrum = BioAcoustics.calculateFft((int)_fftSize, enWIN_TYPE.BLACKMAN_HARRIS_7, inputSignal);
      }
      else
      {
        // Apply window to the Input Data & calculate Scale Factor
        double[] wCoefs = DSP.Window.Coefficients(window, (uint)inputSignal.Length);
        double[] wInputData = DSP.Math.Multiply(inputSignal, wCoefs);
        double wScaleFactor = DSP.Window.ScaleFactor.Signal(wCoefs);
        // Instantiate & Initialize a new DFT
        FFT fft = new FFT();
        fft.Initialize(length, zeroPadding); // NOTE: Zero Padding

        // Call the DFT and get the scaled spectrum back
        Complex[] cSpectrum = fft.Execute(wInputData);

        // Convert the complex spectrum to note: Magnitude Format
        lmSpectrum = DSPLib.DSP.ConvertComplex.ToMagnitude(cSpectrum);
        // Properly scale the spectrum for the added window
        lmSpectrum = DSP.Math.Multiply(lmSpectrum, wScaleFactor);

      }
      for (int i = 0; i < lmSpectrum.Length; i++)
      {
        lmSpectrum[i] = Math.Log(lmSpectrum[i]);        
      }
      lmSpectrum[0] = -100; //DC offset
      lmSpectrum[1] = -100; //DC offset
      lmSpectrum[2] = -100; //DC offset

      return lmSpectrum;
    }

    public double findMinAmplitude()
    {
      double min = 100000;
      foreach(double a in _ampl)
      {
        if (min > a)
          min = a;
      }
      return Math.Pow(10, min / 10);
    }

    public double findMaxAmplitude()
    {
      double max = -100000;
      foreach (double a in _ampl)
      {
        if (max < a)
          max = a;
      }
      return Math.Pow(10, max / 10);
    }

    public double getMeanAmpl(int idx, int n)
    {
      double retVal = 0;
      if(idx >= 0)
      {
        for(int i = idx; i < (idx + n); i++)
        {
          if (i < _ampl.Length)
            retVal += Math.Pow(10, _ampl[i]/10);
          else
            n--;
        }
        if(n > 0)
          retVal /= n;
      }
      return retVal;
    }
  }
}
