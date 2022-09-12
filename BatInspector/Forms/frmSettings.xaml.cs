/********************************************************************************
 *               Author: Christian Müller
 *      Date of cration: 2021-08-10                                       
 *   Copyright (C) 2022: Christian Müller christian(at)chrmue(dot).de
 *
 *              Licence:
 * 
 * THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND,
 * EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
 ********************************************************************************/
using BatInspector.Properties;
using libParser;
using System.Threading;
using System.Windows;

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
