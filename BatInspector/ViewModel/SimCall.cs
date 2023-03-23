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

    public SimCall(List<FreqItem> list, int sampleRate)
    {
      _file = new WavFile();
      _list = list;
      create(sampleRate);
    }


    public void create(int sampleRate)
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
           double f = (fEnd - fStart) * t + fStart;
           double a = (aEnd - aStart) * t + aStart;
           _samples[s] = Math.Sin(phi) * a;
          phi += 2 * Math.PI * f / sampleRate;
        }
      }
      _file.createFile(1, sampleRate, 0, _samples.Length - 1, _samples);
      _file.saveFileAs("temp.wav");
    }
  }
}
