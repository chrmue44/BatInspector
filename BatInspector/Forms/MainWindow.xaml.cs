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
using System.Reflection;
using BatInspector.Controls;
using libParser;
using BatInspector.Properties;
using System.Windows.Threading;
using System.Linq;

namespace BatInspector.Forms
{

  delegate void dlgProgress(string pngName);
  delegate void dlgInitPrj(DirectoryInfo dir);
  delegate void dlgInitQuery(FileInfo file);
  delegate void dlgOneInt(int a);

  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    public const int MAX_IMG_HEIGHT = 256;
    const int MAX_IMG_WIDTH = 512;

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
    frmExport _frmExp = null;
    int _imgHeight = MAX_IMG_HEIGHT;
    FrmZoom _frmZoom = null;
    CtrlZoom _ctlZoom = null;
    TabItem _tbZoom = null;
    frmSpeciesData _frmSpecies = null;
    Thread _workerPredict = null;
    Thread _workerStartup = null;
    Thread _pngThread = null;
    System.Windows.Threading.DispatcherTimer _timer;
    bool _switchTabToPrj = false;
    Stopwatch _sw = new Stopwatch();
    DirectoryInfo _projectDir;
    FileInfo _queryFile;
    string _oldTab = "";
    Pool<ctlWavFile> _wavCtls;

    double _scrollBarPrjPos = 0;
    bool _mouseIsDownOnScrollPrj = false;
    double _scrollBarListPos = 0;
    bool _mouseIsDownOnScrollList = false;
    bool _treeViewCollaped = false;
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
       
