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
using System.Collections.ObjectModel;
using System.Diagnostics;
using libParser;

namespace libScripter
{
  public class OsCommands : BaseCommands
  {
    ProcessRunner _proc;
    string _lastDosResult;
    public OsCommands(delegateUpdateProgress delUpd) : base(delUpd)
    {

      _proc = new ProcessRunner();
      _features = new ReadOnlyCollection<OptItem>(new[]
      {
        new OptItem("DOS", "<pae> <par> ... execute dos command", 1, fctExecDosCmd),
        new OptItem("REN_DIR", "<old_name> <new_name> rename directory", 2, fctRenameDir),
        new OptItem("DEL_DIR", "<name> delete directory", 1, fctDeleteDir),
        new OptItem("DEL", "<name> deltet file", 1, fctDeleteFile),
        new OptItem("MKDIR", "<name> create directory", 1, fctMakeDir),
        new OptItem("REN", "<old_name> <new_name> rename file", 2, fctRenameFile),
        new OptItem("XCOPY", "<srcDir> <destDir>", 2, fctXcopyFile),
        new OptItem("REPLACE", "<file> <oldString> <newString> replace string in file", 3, fctReplace),
      });

      _options = new Options(_features, false);
    }

    public string WorkDir { get; set; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pars"></param> parameter 0: workDir, parameter 1..n DOS parameters
    /// <param name="ErrText"></param>
    /// <returns></returns>
    int fctExecDosCmd(List<string> pars, out string ErrText)
    {
      int retVal = 0;
      ErrText = "";
      _lastDosResult = "";
      string cmd = "cmd.exe";
      string args = "/C ";
      for (int i = 0; i < pars.Count; i++)
      {
        args += pars[i] + " ";
      }
      DebugLog.log(cmd + " " + args, enLogType.INFO);
      BaseCommands.EnableLog = true;
      retVal = _proc.LaunchCommandLineApp(cmd, dataHandler, WorkDir, true, args);
      _parser.VarTable.VarList.set("_DOS_RESULT", _lastDosResult);
      return retVal;
    }

    int fctRenameDir(List<string> pars, out string ErrText)
    {
      int retVal = 0;
      ErrText = "";
      DebugLog.log("REN_DIR "+ pars[0] + " " + pars[1], enLogType.INFO);
      try
      {
        Directory.Move(pars[0], pars[1]);
      }
      catch (Exception ex)
      {
        retVal = 1;
        ErrText = ex.ToString();
      }
      return retVal;
    }

    int fctRenameFile(List<string> pars, out string ErrText)
    {
      int retVal = 0;
      ErrText = "";
      DebugLog.log("REN " + pars[0] + " " + pars[1], enLogType.INFO);
      try
      {
        File.Move(pars[0], pars[1]);
      }
      catch (Exception ex)
      {
        retVal = 1;
        ErrText = ex.ToString();
      }
      return retVal;
    }


    int fctMakeDir(List<string> pars, out string ErrText)
    {
      int retVal = 0;
      ErrText = "";
      DebugLog.log("MK_DIR " + pars[0], enLogType.INFO);
      try
      {
        Directory.CreateDirectory(pars[0]);
      }
      catch (Exception ex)
      {
        retVal = 1;
        ErrText = ex.ToString();
      }
      return retVal;
    }

    int fctDeleteDir(List<string> pars, out string ErrText)
    {
      int retVal = 0;
      ErrText = "";
      DebugLog.log("DEL_DIR " + pars[0], enLogType.INFO);
      try
      {
        Directory.Delete(pars[0], true);
      }
      catch (Exception ex)
      {
        retVal = 1;
        ErrText = ex.ToString();
      }
      return retVal;
    }

    int fctDeleteFile(List<string> pars, out string ErrText)
    {
      int retVal = 0;
      ErrText = "";
      DebugLog.log("DEL " + pars[0], enLogType.INFO);
      try
      {
        File.Delete(pars[0]);
      }
      catch (Exception ex)
      {
        retVal = 1;
        ErrText = ex.ToString();
      }
      return retVal;
    }

    int fctXcopyFile(List<string> pars, out string ErrText)
    {
      int retVal = 0;
      ErrText = "";
      DebugLog.log("XCOPY " + pars[0] + " " + pars[1], enLogType.INFO);
      try
      {
        Utils.CopyFolder(pars[0], pars[1]);
      }
      catch (Exception ex)
      {
        retVal = 1;
        ErrText = ex.ToString();
      }
      return retVal;
    }

    int fctReplace(List<string> pars, out string ErrText)
    {
      int retVal = 0;
      ErrText = "";
      DebugLog.log("REPLACE " + pars[0] + " " + pars[1] + " " + pars[2], enLogType.INFO);
      try
      {
        string f = File.ReadAllText(pars[0]);
        f = f.Replace(pars[1], pars[2]);
        File.WriteAllText(pars[0], f);
      }
      catch (Exception ex)
      {
        retVal = 1;
        ErrText = ex.ToString();
      }
      return retVal;
    }

    private void dataHandler(object sender, DataReceivedEventArgs e)
    {
      _lastDosResult += e.Data;
      if(EnableLog)
        DebugLog.log(e.Data, enLogType.INFO);
    }
  }
}
