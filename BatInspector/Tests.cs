using libParser;
using libScripter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatInspector
{
  public class Tests
  {
    ProcessRunner _proc;
    int _errors = 0;

    public Tests()
    {
      _proc = new ProcessRunner();
    }

    public void exec()
    {
      testIf();
      if (_errors == 0)
        DebugLog.log("Tests passed", enLogType.INFO);
    }

    private void testIf()
    {
      string wrkDir = "D:\\prj\\BatInspector\\scripts\\";
      ScriptRunner scr = new ScriptRunner(ref _proc, wrkDir, null);

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
      assert(scr.getVariable("Res2"), "\"AhighBlow\"");
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
