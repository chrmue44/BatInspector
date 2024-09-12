using BatInspector.Forms;
using BatInspector.Properties;
using libParser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace BatInspector.Controls
{
  /// <summary>
  /// Interaction logic for CtlPrjButtons.xaml
  /// </summary>
  public partial class CtlPrjButtons : UserControl
  {
    ViewModel _model;
    MainWindow _parent;
    bool _isPrjView = false;
    bool _isListView = false;

    public CtlPrjButtons()
    {
      InitializeComponent();
    }

    public void setup(ViewModel model, MainWindow parent, bool isPrjView, bool isListView)
    {
      _model = model;
      _parent = parent;
      _isPrjView = isPrjView;
      _isListView = isListView;
      if(_isPrjView)
      {
        _btnAll.Visibility = Visibility.Visible;
        _btnNone.Visibility = Visibility.Visible;
        _btnDelSelected.Visibility = Visibility.Visible;
        _btnCopySpec.Visibility = Visibility.Visible;
        _btnaddFile.Visibility = Visibility.Visible;
        _btnCallInfo.Visibility = Visibility.Visible;
        _btnSize.Visibility = Visibility.Visible;
        _sep2.Visibility = Visibility.Visible;
        _sep3.Visibility = Visibility.Visible;
      }
      else
      {
        _btnAll.Visibility = Visibility.Collapsed;
        _btnNone.Visibility = Visibility.Collapsed;
        _btnDelSelected.Visibility = Visibility.Collapsed;
        _btnCopySpec.Visibility = Visibility.Collapsed;
        _btnaddFile.Visibility = Visibility.Collapsed;
        _btnCallInfo.Visibility = Visibility.Collapsed;
        _btnSize.Visibility = Visibility.Collapsed;
        _sep2.Visibility = Visibility.Collapsed;
        _sep3.Visibility = Visibility.Collapsed;
      }
    }


    public void initFileButton(bool isQuery)
    {
      if (isQuery)
      {
        _btnaddFile.Content = MyResources.MainBtnExportSel;
        _btnaddFile.ToolTip = MyResources.MainToolExportSel;
      }
      else
      {
        _btnaddFile.Content = MyResources.MainBtnAddFile;
        _btnaddFile.ToolTip = MyResources.MainToolAddFile;
      }
    }

    private void _btnAll_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        BatExplorerProjectFileRecordsRecord[] recList = _model.CurrentlyOpen?.getRecords();
        if (recList != null)
        {
          foreach (BatExplorerProjectFileRecordsRecord rec in recList)
          {
            rec.Selected = true;
          }

          _parent.updateControls();
          DebugLog.log("select all files", enLogType.DEBUG);
        }
      }
      catch (Exception ex)
      {
        DebugLog.log("MainWin:BTN Select all failed: " + ex.ToString(), enLogType.ERROR);
      }
    }

    private void _btnNone_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        BatExplorerProjectFileRecordsRecord[] recList = _model.CurrentlyOpen?.getRecords();
        if (recList != null)
        {
          foreach (BatExplorerProjectFileRecordsRecord rec in recList)
          {
            rec.Selected = false;
          }
          _parent.updateControls();
          DebugLog.log("deselect all files", enLogType.DEBUG);
        }
      }
      catch (Exception ex)
      {
        DebugLog.log("MainWin:BTN Deselect all failed: " + ex.ToString(), enLogType.ERROR);
      }
    }


    private void _btnDelSelected_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        List<string> files = new List<string>();
        MessageBoxResult res = MessageBox.Show(MyResources.msgDeleteFiles, MyResources.msgQuestion, MessageBoxButton.YesNo, MessageBoxImage.Question);
        if ((res == MessageBoxResult.Yes) && (_model.CurrentlyOpen != null))
        {
          foreach (BatExplorerProjectFileRecordsRecord rec in _model.CurrentlyOpen.getRecords())
          {
            if (rec.Selected == true)
              files.Add(rec.File);
          }

          _model.deleteFiles(files);
          _parent.buildWavFileList(false);
          _parent.showStatus();
          DebugLog.log("MainWin:BTN 'dletete all files' clicked", enLogType.DEBUG);
        }
      }
      catch (Exception ex)
      {
        DebugLog.log("MainWin:BTN 'dletete all files' failed: " + ex.ToString(), enLogType.ERROR);
      }
    }

    private void _btnHideUnSelected_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        _parent.buildWavFileList(true);
        _parent.showStatus();
        DebugLog.log("MainWin:BTN 'hide unselected files' clicked", enLogType.DEBUG);
      }
      catch (Exception ex)
      {
        DebugLog.log("MainWin:BTN 'hide unselected files' failed: " + ex.ToString(), enLogType.ERROR);
      }
    }


    private void _btnSize_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        if (_isPrjView)
          _parent.togglePngSize();
        DebugLog.log("MainWin:BTN 'Size' clicked ", enLogType.DEBUG);
      }
      catch (Exception ex)
      {
        DebugLog.log("MainWin:BTN 'Size' failed: " + ex.ToString(), enLogType.ERROR);
      }
    }

    private void _btnaddFile_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        if ((_model.Prj != null) && _model.Prj.Ok)
        {
          System.Windows.Forms.OpenFileDialog ofi = new System.Windows.Forms.OpenFileDialog();
          ofi.Filter = "*.wav|*.wav";
          ofi.Title = BatInspector.Properties.MyResources.AddWAVFileSToProject;
          ofi.Multiselect = true;
          System.Windows.Forms.DialogResult ok = ofi.ShowDialog();
          if (ok == System.Windows.Forms.DialogResult.OK)
          {
            _model.Prj.addFiles(ofi.FileNames);
            if (_model.Prj.Analysis != null)
              _model.Prj.Analysis.save(_model.Prj.ReportName, _model.Prj.Notes);
            _model.Prj.writePrjFile();
            _parent._spSpectrums.Children.Clear();
            DirectoryInfo dir = new DirectoryInfo(_model.SelectedDir);
            _parent.initializeProject(dir);
          }
        }
        else if (_model.Query != null)
        {
          System.Windows.Forms.FolderBrowserDialog ofo = new System.Windows.Forms.FolderBrowserDialog();
          ofo.Description = MyResources.SelectExportDirectory;
          System.Windows.Forms.DialogResult res = ofo.ShowDialog();
          if (res == System.Windows.Forms.DialogResult.OK)
            _model.Query.exportFiles(ofo.SelectedPath);
        }
        else
          MessageBox.Show(BatInspector.Properties.MyResources.OpenProjectFirst, MyResources.msgInformation, MessageBoxButton.OK, MessageBoxImage.Error);
        DebugLog.log("MainWin:BTN 'Add file' clicked", enLogType.DEBUG);
      }
      catch (Exception ex)
      {
        DebugLog.log("MainWin:BTN 'Add file' failed: " + ex.ToString(), enLogType.ERROR);
      }
    }

    private void _cbFilter_DropDownOpened(object sender, EventArgs e)
    {
      _model.Filter.TempFilter = null;
      _cbFilter.Items[1] = MyResources.MainFilterNew;
    }

    private void _cbFilter_DropDownClosed(object sender, EventArgs e)
    {
      DebugLog.log("Main: Filter dropdown closed", enLogType.DEBUG);
      bool apply;
      bool resetFilter;
      CtlScatter.handleFilterDropdown(out apply, out resetFilter, _model, _cbFilter);
      if (apply)
        _btnApplyFilter_Click(sender, null);
      if (resetFilter)
        _btnShowAll_Click(sender, null);
    }

    private void _btnShowAll_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        //       foreach (ctlWavFile ctl in _spSpectrums.Children)
        //         ctl.Visibility = Visibility.Visible;
        _parent.buildWavFileList(false);
        _parent.showStatus();
        DebugLog.log("MainWin:BTN 'show all' clicked", enLogType.DEBUG);
      }
      catch (Exception ex)
      {
        DebugLog.log("MainWin:BTN 'show all' failed: " + ex.ToString(), enLogType.ERROR);
      }
    }

    private void _btnApplyFilter_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        FilterItem filter = (_cbFilter.SelectedIndex == 1) ?
                         filter = _model.Filter.TempFilter : filter = _model.Filter.getFilter(_cbFilter.Text);
        if ((filter != null) && (_model.CurrentlyOpen != null))
        {
          foreach (AnalysisFile a in _model.CurrentlyOpen.Analysis.Files)
          {
            bool res = _model.Filter.apply(filter, a);
            BatExplorerProjectFileRecordsRecord rec = _model.CurrentlyOpen.findRecord(a.Name);
            if (rec != null)
              rec.Selected = res;
          }
          _parent.buildWavFileList(true, _model.Filter, filter);
          _parent.showStatus();
          DebugLog.log("filter '" + filter.Name + "'  [" + filter.Expression + "] applied", enLogType.INFO);
        }
        else
          DebugLog.log("no filter applied", enLogType.INFO);
      }
      catch (Exception ex)
      {
        DebugLog.log("MainWin:BTN 'Apply Filter' failed:" + ex.ToString(), enLogType.ERROR);
      }
    }


    private void _btnCallInfo_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        _parent.toggleCallInfo();
      }
      catch (Exception ex)
      {
        DebugLog.log("MainWin:BTN 'Call Info' failed:" + ex.ToString(), enLogType.ERROR);
      }
    }

    private void _btnCopySpec_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        if ((AppParams.Inst.ScriptCopyAutoToMan != null) && (AppParams.Inst.ScriptCopyAutoToMan != ""))
          _model.Scripter.runScript(AppParams.Inst.ScriptCopyAutoToMan);
        else
          MessageBox.Show(MyResources.MsgSpecifyScript, "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        DebugLog.log("MainWin:BTN 'Copy Species' clicked", enLogType.DEBUG);
      }
      catch (Exception ex)
      {
        DebugLog.log("MainWin:BTN 'Copy Species' failed:" + ex.ToString(), enLogType.ERROR);
      }
    }


  }
}
