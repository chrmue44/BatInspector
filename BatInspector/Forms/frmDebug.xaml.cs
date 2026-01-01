/********************************************************************************
 *               Author: Christian Müller
 *     Date of creation: 2021-08-10                                       
 *   Copyright (C) 2023: Christian Müller chrmue44(at)gmail(dot).com
 *
 *              Licence:  CC BY-NC 4.0 
 ********************************************************************************/

using BatInspector.Controls;
using DSPLib;
using libParser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Emit;
//using System.Web.UI.WebControls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Interop;

namespace BatInspector.Forms
{
  public delegate void dlgSetBreakCondition(int lineNr, string condition);
  public delegate void dlgUpdateDbg();

  /// <summary>
  /// Interaction logic for frmDebug.xaml
  /// </summary>
  public partial class frmDebug : Window
  { 
    string _script;
    List<ParamItem> _params;

    public frmDebug()
    {
      InitializeComponent();
      _ctlVarTable.setup(App.Model.Scripter.VarList);
    }

    public void setup(string script, List<ParamItem> pars)
    {
      Title = "Script Debugger: " + script;
      _script = script;
      _params = pars;
      initScriptView();
    }

    private void initScriptView()
    {
      try
      {
        string[] lines = File.ReadAllLines(_script);
        _spScript.Children.Clear();
        for (int i = 0; i < lines.Length; i++)
        {
          ctlDebugLine line = new ctlDebugLine();
          line.setup(i, lines[i], setBreakCondition);
          _spScript.Children.Add(line);
        }
        App.Model.Scripter.initScriptForDbg(_script);
        highlightActLine(true);
        initScriptParams();
        _ctlVarTable.setup(App.Model.Scripter.VarList);
      }
      catch (Exception ex)
      {
        DebugLog.log("error reading script: " + _script + " " + ex.ToString(), enLogType.ERROR);
      }

    }

    private void initScriptParams() 
    {
      int wLbl = 160;
      if ((_params != null) && (_params.Count > 0))
      {
        _spPars.Children.Clear();
        for (int i = 0; i < _params.Count; i++)
        {
          CtlSelectFile ctl = new CtlSelectFile();
          switch (_params[i].Type)
          {
            case enParamType.FILE:
              ctl.setup(_params[i].Name, wLbl, false, "", setParams);
              ctl.Margin = new Thickness(2, 2, 0, 0);
              _spPars.Children.Add(ctl);
              break;
            case enParamType.DIRECTORY:
              ctl.setup(_params[i].Name, wLbl, true, "", setParams);
              ctl.Margin = new Thickness(2, 2, 0, 0);
              _spPars.Children.Add(ctl);
              break;
            case enParamType.MICSCELLANOUS:
              ctlDataItem ctld = new ctlDataItem();
              ctld.setup(_params[i].Name, enDataType.STRING, 0, wLbl, true, setParams);
              ctld.Margin = new Thickness(2, 2, 0, 0);
              ctld.setValue("");
              _spPars.Children.Add(ctld);
              break;
            case enParamType.BOOL:
              CheckBox chk = new CheckBox();
              chk.Content = _params[i].Name;
              chk.Click += setParamsChk;
              _spPars.Children.Add(chk);
              break;
          }
        }
      }
    }

    void setParamsChk(object sender, RoutedEventArgs e)
    {
      setParams();
    }

    private void setParams()
    {
      if ((_params != null) && (_params.Count > 0))
      {
        for (int i = 0; i < _params.Count; i++)
        {
          switch (_params[i].Type)
          {
            case enParamType.FILE:
            case enParamType.DIRECTORY:
              CtlSelectFile ctl = _spPars.Children[i] as CtlSelectFile;
              App.Model.Scripter.VarList.set(_params[i].VarName, ctl.getValue());
              break;
            case enParamType.MICSCELLANOUS:
              ctlDataItem ctld = _spPars.Children[i] as ctlDataItem;
              App.Model.Scripter.VarList.set(_params[i].VarName, ctld.getValue());
              break;
            case enParamType.BOOL:
              CheckBox chk = _spPars.Children[i] as CheckBox;
              String boolVal = "0";
              if ((chk != null) && (chk.IsChecked == true))
                boolVal = "1";
              App.Model.Scripter.VarList.set(_params[i].VarName, boolVal);
              break;
          }
        }
      }
      _ctlVarTable.setup(App.Model.Scripter.VarList);
    }


