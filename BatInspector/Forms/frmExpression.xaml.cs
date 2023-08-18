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

    public bool SaveFilter { get { return _cbSave.IsChecked == true; } }

    public string FilterName {  get{ return _ctlExpName.getValue(); } }

    public frmExpression(ExpressionGenerator gen, bool withSaveOption, bool withAllCallsCb = true)
    {
      InitializeComponent();
      _gen = gen;
      _ctlEditor.setup(gen, enableOk);
      _ctlExpName.setup("Name", enDataType.STRING, 0, 100, true, valueOfNameChanged);
      _ctlExpName.IsEnabled = false;
      _ctlExpName.setValue("");
      if(!withSaveOption)
      {
        this.Height = 300;
        _grpSave.Visibility = Visibility.Hidden;
        _grd.RowDefinitions[1].Height = new GridLength(5);
      }
      _cbAll.Visibility = withAllCallsCb ? Visibility.Visible : Visibility.Hidden;
    }


    private void valueOfNameChanged(enDataType type, object val)
    {
      if ((_cbSave.IsChecked == true) && (_ctlExpName.getValue().Length > 0))
        _btnOk.IsEnabled = _ctlEditor.FormulaOk;
      else
        _btnOk.IsEnabled = false;
    }

    private void enableOk(bool en)
    {
      if((!_cbSave.IsChecked == true) || ((_cbSave.IsChecked == true) && (_ctlExpName.getValue().Length >= 3)))
      _btnOk.IsEnabled = en;
      else
        _btnOk.IsEnabled = false;
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
      if (_ctlExpName.getValue().Length < 3)
        _btnOk.IsEnabled = false;
    }

    private void _cbSave_Unchecked(object sender, RoutedEventArgs e)
    {
      _ctlExpName.IsEnabled = false;
    }
  }
}
