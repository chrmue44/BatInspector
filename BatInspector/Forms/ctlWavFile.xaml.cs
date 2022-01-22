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
          it.setValue(call.Species + "(Prob: " + call.Probability.ToString("0.###") + ")");
          callNr++;
          _spData.Children.Add(it);
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
        frm._imgFt.Source = this.Img.Source;
        frm._imgFt.Width = this.Img.Width;
        frm._imgFt.Height = this.Img.Height;
        frm.Show();
      }
      else
        MessageBox.Show("Zoom not supported without analysis, perform analysis first!", "Information", MessageBoxButton.OK, MessageBoxImage.Warning);
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
  }
}
