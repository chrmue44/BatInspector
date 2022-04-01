/********************************************************************************
 *               Author: Christian Müller
 *      Date of cration: 2021-08-10                                       
 *   Copyright (C) 2021: christian Müller christian(at)chrmue(dot).de
 *
 *              Licence:
 * 
 * THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND,
 * EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
 ********************************************************************************/

using BatInspector.Controls;
using BatInspector.Forms;
using libParser;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using BatInspector.Properties;

namespace BatInspector.Controls
{
  /// <summary>
  /// Interaktionslogik für ctlWavFile.xaml
  /// </summary>
  public partial class ctlWavFile : UserControl
  {
    AnalysisFile _analysis;
  //  List<ListItem> _list;
    string _wavFilePath;
    string _wavName;
    int _index;
    dlgSetFocus _dlgFocus;
    ViewModel _model;
    MainWindow _parent;

    public bool InfoVisible
    {
      get { return _grpInfoAuto.Visibility == Visibility.Visible; }
      set
      {
        _grpInfoAuto.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
        _grpInfoMan.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
        _btnCopy.Visibility = value ? Visibility.Visible : Visibility.Hidden;
        _btnIgnore.Visibility = value ? Visibility.Visible : Visibility.Hidden;
        if (!value)
        {
          _grid.ColumnDefinitions[1].Width = new GridLength(0);
          _grid.ColumnDefinitions[2].Width = new GridLength(0);
        }
        else
        {
          _grid.ColumnDefinitions[1].Width = new GridLength(200);
          _grid.ColumnDefinitions[2].Width = new GridLength(200);
        }
      }
    }

    public AnalysisFile Analysis { get { return _analysis; } }
    public ctlWavFile(int index, dlgSetFocus setFocus, ViewModel model, MainWindow parent)
    {
      _index = index;
      _dlgFocus = setFocus;
      _model = model;
      _parent = parent;
      InitializeComponent();
     /* _duration.setup(MyResources.Duration + " [s]: ", enDataType.DOUBLE, 3, 110);
      _duration.Focusable = false;
      _sampleRate.setup(MyResources.SamplingRate + " [Hz]: ", enDataType.INT, 0, 110);
      _sampleRate.Focusable = false; */
      _cbSel.Focusable = true;
    }

    public int Index { get { return _index; } set { _index = value; } }
    public string WavName { get { return _wavName; } }
    public void setFileInformations(string Name, AnalysisFile analysis, string wavFilePath)
    {
      _analysis = analysis;
      _wavFilePath = wavFilePath;
      _wavName = Name;
      _grp.Header = Name.Replace("_", "__");  //hack, because single '_' shows as underlined char
      List<string> spec = new List<string>();
      foreach(SpeciesInfos si in _model.Settings.Species)
      {
        if(si.Show)
          spec.Add(si.Abbreviation);
      }
      spec.Add("todo");
      spec.Add("?");
      spec.Add("---");
      if (analysis != null)
      {
      //  _sampleRate.setValue(_analysis.SampleRate);
      //  _duration.setValue(_analysis.Duration);
        int callNr = 1;
        foreach (AnalysisCall call in _analysis.Calls)
        {
          ctlDataItem it = new ctlDataItem();
          it.Focusable = false;
          it.setup(MyResources.CtlWavCall + " " + callNr.ToString() + ": ", enDataType.STRING, 0, 60, 100);
          it.setValue(call.SpeciesAuto + "(Prob: " + call.Probability.ToString("0.###") + ")");
          _spDataAuto.Children.Add(it);

          ctlSelectItem im = new ctlSelectItem();
          im.setup(MyResources.CtlWavCall + " " + callNr.ToString() + ": ", callNr - 1, 60, 65, selItemChanged);
          im.setItems(spec.ToArray());
          im.setValue(call.SpeciesMan);
          _spDataMan.Children.Add(im);
          callNr++;
        }
      }
    }

    public void setFocus()
    {
      _cbSel.Focus();
    }

    public void toggleCheckBox()
    {
      _cbSel.IsChecked = !_cbSel.IsChecked;
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
      if (_analysis != null)
      {
        _parent.setZoom(_wavName, _analysis, _wavFilePath, _img.Source);
      }
      else
      {
        AnalysisFile ana = new AnalysisFile(_wavName);
        ana.SampleRate = 383500;
        ana.Duration = 3.001;
        _parent.setZoom(_wavName, ana, _wavFilePath, _img.Source);

      }
    }

    private void selItemChanged(int index, string val)
    {
      if (_analysis != null)
      {
        if ((index >= 0) && (index < _analysis.Calls.Count))
          _analysis.Calls[index].SpeciesMan = val;
        else
          DebugLog.log("ctlWavFile.selItemChanged(): index error", enLogType.ERROR);
      }
    }

    private void ctlLostFocus(object sender, RoutedEventArgs e)
    {
      _cbSel.BorderThickness = new Thickness(1, 1, 1, 1);
    }

    private void ctlGotFocus(object sender, RoutedEventArgs e)
    {
      _cbSel.BorderThickness = new Thickness(3, 3, 3, 3);
      _dlgFocus(_index);
    }

    private void _btnCopy_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        for (int i = 0; i < _analysis.Calls.Count; i++)
        {
          ctlSelectItem ctlm = _spDataMan.Children[i] as ctlSelectItem;
          if (_analysis.Calls[i].Probability >= _model.Settings.ProbabilityMin)
            ctlm.setValue(Analysis.Calls[i].SpeciesAuto);
          else
            ctlm.setValue("");
          _analysis.Calls[i].SpeciesMan = ctlm.getValue();
        }
      }
      catch (Exception ex)
      {
        DebugLog.log("Error copying species: " + ex.ToString(), enLogType.ERROR);
      }
    }

    private void _tbRemarks_TextChanged(object sender, TextChangedEventArgs e)
    {
      _analysis.Remarks = _tbRemarks.Text;
    }

    private void _btnIgnore_Click(object sender, RoutedEventArgs e)
    {
      for (int i = 0; i < _analysis.Calls.Count; i++)
      {
        _analysis.Calls[i].SpeciesMan = "---";
        ctlSelectItem ctlm = _spDataMan.Children[i] as ctlSelectItem;
        ctlm.setValue("---");
      }
    }
  }
}
