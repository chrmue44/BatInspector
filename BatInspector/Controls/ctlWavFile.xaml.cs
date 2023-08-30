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
    bool _isSetupCall = false;

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
        _btnTools.Visibility = value ? Visibility.Visible : Visibility.Hidden;
        if (!value)
        {
          _grid.ColumnDefinitions[1].Width = new GridLength(0);
          _grid.ColumnDefinitions[2].Width = new GridLength(0);
        }
        else
        {
          _grid.ColumnDefinitions[1].Width = new GridLength(160);
          _grid.ColumnDefinitions[2].Width = new GridLength(160);
        }
      }
    }

    public ctlWavFile(int index, dlgSetFocus setFocus, ViewModel model, MainWindow parent, bool showButtons)
    {
      _index = index;
      _dlgFocus = setFocus;
      _model = model;
      _parent = parent;
      InitializeComponent();
      _ctlRemarks.setup(MyResources.CtlWavRemarks, enDataType.STRING, 0, 80, true, _tbRemarks_TextChanged);
      if ((_model.Prj != null) && (_model.Prj.Analysis.Files.Count > index))
      {
        _analysis = _model.Prj.Analysis.Files[index];
        string fName = _analysis.Name;
        int pos = fName.LastIndexOf('/');
        _wavName = fName.Substring(pos + 1);
        if (fName.IndexOf(_wavName) < 0)
          DebugLog.log("WAV name mismatch to Report for " + _wavName, enLogType.ERROR);
      }
      _btnCopy.Visibility = showButtons ? Visibility.Visible : Visibility.Collapsed;
      _btnCopy.IsEnabled = showButtons;
      _btnTools.Visibility = showButtons ? Visibility.Visible : Visibility.Collapsed;
      _btnTools.IsEnabled = showButtons;
      _cbSel.Focusable = true;
      Visibility = Visibility.Visible;
    }


    public void updateCallInformations(AnalysisFile analysis)
    {
      _analysis = analysis;
      for(int i = 0; i < _analysis.Calls.Count; i++)
      {
        AnalysisCall call = _analysis.Calls[i];
        if (i < _spDataAuto.Children.Count)
        {
          ctlDataItem it = _spDataAuto.Children[i] as ctlDataItem;
          it.setValue(call.getString(Cols.SPECIES) + "(" + ((int)(call.getDouble(Cols.PROBABILITY) * 100 + 0.5)).ToString() + "%)");
          ctlSelectItem im = _spDataMan.Children[i] as ctlSelectItem;
          im.setValue(call.getString(Cols.SPECIES_MAN));
        }
      }
      _cbSel.IsChecked = _analysis.Selected;
    }

    public void setFileInformations(string Name, string wavFilePath, AnalysisFile analysis, List<string> spec)
    {
      _wavFilePath = wavFilePath;
      _wavName = Name;
      _grp.Header = Name.Replace("_", "__");  //hack, because single '_' shows as underlined char
      _analysis = analysis;
      _isSetupCall = true;
      if (_analysis != null)
      {
        int callNr = 1;
        _spDataAuto.Children.Clear();
        _spDataMan.Children.Clear();
        _ctlRemarks.setValue(_analysis.getString(Cols.REMARKS));
        foreach (AnalysisCall call in _analysis.Calls)
        {
          string callStr = call.getString(Cols.NR);
          ctlDataItem it = new ctlDataItem();
          it.Focusable = false;
          it.setup(MyResources.CtlWavCall + " " + callStr + ": ", enDataType.STRING, 0, 60);
          it.setValue(call.getString(Cols.SPECIES) + "(" + ((int)(call.getDouble(Cols.PROBABILITY) * 100 + 0.5)).ToString() + "%)");
          _spDataAuto.Children.Add(it);

          ctlSelectItem im = new ctlSelectItem();
          im.setup(MyResources.CtlWavCall + " " + callStr + ": ", callNr - 1, 45, 90, selItemChanged, clickCallLabel,
          MyResources.ctlWavToolTipCall);
          im.setItems(spec.ToArray());
          im.setValue(call.getString(Cols.SPECIES_MAN));
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
      if(_analysis != null)
        _analysis.Selected = _cbSel.IsChecked == true;
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
      string fName = Path.Combine(_wavFilePath ,_wavName);
      if (File.Exists(fName))
      {
        if (_analysis != null)
        {
          _parent.setZoom(_wavName, _analysis, _wavFilePath, _img.Source);
        }
        else
        {
          AnalysisFile ana = new AnalysisFile(_wavName, 383500, 3.001);
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
          _analysis.Calls[index].setString(Cols.SPECIES_MAN, val);
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
          if ((_analysis.Calls[i].getDouble(Cols.PROBABILITY) >= AppParams.Inst.ProbabilityMin) &&
               SpeciesInfos.isInList(_model.SpeciesInfos, Analysis.Calls[i].getString(Cols.SPECIES)))
            ctlm.setValue(Analysis.Calls[i].getString(Cols.SPECIES).ToUpper());
          else
            ctlm.setValue("?");
          _analysis.Calls[i].setString(Cols.SPECIES_MAN, ctlm.getValue());
        }
      }
      catch (Exception ex)
      {
        DebugLog.log("Error copying species: " + ex.ToString(), enLogType.ERROR);
      }
    }

    private void _tbRemarks_TextChanged(enDataType type, object val)
    {
      if (_isSetupCall)
        _isSetupCall = false;
      else
      {
        if ((type == enDataType.STRING) && (_analysis != null))
          _analysis.setString(Cols.REMARKS, _ctlRemarks.getValue());
      }
    }

    private void clickCallLabel(int index)
    {
      Button_Click(null, null);
      _parent.changeCallInZoom(index);
    }

    private void update()
    {
      updateCallInformations(_analysis);
      this.InvalidateVisual();
      this.UpdateLayout();
    }

    private void _btnTools_Click(object sender, RoutedEventArgs e)
    {
      if (_analysis != null)
      {
        FrmTools frm = new FrmTools(_wavName, _model);
        bool upd = frm.ShowDialog() == true;
        if (upd)
          update();
      }
    }

    private void _cbSel_Click(object sender, RoutedEventArgs e)
    {
      if(_analysis != null)
        _analysis.Selected = _cbSel.IsChecked == true;
    }
  }
}
