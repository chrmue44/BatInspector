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
  /// Interaction logic for frmCreateReport.xaml
  /// </summary>
  public partial class frmCreateReport : Window
  {
    public frmCreateReport()
    {
      InitializeComponent();
    }

    private void _btnCreate_Click(object sender, RoutedEventArgs e)
    {
      this.Close();
    }

    private void _btnCancel_Click(object sender, RoutedEventArgs e)
    {
      this.Close();
    }
  }
}
