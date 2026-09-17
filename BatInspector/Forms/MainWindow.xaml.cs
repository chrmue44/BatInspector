/********************************************************************************
 *               Author: Christian Müller
 *     Date of creation: 2021-08-10                                       
 *   Copyright (C) 2023: Christian Müller chrmue44(at)gmail(dot).com
 *
 *              Licence:  CC BY-NC 4.0 
 ********************************************************************************/
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Interop;
using System.Windows.Input;
using System.Reflection;
using BatInspector.Controls;
using libParser;
using BatInspector.Properties;
using System.Windows.Threading;
using System.Data;

namespace BatInspector.Forms
{

  delegate void dlgProgress(string pngName);
  delegate Task dlgInitPrj(DirectoryInfo dir);
  delegate Task dlgInitQuery(FileInfo file);
  delegate void dlgInitProjectAsync(DirectoryInfo dir);
  delegate void dlgInitQueryAsync(FileInfo file);
  delegate void dlgOneInt(int a);


  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    public const int MAX_IMG_HEIGHT = 256;
    const int MAX_IMG_WIDTH = 512;

    FrmFilter? _frmFilter = null;
    FrmScript? _frmScript = null;
    FrmAbout? _frmAbout = null;
    frmSettings? _frmSettings = null;
    FrmCreatePrj? _frmCreatePrj = null;
    frmCreateReport? _frmCreateReport = null;
    frmWavFile? _frmWavFile = null;
    FrmColorMap? _frmColorMap = null;
    frmDebug? _frmDebug = null;
    FrmQuery? _frmQuery = null;
    frmCleanup? _frmCleanup = null;
    FrmMessage? _frmMsg = new FrmMessage();
    frmExport? _frmExp = null;
    frmExportData? _frmExportData = null;
    int _imgHeight = MAX_IMG_HEIGHT;
    FrmZoom? _frmZoom = null;
    CtrlZoom? _ctlZoom = null;
    TabItem? _tbZoom = null;
    System.Windows.Threading.DispatcherTimer _timer;
    bool _switchTabToPrj = false;
    Stopwatch _sw = new Stopwatch();
    DirectoryInfo? _projectDir;
    string _oldTab = "";
    Pool<ctlWavFile> _wavCtls;

    double _scrollBarPrjPos = 0;
    bool _mouseIsDownOnScrollPrj = false;
//    double _scrollBarListPos = 0;
//    bool _mouseIsDownOnScrollList = false;
    bool _treeViewCollaped = false;
    bool _infoVisible = true;


    public MainWindow()
    {
      DateTime linkTimeLocal = System.IO.File.GetLastWriteTime(Assembly.GetExecutingAssembly().Location);

      App.Model.Status.State = enAppState.IDLE;
      string versionStr = "BatInspector V" + AppParams.AppVersion + " " + linkTimeLocal.ToString();
      setLanguage();
      InitializeComponent();
      _ctlStatistic.setup();
      StateChanged += MainWindowStateChangeRaised;
      _ctlLog._cbErr.IsChecked = AppParams.Inst.LogShowError;
      _ctlLog._cbWarn.IsChecked = AppParams.Inst.LogShowWarning;
      _ctlLog._cbInfo.IsChecked = AppParams.Inst.LogShowInfo;
      _ctlLog._cbDebug.IsChecked = AppParams.Inst.LogShowDebug;
      _ctlPrjBtn.setup(true, false);
      _ctlListBtn.setup(false, true);
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

      Screen? s = Screen.PrimaryScreen;
      if (s != null)
      {
        double maxWidth = s.WorkingArea.Width;
        double maxHeight = s.WorkingArea.Height;
        if ((this.Left + this.Width) > maxWidth)
        {
          double w = maxHeight - this.Left;
          this.Width = w > 0 ? w : maxWidth - 50;
        }
        if ((this.Top + this.Height) > maxHeight)
        {
          double h = maxHeight - this.Top;
          this.Height = h > 0 ? h : maxHeight - 50;
        }
      }
      _timer = new System.Windows.Threading.DispatcherTimer();
      _timer.Tick += new EventHandler(timer_Tick);
      _timer.Interval = new TimeSpan(0, 0, 0, 0, 300);
      _timer.Start();
      _ctlLog.setup(App.Model.executeCmd, false);
      populateToolsMenu();
#if DEBUG
      Tests tests = new Tests();
      tests.exec();
      _switchTabToPrj = true;
#endif
      DebugLog.log(versionStr + " started", enLogType.DEBUG);
      Installer.hideSplash();
      collapseTreeView(false);
      _wavCtls = new Pool<ctlWavFile>(AppParams.CNT_WAV_CONTROLS);

      if (string.IsNullOrEmpty(AppParams.Inst.ExeAcrobat))
      {
        System.Windows.MessageBox.Show(this, MyResources.MainWindow_MsgAcrobat, MyResources.Attention, MessageBoxButton.OK, MessageBoxImage.Exclamation);
        _frmSettings = new frmSettings(AppParams.Inst);
        _frmSettings.ShowDialog();
      }
      setExpertMode(AppParams.Inst.ExpertMode);
    }


    public void initTreeView()
    {
      _trvStructure.Items.Clear();
      DriveInfo[] drives = DriveInfo.GetDrives();
      if (AppParams.Inst.ShowOnlyFilteredDirs)
      {
        foreach (string dir in AppParams.Inst.DirFilter)
        {
          if ((dir != null) && (dir.Length > 0))
          {
            DirectoryInfo batDataDir = new DirectoryInfo(dir);
            TreeViewItem? child = CreateTreeItem(batDataDir);
            if (child == null)
              continue;
            _trvStructure.Items.Add(child);
          }
        }
      }
      else
      {
        foreach (DriveInfo driveInfo in drives)
        {
          DirectoryInfo dir = new DirectoryInfo(driveInfo.Name);
          TreeViewItem? child = CreateTreeItem(dir);
          if (child == null)
            continue;
          _trvStructure.Items.Add(child);
        }
      }
    }

    public void TreeViewItem_Expanded(object? sender, RoutedEventArgs? e)
    {
      TreeViewItem? item = e?.Source as TreeViewItem;
      if ((item != null) && (item.Items.Count >= 1) /*&& (item.Items[0] is string) */)
      {
        item.Items.Clear();

        DirectoryInfo? expandedDir = null;
        if (item.Tag is DriveInfo)
          expandedDir = ((DriveInfo)item.Tag).RootDirectory;
        if (item.Tag is DirectoryInfo)
          expandedDir = (item.Tag as DirectoryInfo);
        try
        {
          if (expandedDir != null)
          {
            if (Project.containsProject(expandedDir) == "")
            {
              DebugLog.log("start evaluation TODO", enLogType.DEBUG);
              foreach (DirectoryInfo subDir in expandedDir.GetDirectories().OrderBy(f => f.Name))
              {
                TreeViewItem? childItem = CreateTreeItem(subDir);
                if (childItem == null)
                  continue;
                item.Items.Add(childItem);
                string prjFile = Project.containsProject(subDir);
                if (prjFile != "")
                {
                  ModelParams[]? modelParams = Project.readModelParams(prjFile);
                  childItem.FontWeight = FontWeights.Bold;
                  if (Project.evaluationDone(subDir, modelParams))
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
                  TreeViewItem? childItem = CreateTreeItem(subFile);
                  if (childItem == null)
                    continue;
                  item.Items.Add(childItem);
                  childItem.FontWeight = FontWeights.Bold;
                  childItem.Foreground = new SolidColorBrush(Colors.Orange);
                }
              }
            }
            DebugLog.log("evaluation of dir '" + expandedDir.Name + "' for TODOs finished", enLogType.DEBUG);
          }
        }
        catch (Exception ex)
        {
          DebugLog.log($"problem Mainwindow;{ex}", enLogType.ERROR);
        }
      }
    }

