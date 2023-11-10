/********************************************************************************
 *               Author: Christian Müller
 *     Date of creation: 2021-08-10                                       
 *   Copyright (C) 2023: Christian Müller chrmue44(at)gmail(dot).com
 *
 *              Licence:  CC BY-NC 4.0 
 ********************************************************************************/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Interop;
using System.Windows.Input;
using System.Deployment.Application;
using System.Reflection;
using BatInspector.Controls;
using libParser;
using BatInspector.Properties;
using System.Windows.Threading;

namespace BatInspector.Forms
{

  delegate void dlgProgress(string pngName);

  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    const int MAX_IMG_HEIGHT = 256;
    const int MAX_IMG_WIDTH = 512;
    int _cbFocus = -1;

    ViewModel _model;
    FrmFilter _frmFilter = null;
    FrmScript _frmScript = null;
    FrmAbout _frmAbout = null;
    frmSettings _frmSettings = null;
    FrmCreatePrj _frmCreatePrj = null;
    frmCreateReport _frmCreateReport = null;
    frmWavFile _frmWavFile = null;
    FrmColorMap _frmColorMap = null;
    frmDebug _frmDebug = null;
    FrmQuery _frmQuery = null;
    frmCleanup _frmCleanup = null;
    FrmMessage _frmMsg = new FrmMessage();
    int _imgHeight = MAX_IMG_HEIGHT;
    FrmZoom _frmZoom = null;
    CtrlZoom _ctlZoom = null;
    TabItem _tbZoom = null;
    frmSpeciesData _frmSpecies = null;
    bool _fastOpen = true;
    Thread _workerPredict = null;
    Thread _workerStartup = null;
    System.Windows.Threading.DispatcherTimer _timer;
    bool _switchTabToPrj = false;

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
      DateTime linkTimeLocal = System.IO.File.GetLastWriteTime(Assembly.GetExecutingAssembly().Location);
      string versionStr = "BatInspector V" + version.ToString() + " " + linkTimeLocal.ToString();
      _model = new ViewModel(this, versionStr, callbackUpdateAnalysis);
      _model.Status.State = enAppState.IDLE;
      setLanguage();
      InitializeComponent();
      _ctlScatter.setup(_model);
      StateChanged += MainWindowStateChangeRaised;
      _ctlLog._cbErr.IsChecked = AppParams.Inst.LogShowError;
      _ctlLog._cbWarn.IsChecked = AppParams.Inst.LogShowWarning;
      _ctlLog._cbInfo.IsChecked = AppParams.Inst.LogShowInfo;
      _ctlLog._cbDebug.IsChecked = AppParams.Inst.LogShowDebug;
      DebugLog.setLogDelegate(_ctlLog.log, _ctlLog.clearLog, _ctlLog.checkMaxLogSize, AppParams.LogDataPath);
      initTreeView();
      populateFilterComboBoxes();
      initZoomWindow();
      this._windowTitle.Text = versionStr;
      if ((AppParams.Inst.MainWindowWidth > 100) && (AppParams.Inst.MainWindowHeight > 100))
      {
        this.Width = AppParams.Inst.MainWindowWidth;
        this.Height = AppParams.Inst.MainWindowHeight;
        _grdMain.RowDefinitions[3].Height = new GridLength(AppParams.Inst.LogControlHeight);
        _grdCtrl.ColumnDefinitions[0].Width = new GridLength(AppParams.Inst.WidthFileSelector);
      }
      this.Top = AppParams.Inst.MainWindowPosX;
      this.Left = AppParams.Inst.MainWindowPosY;
      _timer = new System.Windows.Threading.DispatcherTimer();
      _timer.Tick += new EventHandler(timer_Tick);
      _timer.Interval = new TimeSpan(0, 0, 0, 0, 300);
      _timer.Start();
      _ctlLog.setViewModel(_model);
      populateToolsMenu();
#if DEBUG
      Tests tests = new Tests(_model);
      tests.exec();
      _switchTabToPrj = true;
#endif
      DebugLog.log(versionStr + " started", enLogType.DEBUG);
      Installer.hideSplash();
#if !DEBUG
#endif
    }



    public void initTreeView()
    {
      trvStructure.Items.Clear();
      DriveInfo[] drives = DriveInfo.GetDrives();
      if (AppParams.Inst.ShowOnlyFilteredDirs)
      {
        foreach (string dir in AppParams.Inst.DirFilter)
        {
          if ((dir != null) && (dir.Length > 0))
          {
            DirectoryInfo batDataDir = new DirectoryInfo(dir);
            trvStructure.Items.Add(CreateTreeItem(batDataDir));
          }
        }
      }
      else
      {
        foreach (DriveInfo driveInfo in drives)
        {
          DirectoryInfo dir = new DirectoryInfo(driveInfo.Name);
          trvStructure.Items.Add(CreateTreeItem(dir));
        }
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
          if (expandedDir != null)
          {
            if (Project.containsProject(expandedDir) == "")
            {
              DebugLog.log("start evaluation TODO", enLogType.DEBUG);
              foreach (DirectoryInfo subDir in expandedDir.GetDirectories())
              {
                TreeViewItem childItem = CreateTreeItem(subDir);
                item.Items.Add(childItem);
                if (Project.containsProject(subDir) != "")
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

              foreach (FileInfo subFile in expandedDir.GetFiles())
              {
                if (Query.isQuery(subFile))
                {
                  TreeViewItem childItem = CreateTreeItem(subFile);
                  item.Items.Add(childItem);
                  childItem.FontWeight = FontWeights.Bold;
                  childItem.Foreground = new SolidColorBrush(Colors.Orange);
                }
              }
            }
            DebugLog.log("evaluation of dir '" + expandedDir.Name + "' for TODOs finished", enLogType.DEBUG);
          }
        }
        catch
        {
        }
      }
    }

    private void setLanguage()
    {
      string culture = AppParams.Inst.Culture.ToString().Replace('_', '-');
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
      if ((dir != null) && (Project.containsProject(dir) != ""))
        initializeProject(dir);

      FileInfo file = item.Tag as FileInfo;
      if ((file != null) && Query.isQuery(file))
        initializeQuery(file);
    }

    public void setStatus(string status)
    {
      _lblStatus.Text = status;
    }

    public void populateFilterComboBoxes()
    {
      Filter.populateFilterComboBox(_cbFilter, _model);
      _ctlScatter.populateComboBoxes();
    }


    public void closeWindow(enWinType w)
    {
      switch (w)
      {
        case enWinType.ZOOM:
          _frmZoom = null;
          break;
        case enWinType.BAT:
          _frmSpecies = null;
          break;
      }
    }

    void initializeQuery(FileInfo file)
    {
      DebugLog.log("start to open query", enLogType.DEBUG);
      _scrlViewer.ScrollToVerticalOffset(0);
      checkSavePrj();
      _model.initQuery(file);
      if (_frmQuery == null)
        _frmQuery = new FrmQuery(_model);
      _frmQuery.initFieldsFromQuery();

      _switchTabToPrj = true;
      if (_model.Query == null)              //remove all spectrograms if project was closed
        _spSpectrums.Children.Clear();
      _lblProject.Text = "QUERY: " + file.FullName;
      if (_model.Query != null)
      {
        if (_model.Query.Records.Length < AppParams.MAX_FILES_PRJ_OVERVIEW)
          populateFiles();
        else
        {
          DebugLog.log("too much files in project, could not open file views", enLogType.INFO);
          hideMsg();
        }
      }

    }

    /// <summary>
    /// callback in case of Analysis has changed (manual species)
    /// All connected GUI elements are updated
    /// </summary>
    /// <param name="fName">Name of the WAV file</param>
    void callbackUpdateAnalysis(string fName)
    {
      if (Dispatcher.CheckAccess()) // CheckAccess returns true if you're on the dispatcher thread
      {
        _tbReport_GotFocus(null, null);
        foreach (ctlWavFile ctl in _spSpectrums.Children)
        {
          if (ctl.Analysis?.getString(Cols.NAME) == fName)
          {
            ctl.updateCallInformations(ctl.Analysis);
            if (ctl.Analysis == _model.ZoomView.Analysis)
              _ctlZoom.updateManSpecies();
            break;
          }
        } 
      }
    }

    void initializeProject(DirectoryInfo dir)
    {
      showMsg(BatInspector.Properties.MyResources.msgInformation, BatInspector.Properties.MyResources.MainWindowMsgLoading, true);
      DebugLog.log("start to open project", enLogType.DEBUG);
      _scrlViewer.ScrollToVerticalOffset(0);
      checkSavePrj();
      _lblProject.Text = BatInspector.Properties.MyResources.MainWindowMsgLoading;
      setStatus("");
      _spSpectrums.Children.Clear();
      _tbReport_GotFocus(_spSpectrums, null);
      _model.initProject(dir, callbackUpdateAnalysis);
      if ((_model.Prj != null) && _model.Prj.Ok)
      {
        _ctlPrjInfo.setup(_model.Prj);
        _ctlScatter.initPrj();
        _switchTabToPrj = true;
        if (_model.Prj.Records.Length < AppParams.MAX_FILES_PRJ_OVERVIEW)
          populateFiles();
        else
          DebugLog.log("too much files in project, could not open file views", enLogType.INFO);
      }
    }

    private void initZoomWindow()
    {
      if (AppParams.Inst.ZoomSeparateWin)
      {
        _frmZoom = new FrmZoom(_model, closeWindow)
        {
          Width = AppParams.Inst.ZoomWindowWidth,
          Height = AppParams.Inst.ZoomWindowHeight
        };
        _ctlZoom = _frmZoom._ctl;
      }
      else
      {
        _tbZoom = new TabItem();
        _tbZoom.Header = "Zoom";
        _tbMain.Items.Add(_tbZoom);
        _ctlZoom = new CtrlZoom();
        _tbZoom.Content = _ctlZoom;
        _tbZoom.IsSelected = false;
        _tbZoom.Visibility = Visibility.Hidden;
      }
    }

    public void setZoom(string name, AnalysisFile analysis, string wavFilePath, ctlWavFile ctlWav)
    {
      DebugLog.log("activate zoom view of: " + name, enLogType.DEBUG);
      if (AppParams.Inst.ZoomSeparateWin)
      {
        if (_frmZoom == null)
          _frmZoom = new FrmZoom(_model, closeWindow);
        _frmZoom.setup(name, analysis, wavFilePath, ctlWav);
        setZoomPosition();
        _frmZoom.Show();
      }
      else
      {
        _ctlZoom.setup(analysis, wavFilePath, _model, _model.CurrentlyOpen.Species, ctlWav);
        _tbZoom.Header = "Zoom: " + name;
        _tbZoom.Visibility = Visibility.Visible;
        //      https://stackoverflow.com/questions/7929646/how-to-programmatically-select-a-tabitem-in-wpf-tabcontrol
        Dispatcher.BeginInvoke((Action)(() => _tbMain.SelectedItem = _tbZoom));
      }
    }

    public void changeCallInZoom(int call)
    {
      if (_ctlZoom != null)
        _ctlZoom.changeCall(call);
    }
    /// <summary>
    /// check if project should be saved if changed
    /// </summary>
    void checkSavePrj()
    {
      if (_model.Prj != null)
      {
        if (_model.Prj.Ok && _model.Prj.Analysis.Changed && (_model.Prj.Analysis.Files.Count > 0))
        {
          MessageBoxResult res = MessageBox.Show(MyResources.msgSaveBeforeClose, MyResources.msgQuestion, MessageBoxButton.YesNo, MessageBoxImage.Question);
          if (res == MessageBoxResult.Yes)
            _model.Prj.Analysis.save(_model.Prj.ReportName, _model.Prj.Notes);
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
      item.Items.Add(BatInspector.Properties.MyResources.MainWindowMsgLoading);
      item.Foreground = (SolidColorBrush)Application.Current.Resources["colorForeGroundLabel"];
      return item;
    }


    private void updateWavControls()
    {
      List<string> spec = new List<string>();
      foreach (SpeciesInfos si in _model.SpeciesInfos)
      {
        if (si.Show)
          spec.Add(si.Abbreviation);
      }
      spec.Add("todo");
      spec.Add("?");
      spec.Add("---");

      foreach (ctlWavFile ctl in _spSpectrums.Children)
      {
        AnalysisFile anaF = _model.Prj.Analysis.find(ctl.WavName);
        if (anaF != null)
          ctl.updateCallInformations(anaF);
      }
    }

    private void populateFiles()
    {
      _model.Busy = true;
      setMouseStatus();
      _workerStartup = new Thread(createImageFiles);
      _workerStartup.Priority = ThreadPriority.AboveNormal;
      _workerStartup.Start();
      _model.Status.State = enAppState.OPEN_PRJ;
    }

    void worker_ProgressChanged(string pngName)
    {
      if (!Dispatcher.CheckAccess()) // CheckAccess returns true if you're on the dispatcher thread
      {
        Dispatcher.BeginInvoke(new dlgProgress(worker_ProgressChanged));
      }
      this.setStatus(pngName);

    }

    private void populateControls()
    {
      createFftImages();
    }

    private void createImageFiles()
    {
      Stopwatch s = new Stopwatch();
      s.Start();
      BatExplorerProjectFileRecordsRecord[] recList = null;
      if ((_model.Prj != null) && (_model.Prj.Ok) || _model.Query!=null)
        recList = _model.Prj.Records;
      else if (_model.Query!=null)
        recList= _model.Query.Records;
      if(recList != null)
      { 
        _model.Status.Msg = BatInspector.Properties.MyResources.MainWindowMsgOpenPrj;
        for(int i = 0; i < _model.Prj.Records.Length; i++)
        {
          _model.createPngIfMissing(_model.Prj.Records[i], false);
          if(s.ElapsedMilliseconds > 2500)
          {
            _model.Status.Msg = i.ToString() + "/" + _model.Prj.Records.Length + " " + BatInspector.Properties.MyResources.MainWindowFilesProcessed;
            s.Restart();
            Thread.Yield();
          }
        }
      }
    }

    void initCtlWav(ctlWavFile ctl, BatExplorerProjectFileRecordsRecord rec, bool fromQuery)
    {
      ctl._img.Source = _model.getFtImage(rec, fromQuery);
      ctl._img.MaxHeight = _imgHeight;
      AnalysisFile analysis;
      List<string> species;
      string fullWavName;
      string wavName;
      string wavFilePath;
      if (fromQuery)
      {
        fullWavName = Path.Combine(_model.SelectedDir, rec.File);
        wavName = Path.GetFileName(fullWavName);
        wavFilePath = Path.GetDirectoryName(fullWavName);
        analysis = _model.Query.Analysis.find(rec.File);
        species = _model.Query.Species;
      }
      else
      {
        wavFilePath = Path.Combine(_model.SelectedDir, _model.Prj.WavSubDir);
        wavName = rec.File;
        fullWavName = Path.Combine(_model.SelectedDir, rec.File);
        analysis = _model.Prj.Analysis.find(rec.File);
        species = _model.Prj.Species;
      }

      ctl.setFileInformations(wavName, wavFilePath, analysis, species);
      ctl.InfoVisible = !AppParams.Inst.HideInfos;
      if (!_fastOpen)
        setStatus("loading [" + ctl.Index.ToString() + "/" + _model.Prj.Records.Length.ToString() + "]");
    }

    void createFftImages()
    {
      int index = 0;

      if ((_model.Prj != null) && (_model.Prj.Ok))
      {
        _spSpectrums.Children.Clear();
        _cbFocus = -1;

        foreach (BatExplorerProjectFileRecordsRecord rec in _model.Prj.Records)
        {
          ctlWavFile ctl = new ctlWavFile(index++, setFocus, _model, this, true);
          DockPanel.SetDock(ctl, Dock.Bottom);
          _spSpectrums.Dispatcher.BeginInvoke((Action)(() =>
          {
            _cbFocus = 0;
            _spSpectrums.Children.Add(ctl);
          }));

          if (!_fastOpen || (ctl.Index < 5))
          {
            initCtlWav(ctl, rec, false);
          }
        }
        DebugLog.log("project opened: " + _model.Prj.Name, enLogType.INFO);
        showStatus();
      }
      else if (_model.Query != null)
      {
        _spSpectrums.Children.Clear();
        _cbFocus = -1;

        foreach (BatExplorerProjectFileRecordsRecord rec in _model.Query.Records)
        {
          ctlWavFile ctl = new ctlWavFile(index++, setFocus, _model, this, false);
          DockPanel.SetDock(ctl, Dock.Bottom);
          _spSpectrums.Dispatcher.BeginInvoke((Action)(() =>
          {
            _cbFocus = 0;
            _spSpectrums.Children.Add(ctl);
          }));

          if (!_fastOpen || (ctl.Index < 5))
          {
            initCtlWav(ctl, rec, true);
          }
        }
        if (_model.Prj != null)
          DebugLog.log("Project opened: " + _model.Prj.Name, enLogType.INFO);
        else if (_model.Query != null)
          DebugLog.log("Query opened: " + _model.Query.Name, enLogType.INFO);
        showStatus();
      }
    }

    void showStatus()
    {
      string report = "";
      if (_model.CurrentlyOpen != null)
        report = _model.CurrentlyOpen.Analysis.Report != null ?
                 BatInspector.Properties.MyResources.MainWindowMsgReport :
                 BatInspector.Properties.MyResources.MainWindow_showStatus_NoReport;

      int vis = 0;
      foreach (ctlWavFile c in _spSpectrums.Children)
      {
        if (c.Visibility == Visibility.Visible)
          vis++;
      }
      setStatus("  [ "+ BatInspector.Properties.MyResources.MainWindowFiles + ": " + vis.ToString() + "/" + _spSpectrums.Children.Count.ToString() + " | " + report + " ]");
    }

    private void _btnAll_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        foreach (UIElement it in _spSpectrums.Children)
        {
          ctlWavFile ctl = it as ctlWavFile;
          ctl._cbSel.IsChecked = true;
          if (ctl.Analysis != null)
            ctl.Analysis.Selected = true;
        }
        DebugLog.log("select all files", enLogType.DEBUG);
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
        foreach (UIElement it in _spSpectrums.Children)
        {
          ctlWavFile ctl = it as ctlWavFile;
          ctl._cbSel.IsChecked = false;
          if (ctl.Analysis != null)
            ctl.Analysis.Selected = false;
        }
        DebugLog.log("deselect all files", enLogType.DEBUG);
      }
      catch (Exception ex)
      {
        DebugLog.log("MainWin:BTN Deselect all failed: " + ex.ToString(), enLogType.ERROR);
      }
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
      try
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
        foreach (ctlWavFile ctl in _spSpectrums.Children)
          ctl.Visibility = ctl._cbSel.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
        showStatus();
        DebugLog.log("MainWin:BTN 'hide unselected files' clicked", enLogType.DEBUG);
      }
      catch (Exception ex)
      {
        DebugLog.log("MainWin:BTN 'hide unselected files' failed: " + ex.ToString(), enLogType.ERROR);
      }
    }

    private void _btnShowAll_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        foreach (ctlWavFile ctl in _spSpectrums.Children)
          ctl.Visibility = Visibility.Visible;
        showStatus();
        DebugLog.log("MainWin:BTN 'show all' clicked", enLogType.DEBUG);
      }
      catch (Exception ex)
      {
        DebugLog.log("MainWin:BTN 'show all' failed: " + ex.ToString(), enLogType.ERROR);
      }

    }

    public void setFocus(int index)
    {
      if ((index >= 0) && (index < _spSpectrums.Children.Count))
      {
        _cbFocus = index;
      }
    }

    void showMsg(string title, string msg, bool topmost = false)
    {
      Application.Current.Dispatcher.BeginInvoke((Action)(() =>
      {
        _frmMsg.showMessage(title, msg, topmost);
        _frmMsg.Visibility = Visibility.Visible;
      }), DispatcherPriority.Send);
    }

    void hideMsg()
    {
      Application.Current.Dispatcher.BeginInvoke((Action)(() =>
      {
        _frmMsg.Visibility = Visibility.Hidden;
      }), DispatcherPriority.ContextIdle);
    }

    private void workerPrediction()
    {
      showMsg(BatInspector.Properties.MyResources.msgInformation, BatInspector.Properties.MyResources.MainWindowMsgClassification, true);
      _model.evaluate();
    }


    private void _btnFindCalls_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        MessageBoxResult res = MessageBoxResult.Yes;
        DebugLog.log("MainWin:BTN 'Find calls' clicked ", enLogType.DEBUG);
        if (_model.CurrentlyOpen?.Analysis.Report != null)
          res = MessageBox.Show(BatInspector.Properties.MyResources.MainWinMsgOverwriteReport,
          BatInspector.Properties.MyResources.msgQuestion, 
          MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (res == MessageBoxResult.Yes)
        {
          _model.Status.State = enAppState.AI_ANALYZE;
          _workerPredict = new Thread(new ThreadStart(workerPrediction));
          _workerPredict.Start();
        }
      }
      catch (Exception ex)
      {
        _model.Status.State = enAppState.IDLE;
        DebugLog.log("MainWin:BTN 'Find calls' failed: " + ex.ToString(), enLogType.ERROR);
      }
    }


    private void _btnSize_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        _imgHeight >>= 1;
        if (_imgHeight < 64)
          _imgHeight = MAX_IMG_HEIGHT;
        foreach (UIElement ui in _spSpectrums.Children)
        {
          ctlWavFile ctl = ui as ctlWavFile;
          ctl._img.MaxHeight = _imgHeight;
          ctl._img.Height = _imgHeight;
        }
        DebugLog.log("MainWin:BTN 'Size' clicked ", enLogType.DEBUG);
      }
      catch (Exception ex)
      {
        DebugLog.log("MainWin:BTN 'Size' failed: " + ex.ToString(), enLogType.ERROR);
      }
    }

    private void Window_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
    {
      try
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
        DebugLog.log("MainWin:BTN 'Window Up' clicked ", enLogType.DEBUG);
      }
      catch (Exception ex)
      {
        DebugLog.log("MainWin:BTN 'Window up: " + ex.ToString(), enLogType.ERROR);
      }
    }

    private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
      _model.KeyPressed = e.Key;
    }

    private void _tbReport_GotFocus(object sender, RoutedEventArgs e)
    {
      try
      {
        if (_model.CurrentlyOpen != null)
        {
          if (_model.CurrentlyOpen.Analysis.Report != null)
          {
            _dgData.ItemsSource = _model.CurrentlyOpen.Analysis.Report;
            if (_dgData.Columns.Count > 0)
              _dgData.Columns[0].Visibility = Visibility.Hidden; // hide 'changed'
            _dgData.IsReadOnly = true;
          }
          else
            _dgData.ItemsSource= null;
        }
        DebugLog.log("TAB 'Report' got focus", enLogType.DEBUG);
      }
      catch (Exception ex)
      {
        DebugLog.log("TAB 'Report' focus failed:" + ex.ToString(), enLogType.ERROR);
      }
    }

    private void _tbSum_GotFocus(object sender, RoutedEventArgs e)
    {
      try
      {
        if (_model.CurrentlyOpen.Analysis.Summary != null)
          _dgSum.ItemsSource = _model.CurrentlyOpen.Analysis.Summary;
        DebugLog.log("TAB 'Summary' got focus", enLogType.DEBUG);
      }
      catch (Exception ex)
      {
        DebugLog.log("TAB 'Summary' focus failed:" + ex.ToString(), enLogType.ERROR);
      }
    }


    private void Window_Closing(object sender, CancelEventArgs e)
    {
      DebugLog.log("closing application", enLogType.DEBUG);
      checkSavePrj();
      if (_frmColorMap != null)
        _frmColorMap.Close();
      if (_frmZoom != null)
        _frmZoom.Close();
      if (_frmSpecies != null)
        _frmSpecies.Close();
      if (_frmFilter != null)
        _frmFilter.Close();
      if (_frmScript != null)
        _frmScript.Close();
      if (_frmAbout != null)
        _frmAbout.Close();
      if (_frmSettings != null)
        _frmSettings.Close();
      if (_frmCreatePrj != null)
        _frmCreatePrj.Close();
      if (_frmCreateReport != null)
        _frmCreateReport.Close();
      if (_frmWavFile != null)
        _frmWavFile.Close();
      if (_frmDebug != null)
        _frmDebug.Close();
      if (_frmQuery != null)
        _frmQuery.Close();
      if (_frmCleanup != null)
        _frmCleanup.Close();
      if(_frmMsg != null)
        _frmMsg.Close();
      DebugLog.save();
    }

    private void _btnFilter_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        if (_frmFilter == null)
          _frmFilter = new FrmFilter(_model.Filter, populateFilterComboBoxes);
        _frmFilter.Show();
        _frmFilter.Visibility = Visibility.Visible;
        _frmFilter.Topmost = true;
        DebugLog.log("MainWin:BTN 'Filter' clicked", enLogType.DEBUG);
      }
      catch (Exception ex)
      {
        DebugLog.log("MainWin:BTN 'Filter' failed:" + ex.ToString(), enLogType.ERROR);
      }
    }

    private void _btnApplyFilter_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        FilterItem filter = (_cbFilter.SelectedIndex == 1) ?
                         filter = _model.Filter.TempFilter : filter = _model.Filter.getFilter(_cbFilter.Text);
        if (filter != null)
        {
          foreach (ctlWavFile c in _spSpectrums.Children)
          {
            bool res = _model.Filter.apply(filter, c.Analysis);
            c._cbSel.IsChecked = res;
          }
          _btnHideUnSelected_Click(sender, e);
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

    private void _btnSave_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        if ((_model.Prj != null) && (_model.Prj.Notes != null))
        {
          _model.Prj.Notes = _ctlPrjInfo._tbNotes.Text;
          _model.Prj.writePrjFile();
        }
        _model.saveSettings();
        if ((_model != null) && (_model.Prj != null) && (_model.Prj.Analysis != null) &&
          (_model.Prj.Analysis.Report != null))
          _model.Prj.Analysis.save(_model.Prj.ReportName, _model.Prj.Notes);
        DebugLog.log("MainWin:BTN 'save' clicked", enLogType.DEBUG);
      }
      catch (Exception ex)
      {
        DebugLog.log("MainWin:BTN 'save' failed:" + ex.ToString(), enLogType.ERROR);
      }
    }

    private void _btnInfo_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        if (_frmAbout == null)
          _frmAbout = new FrmAbout(_model.Version);
        _frmAbout.Show();
        _frmAbout.Visibility = Visibility.Visible;
        _frmAbout.Topmost = true;
        DebugLog.log("MainWin:BTN 'info' clicked", enLogType.DEBUG);
      }
      catch (Exception ex)
      {
        DebugLog.log("MainWin:BTN 'info' failed:" + ex.ToString(), enLogType.ERROR);
      }
    }

    private void _btnCallInfo_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        foreach (ctlWavFile ctl in _spSpectrums.Children)
          ctl.InfoVisible = !ctl.InfoVisible;
        if (_spSpectrums.Children.Count > 0)
        {
          ctlWavFile ctl0 = _spSpectrums.Children[0] as ctlWavFile;
          AppParams.Inst.HideInfos = !ctl0.InfoVisible;
          DebugLog.log("MainWin:BTN 'Call Info' clicked", enLogType.DEBUG);
        }
      }
      catch (Exception ex)
      {
        DebugLog.log("MainWin:BTN 'Call Info' failed:" + ex.ToString(), enLogType.ERROR);
      }
    }

    private void _btnColorPalette_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        if (_frmColorMap == null)
          _frmColorMap = new FrmColorMap(_model);
        _frmColorMap.Show();
        _frmColorMap.Visibility = Visibility.Visible;
        _frmColorMap.Topmost = true;
        DebugLog.log("MainWin:BTN 'Color Palette' clicked", enLogType.DEBUG);
      }
      catch (Exception ex)
      {
        DebugLog.log("MainWin:BTN 'Color Palette' failed:" + ex.ToString(), enLogType.ERROR);
      }
    }

    private void _btnSpecies_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        if (_frmSpecies == null)
          _frmSpecies = new frmSpeciesData(_model, closeWindow, this);
        _frmSpecies.Show();
        _frmSpecies.Visibility = Visibility.Visible;
        DebugLog.log("MainWin:BTN 'Species' clicked", enLogType.DEBUG);
      }
      catch (Exception ex)
      {
        DebugLog.log("MainWin:BTN 'Species' failed:" + ex.ToString(), enLogType.ERROR);
      }
    }

    private void _btnSettings_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        if (_frmSettings == null)
          _frmSettings = new frmSettings(AppParams.Inst);
        _frmSettings.Show();
        _frmSettings.Visibility = Visibility.Visible;
