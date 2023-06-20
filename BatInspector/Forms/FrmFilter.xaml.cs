/********************************************************************************
 *               Author: Christian Müller
 *      Date of cration: 2021-08-10                                       
 *   Copyright (C) 2022: Christian Müller christian(at)chrmue(dot).de
 *
 *              Licence:
 * 
 * THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND,
 * EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
 ********************************************************************************/
using System.Linq;
using System.Windows;
using System.Windows.Interop;

namespace BatInspector.Forms
{
  /// <summary>
  /// Interaktionslogik für FrmFilter.xaml
  /// </summary>
  public partial class FrmFilter : Window
  {
    Filter _filter;
    dlgUpdate _dlgUpdate;
    public FrmFilter(Filter filter, dlgUpdate dlgUpdate)
    {
      _filter = filter;
      _dlgUpdate = dlgUpdate;
      InitializeComponent();
      populateTree();
    }

    void populateTree()
    {
      _sp.Children.Clear();
      int index = 0;
      foreach (FilterItem fItem in _filter.Items)
      {
        ctlFilterItem fIt = new ctlFilterItem();
        fIt.setup(fItem, index++, deleteFilter);
        _sp.Children.Add(fIt);
      }
    }

    private void _btnAdd_Click(object sender, RoutedEventArgs e)
    {
      FilterItem fIt = new FilterItem();
      fIt.Index = _filter.Items.Count();
      fIt.Name = "FILTER" + fIt.Index.ToString();
      _filter.Items.Add(fIt);

      ctlFilterItem item = new ctlFilterItem();
      item.setup(fIt, _filter.Items.Count - 1, deleteFilter);
      _sp.Children.Add(item);
    }

    private void deleteFilter(int index)
    {
      if (index < _filter.Items.Count)
      {
        FilterItem f = _filter.Items[index];
        _filter.Items.Remove(f);
      }
      for(int i = 0; i < _filter.Items.Count; i++)
        _filter.Items[i].Index = i;

      populateTree();
    }

    private void _btnOk_Click(object sender, RoutedEventArgs e)
    {      
      this.Visibility = Visibility.Hidden;
      _dlgUpdate();
    }

    private void _btnHelp_Click(object sender, RoutedEventArgs e)
    {
      string str = BatInspector.Properties.MyResources.FrmFilterListOfVars + ":\n\n";
      str += _filter.getVariables();
      FrmHelpFilter frm = new FrmHelpFilter();
      frm._tbHelp.Text = str;
      frm.Show();
    }

    private void _btnCancel_Click(object sender, RoutedEventArgs e)
    {
      Visibility = Visibility.Hidden;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      winUtils.hideCloseButton(new WindowInteropHelper(this).Handle);
    }

    private void _scrlViewer_ScrollChanged(object sender, System.Windows.Controls.ScrollChangedEventArgs e)
    {

    }
  }
  public delegate void dlgUpdate();
}
