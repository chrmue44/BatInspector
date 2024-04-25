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
    ViewModel _model = null;

    public ctlStatistic()
    {
      InitializeComponent();
    }

    public void setup(ViewModel model)
    {
      _model = model;
      _stat1.initHistogram(_model.Statistic.Fmin, BatInspector.Properties.MyResources.ctlStatMinFreq);
      _stat2.initHistogram(_model.Statistic.Fmax, BatInspector.Properties.MyResources.ctlStatMaxFreq);
      _stat3.initHistogram(_model.Statistic.FmaxAmp, BatInspector.Properties.MyResources.ctlStatFreqMaxAmp);
      _stat4.initHistogram(_model.Statistic.Duration, BatInspector.Properties.MyResources.ctlStatDuration);
      _stat5.initHistogram(_model.Statistic.CallDist, BatInspector.Properties.MyResources.ctlStatCallInterval);
    }

    public void populateComboBoxes()
    {
      Filter.populateFilterComboBox(_cbFilterStatistic, _model);
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
                    filterExp = _model.Filter.TempFilter : filterExp = _model.Filter.getFilter(_cbFilterStatistic.Text);
        if (_model.CurrentlyOpen != null)
        {
          _model.Statistic.calcStatistic(filterExp, _model.CurrentlyOpen.Analysis, _model.Filter);
          _stat1.createHistogram();
          _stat2.createHistogram();
          _stat3.createHistogram();
          _stat4.createHistogram();
          _stat5.createHistogram();
        }
      }
      catch(Exception ex) 
      {
        DebugLog.log("ctlStatistic: " + ex.ToString(), enLogType.ERROR);
      }
    }

    private void _cbFilter_DropDownOpened(object sender, EventArgs e)
    {
      _model.Filter.TempFilter = null;
      _cbFilterStatistic.Items[1] = MyResources.MainFilterNew;
    }

    private void _cbFilter_DropDownClosed(object sender, EventArgs e)
    {
      DebugLog.log("CtlStatistic: Filter dropdown closed", enLogType.DEBUG);
      bool apply;
      bool resetFilter;

      CtlScatter.handleFilterDropdown(out apply, out resetFilter, _model, _cbFilterStatistic);
      createPlot();
    }

    private void UserControl_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
    {
      createPlot();
    }
  }
}
