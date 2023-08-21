using System.Windows;


namespace BatInspector.Forms
{
  /// <summary>
  /// Interaction logic for frmInstall.xaml
  /// </summary>
  public partial class frmInstall : Window
  {
    dlgInstall _dlgInstall;
    public frmInstall(dlgInstall dlgInstall)
    {
      InitializeComponent();
      _dlgInstall = dlgInstall;
    }

    private void _btnOk_Click(object sender, RoutedEventArgs e)
    {
      if (_dlgInstall != null)
        _dlgInstall(_cbData.IsChecked == true, _cbPython.IsChecked == true, _cbModel.IsChecked == true);
      this.Close();
    }

    private void _btnCance_Click(object sender, RoutedEventArgs e)
    {
      this.Close();
    }
  }
}
