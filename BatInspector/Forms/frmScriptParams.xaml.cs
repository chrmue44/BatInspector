using BatInspector.Controls;
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
  /// Interaction logic for frmParams.xaml
  /// </summary>
  public partial class frmScriptParams : Window
  {
    int _cnt;
    List<string> _paramVals;
    List<string> _paramText;

    public List<string> ParameteValues { get { return _paramVals; } }
    public frmScriptParams(string title, List<string> paramTexts)
    {
      InitializeComponent();
      _paramText = paramTexts;
      this.Title = title;
      _paramVals = new List<string>();
      this.Height = 120;
      for(int i = 0; i <  _paramText.Count; i++)
      {
        CtlSelectFile ctl = new CtlSelectFile();
        ctl.setup(_paramText[i], 150, false, "", checkParams);
        ctl.Margin = new Thickness(2,2,0,0);
        _sp.Children.Add(ctl);
        this.Height += 35;
      }
      _btnOk.IsEnabled = false;
    }

    private void checkParams()
    {
      bool en = true;
      for(int i = 0; i < _sp.Children.Count; i++)
      {
        CtlSelectFile ctl = _sp.Children[i] as CtlSelectFile;
        if (ctl.getValue().Length == 0)
        {
          en = false;
          break;
        }
      }
      _btnOk.IsEnabled = en;
    }
    private void _btnCancel_Click(object sender, RoutedEventArgs e)
    {
      DialogResult = false;
      this.Close();
    }

    private void _btnOk_Click(object sender, RoutedEventArgs e)
    {
      for(int i = 0; i < _cnt; i++) 
      {
        CtlSelectFile ctl = _sp.Children[i] as CtlSelectFile;
        _paramVals.Add(ctl.getValue());
      }
      DialogResult = true;
      this.Close();
    }
  }
}
