/********************************************************************************
 *               Author: Christian Müller
 *      Date of cration: 2021-08-10                                       
 *   Copyright (C) 2022: Christian Müller christian(at)chrmue(dot).de
 *
 *              Licence:
 * 
 * THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND,
 * EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
 ********************************************************************************/

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Interop;

namespace BatInspector.Forms
{
  /// <summary>
  /// Interaktionslogik für FrmFilter.xaml
  /// </summary>
  public partial class FrmScript : Window
  {
    ViewModel _model;
    List<ScriptItem> _temp;

    public FrmScript(ViewModel model)
    {
      _model = model;
      _temp = _model.Scripter.getScripts();
      InitializeComponent();
      populateTree();
    }


    void populateTree()
    {
      _sp.Children.Clear();
      foreach (ScriptItem sItem in _temp)
      {
        ctlScriptItem sIt = new ctlScriptItem();
        sIt.setup(sItem, deleteScript, _model);
        _sp.Children.Add(sIt);
      }
    }

    private void _btnAdd_Click(object sender, RoutedEventArgs e)
    {
      ScriptItem s = new ScriptItem(_temp.Count, "SCRIPT_NAME", "DESCRIPTION", false);
      _temp.Add(s);
      ctlScriptItem it = new ctlScriptItem();
      it.setup(s, deleteScript, _model);
      populateTree();
    }

    private void deleteScript(int index)
    {
      if (index < _temp.Count)
      {
        ScriptItem f = _temp[index];
        _temp.Remove(f);
      }
      for (int i = 0; i < _temp.Count; i++)
        _temp[i].Index = i;
      populateTree();
    }

    private void _btnOk_Click(object sender, RoutedEventArgs e)
    {   
      foreach(UIElement el in _sp.Children)
      {
        ctlScriptItem it = el as ctlScriptItem;
        if(it.Index < _temp.Count)
        {
          _temp[it.Index].Name = it.ScriptName;
          _temp[it.Index].Description = it.Description;
          _temp[it.Index].IsTool = it.IsTool;
        }
      }
      _model.Scripter.setScripts(_temp);
      this.Visibility = Visibility.Hidden;
    }

    private void _btnHelp_Click(object sender, RoutedEventArgs e)
    {
    /*
      string str = BatInspector.Properties.MyResources.FrmFilterListOfVars + ":\n\n";
      str += _filter.getVariables();
      FrmHelpFilter frm = new FrmHelpFilter();
      frm._tbHelp.Text = str;
      frm.Show(); */
    }

    private void _btnCancel_Click(object sender, RoutedEventArgs e)
    {
      this.Visibility = Visibility.Hidden; ;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      winUtils.hideCloseButton(new WindowInteropHelper(this).Handle);
    }
  }

}
