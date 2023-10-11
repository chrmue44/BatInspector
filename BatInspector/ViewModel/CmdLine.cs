using System.IO;

namespace BatInspector
{
  public class CmdLine
  {
    static ViewModel _model;

    public static int main(string[] args)
    {
      _model = new ViewModel(null, "1", null);
      int retVal = 0;
      if(args.Length == 0)
        return 1;

      switch(args[0]) 
      {
        case "-script":
          if(args.Length > 1)
            retVal = runScript(args[1]);
          break;
      }
      return retVal;
    }


    public static int runScript(string script) 
    {
      int retVal = 0;
      string fullPath = Path.Combine(AppParams.AppDataPath, AppParams.DIR_SCRIPT, script);
      _model.executeScript(fullPath, true, false);
      return retVal;
    }
  }
}
