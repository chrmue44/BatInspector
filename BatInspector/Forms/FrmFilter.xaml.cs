using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BatInspector.Forms
{
  /// <summary>
  /// Interaktionslogik für FrmFilter.xaml
  /// </summary>
  public partial class FrmFilter : Window
  {
    Filter _filter;

    public FrmFilter(Filter filter)
    {
      _filter = filter;
      InitializeComponent();
      populateTree();
    }

    void populateTree()
    {
      _sp.Children.Clear();
      foreach (FilterItem fItem in _filter.Items)
      {
        ctlFilterItem fIt = new ctlFilterItem();
        fIt.setup(fItem, deleteFilter);
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
      item.setup(fIt, deleteFilter);
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

    }
  }
}
