using BatInspector.Forms;
using libParser;
using libScripter;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Windows;

namespace BatInspector
{
  public class Installer
  {
    const string INST_TOOLS = "install_tools.bat";
    const string INST_PYTHON = "python-3.10.10-amd64.exe";
    static string _outputData;
    static SplashScreen _splashScreen;
    static public bool InstData { get; private set; } = false;
    static public bool InstPy { get; private set; } = false;
    static public bool InstMod { get; private set; } = false;


    static public void showSplash()
    {
      _splashScreen = new SplashScreen("..\\images\\splash.png");
      _splashScreen.Show(false);
    }

    static public void hideSplash() 
    {
      _splashScreen.Close(TimeSpan.FromSeconds(1));
    }

    public static bool checkTools(string pythonVer, string modelVer)
    {
      InstData = isDataInstalled(); // call before isModelInstalled
      InstPy = isPythonInstalled(pythonVer);
      InstMod = isModelInstalled();
      return InstData && InstPy && InstMod;
    }

    public static void install(bool instData, bool instPy, bool instMod)
    {
      try
      {
        ProcessRunner p = new ProcessRunner();
        if(instData)
        {
          string srcDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                                     AppParams.PROG_DAT_DIR, "setup", AppParams.DIR_SCRIPTS);
          string fInstall = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                                     AppParams.PROG_DAT_DIR, "install.txt");
          string dstDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                                       AppParams.PROG_DAT_DIR, AppParams.DIR_SCRIPTS);
          if(!Directory.Exists(dstDir))
            Directory.CreateDirectory(dstDir);
          dirCopy(srcDir, dstDir);
            string inst = DateTime.Now.ToString();
            File.WriteAllText(fInstall, inst);
        }

        if (instPy)
        {
          string wrkDir = Path.Combine(AppParams.AppDataPath, "setup");
          string exe = Path.Combine(wrkDir, INST_PYTHON);
          p.launchCommandLineApp(exe, null, "", true, "", true);    
        }

