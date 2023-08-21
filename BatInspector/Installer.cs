using BatInspector.Forms;
using libParser;
using libScripter;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace BatInspector
{
  public delegate void dlgInstall(bool instDat, bool instPy, bool instMod);
  public class Installer
  {
    const string INST_TOOLS = "install_tools.bat";
    const string INST_PYTHON = "python-3.10.10-amd64.exe";
    static string _outputData;
    public static void installToolsIfNotPresent(string pythonVer, string modelVer)
    {
      bool instData = !isDataInstalled();
      bool instPy = !isPythonInstalled("3.10");
      bool instMod = !isModelInstalled();

      if (instData || instPy || instMod)
      {
        frmInstall frm = new frmInstall(install);
        frm._cbData.IsChecked = instData;
        frm._cbData.IsEnabled = instData;
        frm._cbData.Content = BatInspector.Properties.MyResources.InstallerDataTxt1;
  
        frm._cbModel.IsChecked = instMod;
        frm._cbModel.IsEnabled = instMod;
        frm._cbModel.Content = BatInspector.Properties.MyResources.InstallerModTxt1 +
                              BatInspector.Properties.MyResources.InstallerModTxt2;
        
        frm._cbPython.IsEnabled = instPy;
        frm._cbPython.IsChecked = instPy;
        frm._cbPython.Content = BatInspector.Properties.MyResources.InstallerPyTxt1 + " 3.10.10\n\n" +
                              "**********    !!! " + BatInspector.Properties.MyResources.InstallerPyTxt2 + " !!!   *********\n" +
                              "*   "+ BatInspector.Properties.MyResources.InstallerPyTxt3 +
                              "******************************************\n\n" +
                              BatInspector.Properties.MyResources.InstallerPyTxt4 +
                              BatInspector.Properties.MyResources.InstallerPyTxt5 +
                              BatInspector.Properties.MyResources.InstallerPyTxt6 +
                              BatInspector.Properties.MyResources.InstallerPyTxt7;
      frm.ShowDialog();
      }
    }

    public static void install(bool instData, bool instPy, bool instMod)
    {
      try
      {
        bool restart = false;
        ProcessRunner p = new ProcessRunner();
        if (instData)
        {
          string srcDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                     AppParams.PROG_DAT_DIR);
          string dstDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments),
                                       AppParams.PROG_DAT_DIR);
          string fInstall = Path.Combine(srcDir, "install.txt");
            if (Directory.Exists(dstDir))
              Directory.Delete(dstDir, true);
            dirCopy(srcDir, dstDir);
            string inst = DateTime.Now.ToString();
            File.WriteAllText(fInstall, inst);
          restart = true;
        }

        if (instPy)
        {
            string wrkDir = Path.Combine(AppParams.AppDataPath, "setup");
            string exe = Path.Combine(wrkDir, INST_PYTHON);
            p.launchCommandLineApp(exe, null, wrkDir, true, "", true);
        }

        // install AI model
        if (instMod)
        {
            string wrkDir = Path.Combine(AppParams.AppDataPath, "setup");
            p.launchCommandLineApp("cmd.exe", null, wrkDir, true, "/C install_tools.bat", true);
        }
        if (restart)
        {
          Process.Start(AppDomain.CurrentDomain.FriendlyName);
          Environment.Exit(0);
        }
      }
      catch (Exception ex)
      {
        DebugLog.log("error during installation: " + ex.ToString(), enLogType.ERROR);
      }
    }


    static bool isDataInstalled()
    {
      string srcDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                 AppParams.PROG_DAT_DIR);
      string fInstall = Path.Combine(srcDir, "install.txt");
      string dstDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments),
                                   AppParams.PROG_DAT_DIR);

      bool retVal =  File.Exists(fInstall) && Directory.Exists(dstDir);
      return retVal;
    }

    static bool isPythonInstalled(string ver)
    {
      bool retVal = false;
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
      return retVal;
    }


    static bool isModelInstalled()
    {
      bool retVal = false;

      string fPath = AppParams.Inst.ModelRootPath;
      if (!Path.IsPathRooted(fPath))
        fPath = Path.Combine(AppParams.AppDataPath, AppParams.Inst.ModelRootPath);
      string venv = Path.Combine(fPath, "bd2", "_venv");
      if (Directory.Exists(venv))
      {
        string batdetect2 = Path.Combine(venv, "Scripts", "batdetect2.exe");
        if (File.Exists(batdetect2))
          retVal = true;
      }
      return retVal;
    }

    static void dirCopy(string src, string dst)
    {
      DirectoryInfo dir = new DirectoryInfo(src);
      Directory.CreateDirectory(dst);
      foreach (FileInfo f in dir.GetFiles())
      {
        string srcF = f.FullName;
        string dstF = Path.Combine(dst, f.Name);
        File.Copy(srcF, dstF, true);
      }

      foreach (DirectoryInfo d in dir.GetDirectories())
      {
        string srcD = d.FullName;
        string dstD = Path.Combine(dst, d.Name);
        Directory.CreateDirectory(dstD);
        dirCopy(srcD, dstD);
      }
    }
  }
}
