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
  /// Interaction logic for ctlDebugLine.xaml
  /// </summary>
  public partial class ctlDebugLine : UserControl
  {
    bool _breakActive = false;
    public ctlDebugLine()
    {
      InitializeComponent();
    }

    public void setup(int i, string line)
    {
      _tbLine.Text = line;
      _tbNr.Text = i.ToString();
      _imgBreak.Visibility = Visibility.Hidden;
    }

    public void activate(bool on)
    {
      if (on)
        _tbLine.Background = Brushes.LightGreen;
      else
        _tbLine.Background = Brushes.White;
    }

    private void _btnBreakPt_Click(object sender, RoutedEventArgs e)
    {
      _breakActive = !_breakActive;
      _imgBreak.Visibility = _breakActive ? Visibility.Visible : Visibility.Hidden;
    }
  }
}
