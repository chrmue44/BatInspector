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

    public ctlWavFile()
    {
      InitializeComponent();
      _list = new List<ListItem>();
    }

    public void setFileInformations(string Name, AnalysisFile analysis)
    {
      _list.Clear();
      _analysis = analysis;
      _grp.Header = Name;
      if (analysis != null)
      {
        ListItem item = new ListItem("sample Rate", _analysis.SampleRate);
        _list.Add(item);
        item = new ListItem("Duration", _analysis.Duration);
        _list.Add(item);
        item = new ListItem("Nr. of Calls", _analysis.Calls.Count);
        _list.Add(item);
        _lvFileInfo.ItemsSource = _list;
      }
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
      int sampleRate = (int)_list[0].Value;
      FrmZoom frm = new FrmZoom(_grp.Header.ToString(), _analysis);
      frm._img.Source = this.Img.Source;
      frm._img.Width = this.Img.Width;
      frm._img.Height = this.Img.Height;
      frm.Show();
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
