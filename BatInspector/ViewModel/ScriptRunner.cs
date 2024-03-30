/********************************************************************************
 *               Author: Christian Müller
 *     Date of creation: 2021-08-10                                       
 *   Copyright (C) 2023: Christian Müller chrmue44(at)gmail(dot).com
 *
 *              Licence:  CC BY-NC 4.0 
 ********************************************************************************/

using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using libParser;
using libScripter;

namespace BatInspector
{


  public class ScriptRunner
  {
    ProcessRunner _proc;
    delegateUpdateProgress _updProgress;
    Parser _parser;
    BaseCommands[] _cmds;
    string _wrkDir;
    MthdListScript _mthdListScript;
    ViewModel _model;

    public VarList VarList { get { return _parser.VarTable.VarList; } }
    public List<ScriptItem> Scripts { get { return AppParams.Inst.ScriptInventory.Scripts; } }
    public int CurrentLineNr { get { return _parser.CurrentLineNr; } }
    public ScriptRunner(ref ProcessRunner proc, string wrkDir, delegateUpdateProgress updProg, ViewModel model)
    {
      _proc = proc;
      _updProgress = updProg;
      _cmds = new BaseCommands[2];
      BatCommands batCmds = new BatCommands(updProg, model);
      _cmds[0] = batCmds;
      OsCommands os = new OsCommands(updProg);
      os.WorkDir = wrkDir;
      _wrkDir = wrkDir;
      _cmds[1] = os;
      _mthdListScript = new MthdListScript(model, wrkDir);
      _parser = new Parser(ref _proc, _cmds, wrkDir, _updProgress);
      _parser.addMethodList(_mthdListScript);
      _model = model;
    }

    public bool IsBusy { get { return _parser.Busy; } }

    public void execCmd(string cmd)
    {
      string[] lines = new string[1];
      lines[0] = cmd;

      _parser.ParseLines(lines);
      string res = _parser.VarTable.GetValue(Parser.ERROR_LEVEL);
      DebugLog.log("ConsoleCmd:" + cmd + ": " + res, enLogType.INFO);
    }

    public int runScript(string fileName, List<string> pars, bool background = true)
    {
      VarList.init();
      for (int i = 0; i < pars.Count; i++)
      {
        string varName = "PAR" + (i + 1).ToString();
        VarList.set(varName, pars[i]);
      }
      return runScript(fileName, background, false);
    }

    public int initScriptForDbg(string fileName)
    {
      int retVal = 0;
      fileName = checkScriptName(fileName);
      if (fileName != null)
      {
        initScriptVars(true);
        _parser.restartForDbg(fileName);
      }
      else
        retVal = 1;
      return retVal;
    }

    public int debugOneStep()
    {
      _parser.step();
      return _parser.CurrentLineNr;
    }

    public void continueDebugging()
    {
      if (_parser.CurrentLineNr > 0)
        _parser.step();
      _parser.continueParsing();
    }

    public int runScript(string fileName, bool background = true, bool initVars = true)
    {
      int retVal = 0;
      fileName = checkScriptName(fileName);
      if (fileName != null)
      {
        initScriptVars(initVars);
        if (background)
          _parser.StartParsing(fileName);
        else
        {
          retVal = _parser.ParseScript(fileName);
          if (_parser.VarTable.GetValue(Parser.ERROR_LEVEL) != "0")
            retVal = 2;
          if ((_parser.VarTable.GetValue(Parser.RET_VALUE) != "") &&
            (_parser.VarTable.GetValue(Parser.RET_VALUE) != "0"))
            retVal = 3;
        }
      }
      else
        retVal = 1;
      return retVal;
    }

    public void restartScript()
    {

      initScriptVars(true);
      _parser.restartScript();
    }

    public void cancelExecution()
    {
      _parser.StopParsing();
    }

    public void SetVariable(string name, string value)
    {
      DebugLog.log("SET " + name + " " + value, enLogType.INFO);
      if (Utils.isNum(value, false) && value.Length > 0)
        _parser.VarTable.VarList.set(name, int.Parse(value));
      else if (Utils.isNum(value, true))
      {
        double val = 0;
        double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out val);
        _parser.VarTable.VarList.set(name, val);
      }
      else
        _parser.VarTable.VarList.set(name, value);
    }

    public void setBreakCondition(int lineNr, string condition)
    {
      _parser.setBreakCondition(lineNr, condition);
    }

    public string getVariable(string name)
    {
      string retVal = _parser.VarTable.GetValue(name);
      DebugLog.log("GET " + name + ": " + retVal, enLogType.INFO);
      return retVal;
    }


    public void RemoveVariable(string name)
    {
      _parser.VarTable.Remove(name);
    }

    public ScriptItem getScript(string name)
    {
      ScriptItem retVal = null;
      foreach (ScriptItem sItem in AppParams.Inst.ScriptInventory.Scripts)
      {
        if (sItem.Name == name)
        {
          retVal = sItem;
          break;
        }
      }
      return retVal;
    }

    public List<ScriptItem> getScripts()
    {
      List<ScriptItem> retVal = new List<ScriptItem>();
      foreach (ScriptItem sItem in AppParams.Inst.ScriptInventory.Scripts)
        retVal.Add(sItem);
      return retVal;
    }

    public void setScripts(List<ScriptItem> list)
    {
      foreach (ScriptItem sItem in AppParams.Inst.ScriptInventory.Scripts)
      {
        bool foundFile = false;
        foreach (ScriptItem lItem in list)
        {
          if (lItem.Name == sItem.Name)
          {
            foundFile = true;
            break;
          }
        }
        if (!foundFile && File.Exists(sItem.Name))
          File.Delete(sItem.Name);
      }

      AppParams.Inst.ScriptInventory.Scripts = list;
    }

    public void saveScripts()
    {
      AppParams.Inst.ScriptInventory.save();
    }

    void outputDataHandler(object sender, DataReceivedEventArgs ev)
    {
      DebugLog.log(ev.Data, enLogType.INFO);
    }

    private string checkScriptName(string fileName)
    {
      string retVal = null;
      string ext = Path.GetExtension(fileName);
      if (ext.ToLower() == ".scr")
      {
        if (!System.IO.Path.IsPathRooted(fileName))
          retVal = System.IO.Path.Combine(AppParams.Inst.ScriptInventoryPath, fileName);
        else
          retVal = fileName;
      }
      else
        DebugLog.log("unknown script extension: " + ext, enLogType.ERROR);
      return retVal;
    }

    private void initScriptVars(bool initVars)
    {
      if (initVars)
        _parser.VarTable.VarList.init();
      SetVariable(AppParams.VAR_WRK_DIR, _wrkDir);
      SetVariable(AppParams.VAR_DATA_PATH, AppParams.AppDataPath);
    }
  }
}
