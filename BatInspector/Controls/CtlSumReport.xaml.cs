
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
    Window _parent = null;
    public CtlSumReport()
    {
      InitializeComponent();
      _cbPeriod.SelectedIndex = 0;
      _tbReportName.Text = "sum_report.csv";
    }

    public void setup( Window parent)
    {
      _parent = parent;
    }

    private void btnSetDir_Click(object sender, RoutedEventArgs e)
    {
      FolderPicker dlg = new FolderPicker();
      if(dlg.ShowDialog() == true)
      {
        _tbRootDir.Text = dlg.ResultPath;
      }
      bringParentToFront();
    }

    private void btnSetDestDir_Click(object sender, RoutedEventArgs e)
    {
      FolderPicker dlg = new FolderPicker();
      if (dlg.ShowDialog() == true)
      {
        _tbDstDir.Text = dlg.ResultPath;
      }
      bringParentToFront();
    }
    private void bringParentToFront()
    {
      if (_parent != null)
      {
        _parent.Topmost = true;
        _parent.Topmost = false;
      }
    }
  }
}
