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
  /// Interaction logic for CtlScatter.xaml
  /// </summary>
  public partial class CtlScatter {
    public CtlScatter()
    {
    }


    public static void handleFilterDropdown(out bool applyFilter, out bool resetFilter, System.Windows.Controls.ComboBox cbFilter)
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
            frmExpression frm = new frmExpression(App.Model.Filter.ExpGenerator, true);
            bool? res = frm.ShowDialog();
            if (res == true)
            {
              if (frm.SaveFilter)
              {
                int idx = App.Model.Filter.Items.Count;
                FilterItem filter = new FilterItem(idx, frm.FilterName, frm.FilterExpression);
                App.Model.Filter.Items.Add(filter);
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
                App.Model.Filter.TempFilter = new FilterItem(-1, "TempFilter", frm.FilterExpression);
              }
              applyFilter = true;
            }
            else
            {
              cbFilter.SelectedIndex = 0;
              resetFilter = true;
            }
          }
          else
          {
            applyFilter = true;
          }
        }
      }
      catch (Exception ex)
      {
        DebugLog.log("Main: Filter dropdown close failed: " + ex.ToString(), enLogType.ERROR);
      }
    }


  }
}
