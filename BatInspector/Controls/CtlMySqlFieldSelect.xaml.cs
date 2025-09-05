using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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

namespace BatInspector.Controls
{

  public delegate void dlgCbClick(object sender, RoutedEventArgs e);

  /// <summary>
  /// Interaction logic for CtlMySqlFieldSelect.xaml
  /// </summary>
  public partial class CtlMySqlFieldSelect : UserControl
  {
    dlgCbClick _dlg = null;
    int _rank = 0;

    public int Order 
    { 
      get { return _rank; } 
      set
      {
       _rank = value; 
       if (_rank > 5) 
         _rank = 5;
        if (_rank > 0)
        {
          _order.Content = _rank.ToString();
          _reverse.IsEnabled = true;
        }
        else
        {
          _order.Content = "-";
          _reverse.IsEnabled = false;
        }

      }
    }

    public bool Selected 
    { 
      get { return _select.IsChecked == true; } 
      set
      {
        _select.IsChecked = value;
        _order.IsEnabled = value;
      }
    }
    public bool Reverse {  get { return _reverse.IsChecked == true; } set { _reverse.IsChecked = value; } }
    public string FieldName { get { return (string)_lbl.Content; } }
    public CtlMySqlFieldSelect()
    {
      InitializeComponent();
    }

    public void setup(string text, dlgCbClick dlg)
    {
      _dlg = dlg;
      _lbl.Content = text;
      _order.Content = "-";
      _order.IsEnabled = false;
      _reverse.IsEnabled = false;
    }

    private void _select_Click(object sender, RoutedEventArgs e)
    {
      if (_select.IsChecked == true)
      {
        _order.IsEnabled = true;
      }
      else
      {
        _order.IsEnabled = false;
        _reverse.IsEnabled = false; 
        _order.Content = "-";
        _rank = 0;
      }
      if (_dlg != null)
        _dlg(this, null);
    }

    private void _reverse_Click(object sender, RoutedEventArgs e)
    {
      if (_dlg != null)
        _dlg(this, null);
    }

    private void _order_DropDownClosed(object sender, EventArgs e)
    {
      if (_dlg != null)
        _dlg(this, null);
    }

    private void _order_Click(object sender, RoutedEventArgs e)
    {
      _rank++;
      if (_rank > 5)
        _rank = 0;
      if (_rank == 0)
      {
        _order.Content = "-";
        _reverse.IsEnabled = false;
        _reverse.IsChecked = false;
      }
      else
      {
        _reverse.IsEnabled = true;
        _order.Content = _rank.ToString();
      }
      if (_dlg != null)
        _dlg(this, null);
    }
  }
}
