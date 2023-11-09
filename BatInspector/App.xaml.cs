using libParser;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace BatInspector
{
  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  public partial class App : Application
  {
    public static string[] _args;

    [STAThread]
    public static void Main()
    {
  	  try
	    {
        var application = new App();
        application.InitializeComponent();
        application.Run();
	    }
	    catch (Exception e)
	    {
		    DebugLog.log(e.ToString(),enLogType.ERROR);
		    DebugLog.save();
	    }
    }

  //  [DllImport("Kernel32.dll")]
   // public static extern bool AttachConsole(int processId);


    protected override void OnStartup(StartupEventArgs e)
    {
      _args = e.Args;
      AppParams.load();
      if (_args.Length == 0)
      {
#if !DEBUG
        Installer.showSplash();
#endif
        /*
        this.Resources["colorBackGround"] = new SolidColorBrush(Color.FromArgb(0xFF, 40, 38, 43));
        this.Resources["colorForeGroundSlider"] = new SolidColorBrush(Color.FromArgb(0xFF, 70, 108, 103));
        this.Resources["colorBackGroundTextB"] = new SolidColorBrush(Color.FromArgb(0xFF, 20, 20, 20));
        this.Resources["colorBackGroundButton"] = new SolidColorBrush(Color.FromArgb(0xFF, 0xC0, 0xC0, 0xC0));
        this.Resources["colorForeGroundButton"] = new SolidColorBrush(Color.FromArgb(0xFF, 0, 0, 0));
        this.Resources["colorBackGroundToolB"] = new SolidColorBrush(Color.FromArgb(0xFF, 0xB0, 0xB0, 0xB0));
        this.Resources["colorBackGroundWindow"] = new SolidColorBrush(Color.FromArgb(0xFF, 10, 10, 10));
        this.Resources["colorBackGroundCheckB"] = new SolidColorBrush(Color.FromArgb(0xFF, 10, 10, 10));
        this.Resources["colorBackGroundCombo"] = new SolidColorBrush(Color.FromArgb(0xFF, 10, 10, 10));
        this.Resources["colorForeGroundLabel"] = new SolidColorBrush(Color.FromArgb(0xFF, 0xF0, 0xF0, 0xF0));
        this.Resources["colorBackGroundGrpFrame"] = new SolidColorBrush(Color.FromArgb(0xFF, 10, 10, 10));
        */


        DebugLog.setLogDelegate(null, null, null, AppParams.LogDataPath);

        // bool ok = Installer.checkTools("3.10", "1.0.6");
        // if (ok)
        this.StartupUri = new System.Uri("Forms/MainWindow.xaml", System.UriKind.Relative);
        // else
        //   this.StartupUri = new System.Uri("Forms/frmInstall.xaml", System.UriKind.Relative);
      }
      else
      {

//        AttachConsole(-1);
        DebugLog.setLogDelegate(null, null, null, AppParams.LogDataPath);
        int ret = CmdLine.main(_args);
        DebugLog.save();
        System.Environment.Exit(0);
      }
    }
  }
}
