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
    public CtlSelectFile()
    {
      InitializeComponent();
      IsFolder = false;
    }

    public bool IsFolder { get; set; }
    private void _btnOpen_Click(object sender, RoutedEventArgs e)
    {
      if (IsFolder)
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
