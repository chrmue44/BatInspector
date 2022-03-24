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
using System.Collections.Generic;

namespace libScripter
{
  public delegate int delegateTiaFunction(List<string> paramList, out string ErrorText);

  public enum enErrOption
  {
    OK,
    OPT_NOT_FOUND,
    FUNC_ERR,
    PARAM_ERROR,    ///not enough parameters for function
  }

  /// <summary>
  /// representation of one feature 
  /// </summary>
  public class OptItem
  {
    public OptItem(string option, string help, int parCount, delegateTiaFunction func)
    {
      this.Option = option;
      this.Help = help;
      this.ParamCount = parCount;
      this.Params = new List<string>();
      this.Func = func;
    }

    public OptItem(OptItem item)
    {
      this.Option = item.Option;
      this.Help = item.Help;
      this.ParamCount = item.ParamCount;
      this.Params = new List<string>();
      this.Func = item.Func;

    }

    public void addParam(string par)
    {
      Params.Add(par);
    }

    public string Option { get; set; }
    public string Help { get; set; }
    public int ParamCount { get; set; }
    public List<string> Params { get; set; }
    public delegateTiaFunction Func { get; set; }
  }



  public class Options
  {
    List<OptItem> _items;
    IList<OptItem> _features;
    bool _isCmdLineOption;
    public string ErrorText { get; set; }

    /// <summary>
    /// constructor
    /// </summary>
    public Options(IList<OptItem> features, bool isCommandLineOption = true)
    {
      _isCmdLineOption = isCommandLineOption;
      _items = new List<OptItem>();
      _features = features;
    }

    /// <summary>
    /// parse command line options. At least one option with '-' is required.
    /// Every option may have 0 to n parameters delimited by a space
    /// </summary>
    /// <param name="args"></param>
    /// <returns>error code</returns>        
    public enErrOption parseCmdLine(string[] args)
    {
      enErrOption retVal = enErrOption.OK;
      int i = 0;
      _items.Clear();
      ErrorText = "";
      bool found = false;
      while (i < args.Length) 
      {
        foreach (OptItem feat in _features) 
        {
          string arg = args[i].Replace(" ", "");
          if (feat.Option == arg) {
            found = true;
            OptItem optItem = new OptItem(feat);
            if (feat.ParamCount > 0)
            {
              while (i < args.Length)
              {
                i++;
                if ((i >= args.Length) || (args[i].Length > 0) && (args[i].Substring(0,1)== "-") && _isCmdLineOption)
                  break;
                else
                  optItem.addParam(args[i]);
              }
            }

  //          DebugLog.log(optItem.Option);
  //            foreach(string par in optItem.Params)
  //            DebugLog.log("Parameter: " + par);
            if ((args.Length - 1) < feat.ParamCount)
            {
              ErrorText = "not enough parameters for command " + feat.Option;
              retVal = enErrOption.PARAM_ERROR;
            }
            else
            {
              ErrorText = "";
              _items.Add(optItem);
            }
          }
          if (i >= args.Length)
            break;
        }

        if (!found)
        {
          retVal = enErrOption.OPT_NOT_FOUND;
          ErrorText = "unknown option: " + args[i];
          break;
        }

        i++;
      }
      return retVal;
    }

    public enErrOption execute()
    {
      enErrOption retVal = enErrOption.OK;
      foreach (OptItem item in _items)
      {
        string errText;
        int ret = item.Func(item.Params, out errText);
        ErrorText = errText;
        if (ret != 0)
          retVal = enErrOption.FUNC_ERR;
      }
      return retVal;
    }
  }
}
