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
  public partial class ctlDataItem : UserControl
  {
    public string Label { get { return _lbl.Text; } set { _lbl.Text = value; } }

    public string Value { get { return _tb.Text; } set { _tb.Text = value; } }
    public ctlDataItem()
    {
      InitializeComponent();
    }
  }
}
