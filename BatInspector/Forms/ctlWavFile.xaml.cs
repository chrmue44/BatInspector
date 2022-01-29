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

using BatInspector.Forms;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;


namespace BatInspector
{
  /// <summary>
  /// Interaktionslogik für ctlWavFile.xaml
  /// </summary>
  public partial class ctlWavFile : UserControl
  {
    AnalysisFile _analysis;
  //  List<ListItem> _list;
    string _wavFilePath;
    int _index;
    dlgSetFocus _dlgFocus;
    ViewModel _model;

    public bool InfoVisible
    {
      get { return _grpInfoAuto.Visibility == Visibility.Visible; }
      set
      {
        _grpInfoAuto.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
        _grpInfoMan.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
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
    public ctlWavFile(int index, dlgSetFocus setFocus, ViewModel model)
    {
      _index = index;
      _dlgFocus = setFocus;
      _model = model;
      InitializeComponent();
      _duration.setup("Duration [s]: ", enDataType.DOUBLE, 3);
      _duration.Focusable = false;
      _sampleRate.setup("Sampling Rate [Hz]: ", enDataType.INT, 0);
      _sampleRate.Focusable = false;
      _cbSel.Focusable = true;
    }

    public int Index { get { return _index; } set { _index = value; } }

    public void setFileInformations(string Name, AnalysisFile analysis, string wavFilePath)
    {
      _analysis = analysis;
      _wavFilePath = wavFilePath;
      _grp.Header = Name;
      string[] spec = { "todo","PPIP", "PNAT", "PPYG", "NNOC", "NLEI", "MDAU", "ESER","BBAR","CRIC","?","---" };
      if (analysis != null)
      {
        _sampleRate.setValue(_analysis.SampleRate);
        _duration.setValue(_analysis.Duration);
        int callNr = 1;
        foreach (AnalysisCall call in _analysis.Calls)
        {
          ctlDataItem it = new ctlDataItem();
          it.Focusable = false;
          it.setup("Call " + callNr.ToString() + ": ", enDataType.STRING);
          it.setValue(call.SpeciesAuto + "(Prob: " + call.Probability.ToString("0.###") + ")");
          _spDataAuto.Children.Add(it);

          ctlSelectItem im = new ctlSelectItem();
          im.setup("Call " + callNr.ToString() + ": ", callNr - 1, selItemChanged);
          im.setItems(spec);
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
        FrmZoom frm = new FrmZoom(_grp.Header.ToString(), _analysis, _wavFilePath, _model);
        frm._imgFt.Source = _img.Source;
        frm.Show();
      }
      else
        MessageBox.Show("Zoom not supported without analysis, perform analysis first!", "Information", MessageBoxButton.OK, MessageBoxImage.Warning);
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
      for (int i = 0; i < _analysis.Calls.Count; i++)
      {
        _analysis.Calls[i].SpeciesMan = Analysis.Calls[i].SpeciesAuto;
        ctlSelectItem ctlm = _spDataMan.Children[i] as ctlSelectItem;
        ctlm.setValue(Analysis.Calls[i].SpeciesAuto);
      }
    }

    private void _tbRemarks_TextChanged(object sender, TextChangedEventArgs e)
    {
      _analysis.Remarks = _tbRemarks.Text;
    }
  }
}
