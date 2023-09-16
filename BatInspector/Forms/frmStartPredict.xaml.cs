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
  public delegate void dlgStartPrediction(int options);
  /// <summary>
  /// Interaction logic for frmStartPredict.xaml
  /// </summary>
  public partial class frmStartPredict : Window
  {
    static frmStartPredict _frm = null;
    public static int Options { get; set; }

    public frmStartPredict()
    {
      InitializeComponent();
      _cbInspect.IsChecked = AppParams.Inst.PredIdentifyCalls;
      _cbCut.IsChecked = AppParams.Inst.PredCutCalls;
      _cbPredict1.IsChecked = AppParams.Inst.PredPredict1;
      _cbPredict2.IsChecked = AppParams.Inst.PredPredict2;
      _cbPredict3.IsChecked = AppParams.Inst.PredPredict3;
      _cbPrepare.IsChecked = AppParams.Inst.PredPrepData;
      _cbConf95.IsChecked = AppParams.Inst.PredConfTest;
      _cbCleanup.IsChecked = AppParams.Inst.PredDelTemp;
    }

    public static void showMsg()
    {
      _frm = new frmStartPredict();
      _frm.ShowDialog();
    }

    private void _btnCancel_Click(object sender, RoutedEventArgs e)
    {
      _frm.Close();
    }

    private void _btnOk_Click(object sender, RoutedEventArgs e)
    {
      int options = 0;

      if (_cbInspect.IsChecked == true)
        options |= ModelCmuTsa.OPT_INSPECT;
      AppParams.Inst.PredIdentifyCalls = (bool)_cbInspect.IsChecked;
      if (_cbCut.IsChecked == true)
        options |= ModelCmuTsa.OPT_CUT;
      AppParams.Inst.PredCutCalls = (bool)_cbCut.IsChecked;
      if (_cbPrepare.IsChecked == true)
        options |= ModelCmuTsa.OPT_PREPARE;
      AppParams.Inst.PredPrepData = (bool)_cbPrepare.IsChecked;
      if (_cbPredict1.IsChecked == true)
        options |= ModelCmuTsa.OPT_PREDICT1;
      AppParams.Inst.PredPredict1 = (bool)_cbPredict1.IsChecked;
      if (_cbPredict2.IsChecked == true)
        options |= ModelCmuTsa.OPT_PREDICT2;
      AppParams.Inst.PredPredict2 = (bool)_cbPredict2.IsChecked;
      if (_cbPredict3.IsChecked == true)
        options |= ModelCmuTsa.OPT_PREDICT3;
      AppParams.Inst.PredPredict3 = (bool)_cbPredict3.IsChecked;
      if (_cbConf95.IsChecked == true)
        options |= ModelCmuTsa.OPT_CONF95;
      AppParams.Inst.PredConfTest = (bool)_cbConf95.IsChecked;
      if (_cbCleanup.IsChecked == true)
        options |= ModelCmuTsa.OPT_CLEANUP;
      AppParams.Inst.PredDelTemp = (bool)_cbCleanup.IsChecked;
      Options = options;
      _frm.Close();
    }
  }
}
