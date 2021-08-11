using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;


namespace BatInspector
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    ViewModel _model;
    FrmLog _log = null;

    public MainWindow()
    {
      InitializeComponent();
      initTreeView();
      _model = new ViewModel();
    }

    public void initTreeView()
    {
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
            if(_model.containsProject(subDir))
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

    private TreeViewItem CreateTreeItem(object o)
    {
      TreeViewItem item = new TreeViewItem();
      item.Header = o.ToString();
      item.Tag = o;
      item.Items.Add("Loading...");
      return item;
    }


    private async void populateFiles()
    {
      _spSpectrums.Children.Clear();
      await createFftImages();
    }


    internal async Task  createFftImages()
    {
      if(_model.Prj != null)
      {
        foreach (BatExplorerProjectFileRecordsRecord rec in _model.Prj.Records)
        {
          ctlWavFile ctl = new ctlWavFile();
          DockPanel.SetDock(ctl, Dock.Bottom);
          ctl.Img.Source = _model.getImage(rec);
          ctl.Img.MaxWidth = 512;
          ctl.Img.MaxHeight = 256;
          ctl.setFileInformations(rec.File, 1);
          _spSpectrums.Dispatcher.Invoke(() =>
          {
            _spSpectrums.Children.Add(ctl);
          });
          await Task.Delay(5);
        }
      }
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

    /*    void WavFileSelected(object sender, RoutedEventArgs e)
{
DataGrid dg = e.Source as DataGrid;
PrjEntry pe = dg.SelectedItem as PrjEntry;

Waterfall wf = new Waterfall(_selectedDir + "Records/" + pe.FileName, 1024);
Bitmap bmp = wf.generatePicture(512, 256);
BitmapImage bImg = Convert(bmp);
//Image img = new Image();
//      img.Source = bImg;
ctlWavFile ctl = new ctlWavFile();
DockPanel.SetDock(ctl, Dock.Bottom);
ctl.Img.Source = bImg;
ctl.Img.MaxWidth = 512;
ctl.Img.MaxHeight = 256;
ctl.setFileInformations(pe.FileName, 1);

_spSpectrums.Children.Add(ctl);
} */
    /*    private void initProjectGrid(string fName)
        {
          string xml = File.ReadAllText(fName);
          var serializer = new XmlSerializer(typeof(BatExplorerProjectFile));
          BatExplorerProjectFile batExplorerPrj;

          TextReader reader = new StringReader(xml);
          batExplorerPrj = (BatExplorerProjectFile)serializer.Deserialize(reader);

          List<PrjEntry> list = new List<PrjEntry>();
          foreach (BatExplorerProjectFileRecordsRecord record in batExplorerPrj.Records)
          {
            PrjEntry item = new PrjEntry();
            item.FileName = record.File;
            list.Add(item);
          }
          _dgProject.ItemsSource = list;
        } */

    /*
    private void initGrid()
    {
      List<DirListEntry> list = new List<DirListEntry>();
      string[] dirs = Directory.GetDirectories(@"c:\", "*", SearchOption.TopDirectoryOnly);
      foreach (string dir in dirs)
      {
        DirListEntry item = new DirListEntry();
        item.IsDir = true;
        item.Name = dir;
        item.Date = Directory.GetCreationTime(dir).ToShortDateString();
        item.Size = "";
        list.Add(item);
      }

      string[] files = Directory.GetFiles(@"c:\", "*", SearchOption.TopDirectoryOnly);
      foreach (string f in files)
      {
        DirListEntry item = new DirListEntry();
        item.Name = f;
        item.IsDir = false;
        item.Date = File.GetLastWriteTime(f).ToShortDateString();
        item.Size = new System.IO.FileInfo(f).Length.ToString();
        list.Add(item);
      }

      _dgData.ItemsSource = list;
    }  

    void createNewFieEntry()
    {
    } */

  }
  /*
  public class PrjEntry
  {
    public string FileName { get; set; }
  }
  */
 
  /*
  public class DirListEntry
  {
    public bool IsDir { get; set; }
    public string Name { get; set; }

    public string Size { get; set; }

    public string Date { get; set; }
  }*/

}
