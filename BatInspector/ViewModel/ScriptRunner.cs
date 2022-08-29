using System.Diagnostics;
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

    public VarList VarList { get { return _parser.VarTable.VarList; } }
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
    }

    public bool IsBusy {  get { return _parser.Busy; } }

    public void execCmd(string cmd)
    {
      string[] lines = new string[1];
      lines[0] = cmd;
      _parser.ParseLines(lines);
      string res = _parser.VarTable.GetValue(Parser.ERROR_LEVEL);
      DebugLog.log("ConsoleCmd:" + cmd + ": " + res, enLogType.INFO);
    }
    public int RunScript(string fileName, bool background = true)
    {
      int retVal = 0;
      string ext = Path.GetExtension(fileName);
      if (ext.ToLower() == ".scr")
      {
        _parser.VarTable.VarList.init();
        SetVariable("WRK_DIR", _wrkDir);
        if (background)
          _parser.StartParsing(fileName);
        else {
          retVal = _parser.ParseScript(fileName);
          if (_parser.VarTable.GetValue(Parser.ERROR_LEVEL) != "0")
            retVal = 2;
          if((_parser.VarTable.GetValue(Parser.RET_VALUE) != "") &&
            (_parser.VarTable.GetValue(Parser.RET_VALUE) != "0") )
            retVal = 3;
        }
      }
      else
      {
        DebugLog.log("unknown script extension: " + ext, enLogType.ERROR);
        retVal = 1;
      }
      return retVal;
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
        double.TryParse(value, out val);
        _parser.VarTable.VarList.set(name, val);
      }
      else
        _parser.VarTable.VarList.set(name, value);
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

    void outputDataHandler(object sender, DataReceivedEventArgs ev)
    {
      DebugLog.log(ev.Data, enLogType.INFO);
    }
  }
}
