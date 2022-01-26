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

namespace BatInspector.Forms
{

  /// <summary>
  /// Interaktionslogik für ctlDataItem.xaml
  /// </summary>
  public partial class ctlSelectItem : UserControl
  {
    string _valString;
    dlgValueChanged _dlgValChange = null;
    

   // public bool Focusable { set { _tb.Focusable = value; } get { return _tb.Focusable; } }

    public void setup(string label, dlgValueChanged dlgValChange = null)
    {
      _lbl.Text = label;
      _lbl.Focusable = false;
      _dlgValChange = dlgValChange;
    }

    public void setValue(string val)
    {
       _valString = val;
      _cb.Text = val;
    }

    public void setItems(string[] items)
    {
      _cb.Items.Clear();
      foreach (string it in items)
        _cb.Items.Add(it);
      _cb.SelectedItem = 0;
    }

    public string getValue()
    {
      return _valString;
    }

    public ctlSelectItem()
    {
      InitializeComponent();
    }


    private void _cb_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      _valString = _cb.Text;
      if (_dlgValChange != null)
        _dlgValChange(enDataType.STRING, _valString);
    }
  }

  public enum enDataType
  {
    STRING,
    DOUBLE,
    INT
  }

  public delegate void dlgValueChanged(enDataType type, object val);

}
