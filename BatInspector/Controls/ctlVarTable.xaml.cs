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
using libParser;

namespace BatInspector.Controls
{
  /// <summary>
  /// Interaction logic for ctlVarTable.xaml
  /// </summary>
  public partial class ctlVarTable : UserControl
  {
    VarList _varList = null;
    public ctlVarTable()
    {
      InitializeComponent();
    }

    public void setup(VarList varList)
    { 
      _varList = varList;
      foreach(VarListItem var in _varList.getVarList(false))
      {
        ctlDataItem it = new ctlDataItem();
        it.setup(var.name, enDataType.STRING, 0, 100, true);
        it.setValue(var.value.ToString());
        _spVars.Children.Add(it);
      }
    }
  }
}
