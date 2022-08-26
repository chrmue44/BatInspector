/********************************************************************************
 *               Author: Christian Müller
 *      Date of cration: 2021-08-10                                       
 *   Copyright (C) 2022: christian Müller christian(at)chrmue(dot).de
 *
 *              Licence:
 * 
 * THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND,
 * EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
 ********************************************************************************/

using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Deployment.Application;
using System.Reflection;
using BatInspector.Controls;
using System.Windows.Input;
using System;
using libParser;
using System.Threading;
using BatInspector.Properties;

namespace BatInspector.Forms
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    const int MAX_IMG_HEIGHT = 256;
    const int MAX_IMG_WIDTH = 512;
    int _cbFocus = -1;

    ViewModel _model;
    FrmLog _log = null;
    FrmFilter _frmFilter = null;
    int _imgHeight = MAX_IMG_HEIGHT;

    FrmZoom _frmZoom = null;
    CtrlZoom _ctlZoom = null;
    TabItem _tbZoom = null;
    frmSpeciesData _frmSpecies = null;

    System.Windows.Threading.DispatcherTimer _dispatcherTimer;


    public MainWindow()
    {
      System.Version version;
      try
      {
        version = ApplicationDeployment.CurrentDeployment.CurrentVersion;
      }
      catch
      {
        version = Assembly.GetExecutingAssembly().GetName().Version;
      }


      _model = new ViewModel(this, version.ToString());
      _model.loadSettings();
      _log = new FrmLog();

      setLanguage();
      InitializeComponent();
      initTreeView();
      populateFilterComboBoxes();
      initZoomWindow();
      this.Title = "BatInspector V" + version.ToString();
      if ((_model.Settings.MainWindowWidth > 100) && (_model.Settings.MainWindowHeight > 100))
      {
        this.Width = _model.Settings.MainWindowWidth;
        this.Height = _model.Settings.MainWindowHeight;
        _grdMain.RowDefinitions[3].Height = new GridLength(_model.Settings.LogControlHeight);
        _grdCtrl.ColumnDefinitions[0].Width = new GridLength(_model.Settings.WidthFileSelector);
      }
      this.Top = _model.Settings.MainWindowPosX;
      this.Left = _model.Settings.MainWindowPosY;
      _dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
      _dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
      _dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
      _dispatcherTimer.Start();
      DebugLog.setLogDelegate(_ctlLog.log);
      _ctlLog.setViewModel(_model);
#if DEBUG
      Tests tests = new Tests(_model);
      tests.exec();      
#endif
    }


    public void initTreeView()
    {
      trvStructure.Items.Clear();
      DriveInfo[] drives = DriveInfo.GetDrives();
      foreach (DriveInfo driveInfo in drives)
      {
        DirectoryInfo dir = new DirectoryInfo(driveInfo.Name);
        trvStructure.Items.Add(CreateTreeItem(dir));
      }
      if (_model.Settings.RootDataDir != null)
      {
        DirectoryInfo batDataDir = new DirectoryInfo(_model.Settings.RootDataDir);
        trvStructure.Items.Add(CreateTreeItem(batDataDir));
      }
    }

    public void TreeViewItem_Expanded(object sender, RoutedEventArgs e)
    {
      TreeViewItem item = e.Source as TreeViewItem;
      if ((item.Items.Count >= 1) /*&& (item.Items[0] is string)*/)
      {
        item.Items.Clear();

        DirectoryInfo expandedDir = null;
        if (item.Tag is DriveInfo)
          expandedDir = (item.Tag as DriveInfo).RootDirectory;
        if (item.Tag is DirectoryInfo)
          expandedDir = (item.Tag as DirectoryInfo);
        try
        {
          _model.Busy = true;
          DebugLog.log("start evaluation TODO", enLogType.DEBUG);
          foreach (DirectoryInfo subDir in expandedDir.GetDirectories())
          {
            TreeViewItem childItem = CreateTreeItem(subDir);
            item.Items.Add(childItem);
            if (Project.containsProject(subDir))
            {
              childItem.FontWeight = FontWeights.Bold;
              if (Project.evaluationDone(subDir))
                childItem.Foreground = new SolidColorBrush(Colors.Green);
              else
                childItem.Foreground = new SolidColorBrush(Colors.Violet);
            }
            else if (Project.containsWavs(subDir))
              childItem.Foreground = new SolidColorBrush(Colors.Blue);
          }
          DebugLog.log("evaluation of dir '" + expandedDir.Name + "' for TODOs finished", enLogType.INFO);
          _model.Busy = false;
        }
        catch
        {
          _model.Busy = false;
        }
      }
    }

    private void setLanguage()
    {
      string culture = _model.Settings.Culture.ToString().Replace('_', '-');
      Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(culture);
      Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(culture);
    }

    private void trvStructure_Collapsed(object sender, RoutedEventArgs e)
    {
      TreeViewItem item = e.Source as TreeViewItem;

    }

    public void TreeViewItem_Selected(object sender, RoutedEventArgs e)
    {
      TreeViewItem item = e.Source as TreeViewItem;
      DirectoryInfo dir = item.Tag as DirectoryInfo;
      {
        DebugLog.log("start to open project", enLogType.DEBUG);
        checkSavePrj();
        _model.initProject(dir);
        _lblProject.Text = dir.FullName;
        setStatus("   loading...");
        populateFiles();
      }
    }

    public void setStatus(string status)
    {
      _lblStatus.Text = status;
    }

    public void populateFilterComboBoxes()
    {
      populateFilterComboBox(_cbFilter);
      populateFilterComboBox(_ctlSum._cbFilter);

    }

    public void populateFilterComboBox(ComboBox fiBox)
    {
      fiBox.Items.Clear();
      fiBox.Items.Add(MyResources.MainFilterNone);
      foreach (FilterItem f in _model.Filter.Items)
      {
        string name = f.Name;
        fiBox.Items.Add(name);
      }
      if (fiBox.Items.Count > 0)
        fiBox.Text = (string)fiBox.Items[0];
    }

    public void closeWindow(enWinType w)
    {
      switch(w)
      {
        case enWinType.ZOOM:
          _frmZoom = null;
          break;
        case enWinType.BAT:
          _frmSpecies = null;
          break;
      }
    }

    private void initZoomWindow()
    {
      if (_model.Settings.ZoomSeparateWin)
      {
        _frmZoom = new FrmZoom(_model, closeWindow);
        _frmZoom.Width = _model.Settings.ZoomWindowWidth;
        _frmZoom.Height = _model.Settings.ZoomWindowHeight;
      }
      else
      {
        _tbZoom = new TabItem();
        _tbZoom.Header = "Zoom";
        _tbMain.Items.Add(_tbZoom);
        _ctlZoom = new CtrlZoom();
        _tbZoom.Content = _ctlZoom;
        _tbMain.SelectedIndex = 0;
      }
    }

    public void setZoom(string name, AnalysisFile analysis, string wavFilePath, System.Windows.Media.ImageSource img)
    {
      if(_model.Settings.ZoomSeparateWin)
      {
        if (_frmZoom == null)
          _frmZoom = new FrmZoom(_model,closeWindow);
        _frmZoom.setup(name, analysis, wavFilePath, img);
        setZoomPosition();
        _frmZoom.Show();
      }
      else
      {
        _ctlZoom.setup(analysis, wavFilePath, _model, img);
        _tbZoom.Header = "Zoom: " + name;
        _tbMain.SelectedItem = _tbZoom;
      }
    }

    /// <summary>
    /// check if project should be saved if changed
    /// </summary>
    void checkSavePrj()
    {
      if (_model.Prj != null)
      {
        if (_model.Analysis.Changed)
        {
          MessageBoxResult res = MessageBox.Show(MyResources.msgSaveBeforeClose, MyResources.msgQuestion, MessageBoxButton.YesNo, MessageBoxImage.Question);
          if (res == MessageBoxResult.Yes)
            _model.Analysis.save(_model.PrjPath + "report.csv");
        }
      }
    }

