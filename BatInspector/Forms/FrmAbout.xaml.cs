using System;
using System.Collections.Generic;
using System.IO;
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
  /// Interaktionslogik für FrmAbout.xaml
  /// </summary>
  public partial class FrmAbout : Window
  {
    public FrmAbout(string version)
    {
      InitializeComponent();
      _tbVersion.Text = "Version V" + version;
      try
      {
        _tbLicences.Text = File.ReadAllText("Licenses.txt");
        _tbHistory.Text = File.ReadAllText("history.txt");
      }
      catch { }
    }

    private void _btnOk_Click(object sender, RoutedEventArgs e)
    {
      this.Close();
    }
  }
}
