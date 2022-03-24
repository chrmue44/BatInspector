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

    public Tests()
    {
      _proc = new ProcessRunner();
    }

    public void exec()
    {
      testIf();
    }

    private void testIf()
    {
      string wrkDir = "D:\\prj\\BatInspector\\scripts\\";
      ScriptRunner scr = new ScriptRunner(ref _proc, wrkDir, null);
      scr.RunScript(wrkDir + "test_if.scr");
    }
  }
}
