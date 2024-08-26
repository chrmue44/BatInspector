using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BatInspector.Forms
{
  public delegate void dlgCreatePlot(ActivityData data);


  /// <summary>
  /// Interaction logic for frmActivity.xaml
  /// </summary>
  public partial class frmActivity : Window
  {
    private ActivityData _data = null;
    public frmActivity(ViewModel model)
    {
      InitializeComponent();
      _ctlActivity.setup(model);
    }



    public void setup()
    {
    }

    /// <summary>
    /// callback function after gathering of data is finished
    /// </summary>
    public void createPlot(ActivityData data)
    {
      if (!Dispatcher.CheckAccess()) // CheckAccess returns true if you're on the dispatcher thread
      {
        Dispatcher.BeginInvoke(new dlgCreatePlot(createPlot), data);
      }
      else
      {
        _data = data;
        _btnCreate.IsEnabled = true;
      }
    }

    private void _btnOK_Click(object sender, RoutedEventArgs e)
    {
      this.Close();
    }

    


    private void _btnCreate_Click(object sender, RoutedEventArgs e)
    {
      if (_data == null)
        return;

      _ctlActivity.createPlot(_data, _ctlActivity._cbMonth.IsChecked == true,
                                     _ctlActivity._cbWeek.IsChecked == true,
                                     _ctlActivity._cbDay.IsChecked == true,
                                     _ctlActivity._cbTwilight.IsChecked == true);
    }

    private void _btnExport_Click(object sender, RoutedEventArgs e)
    {
      SaveFileDialog dlg = new SaveFileDialog();
      string filter = "PNG files (*.png)|*.png";
      dlg.Filter = filter;
      System.Windows.Forms.DialogResult res = dlg.ShowDialog();
      if (res == System.Windows.Forms.DialogResult.OK)
      {
        _ctlActivity.saveBitMap(dlg.FileName);
      }
    }
  }
}
