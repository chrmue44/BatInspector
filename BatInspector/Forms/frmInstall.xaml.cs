/********************************************************************************
 *               Author: Christian Müller
 *     Date of creation: 2021-08-10                                       
 *   Copyright (C) 2023: Christian Müller chrmue44(at)gmail(dot).com
 *
 *              Licence:  CC BY-NC 4.0 
 ********************************************************************************/

using System.Windows;



namespace BatInspector.Forms
{
  /// <summary>
  /// Interaction logic for frmInstall.xaml
  /// </summary>
  public partial class frmInstall : Window
  {
    public frmInstall()
    {
      InitializeComponent();
      if (Installer.IsRunAsAdmin())
        this.Title += "  (Admin Mode)";
      this._cbData.IsChecked = !Installer.InstData;
      this._cbData.IsEnabled = !Installer.InstData;
      this._cbData.Content = BatInspector.Properties.MyResources.InstallerDataTxt1;

      this._cbModel.IsChecked = !Installer.InstMod;
      this._cbModel.IsEnabled = !Installer.InstMod;
      this._cbModel.Content = BatInspector.Properties.MyResources.InstallerModTxt1 + "\n" +
                            BatInspector.Properties.MyResources.InstallerModTxt2;

      this._cbPython.IsEnabled = !Installer.InstPy;
      this._cbPython.IsChecked = !Installer.InstPy;
      this._cbPython.Content = BatInspector.Properties.MyResources.InstallerPyTxt1 + " 3.10.10\n\n" +
                            "**********    !!! " + BatInspector.Properties.MyResources.InstallerPyTxt2 + " !!!   *********\n" +
                            "*   " + BatInspector.Properties.MyResources.InstallerPyTxt3 + "\n" +
                            "******************************************\n\n" +
                            BatInspector.Properties.MyResources.InstallerPyTxt4 + "\n" +
                            BatInspector.Properties.MyResources.InstallerPyTxt5 + "\n" +
                            BatInspector.Properties.MyResources.InstallerPyTxt6 + "\n" +
                            BatInspector.Properties.MyResources.InstallerPyTxt7;
      Installer.hideSplash();
    }

    private void _btnOk_Click(object sender, RoutedEventArgs e)
    {
      Installer.install(_cbPython.IsChecked == true);
   //   MessageBox.Show("Please restart BatInspector!", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
      Installer.restartApp();
      this.Close();
    }

    private void _btnCance_Click(object sender, RoutedEventArgs e)
    {
      this.Close();
    }
  }
}