    private void setParams(enDataType type, object val)
    {
      setParams();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      winUtils.hideCloseButton(new WindowInteropHelper(this).Handle);
    }

    private void _btnClose_Click(object sender, RoutedEventArgs e)
    {
      App.Model.Scripter.breakDebugging();
      this.Visibility = Visibility.Hidden;
    }

    private void _btnStart_Click(object sender, RoutedEventArgs e)
    {
      highlightActLine(false);
      setBusy(true);
      App.Model.Scripter.continueDebugging(updateDebugView);
    }

    private void updateDebugView()
    {
      if (!Dispatcher.CheckAccess()) // CheckAccess returns true if you're on the dispatcher thread
      {
        Dispatcher.BeginInvoke(new dlgUpdateDbg(updateDebugView));
        return;
      }
      highlightActLine(true);
      scrollToCurrentLine();
      setBusy(false);
      _ctlVarTable.setup(App.Model.Scripter.VarList);
    }


    private void _btnStep_Click(object sender, RoutedEventArgs e)
    {
      setBusy(true);
      highlightActLine(false);
     App.Model.Scripter.debugOneStep(updateDebugView);
    }

    void highlightActLine(bool on)
    {
      int lineNr = App.Model.Scripter.CurrentLineNr;
      if ((lineNr >= 0) && (lineNr < _spScript.Children.Count))
      {
        ctlDebugLine ctl = _spScript.Children[lineNr] as ctlDebugLine;
        ctl.activate(on);
      }
    }

    private void scrollToCurrentLine()
    {
      int lineNr = 0;
      if (App.Model.Scripter.CurrentLineNr < _spScript.Children.Count)
        lineNr = App.Model.Scripter.CurrentLineNr;
      else
        lineNr = _spScript.Children.Count - 1;
      ctlDebugLine ctl = _spScript.Children[lineNr] as ctlDebugLine; 
      double verticalOffset = App.Model.Scripter.CurrentLineNr * ctl.ActualHeight - _spScript.ActualHeight/2;
      _scrlViewer.ScrollToVerticalOffset(verticalOffset);
    }

  private void setBreakCondition(int lineNr, string condition)
    {
      App.Model.Scripter.setBreakCondition(lineNr, condition);
    }

    private void _btnPause_Click(object sender, RoutedEventArgs e)
    {
      App.Model.Scripter.breakDebugging();
    }

    private void _btnStop_Click(object sender, RoutedEventArgs e)
    {
      highlightActLine(false);
      App.Model.Scripter.restartScript();
      highlightActLine(true);
      setParams();
      _ctlVarTable.setup(App.Model.Scripter.VarList);
    }

    private void _grdSplitterV_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
    {

    }

    private void _scrlViewer_ScrollChanged(object sender, System.Windows.Controls.ScrollChangedEventArgs e)
    {

    }

    private void _btnSave_Click(object sender, RoutedEventArgs e)
    {

    }

    private void setBusy(bool busy)
    {
      if(busy) 
      {
        _btnStart.IsEnabled = false;
        _btnStart.Opacity = 0.2;
        _btnStep.IsEnabled = false;
        _btnStep.Opacity = 0.2;
      }
      else
      {
        _btnStart.IsEnabled = true;
        _btnStart.Opacity = 1.0;
        _btnStep.IsEnabled = true;
        _btnStep.Opacity = 1.0;
      }
    }

    private void _btnEdit_Click(object sender, RoutedEventArgs e)
    {
      App.Model.editScript(_script);
    }

    private void _btnReload_Click(object sender, RoutedEventArgs e)
    {
      initScriptView();
    }
  }
}
