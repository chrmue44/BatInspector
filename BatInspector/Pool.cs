using libParser;
using System.Collections.Generic;


namespace BatInspector
{
  public class Pool<T> where T : class, new()
  {
    T[] _list;
    List<int> _free;
    int _size = 0;

    public Pool(int Size)
    {
      _list = new T[Size];
      _free = new List<int>();
      _size = Size;
      for (int i = 0; i < Size; i++)
      {
        _free.Add(i);
        _list[i] = new T();
      }
    }

    public T get()
    {
      T retVal = default(T);
      if (_free.Count > 0)
      {
        int free = _free[0];
        _free.RemoveAt(0);
        retVal = _list[free];
      }
      else
      {
        DebugLog.log($"no more elements in pool", enLogType.ERROR);
      }
      return retVal;
    }

    public void release(T obj)
    {
      bool found = false;
      for (int i = 0; i < _list.Length; i++)
      {
        if (_list[i] == obj)
        {
          found = true;
          _free.Add(i);
        }
      }
      if (!found || (_free.Count > _size))
        DebugLog.log("pool error release", enLogType.ERROR);
    }
  }
}