    public void openExportWindow()
    {
      if (_frmExp == null)
        _frmExp = new frmExport();
      _frmExp.Show();

    }

    private void setLanguage()
    {
      string culture = AppParams.Inst.Culture.ToString().Replace('_', '-');
      Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(culture);
      Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(culture);
    }

    private void trvStructure_Collapsed(object sender, RoutedEventArgs e)
    {
      TreeViewItem item = (TreeViewItem)e.Source;

    }

    public async void TreeViewItem_Selected(object sender, RoutedEventArgs e)
    {
      TreeViewItem item = (TreeViewItem)e.Source;
      DirectoryInfo? dir = item.Tag as DirectoryInfo;
      _sw.Restart();
      foreach (ctlWavFile ctl in _spSpectrums.Children)
        ctl.release();
      _spSpectrums.Children.Clear();
      _tbSum.Visibility = Visibility.Visible;
      if ((dir != null) && (Project.containsProject(dir) != ""))
      {
        _ctlPrjBtn.initFileButton(false);
        collapseTreeView(true);
        await initializeProject(dir);
      }
      else
      {
        FileInfo? file = item.Tag as FileInfo;
        if ((file != null) && Query.isQuery(file))
        {
          _ctlPrjBtn.initFileButton(true);
          _tbSum.Visibility = Visibility.Collapsed;
          collapseTreeView(true);
          await initializeQuery(file);
        }
      }
    }

    public void setStatus(string status)
    {
      _lblStatus.Text = status;
    }

    public void populateFilterComboBoxes()
    {
      Filter.populateFilterComboBox(_ctlPrjBtn._cbFilter);
      Filter.populateFilterComboBox(_ctlListBtn._cbFilter);
      Filter.populateFilterComboBox(_ctlMySQL._cbFilter);
      _ctlScatter.populateComboBoxes();
      _ctlStatistic.populateComboBoxes();
    }


    public void closeWindow(enWinType w)
    {
      switch (w)
      {
        case enWinType.ZOOM:
          _frmZoom = null;
          break;
      }
    }

    public void togglePngSize()
    {
      _imgHeight >>= 1;
      if (_imgHeight < 64)
        _imgHeight = MAX_IMG_HEIGHT;
      foreach (UIElement ui in _spSpectrums.Children)
      {
        ctlWavFile ctl = (ctlWavFile)ui;
        ctl.setHeight(_imgHeight);
      }
    }


    /// <summary>
    /// callback in case of Analysis has changed (manual species)
    /// All connected GUI elements are updated
    /// </summary>
    /// <param name="fName">Name of the WAV file</param>
    public void callbackUpdateAnalysis(string fName)
    {
      if (Dispatcher.CheckAccess()) // CheckAccess returns true if you're on the dispatcher thread
      {
        //        _tbReport_GotFocus(null, null);
        foreach (ctlWavFile ctl in _spSpectrums.Children)
        {
          if (ctl.Analysis?.getString(Cols.NAME) == fName)
          {
            if (App.Model.CurrentlyOpen != null)
            {
              AnalysisFile? newAnalysis = App.Model.CurrentlyOpen.Analysis.find(fName);
              PrjRecord? rec = App.Model.CurrentlyOpen.findRecord(fName);
              if ((newAnalysis != null) && (rec != null))
              {
                ctl.updateCallInformations(newAnalysis, rec);
                if (ctl.Analysis == App.Model.ZoomView.Analysis)
                  _ctlZoom?.updateManSpecies();
                break;
              }
            }
          }
        }
      }
    }



    private async Task initProjectAsync(DirectoryInfo dir)
    {
      if (!Dispatcher.CheckAccess()) // CheckAccess returns true if you're on the dispatcher thread
      {
        await Dispatcher.BeginInvoke(new dlgInitPrj(initProjectAsync), dir);
      }
      else
      {
        try
        {
          App.Model.initProject(dir, true);
          if ((App.Model.Prj != null) && App.Model.Prj.Ok )
          {
            _wavCtls.reinitializePool();
            App.Model.View.initSonogramPool();
            _spSpectrums.Children.Clear();
            _scrollPrj.Minimum = 0;
            _scrollBarPrjPos = 0;
            _scrollPrj.Maximum = App.Model.Prj.Records.Length - 1;
           // _scrollBarListPos = 0;
            // TODO set scroll button size
            _ctlPrjInfo.setup(App.Model.Prj);
            _lblPrj.Content = MyResources.ctlProjectInfo + " [" + Path.GetFileNameWithoutExtension(App.Model.Prj.Name) + "]";
            _ctlScatter.initPrj();
            _switchTabToPrj = true;
            buildWavFileList(false);
            App.Model.View.stopCreatingPngFiles();
            App.Model.View.startCreatingPngFiles();
          }
        }
        catch (Exception ex)
        {
          DebugLog.log("Error opening project: " + ex.ToString(), enLogType.ERROR);
        }
      }
    }

    private async Task initQueryAsync(FileInfo queryFile)
    {
      if (!Dispatcher.CheckAccess()) // CheckAccess returns true if you're on the dispatcher thread
      {
        await Dispatcher.BeginInvoke(new dlgInitQuery(initQueryAsync), queryFile);
      }
      else
      {
        try
        {
          App.Model.View.stopCreatingPngFiles();
          App.Model.initQuery(queryFile);
          if (App.Model.Query != null)
          {
            _wavCtls.reinitializePool();
            App.Model.View.initSonogramPool();
            _spSpectrums.Children.Clear();
            _scrollPrj.Minimum = 0;
            _scrollBarPrjPos = 0;
            _scrollPrj.Maximum = App.Model.Query.Records.Length - 1;
            if(_scrollPrj.Maximum < 0)
              _scrollPrj.Maximum = 0;
            //     _scrollBarListPos = 0;
            // TODO set scroll button size
            _lblPrj.Content = "QUERY:" + " [" + Path.GetFileNameWithoutExtension(App.Model.Query.Name) + "]";
            if (_frmQuery == null)
              _frmQuery = new FrmQuery();
            _frmQuery.initFieldsFromQuery();

            _switchTabToPrj = true;
            buildWavFileList(false);

            App.Model.View.stopCreatingPngFiles();
            if (App.Model.Query != null)
              showStatus();
          }
        }
        catch (Exception ex)
        {
          DebugLog.log("Error opening query: " + ex.ToString(), enLogType.ERROR);
        }
      }
    }

    public void toggleCallInfo()
    {
      _infoVisible = !_infoVisible;
      foreach (ctlWavFile ctl in _spSpectrums.Children)
        ctl.InfoVisible = _infoVisible;
      if (_spSpectrums.Children.Count > 0)
      {
        ctlWavFile? ctl0 = _spSpectrums.Children[0] as ctlWavFile;
        if (ctl0 != null)
        {
          AppParams.Inst.HideInfos = !ctl0.InfoVisible;
          DebugLog.log("MainWin:BTN 'Call Info' clicked", enLogType.DEBUG);
        }
      }
    }

