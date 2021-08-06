using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using DSPLib;
using Image = System.Drawing.Image;

namespace BatInspector
{
  class Waterfall
  {
    string _wavName;
    UInt32 _fftSize;
    double[] _samples;
    bool _ok = false;
    List<double[]> _spec;
    int _samplingRate;
    List<Color> _colorMap;
    double _maxAmplitude;
    double _minAmplitude;

    public Waterfall(string wavName, UInt32 fftSize)
    {
      _wavName = wavName;
      _fftSize = fftSize;
      double[] dummy;
      _spec = new List<double[]>();
      _colorMap = new List<Color>();
      _minAmplitude = -30;
      _maxAmplitude = -30;
      prepareColorMap();
      if (File.Exists(_wavName))
      {
        openWav(wavName, out _samples, out dummy, out _samplingRate);
        for(UInt32 idx = 0; idx < _samples.Length; idx += _fftSize)
          generateFft(idx, _fftSize);
        _ok = true;
      }
    }

    void addRgb565ColorToMap(int rgb565)
    {
      _colorMap.Add(Color.FromArgb(
                                   (rgb565 & 0xF800) >> 8,
                                   (rgb565 & 0x07E0) >> 3,
                                   (rgb565 & 0x001F) << 3
                                  ));
    }

    void prepareColorMap()
    {
      addRgb565ColorToMap(0x0000);
      addRgb565ColorToMap(0x0002);
      addRgb565ColorToMap(0x0004);
      addRgb565ColorToMap(0x0010);
      addRgb565ColorToMap(0x0038);
      addRgb565ColorToMap(0x00F8);
      addRgb565ColorToMap(0x01D8);
      addRgb565ColorToMap(0x0238);
      addRgb565ColorToMap(0x0438);
      addRgb565ColorToMap(0x0730);
      addRgb565ColorToMap(0x07F0);
      addRgb565ColorToMap(0x0FC0);
      addRgb565ColorToMap(0x2F40);
      addRgb565ColorToMap(0xFFC0);
      addRgb565ColorToMap(0xFF00);
      addRgb565ColorToMap(0xFD00);
      addRgb565ColorToMap(0xFC00);
    }

    Color getColor(double val, double min, double max)
    {
      if (val < min)
        return _colorMap[0];
      else if (val > max)
        return _colorMap.Last();
      else
      {
        int i = (int)((val - min) / (max - min) * _colorMap.Count);
        if (i < _colorMap.Count)
          return _colorMap[i];
        else
          return _colorMap.Last();
      }
    }

    void generateFft(UInt32 idx, UInt32 length)
    {
   //   double amplitude = 1.0; double frequency = 20000.5;
      UInt32 zeroPadding = 0; // NOTE: Zero Padding
      if(idx + length > _samples.Length)
      {
        length = (UInt32)_samples.Length - idx - 1;
        zeroPadding = _fftSize - length;
      }
      double[] inputSignal = new double[length];
      Array.Copy(_samples, idx, inputSignal, 0, length);
      // Apply window to the Input Data & calculate Scale Factor
      double[] wCoefs = DSP.Window.Coefficients(DSP.Window.Type.Hanning, length);
      double[] wInputData = DSP.Math.Multiply(inputSignal, wCoefs);
      double wScaleFactor = DSP.Window.ScaleFactor.Signal(wCoefs);

      // Instantiate & Initialize a new DFT
      FFT fft = new FFT();
      fft.Initialize(length, zeroPadding); // NOTE: Zero Padding

      // Call the DFT and get the scaled spectrum back
      Complex[] cSpectrum = fft.Execute(wInputData);

      // Convert the complex spectrum to note: Magnitude Format
      double[] lmSpectrum = DSPLib.DSP.ConvertComplex.ToMagnitude(cSpectrum);
      for(int i =0; i < lmSpectrum.Length; i++)
      {
        lmSpectrum[i] = Math.Log(lmSpectrum[i]);
        if (lmSpectrum[i] > _maxAmplitude)
          _maxAmplitude = lmSpectrum[i];
      }


      // Properly scale the spectrum for the added window
      lmSpectrum = DSP.Math.Multiply(lmSpectrum, wScaleFactor);

      _spec.Add(lmSpectrum);
    }

    public Bitmap generatePicture(int width, int height)
    {
      Bitmap bmp = new Bitmap(width, height);
      if (_ok)
      {
        for (int x = 0; x < width; x++)
        {
          int idxSpec = (int)((double)_spec.Count / (double)width * (double)x);
          for (int y = 0; y < height; y++)
          {
            int idxFreq = (int)((double)_spec[0].Length / (double)height * (double)y);
            double val = _spec[idxSpec][idxFreq];
            Color col = getColor(val, _minAmplitude, _maxAmplitude);
            bmp.SetPixel(x, height - 1 - y, col);
          }
        }
      }
      return bmp;
    }

    // convert two bytes to one double in the range -1 to 1
    static double bytesToDouble(sbyte firstByte, sbyte secondByte)
    {
      // convert two bytes to one short (little endian)
      int s = (int)secondByte << 8;
      s |=  (int)firstByte;
      // convert to range from -1 to (just below) 1
      return s / 32768.0;
    }

    // Returns left and right double arrays. 'right' will be null if sound is mono.
    public static void openWav(string filename, out double[] left, out double[] right, out int samplingRate)
    {
      byte[] wav = File.ReadAllBytes(filename);

      // Determine if mono or stereo
      int channels = wav[22];     // Forget byte 23 as 99.999% of WAVs are 1 or 2 channels
     samplingRate = wav[24] + wav[25] * 0x100 + wav[26] * 0x10000 + wav[27] * 0x1000000;

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
      int samples = (wav.Length - pos) / 2;     // 2 bytes per sample (16 bit sound mono)
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
        while (i < left.Length)
        {
          left[i] = bytesToDouble((sbyte)wav[pos], (sbyte)wav[pos + 1]);
          pos += 2;
          i++;
        }
      }
      else
      {
        while (i < left.Length)
        {
          left[i] = bytesToDouble((sbyte)wav[pos], (sbyte)wav[pos + 1]);
          pos += 2;
          right[i] = bytesToDouble((sbyte)wav[pos], (sbyte)wav[pos + 1]);
          pos += 2;
          i++;
        }
      }
    }
  }
}
