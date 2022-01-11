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
    List<ListItem> _list;
    string _wavFilePath;
    int _index;
    dlgSetFocus _dlgFocus;

    public ctlWavFile(int index, dlgSetFocus setFocus)
    {
      _index = index;
      _dlgFocus = setFocus;
      InitializeComponent();
      _list = new List<ListItem>();
      _duration.Label = "Duration:";
      _duration.Focusable = false;
      _sampleRate.Label = "Sampling Rate:";
      _sampleRate.Focusable = false;
      _nrOfCalls.Label = "Nr. of Calls:";
      _nrOfCalls.Focusable = false;
      _cbSel.Focusable = true;
    }

    public void setFileInformations(string Name, AnalysisFile analysis, string wavFilePath)
    {
      _list.Clear();
      _analysis = analysis;
      _wavFilePath = wavFilePath;
      _grp.Header = Name;
      if (analysis != null)
      {
        _sampleRate.Value = _analysis.SampleRate.ToString() + " Hz";
        _duration.Value = _analysis.Duration.ToString("0.###") + " s";
        _nrOfCalls.Value = _analysis.Calls.Count.ToString();
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
      int sampleRate = (int)_list[0].Value;
      FrmZoom frm = new FrmZoom(_grp.Header.ToString(), _analysis, _wavFilePath);
      frm._img.Source = this.Img.Source;
      frm._img.Width = this.Img.Width;
      frm._img.Height = this.Img.Height;
      frm.Show();
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

  public class ListItem
  {
    public string Name { get; set; }
    public object Value { get; set; }

    public ListItem(string name, object value)
    {
      Name = name;
      Value = value;
    }

    public override string ToString()
    {
      return Name + ": " + Value.ToString();
    }
  }
}
