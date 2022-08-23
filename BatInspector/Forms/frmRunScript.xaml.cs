using Microsoft.Win32;
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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BatInspector.Forms
{
  /// <summary>
  /// Interaction logic for frmRunScript.xaml
  /// </summary>
  public partial class frmRunScript : Window
  {
    ViewModel _model;
    public frmRunScript(ViewModel model)
    {
      _model = model;
      InitializeComponent();
      _tbFileName.Text = _model.ScriptName;
    }

    private void _btnSelect_Click(object sender, RoutedEventArgs e)
    {
      OpenFileDialog openFileDialog = new OpenFileDialog();
      openFileDialog.Filter = "Script files (*.scr)|*.scr|All files (*.*)|*.*";
      if (openFileDialog.ShowDialog() == true)
        _tbFileName.Text = openFileDialog.FileName;
    }

    private void _btnOk_Click(object sender, RoutedEventArgs e)
    {
      _model.executeScript(_tbFileName.Text);
      this.Close();
    }

    private void _btnCancel_Click(object sender, RoutedEventArgs e)
    {
      this.Close();
    }

    private void _btnEdit_Click(object sender, RoutedEventArgs e)
    {
      _model.editScript(_tbFileName.Text);
      this.Close();
    }
  }
}