//        _frmSettings.Topmost = true;
        DebugLog.log("MainWin:BTN 'Settings' clicked", enLogType.DEBUG);
      }
      catch (Exception ex)
      {
        DebugLog.log("MainWin:BTN 'Settings' failed:" + ex.ToString(), enLogType.ERROR);
      }
    }

    private void _btnWavTool_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        if (_frmWavFile == null)
          _frmWavFile = new frmWavFile(_model);
        _frmWavFile.Show();
        _frmWavFile.Visibility = Visibility.Visible;
        _frmWavFile.Topmost = true;
        DebugLog.log("MainWin:BTN 'WavTool' clicked", enLogType.DEBUG);
      }
      catch (Exception ex)
      {
        DebugLog.log("MainWin:BTN 'WavTool' failed:" + ex.ToString(), enLogType.ERROR);
      }
    }

    private void Content_rendered(object sender, System.EventArgs e)
    {
      if ((_ctlZoom != null) && (_model.SelectedDir != null))
      {
        _ctlZoom.update();
        _tbMain.SelectedIndex = 2;
      }
    }

    private void Window_LocationChanged(object sender, System.EventArgs e)
    {
      setZoomPosition();
      AppParams.Inst.MainWindowPosX = this.Top;
      AppParams.Inst.MainWindowPosY = this.Left;

    }

    private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
    {
      setZoomPosition();
      AppParams.Inst.MainWindowWidth = this.Width;
      AppParams.Inst.MainWindowHeight = this.Height;
    }

    void setMouseStatus()
    {
      Application.Current.Dispatcher.BeginInvoke((Action)(() =>
      {
        if (_model.Busy)
          Mouse.OverrideCursor = Cursors.Wait;
        else
          Mouse.OverrideCursor = null;
      }));
    }

    private void timer_Tick(object sender, EventArgs e)
    {
      setMouseStatus();
      if (_model.Status.Msg != null)
      {
        showMsg(BatInspector.Properties.MyResources.msgInformation, _model.Status.Msg);
        _lblStatus.Text = _model.Status.Msg;
        _model.Status.Msg = null;
      }

      if (_model.UpdateUi)
      {
        if ((_model.Prj != null) && (_model.Prj.Analysis != null))
          _model.Prj.Analysis.updateSpeciesCount();
        updateWavControls();
        _model.UpdateUi = false;
      }

      if ((_model.Prj != null) && (_model.Prj.ReloadInGui))
      {
        if ((_model.Prj != null) && _model.Prj.Ok && (_model.Prj.Analysis != null))
        {
          _spSpectrums.Children.Clear();
          DirectoryInfo dir = new DirectoryInfo(_model.SelectedDir);
          initializeProject(dir);
        }
        _model.Prj.ReloadInGui = false;
      }

      switch (_model.Status.State)
      {
        case enAppState.IDLE:
          hideMsg();
          break;

        case enAppState.AI_ANALYZE:
          if ((_workerPredict != null) && (!_workerPredict.IsAlive))
          {
            _workerPredict = null;
            _model.updateReport();
            DirectoryInfo dir = new DirectoryInfo(_model.SelectedDir);
            initializeProject(dir);
          }
          break;

        case enAppState.OPEN_PRJ:
          if ((_workerStartup != null) && (!_workerStartup.IsAlive))
          {
            _workerStartup = null;
            populateControls();
            if ((_model.Prj != null) && _model.Prj.Ok)
              _lblProject.Text = BatInspector.Properties.MyResources.MainWindowPROJECT + ": " + _model.Prj.Name;
            if (_model.Query != null)
              _lblProject.Text = BatInspector.Properties.MyResources.MainWindow_timer_Tick_QUERY + ": " + _model.Query.Name;
            _model.Status.State = enAppState.WAIT_FOR_GUI;
          }
          break;

        case enAppState.WAIT_FOR_GUI:
          showStatus();
          _model.Status.State = enAppState.IDLE;
          break;

      }
      if (_workerStartup == null)
        _model.Busy = false;
    
      if (_switchTabToPrj)
      {
        _tbPrj.IsSelected = true;
        _switchTabToPrj = false;
      }

      if (_frmZoom != null)
        _frmZoom._ctl.tick(_timer.Interval.TotalMilliseconds);
      if (_ctlZoom != null)
        _ctlZoom.tick(_timer.Interval.TotalMilliseconds);
    }

    private void _btnReport_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        if (_frmCreateReport == null)
          _frmCreateReport = new frmCreateReport(_model);
        Filter.populateFilterComboBox(_frmCreateReport._ctlReport._cbFilter, _model);
        _frmCreateReport.Show();
        _frmCreateReport.Visibility = Visibility.Visible;
