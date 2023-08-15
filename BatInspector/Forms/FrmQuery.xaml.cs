using System.Threading;
using System.Windows;
using System.Windows.Interop;


namespace BatInspector.Forms
{
  /// <summary>
  /// Interaction logic for FrmQuery.xaml
  /// </summary>
  public partial class FrmQuery : Window
  {
    ViewModel _model;
    FrmHelpFilter _frmHelp = null;
    public FrmQuery(ViewModel model)
    {
      InitializeComponent();
      _model = model;
      int wLabel = 150;
      _ctlName.setup( BatInspector.Properties.MyResources.frmQueryQueryName, Controls.enDataType.STRING, 0, wLabel, true);
      _ctlSelectSource.setup(BatInspector.Properties.MyResources.frmQuerySelSrcDir, wLabel, true);
      _ctlSelectDest.setup(BatInspector.Properties.MyResources.frmQuerySelDestDir, wLabel, true);
      _grdCol1.Width = new GridLength(wLabel);
    }

    public void initFieldsFromQuery()
    {
      if(_model.Query != null)
      {
        _ctlName.setValue(_model.Query.Name);
        _ctlSelectSource.setValue(_model.Query.SrcDir);
        _ctlSelectDest.setValue(_model.Query.DestDir);
        _tbQuery.Text = _model.Query.Expression;
      }
    }
    private void btnCancel_Click(object sender, RoutedEventArgs e)
    {
      this.Visibility = Visibility.Hidden;
      if(_frmHelp != null)
        _frmHelp.Close();
    }

    private void btnOK_Click(object sender, RoutedEventArgs e)
    {
      _model.Query = new Query(_ctlName.getValue(), _ctlSelectSource.getValue(), _ctlSelectDest.getValue(), _tbQuery.Text, _model.SpeciesInfos, _model.Regions );
      this.Visibility = Visibility.Hidden;
      if (_frmHelp != null)
        _frmHelp.Close();
      Thread thr = new Thread(evaluateInBackground);
      thr.Start();
    }

    private void evaluateInBackground()
    {
      _model.Query.evaluate(_model);
    }
    private void _btnHelp_Click(object sender, RoutedEventArgs e)
    {
      string str = BatInspector.Properties.MyResources.FrmFilterListOfVars + ":\n\n";
      str += _model.Filter.getVariables();
      if (_frmHelp == null)
        _frmHelp = new FrmHelpFilter();
      _frmHelp._tbHelp.Text = str;
      _frmHelp.Show();
      _frmHelp.Visibility = Visibility.Visible;
      _frmHelp.Topmost = true;
    }

    private void Window_Loaded_1(object sender, RoutedEventArgs e)
    {
      winUtils.hideCloseButton(new WindowInteropHelper(this).Handle);
    }
  }
}
