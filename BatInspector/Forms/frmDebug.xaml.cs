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
using System.Windows;
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
 
    ViewModel _model;
    string _script;
    List<ParamItem> _params;

    public frmDebug(ViewModel model)
    {
      InitializeComponent();
      _model = model;
      _ctlVarTable.setup(_model.Scripter.VarList);
    }

    public void setup(string script, List<ParamItem> pars)
    {
      try
      {
        Title = "Script Debugger: " + script;
        _script = script;
        string[] lines = File.ReadAllLines(script);
        _spScript.Children.Clear();
        for(int i= 0; i<lines.Length; i++)
        {
          ctlDebugLine line = new ctlDebugLine();
          line.setup(i, lines[i], setBreakCondition);
          _spScript.Children.Add(line);
        }
        _model.Scripter.initScriptForDbg(script);
        highlightActLine(true);
        _params = pars;
        initScriptParams();
        _ctlVarTable.setup(_model.Scripter.VarList);
      }
      catch (Exception ex)
      {
        DebugLog.log("error reading script: " + script + " " + ex.ToString(), enLogType.ERROR);
      }
    }

    private void initScriptParams() 
    {
      if ((_params != null) && (_params.Count > 0))
      {
        _spPars.Children.Clear();
        for (int i = 0; i < _params.Count; i++)
        {
          CtlSelectFile ctl = new CtlSelectFile();
          switch (_params[i].Type)
          {
            case enParamType.FILE:
              ctl.setup(_params[i].Name, 150, false, "", setParams);
              ctl.Margin = new Thickness(2, 2, 0, 0);
              _spPars.Children.Add(ctl);
              break;
            case enParamType.DIRECTORY:
              ctl.setup(_params[i].Name, 150, true, "", setParams);
              ctl.Margin = new Thickness(2, 2, 0, 0);
              _spPars.Children.Add(ctl);
              break;
            case enParamType.MICSCELLANOUS:
              ctlDataItem ctld = new ctlDataItem();
              ctld.setup(_params[i].Name, enDataType.STRING, 0, 150, true, setParams);
              ctld.Margin = new Thickness(2, 2, 0, 0);
              ctld.setValue("");
              _spPars.Children.Add(ctld);
              break;
          }
        }
      }
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
              _model.Scripter.VarList.set(_params[i].VarName, ctl.getValue());
              break;
            case enParamType.MICSCELLANOUS:
              ctlDataItem ctld = _spPars.Children[i] as ctlDataItem;
              _model.Scripter.VarList.set(_params[i].VarName, ctld.getValue());
              break;
          }
        }
      }
      _ctlVarTable.setup(_model.Scripter.VarList);
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
      _model.Scripter.breakDebugging();
      this.Visibility = Visibility.Hidden;
    }

    private void _btnStart_Click(object sender, RoutedEventArgs e)
    {
      highlightActLine(false);
      setBusy(true);
      _model.Scripter.continueDebugging(updateDebugView);
    }

    private void updateDebugView()
    {
      if (!Dispatcher.CheckAccess()) // CheckAccess returns true if you're on the dispatcher thread
      {
        Dispatcher.BeginInvoke(new dlgUpdateDbg(updateDebugView));
        return;
      }
      highlightActLine(true);
      setBusy(false);
      _ctlVarTable.setup(_model.Scripter.VarList);
    }


    private void _btnStep_Click(object sender, RoutedEventArgs e)
    {
      setBusy(true);
      highlightActLine(false);
     _model.Scripter.debugOneStep(updateDebugView);
    }

    void highlightActLine(bool on)
    {
      int lineNr = _model.Scripter.CurrentLineNr;
      if ((lineNr >= 0) && (lineNr < _spScript.Children.Count))
      {
        ctlDebugLine ctl = _spScript.Children[lineNr] as ctlDebugLine;
        ctl.activate(on);
      }
    }

  private void setBreakCondition(int lineNr, string condition)
    {
      _model.Scripter.setBreakCondition(lineNr, condition);
    }

    private void _btnPause_Click(object sender, RoutedEventArgs e)
    {
      _model.Scripter.breakDebugging();
    }

    private void _btnStop_Click(object sender, RoutedEventArgs e)
    {
      highlightActLine(false);
      _model.Scripter.restartScript();
      highlightActLine(true);
      setParams();
      _ctlVarTable.setup(_model.Scripter.VarList);
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
  }
}
