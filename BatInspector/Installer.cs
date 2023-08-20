using libParser;
using libScripter;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BatInspector
{
  public class Installer
  {
    const string INST_TOOLS = "install_tools.bat";
    const string INST_PYTHON = "python-3.10.10-amd64.exe";
    static string _outputData;
    public static void installToolsIfNotPresent(string pythonVer, string modelVer)
    {
      ProcessRunner p = new ProcessRunner();
      MessageBoxResult res;
      if (!isPythonInstalled("3.10"))
      {
        res = MessageBox.Show("Python 3.10.10 not yet installed. Continue installing it?\n\n " +
                              "**********    !!! IMPORTANT !!!  *********\n" +
                              "*   refer to chapter 2 in the manual\n" +
                              "******************************************\n\n" +
                              "1. start installer with click on YES\n" +
                              "2. Check the box 'add python.exe to path'\n" +
                              "3. start customize installation\n" +
                              "4. check box 'install for all users on 3rd page",
         "Question", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (res == MessageBoxResult.Yes)
        {
          string wrkDir = Path.Combine(AppParams.AppDataPath, "setup");
          string exe = Path.Combine(wrkDir, INST_PYTHON);
          p.launchCommandLineApp(exe, null, wrkDir, true, "", true);
        }
      }

      // install AI model
      if (!isModelInstalled())
      {
        res = MessageBox.Show("AI model batdetect2 not yet installed.\n" +
                              "Continue installing it?\n",
         "Question", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (res == MessageBoxResult.Yes)
        {
          string wrkDir = Path.Combine(AppParams.AppDataPath, "setup");
          p.launchCommandLineApp("cmd.exe", null, wrkDir, true, "/C install_tools.bat", true);
        }
      }
    }

    static bool isPythonInstalled(string ver)
    {
      bool retVal = false;
      try
      {
        _outputData = "";
        ProcessRunner p = new ProcessRunner();
        int err = p.launchCommandLineApp("cmd.exe", null,
                                         AppParams.AppDataPath, true, "/C where python > where.txt");
        if (err == 0)
        {
          string fName = Path.Combine(AppParams.AppDataPath, "where.txt");
          _outputData = File.ReadAllText(fName);
          File.Delete(fName);
          int pos1 = _outputData.IndexOf("C:\\Pro");
          if (pos1 >= 0)
          {
            err = p.launchCommandLineApp("cmd.exe", null,
                                          AppParams.AppDataPath, true,
                                          "/C python --version > version.txt 2>&1");
            if (err == 0)
            {
              fName = Path.Combine(AppParams.AppDataPath, "version.txt");
              _outputData = File.ReadAllText(fName);
              File.Delete(fName);
              string verStr = "Python " + ver;
              if (_outputData.IndexOf(verStr) >= 0)
              {
                retVal = true;
              }
            }
          }
        }
      }
      catch(Exception ex)
      {
        DebugLog.log("could not check for python version: " + ex.ToString(), enLogType.ERROR);
        retVal = false;
      }
      return retVal;
    }
    

    static bool isModelInstalled()
    {
      bool retVal = false;

      string fPath = AppParams.Inst.ModelRootPath;
      if (!Path.IsPathRooted(fPath))
        fPath = Path.Combine(AppParams.AppDataPath, AppParams.Inst.ModelRootPath);
      string venv = Path.Combine(fPath, "bd2","_venv");
      if (Directory.Exists(venv))
      {
        string batdetect2 = Path.Combine(venv, "Scripts", "batdetect2.exe");
        if(File.Exists(batdetect2))
          retVal = true;
      }
      return retVal;
    }
  }
}
