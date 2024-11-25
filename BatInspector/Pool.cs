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
    string Id { get; set; }
  }

  public class Pool<T> where T : class, IPool, new()
  {
    T[] _list;
    List<int> _free;
    int _size = 0;
    string[] _allocated; 


    public Pool(int Size)
    {
      _list = new T[Size];
      _allocated = new string[Size];
      _free = new List<int>();
      _size = Size;
      for (int i = 0; i < Size; i++)
      {
        _free.Add(i);
        _list[i] = new T();
        _list[i].setCallBack(releaseSelf);
        _allocated[i] = "";
      }
    }


    public T get(string id = "")
    {
      T retVal = null;
      if (_free.Count > 0)
      {
        int free = _free[0];
        _free.RemoveAt(0);
        retVal = _list[free];
        retVal.Id = id;
        _allocated[free] = $"{id}:{free}";
      }
      else
      {
        DebugLog.log($"no more elements in pool", enLogType.ERROR);
      }
      return retVal;
    }

    public T find(string id = "")
    {
      foreach(T t in _list) 
      {
        if(t.Id == id)
          return t;
      }
      return null;
    }


    private void release(T obj)
    {
      bool found = false;
      obj.Id = "";
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
