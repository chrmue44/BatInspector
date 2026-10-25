/********************************************************************************
 *               Author: Christian Müller
 *     Date of creation: 2026-10-03                                       
 *   Copyright (C) 2023: Christian Müller chrmue44(at)gmail(dot).com
 *
 *              Licence:  CC BY-NC 4.0 
 ********************************************************************************/

using BatInspector.Controls;
using BatInspector.Properties;
using BatInspectorPublisher.Core.InputSchema;
using BatInspectorPublisher.Core.Results;
using libParser;
using System.IO;

using System.Windows;

namespace BatInspector.Forms
{
  /// <summary>
  /// Interaction logic for frmExportData.xaml
  /// </summary>
  public partial class frmExportData : Window
  {
    ExportDataJson? _exp = null;
    string _evaluationPath = AppParams.AppParamsPath;
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
      _exp = App.Model.SumReport.getListOfDocumentRecordings(_ctlWavFolder.getValue(), ref _evaluationPath);
      initControls();
    }

    private void initControls()
    {
      _sp.Children.Clear();
      if (_exp != null)
      {
        for (int i = 0; i < _exp.DocumentFiles.Count; i++)
        {
          ctlExportData ctl = new ctlExportData();
          ctl.setup(_exp.DocumentFiles[i], i, deleteItem);
          _sp.Children.Add(ctl);
        }
      }
    }
    private void _btnCancel_Click(object sender, RoutedEventArgs e)
    {
      this.Close();
    }

    private void deleteItem(int index)
    {
      if (_exp != null && index < _exp.DocumentFiles.Count)
        _exp.DocumentFiles.RemoveAt(index);
      initControls();
    }

    private async void _btnInaturalist_Click(object sender, RoutedEventArgs e)
    {
      getDataFromFields();
      string expName = Path.Combine(_evaluationPath, "exp_inaturalist.json");
      if (_exp != null)
      {
        _exp.save(expName, enPlatform.INATURALIST);
        enableButtons(false);
        RunReport? rep = await App.Model.Export.upload(expName);
        if (rep != null)
        {
          bool hasSomethingToFix = false;
          if (rep.Warnings.Count > 0)
          {
            hasSomethingToFix = true;
            foreach (EntryWarning w in rep.Warnings)
            {
              DebugLog.log($"{w}", enLogType.WARNING);
            }
          }
          if(rep.Rejected.Count > 0)
          {
            hasSomethingToFix = true;
            foreach (RejectedEntry w in rep.Rejected)
            {
              DebugLog.log($"{w}", enLogType.ERROR);
            }
          }
          foreach (PublishResult r in rep.Results)
          {
            if ((r.Status == PublishStatus.SkippedUnresolvedTaxon) || (r.Status == PublishStatus.Failed))
            {
              hasSomethingToFix = true;
              DebugLog.log($"{r}", enLogType.ERROR);
            }
            else
              DebugLog.log($"check for upload to iNaturalist: {r.TaxonName}, Status: {r.Status}", enLogType.INFO);
          }
          
          if (hasSomethingToFix)
          {
            await App.MainWin.showMsg(MyResources.frmExportData_iNaturalistFix, MyResources.msgInformation, MessageBoxButton.OK, MessageBoxImage.Information);
            this.Activate();
          }
          else
          {
            bool ok = await App.MainWin.showMsg(MyResources.frmExportData_iNaturalistOkContinue, MyResources.msgInformation, MessageBoxButton.YesNo, MessageBoxImage.Information);
            if (ok)
            {
              rep = await App.Model.Export.upload(expName, true);
              if (rep != null)
              {
                foreach (PublishResult r in rep.Results)
                {
                  DebugLog.log($"Upload to iNaturalist: {r.TaxonName}, Status: {r.Status}", enLogType.INFO);
                }
              }
              await App.MainWin.showMsg(MyResources.frmExportData_iNatExportSuccess, MyResources.msgInformation, MessageBoxButton.OK, MessageBoxImage.Information);
            }
            this.Close();
          }
        }
        enableButtons(true);
      }
    }

    private void enableButtons(bool on)
    {
      _btnInaturalist.IsEnabled = on;
      _btnMultiBase.IsEnabled = on;
      _btnNaturGucker.IsEnabled = on;
    }

    private void getDataFromFields()
    {
      if (_exp != null)
      {
        for (int i = 0; i < _exp.DocumentFiles.Count; i++)
        {
          ctlExportData? ctl = _sp.Children[i] as ctlExportData;
          if (ctl != null)
          {
            _exp.DocumentFiles[i] = ctl.getData();
          }
        }
      }
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
        getDataFromFields();
        if (_exp != null)
        {
          string expName = Path.Combine(AppParams.AppParamsPath, "exp_multibase.csv");
          _exp.saveAsCsv(expName, true);
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
