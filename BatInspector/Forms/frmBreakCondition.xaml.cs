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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BatInspector.Forms
{
  /// <summary>
  /// Interaction logic for frmBreakCondition.xaml
  /// </summary>
  public partial class frmBreakCondition : Window
  {
    public frmBreakCondition(string lineNr, string breakCond)
    {
      InitializeComponent();
      _ctlLineNr.setup("Line Number", Controls.enDataType.STRING, 0, 100);
      _ctlLineNr.setValue(lineNr);
      _ctlCondition.setup("Break Condition", Controls.enDataType.STRING, 0, 100, true);
      _ctlCondition.setValue(breakCond);
      this.Topmost = true;
    }

    public string BreakCondition { get { return _ctlCondition.getValue();} }

    private void _btnOk_Click(object sender, RoutedEventArgs e)
    {
      DialogResult = true;
      this.Close();
    }

    private void _btnCancel_Click(object sender, RoutedEventArgs e)
    {
      DialogResult = false;
      this.Close();
    }
  }
}
