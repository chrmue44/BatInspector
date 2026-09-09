using BatInspector.Controls;
using System.IO;
using System.Windows;

namespace BatInspector.Forms
{
  /// <summary>
  /// Interaction logic for frmExportData.xaml
  /// </summary>
  public partial class frmExportData : Window
  {
    public frmExportData()
    {
      InitializeComponent();
      _sp.Children.Clear();
      _btnNaturGucker.IsEnabled = false;
    }

    public void setup()
    {
      _ctlWavFolder.setup(BatInspector.Properties.MyResources.frmExportData_SelectWAVFolder, 250, true,"", initFileList);

    }

    private void initFileList()
    {
      ExportDataJson exp = App.Model.SumReport.getListOfDocumentRecordings(_ctlWavFolder.getValue());
      _sp.Children.Clear();
      foreach (ExportDataItem item in exp.DocumentFiles)
      {
        ctlExportData ctl = new ctlExportData();
        ctl.setup(item);
        _sp.Children.Add(ctl);
      }
    }

    private void _btnCancel_Click(object sender, RoutedEventArgs e)
    {
      this.Close();
    }

    private void _btnInaturalist_Click(object sender, RoutedEventArgs e)
    {
      ExportDataJson exp = App.Model.SumReport.getListOfDocumentRecordings(_ctlWavFolder.getValue());

      for (int i = 0; i < exp.DocumentFiles.Length; i++)
      {
        ctlExportData? ctl = _sp.Children[i] as ctlExportData;
        if (ctl != null)
        {
          exp.DocumentFiles[i] = ctl.getData();
        }
      }
      string expName = Path.Combine(AppParams.AppParamsPath, "exp_inaturalist.json");
      exp.save(expName);
      this.Close();
    }
  }
}
