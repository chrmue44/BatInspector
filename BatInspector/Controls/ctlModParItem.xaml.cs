﻿using BatInspector.Properties;
using System.Globalization;
using System.Windows.Controls;


namespace BatInspector.Controls
{
  /// <summary>
  /// Interaction logic for ctlModParItem.xaml
  /// </summary>
  public partial class ctlModParItem : UserControl
  {
    ModelParams _pars;
    public ctlModParItem()
    {
      InitializeComponent();
    }

    public void setup(ModelParams pars, string[] dataSetItems)
    {
      int w = 120;
      int wCombo = 250;
      _pars = pars;
      _ctlDataSet.setup(BatInspector.Properties.MyResources.ctlModParDataSet, 0, w, wCombo);
      _ctlDataSet.setItems(dataSetItems);
      _ctlDataSet.setValue(pars.DataSet);
      _cbEnabled.IsChecked = pars.Enabled;
      _cbEnabled.Visibility = AppParams.Inst.AllowMoreThanOneModel ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
      _ctlSubDir.setup(BatInspector.Properties.MyResources.SubDirectory, enDataType.STRING, 0, w);
      _ctlSubDir.setValue(pars.SubDir);
      _spPars.Children.Clear();
      _header.Text = MyResources.ctlModParItemModellparameter + " " + pars.Name;
      for (int i = 0; i < _pars.Parameters.Length; i++)
      {
        ctlDataItem it = new ctlDataItem();
        it.setup(_pars.Parameters[i].Name, _pars.Parameters[i].Type, _pars.Parameters[i].Decimals, w, true);
        switch (_pars.Parameters[i].Type)
        {
          case enDataType.STRING:
            it.setValue(_pars.Parameters[i].Value);
            break;
          case enDataType.DOUBLE:
            {
              double.TryParse(_pars.Parameters[i].Value, NumberStyles.Any, CultureInfo.InvariantCulture, out double val);
              it.setValue(val);
            }
            break;
          case enDataType.INT:
            {
              int.TryParse(_pars.Parameters[i].Value, out int val);
              it.setValue(val);
            }
            break;
          case enDataType.UINT:
            {
              uint.TryParse(_pars.Parameters[i].Value, out uint val);
              it.setValue(val);
            }
            break;
        }
        _spPars.Children.Add(it);
      }
    }
  }
}
