/********************************************************************************
 *               Author: Christian Müller
 *     Date of creation: 2023-08-18                                       
 *   Copyright (C) 2023: Christian Müller chrmue44(at)gmail(dot).com
 *
 *              Licence:  CC BY-NC 4.0 
 ********************************************************************************/

using BatInspector.Forms;
using libParser;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using BatInspector.Properties;
using System.IO;
using libScripter;

namespace BatInspector.Controls
{
  /// <summary>
  /// Interaktionslogik für ctlWavFile.xaml
  /// </summary>
  public partial class ctlWavFile : UserControl
  {
    AnalysisFile _analysis;
    string _wavFilePath;
    PrjRecord _record;
    ViewModel _model;
    MainWindow _parent;
    bool _initialized = false;

    public string WavFilePath {  get { return _wavFilePath; } }
    public bool WavInit { get { return _initialized; } }
    public AnalysisFile Analysis { get { return _analysis; } }
    public string WavName { get { return _record.File; } }

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


    public ctlWavFile(AnalysisFile analysis, PrjRecord record,  ViewModel model, MainWindow parent, bool showButtons)
    {
      _model = model;
      _parent = parent;
      InitializeComponent();
      _ctlRemarks.setup(MyResources.CtlWavRemarks, enDataType.STRING, 0, 80, true, _tbRemarks_TextChanged);
      _analysis = analysis;
      _record = record;
      _btnCopy.Visibility = showButtons ? Visibility.Visible : Visibility.Collapsed;
      _btnCopy.IsEnabled = showButtons;
      _btnTools.Visibility = showButtons ? Visibility.Visible : Visibility.Collapsed;
      _btnTools.IsEnabled = showButtons;
      _cbSel.Focusable = true;
      _cbSel.IsChecked = record.Selected;
      Visibility = Visibility.Visible;
    }


    public void updateCallInformations(AnalysisFile analysis, PrjRecord rec)
    {
      _analysis = analysis;
      List<string> spec = new List<string>();
      if (_spDataMan.Children.Count > 0)
      {
        ctlSelectItem ctl = _spDataMan.Children[0] as ctlSelectItem;
        spec = ctl.getItems();
      }
      initCallInformations(spec);
/*      for(int i = 0; i < _analysis.Calls.Count; i++)
      {
        AnalysisCall call = _analysis.Calls[i];
        if (i < _spDataAuto.Children.Count)
        {
          ctlDataItem it = _spDataAuto.Children[i] as ctlDataItem;
          it.setValue(call.getString(Cols.SPECIES) + "(" + ((int)(call.getDouble(Cols.PROBABILITY) * 100 + 0.5)).ToString() + "%)");
          ctlSelectItem im = _spDataMan.Children[i] as ctlSelectItem;
          im.setValue(call.getString(Cols.SPECIES_MAN));
        }
      } */
      _cbSel.IsChecked = rec.Selected;
    }

    public void setFileInformations(PrjRecord record, string wavFilePath, AnalysisFile analysis, List<string> spec)
    {
      _wavFilePath = wavFilePath;
      _record = record;
      _cbSel.IsChecked = record.Selected;
      _btnWavFile.Content = _record.File.Replace("_", "__");  //hack, because single '_' shows as underlined char
      //_grp.Header = Name.Replace("_", "__");  //hack, because single '_' shows as underlined char
      _analysis = analysis;
      initCallInformations(spec);
      _initialized = true;
    }

    public void setFocus()
    {
      _cbSel.Focus();
    }

    public void toggleCheckBox()
    {
      _cbSel.IsChecked = !_cbSel.IsChecked;
      _record.Selected = !_record.Selected;
    }

    private void initCallInformations(List<string> spec)
    {
      if (_analysis != null)
      {
        int wLbl = 48;
        int callNr = 1;
        _spDataAuto.Children.Clear();
        _spDataMan.Children.Clear();
        _ctlRemarks.setValue(_analysis.getString(Cols.REMARKS));
        foreach (AnalysisCall call in _analysis.Calls)
        {
          string callStr = call.getString(Cols.NR);
          ctlDataItem it = new ctlDataItem();
          it.Focusable = false;
          it.setup(MyResources.CtlWavCall + " " + callStr + ": ", enDataType.STRING, 0, wLbl);
          it.setValue(call.getString(Cols.SPECIES) + "(" + ((int)(call.getDouble(Cols.PROBABILITY) * 100 + 0.5)).ToString() + "%)");
          _spDataAuto.Children.Add(it);

          ctlSelectItem im = new ctlSelectItem();
          im.setup(MyResources.CtlWavCall + " " + callStr + ": ", callNr - 1, wLbl, 90, selItemChanged, clickCallLabel,
                MyResources.ctlWavToolTipCall);
          im.setItems(spec.ToArray());
          im.setValue(call.getString(Cols.SPECIES_MAN));
          _spDataMan.Children.Add(im);
          callNr++;
        }
      }
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
      string fName = Path.Combine(_wavFilePath ,_record.File);
      if (File.Exists(fName))
      {
        if (_analysis != null)
        {
          _parent.setZoom(_record.File, _analysis, _wavFilePath, this);
        }
        else
        {
          AnalysisFile ana = new AnalysisFile(_record.File, 383500, 3.001);
           _parent.setZoom(_record.File, ana, _wavFilePath, this);
        }
      }
      else
        DebugLog.log("Zoom not possible, file '" + _record.File + "' does not exist", enLogType.ERROR);
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
    }

    public void _btnCopy_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        for (int i = 0; i < _analysis.Calls.Count; i++)
        {
          ctlSelectItem ctlm = _spDataMan.Children[i] as ctlSelectItem;
          if ((_analysis.Calls[i].getDouble(Cols.PROBABILITY) >= 0.5) &&  //TODO no fix value
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
 //     if (_isSetupCall)
 //       _isSetupCall = false;
   //   else
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
      updateCallInformations(_analysis, _record);
      this.InvalidateVisual();
      this.UpdateLayout();
    }

    private void _btnTools_Click(object sender, RoutedEventArgs e)
    {
      if (_analysis != null)
      {
        FrmTools frm = new FrmTools(_record.File, _model);
        bool upd = frm.ShowDialog() == true;
        if (upd)
          update();
      }
    }

    private void _cbSel_Click(object sender, RoutedEventArgs e)
    {
      _record.Selected = _cbSel.IsChecked == true;
    }

    private void _btnWavFile_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        string exe = AppParams.Inst.WavTool;
        if (!string.IsNullOrEmpty(exe))
        {
          ProcessRunner p = new ProcessRunner();
          string fName = Path.Combine(WavFilePath, _record.File);
          p.launchCommandLineApp(exe, null, "", true, fName);
        }
      }
      catch(Exception ex) 
      {
        DebugLog.log("error launching WAV tool: " + ex.ToString(), enLogType.ERROR);
      }
    }
  }
}
