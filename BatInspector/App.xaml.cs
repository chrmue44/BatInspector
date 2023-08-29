using BatInspector.Controls;
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
      Installer.showSplash();
      AppParams.load();
      DebugLog.setLogDelegate(null, null, null, AppParams.LogDataPath);

     // bool ok = Installer.checkTools("3.10", "1.0.6");
     // if (ok)
        this.StartupUri = new System.Uri("Forms/MainWindow.xaml", System.UriKind.Relative);
     // else
     //   this.StartupUri = new System.Uri("Forms/frmInstall.xaml", System.UriKind.Relative);
    }
  }
}
