/********************************************************************************
 *               Author: Christian Müller
 *     Date of creation: 2023-08-18                                       
 *   Copyright (C) 2023: Christian Müller chrmue44(at)gmail(dot).com
 *
 *              Licence:  CC BY-NC 4.0 
 ********************************************************************************/
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using libParser;
using Microsoft.Win32;


namespace BatInspector.Forms
{
  /// <summary>
  /// Interaktionslogik für ctlFilterItem.xaml
  /// </summary>
  public partial class ctlScriptItem : UserControl
  {
    int _index;
    dlgDelete _dlgDelete;
    dlgDelete _dlgDebug;

    public int Index { get { return _index; } }
    public string ScriptName { get { return _tbScriptName.Text; } }
    public string Description { get { return _tbDescription.Text; } }
    public bool IsTool { get { return _cbTool.IsChecked == true; } }

    public List<ParamItem> Parameter { get; private set;}

    public ctlScriptItem()
    {
      InitializeComponent();
    }

    public void setup(ScriptItem script, dlgDelete del, dlgDelete debug)
    {
      _index = script.Index;
      _dlgDelete = del;
      _dlgDebug = debug;
      Parameter = script.Parameter;
      if(Parameter == null)
        Parameter = new List<ParamItem>(); 
      _tbScriptName.Text = script.Name;
      _tbDescription.Text = script.Description;
      _cbTool.IsChecked = script.IsTool;
      if (_cbTool.IsChecked == true)
      {
        _btnRun.Visibility = Visibility.Hidden;
        _btnPars.Visibility = Visibility.Hidden;
      }
      _lblIdx.Text = _index.ToString();
      _btnPars.Content = "Pars(" + Parameter.Count.ToString() + ")";
    }



    private void _btnDel_Click(object sender, RoutedEventArgs e)
    {
      _dlgDelete(_index);
    }

    private void _btnRun_Click(object sender, RoutedEventArgs e)
    {
      string script = _tbScriptName.Text;
      ScriptItem item = App.Model.Scripter.getScript(script);
      if (item != null)
      {
        if (item.Parameter.Count > 0)
        {
          string winTitle = BatInspector.Properties.MyResources.frmScriptParamTitle + ": " + script;
          frmScriptParams frm = new frmScriptParams(winTitle, item.Parameter);
          frm.ShowDialog();
          if (frm.DialogResult == true)
            App.Model.Scripter.runScript(script, frm.ParameterValues);
        }
        else
          App.Model.Scripter.runScript(script);
      }
    }

    private void _btnEdit_Click(object sender, RoutedEventArgs e)
    {
      App.Model.editScript(_tbScriptName.Text);
    }

    private void _cbTool_Click(object sender, RoutedEventArgs e)
    {
      _btnRun.Visibility = _cbTool.IsChecked == true ? Visibility.Hidden : Visibility.Visible;
      _btnPars.Visibility = _cbTool.IsChecked == true ? Visibility.Hidden : Visibility.Visible;
    }

    private void _btnPars_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        DebugLog.log("ScriptItem BTN Pars clicked", enLogType.DEBUG);
        
        FrmScriptParamEditor frm = new FrmScriptParamEditor(Parameter, ScriptName);
        frm.ShowDialog();
        if (frm.DialogResult == true)
        {
          this.Parameter = frm.Parameter;
          _btnPars.Content = "Pars(" + Parameter.Count.ToString() + ")";
        }
      }
      catch(Exception ex)
      {
        DebugLog.log("error ScriptItem BTN Pars: " + ex.ToString(), enLogType.ERROR);
      }
    }

    private void _btnDebug_Click(object sender, RoutedEventArgs e)
    {
      _dlgDebug(_index);
    }
  }
}
