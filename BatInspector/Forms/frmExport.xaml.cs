using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BatInspector.Forms
{
  /// <summary>
  /// Interaction logic for frmExport.xaml
  /// </summary>
  public partial class frmExport : Window
  {

    public frmExport()
    {
      InitializeComponent();
      _ctlDest.setup(BatInspector.Properties.MyResources.frmExpSelectDst, 150, true);
      _ctlPrefix.setup(BatInspector.Properties.MyResources.frmExportPrefix, Controls.enDataType.STRING, 0, 145, true);
      _cbIncPng.IsChecked = true;
      _cbIncXML.IsChecked = true;
      _cbTimeStretch.IsChecked = true;
      if (App.Model.Prj.Ok)
      {
        int call = App.Model.ZoomView.SelectedCallIdx;
        if ((call >= 0) && (call <= App.Model.ZoomView.Analysis.Calls.Count))
        {          
          string pref = App.Model.ZoomView.Analysis.Calls[call].getString(Cols.SPECIES_MAN);
          _ctlPrefix.setValue(pref);
        }
      }
    }

    private void _btnOk_Click(object sender, RoutedEventArgs e)
    {
      string dstDir = _ctlDest.getValue();
      bool incPng = _cbIncPng.IsChecked == true;
      bool incXml= _cbIncXML.IsChecked == true;
      uint timeStretch = (uint)(_cbTimeStretch.IsChecked == true ? 10 : 1);
      string prefix = _ctlPrefix.getValue();
      App.Model.ZoomView.export(dstDir, incPng, incXml, timeStretch, prefix);
      this.Hide();
    }

    private void _btnCancel_Click(object sender, RoutedEventArgs e)
    {
      this.Hide();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      winUtils.hideCloseButton(new WindowInteropHelper(this).Handle);
    }
  }
}