    public async Task initializeProject(DirectoryInfo dir)
    {
      if (!Dispatcher.CheckAccess()) // CheckAccess returns true if you're on the dispatcher thread
      {
        await Dispatcher.BeginInvoke(new dlgInitPrj(initializeProject), dir, DispatcherPriority.Send);
      }
      else
      {
        _projectDir = dir;
        App.Model.Status.State = enAppState.BUSY;
        setMouseStatus();
        DebugLog.log("start to open project", enLogType.DEBUG);
        TimeSpan t = _sw.Elapsed;
        _lblProject.Text = BatInspector.Properties.MyResources.MainWindowMsgOpenPrj;
        setStatus("");
        await showMsgInUiThread(MyResources.msgInformation, MyResources.MainWindowMsgOpenPrj, true);

        _scrollPrj.Value = 0;
        checkSavePrj();
        _tbPrj.Focus();
        _dgData.ItemsSource = null;
        await initProjectAsync(dir);

        _ctlPrjBtn._cbFilter.SelectedIndex = 0;
        if (_btnCreatePrj.IsEnabled)
          _btnCreatePrj.IsEnabled = false;
        if ((App.Model.Prj != null) && App.Model.Prj.Ok)
        {
          _lblProject.Text = MyResources.MainWindowPROJECT + ": " + App.Model.Prj.Name;
          DebugLog.log("Project opened: " + App.Model.Prj.Name, enLogType.INFO);
        }
        App.Model.Status.State = enAppState.IDLE;
        showStatus();
      }
    }


    async Task initializeQuery(FileInfo file)
    {
      if (!Dispatcher.CheckAccess()) // CheckAccess returns true if you're on the dispatcher thread
      {
        await Dispatcher.BeginInvoke(new dlgInitQuery(initializeQuery), file);
      }
      else
      {
        _projectDir = null;
        App.Model.Status.State = enAppState.BUSY;
        setMouseStatus();
        DebugLog.log("start to open query", enLogType.DEBUG);
        _lblProject.Text = BatInspector.Properties.MyResources.MainWindowMsgOpenPrj;
        setStatus("");
        await showMsgInUiThread(MyResources.msgInformation, MyResources.MainWindowMsgOpenQuery);
        checkSavePrj();
        _tbPrj.Focus();
        await initQueryAsync(file);

        _ctlPrjBtn._cbFilter.SelectedIndex = 0;
        if (_btnCreatePrj.IsEnabled)
          _btnCreatePrj.IsEnabled = false;
        if (App.Model.Query != null)
        {
          _lblProject.Text = MyResources.MainWindow_timer_Tick_QUERY + ": " + App.Model.Query.Name;
          DebugLog.log("Query opened: " + App.Model.Query.Name, enLogType.INFO);
        }
        App.Model.Status.State = enAppState.IDLE;
        showStatus();
      }
    }


    public void buildWavFileList(bool selectedOnly, Filter? filter = null, FilterItem? filterItem = null, bool reInitList = false)
    {
      if (App.Model.CurrentlyOpen == null)
        return;
      App.Model.View.buildListOfVisibles(selectedOnly);
      if (_tbPrj.IsSelected)
      {
        double oldValue = _scrollPrj.Value;
        _scrollPrj.SmallChange = 1.0;
        _scrollPrj.Maximum = Math.Max(1, App.Model.View.VisibleFiles.Count - 1);
        _scrollPrj.Track.ViewportSize = double.NaN;
        _scrollPrj.Track.Thumb.Height = Math.Max(10, _spSpectrums.ActualHeight / _scrollPrj.Maximum / 3);
        _scrollPrj.InvalidateVisual();
        _scrollPrj.Value = 0;
        if (oldValue == _scrollPrj.Value)  //if value is different list will be built by change event  
          populateControls(0);
        if(_spSpectrums.Children.Count == 0)
          populateControls(0);

      }
      else if (_tbReport.IsSelected)
      {
        if (reInitList)
          _dgData.ItemsSource = null;
        if ((_dgData.ItemsSource == null) && (filter != null) && (filterItem != null))
        {
          if (App.Model.View.populateList(filter, filterItem))
            initDataGridSource();
        }
      }
    }

