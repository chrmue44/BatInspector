/********************************************************************************
 *               Author: Christian Müller
 *     Date of creation: 2021-08-10                                       
 *   Copyright (C) 2023: Christian Müller chrmue44(at)gmail(dot).com
 *
 *              Licence:  CC BY-NC 4.0 
 ********************************************************************************/

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Interop;

namespace BatInspector.Forms
{
  public delegate void dlgUpdateMenu();

  /// <summary>
  /// Interaktionslogik für FrmFilter.xaml
  /// </summary>
  public partial class FrmScript : Window
  {
    ViewModel _model;
    List<ScriptItem> _temp;
    dlgUpdateMenu _updateMenu;

    public FrmScript(ViewModel model, dlgUpdateMenu updateMenu)
    {
      _model = model;
      _temp = _model.Scripter.getScripts();
      InitializeComponent();
      populateTree();
      _updateMenu = updateMenu;
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
      ScriptItem s = new ScriptItem(_temp.Count, "SCRIPT_NAME", "DESCRIPTION", false, new List<ParamItem>());
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
          _temp[it.Index].Parameter = it.Parameter;
        }
      }
      _model.Scripter.setScripts(_temp);
      _model.Scripter.saveScripts();
      this.Visibility = Visibility.Hidden;
      if (_updateMenu != null)
        _updateMenu();
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
