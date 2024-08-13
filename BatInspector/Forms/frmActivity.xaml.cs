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
    private List<List<int>> _heatMap = null;
    public frmActivity()
    {
      InitializeComponent();
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
      this.Close();
    }

    public void createPlot(List<List<int>> hm)
    {
      _heatMap = hm;
    }

    private void _btnCreate_Click(object sender, RoutedEventArgs e)
    {
      if (_heatMap != null)
      {
        _ctlActivity.createPlot(_heatMap);
      }
    }
  }
}
