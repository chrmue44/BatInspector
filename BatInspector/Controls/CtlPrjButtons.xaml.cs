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
    bool _isPrjView = false;
    bool _isListView = false;

    public CtlPrjButtons()
    {
      InitializeComponent();
    }

    public void setup(bool isPrjView, bool isListView)
    {
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
        _btnaddFile.Visibility = Visibility.Collapsed;
      else
        _btnaddFile.Visibility = Visibility.Visible;
    }

    private void _btnAll_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        PrjRecord[] recList = App.Model.CurrentlyOpen?.getRecords();
        if (recList != null)
        {
          foreach (PrjRecord rec in recList)
          {
            rec.Selected = true;
          }

          App.MainWin.updateControls();
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
        PrjRecord[] recList = App.Model.CurrentlyOpen?.getRecords();
        if (recList != null)
        {
          foreach (PrjRecord rec in recList)
          {
            rec.Selected = false;
          }
          App.MainWin.updateControls();
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
        if ((res == MessageBoxResult.Yes) && (App.Model.CurrentlyOpen != null))
        {
          foreach (PrjRecord rec in App.Model.CurrentlyOpen.getRecords())
          {
            if (rec.Selected == true)
              files.Add(rec.File);
          }

          App.Model.deleteFiles(files);
          App.MainWin.buildWavFileList(false);
          App.MainWin.showStatus();
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
        App.MainWin.buildWavFileList(true);
        App.MainWin.showStatus();
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
          App.MainWin.togglePngSize();
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
        if ((App.Model.Prj != null) && App.Model.Prj.Ok)
        {
          System.Windows.Forms.OpenFileDialog ofi = new System.Windows.Forms.OpenFileDialog();
          ofi.Filter = "*.wav|*.wav";
          ofi.Title = BatInspector.Properties.MyResources.AddWAVFileSToProject;
          ofi.Multiselect = true;
          System.Windows.Forms.DialogResult ok = ofi.ShowDialog();
          if (ok == System.Windows.Forms.DialogResult.OK)
          {
            App.Model.Prj.addFiles(ofi.FileNames);
            if (App.Model.Prj.Analysis != null)
              App.Model.Prj.Analysis.save(App.Model.Prj.ReportName, App.Model.Prj.Notes, App.Model.Prj.SummaryName);
            App.Model.Prj.writePrjFile();
            App.MainWin._spSpectrums.Children.Clear();
            DirectoryInfo dir = new DirectoryInfo(App.Model.SelectedDir);
            App.MainWin.initializeProject(dir);
          }
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
      App.Model.Filter.TempFilter = null;
      _cbFilter.Items[1] = MyResources.MainFilterNew;
    }

    private void _cbFilter_DropDownClosed(object sender, EventArgs e)
    {
      DebugLog.log("Main: Filter dropdown closed", enLogType.DEBUG);
      bool apply;
      bool resetFilter;
      CtlScatter.handleFilterDropdown(out apply, out resetFilter, _cbFilter);
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
        App.MainWin.buildWavFileList(false);
        App.MainWin.showStatus();
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
        _btnNone_Click(sender, null);
        FilterItem filter = (_cbFilter.SelectedIndex == 1) ?
                          App.Model.Filter.TempFilter :  App.Model.Filter.getFilter(_cbFilter.Text);
        if ((filter != null) && (App.Model.CurrentlyOpen != null))
        {
          foreach (AnalysisFile a in App.Model.CurrentlyOpen.Analysis.Files)
          {
            bool res = App.Model.Filter.apply(filter, a);
            PrjRecord rec = App.Model.CurrentlyOpen.findRecord(a.Name);
            if (res && (rec != null))
              rec.Selected = res;
          }
          App.MainWin.buildWavFileList(true, App.Model.Filter, filter);
          App.MainWin.showStatus();
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
        App.MainWin.toggleCallInfo();
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
          App.Model.Scripter.runScript(AppParams.Inst.ScriptCopyAutoToMan);
        else
          MessageBox.Show(MyResources.MsgSpecifyScript, "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        DebugLog.log("MainWin:BTN 'Copy Species' clicked", enLogType.DEBUG);
      }
      catch (Exception ex)
      {
        DebugLog.log("MainWin:BTN 'Copy Species' failed:" + ex.ToString(), enLogType.ERROR);
      }
    }

    private void _btnExport_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        if (App.Model.Prj?.Ok ==  true)
        {
          System.Windows.Forms.FolderBrowserDialog ofo = new System.Windows.Forms.FolderBrowserDialog();
          ofo.Description = MyResources.SelectExportDirectory;
          System.Windows.Forms.DialogResult res = ofo.ShowDialog();
          if (res == System.Windows.Forms.DialogResult.OK)
            App.Model.Prj.exportFiles(ofo.SelectedPath);
        }
        else if (App.Model.Query != null)
        {
          System.Windows.Forms.FolderBrowserDialog ofo = new System.Windows.Forms.FolderBrowserDialog();
          ofo.Description = MyResources.SelectExportDirectory;
          System.Windows.Forms.DialogResult res = ofo.ShowDialog();
          if (res == System.Windows.Forms.DialogResult.OK)
            App.Model.Query.exportFiles(ofo.SelectedPath);
        }
        else
          MessageBox.Show(BatInspector.Properties.MyResources.OpenProjectFirst, MyResources.msgInformation, MessageBoxButton.OK, MessageBoxImage.Error);

        DebugLog.log("MainWin:BTN 'Export sel.' clicked", enLogType.DEBUG);
      }
      catch (Exception ex)
      {
        DebugLog.log("MainWin:BTN 'Export sel.' failed:" + ex.ToString(), enLogType.ERROR);
      }
    }
  }
}
