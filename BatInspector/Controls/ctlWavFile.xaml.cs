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
using System.IO;

namespace BatInspector.Controls
{
  /// <summary>
  /// Interaktionslogik für ctlWavFile.xaml
  /// </summary>
  public partial class ctlWavFile : UserControl
  {
    AnalysisFile _analysis;
    string _wavFilePath;
    string _wavName;
    int _index;
    dlgSetFocus _dlgFocus;
    ViewModel _model;
    MainWindow _parent;
    bool _initialized = false;

    public string WavFilePath {  get { return _wavFilePath; } }
    public bool WavInit { get { return _initialized; } }
    public AnalysisFile Analysis { get { return _analysis; } }
    public int Index { get { return _index; } set { _index = value; } }
    public string WavName { get { return _wavName; } }

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

    public ctlWavFile(int index, dlgSetFocus setFocus, ViewModel model, MainWindow parent)
    {
      _index = index;
      _dlgFocus = setFocus;
      _model = model;
      _parent = parent;
      InitializeComponent();
      if (_model.Analysis.Files.Count > index)
      {
        _analysis = _model.Analysis.Files[index];
        int pos = _analysis.FileName.LastIndexOf('/');
        _wavName = _analysis.FileName.Substring(pos + 1);
        if (_analysis.FileName.IndexOf(_wavName) < 0)
          DebugLog.log("WAV name mismatch to Report for " + _wavName, enLogType.ERROR);
      }
      _cbSel.Focusable = true;
      Visibility = Visibility.Visible;
    }


    public void updateCallInformations(AnalysisFile analysis)
    {
      _analysis = analysis;
      for(int i = 0; i < _analysis.Calls.Count; i++)
      {
        AnalysisCall call = _analysis.Calls[i];
        ctlDataItem it = _spDataAuto.Children[i] as ctlDataItem; 
        it.setValue(call.SpeciesAuto + "(" + ((int)(call.Probability * 100 + 0.5)).ToString() + "%)");
        ctlSelectItem im = _spDataMan.Children[i] as ctlSelectItem;
        im.setValue(call.SpeciesMan);
      }
      _cbSel.IsChecked = _analysis.Selected;
    }

    public void setFileInformations(string Name, string wavFilePath, List<string> spec)
    {
      _wavFilePath = wavFilePath;
      _wavName = Name;
      _grp.Header = Name.Replace("_", "__");  //hack, because single '_' shows as underlined char
      _analysis = _model.Analysis.find(_wavName);
      if (_analysis != null)
      {
      //  _sampleRate.setValue(_analysis.SampleRate);
      //  _duration.setValue(_analysis.Duration);
        int callNr = 1;
        _spDataAuto.Children.Clear();
        _spDataMan.Children.Clear();
        foreach (AnalysisCall call in _analysis.Calls)
        {
          ctlDataItem it = new ctlDataItem();
          it.Focusable = false;
          it.setup(MyResources.CtlWavCall + " " + callNr.ToString() + ": ", enDataType.STRING, 0, 60, 100);
          it.setValue(call.SpeciesAuto + "(" + ((int)(call.Probability * 100 + 0.5)).ToString() + "%)");
          _spDataAuto.Children.Add(it);

          ctlSelectItem im = new ctlSelectItem();
          im.setup(MyResources.CtlWavCall + " " + callNr.ToString() + ": ", callNr - 1, 60, 65, selItemChanged);
          im.setItems(spec.ToArray());
          im.setValue(call.SpeciesMan);
          _spDataMan.Children.Add(im);
          callNr++;
        }
      }
      _initialized = true;
    }

    public void setFocus()
    {
      _cbSel.Focus();
    }

    public void toggleCheckBox()
    {
      _cbSel.IsChecked = !_cbSel.IsChecked;
      _analysis.Selected = _cbSel.IsChecked == true;
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
      if (File.Exists(_wavFilePath + _wavName))
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
      else
        DebugLog.log("Zoom not possible, file '" + _wavName + "' does not exist", enLogType.ERROR);
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

    public void _btnCopy_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        for (int i = 0; i < _analysis.Calls.Count; i++)
        {
          ctlSelectItem ctlm = _spDataMan.Children[i] as ctlSelectItem;
          if ((_analysis.Calls[i].Probability >= _model.Settings.ProbabilityMin) &&
               SpeciesInfos.isInList(_model.SpeciesInfos, Analysis.Calls[i].SpeciesAuto))
            ctlm.setValue(Analysis.Calls[i].SpeciesAuto.ToUpper());
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
      if (_analysis != null)
      {
        for (int i = 0; i < _analysis.Calls.Count; i++)
        {
          _analysis.Calls[i].SpeciesMan = "---";
          ctlSelectItem ctlm = _spDataMan.Children[i] as ctlSelectItem;
          ctlm.setValue("---");
        }
      }
    }

    private void _cbSel_Click(object sender, RoutedEventArgs e)
    {
      if(_analysis != null)
        _analysis.Selected = _cbSel.IsChecked == true;
    }
  }
}
