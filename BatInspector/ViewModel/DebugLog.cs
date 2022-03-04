using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatInspector
{
  public enum enLogType
  {
    ERROR = 0,
    WARNING = 1,
    INFO = 2,
    DEBUG = 3
  }

  public struct stLogEntry
  {
    public stLogEntry(string text, enLogType type, DateTime time)
    {
      Text = text;
      Type = type;
      Time = time;
    }
    public string Text;
    public enLogType Type;
    public DateTime Time;
  }

  public delegate void delegateLogMsg(string Text, enLogType logType);
  public delegate void delegateLogEntry(stLogEntry entry, List<stLogEntry> list);

  public class DebugLog
  {
    static DebugLog _inst = null;
    List<stLogEntry> _list;
    delegateLogEntry _dlgLog = null;

    static public void log(string msg, enLogType type)
    {
      Inst().logMsg(msg, type);
    }

    static public void setLogDelegate(delegateLogEntry dlg)
    {
      Inst()._dlgLog = dlg;
    }

    DebugLog()
    {
      _list = new List<stLogEntry>();
    }

    static DebugLog Inst()
    {
      if (_inst == null)
        _inst = new DebugLog();
      return _inst;
    }

    void logMsg(string msg, enLogType type)
    {
      stLogEntry entry = new stLogEntry(msg, type, DateTime.Now);
      if (_dlgLog != null)
        _dlgLog(entry, _list);
    }
  }
}
