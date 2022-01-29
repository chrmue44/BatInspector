using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using DSPLib;


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
    double _range = 20;
    int _width;
    int _heightFt;
    int _heightXt;
    AppParams _settings;
    WavFile _wav = null;

    public double Duration { get { return (double)_samples.Length / _samplingRate; } }
    public int SamplingRate { get { return _samplingRate; } }
    public double[] Samples { get { return _samples; } }
    public List<double[]> Spec {  get { return _spec; } }
    public bool Ok { get { return _ok; } }

    public double Range { get { return _range; } set { _range = value; _minAmplitude = _maxAmplitude - _range; } }
    public double MinAmplitude {  get { return _minAmplitude; } set { _minAmplitude = value; } }
    public double MaxAmplitude { get { return _maxAmplitude; } set { _maxAmplitude = value; } }

    public Waterfall(string wavName, UInt32 fftSize, int w, int h, AppParams settings)
    {
      _width = w;
      _heightFt = h;
      _heightXt = h / 5;
      _wavName = wavName;
      _fftSize = fftSize;
      _settings = settings;
      double[] dummy;
      _spec = new List<double[]>();
      _colorMap = new List<Color>();
      _minAmplitude = -25;
      _maxAmplitude = _minAmplitude;
      prepareColorMap();
      if (File.Exists(_wavName))
      {
        openWav(wavName, out _samples, out dummy, out _samplingRate);
        _ok = true;
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
        Spec.Clear();
        uint idxStart = (uint)(startTime * _samplingRate);
        if (idxStart > _samples.Length)
          idxStart = (uint)_samples.Length;
        uint idxEnd = (uint)(EndTime * _samplingRate);
        if (idxEnd > _samples.Length)
          idxEnd = (uint)_samples.Length;
        uint step = (idxEnd - idxStart) / width;
        if (step == 0)
          step = 1;
        if (step > _fftSize)
          step = _fftSize;
        for (uint idx = idxStart; idx < idxEnd; idx += step)
          generateFft(idx, _fftSize, DSP.Window.Type.Hanning);
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

    void generateFft(UInt32 idx, UInt32 length, DSP.Window.Type window)
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
      for(int i =0; i < lmSpectrum.Length; i++)
      {
        lmSpectrum[i] = Math.Log(lmSpectrum[i]);
        if (lmSpectrum[i] > _maxAmplitude)
        {
          _maxAmplitude = lmSpectrum[i];
          _minAmplitude = _maxAmplitude - _range;
        }
      }


      // Properly scale the spectrum for the added window
      lmSpectrum = DSP.Math.Multiply(lmSpectrum, wScaleFactor);

      _spec.Add(lmSpectrum);
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
              if (idxFreq >= _spec[idxSpec].Length)
                idxFreq = _spec[idxSpec].Length - 1;
              double val = _spec[idxSpec][idxFreq];
              Color col = getColor(val, _minAmplitude, _maxAmplitude);
              bmp.SetPixel(x, _heightFt - 1 - y, col);
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
        int y1 = (int)((1 - (this.Samples[idx] - aMin) / (aMax - aMin)) * _heightXt);
        bmp.SetPixel(x, y1, _settings.ColorXtLine);
      }
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
    public void openWav(string filename, out double[] left, out double[] right, out int samplingRate)
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
