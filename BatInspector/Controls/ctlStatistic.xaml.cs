/********************************************************************************
 *               Author: Christian Müller
 *     Date of creation: 2023-08-18                                       
 *   Copyright (C) 2023: Christian Müller chrmue44(at)gmail(dot).com
 *
 *              Licence:  CC BY-NC 4.0 
 ********************************************************************************/
using BatInspector.Forms;
using BatInspector.Properties;
using libParser;

using System;
using System.Windows.Controls;


namespace BatInspector.Controls
{
  /// <summary>
  /// Interaction logic for ctlStatitic.xaml
  /// </summary>
  public partial class ctlStatistic : UserControl
  {

    public ctlStatistic()
    {
      InitializeComponent();
    }

    public void setup()
    {
      _stat1.initHistogram(App.Model.Statistic.Fmin, BatInspector.Properties.MyResources.ctlStatMinFreq);
      _stat2.initHistogram(App.Model.Statistic.Fmax, BatInspector.Properties.MyResources.ctlStatMaxFreq);
      _stat3.initHistogram(App.Model.Statistic.FmaxAmp, BatInspector.Properties.MyResources.ctlStatFreqMaxAmp);
      _stat4.initHistogram(App.Model.Statistic.Duration, BatInspector.Properties.MyResources.ctlStatDuration);
      _stat5.initHistogram(App.Model.Statistic.CallDist, BatInspector.Properties.MyResources.ctlStatCallInterval);
//      double[] ticksX = { 0,2,4,6,8,10,12,14,16,18,20,22};
      double[] ticksX = { 0, 1,2,3, 4,5, 6,7, 8,9, 10,11, 12,13, 14,15, 16,17, 18,19, 20,21, 22, 23 };
      _stat6.initHistogram(App.Model.Statistic.RecTime, BatInspector.Properties.MyResources.ctlStatRecTime, ticksX, true);
    }

    public void populateComboBoxes()
    {
      Filter.populateFilterComboBox(_cbFilterStatistic);
    }


    public static void handleFilterDropdown(out bool applyFilter, out bool resetFilter, ViewModel model, ComboBox cbFilter)
    {
      applyFilter = false;
      resetFilter = false;
      try
      {
        if (cbFilter.SelectedIndex == 0)
        {
          resetFilter = true;
        }
        else
        {
          if (cbFilter.SelectedIndex == 1)
          {
            frmExpression frm = new frmExpression(model.Filter.ExpGenerator, true);
            bool? res = frm.ShowDialog();
            if (res == true)
            {
              if (frm.SaveFilter)
              {
                int idx = model.Filter.Items.Count;
                FilterItem filter = new FilterItem(idx, frm.FilterName, frm.FilterExpression, frm.AllCalls);
                model.Filter.Items.Add(filter);
                cbFilter.Items.Add(filter.Name);
                cbFilter.SelectedIndex = cbFilter.Items.Count - 1;
              }
              else
              {
                if (frm.FilterExpression.Length < 25)
                  cbFilter.Items[1] = frm.FilterExpression;
                else
                  cbFilter.Items[1] = frm.FilterExpression.Substring(0, 21) + "...";
                cbFilter.SelectedIndex = 1;
                model.Filter.TempFilter = new FilterItem(-1, "TempFilter", frm.FilterExpression, frm.AllCalls);
              }
            }
          }
          applyFilter = true;
        }
      }
      catch (Exception ex)
      {
        DebugLog.log("Main: Filter dropdown close failed: " + ex.ToString(), enLogType.ERROR);
      }
    }

    /*
    private void _cbXaxis_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      createPlot();
    }*/

    public void createPlot()
    {
      try
      {
        FilterItem filterExp = (_cbFilterStatistic.SelectedIndex == 1) ?
                    filterExp = App.Model.Filter.TempFilter : filterExp = App.Model.Filter.getFilter(_cbFilterStatistic.Text);
        if (App.Model.CurrentlyOpen != null)
        {
          App.Model.Statistic.calcStatistic(filterExp, App.Model.CurrentlyOpen.Analysis, App.Model.Filter);
          _stat1.createHistogram();
          _stat2.createHistogram();
          _stat3.createHistogram();
          _stat4.createHistogram();
          _stat5.createHistogram();
          _stat6.createHistogram();
        }
      }
      catch(Exception ex) 
      {
        DebugLog.log("ctlStatistic: " + ex.ToString(), enLogType.ERROR);
      }
    }

    private void _cbFilter_DropDownOpened(object sender, EventArgs e)
    {
      App.Model.Filter.TempFilter = null;
      _cbFilterStatistic.Items[1] = MyResources.MainFilterNew;
    }

    private void _cbFilter_DropDownClosed(object sender, EventArgs e)
    {
      DebugLog.log("CtlStatistic: Filter dropdown closed", enLogType.DEBUG);
      bool apply;
      bool resetFilter;

      CtlScatter.handleFilterDropdown(out apply, out resetFilter, _cbFilterStatistic);
      createPlot();
    }

    private void UserControl_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
    {
      createPlot();
    }


    private void _btnExport_Click(object sender, System.Windows.RoutedEventArgs e)
    {
      System.Windows.Forms.SaveFileDialog dlg = new System.Windows.Forms.SaveFileDialog();
      dlg.Filter = "CSV files (*.csv)|*.csv";
      System.Windows.Forms.DialogResult res = dlg.ShowDialog();
      if (res == System.Windows.Forms.DialogResult.OK)
      {
        string fileName = dlg.FileName;
        string prjName = "";
        if (App.Model.Prj != null)
          prjName = App.Model.Prj.Name;
        else if(App.Model.Query != null)
          prjName = App.Model.Query.Name;
        App.Model.Statistic.exportToCsv(fileName, prjName);
      }
    }
  }
}
