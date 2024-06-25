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
    ViewModel _model;
    string _delFolder;
    bool _delWavs;
    bool _delLogs;
    bool _delPngs;
    bool _delOrigs;
    bool _delAnn;

    public frmCleanup(ViewModel model)
    {
      InitializeComponent();
      _model = model;
      _ctlSelectFolder.setup(MyResources.frmCleanupSelRootFolder, 120, true, "", folderSelected);
    }

    private void folderSelected()
    {
      int logSpace;
      int wavSpace;
      int pngSpace;
      int origSpace;
      int annSpace;
      string wavUnit = "kB";
      string pngUnit = "kB";
      string logUnit = "kB";
      string origUnit = "kB";
      string annUnit = "kB";
      _model.checkMem(_ctlSelectFolder.getValue(), out wavSpace, out logSpace, out pngSpace, out origSpace, out annSpace);
      if( wavSpace > 2048)
      {
        wavSpace /= 1024;
        wavUnit = "MB";
      }
      if (pngSpace > 2048)
      {
        pngSpace /= 1024;
        pngUnit = "MB";
      }
      if (origSpace > 2048)
      {
        origSpace /= 1024;
        origUnit = "MB";
      }
      if (annSpace > 2048)
      {
        annSpace /= 1024;
        annUnit = "MB";
      }
      _cbDelWav.Content = MyResources.frmCleanupDeletedFiles + "  (" + wavSpace.ToString() + " " + wavUnit + ")";
      _cbDelPNG.Content = MyResources.frmCleanupPngFiles + "  (" + pngSpace.ToString() + " " + pngUnit + ")";
      _cbDelLog.Content = MyResources.frmCleanupLogFiles + "  (" + logSpace.ToString() + " " + logUnit + ")";
      _cbOriginal.Content = MyResources.frmCleanupOrigFiles + "  (" + origSpace.ToString() + " " + origUnit + ")";
      _cbDelAnn.Content = MyResources.frmCleanupAnnotations + "  (" + annSpace.ToString() + " " + annUnit + ")";
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
      _model.cleanup(_delFolder, _delWavs, _delLogs, _delPngs, _delOrigs, _delAnn);
      DebugLog.log("finished tidying up", enLogType.INFO);
    }
  }
}
