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
  /// Interaction logic for frmActivity.xaml
  /// </summary>
  public partial class frmActivity : Window
  {
    private List<List<int>> _data = null;
    private DateTime _start;
    public frmActivity(ViewModel model)
    {
      InitializeComponent();
      _ctlActivity.setup(model);
    }

    public void setup(DateTime start)
    {
      _start = start;
    }

    /// <summary>
    /// callback function after gathering of data is finished
    /// </summary>
    public void createPlot(List<List<int>> data)
    {
      _data = data;
      _btnCreate.IsEnabled = true;
    }

    private void _btnOK_Click(object sender, RoutedEventArgs e)
    {
      this.Close();
    }

    


    private void _btnCreate_Click(object sender, RoutedEventArgs e)
    {
      if (_data == null)
        return;

      double lat = 49;
      double lon = 8;
      _ctlActivity.createPlot(_data, _start, _ctlActivity._cbMonth.IsChecked == true,
                                             _ctlActivity._cbWeek.IsChecked == true,
                                             _ctlActivity._cbDay.IsChecked == true,
                                             _ctlActivity._cbTwilight.IsChecked == true,
                                             lat, lon);
    }
  }
}
