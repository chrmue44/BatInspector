using NAudio.SoundFont;
using System;
using System.IO;
using System.Reflection;

namespace BatInspector
{
  public class CmdLine
  {
    static ViewModel _model;

    public static int main(string[] args)
    {
      _model = new ViewModel(null, null);
      int retVal = 0;
      if(args.Length == 0)
        return 1;

      switch(args[0]) 
      {
        case "--script":
          if(args.Length > 1)
            retVal = runScript(args[1]);
          break;

        case "--version":
          Version version = Assembly.GetExecutingAssembly().GetName().Version;
          DateTime linkTimeLocal = System.IO.File.GetLastWriteTime(Assembly.GetExecutingAssembly().Location);
          string versionStr = "BatInspector V" + version.ToString() + " " + linkTimeLocal.ToString();
          Console.WriteLine(versionStr);
          break;
        default:
          Console.WriteLine("unkonwn option: " + args[0]);
          break;
      }
      return retVal;
    }


    public static int runScript(string script) 
    {
      int retVal = 0;
      string fullPath = Path.Combine(AppParams.AppDataPath, AppParams.DIR_SCRIPT, script);
      _model.Scripter.runScript(fullPath, true, false);
      return retVal;
    }
  }
}
