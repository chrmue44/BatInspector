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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BatInspector.Forms
{
  /// <summary>
  /// Interaction logic for frmCreatePrjFromQuery.xaml
  /// </summary>
  public partial class frmCreatePrjFromQuery : Window
  {
    public frmCreatePrjFromQuery()
    {
      InitializeComponent();
      int wl = 150;
      _ctlPrjName.setup(MyResources.frmCreatePrjName, Controls.enDataType.STRING, 0, wl, true);
      _ctlPrjNotes.setup(MyResources.ctlPrjInfoNotes, Controls.enDataType.STRING, 0, wl, true);
      _ctlTargetDir.setup(BatInspector.Properties.MyResources.TargetDirectory, wl, true);
    }

    private void _btnOk_Click(object sender, RoutedEventArgs e)
    {
      Close();
    }

    private void _btnCancel_Click(object sender, RoutedEventArgs e)
    {
      Close();

    }
  }
}
