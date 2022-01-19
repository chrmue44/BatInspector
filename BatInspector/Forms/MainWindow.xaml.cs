using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;


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
    List<UIElement> _listBak = null;

    public MainWindow()
    {
      InitializeComponent();
      initTreeView();
      _model = new ViewModel(this);
      _model.loadSettings();
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
    }

    public void TreeViewItem_Expanded(object sender, RoutedEventArgs e)
    {
      TreeViewItem item = e.Source as TreeViewItem;
      if ((item.Items.Count == 1) && (item.Items[0] is string))
      {
        item.Items.Clear();

        DirectoryInfo expandedDir = null;
        if (item.Tag is DriveInfo)
          expandedDir = (item.Tag as DriveInfo).RootDirectory;
        if (item.Tag is DirectoryInfo)
          expandedDir = (item.Tag as DirectoryInfo);
        try
        {
          foreach (DirectoryInfo subDir in expandedDir.GetDirectories())
          {
            TreeViewItem childItem = CreateTreeItem(subDir);
            item.Items.Add(childItem);
            if(Project.containsProject(subDir))
            {
              childItem.FontWeight = FontWeights.Bold;
              childItem.Foreground = new SolidColorBrush(Colors.Violet);
            }
          }
        }
        catch { }
      }
    }

    public void TreeViewItem_Selected(object sender, RoutedEventArgs e)
    {
      TreeViewItem item = e.Source as TreeViewItem;
      DirectoryInfo dir = item.Tag as DirectoryInfo;
      _model.initProject(dir);
      if (_model.Prj != null)
      {
        _lblProject.Text = dir.FullName;
        populateFiles();
      }
    }

    public void setStatus(string status)
    {
      _lblStatus.Text = status;
    }

    private TreeViewItem CreateTreeItem(object o)
    {
      TreeViewItem item = new TreeViewItem();
      item.Header = o.ToString();
      item.Tag = o;
      item.Items.Add("Loading...");
      return item;
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
      if (_model.Prj != null)
      {
        foreach (BatExplorerProjectFileRecordsRecord rec in _model.Prj.Records)
        {
          bool newImage;
          _model.getFtImage(rec, out newImage);
          if(newImage)
          {
            (sender as BackgroundWorker).ReportProgress(55, rec.Name);
          }

        }
      }
    }

     internal async Task  createFftImages()
    {
      int index = 0;

      if(_model.Prj != null)
      {
        _spSpectrums.Children.Clear();
        _cbFocus = -1;
        foreach (BatExplorerProjectFileRecordsRecord rec in _model.Prj.Records)
        {
          ctlWavFile ctl = new ctlWavFile(index++, setFocus, _model);
          DockPanel.SetDock(ctl, Dock.Bottom);
          bool newImage;
          ctl.Img.Source = _model.getFtImage(rec, out newImage);
          ctl.Img.MaxWidth = MAX_IMG_WIDTH;
          ctl.Img.MaxHeight = _imgHeight;
          ctl.setFileInformations(rec.File, _model.Analysis.getAnalysis(rec.File), _model.WavFilePath);
          _spSpectrums.Dispatcher.Invoke(() =>
          {
            _cbFocus = 0;
            _spSpectrums.Children.Add(ctl);
          });
          await Task.Delay(2);
        }
      }
      _lblStatus.Text = "";
    }

    private void _btnAll_Click(object sender, RoutedEventArgs e)
    {
      foreach(UIElement it in _spSpectrums.Children)
      {
        ctlWavFile ctl = it as ctlWavFile;
        ctl._cbSel.IsChecked = true;
      }
    }

    private void _btnNone_Click(object sender, RoutedEventArgs e)
    {
      foreach (UIElement it in _spSpectrums.Children)
      {
        ctlWavFile ctl = it as ctlWavFile;
        ctl._cbSel.IsChecked = false;
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
      List<UIElement> list = new List<UIElement>();
      foreach (UIElement it in _spSpectrums.Children)
      {
        ctlWavFile ctl = it as ctlWavFile;
        if (ctl._cbSel.IsChecked == true)
        {
          _model.deleteFile(ctl._grp.Header.ToString());
          list.Add(it);
        }
      }

      foreach(UIElement it in list)
        _spSpectrums.Children.Remove(it);
      reIndexSpectrumControls();

    }

    private void _btnHideUnSelected_Click(object sender, RoutedEventArgs e)
    {
      List<UIElement> list = new List<UIElement>();
      _listBak = new List<UIElement>();
      foreach (UIElement it in _spSpectrums.Children)
      {
        _listBak.Add(it);
        ctlWavFile ctl = it as ctlWavFile;
        if (ctl._cbSel.IsChecked == true)
          list.Add(it);
      }
      _spSpectrums.Children.Clear();
      foreach (UIElement it in list)
        _spSpectrums.Children.Add(it);
    }

    private void _btnShowAll_Click(object sender, RoutedEventArgs e)
    {
      if (_listBak != null)
      {
        _spSpectrums.Children.Clear();
        foreach (UIElement it in _listBak)
        {
          ctlWavFile ctl = it as ctlWavFile;
          _spSpectrums.Children.Add(it);
        }
        _listBak = null;
      }
    }

    public void setFocus(int index)
    {
      if((index >= 0) && (index < _spSpectrums.Children.Count))
      {
        _cbFocus = index;
      }
    }

    private void _btnFindCalls_Click(object sender, RoutedEventArgs e)
    {
      MessageBoxResult res = MessageBox.Show("Start data evaluation to find calls (may take a long time!)", "", MessageBoxButton.OKCancel, MessageBoxImage.Information);
      if(res == MessageBoxResult.OK)
      {
        _model.startEvaluation();
      }
    }

    private void _btnDebug_Click(object sender, RoutedEventArgs e)
    {
      if(_log == null)
      {
        _log = new FrmLog();
        DebugLog.setLogDelegate(_log.log);
        _log.Show();
        DebugLog.log("Debug log opened", enLogType.INFO);
        
      }
      else
      {
        DebugLog.setLogDelegate(null);
        _log.Close();
        _log = null;
      }
    }

    private void _btnSize_Click(object sender, RoutedEventArgs e)
    {
      _imgHeight >>= 1;
      if (_imgHeight < 32)
        _imgHeight = MAX_IMG_HEIGHT;
      foreach (UIElement ui in _spSpectrums.Children)
      {
        ctlWavFile ctl = ui as ctlWavFile;
        ctl.Img.MaxHeight = _imgHeight;
        ctl.Img.Height = _imgHeight;
        ctl.Img.Width = MAX_IMG_WIDTH;
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
      if (_log != null)
        _log.Close();
    }

    private void _btnFilter_Click(object sender, RoutedEventArgs e)
    {
      _frmFilter = new FrmFilter(_model.Filter);
      _frmFilter.Show();
    }
  }

  public delegate void dlgSetFocus(int index);
}