private void setZoomPosition()
    {
      if (_frmZoom != null)
      {
        _frmZoom.Top = this.Top;
        _frmZoom.Left = this.Left + this.Width - 10;
      }
    }

    private TreeViewItem CreateTreeItem(object o)
    {
      TreeViewItem item = new TreeViewItem();
      item.Header = o.ToString();
      item.Tag = o;
      item.Items.Add("Loading...");
      return item;
    }


    private void updateWavControls()
    {
      foreach (ctlWavFile ctl in _spSpectrums.Children)
      {
        if (ctl.Analysis != null)
        {
          string name = ctl.Analysis.FileName;
          AnalysisFile anaF = _model.Analysis.find(name);
          if (anaF != null)
            ctl.setFileInformations(ctl.Analysis.FileName, anaF, ctl.WavFilePath);
        }
      }
    }

    private void populateFiles()
    {
      _spSpectrums.Children.Clear();

      BackgroundWorker worker = new BackgroundWorker();
      worker.WorkerReportsProgress = true;
      worker.DoWork += createImageFiles;
      worker.ProgressChanged += worker_ProgressChanged;
      worker.RunWorkerCompleted += worker_RunWorkerCompleted;
      worker.RunWorkerAsync(10000);
    }

    void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
    {
      if (e.UserState != null)
        this.setStatus("generating PNG image " + e.UserState);

    }

    void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
    {
      populateControls();
    }

    private async void populateControls()
    {
      await createFftImages();
    }

    private void createImageFiles(object sender, DoWorkEventArgs e)
    {
      if ((_model.Prj!= null) && ( _model.Prj.Ok))
      {
        _model.Busy = true;
        Parallel.ForEach(_model.Prj.Records, rec =>
        //        foreach (BatExplorerProjectFileRecordsRecord rec in _model.Prj.Records)
        {
          bool newImage;
          _model.getFtImage(rec, out newImage);
          if (newImage)
          {
            (sender as BackgroundWorker).ReportProgress(55, rec.Name);
          }
        });
        _model.Busy = false;
      }
    }

    internal async Task createFftImages()
    {
      int index = 0;

      if ((_model.Prj != null) && (_model.Prj.Ok))
      {
        _spSpectrums.Children.Clear();
        _cbFocus = -1;
        foreach (BatExplorerProjectFileRecordsRecord rec in _model.Prj.Records)
        {
          ctlWavFile ctl = new ctlWavFile(index++, setFocus, _model, this);
          DockPanel.SetDock(ctl, Dock.Bottom);
          bool newImage;
          ctl._img.Source = _model.getFtImage(rec, out newImage);
          ctl._img.MaxHeight = _imgHeight;
          ctl.setFileInformations(rec.File, _model.Analysis.getAnalysis(rec.File), _model.WavFilePath);
          ctl.InfoVisible = !_model.Settings.HideInfos;
          _spSpectrums.Dispatcher.Invoke(() =>
          {
            _cbFocus = 0;
            _spSpectrums.Children.Add(ctl);
          });
         await Task.Delay(1);
        }
        DebugLog.log("project opened", enLogType.INFO);
        showStatus();
      }
    }

    void showStatus()
    {
      string report = _model.Analysis.Report != null ? "report available" : "no report";
      int vis = 0;
      foreach(ctlWavFile c in _spSpectrums.Children)
      {
        if (c.Visibility == Visibility.Visible)
          vis++;
      }
      setStatus("  [ nr of files: " + vis.ToString() + "/" + _spSpectrums.Children.Count.ToString() +" | " + report + " ]");
    }

    private void _btnAll_Click(object sender, RoutedEventArgs e)
    {
      foreach (UIElement it in _spSpectrums.Children)
      {
        ctlWavFile ctl = it as ctlWavFile;
        ctl._cbSel.IsChecked = true;
      }
      DebugLog.log("select all files", enLogType.INFO);
    }

    private void _btnNone_Click(object sender, RoutedEventArgs e)
    {
      foreach (UIElement it in _spSpectrums.Children)
      {
        ctlWavFile ctl = it as ctlWavFile;
        ctl._cbSel.IsChecked = false;
      }
      DebugLog.log("deselect all files", enLogType.INFO);
    }

    private void reIndexSpectrumControls()
    {
      int index = 0;
      foreach (UIElement it in _spSpectrums.Children)
      {
        ctlWavFile ctl = it as ctlWavFile;
        ctl.Index = index++;
      }
    }

    private void _btnDelSelected_Click(object sender, RoutedEventArgs e)
    {
      List<UIElement> list = new List<UIElement>();
      List<string> files = new List<string>();
      MessageBoxResult res = MessageBox.Show(MyResources.msgDeleteFiles, MyResources.msgQuestion, MessageBoxButton.YesNo, MessageBoxImage.Question);
      if (res == MessageBoxResult.Yes)
      {

        foreach (UIElement it in _spSpectrums.Children)
        {
          ctlWavFile ctl = it as ctlWavFile;
          if (ctl._cbSel.IsChecked == true)
          {
            files.Add(ctl.WavName.ToString());
            list.Add(it);
          }
        }

        _model.deleteFiles(files);
        foreach (UIElement it in list)
          _spSpectrums.Children.Remove(it);

        reIndexSpectrumControls();
        showStatus();
      }
    }

    private void _btnHideUnSelected_Click(object sender, RoutedEventArgs e)
    {
      foreach (ctlWavFile ctl in _spSpectrums.Children)
      
        ctl.Visibility = ctl._cbSel.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
        DebugLog.log("hide unselected files", enLogType.INFO);
      showStatus();
    }

    private void _btnShowAll_Click(object sender, RoutedEventArgs e)
    {
      foreach (ctlWavFile ctl in _spSpectrums.Children)
        ctl.Visibility = Visibility.Visible;


      DebugLog.log("show all files", enLogType.INFO);
      showStatus();
    }

    public void setFocus(int index)
    {
      if ((index >= 0) && (index < _spSpectrums.Children.Count))
      {
        _cbFocus = index;
      }
    }

    private void startPrediction(int options)
    {
      Thread.Sleep(100);
      if (_model.startEvaluation(options) == 0)
      {
        _model.updateReport();
        updateWavControls();
      }
    }

    private void _btnFindCalls_Click(object sender, RoutedEventArgs e)
    {
      frmStartPredict.showMsg(startPrediction, _model.Settings);
    }

  
    private void _btnSize_Click(object sender, RoutedEventArgs e)
    {
      _imgHeight >>= 1;
      if (_imgHeight < 32)
        _imgHeight = MAX_IMG_HEIGHT;
      foreach (UIElement ui in _spSpectrums.Children)
      {
        ctlWavFile ctl = ui as ctlWavFile;
        ctl._img.MaxHeight = _imgHeight;
        ctl._img.Height = _imgHeight;
      }
    }

    private void Window_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
    {
      _model.KeyPressed = System.Windows.Input.Key.None;
      _model.LastKey = e.Key;
      if (e.Key == System.Windows.Input.Key.Return)
      {
        if ((_cbFocus >= 0) && (_cbFocus < _spSpectrums.Children.Count))
        {
          ctlWavFile ctl = (ctlWavFile)_spSpectrums.Children[_cbFocus];
          ctl.toggleCheckBox();
        }
      }
    }

    private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
      _model.KeyPressed = e.Key;
    }

    private void _tbReport_GotFocus(object sender, RoutedEventArgs e)
    {
      if (_model.Analysis.Report != null)
        _dgData.ItemsSource = _model.Analysis.Report;

    }

    private void Window_Closing(object sender, CancelEventArgs e)
    {
      checkSavePrj();      
      if (_log != null)
        _log.Close();
      if (_frmZoom != null)
        _frmZoom.Close();
      if (_frmSpecies != null)
        _frmSpecies.Close();
    }

    private void _btnFilter_Click(object sender, RoutedEventArgs e)
    {
      _frmFilter = new FrmFilter(_model.Filter, populateFilterComboBoxes);
      _frmFilter.Show();
    }

    private void _btnApplyFilter_Click(object sender, RoutedEventArgs e)
    {
      FilterItem filter = _model.Filter.getFilter(_cbFilter.Text);
      if (filter != null)
      {
        foreach (ctlWavFile c in _spSpectrums.Children)
        {
          bool res = _model.Filter.apply(filter, c.Analysis);
          c._cbSel.IsChecked = res;
        }
        DebugLog.log("filter '" + filter.Name + "' applied", enLogType.INFO);
      }
      else
        DebugLog.log("no filter applied", enLogType.INFO);

    }

    private void _btnSave_Click(object sender, RoutedEventArgs e)
    {
      _model.saveSettings();
      _model.Analysis.save(_model.PrjPath + "report.csv");
      _model.Prj.writePrjFile();
      DebugLog.log("project '" + _model.Prj.Name + "' saved", enLogType.INFO);
    }

    private void _btnHelp_Click(object sender, RoutedEventArgs e)
    {

    }

    private void _btnInfo_Click(object sender, RoutedEventArgs e)
    {
      FrmAbout frm = new FrmAbout(_model.Version);
      frm.Show();
    }

    private void _btnCallInfo_Click(object sender, RoutedEventArgs e)
    {
      if (_spSpectrums.Children.Count > 0)
      {
        ctlWavFile ctl0 = _spSpectrums.Children[0] as ctlWavFile;
        _model.Settings.HideInfos = !ctl0.InfoVisible;
      }
      
      foreach (ctlWavFile ctl in _spSpectrums.Children)
        ctl.InfoVisible = !ctl.InfoVisible;
    }

    private void _btnDel_Click(object sender, RoutedEventArgs e)
    {
      FrmColorMap frm = new FrmColorMap(_model);
      frm.Show();
    }

    private void _btnSpecies_Click(object sender, RoutedEventArgs e)
    {
      if(_frmSpecies == null)
        _frmSpecies = new frmSpeciesData(_model, closeWindow, this);
      _frmSpecies.Show();
    }

    private void _btnSettings_Click(object sender, RoutedEventArgs e)
    {
      frmSettings frm = new frmSettings(_model.Settings);
      frm.Show();
    }

    private void _btnWavTool_Click(object sender, RoutedEventArgs e)
    {
      frmWavFile frm = new frmWavFile(_model);
      frm.Show();
    }

    private void Content_rendered(object sender, System.EventArgs e)
    {
      if ((_ctlZoom != null) && (_model.PrjPath != null))
      {
        _ctlZoom.update();
        _tbMain.SelectedIndex = 2;
      }
    }

    private void Window_LocationChanged(object sender, System.EventArgs e)
    {
      setZoomPosition();
      _model.Settings.MainWindowPosX = this.Top;
      _model.Settings.MainWindowPosY = this.Left;

    }

    private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
    {
      setZoomPosition ();
      _model.Settings.MainWindowWidth = this.Width;
      _model.Settings.MainWindowHeight = this.Height;
    }

    private void dispatcherTimer_Tick(object sender, EventArgs e)
    {
      Application.Current.Dispatcher.Invoke(() =>
      {
        Mouse.OverrideCursor = _model.Busy ? Cursors.Wait : null;
      });
    }

    private void _btnReport_Click(object sender, RoutedEventArgs e)
    {

    }

    private void _btnFindSpecies_Click(object sender, RoutedEventArgs e)
    {
      frmFindBat frm = new frmFindBat(_model);
      frm.Show();
    }

    private void _btnCopySpec_Click(object sender, RoutedEventArgs e)
    {
      foreach(ctlWavFile ctl in _spSpectrums.Children)
      {
        if(ctl._cbSel.IsChecked == true)
        {
          ctl._btnCopy_Click(null, null);
        }
      }
    }

    private void _grdSplitterH_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
    {
      _model.Settings.LogControlHeight = _grdMain.RowDefinitions[3].Height.Value;
    }

    private void _grdSplitterV_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
    {
      _model.Settings.WidthFileSelector = _grdCtrl.ColumnDefinitions[0].Width.Value;
    }

    private void _btnScript_Click(object sender, RoutedEventArgs e)
    {
      frmRunScript frmScript = new frmRunScript(_model);
      frmScript.Show();
    }

    private void _btnCancelScript_Click(object sender, RoutedEventArgs e)
    {

    }
  }

  public enum enWinType 
  {
    ZOOM,
    BAT
  }

  public delegate void dlgSetFocus(int index);
  public delegate void dlgcloseChildWindow(enWinType w);
}
