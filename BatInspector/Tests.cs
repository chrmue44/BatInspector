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
      string wrkDir = _model.Settings.ScriptDir == null ? "" : _model.Settings.ScriptDir;
      testIf(wrkDir);
      testWhile(wrkDir);
      testParser();
      testCsvFuncs(wrkDir);
      testClassifier();
      if (_errors == 0)
        DebugLog.log("Tests passed", enLogType.INFO);
    }

    private void testCsvFuncs(string wrkDir)
    {
      ScriptRunner scr = new ScriptRunner(ref _proc, wrkDir, null, null);
      scr.RunScript("test_csv.scr", false);
      assert(scr.getVariable("c23"), "23");
      assert(scr.getVariable("c123"), "123");
      assert(scr.getVariable("c43"), "43");
      assert(scr.getVariable("c55"), "55");
      assert(scr.getVariable("c62"), "62");
    }

    private void testWhile(string wrkDir)
    {
      ScriptRunner scr = new ScriptRunner(ref _proc, wrkDir, null, null);
      scr.SetVariable("Limit", "9");
      scr.SetVariable("b", "0");
      scr.RunScript("test_while.scr", false);
      assert(scr.getVariable("sum"), "9");
      scr.SetVariable("Limit", "23");
      scr.SetVariable("b", "0");
      scr.RunScript("test_while.scr", false);
      assert(scr.getVariable("sum"), "23");
      scr.SetVariable("Limit", "9");
      scr.SetVariable("b", "1");
      scr.RunScript("test_while.scr", false);
      assert(scr.getVariable("sum"), "5");
      scr.SetVariable("Limit", "23");
      scr.SetVariable("b", "1");
      scr.RunScript("test_while.scr", false);
      assert(scr.getVariable("sum"), "5");
    }

    private void testIf(string wrkDir)
    {
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

    private int testClassifier()
    {
      int retVal = 0;
      List<ParLocation> locs = new List<ParLocation>();
      locs.Add(new ParLocation(49.963175, 8.563220));
      locs.Add(new ParLocation(49.727209, 8.539645));
      locs.Add(new ParLocation(49.728493, 8.788927));
      locs.Add(new ParLocation(49.839054, 9.030958));
      locs.Add(new ParLocation(49.995876, 9.010885));

      ParLocation pos = new ParLocation(49.753933, 8.632084);
      bool ret = _model.Classifier.inside(pos, locs);
      assert("loc inside", ret == true);
      pos = new ParLocation(50.1, 8.632084);
      ret = _model.Classifier.inside(pos, locs);
      assert("loc inside", ret == false);
      pos = new ParLocation(49.8, 9.1);
      ret = _model.Classifier.inside(pos, locs);
      assert("loc inside", ret == false);
      pos = new ParLocation(49.8, 8.4);
      ret = _model.Classifier.inside(pos, locs);
      assert("loc inside", ret == false);

      return retVal;
    }
    private void assert(string a, string exp)
    {
      if( a != exp)
      {
        DebugLog.log("assertion error: " + exp + " != " + a, enLogType.ERROR);
        _errors++;
      }
    }

    private void assert(string a, bool exp)
    {
      if (!exp)
      {
        DebugLog.log("assertion error: " + a, enLogType.ERROR);
        _errors++;
      }
    }

  }
}