    private void initZoomWindow()
    {
      if (AppParams.Inst.ZoomSeparateWin)
      {
        _frmZoom = new FrmZoom(closeWindow)
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



    public void setZoom(string name, AnalysisFile analysis, string wavFilePath, ctlWavFile? ctlWav, enModel modelType, string[]? species = null)
    {
      DebugLog.log("activate zoom view of: " + name, enLogType.DEBUG);
      if (AppParams.Inst.ZoomSeparateWin)
      {
        if (_frmZoom == null)
          _frmZoom = new FrmZoom(closeWindow);
        _frmZoom.setup(name, analysis, wavFilePath, ctlWav, openExportWindow, modelType);
        setZoomPosition();
        _frmZoom.Show();
      }
      else
      {
        if (App.Model.CurrentlyOpen != null)
          _ctlZoom?.setup(analysis, wavFilePath, App.Model.CurrentlyOpen.Species, ctlWav, openExportWindow, modelType);
        else
          _ctlZoom?.setup(analysis, wavFilePath, species, null, openExportWindow, modelType);
        _tbZoom?.Header = "Zoom: " + Path.GetFileName(name);
        _tbZoom?.Visibility = Visibility.Visible;
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
      if (App.Model.Prj != null)
      {
        if (App.Model.Prj.Ok && App.Model.Prj.Analysis.Changed && (App.Model.Prj.Analysis.Files.Count > 0))
        {
          MessageBoxResult res = System.Windows.MessageBox.Show(MyResources.msgSaveBeforeClose, MyResources.msgQuestion, MessageBoxButton.YesNo, MessageBoxImage.Question);
          if (res == MessageBoxResult.Yes)
            App.Model.Prj.Analysis.save(App.Model.Prj.ReportName, App.Model.Prj.Notes, App.Model.Prj.SummaryName);
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

    private TreeViewItem? CreateTreeItem(object o)
    {
      TreeViewItem item = new TreeViewItem();
      DirectoryInfo? d = o as DirectoryInfo;
      if (d != null)
        item.Header = d.Name;
      else
      {
        FileInfo? f = o as FileInfo;
        if (f != null)
          item.Header = f.Name;
        else
        {
          DebugLog.log($"could not create tree view item from {o.ToString()}", enLogType.ERROR);
          return null;
        }
      }
      item.Tag = o;
      item.Items.Add(BatInspector.Properties.MyResources.MainWindowMsgLoading);
      item.Foreground = (SolidColorBrush)System.Windows.Application.Current.Resources["colorForeGroundLabel"];
      return item;
    }



    private void updateWavControls()
    {
      List<string> spec = new List<string>();
      foreach (SpeciesInfos si in App.Model.SpeciesInfos)
      {
        if (si.Show)
          spec.Add(si.Abbreviation);
      }
      spec.Add("todo");
      //spec.Add("?");
      spec.Add("---");

      foreach (ctlWavFile ctl in _spSpectrums.Children)
      {
        AnalysisFile? anaF = App.Model.Prj.Analysis.find(ctl.WavName);
        PrjRecord? rec = App.Model.Prj.findRecord(ctl.WavName);
        if ((anaF != null) && (rec != null))
          ctl.updateCallInformations(anaF, rec);
      }

    }


    void worker_ProgressChanged(string pngName)
    {
      if (!Dispatcher.CheckAccess()) // CheckAccess returns true if you're on the dispatcher thread
      {
        Dispatcher.BeginInvoke(new dlgProgress(worker_ProgressChanged));
      }
      this.setStatus(pngName);

    }

    /// <summary>
    /// faster up down scroll for increment 1
    /// </summary>
    /// <param name="up"></param>
    private void incrementControls(bool up)
    {
      if (App.Model.CurrentlyOpen == null)
        return;
      bool append = false;
      string wavName = "";
      if (up)
      {
        if (App.Model.View.StartIdx < (App.Model.View.VisibleFiles.Count - 1))
        {
          App.Model.View.StartIdx++;
          if (_spSpectrums.Children.Count > 0)
          {
            ctlWavFile ctl = (ctlWavFile)_spSpectrums.Children[0];
            ctl.release();
            _spSpectrums.Children.RemoveAt(0);
          }
          if ((App.Model.View.StartIdx + _spSpectrums.Children.Count) <
             (App.Model.View.VisibleFiles.Count - 1))
          {
            wavName = App.Model.View.VisibleFiles[App.Model.View.StartIdx + _spSpectrums.Children.Count + 1];
            append = true;
          }
        }
      }
      else
      {
        if (App.Model.View.StartIdx > 0)
        {
          App.Model.View.StartIdx--;
          wavName = App.Model.View.VisibleFiles[App.Model.View.StartIdx];
          if (_spSpectrums.Children.Count > 0)
          {
            ctlWavFile ctl = (ctlWavFile)_spSpectrums.Children[_spSpectrums.Children.Count - 1];
            ctl.release();
            _spSpectrums.Children.RemoveAt(_spSpectrums.Children.Count - 1);
          }
          append = true;
        }
      }
      if (append)
      {
        PrjRecord? rec = App.Model.CurrentlyOpen.findRecord(wavName);
        if (rec != null)
        {
          AnalysisFile? analysisFile = null;
          if (App.Model.CurrentlyOpen.Analysis != null)
            analysisFile = App.Model.CurrentlyOpen.Analysis.find(rec.File);
          ctlWavFile? ctl = _wavCtls.get("wavCtl append");
          if (ctl != null)
          {
            ctl.setup(analysisFile, rec, this, true, App.Model.CurrentlyOpen.IsBirdPrj, _infoVisible);
            if (up)
              _spSpectrums.Children.Add(ctl);
            else
              _spSpectrums.Children.Insert(0, ctl);
            bool isQuery = App.Model.Query != null;
            initCtlWav(ctl, rec, isQuery);
          }
          else
            DebugLog.log("no more WAV controls in pool available", enLogType.ERROR);
        }
      }
      setPrjHeader(App.Model.View.StartIdx);
    }

    void setPrjHeader(int idx)
    {
      _tbPrj.Header = $"{MyResources.MainWinProjectView} ({idx + 1}/{App.Model.View.VisibleFiles.Count})";
    }

    private void populateControls(int startIdx)
    {
      if (!Dispatcher.CheckAccess()) // CheckAccess returns true if you're on the dispatcher thread
      {
        Dispatcher.BeginInvoke(new dlgOneInt(populateControls), startIdx);
      }
      else if (App.Model.CurrentlyOpen != null)
      {
        App.Model.View.StartIdx = startIdx;
        PrjRecord[] recList = App.Model.CurrentlyOpen.getRecords();
        bool isQuery = App.Model.Query != null;
        Analysis analysis = App.Model.CurrentlyOpen.Analysis;
        if (recList != null)
        {
          foreach (ctlWavFile c in _spSpectrums.Children)
            c.release();
          _spSpectrums.Children.Clear();

          int maxCtl = Math.Min(App.Model.View.VisibleFiles.Count, AppParams.MAX_WAVCTL_COUNT);
          for (int i = 0; i < maxCtl; i++)
          {
            if ((startIdx + i) < App.Model.View.VisibleFiles.Count)
            {
              string wavName = App.Model.View.VisibleFiles[i + startIdx];
              PrjRecord? rec = App.Model.CurrentlyOpen.findRecord(wavName);
              if (rec != null)
              {
                lock (rec)
                {
                  AnalysisFile? analysisFile = null;
                  if ((analysis != null) && (rec != null))
                    analysisFile = analysis.find(rec.File);
                  ctlWavFile? ctl = _wavCtls.get($"wavCtl[{i}]");
                  if (ctl != null)
                  {
                    ctl.setup(analysisFile, rec!, this, true, App.Model.CurrentlyOpen.IsBirdPrj, _infoVisible);
                    DockPanel.SetDock(ctl, Dock.Bottom);
                    _spSpectrums.Children.Add(ctl);
                    initCtlWav(ctl, rec!, isQuery);
                  }
                  else
                  {
                    DebugLog.log("no more WAV controls in pool available", enLogType.ERROR);
                  }
                }
              }
              else
                DebugLog.log($"could not find file {wavName}", enLogType.ERROR);
            }
          }
        }
        setPrjHeader(startIdx);
      }
    }


    void initCtlWav(ctlWavFile ctl, PrjRecord rec, bool fromQuery)
    {
      AnalysisFile? analysis;
      string[] species;
      string fullWavName;
      string wavName;
      string wavFilePath;
      enModel modelType;
      if ((fromQuery) && (App.Model.Query != null))
      {
        fullWavName = Path.Combine(App.Model.SelectedDir, rec.File);
        wavName = Path.GetFileName(fullWavName);
        wavFilePath = App.Model.SelectedDir;
        analysis = App.Model.Query.Analysis.find(rec.File);
        species = App.Model.Query.Species;
        modelType = App.Model.Query.Analysis.ModelType;
      }
      else
      {
        wavFilePath = Path.Combine(App.Model.SelectedDir, App.Model.Prj.WavSubDir);
        wavName = rec.File;
        fullWavName = Path.Combine(App.Model.SelectedDir, rec.File);
        analysis = App.Model.Prj.Analysis.find(rec.File);
        species = App.Model.Prj.Species;
        modelType = App.Model.Prj.Analysis.ModelType;
      }
      ctl.setFileInformations(rec, wavFilePath, analysis, species, modelType, _imgHeight);
      ctl.InfoVisible = !AppParams.Inst.HideInfos;
      ctl.createNewPng();
    }


    public void showStatus()
    {
      string report = "";
      if (App.Model.CurrentlyOpen != null)
      {
        report = !App.Model.CurrentlyOpen.Analysis.IsEmpty ?
                 MyResources.MainWindowMsgReport :
                 MyResources.MainWindow_showStatus_NoReport;

        setStatus($"  [{MyResources.MainWindowFiles}: {App.Model.View.VisibleFiles.Count}/{App.Model.CurrentlyOpen.getRecords().Length} | {report} ]");
      }
    }


    public void updateControls()
    {
      if (App.Model.CurrentlyOpen != null)
      {
        foreach (UIElement it in _spSpectrums.Children)
        {
          ctlWavFile ctl = (ctlWavFile)it;
          PrjRecord? rec = App.Model.CurrentlyOpen.findRecord(ctl.WavName);
          if (rec != null)
            setCheckboxInWavCtl(ctl, rec.Selected);
        }
      }
    }


    async Task  showMsgInUiThread(string title, string msg, bool topmost = false)
    {
      if (_frmMsg != null)
      {
        TimeSpan t = _sw.Elapsed;
        _frmMsg.Owner = this;
        _frmMsg.showMessage(title, msg, topmost);
        _frmMsg.Visibility = Visibility.Visible;
        _frmMsg.Show();
        _frmMsg.Activate();
        await Dispatcher.Yield();
      }
    }

    void showMsg(string title, string msg, bool topmost = false)
    {
      System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
      {
        if (_frmMsg != null)
        {
          TimeSpan t = _sw.Elapsed;
          _frmMsg.showMessage(title, msg, topmost);
          _frmMsg.Visibility = Visibility.Visible;
        }
      }), DispatcherPriority.Send);
    }

    void hideMsg()
    {
      System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
      {
        _frmMsg?.Visibility = Visibility.Hidden;
      }), DispatcherPriority.ContextIdle);
    }

    private void workerPrediction()
    {
      try
      {
        showMsg(BatInspector.Properties.MyResources.msgInformation, BatInspector.Properties.MyResources.MainWindowMsgClassification, true);
        App.Model.evaluate(false);
        App.Model.updateReport();
      }
      catch (Exception ex)
      {
        DebugLog.log("error predicting species: " + ex.ToString(), enLogType.ERROR);
        DebugLog.save();
      }
    }


    private async void _btnFindCalls_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        if ((App.Model.Prj != null) && (App.Model.Prj.Ok))
        {
          if (App.Model.Prj.checkModelType())
          {
            MessageBoxResult res = MessageBoxResult.Yes;
            DebugLog.log("MainWin:BTN 'Find calls' clicked ", enLogType.DEBUG);
            if (App.Model.CurrentlyOpen?.Analysis.IsEmpty == false)
              res = System.Windows.MessageBox.Show(BatInspector.Properties.MyResources.msgMainWinMsgOverwriteReport,
              BatInspector.Properties.MyResources.msgQuestion,
              MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (res == MessageBoxResult.Yes)
            {
              App.Model.View.stopCreatingPngFiles();
              App.Model.Status.State = enAppState.BUSY; 
              await Task.Run(() => { workerPrediction(); });
              DirectoryInfo dir = new DirectoryInfo(App.Model.SelectedDir);
              await initializeProject(dir);
              App.Model.Status.State = enAppState.IDLE;
            }
          }
          else
            System.Windows.MessageBox.Show(BatInspector.Properties.MyResources.msgWrongModelType, MyResources.msgInformation, MessageBoxButton.OK, MessageBoxImage.Exclamation);

        }
        else
        {
          System.Windows.MessageBox.Show(BatInspector.Properties.MyResources.msgPleaseOpenProjectFirst, MyResources.msgInformation, MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }
      }
      catch (Exception ex)
      {
        App.Model.Status.State = enAppState.IDLE;
        DebugLog.log("MainWin:BTN 'Find calls' failed: " + ex.ToString(), enLogType.ERROR);
      }
    }


    public void initDataGridSource()
    {
      _dgData.EnableColumnVirtualization = true;
      _dgData.ItemsSource = null;
      _dgData.ItemsSource = App.Model.View.getListSource();

      for (int i = 0; i < _dgData.Columns.Count; i++)
      {
        _dgData.Columns[i].IsReadOnly = true;
      }
    }

    private void _tbMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (_tbPrj.IsSelected && (_oldTab != "Prj"))
      {
        _oldTab = "Prj";
        try
        {
          //    if (App.Model.CurrentlyOpen != null)
          //      buildWavFileList(false);
          DebugLog.log("TAB 'Project' got selected", enLogType.DEBUG);
        }
        catch (Exception ex)
        {
          DebugLog.log("TAB 'Project' selection failed:" + ex.ToString(), enLogType.ERROR);
        }

      }

      else if (_tbReport.IsSelected && (_oldTab != "Report"))
      {
        _oldTab = "Report";
        try
        {
          if (App.Model.CurrentlyOpen != null)
            buildWavFileList(false);
          else
            _dgData.ItemsSource = null;
          DebugLog.log("TAB 'Report' got selected", enLogType.DEBUG);
        }
        catch (Exception ex)
        {
          DebugLog.log("TAB 'Report' selection failed:" + ex.ToString(), enLogType.ERROR);
        }
      }

      else if (_tbSum.IsSelected && (_oldTab != "Sum"))
      {
        _oldTab = "Sum";
        try
        {
          if ((App.Model.CurrentlyOpen != null) && (App.Model.CurrentlyOpen.Analysis.Summary != null))
            _dgSum.ItemsSource = App.Model.CurrentlyOpen.Analysis.Summary;
          DebugLog.log("TAB 'Summary' selected", enLogType.DEBUG);
        }
        catch (Exception ex)
        {
          DebugLog.log("TAB 'Summary' selection failed:" + ex.ToString(), enLogType.ERROR);
        }
      }

      else if (_tbScatter.IsSelected && (_oldTab != "Scat"))
      {
        _oldTab = "Scat";
      }
    }

    private void Window_Closing(object sender, CancelEventArgs e)
    {
      DebugLog.log("closing application", enLogType.DEBUG);
      App.Model.View.stopCreatingPngFiles();
      checkSavePrj();
        _frmColorMap?.Close();
        _frmZoom?.Close();
        _frmFilter?.Close();
        _frmScript?.Close();
        _frmAbout?.Close();
        _frmSettings?.Close();
        _frmCreatePrj?.Close();
        _frmCreateReport?.Close();
        _frmWavFile?.Close();
        _frmDebug?.Close();
        _frmQuery?.Close();
        _frmCleanup?.Close();
        _frmMsg?.Close();
      _frmExp?.Close();
      _frmExportData?.Close();
      DebugLog.save();
    }

    private void _btnFilter_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        if (_frmFilter == null)
          _frmFilter = new FrmFilter(App.Model.Filter, populateFilterComboBoxes);
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

    private static void setCheckboxInWavCtl(ctlWavFile ctl, bool check)
    {
      ctl._cbSel.IsChecked = check;
    }

    private void _btnSave_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        if ((App.Model.Prj != null) && (App.Model.Prj.Notes != null))
        {
          App.Model.Prj.Notes = _ctlPrjInfo._tbNotes.Text;
          App.Model.Prj.writePrjFile();
        }
        if ((App.Model.ZoomView != null) && (App.Model.ZoomView.Waterfall != null) &&
           ((_ctlZoom?._gradientRange != AppParams.Inst.GradientRange) ||
            (App.Model.ZoomView.Waterfall.BlackLevel != AppParams.Inst.BlackLevel))
          )
        {
          MessageBoxResult res = System.Windows.MessageBox.Show(BatInspector.Properties.MyResources.MsgDisplayContrast, MyResources.msgQuestion, MessageBoxButton.YesNo, MessageBoxImage.Question);
          if ((res == MessageBoxResult.Yes) && (_ctlZoom != null))
          {
            AppParams.Inst.GradientRange = _ctlZoom._gradientRange;
            AppParams.Inst.BlackLevel = App.Model.ZoomView.Waterfall.BlackLevel;           
          }
        }
        App.Model.saveSettings();
        if ((App.Model != null) && (App.Model.Prj != null) && (App.Model.Prj.Analysis != null) &&
          (!App.Model.Prj.Analysis.IsEmpty))
          App.Model.Prj.Analysis.save(App.Model.Prj.ReportName, App.Model.Prj.Notes!, App.Model.Prj.SummaryName);
        DebugLog.log("MainWin:BTN 'save' clicked", enLogType.DEBUG);
        foreach (ctlWavFile c in _spSpectrums.Children)
          c.update();
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
          _frmAbout = new FrmAbout("Version " + AppParams.AppVersion);
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


    private void _btnColorPalette_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        if (_frmColorMap == null)
          _frmColorMap = new FrmColorMap();
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


    private void _btnSettings_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        if (_frmSettings == null)
          _frmSettings = new frmSettings(AppParams.Inst);
        else
          _frmSettings.update(AppParams.Inst);
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
          _frmWavFile = new frmWavFile();
        _frmWavFile.Show();
        _frmWavFile.Visibility = Visibility.Visible;
        _frmWavFile.Topmost = true;  // important
        _frmWavFile.Topmost = false; // important
        _frmWavFile.Focus();         // important
                                     //    _frmWavFile.Topmost = true;
        DebugLog.log("MainWin:BTN 'WavTool' clicked", enLogType.DEBUG);
      }
      catch (Exception ex)
      {
        DebugLog.log("MainWin:BTN 'WavTool' failed:" + ex.ToString(), enLogType.ERROR);
      }
    }

    private void Content_rendered(object sender, System.EventArgs e)
    {
      if ((_ctlZoom != null) && (App.Model.SelectedDir != null))
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

    static void setMouseStatus()
    {
      System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
      {
        if (App.Model.Status.State != enAppState.IDLE)
          Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
        else
          Mouse.OverrideCursor = null;
      }));
    }

    private async void timer_Tick(object? sender, EventArgs? e)
    {
      if (App.Model.Status.Msg != null)
      {
        showMsg(BatInspector.Properties.MyResources.msgInformation, App.Model.Status.Msg);
        _lblStatus.Text = App.Model.Status.Msg;
        App.Model.Status.Msg = null;
      }

      if (App.Model.UpdateUi)
      {
        if ((App.Model.Prj != null) && (App.Model.Prj.Analysis != null))
          App.Model.Prj.Analysis.updateSpeciesCount();
        //        _tbReport_GotFocus(null, null);
        updateWavControls();
        App.Model.UpdateUi = false;
      }

      if ((App.Model.Prj != null) && (App.Model.Prj.ReloadInGui))
      {
        if (App.Model.Prj.Ok && (App.Model.Prj.Analysis != null))
        {
          _spSpectrums.Children.Clear();
          DirectoryInfo dir = new DirectoryInfo(App.Model.SelectedDir);
          await initializeProject(dir);
        }
        App.Model.Prj!.ReloadInGui = false;
      }

      switch (App.Model.Status.State)
      {
        default:
        case enAppState.IDLE:
          if (!_btnCreatePrj.IsEnabled)
          {
            _btnCreatePrj.IsEnabled = true;
            _btnCreatePrj.Opacity = 1.0;
          }
          hideMsg();
          break;

        case enAppState.IMPORT_PRJ:
          if (_btnCreatePrj.IsEnabled)
          {
            _btnCreatePrj.IsEnabled = false;
            _btnCreatePrj.Opacity = 0.25;
          }
          break;

        case enAppState.BUSY:
          break;
      }

      if (_switchTabToPrj)
      {
        _tbPrj.IsSelected = true;
        _switchTabToPrj = false;
      }
      setMouseStatus();

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
          _frmCreateReport = new frmCreateReport();
        Filter.populateFilterComboBox(_frmCreateReport._ctlReport._cbFilter);
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


    private void _grdSplitterH_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
    {
      AppParams.Inst.LogControlHeight = _grdMain.RowDefinitions[3].Height.Value;
    }

    private void _grdSplitterV_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
    {
      if (_grdCtrl.ColumnDefinitions[0].Width.Value < 50)
        _grdCtrl.ColumnDefinitions[0].Width = new GridLength(50);

      AppParams.Inst.WidthFileSelector = _grdCtrl.ColumnDefinitions[0].Width.Value;
    }

    private void _btnScript_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        if (_frmScript == null)
          _frmScript = new FrmScript(populateToolsMenu, debugScript);
        _frmScript.Show();
        _frmScript.Activate();
        _frmScript.Topmost = true;
        _frmScript.Topmost = false;
        _frmScript.Focus();
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
        App.Model.cancelScript();
        DebugLog.log("MainWin:BTN 'cancel Script' clicked", enLogType.DEBUG);
      }
      catch (Exception ex)
      {
        DebugLog.log("MainWin:BTN 'Cancel Script' failed: " + ex.ToString(), enLogType.ERROR);
      }
    }

    private void debugScript(string script)
    {
      try
      {
        if (_frmDebug == null)
          _frmDebug = new frmDebug();
        ScriptItem? s = AppParams.Inst.ScriptInventory.getScriptInfo(script);
        List<ParamItem>? pars = null;
        if (s != null)
        {
          if (s.IsTool)
          {
            pars = new List<ParamItem>();
            pars.Add(new ParamItem("VAR_FILE_NAME", enParamType.FILE, "VAR_FILE_NAME"));
          }
          else
            pars = s.Parameter;
        
          _frmDebug.Visibility = Visibility.Visible;
          _frmDebug.setup(Path.Combine(AppParams.Inst.ScriptInventoryPath, script), pars);
          DebugLog.log("MainWin:BTN 'Debug' clicked", enLogType.DEBUG);
          _frmDebug.Show();
/*          _frmDebug.Activate();
          _frmDebug.Topmost = true;
          _frmDebug.Topmost = false;
          _frmDebug.Focus(); */
        }
      }
      catch (Exception ex)
      {
        DebugLog.log("MainWin:debugScript() failed: " + ex.ToString(), enLogType.ERROR);
      }
    }

    void setExpertMode(bool on)
    {
      if (on)
      {
        _tbScatter.Visibility = Visibility.Visible;
        _tbStatistic.Visibility = Visibility.Visible;
        _tbMySql.Visibility = Visibility.Visible;
        _tbReport.Visibility = Visibility.Visible;
        _btnScript.Visibility = Visibility.Visible;
        _btnCancelScript.Visibility = Visibility.Visible;
        _sepScript.Visibility = Visibility.Visible;
        _btnWavTool.Visibility = Visibility.Visible;
        _btnRecorder.Visibility = Visibility.Visible;
      }
      else
      {
        _tbScatter.Visibility = Visibility.Collapsed;
        _tbStatistic.Visibility = Visibility.Collapsed;
        _tbMySql.Visibility = Visibility.Collapsed;
        _tbReport.Visibility = Visibility.Collapsed;
        _btnWavTool.Visibility = Visibility.Collapsed;
        _sepScript.Visibility= Visibility.Collapsed;
        _btnCancelScript.Visibility = Visibility.Collapsed;
        _btnScript.Visibility = Visibility.Collapsed;
        _btnRecorder.Visibility= Visibility.Collapsed;
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

    private void _scrollPrj_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
      _mouseIsDownOnScrollPrj = true;
    }

    private void _scrollList_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
    //  _mouseIsDownOnScrollList = true;
    }


    private void _scrollPrj_ValueChanged(object? sender, RoutedPropertyChangedEventArgs<double>? e)
    {
      try
      {
        double[] sec = new double[5];
        if (!_mouseIsDownOnScrollPrj)
        {
          if (App.Model.CurrentlyOpen == null)
            return;

          if (_scrollBarPrjPos == _scrollPrj.Value)
            return;

          double diff = _scrollPrj.Value - _scrollBarPrjPos;
          if (_spSpectrums.Children.Count < 2)
            populateControls((int)_scrollPrj.Value);
          else if ((diff < 2) && (diff > 0) && (_scrollBarPrjPos < _scrollPrj.Maximum))
            incrementControls(true);
          else if ((diff > -2) && (diff < 0) && (_scrollBarPrjPos < _scrollPrj.Maximum))
            incrementControls(false);
          else
            populateControls((int)_scrollPrj.Value);
          _scrollBarPrjPos = _scrollPrj.Value;
          DebugLog.log($"New scrollbar position: {_scrollBarPrjPos}", enLogType.DEBUG);
        }
      }
      catch (Exception ex)
      {
        DebugLog.log(ex.ToString(), enLogType.ERROR);
      }
    }

    private void _scrollPrj_MouseUp(object sender, MouseButtonEventArgs e)
    {
      try
      {
        _mouseIsDownOnScrollPrj = false;
        _scrollPrj_ValueChanged(null, null);
      }
      catch (Exception ex)
      {
        DebugLog.log(ex.ToString(), enLogType.ERROR);
      }
    }

    private void DropdownButton_Checked(object sender, RoutedEventArgs e)
    {
      var menu = ((ToggleButton)sender).ContextMenu;
      menu.PlacementTarget = sender as ToggleButton;
      menu.Placement = PlacementMode.Bottom;
      menu.IsOpen = true;
    }

    private void ContextMenu_Closed(object sender, RoutedEventArgs e)
    {
      ((ToggleButton)((System.Windows.Controls.ContextMenu)sender).PlacementTarget).IsChecked = false;
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

    private void _mnReportError_Click(object sender, RoutedEventArgs e)
    {
      MessageBoxResult res = System.Windows.MessageBox.Show(MyResources.MsgReportError, MyResources.msgQuestion, MessageBoxButton.YesNo, MessageBoxImage.Question);
      if (res == MessageBoxResult.Yes)
      {
        string subject = $"Error Report BatInspector V{AppParams.AppVersion}";
        DebugLog.save();
        string[] logs = App.Model.getLastLogs();
        if ((logs != null) && (logs.Length > 0))
          App.Model.sendEmail(AppParams.ERROR_RECIPIENT, subject, MyResources.msgErrorEmail, logs);
      }
    }

    private void _mnReportInstall_Click(object sender, RoutedEventArgs e)
    {
      MessageBoxResult res = System.Windows.MessageBox.Show(MyResources.MsgInstallReport, MyResources.msgQuestion, MessageBoxButton.YesNo, MessageBoxImage.Question);
      if (res == MessageBoxResult.Yes)
      {
        string subject = $"Installation Report BatInspector V{AppParams.AppVersion}";
        DebugLog.save();
        string[] logs = App.Model.getInstallationLogs();
        if ((logs != null) && (logs.Length > 0))
          App.Model.sendEmail(AppParams.ERROR_RECIPIENT, subject, MyResources.msgInstallationEmail, logs);
      }
    }

    private void _btnCreatePrj_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        if (_frmCreatePrj == null)
          _frmCreatePrj = new FrmCreatePrj();
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


    private void _btnQuery_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        if (_frmQuery == null)
          _frmQuery = new FrmQuery();
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
        while ((dep != null) && !(dep is System.Windows.Controls.DataGridCell))
          dep = VisualTreeHelper.GetParent(dep);

        if (dep == null)
          return;

        ReportItemBd2? it = null;
        if (dep is System.Windows.Controls.DataGridCell)
        {
          System.Windows.Controls.DataGridCell cell = (System.Windows.Controls.DataGridCell)dep;
          // navigate further up the tree
          while ((dep != null) && !(dep is System.Windows.Controls.DataGridRow))
            dep = VisualTreeHelper.GetParent(dep);

          System.Windows.Controls.DataGridRow? row = dep as System.Windows.Controls.DataGridRow;
          if(row != null)
            it = row.DataContext as ReportItemBd2;
        }

        if ((App.Model.CurrentlyOpen != null) &&  (it != null))
        {
          int.TryParse(it.CallNr, out int callNr);
          AnalysisFile? analysis = App.Model.CurrentlyOpen.Analysis.find(it.FileName);
          string fileName = Path.GetFileName(it.FileName);
          string? wavPath = Path.GetDirectoryName(App.Model.CurrentlyOpen.getFullFilePath(it.FileName));
          if((analysis != null) && (wavPath != null))
            setZoom(fileName, analysis, wavPath, null, App.Model.CurrentlyOpen.Analysis.ModelType);
          changeCallInZoom(callNr - 1);
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
          _frmCleanup = new frmCleanup();
        _frmCleanup.Show();
        DebugLog.log("MainWin:BTN 'Clean Up' clicked", enLogType.DEBUG);
      }
      catch (Exception ex)
      {
        DebugLog.log("MainWin:BTN 'Clean Up' failed: " + ex.ToString(), enLogType.ERROR);
      }
    }


    private void populateToolsMenu()
    {
      _mnuToolsItems.Items.Clear();
      if ((AppParams.Inst.ScriptInventory != null) && (AppParams.Inst.ScriptInventory.Scripts != null))
      {
        foreach (ScriptItem s in AppParams.Inst.ScriptInventory.Scripts)
        {
          if (!s.IsTool && s.IsInMenue)
          {
            System.Windows.Controls.MenuItem m = new System.Windows.Controls.MenuItem();
            m.Header = s.Description;
            m.Tag = s.Name;
            m.Click += _mnTool1_Click;
            _mnuToolsItems.Items.Add(m);
          }
        }
      }
    }

    private void _mnTool1_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        DebugLog.log("BTN custom tool pressed", enLogType.DEBUG);
        System.Windows.Controls.MenuItem m = (System.Windows.Controls.MenuItem)sender;
        string script = (string)m.Tag;
        ScriptItem? item = ScriptRunner.getScript(script);
        if (item != null)
        {
          if (item.Parameter.Count > 0)
          {
            string winTitle = BatInspector.Properties.MyResources.frmScriptParamTitle + ": " + script;
            frmScriptParams frm = new frmScriptParams(winTitle, item.Parameter);
            frm.ShowDialog();
            if (frm.DialogResult == true)
              App.Model.Scripter.runScript(script, frm.ParameterValues);
          }
          else
            App.Model.Scripter.runScript(script);
        }
      }
      catch (Exception ex)
      {
        DebugLog.log("Error BTN custom tool: " + ex.ToString(), enLogType.ERROR);
      }
    }

    // https://stackoverflow.com/questions/16245706/check-for-device-change-add-remove-events/16245901#16245901

    protected override void OnSourceInitialized(EventArgs e)
    {
      base.OnSourceInitialized(e);
      HwndSource source = (HwndSource)PresentationSource.FromVisual(this);
      source.AddHook(WndProc);
    }


    /// <summary>
    /// checks for new USB devices
    /// </summary>
    /// <param name="hwnd"></param>
    /// <param name="msg"></param>
    /// <param name="wParam"></param>
    /// <param name="lParam"></param>
    /// <param name="handled"></param>
    /// <returns></returns>
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
        if ((App.Model.Prj != null) && App.Model.Prj.Ok)
        {
          FrmRecovery frm = new FrmRecovery(App.Model.Prj.Name);
          bool res = frm.ShowDialog() == true;
          if (res)
            App.Model.Prj.recovery(frm._cbDel.IsChecked == true, frm._cbChanged.IsChecked == true);
        }
        else
        {
          System.Windows.MessageBox.Show(MyResources.msgPleaseOpenProjectFirst, MyResources.msgInformation, MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }
      }
      catch (Exception ex)
      {
        DebugLog.log("Main:BTN Recovery click failed: " + ex.ToString(), enLogType.ERROR);
      }
    }

    private void _btnRecorder_Click(object sender, RoutedEventArgs e)
    {
      FrmSetupRecorder frm = new FrmSetupRecorder(App.Model.Recorder);
      frm.Owner = this;
      frm.Show();
    }

    private void _btnExpert_Click(object sender, RoutedEventArgs e)
    {
      AppParams.Inst.ExpertMode = !AppParams.Inst.ExpertMode;
      setExpertMode(AppParams.Inst.ExpertMode);
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
    private void MainWindowStateChangeRaised(object? sender, EventArgs? e)
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

    private void _tbStatistic_GotFocus(object sender, RoutedEventArgs e)
    {
      App.Model.MySQL.disconnect();
      _ctlStatistic.createPlot();
    }

    private int getScrollDist()
    {
      if (_imgHeight >= 256)
        return 2;
      else if (_imgHeight >= 128)
        return 3;
      else
        return 4;
    }

    public void _scrollPrj_MouseWheel(object sender, MouseWheelEventArgs e)
    {
      int idx = -e.Delta / 100;
      if (idx > 0)
      {
        if (_scrollPrj.Value + idx <= _scrollPrj.Maximum)
          _scrollPrj.Value += idx;
        else
          _scrollPrj.Value = _scrollPrj.Maximum;
      }
      else if (idx < 0)
      {
        if (_scrollPrj.Value + idx >= 0)
          _scrollPrj.Value += idx;
        else
          _scrollPrj.Value = 0;
      }
      _mouseIsDownOnScrollPrj = false;
    }

    private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
      if (_tbPrj.IsSelected)
      {
        _tbPrj.Focus();
        switch (e.Key)
        {
          case Key.Down:
            if (_scrollPrj.Value < _scrollPrj.Maximum)
              _scrollPrj.Value += 1;
            break;
          case Key.PageDown:
            if (_scrollPrj.Value < _scrollPrj.Maximum - getScrollDist())
              _scrollPrj.Value += getScrollDist();

            break;
          case Key.PageUp:
            if (_scrollPrj.Value > getScrollDist())
              _scrollPrj.Value -= getScrollDist();
            else
              _scrollPrj.Value = 0;
            break;
          case Key.Up:
            if (_scrollPrj.Value > 0)
              _scrollPrj.Value -= 1;
            break;

          default:
            _ctlLog._tbCmd.Focus();
            break;
        }
      }
      else if (_tbReport.IsSelected)
      {
        _tbReport.Focus();
        switch (e.Key)
        {
          case Key.PageDown:
          case Key.Down:
            if (_dgData.SelectedIndex < (_dgData.Items.Count - 20))
              _dgData.SelectedIndex += 20;
            _dgData.ScrollIntoView(_dgData.Items[_dgData.SelectedIndex]);
            break;
          case Key.PageUp:
          case Key.Up:
            if (_dgData.SelectedIndex > 20)
              _dgData.SelectedIndex -= 20;
            else
              _dgData.SelectedIndex = 0;
            _dgData.ScrollIntoView(_dgData.Items[_dgData.SelectedIndex]);
            break;

          default:
            _ctlLog._tbCmd.Focus();
            break;
        }
      }
    }

    private void _btnCollapse_Click(object sender, RoutedEventArgs e)
    {
      collapseTreeView(!_treeViewCollaped);
    }


    private void collapseTreeView(bool collapse)
    {
      _treeViewCollaped = collapse;
      if (_treeViewCollaped)
      {
        _btnCollapse.Content = ">";
        _grdCtrl.ColumnDefinitions[0].Width = new GridLength(20);
        _ctlPrjInfo.Visibility = Visibility.Collapsed;
        _lblProjectSelect.Content = "";
        //        _spTreeView.Background = (SolidColorBrush)App.Current.Resources["colorBackGroundWindow"];
        _spPrjInfo.Background = (SolidColorBrush)App.Current.Resources["colorBackGroundWindow"];
        _trvStructure.Visibility = Visibility.Collapsed;
        _rect.Visibility = Visibility.Visible;
        _grdCtrl.ColumnDefinitions[1].Width = new GridLength(0);
        _btnOpenPrj.Visibility = Visibility.Visible;
      }
      else
      {
        _btnCollapse.Content = "<";
        _grdCtrl.ColumnDefinitions[0].Width = new GridLength(AppParams.Inst.WidthFileSelector);
        _ctlPrjInfo.Visibility = Visibility.Visible;
        _lblProjectSelect.Content = MyResources.MainSelectFolder;
        _trvStructure.Background = (SolidColorBrush)App.Current.Resources["colorBackGround"];
        //       _spTreeView.Background = (SolidColorBrush)App.Current.Resources["colorBackGround"];
        _spPrjInfo.Background = (SolidColorBrush)App.Current.Resources["colorBackGround"];
        _trvStructure.Visibility = Visibility.Visible;
        _rect.Visibility = Visibility.Collapsed;
        _grdCtrl.ColumnDefinitions[1].Width = new GridLength(8);
        _btnOpenPrj.Visibility = Visibility.Hidden;
      }
    }



    private void _btnOpenPrj_Click(object sender, RoutedEventArgs e)
    {
      collapseTreeView(false);
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {

    }

    private void _tbMySql_GotFocus(object sender, RoutedEventArgs e)
    {
      collapseTreeView(true);
    }

    private void _tbScatter_GotFocus(object sender, RoutedEventArgs e)
    {
    }

    private void _tbSum_GotFocus(object sender, RoutedEventArgs e)
    {
    }

    private void _tbReport_GotFocus(object sender, RoutedEventArgs e)
    {
    }

    private void _tbPrj_GotFocus(object sender, RoutedEventArgs e)
    {
    }

    private void _btnExport_Click(object sender, RoutedEventArgs e)
    {
      if (_frmExportData != null)
      {
        _frmExportData = new frmExportData();
        _frmExportData.setup();
      }
      _frmExportData?.Show();
    }
  }

  public enum enWinType
  {
    ZOOM,
  }

  public delegate void dlgcloseChildWindow(enWinType w);
}
