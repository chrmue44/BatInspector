using BatInspector.Controls;
using BatInspector.Properties;
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
    ctlDataItem _ctlCr;
    ctlDataItem _ctlLoc;
    int _nrOfPrjParams;

    public frmModelParams(Project prj)
    {
      InitializeComponent();
      _prj = prj;
      setup();
    }

    private void setup()
    {
      ModelParams[] mp = _prj.AvailableModelParams;
      _sp.Children.Clear();
      _ctlCr = new ctlDataItem();
      _ctlCr.setup(MyResources.frmModParsCreator, enDataType.STRING, 0, 140, true);
      _ctlCr.setValue(_prj.CreateBy);
      _sp.Children.Add(_ctlCr);
      _ctlLoc = new ctlDataItem();
      _ctlLoc.setup(MyResources.frmModParsLocation, enDataType.STRING, 0, 140, true);
      _ctlLoc.setValue(_prj.Location);
      _sp.Children.Add(_ctlLoc);
      _nrOfPrjParams = 2;

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
        ModelParams[] mp = _prj.AvailableModelParams;

        for (int i = _nrOfPrjParams; i < _sp.Children.Count; i++)
        {
          int mIdx = i - _nrOfPrjParams;
          ctlModParItem ctl = _sp.Children[i] as ctlModParItem;
          mp[mIdx].Enabled = ctl._cbEnabled.IsChecked == true;
          mp[mIdx].DataSet = ctl._ctlDataSet.getValue();
          for (int p = 0; p < mp[i - _nrOfPrjParams].Parameters.Length; p++)
          {
            ctlDataItem dat = ctl._spPars.Children[p] as ctlDataItem;
            mp[mIdx].Parameters[p].Value = dat.getValue();
          }
        }
        _prj.CreateBy = _ctlCr.getValue();
        _prj.Location = _ctlLoc.getValue();
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
      if( App.Model.DefaultModelParams.Length > _prj.AvailableModelParams.Length)
      {
        ModelParams[] mp = new ModelParams[App.Model.DefaultModelParams.Length];
        for (int i = _nrOfPrjParams; i < mp.Length; i++)
        {
          int mIdx = i - _nrOfPrjParams;
          if (i < _prj.AvailableModelParams.Length)
            mp[mIdx] = _prj.AvailableModelParams[mIdx];
          else
            mp[mIdx] = App.Model.DefaultModelParams[mIdx].getCopy();
        }
        _prj.assignNewModelParams( mp);
        this.Close();
      }
    }
  }
}
