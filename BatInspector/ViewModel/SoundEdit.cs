/********************************************************************************
 *               Author: Christian Müller
 *     Date of creation: 2023-05-25                                       
 *   Copyright (C) 2023: Christian Müller chrmue44(at)gmail(dot).com
 *
 *              Licence:  CC BY-NC 4.0 
 ********************************************************************************/

using BatInspector.Forms;
using libParser;
using Org.BouncyCastle.Math.EC;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Xml.Linq;

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
    List<Tuple<int, int>> _overdrive;
    Guano _guano;

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

    public Guano Guano { get { return _guano; } }
    /*
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
    */

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

    /// <summary>
    /// divide by the transferred frequency response to linearize the recording
    /// </summary>
    /// <param name="freqResponse"></param>
    /// <param name="softening">"softens" the application of freq. response: 1.0 apply fully
    /// 0.0: don't apply freq response at all </param>
    public void applyFreqResponse(FreqResponseRecord[] freqResponse, double softening)
    {
      FftForward();
      for (int i = 0; i < _spectrum.Length/2; i++)
      {
        double f = (double) i / _spectrum.Length * _samplingRate;
        double a = getAmplitude(f, freqResponse);
        double fact = Math.Pow(10.0, a * softening / 20.0);
        _spectrum[i] /= fact;
        _spectrum[_spectrum.Length - i - 1] /= fact;
      }
      FftBackward();
    }


    public FreqResponseRecord[] createMicSpectrumFromNoiseFile()
    {
      FreqResponseRecord[] retVal = AirAbsorbtion.initFreqResponse();
      FftForward();
      int idxSpec = 0;
      int idxResp = 1;
      double freqBandWidth = 100.0;
      int avgCnt = (int)(freqBandWidth * (double)_samples.Length / _samplingRate);
      while ((avgCnt % 2) != 0)
        avgCnt++;
      while (idxSpec < _spectrum.Length / 2)
      {
        double f = (double)idxSpec / _spectrum.Length * _samplingRate;
        if (f >= retVal[idxResp].Frequency)
        {
          double sum = 0.0;
          for (int i = 0; i < avgCnt; i++)
          {
            sum += Math.Abs(Spectrum[idxSpec - avgCnt/2]);
            idxSpec++;
          }
          sum /= avgCnt;
          retVal[idxResp].Amplitude = 10.0 * Math.Log(sum);
          idxResp++;
        }
        idxSpec++;
        if (idxResp >= retVal.Length)
          break;
      }

      //find 0db
      int idx10k = 0;
      for (int i = 0; i < retVal.Length; i++)
      {
        if (Math.Abs(retVal[i].Frequency - 10000.0) < 1)
        {
          idx10k = i;
          break;
        }
      }

      // relative to 10 kHz
      double db = retVal[idx10k].Amplitude;
      for (int i = 0; i < retVal.Length; i++)
      {
        double amp = retVal[i].Amplitude - db;
        retVal[i].Amplitude = amp;
      }

      retVal[0].Amplitude = 0;
      retVal[retVal.Length - 1].Amplitude = 0;
      retVal[retVal.Length - 2].Amplitude = 0;
      return retVal;
    }


    private double getAmplitude(double f, FreqResponseRecord[] freqResponse)
    {
      double retVal = 0.0;
      for(int i = 1; i < freqResponse.Length; i++)
      {
        if(freqResponse[i].Frequency >= f)
        {
          double m = (freqResponse[i].Amplitude - freqResponse[i - 1].Amplitude) /
          (freqResponse[i].Frequency - freqResponse[i - 1].Frequency);
          double b = freqResponse[i].Amplitude - m * freqResponse[i].Frequency;
          retVal = m * f + b;
          break;
        }
      }
      return retVal;
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

    public bool isOverdrive(int idxS, int idxE)
    {
      foreach (Tuple<int, int> t in _overdrive)
      {
        if ((idxE >= t.Item1) && (idxE <= t.Item2) || (idxS >= t.Item1) && (idxS <= t.Item2) || 
            (t.Item1 >= idxS) && (t.Item1 <= idxE) || (t.Item2 >= idxS) && (t.Item2 <= idxE))
          return true;
      }
      return false;
    }

    public void FftForward()
    {
      int handle = initFft();
      _spectrum = BioAcoustics.calculateFftComplexOut(handle, _samples);
    }

    public void copySamples(short[] samples)
    {
      _samples = new double[samples.Length];
      _originalSamples = new double[samples.Length];
      _size = samples.Length; 
      for (int i = 0; i < samples.Length; i++)
      {
        _samples[i] = samples[i] / 32768.0;
        _originalSamples[i] = samples[i];
      }
    }

    public void copySamples(double[] samples)
    {
      _samples = new double[samples.Length];
      _originalSamples = new double[samples.Length];
      _size = samples.Length;
      for (int i = 0; i < samples.Length; i++)
      {
        _samples[i] = samples[i];
        _originalSamples[i] = samples[i];
      }
    }

    public void copySampleRange(double[] samples, int iStart, int iEnd)
    {
      _samples = new double[iEnd - iStart + 1];
      _originalSamples = new double[_samples.Length];
      _size = _samples.Length;
      int iSource = iStart;
      int i = 0;
        for (; i < _samples.Length; i++)
        {
          _samples[i] = samples[iSource];
          _originalSamples[i] = samples[iSource];
          iSource++;
        }
      
    }

    public void FftBackward()
    {
      int handle = initFft();  
      _samples = BioAcoustics.calculateFftReversed(handle, _spectrum);
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
      retVal = inFile.readFile(name);
      _guano = inFile.Guano;
      if (retVal == 0)
      {
        _samples = new double[inFile.AudioSamples.Length];
        for (int i = 0; i < _samples.Length; i++)
          _samples[i] = ((double)inFile.AudioSamples[i]) / 32768.0;

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
        }
      }
      return retVal;
    }


    public void save()
    {
      try
      {
        WavFile outFile = new WavFile();
        outFile.createFile(1, _samplingRate, 0, Samples.Length - 1, Samples);
        outFile.saveFileAs(_fName);
      }
      catch (Exception ex)
      {
        DebugLog.log($"unable to save file: {_fName}  {ex.ToString()}", enLogType.ERROR);
      }
    }


    public void saveAs(string fName)
    {
      try
      {
        WavFile outFile = new WavFile();
        outFile.createFile(1, _samplingRate, 0, Samples.Length - 1, Samples);
        outFile.saveFileAs(fName);
      }
      catch (Exception ex)
      {
        DebugLog.log($"unable to save file: {_fName}  {ex.ToString()}", enLogType.ERROR);
      }
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
          string pngName = _fName.ToLower().Replace(AppParams.EXT_WAV, AppParams.EXT_IMG);
          if (File.Exists(pngName))
            File.Delete(pngName);
        }
        WavFile outFile = new WavFile();
        outFile.createFile(1, _samplingRate, 0, Samples.Length - 1, Samples);
        outFile.saveFileAs(name);
      }
      catch (Exception ex)
      {
        DebugLog.log($"unable to save file: {name}  {ex.ToString()}", enLogType.ERROR);
      }
    }


    public double findMax(out int index)
    {
      double max = 0;
      index = 0;
      for (int i = 0; i < _samples.Length; i++)
      {
        double s = Math.Abs(_samples[i]);
        if (s > max)
        {
          max = s;
          index = i;
        }
      }
      return max;
    }

    public void normalize(double maxVal = 0.95)
    {
      double max = findMax(out int index);
      double fact = maxVal / max;
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
      for (int i = 1; i < _spectrum.Length/2; i++)
      {
        int ib = _spectrum.Length - i - 1;
        double p = _spectrum[i] * _spectrum[i] + _spectrum[ib] + _spectrum[ib];
        if (p < limit)
        {
          _spectrum[i] = 0;
          _spectrum[ib] = 0;
        }
      }
    }

    public bool undo()
    {
      bool update = false;
      if(_originalSamples != null)
      {
        update = true;
        _samples = new double[_originalSamples.Length];
        Array.Copy(_originalSamples, _samples, _originalSamples.Length);
      }
      return update;
    }

    public void applyHeterodynModulation(double freq)
    {
      for(int i = 0; i < _samples.Length; i++)
      {
        double s = Math.Sin(2*Math.PI*i/_samplingRate * freq);
        _samples[i] *= s;
      }
    }

    int initFft()
    {
      int retVal = BioAcoustics.getFft((uint)_size, enWIN_TYPE.NONE);
      return retVal;
    }

    void findMaxPower(out double p, out double f)
    {
      p = 0;
      f = 0;
      int ib = _spectrum.Length - 2;
      for (int i = 1; i < _spectrum.Length/2; i++)
      {
        double p1 = _spectrum[i]*_spectrum[i] + _spectrum[ib] * _spectrum[ib];
        if(p1 > p)
        {
          p = p1;
          f = i;
        }
        ib--;
      }
      p = Math.Sqrt(p);
      f = _samplingRate / 2 * f / _spectrum.Length;
    }

    /// <summary>
    /// simulation of reduction of signal amplitude due to increasing distance to object.
    /// The amplitude of recording is reduced according the original and new recording distance
    /// The additional frequency dependent absorbtion of air is taken into account
    /// </summary>
    /// <param name="temp">ambient temperature [°C]</param>
    /// <param name="humidity">relative humidity [%]</param>
    /// <param name="distance1">original recording distance to object [m]</param>
    /// <param name="distance2">new distance to object [m]</param>
    public void applyDampening(double temp, double humidity, double distance1, double distance2)
    {
      FreqResponseRecord[] freqResponse = AirAbsorbtion.createFreqResponse(temp, humidity, distance1, distance2);
      applyFreqResponse(freqResponse, 1.0);
    }


    /// <summary>
    /// add a signal from another wav file. The other WAV file must be as least as long as the original file.
    /// With the factor it is possible to adjust the volume relative to the original
    /// </summary>
    /// <param name="wavName">name of wav file to add</param>
    /// <param name="fact">factor to apply to added signal (0.0 ... 1.0)</param>
    /// <returns></returns>
    public int addSignal(string wavName, double fact)
    {
      int retVal = 0;
      WavFile wav = new WavFile();
      if ((0 < fact) && (fact < 1.0))
      {
        double f1 = 1.0 - fact;
        retVal = wav.readFile(wavName);
        if (retVal == 0)
        {
          if (_samples.Length <= wav.AudioSamples.Length)
          {
            for (int i = 0; i < _samples.Length; i++)
              _samples[i] = f1 * _samples[i] + fact * wav.AudioSamples[i] / 32768.0;
          }
          else
            retVal = 2;
        }
      }
      else
        retVal = 3;

      return retVal;
    }
  }


  /// <summary>
  /// functions to calculate the damping of air in relation to frequency, temperature and rel. humidity
  /// </summary>
  public class AirAbsorbtion
  {
    static double p0 = 20e-6;  //ref. sound pressure ampitude [Pa];
    static double pa = 102.325; // ambient atmospheric pressure [kPa];
    static double pr = 102.325; // reference atmospheric pressure [kPa];
    static double t0 = 293;  // reference athmospheric temperatur [K]
    static double to1 = 273.16; //triple-point isotherm temp: 273.16 K = 273.15 + 0.01 K(0.01°C)


    public static FreqResponseRecord[] initFreqResponse()
    {
      int stepCnt = 33;

      double[] steps = new double[stepCnt];
      for (int i = 0; i < stepCnt; i++)
        steps[i] = Math.Pow(10, (double)i / (double)stepCnt);
      FreqResponseRecord[] retVal = new FreqResponseRecord[steps.Length * 6 / 5];
      retVal[0] = new FreqResponseRecord() { Amplitude = 0.0, Frequency = 100.0 };

      for (int s = 0; s < steps.Length; s++)
      {
        FreqResponseRecord r = new FreqResponseRecord() { Frequency = steps[s] * 10000.0, Amplitude = 0.0 };
        retVal[1 + s] = r;
      }

      for (int s = 0; s < steps.Length; s++)
      {
        FreqResponseRecord r = new FreqResponseRecord() { Frequency = steps[s] * 100000.0, Amplitude = 0.0 };
        if ((1 + s + steps.Length) >= retVal.Length)
          break;
        retVal[1 + s + steps.Length] = r;
      }
      return retVal;
    }

    public static FreqResponseRecord[] createFreqResponse(double temp, double humidity, double distance1, double distance2)
    {
      FreqResponseRecord[] fr = initFreqResponse();
      for(int i = 0; i < fr.Length; i++)
      {
        fr[i].Amplitude = damping(fr[i].Frequency, temp + 273.15, humidity) * (distance2 - distance1);
        fr[i].Amplitude += Math.Log10(distance2 / distance1) * 6.0 / Math.Log10(2);
      }
      return fr;
    }

    /// <summary>
    /// calculate relaxation frequency of oxygen
    /// </summary>
    /// <param name="h"> molar water concentration [%]</param>
    /// <returns></returns>
    static double frO(double h = 1.0)
    {
      //frO = (pa / pr) · (24 + 4.04 · 104 · h · ((0.02 + h) / (0.391 + h)))
      return pa / pr * (24 + 4.04 * 10000 * h * (0.02 + h) / (0.392 + h));
    }


    /// <summary>
    /// calculate relaxation frequency of N
    /// </summary>
    /// <param name="h"> molar water concentration [%]</param>
    /// <param name="t">ambient athmospheric temperature [K]</param>
    /// <returns></returns>
    static double frN(double t = 293.0, double h = 1.0)
    {
      //frN = (pa / pr) · (T / To)−1 / 2 · (9 + 280 · h · exp(−4.170 · ((T / To)−1 / 3−1)))
      return pa / pr * Math.Sqrt(t / t0) * (9 + 280 * h * Math.Exp(-4.170 * (Math.Pow(t / t0, 1.0 / 3.0) - 1.0)));
    }


    /// <summary>
    /// calculates the noise absorption of air 
    /// </summary>
    /// <param name="f">frequency [Hz]</param>
    /// <param name="t">temperature of air [K]</param>
    /// <param name="hr">relative humidity [%]</param>
    /// <returns>damping factor [dB/m]</returns>

    public static double damping(double f, double t, double hr)
    {
      double psat = pr * Math.Pow(10.0, -6.8346 * Math.Pow(to1 / t, 1.261) + 4.6151);
      double h = hr * (psat / pa);
      double z = 0.1068 * Math.Exp(-3352 / t) * Math.Pow(frN(t, h) + f * f / frN(t, h), -1.0);
      double y = Math.Pow(t / t0, -2.5) * (0.01275 * Math.Exp(-2239.1 / t) * Math.Pow(frO(h) + f * f / frO(h), -1.0) + z);
      double a = 8.686 * f * f * (1.84e-11 * Math.Pow(pa / pr, -1.0) * Math.Pow(t / t0, 0.5) + y);

      return a;
    }
  }
}
