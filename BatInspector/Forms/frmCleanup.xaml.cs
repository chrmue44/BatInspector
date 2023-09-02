using BatInspector.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BatInspector.Forms
{
  /// <summary>
  /// Interaction logic for frmCleanup.xaml
  /// </summary>
  public partial class frmCleanup : Window
  {
    ViewModel _model;

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
      string wavUnit = "kB";
      string pngUnit = "kB";
      string logUnit = "kB";
      string origUnit = "kB";
      _model.checkMem(_ctlSelectFolder.getValue(), out wavSpace, out logSpace, out pngSpace, out origSpace);
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
      _cbDelWav.Content = MyResources.frmCleanupDeletedFiles + "  (" + wavSpace.ToString() + " " + wavUnit + ")";
      _cbDelPNG.Content = MyResources.frmCleanupPngFiles + "  (" + pngSpace.ToString() + " " + pngUnit + ")";
      _cbDelLog.Content = MyResources.frmCleanupLogFiles + "  (" + logSpace.ToString() + " " + logUnit + ")";
      _cbOriginal.Content = MyResources.frmCleanupOrigFiles + "  (" + origSpace.ToString() + " " + origUnit + ")";
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
      _model.cleanup(_ctlSelectFolder.getValue(), _cbDelWav.IsChecked == true,
                     _cbDelLog.IsChecked == true, _cbDelPNG.IsChecked == true,
                     _cbOriginal.IsChecked == true);
      this.Visibility = Visibility.Hidden;
    }
  }
}
