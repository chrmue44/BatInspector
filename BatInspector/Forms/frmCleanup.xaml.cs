/********************************************************************************
 *               Author: Christian Müller
 *     Date of creation: 2021-08-10                                       
 *   Copyright (C) 2023: Christian Müller chrmue44(at)gmail(dot).com
 *
 *              Licence:  CC BY-NC 4.0 
 ********************************************************************************/
using BatInspector.Properties;
using libParser;
using System.Threading;
using System.Windows;
using System.Windows.Interop;

namespace BatInspector.Forms
{
  /// <summary>
  /// Interaction logic for frmCleanup.xaml
  /// </summary>
  public partial class frmCleanup : Window
  {
    string _delFolder;
    bool _delWavs;
    bool _delLogs;
    bool _delPngs;
    bool _delOrigs;
    bool _delAnn;
    int _logSpace;
    int _wavSpace;
    int _pngSpace;
    int _origSpace;
    int _annSpace;
    string _rootDir;

    bool _memCalcDone;
    public frmCleanup()
    {
      InitializeComponent();
      _ctlSelectFolder.setup(MyResources.frmCleanupSelRootFolder, 120, true, "", folderSelected);
    }

    void threadCheckMem()
    {
      App.Model.checkMem(_rootDir, out _wavSpace, out _logSpace, out _pngSpace, out _origSpace, out _annSpace);
      update();
    }

    void update()
    {
      if (!Dispatcher.CheckAccess()) // CheckAccess returns true if you're on the dispatcher thread
      {
        Dispatcher.BeginInvoke(new delegateLogClear(update));
      }
      else
      {

        _memCalcDone = true;
        _btnOk.IsEnabled = true;
        string wavUnit = "kB";
        string pngUnit = "kB";
        string logUnit = "kB";
        string origUnit = "kB";
        string annUnit = "kB";

        if (_wavSpace > 2048)
        {
          _wavSpace /= 1024;
          wavUnit = "MB";
        }
        if (_pngSpace > 2048)
        {
          _pngSpace /= 1024;
          pngUnit = "MB";
        }
        if (_origSpace > 2048)
        {
          _origSpace /= 1024;
          origUnit = "MB";
        }
        if (_annSpace > 2048)
        {
          _annSpace /= 1024;
          annUnit = "MB";
        }
        _cbDelWav.Content = MyResources.frmCleanupDeletedFiles + "  (" + _wavSpace.ToString() + " " + wavUnit + ")";
        _cbDelPNG.Content = MyResources.frmCleanupPngFiles + "  (" + _pngSpace.ToString() + " " + pngUnit + ")";
        _cbDelLog.Content = MyResources.frmCleanupLogFiles + "  (" + _logSpace.ToString() + " " + logUnit + ")";
        _cbOriginal.Content = MyResources.frmCleanupOrigFiles + "  (" + _origSpace.ToString() + " " + origUnit + ")";
        _cbDelAnn.Content = MyResources.frmCleanupAnnotations + "  (" + _annSpace.ToString() + " " + annUnit + ")";
      }
    }
private void folderSelected()
    {
      _memCalcDone = false;
      _btnOk.IsEnabled = false;
      _rootDir = _ctlSelectFolder.getValue();
      Thread t = new Thread(threadCheckMem);
      t.Start();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      winUtils.hideCloseButton(new WindowInteropHelper(this).Handle);
    }

    private void _btnCancel_Click(object sender, RoutedEventArgs e)
    {
      this.Visibility = Visibility.Hidden;
    }

    private void _btnOk_Click(object sender, RoutedEventArgs e)
    {
      _delFolder = _ctlSelectFolder.getValue();
      _delWavs = _cbDelWav.IsChecked == true;
      _delLogs = _cbDelLog.IsChecked == true;
      _delPngs = _cbDelPNG.IsChecked == true;
      _delOrigs = _cbOriginal.IsChecked == true;
      _delAnn = _cbDelAnn.IsChecked == true;
      Thread thr = new Thread(threadCleanup);
      thr.Start();
      this.Visibility = Visibility.Hidden;
    }

    private void threadCleanup()
    {
      App.Model.cleanup(_delFolder, _delWavs, _delLogs, _delPngs, _delOrigs, _delAnn);
      DebugLog.log("finished tidying up", enLogType.INFO);
    }
  }
}
