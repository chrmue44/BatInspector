/********************************************************************************
 *               Author: Christian Müller
 *      Date of cration: 2021-08-10                                       
 *   Copyright (C) 2022: christian Müller christian(at)chrmue(dot).de
 *
 *              Licence:
 * 
 * THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND,
 * EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
 ********************************************************************************/
using System;
using System.Collections.Generic;

namespace libParser
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
   //   if (type == enLogType.ERROR)
   //     type = enLogType.ERROR;
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
