using DSPLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace BatInspector
{
  public class Spectrum
  {
    double[] _samples;
    RulerData _rulerDataF;
    uint _fftSize;
    double[] _ampl;

    public RulerData RulerDataF { get { return _rulerDataF; } }
    public double[] Amplitude {  get { return _ampl; } }

    public Spectrum()
    {
      _rulerDataF = new RulerData();
    }

    public void create(double[] samples, double tStart, double tEnd, int samplingRate)
    {
      uint idxStart = (uint)(tStart * samplingRate);
      uint idxEnd = (uint)(tEnd *samplingRate);
      _samples = samples;
      uint len = idxEnd - idxStart;
      _fftSize = 256;
      while (_fftSize < len)
        _fftSize <<= 1;
      _ampl = generateFft(idxStart, len);
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
      double[] inputSignal = new double[length];
      Array.Copy(_samples, idx, inputSignal, 0, length);
      // Apply window to the Input Data & calculate Scale Factor
      double[] wCoefs = DSP.Window.Coefficients(window, length);
      double[] wInputData = DSP.Math.Multiply(inputSignal, wCoefs);
      double wScaleFactor = DSP.Window.ScaleFactor.Signal(wCoefs);

      // Instantiate & Initialize a new DFT
      FFT fft = new FFT();
      fft.Initialize(length, zeroPadding); // NOTE: Zero Padding

      // Call the DFT and get the scaled spectrum back
      Complex[] cSpectrum = fft.Execute(wInputData);

      // Convert the complex spectrum to note: Magnitude Format
      double[] lmSpectrum = DSPLib.DSP.ConvertComplex.ToMagnitude(cSpectrum);
      for (int i = 0; i < lmSpectrum.Length; i++)
      {
        lmSpectrum[i] = Math.Log(lmSpectrum[i]);        
      }


      // Properly scale the spectrum for the added window
      lmSpectrum = DSP.Math.Multiply(lmSpectrum, wScaleFactor);

      return lmSpectrum;
    }
  }
}
