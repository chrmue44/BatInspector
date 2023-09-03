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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BatInspector.Controls
{
  /// <summary>
  /// Interaction logic for ctlPrjInfo.xaml
  /// </summary>
  public partial class ctlPrjInfo : UserControl
  {
    Project _prj = null;

    public ctlPrjInfo()
    {
      InitializeComponent();
    }

    private void _tbNotes_TextChanged(object sender, TextChangedEventArgs e)
    {
      if(_prj != null) 
      {
        _prj.Notes = _tbNotes.Text;
      }
    }

    public void setup(Project prj) 
    {
      _prj = prj;
      _tbCreated.Text = _prj.Created;
      _tbNotes.Text = _prj.Notes;
    }
  }
}
