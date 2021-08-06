using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
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
using System.Windows.Navigation;
using System.Xml.Serialization;
using Image = System.Windows.Controls.Image;

namespace BatInspector
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    string _selectedDir;
    public MainWindow()
    {
      InitializeComponent();
      initGrid();
      initTreeView();
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
            item.Items.Add(CreateTreeItem(subDir));
        }
        catch { }
      }
    }

    public void TreeViewItem_Selected(object sender, RoutedEventArgs e)
    {
      TreeViewItem item = e.Source as TreeViewItem;
      DirectoryInfo dir = item.Tag as DirectoryInfo;
      _selectedDir = dir.FullName + "/";
      string[] files = System.IO.Directory.GetFiles(dir.FullName, "*.bpr",
                       System.IO.SearchOption.TopDirectoryOnly);
      if (files.Length > 0)
        initProjectGrid(files[0]);
    }

    private TreeViewItem CreateTreeItem(object o)
    {
      TreeViewItem item = new TreeViewItem();
      item.Header = o.ToString();
      item.Tag = o;
      item.Items.Add("Loading...");
      return item;
    }

    private void initProjectGrid(string fName)
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
    }

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

    void WavFileSelected(object sender, RoutedEventArgs e)
    {
      DataGrid dg = e.Source as DataGrid;
      PrjEntry pe = dg.SelectedItem as PrjEntry;
      Waterfall wf = new Waterfall(_selectedDir + "Records/" + pe.FileName, 1024);
      Bitmap bmp = wf.generatePicture(512, 256);
      BitmapImage bImg = Convert(bmp);
      Image img = new Image();
      img.Source = bImg;
      DockPanel.SetDock(img, Dock.Bottom);
      img.MaxWidth = 512;
      img.MaxHeight = 256;
      _spSpectrums.Children.Add(img); 
    }


    //http://www.shujaat.net/2010/08/wpf-images-from-project-resource.html
    public BitmapImage Convert(Bitmap value)
    {
      MemoryStream ms = new MemoryStream();
      value.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
      BitmapImage image = new BitmapImage();
      image.BeginInit();
      ms.Seek(0, SeekOrigin.Begin);
      image.StreamSource = ms;
      image.EndInit();

      return image;
    }

    //https://stackoverflow.com/questions/6484357/converting-bitmapimage-to-bitmap-and-vice-versa
    [System.Runtime.InteropServices.DllImport("gdi32.dll")]
    public static extern bool DeleteObject(IntPtr hObject);

    private BitmapImage Bitmap2BitmapImage(Bitmap bitmap)
    {
      IntPtr hBitmap = bitmap.GetHbitmap();
      BitmapImage retval;

      try
      {
        retval = (BitmapImage)Imaging.CreateBitmapSourceFromHBitmap(
                     hBitmap,
                     IntPtr.Zero,
                     Int32Rect.Empty,
                     BitmapSizeOptions.FromEmptyOptions());
      }
      finally
      {
        DeleteObject(hBitmap);
      }

      return retval;
    }
    /*
    private void initPrjGrid()
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
    }*/

    /*
private TreeViewItem GetTreeView(string uid, string text, string imagePath)
{
TreeViewItem item = new TreeViewItem();
item.Uid = uid;
item.IsExpanded = false;

// create stack panel
StackPanel stack = new StackPanel();
stack.Orientation = Orientation.Horizontal;

// create Image
Image image = new Image();
image.Source = new BitmapImage(new Uri("pack://application:,,,/images/" + imagePath));
image.Width = 16;
image.Height = 16;
// Label
Label lbl = new Label();
lbl.Content = text;


// Add into stack
stack.Children.Add(image);
stack.Children.Add(lbl);

// assign stack to header
item.Header = stack;
return item;
} */

  }

  public class PrjEntry
  {
    public string FileName { get; set; }
  }

  public class DirListEntry
  {
    public bool IsDir { get; set; }
    public string Name { get; set; }

    public string Size { get; set; }

    public string Date { get; set; }
  }

}
