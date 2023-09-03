using System;
using System.Windows;
using System.Windows.Interop;

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
      _ctlReport.setup(this);
    }

    private void _btnCreate_Click(object sender, RoutedEventArgs e)
    {

        if ((_ctlReport._dtStart.SelectedDate != null) && (_ctlReport._dtEnd.SelectedDate != null))
        {
          DateTime start = (DateTime)_ctlReport._dtStart.SelectedDate;
          DateTime end = (DateTime)_ctlReport._dtEnd.SelectedDate;
          enPeriod period = (enPeriod)_ctlReport._cbPeriod.SelectedIndex;
          _model.SumReport.createReport(start, end, period, _ctlReport._tbRootDir.Text,
                                        _ctlReport._tbReportName.Text, _model.SpeciesInfos);
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