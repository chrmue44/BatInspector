using Microsoft.Win32;
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
    public CtlSelectFile()
    {
      InitializeComponent();
    }

    public bool IsFolder { get { return _isFolder; } }

    public void setup(string label, int widthLbl = 80, bool isFolder = false)
    {
      _lbl.Text = label;
      _lbl.Width = widthLbl;
      _isFolder = isFolder;
      _txt.Text = "";
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
      }
    }
  }
}
