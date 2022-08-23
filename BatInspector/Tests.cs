using libParser;
using libScripter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatInspector
{

  struct stFormulaData
  {
    public stFormulaData(int i, string f, string r, string e)
    {
      Id = i;
      Formula = f;
      Result = r;
      Error = e;
    }

    public int Id;
    public string Formula;
    public string Result;
    public string Error;
  }

  public class Tests
  {
    ProcessRunner _proc;
    int _errors = 0;
    stFormulaData[] _dataForm;
    ViewModel _model;

  public Tests(ViewModel model)
    {
      _proc = new ProcessRunner();
      _model = model;
      _dataForm = new stFormulaData[]
        {
        new stFormulaData(1, "substr(\"ABCDE\",0,2)", "AB",""),
        new stFormulaData(2, "substr(\"ABCDE\",3,2)", "DE",""),
        new stFormulaData(3, "substr(\"ABCDE\",4,2)", "0","ARG2_OUT_OF_RANGE"),
        new stFormulaData(4, "cast(2.5,\"RT_INT\"", "2",""),
        };

    }

    public void exec()
    {
      testIf();
      testParser();
      if (_errors == 0)
        DebugLog.log("Tests passed", enLogType.INFO);
    }

    private void testIf()
    {
      string wrkDir = _model.Settings.ScriptDir;
      ScriptRunner scr = new ScriptRunner(ref _proc, wrkDir, null, null);

      scr.SetVariable("A", "55");
      scr.SetVariable("B", "34");
      scr.RunScript("test_if.scr", false);
      assert(scr.getVariable("Result"), "\"AlowBhigh\"");
      assert(scr.getVariable("Res2"), "\"AlowBhigh\"");

      scr.SetVariable("A", "55");
      scr.SetVariable("B", "32");
      scr.RunScript("test_if.scr", false);
      assert(scr.getVariable("Result"), "\"AlowBlow\"");
      assert(scr.getVariable("Res2"), "\"AlowBlow\"");

      scr.SetVariable("A", "101");
      scr.SetVariable("B", "34");
      scr.RunScript("test_if.scr", false);
      assert(scr.getVariable("Result"), "\"AhighBhigh\"");
      assert(scr.getVariable("Res2"), "\"AhighBhigh\"");

      scr.SetVariable("A", "101");
      scr.SetVariable("B", "33");
      scr.RunScript("test_if.scr", false);
      assert(scr.getVariable("Result"), "\"AhighBlow\"");
      assert(scr.getVariable("Res2"), "\"AhighBhigh\"");
    }

    int testParser()
    {

      int retVal = 0;
      DebugLog.log("Testing parser", enLogType.INFO);
      foreach (stFormulaData f in _dataForm)
      {
        string form = f.Formula;
        Expression exp = new Expression(null);
        string res = exp.parseToString(form);

        if ((f.Result != res) && (exp.Errors == 0))
        {
          DebugLog.log("Error calculation of formula '" + f.Formula + "'.  expected: " + f.Result + ", got: " + res, enLogType.ERROR);
          retVal++;
        }
        else if ((f.Error != "") && (f.Error != res))
        {
          DebugLog.log("wrong error message '" + f.Error + "'.  expected: " + f.Result + ", got: " + res, enLogType.ERROR);
          retVal++;
        }
      }
      return retVal;
    }

    private void assert(string a, string exp)
    {
      if( a != exp)
      {
        DebugLog.log("error", enLogType.ERROR);
        _errors++;
      }
    }
  }
}
