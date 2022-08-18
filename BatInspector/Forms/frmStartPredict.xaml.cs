
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
    AppParams _pars;

    public frmStartPredict(dlgStartPrediction startPrediction, AppParams pars)
    {
      InitializeComponent();
      _dlg = startPrediction;
      _pars = pars;
      _cbResample.IsChecked = _pars.PredAdaptSampleRate;
      _cbInspect.IsChecked = _pars.PredIdentifyCalls;
      _cbCut.IsChecked = _pars.PredCutCalls;
      _cbPredict1.IsChecked = _pars.PredPredict1;
      _cbPredict2.IsChecked = _pars.PredPredict2;
      _cbPredict3.IsChecked = _pars.PredPredict3;
      _cbPrepare.IsChecked = _pars.PredPrepData;
      _cbConf95.IsChecked = _pars.PredConfTest;
      _cbCleanup.IsChecked = _pars.PredDelTemp;
    }

    public static void showMsg(dlgStartPrediction dlg, AppParams pars)
    {
      _frm = new frmStartPredict(dlg, pars);
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
      _pars.PredAdaptSampleRate = (bool)_cbResample.IsChecked;
      if (_cbInspect.IsChecked == true)
        options |= ViewModel.OPT_INSPECT;
      _pars.PredIdentifyCalls = (bool)_cbInspect.IsChecked;
      if (_cbCut.IsChecked == true)
        options |= ViewModel.OPT_CUT;
      _pars.PredCutCalls = (bool)_cbCut.IsChecked;
      if (_cbPrepare.IsChecked == true)
        options |= ViewModel.OPT_PREPARE;
      _pars.PredPrepData = (bool)_cbPrepare.IsChecked;
      if (_cbPredict1.IsChecked == true)
        options |= ViewModel.OPT_PREDICT1;
      _pars.PredPredict1 = (bool)_cbPredict1.IsChecked;
      if (_cbPredict2.IsChecked == true)
        options |= ViewModel.OPT_PREDICT2;
      _pars.PredPredict2 = (bool)_cbPredict2.IsChecked;
      if (_cbPredict3.IsChecked == true)
        options |= ViewModel.OPT_PREDICT3;
      _pars.PredPredict3 = (bool)_cbPredict3.IsChecked;
      if (_cbConf95.IsChecked == true)
        options |= ViewModel.OPT_CONF95;
      _pars.PredConfTest = (bool)_cbConf95.IsChecked;
      if (_cbCleanup.IsChecked == true)
        options |= ViewModel.OPT_CLEANUP;
      _pars.PredDelTemp = (bool)_cbCleanup.IsChecked;
      _frm.Close();
      _dlg(options);
    }
  }
}
