using libParser;
using System.Collections.Generic;
using System.Data;

namespace BatInspector
{
  public delegate void dlgRelease(object o);
  
  /// <summary>
  /// pooled objects have to callback the release delegate of their pool.
  /// Therefore they need to have a member to store the delegate individually
  /// release() calls the delegate provided by setCallBack.
  /// Multiple inheritance would be vvery helpful here...
  /// </summary>
  public interface IPool
  {
    void setCallBack(dlgRelease dlg);
    void release();
  }

  public class Pool<T> where T : class, IPool, new()
  {
    T[] _list;
    List<int> _free;
    int _size = 0;
    string[] _allocated; 


    public Pool(int Size)
    {
      _size = Size;
      reinitializePool();
    }

    public void reinitializePool()
    {      
      _list = new T[_size];
      _allocated = new string[_size];
      _free = new List<int>();
      for (int i = 0; i < _size; i++)
      {
        _free.Add(i);
        _list[i] = new T();
        _list[i].setCallBack(releaseSelf);
        _allocated[i] = "";
      }
    }


    public T get(string id = "")
    {
      T retVal = default(T);
      if (_free.Count > 0)
      {
        int free = _free[0];
        _free.RemoveAt(0);
        retVal = _list[free];
        _allocated[free] = $"{id}:{free}";
      }
      else
      {
        DebugLog.log($"no more elements in pool", enLogType.ERROR);
      }
      return retVal;
    }



    private void release(T obj)
    {
      bool found = false;
      for (int i = 0; i < _list.Length; i++)
      {
        if (_list[i] == obj)
        {
          found = true;
          _free.Add(i);
          _allocated[i] = "";
          break;
        }
      }
      HashSet<int> distinctNumbers = new HashSet<int>();

      foreach (int number in _free)
      {
        if (!distinctNumbers.Add(number))
          DebugLog.log("Pool element released twice!! (should NEVER happen", enLogType.ERROR);
      }

      if (!found || (_free.Count > _size))
        DebugLog.log("pool error release", enLogType.ERROR);
    }

    private void releaseSelf(object o)
    {
      T obj = o as T;
      release(obj);
    }
  }
}
