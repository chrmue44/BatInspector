
using System.Windows;


namespace BatInspector.Forms
{ 
  public delegate void dlgStartPrediction(int options);
  /// <summary>
  /// Interaction logic for frmStartPredict.xaml
  /// </summary>
  public partial class frmStartPredict : Window
  {
    static frmStartPredict _frm = null;
    dlgStartPrediction _dlg;

    public frmStartPredict(dlgStartPrediction startPrediction)
    {
      InitializeComponent();
      _dlg = startPrediction;
      _cbResample.IsChecked = true;
      _cbInspect.IsChecked = true;
      _cbCut.IsChecked = true;
      _cbPredict1.IsChecked = true;
      _cbPredict2.IsChecked = false;
      _cbPredict3.IsChecked = false;
      _cbPrepare.IsChecked = true;
      _cbConf95.IsChecked = true;
    }

    public static void showMsg(dlgStartPrediction dlg)
    {
      _frm = new frmStartPredict(dlg);
      _frm.Show();
    }

    private void _btnCancel_Click(object sender, RoutedEventArgs e)
    {
      _frm.Close();
    }

    private void _btnOk_Click(object sender, RoutedEventArgs e)
    {
      int options = 0;
      if (_cbResample.IsChecked == true)
        options |= ViewModel.OPT_RESAMPLE;
      if (_cbInspect.IsChecked == true)
        options |= ViewModel.OPT_INSPECT;
      if (_cbCut.IsChecked == true)
        options |= ViewModel.OPT_CUT;
      if (_cbPrepare.IsChecked == true)
        options |= ViewModel.OPT_PREPARE;
      if (_cbPredict1.IsChecked == true)
        options |= ViewModel.OPT_PREDICT1;
      if (_cbConf95.IsChecked == true)
        options |= ViewModel.OPT_CONF95;
      _frm.Close();
      _dlg(options);
    }
  }
}
