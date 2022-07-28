
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
      _cbInspect.IsChecked = true;
      _cbCut.IsChecked = true;
      _cbPredict.IsChecked = true;
      _cbPrepare.IsChecked = true;
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
      if (_cbInspect.IsChecked == true)
        options |= ViewModel.OPT_INSPECT;
      if (_cbCut.IsChecked == true)
        options |= ViewModel.OPT_CUT;
      if (_cbPrepare.IsChecked == true)
        options |= ViewModel.OPT_PREPARE;
      if (_cbPredict.IsChecked == true)
        options |= ViewModel.OPT_PREDICT;
      _dlg(options);
      _frm.Close();
    }
  }
}
