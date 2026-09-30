using BatInspector.Controls;
using System.IO;
using System.Windows;
using BatInspector.Properties;

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
      _ctlWavFolder.setup(MyResources.frmExportData_SelectWAVFolder, 250, true,"", initFileList);
      _ctlEvalFolderName.setup(MyResources.frmExportData_DirEval, enDataType.STRING, 0, 245, true);
      _ctlEvalFolderName.setValue("Auswertungen");
      _ctlEvalFolderName.Visibility = Visibility.Collapsed;
      _ctlWavFolderName.setup(BatInspector.Properties.MyResources.frmExportData_DirWav, enDataType.STRING, 0, 250, true);
      _ctlWavFolderName.setValue("wav");
      _ctlWavFolderName.Visibility = Visibility.Collapsed;
    }

    private void initFileList()
    {
      ExportDataJson? exp = App.Model.SumReport.getListOfDocumentRecordings(_ctlWavFolder.getValue());
      _sp.Children.Clear();
      if (exp != null)
      {
        foreach (ExportDataItem item in exp.DocumentFiles)
        {
          ctlExportData ctl = new ctlExportData();
          ctl.setup(item);
          _sp.Children.Add(ctl);
        }
      }
    }

    private void _btnCancel_Click(object sender, RoutedEventArgs e)
    {
      this.Close();
    }

    private void _btnInaturalist_Click(object sender, RoutedEventArgs e)
    {
      ExportDataJson? exp = getDataFields();
      if (exp != null)
      {
        string expName = Path.Combine(AppParams.AppParamsPath, "exp_inaturalist.json");
        exp.save(expName);
      }
      this.Close();
    }

    private ExportDataJson? getDataFields()
    {
      ExportDataJson? exp = App.Model.SumReport.getListOfDocumentRecordings(_ctlWavFolder.getValue());
      if (exp != null)
      {
        for (int i = 0; i < exp.DocumentFiles.Length; i++)
        {
          ctlExportData? ctl = _sp.Children[i] as ctlExportData;
          if (ctl != null)
          {
            exp.DocumentFiles[i] = ctl.getData();
          }
        }
      }
      return exp;
    }



    private void _btnMultiBase_Click(object sender, RoutedEventArgs e)
    {
      if (_cbRecursive.IsChecked == true)
      {
        bool append = false;
        ExportDataJson.exportRecursive(_ctlWavFolder.getValue(), _ctlEvalFolderName.getValue(), _ctlWavFolderName.getValue(), ref append);
      }
      else
      {
        ExportDataJson? exp = getDataFields();
        if (exp != null)
        {
          string expName = Path.Combine(AppParams.AppParamsPath, "exp_multibase.csv");
          exp.saveAsCsv(expName, true);
        }
      }
      this.Close();
    }


    private void _cbRecursive_Click(object sender, RoutedEventArgs e)
    {
      string path = _ctlWavFolder.getValue();
      if (_cbRecursive.IsChecked == true)
      {
        _ctlEvalFolderName.Visibility = Visibility.Visible;
        _ctlWavFolderName.Visibility = Visibility.Visible;
        _ctlWavFolder.setup(MyResources.frmExportData_SelectRoot, 250, true, "", null);
      }
      else
      {
        _ctlEvalFolderName.Visibility = Visibility.Collapsed;
        _ctlWavFolderName.Visibility = Visibility.Collapsed;
        _ctlWavFolder.setup(MyResources.frmExportData_SelectWAVFolder, 250, true, "", initFileList);
      }
      _ctlWavFolder.setValue(path);
    }
  }
}
