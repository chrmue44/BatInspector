using libParser;
using System;
using System.Windows;

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

    protected override void OnStartup(StartupEventArgs e)

    {
      _args = e.Args;
      Installer.showSplash();
      AppParams.load();
      //this.Resources["colorBackGroundToolB"] = new SolidColorBrush(Color.FromArgb(0xFF, 40, 38, 43));

      DebugLog.setLogDelegate(null, null, null, AppParams.LogDataPath);

     // bool ok = Installer.checkTools("3.10", "1.0.6");
     // if (ok)
        this.StartupUri = new System.Uri("Forms/MainWindow.xaml", System.UriKind.Relative);
     // else
     //   this.StartupUri = new System.Uri("Forms/frmInstall.xaml", System.UriKind.Relative);
    }
  }
}
