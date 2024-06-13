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
  /// Interaction logic for ctlWebRepSpecies.xaml
  /// </summary>
  public partial class ctlWebRepSpecies : UserControl
  {
    dlgClickLabel _dlgDel = null;
    int _index = 0;
    public string Species { get { return _tbSpecies.Text; } set{ _tbSpecies.Text = value; } }
    public string Comment {  get { return _tbComment.Text; } set { _tbComment.Text = value; } }
    public string Confusion {  get { return _tbConfusion.Text; } set { _tbConfusion.Text = value; } } 
    public ctlWebRepSpecies(int idx, dlgClickLabel dlgDel)
    {
      InitializeComponent();
      _index = idx;
      _dlgDel = dlgDel;
    }

    private void _btnDel_Click(object sender, RoutedEventArgs e)
    {
      if (_dlgDel != null)
        _dlgDel(_index);
    }
  }
}
