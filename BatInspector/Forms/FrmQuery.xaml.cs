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
      _ctlName.setup( BatInspector.Properties.MyResources.frmQueryQueryName, Controls.enDataType.STRING, 0, wLabel, 150, true);
      _ctlSelectSource.setup(BatInspector.Properties.MyResources.frmQuerySelSrcDir, wLabel, true);
      _ctlSelectDest.setup(BatInspector.Properties.MyResources.frmQuerySelDestDir, wLabel, true);
      _grdCol1.Width = new GridLength(wLabel);
    }

    private void btnCancel_Click(object sender, RoutedEventArgs e)
    {
      this.Visibility = Visibility.Hidden;
      if(_frmHelp != null)
        _frmHelp.Close();
    }

    private void btnOK_Click(object sender, RoutedEventArgs e)
    {
      _model.Query = new Query(_ctlName.getValue(), _ctlSelectSource.getValue(), _ctlSelectDest.getValue(), _tbQuery.Text );
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
  }
}