      // TODO find a working way to limit the window size
      if ((this.Left + this.Width) > WpfScreen.Primary.WorkingArea.Width)
        this.Width = WpfScreen.Primary.WorkingArea.Width - this.Left;
      if ((this.Top + this.Height) > WpfScreen.Primary.WorkingArea.Height)
        this.Height = WpfScreen.Primary.WorkingArea.Height - this.Top;
              
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
            _trvStructure.Items.Add(CreateTreeItem(batDataDir));
          }
        }
      }
      else
      {
        foreach (DriveInfo driveInfo in drives)
        {
          DirectoryInfo dir = new DirectoryInfo(driveInfo.Name);
          _trvStructure.Items.Add(CreateTreeItem(dir));
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
              foreach (DirectoryInfo subDir in expandedDir.GetDirectories().OrderBy(f => f.Name))
              {
                TreeViewItem childItem = CreateTreeItem(subDir);
                item.Items.Add(childItem);
                string prjFile = Project.containsProject(subDir);
                if (prjFile != "")
                {
                  ModelParams[] modelParams = Project.readModelParams(prjFile);
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
      TreeViewItem item = e.Source as TreeViewItem;

    }

    public void TreeViewItem_Selected(object sender, RoutedEventArgs e)
    {
      BackgroundWorker worker = new BackgroundWorker();
      TreeViewItem item = e.Source as TreeViewItem;
      DirectoryInfo dir = item.Tag as DirectoryInfo;
      _sw.Restart();
      _spSpectrums.Children.Clear();
      if ((dir != null) && (Project.containsProject(dir) != ""))
      {
        _ctlPrjBtn.initFileButton(false);
        _tbSum.Visibility = Visibility.Visible;
        collapseTreeView(true);
        worker.DoWork += delegate (object s, DoWorkEventArgs args)
        {
          DirectoryInfo d = (DirectoryInfo)args.Argument;
          initializeProject(d);
        };
        worker.RunWorkerAsync(dir);
      }
      else
      {
        FileInfo file = item.Tag as FileInfo;
        if ((file != null) && Query.isQuery(file))
        {
          _ctlPrjBtn.initFileButton(true);
          _tbSum.Visibility = Visibility.Collapsed;
          worker.DoWork += delegate (object s, DoWorkEventArgs args)
          {
            FileInfo f = (FileInfo)args.Argument;
            initializeQuery(f);
          };
          worker.RunWorkerAsync(file);
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
        case enWinType.BAT:
          _frmSpecies = null;
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
        ctlWavFile ctl = ui as ctlWavFile;
        ctl.setHeight(_imgHeight);
      }
    }

    void initializeQuery(FileInfo file)
    {
      if (!Dispatcher.CheckAccess()) // CheckAccess returns true if you're on the dispatcher thread
      {
        Dispatcher.BeginInvoke(new dlgInitQuery(initializeQuery), file);
      }
      else
      {
        _queryFile = file;
        _projectDir = null;
        App.Model.Status.State = enAppState.OPEN_PRJ;
        App.Model.Busy = true;
        setMouseStatus();

        DebugLog.log("start to open query", enLogType.DEBUG);
        showMsg(BatInspector.Properties.MyResources.msgInformation, MyResources.MainWindowMsgOpenQuery);
        checkSavePrj();
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
            AnalysisFile newAnalysis = App.Model.CurrentlyOpen?.Analysis.find(fName);
            PrjRecord rec = App.Model.CurrentlyOpen?.findRecord(fName);
            ctl.updateCallInformations(newAnalysis, rec);
            if (ctl.Analysis == App.Model.ZoomView.Analysis)
              _ctlZoom.updateManSpecies();
            break;
          }
        } 
      }
    }

    private void createPngImages()
    {
      App.Model.View.createPngFiles(App.Model.ColorTable);
      DebugLog.log("generation of PNGs finished", enLogType.INFO);
    }

    private void initProjectAsync()
    {
      if (!Dispatcher.CheckAccess()) // CheckAccess returns true if you're on the dispatcher thread
      {
        Dispatcher.BeginInvoke(new dlgVoid(initProjectAsync));
      }
      else
      {
        try
        {
          App.Model.initProject(_projectDir, true);
          if ((App.Model.Prj != null) && App.Model.Prj.Ok)
          {
            _scrollPrj.Minimum = 0;
            _scrollBarPrjPos = 0;
            _scrollPrj.Maximum = App.Model.Prj.Records.Length - 1;
            _scrollBarListPos = 0;
            // TODO set scroll button size
            _ctlPrjInfo.setup(App.Model.Prj);
            _lblPrj.Content = MyResources.ctlProjectInfo + " [" + Path.GetFileNameWithoutExtension(App.Model.Prj.Name) + "]";
            _ctlScatter.initPrj();
            _switchTabToPrj = true;
            buildWavFileList(false);
            if ((_pngThread != null) && (_pngThread.IsAlive))
            {
              DebugLog.log("abort background generation of PNGs", enLogType.INFO);
              _pngThread.Abort();
            }
            _pngThread = new Thread(createPngImages);
            _pngThread.Start();

          }
        }
        catch(Exception ex)
        {
          DebugLog.log("Error opening project: " + ex.ToString(), enLogType.ERROR);
        }
      }
    }

    private void initQueryAsync()
    {
      if (!Dispatcher.CheckAccess()) // CheckAccess returns true if you're on the dispatcher thread
      {
        Dispatcher.BeginInvoke(new dlgVoid(initQueryAsync));
      }
      else
      {
        try
        {
          App.Model.initQuery(_queryFile);
          if (App.Model.Query!= null) 
          {
            _scrollPrj.Minimum = 0;
            _scrollBarPrjPos = 0;
            _scrollPrj.Maximum = App.Model.Query.Records.Length - 1;
            _scrollBarListPos = 0;
            // TODO set scroll button size
            _lblPrj.Content = "QUERY:" + " [" + Path.GetFileNameWithoutExtension(App.Model.Query.Name) + "]";
            if (_frmQuery == null)
              _frmQuery = new FrmQuery();
            _frmQuery.initFieldsFromQuery();

            _switchTabToPrj = true;
            buildWavFileList(false);
            if (App.Model.Query != null)
              showStatus();
          }
        }
        catch (Exception ex)
        {
          DebugLog.log("Error opening project: " + ex.ToString(), enLogType.ERROR);
        }
      }
    }

    public void toggleCallInfo()
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

    public void initializeProject(DirectoryInfo dir)
    {
      if (!Dispatcher.CheckAccess()) // CheckAccess returns true if you're on the dispatcher thread
      {
        Dispatcher.BeginInvoke(new dlgInitPrj(initializeProject), dir);
      }
      else
      {
        _projectDir = dir;
        _queryFile = null;
        App.Model.Status.State = enAppState.OPEN_PRJ;
        App.Model.Busy = true;
        setMouseStatus();
        DebugLog.log("start to open project", enLogType.DEBUG);
        showMsg(MyResources.msgInformation, BatInspector.Properties.MyResources.MainWindowMsgOpenPrj);
        _lblProject.Text = BatInspector.Properties.MyResources.MainWindowMsgOpenPrj;
        _scrollPrj.Value = 0;
        checkSavePrj();
        setStatus("");
        _workerStartup = null;
        _tbPrj.Focus();
      }
    }


    
    public void buildWavFileList(bool selectedOnly, Filter filter = null, FilterItem filterItem = null)
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
      }
      else if(_tbReport.IsSelected) 
      {
        if(App.Model.View.populateList(filter, filterItem))
          initDataGridSource();
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



    public void setZoom(string name, AnalysisFile analysis, string wavFilePath, ctlWavFile ctlWav, enModel modelType)
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
          _ctlZoom.setup(analysis, wavFilePath, App.Model.CurrentlyOpen.Species, ctlWav, openExportWindow, modelType);
        else
          _ctlZoom.setup(analysis, wavFilePath, null, null, openExportWindow, modelType);
        _tbZoom.Header = "Zoom: " + Path.GetFileName(name);
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
      if (App.Model.Prj != null)
      {
        if (App.Model.Prj.Ok && App.Model.Prj.Analysis.Changed && (App.Model.Prj.Analysis.Files.Count > 0))
        {
          MessageBoxResult res = MessageBox.Show(MyResources.msgSaveBeforeClose, MyResources.msgQuestion, MessageBoxButton.YesNo, MessageBoxImage.Question);
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
        AnalysisFile anaF = App.Model.Prj.Analysis.find(ctl.WavName);
        PrjRecord rec = App.Model.Prj.findRecord(ctl.WavName);
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
          ctlWavFile ctl = _spSpectrums.Children[0] as ctlWavFile;
          ctl.release();
          _spSpectrums.Children.RemoveAt(0);
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
          ctlWavFile ctl = _spSpectrums.Children[_spSpectrums.Children.Count - 1] as ctlWavFile;
          ctl.release();
          _spSpectrums.Children.RemoveAt(_spSpectrums.Children.Count - 1);
          append = true;
        }
      }
      Stopwatch sw = new Stopwatch();
      if (append)
      {
        sw.Start();
        PrjRecord rec = App.Model.CurrentlyOpen.findRecord(wavName);
        AnalysisFile analysisFile = null;
        if (App.Model.CurrentlyOpen.Analysis != null)
          analysisFile = App.Model.CurrentlyOpen.Analysis.find(rec.File);
        ctlWavFile ctl = _wavCtls.get(wavName); 
        ctl.setup(analysisFile, rec, this, true);
        if (up)
          _spSpectrums.Children.Add(ctl);
        else
          _spSpectrums.Children.Insert(0, ctl);
        bool isQuery = App.Model.Query != null;
        initCtlWav(ctl, rec, isQuery);
        sw.Stop();
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
      else
      {
        App.Model.View.StartIdx = startIdx;
        PrjRecord[] recList = App.Model.CurrentlyOpen?.getRecords();
        bool isQuery = App.Model.Query != null;
        Analysis analysis = App.Model.CurrentlyOpen.Analysis;
        if (recList != null)
        {
          foreach (ctlWavFile c in _spSpectrums.Children)
            c.release();
          _spSpectrums.Children.Clear();

          int maxCtl = Math.Min(App.Model.View.VisibleFiles.Count, 6);
          for (int i = 0; i < maxCtl; i++)
          {
            if ((startIdx + i) < App.Model.View.VisibleFiles.Count)
            {
              string wavName = App.Model.View.VisibleFiles[i + startIdx];
              PrjRecord rec = App.Model.CurrentlyOpen.findRecord(wavName);
              lock (rec)
              {
                AnalysisFile analysisFile = null;
                if ((analysis != null) && (rec != null))
                  analysisFile = analysis.find(rec.File);
                ctlWavFile ctl = _wavCtls.get(wavName);
                ctl.setup(analysisFile, rec, this, true);
                DockPanel.SetDock(ctl, Dock.Bottom);
                _spSpectrums.Children.Add(ctl);
                initCtlWav(ctl, rec, isQuery);
              }
            }
          }
        }
        setPrjHeader(startIdx);
      }
    }


    void initCtlWav(ctlWavFile ctl, PrjRecord rec, bool fromQuery)
    {
      AnalysisFile analysis;
      List<string> species;
      string fullWavName;
      string wavName;
      string wavFilePath;
      enModel modelType;
      if (fromQuery)
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
                 BatInspector.Properties.MyResources.MainWindowMsgReport :
                 BatInspector.Properties.MyResources.MainWindow_showStatus_NoReport;

        setStatus($"  [{BatInspector.Properties.MyResources.MainWindowFiles}: {App.Model.View.VisibleFiles.Count}/{App.Model.CurrentlyOpen.getRecords().Length} | {report} ]");
      }
    }


    public void updateControls()
    {
      foreach (UIElement it in _spSpectrums.Children)
      {
        ctlWavFile ctl = it as ctlWavFile;
        PrjRecord rec = App.Model.CurrentlyOpen?.findRecord(ctl.WavName);
        if(rec != null)
          setCheckboxInWavCtl(ctl, rec.Selected);
      }

    }

    
    void showMsg(string title, string msg, bool topmost = false)
    {  Application.Current.Dispatcher.BeginInvoke((Action)(() =>
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
      try
      {
        showMsg(BatInspector.Properties.MyResources.msgInformation, BatInspector.Properties.MyResources.MainWindowMsgClassification, true);
        App.Model.evaluate(false);
      }
      catch(Exception ex)
      {
        DebugLog.log("error predicting species: " + ex.ToString(), enLogType.ERROR);
        DebugLog.save();
      }
    }


    private void _btnFindCalls_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        if ((App.Model.Prj != null) && (App.Model.Prj.Ok))
        {
          MessageBoxResult res = MessageBoxResult.Yes;
          DebugLog.log("MainWin:BTN 'Find calls' clicked ", enLogType.DEBUG);
          if (App.Model.CurrentlyOpen?.Analysis.IsEmpty == false)
            res = MessageBox.Show(BatInspector.Properties.MyResources.MainWinMsgOverwriteReport,
            BatInspector.Properties.MyResources.msgQuestion,
            MessageBoxButton.YesNo, MessageBoxImage.Question);
          if (res == MessageBoxResult.Yes)
          {
            App.Model.Status.State = enAppState.AI_ANALYZE;
            _workerPredict = new Thread(new ThreadStart(workerPrediction));
            _workerPredict.Start();
          }
        }
        else
        {
          MessageBox.Show(BatInspector.Properties.MyResources.msgPleaseOpenProjectFirst, MyResources.msgInformation, MessageBoxButton.OK, MessageBoxImage.Exclamation);
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
      _dgData.ItemsSource =  App.Model.View.ListView;

      for (int i = 0; i < _dgData.Columns.Count; i++)
      {
        _dgData.Columns[i].IsReadOnly = true;
      } 
    }

    private void _tbMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if(_tbPrj.IsSelected && (_oldTab != "Prj"))
      {
        _oldTab = "Prj";
        try
        {
          if (App.Model.CurrentlyOpen != null)
            buildWavFileList(false);
          DebugLog.log("TAB 'Project' got selected", enLogType.DEBUG);
        }
        catch (Exception ex)
        {
          DebugLog.log("TAB 'Project' selection failed:" + ex.ToString(), enLogType.ERROR);
        }

      }

      else if(_tbReport.IsSelected && (_oldTab != "Report"))
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
      if (_frmExp != null)
        _frmExp.Close();
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

    private void setCheckboxInWavCtl(ctlWavFile ctl, bool check)
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
        App.Model.saveSettings();
        if ((App.Model != null) && (App.Model.Prj != null) && (App.Model.Prj.Analysis != null) &&
          (!App.Model.Prj.Analysis.IsEmpty))
          App.Model.Prj.Analysis.save(App.Model.Prj.ReportName, App.Model.Prj.Notes, App.Model.Prj.SummaryName);
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
          _frmAbout = new FrmAbout(AppParams.AppVersion);
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

    private void _btnSpecies_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        if (_frmSpecies == null)
          _frmSpecies = new frmSpeciesData(closeWindow, this);
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

    void setMouseStatus()
    {
      Application.Current.Dispatcher.BeginInvoke((Action)(() =>
      {
        if (App.Model.Busy)
          Mouse.OverrideCursor = Cursors.Wait;
        else
          Mouse.OverrideCursor = null;
      }));
    }

    private void timer_Tick(object sender, EventArgs e)
    {
      setMouseStatus();
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
        if ((App.Model.Prj != null) && App.Model.Prj.Ok && (App.Model.Prj.Analysis != null))
        {
          _spSpectrums.Children.Clear();
          DirectoryInfo dir = new DirectoryInfo(App.Model.SelectedDir);
          initializeProject(dir);
        }
        App.Model.Prj.ReloadInGui = false;
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

        case enAppState.AI_ANALYZE:
          if ((_workerPredict != null) && (!_workerPredict.IsAlive))
          {
            _workerPredict = null;
            App.Model.updateReport();
            DirectoryInfo dir = new DirectoryInfo(App.Model.SelectedDir);
            initializeProject(dir);
          }
          break;

        case enAppState.OPEN_PRJ:
          if (_workerStartup == null)
          {
            if (_projectDir != null)
              _workerStartup = new Thread(initProjectAsync);
            else if (_queryFile != null)
              _workerStartup = new Thread(initQueryAsync);
            _workerStartup.Start();
          }
          else if(!_workerStartup.IsAlive)
          {
            App.Model.Busy = false;
            if (_btnCreatePrj.IsEnabled)
              _btnCreatePrj.IsEnabled = false;
           // populateControls(0);
            if ((App.Model.Prj != null) && App.Model.Prj.Ok)
              _lblProject.Text = BatInspector.Properties.MyResources.MainWindowPROJECT + ": " + App.Model.Prj.Name;
            if (App.Model.Query != null)
              _lblProject.Text = BatInspector.Properties.MyResources.MainWindow_timer_Tick_QUERY + ": " + App.Model.Query.Name;
            App.Model.Status.State = enAppState.WAIT_FOR_GUI;
            if (App.Model.Prj != null)
              DebugLog.log("Project opened: " + App.Model.Prj.Name, enLogType.INFO);
            else if (App.Model.Query != null)
              DebugLog.log("Query opened: " + App.Model.Query.Name, enLogType.INFO);
            showStatus();
          }
          break;

        case enAppState.WAIT_FOR_GUI:
          showStatus();
          App.Model.Status.State = enAppState.IDLE;
          break;
          
      }
    
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
        ScriptItem s = AppParams.Inst.ScriptInventory.getScriptInfo(script);
        List<ParamItem> pars = null;
        if (s != null)
        {
          if (s.IsTool)
          {
            pars = new List<ParamItem>();
            pars.Add(new ParamItem("VAR_FILE_NAME", enParamType.FILE, "VAR_FILE_NAME"));
          }
          else
            pars = s.Parameter;
        }
        _frmDebug.Visibility = Visibility.Visible;  
        _frmDebug.setup(Path.Combine(AppParams.Inst.ScriptInventoryPath, script), pars);
        DebugLog.log("MainWin:BTN 'Debug' clicked", enLogType.DEBUG);

      }
      catch (Exception ex)
      {
        DebugLog.log("MainWin:debugScript() failed: " + ex.ToString(), enLogType.ERROR);
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
      _mouseIsDownOnScrollList = true;
    }


    private void _scrollPrj_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
      try
      {
        if (!_mouseIsDownOnScrollPrj)
        {
          if (App.Model.CurrentlyOpen == null)
            return;

          if (_scrollBarPrjPos == _scrollPrj.Value)
            return;

          double diff = _scrollPrj.Value - _scrollBarPrjPos;
          if ((diff < 2) && (diff > 0))
            incrementControls(true);
          else if ((diff > -2) && (diff < 0))
            incrementControls(false);
          else
            populateControls((int)_scrollPrj.Value);
          _scrollBarPrjPos = _scrollPrj.Value;
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
      catch(Exception ex)
      {
        DebugLog.log(ex.ToString(), enLogType.ERROR);
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
        while ((dep != null) && !(dep is DataGridCell))
          dep = VisualTreeHelper.GetParent(dep);

        if (dep == null)
          return;

        ReportItemBd2 it = null;
        if (dep is DataGridCell)
        {
          DataGridCell cell = dep as DataGridCell;
          // navigate further up the tree
          while ((dep != null) && !(dep is DataGridRow))
            dep = VisualTreeHelper.GetParent(dep);

          DataGridRow row = dep as DataGridRow;
          it = row.DataContext as ReportItemBd2;
        }

        if (App.Model.CurrentlyOpen != null)
        {
          if (it != null)
          {
            int.TryParse(it.CallNr, out int callNr);
            AnalysisFile analysis = App.Model.CurrentlyOpen.Analysis.find(it.FileName);
            string fileName = Path.GetFileName(it.FileName);
            string wavPath = Path.GetDirectoryName(App.Model.CurrentlyOpen.getFullFilePath(it.FileName));
            setZoom(fileName, analysis, wavPath, null, App.Model.CurrentlyOpen.Analysis.ModelType);
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
    }

    private void _mnTool1_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        DebugLog.log("BTN custom tool pressed", enLogType.DEBUG);
        MenuItem m = sender as MenuItem;
        string script = (string)m.Tag;
        ScriptItem item = App.Model.Scripter.getScript(script);
        if (item != null)
        {
          if (item.Parameter.Count > 0)
          {
            string winTitle = BatInspector.Properties.MyResources.frmScriptParamTitle +": " + script;
            frmScriptParams frm = new frmScriptParams(winTitle, item.Parameter);
            frm.ShowDialog();
            if (frm.DialogResult == true)
              App.Model.Scripter.runScript(script, frm.ParameterValues);
          }
          else
            App.Model.Scripter.runScript(script);
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
        if ((App.Model.Prj != null) && App.Model.Prj.Ok)
        {
          FrmRecovery frm = new FrmRecovery(App.Model.Prj.Name);
          bool res = frm.ShowDialog() == true;
          if (res)
            App.Model.Prj.recovery(frm._cbDel.IsChecked == true, frm._cbChanged.IsChecked == true);
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

    private void _tbStatistic_GotFocus(object sender, RoutedEventArgs e)
    {
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
        if (_scrollPrj.Value < _scrollPrj.Maximum + idx - 1)
          _scrollPrj.Value += idx;
        else
          _scrollPrj.Value += _scrollPrj.Maximum;
      }
      else if (idx < 0)
      { 
        if (_scrollPrj.Value > idx)
          _scrollPrj.Value -= idx;
        else
          _scrollPrj.Value = 0;
      }
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
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
      else if(_tbReport.IsSelected)
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
            if (_dgData.SelectedIndex >  20)
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
  }

  public enum enWinType
  {
    ZOOM,
    BAT
  }

  public delegate void dlgcloseChildWindow(enWinType w);
}
