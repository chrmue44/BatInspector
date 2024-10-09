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
using System.IO;
using System.Windows.Controls;


namespace BatInspector.Controls
{
  /// <summary>
  /// Interaction logic for ctlPrjInfo.xaml
  /// </summary>
  public partial class ctlPrjInfo : UserControl
  {
    Project _prj = null;

    public ctlPrjInfo()
    {
      InitializeComponent();
    }

    private void _tbNotes_TextChanged(object sender, TextChangedEventArgs e)
    {
      if(_prj != null) 
      {
        _prj.Notes = _tbNotes.Text;
      }
    }

    public void setup(Project prj) 
    {
      _prj = prj;
      _tbNotes.Text = _prj.Notes;
      initModelComboBox(_cbModels, _prj.AvailableModelParams, _prj.SelectedModelIndex);
    }

    public static void initModelComboBox(ComboBox cbm, ModelParams[] mp, int index)
    {
      cbm.Items.Clear();
      for (int i = 0; i < mp.Length; i++)
        cbm.Items.Add(mp[i].Name);

      cbm.SelectedIndex = index;
    }

    /* alternative way...
    public void initModelComboBox()
    {
      _cbModels.Items.Clear();
      _cbModels.Items.Add(MyResources.selectedAIModels);
      for (int i = 0; i < _prj.ModelParams.Length; i++)
      {
        CheckBox cb = new CheckBox();
        cb.Content = _prj.ModelParams[i].Name;
        cb.IsChecked = _prj.ModelParams[i].Enabled;
        cb.BorderThickness = new System.Windows.Thickness(0);
        _cbModels.Items.Add(cb);
      }
      _cbModels.SelectedIndex = 0;
    } */


    private void _btnPars_Click(object sender, System.Windows.RoutedEventArgs e)
    {
      try
      {
        if ((_prj != null) && _prj.Ok)
        {
          frmModelParams frm = new frmModelParams(_prj);
          bool? result = frm.ShowDialog();
          if (result == true)
            initModelComboBox(_cbModels, _prj.AvailableModelParams, _prj.SelectedModelIndex);
        }
      }
      catch(Exception ex)
      {
        DebugLog.log("Error openening Model Parameter Dialog: " + ex.ToString(), enLogType.ERROR);
      }
    }

    private void _cbModels_DropDownClosed(object sender, EventArgs e)
    {
      try
      {
        if ((_prj == null) || !_prj.Ok)
          return;
        _prj.SelectedModelIndex = _cbModels.SelectedIndex;
        if(!AppParams.Inst.AllowMoreThanOneModel)
        {
          foreach (ModelParams p in _prj.AvailableModelParams)
            p.Enabled = false;
        }
        _prj.AvailableModelParams[_prj.SelectedModelIndex].Enabled = true;
        _prj.SelectedModelParams = _prj.AvailableModelParams[_prj.SelectedModelIndex];
      }
      catch (Exception ex)
      {
        DebugLog.log("error selecting models for project: " + ex.ToString(), enLogType.ERROR);
      }
    }

    /* alternative way
    private void _cbModels_DropDownClosed(object sender, EventArgs e)
    {
      try
      {
        if((_prj == null) || !_prj.Ok)
          return;
        _cbModels.SelectedIndex = 0;
        for (int i = 0; i < _prj.ModelParams.Length; i++)
        {
          CheckBox cb = _cbModels.Items[i + 1] as CheckBox;
          _prj.ModelParams[i].Enabled = (cb.IsChecked == true);
        }
      }
      catch(Exception ex)
      {
        DebugLog.log("error selecting models for project: " + ex.ToString(), enLogType.ERROR);
      }
    } */
  }
}
