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
using System.Windows.Interop;

namespace BatInspector.Forms
{
  /// <summary>
  /// Interaktionslogik für frmSettings.xaml
  /// </summary>
  public partial class frmSettings : Window
  {
    public frmSettings(AppParams settings)
    {
      InitializeComponent();
      _pg.SelectedObject = settings;
      Title = MyResources.Settings;
    }

    private void _btnFactSettings_Click(object sender, RoutedEventArgs e)
    {
      MessageBoxResult res = MessageBox.Show(MyResources.msgFactorySettings, MyResources.msgQuestion, MessageBoxButton.YesNo, MessageBoxImage.Question);
      if (res == MessageBoxResult.Yes)
      {
        DebugLog.log("application settings reset to factory settings", enLogType.INFO);
        AppParams.Inst.init();
      }
    }

    private void _btnCancel_Click(object sender, RoutedEventArgs e)
    {
      AppParams.load();
      DebugLog.log("Change of application settings cancelled", enLogType.INFO);
      this.Visibility = Visibility.Hidden;
    }

    private void _btnOk_Click(object sender, RoutedEventArgs e)
    {
      AppParams.Inst.save();
      setLanguage();

      DebugLog.log("Application Settings changed and saved", enLogType.INFO);
      this.Visibility = Visibility.Hidden;
    }
    private void setLanguage()
    {
      string culture = AppParams.Inst.Culture.ToString().Replace('_', '-');
      Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(culture);
      Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(culture);
    }

    private void _btnSaveAs_Click(object sender, RoutedEventArgs e)
    {
      System.Windows.Forms.SaveFileDialog frm = new System.Windows.Forms.SaveFileDialog();
      frm.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";
      System.Windows.Forms.DialogResult res = frm.ShowDialog();
      if(res == System.Windows.Forms.DialogResult.OK)
      {
        string fName = frm.FileName;
        AppParams.Inst.saveAs(fName);
      }
    }

    private void _btnLoad_Click(object sender, RoutedEventArgs e)
    {
      System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
      openFileDialog.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";
      if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
      {
        string fName = openFileDialog.FileName;
        AppParams.loadFrom(fName);
      }
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      winUtils.hideCloseButton(new WindowInteropHelper(this).Handle);
    }
  }
}
