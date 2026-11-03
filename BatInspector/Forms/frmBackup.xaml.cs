using System.Windows;
using BatInspector.Properties;
using libParser;

namespace BatInspector.Forms
{
  /// <summary>
  /// Interaction logic for frmBackup.xaml
  /// </summary>
  public partial class frmBackup : Window
  {
    string _src;
    string _dst;

    public frmBackup()
    {
      InitializeComponent();
      int w = 150;
      _ctlSrcDir.setup(MyResources.frmBackupSrc, w, true);
      _ctlDstDir.setup(MyResources.TargetDirectory, w, true);
    }


    public string StartDirectory
    {
      set
      {
        _ctlSrcDir.setValue(value);
      }
    }

    private void backupTask()
    {
      App.Model.backup(_src, _dst);
      DebugLog.log($"finished backup from {_src} to {_dst}", enLogType.INFO);
    }

    private void _btnCancel_Click(object sender, RoutedEventArgs e)
    {
      this.Visibility = Visibility.Hidden;
    }


    private void _btnOk_Click(object sender, RoutedEventArgs e)
    {
      _src = _ctlSrcDir.getValue();
      _dst = _ctlDstDir.getValue();

      Thread thr = new Thread(new ThreadStart(backupTask));
       thr.Start();
      this.Visibility = Visibility.Hidden;
    }


  }
}
