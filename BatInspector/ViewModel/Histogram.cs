using System;
using System.Collections.Generic;

namespace BatInspector
{
  public class Histogram
  {
    public Histogram(int c)
    {
      _list = new List<int>();
      _cntClasses = c;
    }
    
    public List<int> Classes { get { return _list; } }

    public double Mean { get { return _cnt > 0 ? _sum / _cnt : 0; } }
    public double Min { get { return _min; } }
    public double Max { get { return _max; } }

    public int Count { get { return _cnt; } }
    public double StdDev {  get {
        if (_cnt < 2)
          return 0;
        else
          return Math.Sqrt((_sumSquare - _sum * _sum/_cnt) / (_cnt - 1));
      } }

    public int MaxCount 
    {
      get
      {
        int cnt = 0;
        for (int i = 0; i < _cntClasses; i++)
        {
          if (cnt < _list[i])
            cnt = _list[i];
        }
        return cnt;
      }
    }

    public void add(double val)
    {
      _sumSquare += val * val;
      _sum += val;
      _cnt++;
      int idx = (int)(val/(_max - _min) * _cntClasses);
      if (idx < 0)
        idx = 0;
      if(idx > (_cntClasses - 1))
        idx = _cntClasses - 1;
      _list[idx]++;        
    }

    public double getClassMin(int i)
    {
      if(i < 0)
        i= 0;
      if(i > (_cntClasses - 1))
        i = _cntClasses - 1;
      return (_max - _min) * i / _cntClasses + _min;
    }

    public double getClassMax(int i)
    {
      if (i < 0)
        i = 0;
      if (i > (_cntClasses - 1))
        i = _cntClasses - 1;
      return (_max - _min) * (i + 1) / _cntClasses + _min;
    }

    public void init(double min, double max)
    {
      _min = min;
      _max = max;
      _sum = 0;
      _sumSquare = 0;
      if (_max == _min)
        _max = _min + 1.0;
      _list.Clear();
      _cnt = 0;
      for(int i = 0; i < _cntClasses; i++)
        _list.Add(0);
    }

    List<int> _list = new List<int>();
    int _cntClasses = 0;
    int _cnt = 0;
    double _sum = 0;
    double _sumSquare = 0;
    double _min = 0;
    double _max = 0;
  }
}
