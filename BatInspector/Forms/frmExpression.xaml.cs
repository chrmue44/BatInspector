using BatInspector.Controls;
using System.Windows;
using System.Windows.Media.TextFormatting;

namespace BatInspector.Forms
{
  /// <summary>
  /// Interaction logic for Window1.xaml
  /// </summary>
  public partial class frmExpression : Window
  {
    ExpressionGenerator _gen;

    public string FilterExpression { get { return _ctlEditor.Expression; } }
    public bool AllCalls { get{ return _cbAll.IsChecked == true; } }
    public frmExpression(ExpressionGenerator gen, bool withSaveOption)
    {
      InitializeComponent();
      _gen = gen;
      _ctlEditor.setup(gen);
      _ctlExpName.setup("Name", enDataType.STRING, 0, 100, true);
      _ctlExpName.IsEnabled = false;
      if(!withSaveOption)
      {
        this.Height = 300;
        _grpSave.Visibility = Visibility.Hidden;
        _grd.RowDefinitions[1].Height = new GridLength(5);
      }
    }

    private void _btnCancel_Click(object sender, RoutedEventArgs e)
    {
      this.Topmost = false;
      this.Visibility = Visibility.Hidden;
      this.DialogResult = false;
    }

    private void _btnOk_Click(object sender, RoutedEventArgs e)
    {
      this.Topmost = false;
      this.Visibility = Visibility.Hidden;
      this.DialogResult = true;
    }

    private void _cbSave_Checked(object sender, RoutedEventArgs e)
    {
      _ctlExpName.IsEnabled = true;
    }

    private void _cbSave_Unchecked(object sender, RoutedEventArgs e)
    {
      _ctlExpName.IsEnabled = false;
    }
  }
}