        // install AI model
        if (instMod)
        {
          string wrkDir = Path.Combine(AppParams.AppDataPath, "setup");
          string installBat = Path.Combine(wrkDir, "install_tools.bat");
          p.launchCommandLineApp("cmd.exe", null, wrkDir, true, "/C " + installBat, true);
        }
      }
      catch (Exception ex)
      {
        DebugLog.log("error during installation: " + ex.ToString(), enLogType.ERROR);
      }
    }



    static bool isDataInstalled()
    {
      string srcDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                                 AppParams.PROG_DAT_DIR, "setup", AppParams.DIR_SCRIPTS);
      string fInstall = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                                 AppParams.PROG_DAT_DIR, "install.txt");
      string dstDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                                   AppParams.PROG_DAT_DIR, AppParams.DIR_SCRIPTS);

      bool retVal =  File.Exists(fInstall) && Directory.Exists(dstDir);
      if (retVal)
      {
        string instTimeStr = File.ReadAllText(fInstall);
        DateTime.TryParse(instTimeStr, out DateTime instTime);
        if (Directory.Exists(dstDir) && Directory.Exists(srcDir))
        {
          DirectoryInfo src = new DirectoryInfo(srcDir);
          if (instTime < src.LastWriteTime)
          {
            if (!IsRunAsAdmin())
            {
              // Launch itself as administrator
              ProcessStartInfo proc = new ProcessStartInfo();
              proc.UseShellExecute = true;
              proc.WorkingDirectory = Environment.CurrentDirectory;
              proc.FileName = AppDomain.CurrentDomain.FriendlyName;
              proc.Verb = "runas";
              try
              {
                DebugLog.log("restart application in admin mode", enLogType.DEBUG);
                DebugLog.save();
                Process.Start(proc);
                Environment.Exit(0);
              }
              catch
              {
                // The user refused the elevation.
                // Do nothing and return directly ...
                return false;
              }
            }

            string delDir = srcDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                                                  AppParams.PROG_DAT_DIR);
            string venvDir = Path.Combine(delDir, "models", "bd2", "_venv");
            if (Directory.Exists(venvDir))
              Directory.Delete(venvDir, true);
            File.Delete(fInstall);
            string appName = Path.Combine(Environment.CurrentDirectory, AppDomain.CurrentDomain.FriendlyName);
            DebugLog.log("deleted data files, restart application in user mode: " + appName, enLogType.DEBUG);
            DebugLog.save();
            RunAsDesktopUser(appName);
            Environment.Exit(0);
            retVal = false;
          }
        }
      }
      return retVal;
    }


    static bool isPythonInstalled(string ver)
    {
      bool retVal = false;
      _outputData = "";
      ProcessRunner p = new ProcessRunner();
      string fName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                "$$where.txt");
      int err = p.launchCommandLineApp("cmd.exe", null, "", true, "/C where python > " + fName);
      if (err == 0)
      {
        _outputData = File.ReadAllText(fName);
        File.Delete(fName);
        int pos1 = _outputData.IndexOf("C:\\Pro");
        if (pos1 >= 0)
        {
          fName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                               "$$version.txt");
          err = p.launchCommandLineApp("cmd.exe", null, "", true,
                                        "/C python --version > "+ fName + " 2>&1");
          _outputData = File.ReadAllText(fName);
          File.Delete(fName);
          DebugLog.log("found " + _outputData, enLogType.INFO);
          string verStr = "Python " + ver;
          if (_outputData.IndexOf(verStr) >= 0)
              retVal = true;
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

    static public void restartApp()
    {
      DebugLog.save();
      Process.Start(AppDomain.CurrentDomain.FriendlyName);
      Environment.Exit(0);
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

    static public bool IsRunAsAdmin()
    {
      WindowsIdentity id = WindowsIdentity.GetCurrent();
      WindowsPrincipal principal = new WindowsPrincipal(id);
      return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }

    // https://stackoverflow.com/questions/11169431/how-to-start-a-new-process-without-administrator-privileges-from-a-process-with

    /// <summary>
    /// The function checks whether the current process is run as administrator.
    /// In other words, it dictates whether the primary access token of the 
    /// process belongs to user account that is a member of the local 
    /// Administrators group and it is elevated.
    /// </summary>
    /// <returns>
    /// Returns true if the primary access token of the process belongs to user 
    /// account that is a member of the local Administrators group and it is 
    /// elevated. Returns false if the token does not.
    /// </returns>

    private static void RunAsDesktopUser(string fileName)
    {
      if (string.IsNullOrWhiteSpace(fileName))
        throw new ArgumentException("Value cannot be null or whitespace.", nameof(fileName));

      // To start process as shell user you will need to carry out these steps:
      // 1. Enable the SeIncreaseQuotaPrivilege in your current token
      // 2. Get an HWND representing the desktop shell (GetShellWindow)
      // 3. Get the Process ID(PID) of the process associated with that window(GetWindowThreadProcessId)
      // 4. Open that process(OpenProcess)
      // 5. Get the access token from that process (OpenProcessToken)
      // 6. Make a primary token with that token(DuplicateTokenEx)
      // 7. Start the new process with that primary token(CreateProcessWithTokenW)

      var hProcessToken = IntPtr.Zero;
      // Enable SeIncreaseQuotaPrivilege in this process.  (This won't work if current process is not elevated.)
      try
      {
        var process = GetCurrentProcess();
        if (!OpenProcessToken(process, 0x0020, ref hProcessToken))
          return;

        var tkp = new TOKEN_PRIVILEGES
        {
          PrivilegeCount = 1,
          Privileges = new LUID_AND_ATTRIBUTES[1]
        };

        if (!LookupPrivilegeValue(null, "SeIncreaseQuotaPrivilege", ref tkp.Privileges[0].Luid))
          return;

        tkp.Privileges[0].Attributes = 0x00000002;

        if (!AdjustTokenPrivileges(hProcessToken, false, ref tkp, 0, IntPtr.Zero, IntPtr.Zero))
          return;
      }
      finally
      {
        CloseHandle(hProcessToken);
      }

      // Get an HWND representing the desktop shell.
      // CAVEATS:  This will fail if the shell is not running (crashed or terminated), or the default shell has been
      // replaced with a custom shell.  This also won't return what you probably want if Explorer has been terminated and
      // restarted elevated.
      var hwnd = GetShellWindow();
      if (hwnd == IntPtr.Zero)
        return;

      var hShellProcess = IntPtr.Zero;
      var hShellProcessToken = IntPtr.Zero;
      var hPrimaryToken = IntPtr.Zero;
      try
      {
        // Get the PID of the desktop shell process.
        uint dwPID;
        if (GetWindowThreadProcessId(hwnd, out dwPID) == 0)
          return;

        // Open the desktop shell process in order to query it (get the token)
        hShellProcess = OpenProcess(ProcessAccessFlags.QueryInformation, false, dwPID);
        if (hShellProcess == IntPtr.Zero)
          return;

        // Get the process token of the desktop shell.
        if (!OpenProcessToken(hShellProcess, 0x0002, ref hShellProcessToken))
          return;

        var dwTokenRights = 395U;

        // Duplicate the shell's process token to get a primary token.
        // Based on experimentation, this is the minimal set of rights required for CreateProcessWithTokenW (contrary to current documentation).
        if (!DuplicateTokenEx(hShellProcessToken, dwTokenRights, IntPtr.Zero, SECURITY_IMPERSONATION_LEVEL.SecurityImpersonation, TOKEN_TYPE.TokenPrimary, out hPrimaryToken))
          return;

        // Start the target process with the new token.
        var si = new STARTUPINFO();
        var pi = new PROCESS_INFORMATION();
        if (!CreateProcessWithTokenW(hPrimaryToken, 0, fileName, "", 0, IntPtr.Zero, Path.GetDirectoryName(fileName), ref si, out pi))
          return;
      }
      finally
      {
        CloseHandle(hShellProcessToken);
        CloseHandle(hPrimaryToken);
        CloseHandle(hShellProcess);
      }

    }

    #region Interop

    private struct TOKEN_PRIVILEGES
    {
      public UInt32 PrivilegeCount;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
      public LUID_AND_ATTRIBUTES[] Privileges;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    private struct LUID_AND_ATTRIBUTES
    {
      public LUID Luid;
      public UInt32 Attributes;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct LUID
    {
      public uint LowPart;
      public int HighPart;
    }

    [Flags]
    private enum ProcessAccessFlags : uint
    {
      All = 0x001F0FFF,
      Terminate = 0x00000001,
      CreateThread = 0x00000002,
      VirtualMemoryOperation = 0x00000008,
      VirtualMemoryRead = 0x00000010,
      VirtualMemoryWrite = 0x00000020,
      DuplicateHandle = 0x00000040,
      CreateProcess = 0x000000080,
      SetQuota = 0x00000100,
      SetInformation = 0x00000200,
      QueryInformation = 0x00000400,
      QueryLimitedInformation = 0x00001000,
      Synchronize = 0x00100000
    }

    private enum SECURITY_IMPERSONATION_LEVEL
    {
      SecurityAnonymous,
      SecurityIdentification,
      SecurityImpersonation,
      SecurityDelegation
    }

    private enum TOKEN_TYPE
    {
      TokenPrimary = 1,
      TokenImpersonation
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct PROCESS_INFORMATION
    {
      public IntPtr hProcess;
      public IntPtr hThread;
      public int dwProcessId;
      public int dwThreadId;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct STARTUPINFO
    {
      public Int32 cb;
      public string lpReserved;
      public string lpDesktop;
      public string lpTitle;
      public Int32 dwX;
      public Int32 dwY;
      public Int32 dwXSize;
      public Int32 dwYSize;
      public Int32 dwXCountChars;
      public Int32 dwYCountChars;
      public Int32 dwFillAttribute;
      public Int32 dwFlags;
      public Int16 wShowWindow;
      public Int16 cbReserved2;
      public IntPtr lpReserved2;
      public IntPtr hStdInput;
      public IntPtr hStdOutput;
      public IntPtr hStdError;
    }

    [DllImport("kernel32.dll", ExactSpelling = true)]
    private static extern IntPtr GetCurrentProcess();

    [DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
    private static extern bool OpenProcessToken(IntPtr h, int acc, ref IntPtr phtok);

    [DllImport("advapi32.dll", SetLastError = true)]
    private static extern bool LookupPrivilegeValue(string host, string name, ref LUID pluid);

    [DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
    private static extern bool AdjustTokenPrivileges(IntPtr htok, bool disall, ref TOKEN_PRIVILEGES newst, int len, IntPtr prev, IntPtr relen);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CloseHandle(IntPtr hObject);


    [DllImport("user32.dll")]
    private static extern IntPtr GetShellWindow();

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr OpenProcess(ProcessAccessFlags processAccess, bool bInheritHandle, uint processId);

    [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern bool DuplicateTokenEx(IntPtr hExistingToken, uint dwDesiredAccess, IntPtr lpTokenAttributes, SECURITY_IMPERSONATION_LEVEL impersonationLevel, TOKEN_TYPE tokenType, out IntPtr phNewToken);

    [DllImport("advapi32", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern bool CreateProcessWithTokenW(IntPtr hToken, int dwLogonFlags, string lpApplicationName, string lpCommandLine, int dwCreationFlags, IntPtr lpEnvironment, string lpCurrentDirectory, [In] ref STARTUPINFO lpStartupInfo, out PROCESS_INFORMATION lpProcessInformation);

    #endregion
  }
}
