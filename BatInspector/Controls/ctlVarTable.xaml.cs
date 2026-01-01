/********************************************************************************
 *               Author: Christian Müller
 *     Date of creation: 2023-08-18                                       
 *   Copyright (C) 2023: Christian Müller chrmue44(at)gmail(dot).com
 *
 *              Licence:  CC BY-NC 4.0 
 ********************************************************************************/
using System.Windows.Controls;
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
      _spVars.Children.Clear();
      foreach(VarListItem var in _varList.getVarList(false))
      {
        if (var.Value.Count == 1)
        {
          ctlDataItem it = new ctlDataItem();
          it.setup(var.name, enDataType.STRING, 0, 100, true);
          it.setValue(var.Value[0].ToString());
          _spVars.Children.Add(it);
        }
        else
        {
          for (int i = 0; i < var.Value.Count; i++)
          {
            ctlDataItem it = new ctlDataItem();
            it.setup($"{var.name}[{i}]", enDataType.STRING, 0, 100, true);
            it.setValue(var.Value[i].ToString());
            _spVars.Children.Add(it);
          }
        }
      }
    }
  }
}
