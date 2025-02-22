/********************************************************************************
 *               Author: Christian Müller
 *     Date of creation: 2021-08-10                                       
 *   Copyright (C) 2023: Christian Müller chrmue44(at)gmail(dot).com
 *
 *              Licence:  CC BY-NC 4.0 
 ********************************************************************************/
using System;
using System.Diagnostics;
using libParser;

namespace libScripter
{
  public delegate void StdOutDataRecHandler(string data);

  public class ProcessRunner
  {
    private Process _pr;
    public bool IsRunning { get { return checkIsRunning(); } }
    private bool _logOutput = false;
    private string[] _errData;
    private int _errDataIdx = 0;

    public ProcessRunner()
    {
      _errData = new string[10];
    }


    public int launchCommandLineApp(string exePath, DataReceivedEventHandler handler = null, string workDir = "", bool wait = false,
                                    string args = "", bool newWindow = false, bool logOutput = false, EventHandler exitHandler = null)
    {
      int retVal = 0;
      _pr = new Process();
      _logOutput = logOutput;
      _pr.StartInfo.FileName = exePath;
      _pr.StartInfo.Arguments = args;
      _pr.StartInfo.CreateNoWindow = !newWindow;
      _pr.StartInfo.UseShellExecute = false;
      if (exitHandler != null)
        _pr.Exited += exitHandler;
      if (!newWindow)
      {
        _pr.StartInfo.RedirectStandardOutput = true;
        _pr.StartInfo.RedirectStandardError = true;
        _pr.EnableRaisingEvents = true;
        _pr.ErrorDataReceived += errDataHandler;
        if (handler != null)
        {
          _pr.OutputDataReceived += handler;
        }
        else
        {
          _pr.OutputDataReceived += outputDataHandler;
//          _pr.ErrorDataReceived += outputDataHandler;
        }
      }
      if (workDir != "")
        _pr.StartInfo.WorkingDirectory = workDir;
      try
      {
        LogMsg("CMD " + exePath + " " + args, enLogType.INFO);
        _pr.Start();
        if (!newWindow)
          _pr.BeginOutputReadLine();
        if (wait)
        {
          _pr.WaitForExit();
          if (_pr.ExitCode != 0)
          {
            retVal = _pr.ExitCode;
            LogMsg("command " + exePath + " " + args + " terminated with exit code " + _pr.ExitCode.ToString() + "; workDir: " + workDir, enLogType.ERROR);
          }
          else
            LogMsg(_pr.ExitCode.ToString(), enLogType.INFO);
          _pr = null;
        }
      }
      catch (Exception ex)
      {
        LogMsg("Error starting " + exePath + " " + args + ": " + "; workDir: " + workDir + "; "+ ex.ToString(), enLogType.ERROR);
        retVal = 1;
        _pr = null;
      }
      return retVal;
    }

    public void Stop()
    {
      if (_pr != null)
      {
        try
        {
          _pr.Kill();
        }
        catch (Exception)
        {
        }
      }
    }

    private bool checkIsRunning()
    {
      if (_pr != null)
      {
        try
        {
          return !_pr.HasExited;
        }
        catch 
        {
          return false;
        }
      }
      else
        return false;
    }

    private void outputDataHandler(object sender, DataReceivedEventArgs ev)
    {
      if (_logOutput)
        DebugLog.log(ev.Data, enLogType.INFO);
    }

    private void errDataHandler(object sender, DataReceivedEventArgs ev)
    {
      _errData[_errDataIdx] = ev.Data;
      _errDataIdx++;
      if (_errDataIdx >= _errData.Length)
        _errDataIdx = 0;
    }

    public string ErrData 
    {
      get
      {
        string s = "";
        for (int i = _errDataIdx; i < _errData.Length; i++)
          s += _errData[i];
        for (int i = 0; i < _errDataIdx; i++)
          s += _errData[i];
        return s;
      }
    }

    private void LogMsg(string text, enLogType type)
    {
        DebugLog.log(text, type);
    }
  }
}
