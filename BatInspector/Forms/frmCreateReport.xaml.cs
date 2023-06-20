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
using System.Windows.Interop;
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
    ViewModel _model;
    public frmCreateReport(ViewModel model)
    {
      InitializeComponent();
      _model = model;
    }

    private void _btnCreate_Click(object sender, RoutedEventArgs e)
    {

        if ((_ctlReport._dtStart.SelectedDate != null) && (_ctlReport._dtEnd.SelectedDate != null))
        {
          DateTime start = (DateTime)_ctlReport._dtStart.SelectedDate;
          DateTime end = (DateTime)_ctlReport._dtEnd.SelectedDate;
          enPeriod period = (enPeriod)_ctlReport._cbPeriod.SelectedIndex;
          _model.SumReport.createReport(start, end, period, _ctlReport._tbRootDir.Text,
                                        _ctlReport._tbReportName.Text);
        }
        else
          MessageBox.Show("Please specify start and end date", "Attention", MessageBoxButton.OK, MessageBoxImage.Exclamation);
      this.Visibility = Visibility.Hidden;
    }

    private void _btnCancel_Click(object sender, RoutedEventArgs e)
    {
      this.Visibility = Visibility.Hidden;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      winUtils.hideCloseButton(new WindowInteropHelper(this).Handle);
    }
  }
}