//        _frmCreateReport.Topmost = true;
        DebugLog.log("MainWin:BTN 'Report' clicked", enLogType.DEBUG);
      }
      catch (Exception ex)
      {
        DebugLog.log("MainWin:BTN 'Report' failed:" + ex.ToString(), enLogType.ERROR);
      }
    }

    private void _btnCopySpec_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        if ((AppParams.Inst.ScriptCopyAutoToMan != null) && (AppParams.Inst.ScriptCopyAutoToMan != ""))
          _model.executeScript(AppParams.Inst.ScriptCopyAutoToMan);
        else
          MessageBox.Show(MyResources.MsgSpecifyScript, "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        DebugLog.log("MainWin:BTN 'Copy Species' clicked", enLogType.DEBUG);
      }
      catch (Exception ex)
      {
        DebugLog.log("MainWin:BTN 'Copy Species' failed:" + ex.ToString(), enLogType.ERROR);
      }
    }

    private void _grdSplitterH_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
    {
      AppParams.Inst.LogControlHeight = _grdMain.RowDefinitions[3].Height.Value;
    }

    private void _grdSplitterV_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
    {
      AppParams.Inst.WidthFileSelector = _grdCtrl.ColumnDefinitions[0].Width.Value;
    }

    private void _btnScript_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        if (_frmScript == null)
          _frmScript = new FrmScript(_model, populateToolsMenu);
        _frmScript.Show();
        DebugLog.log("MainWin:BTN 'Script' clicked", enLogType.DEBUG);
      }
      catch (Exception ex)
      {
        DebugLog.log("MainWin:BTN 'Script' failed: " + ex.ToString(), enLogType.ERROR);
      }
    }

    private void _btnCancelScript_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        _model.cancelScript();
        DebugLog.log("MainWin:BTN 'cancel Script' clicked", enLogType.DEBUG);
      }
      catch (Exception ex)
      {
        DebugLog.log("MainWin:BTN 'Cancel Script' failed: " + ex.ToString(), enLogType.ERROR);
      }
    }

    private void _btnDebug_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        if (_frmDebug == null)
          _frmDebug = new frmDebug(_model);
        _frmDebug.Show();
        _frmDebug.Visibility = Visibility.Visible;
        _frmDebug.Topmost = true;
        DebugLog.log("MainWin:BTN 'Debug' clicked", enLogType.DEBUG);
      }
      catch (Exception ex)
      {
        DebugLog.log("MainWin:BTN 'Debug' failed: " + ex.ToString(), enLogType.ERROR);
      }
    }

    private bool IsUserVisible(FrameworkElement element, FrameworkElement container)
    {
      //https://stackoverflow.com/questions/1517743/in-wpf-how-can-i-determine-whether-a-control-is-visible-to-the-user
      if (!element.IsVisible)
        return false;

      Rect bounds = element.TransformToAncestor(container).TransformBounds(new Rect(0.0, 0.0, element.ActualWidth, element.ActualHeight));
      Rect rect = new Rect(0.0, 0.0, container.ActualWidth, container.ActualHeight);
      //      return rect.Contains(bounds.TopLeft) || rect.Contains(bounds.BottomRight);
      return rect.IntersectsWith(bounds);
    }

    private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
    {
      if (!_fastOpen)
        return;

      double h = e.ExtentHeight;
      double p = e.VerticalOffset;
      int k = (int)((double)_spSpectrums.Children.Count * p / h);

      foreach (ctlWavFile c in _spSpectrums.Children)
      {
        if (IsUserVisible(c, this) && !c.WavInit)
        {
          if ((_model.Prj != null) && (c.Index < _model.Prj.Records.Length))
            initCtlWav(c, _model.Prj.Records[c.Index], false);
          if ((_model.Query != null) && (c.Index < _model.Query.Records.Length))
            initCtlWav(c, _model.Query.Records[c.Index], true);
          c.UpdateLayout();
        }
      }
    }


    private void DropdownButton_Checked(object sender, RoutedEventArgs e)
    {
      var menu = (sender as ToggleButton).ContextMenu;
      menu.PlacementTarget = sender as ToggleButton;
      menu.Placement = PlacementMode.Bottom;
      menu.IsOpen = true;
    }

    private void ContextMenu_Closed(object sender, RoutedEventArgs e)
    {
      ((sender as ContextMenu).PlacementTarget as ToggleButton).IsChecked = false;
    }


    private void showPdf(string name)
    {
      string helpFileName = AppDomain.CurrentDomain.BaseDirectory + name;
      try
      {
        Process.Start(helpFileName);
      }
      catch
      {
        DebugLog.log("could not open PDF file: " + helpFileName, enLogType.ERROR);
      }
    }

    private void _mnHelpSw_Click(object sender, RoutedEventArgs e)
    {
      if (AppParams.Inst.Culture == enCulture.de_DE)
        showPdf(AppParams.HELP_FILE_DE);
      else
        showPdf(AppParams.HELP_FILE_EN);
    }

    private void _mnBat1_Click(object sender, RoutedEventArgs e)
    {
      showPdf(AppParams.BAT_INFO1_PDF);
    }

    private void _mnBat2_Click(object sender, RoutedEventArgs e)
    {
      showPdf(AppParams.BAT_INFO2_PDF);
    }

    private void _btnCreatePrj_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        if (_frmCreatePrj == null)
          _frmCreatePrj = new FrmCreatePrj(_model);
        _frmCreatePrj.init();
        _frmCreatePrj.Show();
        _frmCreatePrj.Visibility = Visibility.Visible;
     //   _frmCreatePrj.Topmost = true;
        DebugLog.log("MainWin:BTN 'Create Project' clicked", enLogType.DEBUG);
      }
      catch (Exception ex)
      {
        DebugLog.log("MainWin:BTN 'Create Project' failed: " + ex.ToString(), enLogType.ERROR);
      }
    }

    private void _btnaddFile_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        if (_model.Prj.Ok)
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
            _spSpectrums.Children.Clear();
            DirectoryInfo dir = new DirectoryInfo(_model.SelectedDir);
            initializeProject(dir);
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

    private void _btnQuery_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        if (_frmQuery == null)
          _frmQuery = new FrmQuery(_model);
        _frmQuery.Show();
        DebugLog.log("MainWin:BTN 'Query' clicked", enLogType.DEBUG);
      }
      catch (Exception ex)
      {
        DebugLog.log("MainWin:BTN 'Query' failed: " + ex.ToString(), enLogType.ERROR);
      }
    }

    private void _dgData_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
      try
      {
        // https://blog.scottlogic.com/2008/12/02/wpf-datagrid-detecting-clicked-cell-and-row.html

        DependencyObject dep = (DependencyObject)e.OriginalSource;

        // iteratively traverse the visual tree
        while ((dep != null) && !(dep is DataGridCell))
          dep = VisualTreeHelper.GetParent(dep);

        if (dep == null)
          return;

        ReportItem it = null;
        if (dep is DataGridCell)
        {
          DataGridCell cell = dep as DataGridCell;
          // navigate further up the tree
          while ((dep != null) && !(dep is DataGridRow))
            dep = VisualTreeHelper.GetParent(dep);

          DataGridRow row = dep as DataGridRow;
          it = row.DataContext as ReportItem;
        }

        if (_model.CurrentlyOpen != null)
        {
          if (it != null)
          {
            int.TryParse(it.CallNr, out int callNr);
            AnalysisFile analysis = _model.CurrentlyOpen.Analysis.find(it.FileName);
            string fileName = Path.GetFileName(it.FileName);
            string wavPath = Path.GetDirectoryName(_model.CurrentlyOpen.getFullFilePath(it.FileName));
            setZoom(fileName, analysis, wavPath, null);
            changeCallInZoom(callNr - 1);
          }
        }
        DebugLog.log("Main:Report double click", enLogType.DEBUG);
      }
      catch (Exception ex)
      {
        DebugLog.log("Main:Report double click failed: " + ex.ToString(), enLogType.ERROR);
      }
    }

    private void _btnCleanup_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        if (_frmCleanup == null)
          _frmCleanup = new frmCleanup(_model);
        _frmCleanup.Show();
        DebugLog.log("MainWin:BTN 'Clean Up' clicked", enLogType.DEBUG);
      }
      catch (Exception ex)
      {
        DebugLog.log("MainWin:BTN 'Clean Up' failed: " + ex.ToString(), enLogType.ERROR);
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
      if(apply)
        _btnApplyFilter_Click(sender, null);
      if(resetFilter)
        _btnShowAll_Click(sender, null);
    }


    private void populateToolsMenu()
    {
      _mnuToolsItems.Items.Clear();
      foreach (ScriptItem s in AppParams.Inst.ScriptInventory.Scripts)
      {
        if (!s.IsTool)
        {
          MenuItem m = new MenuItem();
          m.Header = s.Description;
          m.Tag = s.Name;
          m.Click += _mnTool1_Click;
          _mnuToolsItems.Items.Add(m);
        }
      }
    }

    private void _mnTool1_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        DebugLog.log("BTN custom tool pressed", enLogType.DEBUG);
        MenuItem m = sender as MenuItem;
        string script = (string)m.Tag;
        ScriptItem item = _model.Scripter.getScript(script);
        if (item != null)
        {
          if (item.Parameter.Count > 0)
          {
            string winTitle = BatInspector.Properties.MyResources.frmScriptParamTitle +": " + script;
            frmScriptParams frm = new frmScriptParams(winTitle, item.Parameter);
            frm.ShowDialog();
            if (frm.DialogResult == true)
              _model.executeScript(script, frm.ParameteValues);
          }
          else
            _model.executeScript(script);
        }
      }
      catch(Exception ex)
      {
        DebugLog.log("Error BTN custom tool: " + ex.ToString(), enLogType.ERROR);
      }
    }

   // https://stackoverflow.com/questions/16245706/check-for-device-change-add-remove-events/16245901#16245901

    protected override void OnSourceInitialized(EventArgs e)
    {
      base.OnSourceInitialized(e);
      HwndSource source = PresentationSource.FromVisual(this) as HwndSource;
      source.AddHook(WndProc);
    }
    protected IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
      switch (msg)
      {
        case 0x0219:       //WM_DEVICECHANGE 
          initTreeView();
          break;             
      }
      return IntPtr.Zero;
    }

    private void _btnRecovery_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        DebugLog.log("Main:BTN Recovery clicked", enLogType.DEBUG);
        if ((_model.Prj != null) && _model.Prj.Ok)
        {
          FrmRecovery frm = new FrmRecovery(_model.Prj.Name);
          bool res = frm.ShowDialog() == true;
          if (res)
            _model.Prj.recovery(frm._cbDel.IsChecked == true, frm._cbChanged.IsChecked == true);
        }
      }
      catch (Exception ex)
      {
        DebugLog.log("Main:BTN Recovery click failed: " + ex.ToString(), enLogType.ERROR);
      }
    }

    private void _btnRecorder_Click(object sender, RoutedEventArgs e)
    {
      FrmSetupRecorder frm = new FrmSetupRecorder(_model.Recorder);
      frm.Owner = this;
      frm.Show();
    }


    #region WindowChrome
    // Can execute
    private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
      e.CanExecute = true;
    }

    // Minimize
    private void CommandBinding_Executed_Minimize(object sender, ExecutedRoutedEventArgs e)
    {
      SystemCommands.MinimizeWindow(this);
    }

    // Maximize
    private void CommandBinding_Executed_Maximize(object sender, ExecutedRoutedEventArgs e)
    {
      SystemCommands.MaximizeWindow(this);
    }

    // Restore
    private void CommandBinding_Executed_Restore(object sender, ExecutedRoutedEventArgs e)
    {
      SystemCommands.RestoreWindow(this);
    }

    // Close
    private void CommandBinding_Executed_Close(object sender, ExecutedRoutedEventArgs e)
    {
      SystemCommands.CloseWindow(this);
    }

    // State change
    private void MainWindowStateChangeRaised(object sender, EventArgs e)
    {
      if (WindowState == WindowState.Maximized)
      {
        MainWindowBorder.BorderThickness = new Thickness(8);
        RestoreButton.Visibility = Visibility.Visible;
        MaximizeButton.Visibility = Visibility.Collapsed;
      }
      else
      {
        MainWindowBorder.BorderThickness = new Thickness(0);
        RestoreButton.Visibility = Visibility.Collapsed;
        MaximizeButton.Visibility = Visibility.Visible;
      }
    }
    #endregion
  }
  public enum enWinType
  {
    ZOOM,
    BAT
  }

  public delegate void dlgSetFocus(int index);
  public delegate void dlgcloseChildWindow(enWinType w);
}
