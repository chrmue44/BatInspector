using BatInspector.Forms;
using System;
using System.Collections.Generic;
using System.Data.Common;
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
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace BatInspector.Controls
{
  /// <summary>
  /// Interaction logic for ctlDebugLine.xaml
  /// </summary>
  public partial class ctlDebugLine : UserControl
  {
    static BitmapImage _imgRed = new BitmapImage(new Uri(@"pack://application:,,,/images/Button-Blank-Red.48.png", UriKind.Absolute));
    static BitmapImage _imgBlue = new BitmapImage(new Uri(@"pack://application:,,,/images/Button-Blank-Blue.48.png", UriKind.Absolute));

    bool _breakActive = false;
    string _breakCond = "TRUE";
    int _lineNr;
    dlgSetBreakCondition _dlgSetBreakCondition;
    public ctlDebugLine()
    {
      InitializeComponent();
    }

    public void setup(int i, string line, dlgSetBreakCondition setBrkCond)
    {
      _lineNr = i;
      _tbLine.Text = line;
      _tbNr.Text = i.ToString();
      _imgBreak.Visibility = Visibility.Hidden;
      _dlgSetBreakCondition = setBrkCond;
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
      _imgBreak.Source = _imgRed;
      if (_breakActive && _breakCond == "TRUE")
        _imgBreak.Source = _imgRed;
      else
        _imgBreak.Source = _imgBlue;
      _imgBreak.Visibility = _breakActive ? Visibility.Visible : Visibility.Hidden;
      if (_dlgSetBreakCondition != null)
      {
        if (_breakActive)
          _dlgSetBreakCondition(_lineNr, _breakCond);
        else
          _dlgSetBreakCondition(_lineNr, "");
      }
    }

    private void _btnBreakPt_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
    {
      if (_breakActive)
      {
        frmBreakCondition frm = new frmBreakCondition(_tbNr.Text, _breakCond);
        bool? ok = frm.ShowDialog();
        if (ok == true)
        {
          _breakCond = frm.BreakCondition;
          if (_breakCond != "TRUE")
            _imgBreak.Source = _imgBlue;
          if (_dlgSetBreakCondition != null)
            _dlgSetBreakCondition(_lineNr, _breakCond);
        }
      }
    }
  }
}