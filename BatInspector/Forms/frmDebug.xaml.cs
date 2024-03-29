/********************************************************************************
 *               Author: Christian Müller
 *     Date of creation: 2021-08-10                                       
 *   Copyright (C) 2023: Christian Müller chrmue44(at)gmail(dot).com
 *
 *              Licence:  CC BY-NC 4.0 
 ********************************************************************************/

using BatInspector.Controls;
using libParser;
using System;
using System.IO;
using System.Windows;
using System.Windows.Interop;

namespace BatInspector.Forms
{
  /// <summary>
  /// Interaction logic for frmDebug.xaml
  /// </summary>
  public partial class frmDebug : Window
  {
 
    ViewModel _model;
    int _activeLine = -1;
    public frmDebug(ViewModel model)
    {
      InitializeComponent();
      _model = model;
      _ctlVarTable.setup(_model.Scripter.VarList);
    }

    public void setup(string script)
    {
      try
      {
        string[] lines = File.ReadAllLines(script);
        _spScript.Children.Clear();
        for(int i= 0; i<lines.Length; i++)
        {
          ctlDebugLine line = new ctlDebugLine();
          line.setup(i + 1, lines[i]);
          _spScript.Children.Add(line);
        }
      }
      catch(Exception ex)
      {
        DebugLog.log("error reading script: " + script + " " + ex.ToString(), enLogType.ERROR);
      }
    }

    public void activate(int i)
    {
      if ((_activeLine >= 0) && (_activeLine < _spScript.Children.Count))
      {
        ctlDebugLine ctl = _spScript.Children[_activeLine] as ctlDebugLine;
        ctl.activate(false);
      }
      if ((i >= 0) && (i < _spScript.Children.Count))
      {
        _activeLine = i;
        ctlDebugLine ctl = _spScript.Children[_activeLine] as ctlDebugLine;
        ctl.activate(true);
      }
    }
    
    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      winUtils.hideCloseButton(new WindowInteropHelper(this).Handle);
    }

    private void _btnClose_Click(object sender, RoutedEventArgs e)
    {
      this.Visibility = Visibility.Hidden;
    }

    private void _btnStart_Click(object sender, RoutedEventArgs e)
    {

        }

    private void _btnStep_Click(object sender, RoutedEventArgs e)
    {
      ctlDebugLine ctl = null;
      if ((_activeLine >= 0) && (_activeLine < _spScript.Children.Count))
      {
        ctl = _spScript.Children[_activeLine] as ctlDebugLine;
        ctl.activate(false);
      }
      _activeLine++;
       ctl = _spScript.Children[_activeLine] as ctlDebugLine;
      ctl.activate(true);
    }

    private void _btnPause_Click(object sender, RoutedEventArgs e)
    {

        }

    private void _btnStop_Click(object sender, RoutedEventArgs e)
    {

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
  }
}
