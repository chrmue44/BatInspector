/********************************************************************************
 *               Author: Christian Müller
 *     Date of creation: 2021-08-10                                       
 *   Copyright (C) 2023: Christian Müller chrmue44(at)gmail(dot).com
 *
 *              Licence:  CC BY-NC 4.0 
 ********************************************************************************/
using System.Collections.Generic;
using System.IO;
using libParser;

namespace libScripter
{
  public class BaseCommands
  {
    public static bool EnableLog {get; set;}
    protected IList<OptItem> _features;
    protected Options _options;
    protected bool isLogOpen = false;
    protected StreamWriter logFile;
    protected delegateUpdateProgress _updateProgress;
    protected Parser _parser;
 
    public BaseCommands(delegateUpdateProgress delUpd)
    {
      _updateProgress = delUpd;
    }

    public string run(string[] args, Parser parser)
    {
      string retVal = "0";
      _parser = parser;
      enErrOption err = _options.parseCmdLine(args);

      if (err == enErrOption.OK)
      {
        LogMsg(CmdLine(args), enLogType.INFO);
        err = _options.execute();
        if (err != enErrOption.OK)
          LogMsg(_options.ErrorText, enLogType.DEBUG);
        else
          LogMsg("0", enLogType.DEBUG);
      }

      if (err != enErrOption.OK)
      {
        retVal = "1 " + _options.ErrorText;
      }
      return retVal;
    }

    public string getOptions()
    {
      string retVal = "";
      foreach(OptItem item in _features)
      {
        retVal += item.Help + "\n";
      }
      return retVal;
    }

    string CmdLine(string[] args)
    {
      string retVal = "";
      foreach (string arg in args)
      {
        retVal += arg + " ";
      }
      return retVal;
    }
    protected void LogMsg(string text, enLogType color)
    {
      if (EnableLog || (color == enLogType.ERROR))
        DebugLog.log(text, color);
    }


   /* protected void UpdateProgess(int progress)
    {
      if (_updateProgress != null)
        _updateProgress(progress);
    } */
  }
}
