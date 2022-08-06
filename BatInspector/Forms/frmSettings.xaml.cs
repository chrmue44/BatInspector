using BatInspector.Properties;
using libParser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BatInspector.Forms
{
  /// <summary>
  /// Interaktionslogik für frmSettings.xaml
  /// </summary>
  public partial class frmSettings : Window
  {
    AppParams _settings;
    public frmSettings(AppParams settings)
    {
      InitializeComponent();
      _settings = settings;
      _pg.SelectedObject= settings;
      Title = MyResources.Settings;
    }

    private void _btnFactSettings_Click(object sender, RoutedEventArgs e)
    {
      MessageBoxResult res = MessageBox.Show(MyResources.msgFactorySettings, MyResources.msgQuestion, MessageBoxButton.YesNo, MessageBoxImage.Question);
      if (res == MessageBoxResult.Yes)
      {
         DebugLog.log("application settings reset to factory settings", enLogType.INFO);
        _settings.init();
      }
    }

    private void _btnCancel_Click(object sender, RoutedEventArgs e)
    {
      _settings = AppParams.load();
      DebugLog.log("Change of application settings cancelled", enLogType.INFO);
      this.Close();
    }

    private void _btnOk_Click(object sender, RoutedEventArgs e)
    {
      _settings.save();
      setLanguage();

      DebugLog.log("Application Settings changed and saved", enLogType.INFO);
      this.Close();
    }
    private void setLanguage()
    {
      string culture = _settings.Culture.ToString().Replace('_', '-');
      Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(culture);
      Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(culture);
    }

  }
}
