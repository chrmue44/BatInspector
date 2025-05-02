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
    public void applyFreqResponse(FreqResponseRecord[] freqResponse)
    {
      FftForward();
      for (int i = 0; i < _spectrum.Length/2; i++)
      {
        double f = (double) i / _spectrum.Length * _samplingRate;
        double a = getAmplitude(f, freqResponse);
        double fact = Math.Pow(10.0, a / 10.0);
        _spectrum[i] /= fact;
        _spectrum[_spectrum.Length - i - 1] /= fact;
      }
      FftBackward();
    }


    public FreqResponseRecord[] createMicSpectrumFromNoiseFile()
    {
      FreqResponseRecord[] retVal = initFreqResponse();
      FftForward();
      int idxSpec = 0;
      int idxResp = 0;
      int avgCnt = 50;
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
        retVal[i].Amplitude -= db;
      retVal[retVal.Length - 1].Amplitude = 0;
      retVal[retVal.Length - 2].Amplitude = 0;


      return retVal;
    }

    private FreqResponseRecord[] initFreqResponse()
    {
      int pb = 4; // power of 10 begin
      int pe = 4; // power of ten end
      int nrAdd = 2; // number of additional values after pe
      double[] steps = { 1.0, 1.1, 1.2,1.3, 1.5,1.6, 1.8,2.0, 2.2,2.4, 2.7, 3.0, 3.3, 3.6, 3.9,
                         4.7, 5.1, 5.6, 6.2, 6.8,7.5, 8.2, 9.1 };
      FreqResponseRecord[] retVal = new FreqResponseRecord[(pe - pb + 1) * steps.Length + nrAdd];
      for (int d = pb; d <= pe; d++)
      {
        for (int s = 0; s < steps.Length; s++)
        {
          FreqResponseRecord r = new FreqResponseRecord() { Frequency = steps[s] * Math.Pow(10, d), Amplitude = 0.0 };
          retVal[s + (d - pb) * steps.Length] = r;
        }
      }
      for (int s = 0; s < nrAdd; s++)
      {
        FreqResponseRecord r = new FreqResponseRecord() { Frequency = steps[s] * Math.Pow(10, pe + 1), Amplitude = 0.0 };
        retVal[s + (pe - pb + 1) * steps.Length] = r;
      }
      return retVal;
    }

    private double getAmplitude(double f, FreqResponseRecord[] freqResponse)
    {
      double retVal = 0.0;
      if (f > 15000)
        f = f + 1; //@@@
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
      retVal = inFile.readFile(name, ref _samples);
      if (retVal == 0)
      {
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
  }
}
