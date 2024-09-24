using BatInspector.Controls;
using libParser;
using System;
using System.Collections.Generic;
using System.Globalization;
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
  /// Interaction logic for frmModelParams.xaml
  /// </summary>
  public partial class frmModelParams : Window
  {
    Project _prj;
    public frmModelParams(Project prj)
    {
      InitializeComponent();
      _prj = prj;
      setup();
    }

    private void setup()
    {
      ModelParams[] mp = _prj.ModelParams;
      for(int i = 0; i < mp.Length; i++) 
      {
        ctlModParItem ctl = new ctlModParItem();
        string[] dataSetItems = mp[i].AvailableDataSets;
        ctl.setup(mp[i], dataSetItems);
        _sp.Children.Add(ctl);
      }
    }

    private void _btnCancel_Click(object sender, RoutedEventArgs e)
    {
      this.DialogResult = false;  
      this.Close();
    }

    private void _btnOK_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        ModelParams[] mp = _prj.ModelParams;

        for (int i = 0; i < _sp.Children.Count; i++)
        {
          ctlModParItem ctl = _sp.Children[i] as ctlModParItem;
          mp[i].Enabled = ctl._cbEnabled.IsChecked == true;
          mp[i].DataSet = ctl._ctlDataSet.getValue();
          for (int p = 0; p < mp[i].Parameters.Length; p++)
          {
            ctlDataItem dat = ctl._spPars.Children[p] as ctlDataItem;
            mp[i].Parameters[p].Value = dat.getValue();
          }
        }
        this.DialogResult = true;
        this.Close();
      }
      catch (Exception ex) 
      {
        DebugLog.log("error writing model parameters: " + ex.ToString(), enLogType.ERROR);
      }
    }

    private void _btnUpdate_Click(object sender, RoutedEventArgs e)
    {

    }
  }
}
