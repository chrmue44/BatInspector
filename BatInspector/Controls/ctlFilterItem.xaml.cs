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
using System.Windows.Navigation;
using System.Windows.Shapes;
using BatInspector.Forms;

namespace BatInspector.Controls
{
  /// <summary>
  /// Interaktionslogik für ctlFilterItem.xaml
  /// </summary>
  public partial class ctlFilterItem : UserControl
  {
    int _index;
    dlgDelete _dlgDelete;
    ExpressionGenerator _gen;
    FilterItem _filter;
    
    public int Index {  get { return _index; } }
    public string FilterName { get { return _tbName.Text; } }
    public string FilterExpression { get { return _tbExpression.Text; } }

    public ctlFilterItem()
    {
      InitializeComponent();
    }

    public void setup(FilterItem filter, int index, dlgDelete del, ExpressionGenerator gen)
    {
      _filter = filter;
      _index = index;
      _dlgDelete = del;
      _tbExpression.Text = filter.Expression;
      _tbName.Text = filter.Name;
      _lblIdx.Text = _filter.Index.ToString();
      _cbAll.IsChecked = filter.IsForAllCalls;
      _gen = gen;
    }

    private void _tbName_LostFocus(object sender, RoutedEventArgs e)
    {
      _filter.Name = _tbName.Text;
    }

    private void _tbExpression_LostFocus(object sender, RoutedEventArgs e)
    {
      _filter.Expression = _tbExpression.Text;
    }

    private void _btnDel_Click(object sender, RoutedEventArgs e)
    {
      _dlgDelete(_filter.Index);
    }

    private void _cbAll_Click(object sender, RoutedEventArgs e)
    {
      _filter.IsForAllCalls = (bool)_cbAll.IsChecked;
    }

    private void _btnCreate_Click(object sender, RoutedEventArgs e)
    {
      frmExpression frm = new frmExpression(_gen, false);
      frm.Topmost = true;
      bool? res = frm.ShowDialog();
      if (res == true)
      {
        _tbExpression.Text = frm.FilterExpression;
        _filter.Expression = frm.FilterExpression;
        _cbAll.IsChecked = frm.AllCalls;
        _filter.IsForAllCalls = frm.AllCalls;
      }
    }
  }
}
