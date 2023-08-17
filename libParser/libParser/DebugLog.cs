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
using System.IO;

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

//  public delegate void delegateLogMsg(string Text, enLogType logType);
  public delegate void delegateLogEntry(stLogEntry entry, List<stLogEntry> list);
  public delegate void delegateLogClear();

  public class DebugLog
  {
    static DebugLog _inst = null;
    List<stLogEntry> _list;
    delegateLogEntry _dlgLog = null;
    delegateLogClear _dlgClear = null;
    string _logPath = null;
    string _fName = "";

    public static string FileName { get { return Inst()._fName; } }

    static public void log(string msg, enLogType type, bool beep = false)
    {
      if (beep)
        Console.Beep();
      Inst().logMsg(msg, type);
    }

    static public void save()
    {
      string log = "";
      foreach(stLogEntry entry in Inst()._list)
      {
        log += entry.Time.ToString() + " " + entry.Type.ToString() + " " + entry.Text + "\n"; 
      }
      if (Inst()._list.Count > 0)
      {
        if (!Directory.Exists(Inst()._logPath))
          Directory.CreateDirectory(Inst()._logPath);
        File.WriteAllText(Inst()._fName, log);
      }
    }

    static public void setLogDelegate(delegateLogEntry dlg, delegateLogClear dlgClear, string logPath)
    {
      Inst()._dlgLog = dlg;
      Inst()._dlgClear = dlgClear;
      Inst()._logPath= logPath;
      DateTime t = DateTime.Now;
      Inst()._fName = t.Year.ToString("D4") + t.Month.ToString("D2") + t.Day.ToString("D2") + "_" +
               t.Hour.ToString("D2") + t.Minute.ToString("D2") + t.Second.ToString("D2") + ".log";
      Inst()._fName = Path.Combine(Inst()._logPath, Inst()._fName);

    }

    static public void clear()
    {
      save();
      Inst()._list.Clear();
      Inst()._dlgClear();
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
