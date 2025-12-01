/********************************************************************************
 *               Author: Christian Müller
 *     Date of creation: 2023-09-01                                       
 *   Copyright (C) 2023: Christian Müller chrmue44(at)gmail(dot).com
 *
 *              Licence:  CC BY-NC 4.0 
 ********************************************************************************/

using System;
using System.Collections.Generic;


namespace BatInspector
{
  public class FreqItem
  {
    double _f;
    double _t;
    double _a;
    public FreqItem(double f, double t, double a)
    {
      _f = f;
      _t = t;
      _a = a;
    }
    
    public double F { get { return _f; } }
    public double T { get { return _t;} }
    public double A { get { return _a;} }
  }

  public class SimCall
  {
    WavFile _file;
    List<FreqItem> _list;
    double[] _samples;

    public SimCall(List<FreqItem> list)
    {
      _file = new WavFile();
      _list = list;
    }

    /// <summary>
    /// create a file from list of points
    /// </summary>
    /// <param name="sampleRate"></param>
    /// <param name="outFile">name of output file</param>
    /// <param name="interpolate">true: interpolate between points</param>
    public void create(int sampleRate, string outFile, bool interpolate)
    {
      int count = (int)(_list[_list.Count - 1].T * (double)sampleRate);
      _samples = new double[count];
      double phi = 0;
      for (int i = 1; i < _list.Count; i++)
      {
        double fStart = _list[i - 1].F;
        int tStart = (int)(_list[i - 1].T * sampleRate);
        double aStart = _list[i - 1].A;
        double fEnd = _list[i].F;
        double aEnd = _list[i].A;
        int tEnd = (int)(_list[i].T * sampleRate);
        for (int s = tStart; s < tEnd; s++)
        {
           double t = (double)(s - tStart) / (tEnd - tStart);
           double f = interpolate ? (fEnd - fStart) * t + fStart : fStart;
           double a = interpolate ? (aEnd - aStart) * t + aStart : aStart;
           _samples[s] = Math.Sin(phi) * a;
          phi += 2 * Math.PI * f / sampleRate;
        }
      }
      _file.createFile(1, sampleRate, 0, _samples.Length - 1, _samples);
      _file.saveFileAs(outFile);
    }
  }
}
