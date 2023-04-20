
using System;
using System.Windows;
using System.Windows.Controls;


namespace BatInspector.Controls
{
  /// <summary>
  /// Interaktionslogik für CtlSumReport.xaml
  /// </summary>
  public partial class CtlSumReport : UserControl
  {
    ViewModel _model = null;

    public CtlSumReport()
    {
      InitializeComponent();
      _cbPeriod.SelectedIndex = 0;
      _tbReportName.Text = "sum_report.csv";
    }

    public void setModel(ViewModel model)
    {
      _model = model;
    }

    private void btnSetDir_Click(object sender, RoutedEventArgs e)
    {
      FolderPicker dlg = new FolderPicker();
      if(dlg.ShowDialog() == true)
      {
        _tbRootDir.Text = dlg.ResultPath;
      }
    }

    private void btnSetDestDir_Click(object sender, RoutedEventArgs e)
    {
      FolderPicker dlg = new FolderPicker();
      if (dlg.ShowDialog() == true)
      {
        _tbDstDir.Text = dlg.ResultPath;
      }
    }

 /*   private void _btnCreate_Click(object sender, RoutedEventArgs e)
    {
      if ((_dtStart.SelectedDate != null) && (_dtEnd.SelectedDate != null))
      {
        DateTime start = (DateTime)_dtStart.SelectedDate;
        DateTime end = (DateTime)_dtEnd.SelectedDate;
        enPeriod period = (enPeriod)_cbPeriod.SelectedIndex;
        _model.SumReport.createReport(start, end, period, _tbRootDir.Text);
      }
      else
        MessageBox.Show("Please specify start and end date", "Attention", MessageBoxButton.OK, MessageBoxImage.Exclamation);
    }*/
  }
}
