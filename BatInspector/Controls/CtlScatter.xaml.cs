﻿/********************************************************************************
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
  /// Interaction logic for CtlScatter.xaml
  /// </summary>
  public partial class CtlScatter : UserControl
  {
    ViewModel _model = null;

    public CtlScatter()
    {
      InitializeComponent();
    }

    public void setup(ViewModel model)
    {
      _model = model;
    }

    public void populateComboBoxes()
    {
      Filter.populateFilterComboBox(_cbFilterScatter, _model);
    }

    public void initPrj()
    {
      _cbXaxis.Items.Clear();
      _cbYaxis.Items.Clear();
      foreach (stAxisItem it in _scattDiagram.AxisItems)
      {
        _cbXaxis.Items.Add(it.Name);
        _cbYaxis.Items.Add(it.Name);
      }
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


    private void _cbXaxis_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      createPlot();
    }

    private void createPlot()
    {
      if ((_cbXaxis.Items.Count > 0) && (_cbXaxis.SelectedItem != null) && (_cbYaxis.SelectedItem != null) && (_cbFilterScatter.SelectedItem != null))
      {
        stAxisItem x = _scattDiagram.findAxisItem(_cbXaxis.SelectedItem.ToString());
        stAxisItem y = _scattDiagram.findAxisItem(_cbYaxis.SelectedItem.ToString());
        FilterItem filter = (_cbFilterScatter.SelectedIndex == 1) ?
                        filter = _model.Filter.TempFilter : filter = _model.Filter.getFilter(_cbFilterScatter.Text);

        _scattDiagram.createScatterDiagram(x, y, _model, filter, _cbFreezeAxis.IsChecked == true);
        _scatterModel.InvalidatePlot();
      }
    }
    private void _cbFilter_DropDownOpened(object sender, EventArgs e)
    {
      _model.Filter.TempFilter = null;
      _cbFilterScatter.Items[1] = MyResources.MainFilterNew;
    }

    private void _cbFilter_DropDownClosed(object sender, EventArgs e)
    {
      DebugLog.log("CtlScatter: Filter dropdown closed", enLogType.DEBUG);
      bool apply;
      bool resetFilter;

      CtlScatter.handleFilterDropdown(out apply, out resetFilter, _model, _cbFilterScatter);
      createPlot();
    }
  }
}
