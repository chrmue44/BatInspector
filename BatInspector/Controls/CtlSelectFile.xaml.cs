using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
//using System.Windows.Forms;

namespace BatInspector.Controls
{
  /// <summary>
  /// Interaktionslogik für CtlSelectFile.xaml
  /// </summary>
  public partial class CtlSelectFile : UserControl
  {
    bool _isFolder = false;
    string _filter = "all files(*.*)|*.*";

    public CtlSelectFile()
    {
      InitializeComponent();
    }

    public bool IsFolder { get { return _isFolder; } }

   /* public bool IsEnabled
    {
      get { return _txt.IsEnabled; }
      set { _txt.IsEnabled = value; _btnOpen.IsEnabled = value; }
    }*/

    public void setup(string label, int widthLbl = 80, bool isFolder = false, string filter = "")
    {
      _lbl.Text = label;
      _lbl.Width = widthLbl;
      _isFolder = isFolder;
      _txt.Text = "";
      _filter = filter;
    }

    public string getValue()
    {
      return _txt.Text; 
    }

    private void _btnOpen_Click(object sender, RoutedEventArgs e)
    {
      if (_isFolder)
      {
        System.Windows.Forms.FolderBrowserDialog ofo = new System.Windows.Forms.FolderBrowserDialog();
        System.Windows.Forms.DialogResult res = ofo.ShowDialog();
        if (res == System.Windows.Forms.DialogResult.OK)
        {
          _txt.Text = ofo.SelectedPath;
        }
      }
      else
      {
        OpenFileDialog ofi = new OpenFileDialog();
        ofi.Filter = _filter;
        bool? ok = ofi.ShowDialog();
        if(ok == true) 
        {
          _txt.Text = ofi.FileName;
        }
      }
    }
   
  }
}
