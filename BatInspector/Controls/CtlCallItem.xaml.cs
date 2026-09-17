using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using libParser;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using ContextMenu = System.Windows.Controls.ContextMenu;
using MenuItem = System.Windows.Controls.MenuItem;


namespace BatInspector.Controls
{

  enum enEditStatus
  {
    VALID = 0,
    EDIT = 1,
    ERROR = 2
  }

  /// <summary>
  /// Interaktionslogik für CtlCallItem.xaml
  /// </summary>
  public partial class CtlCallItem : System.Windows.Controls.UserControl
  {
    string _valString = "";
    dlgSelItemChanged? _dlgValChange = null;
    dlgClickLabel? _dlgClickLabel = null; 
    int _index = 0;
    string[] _items = new string[0];

    bool _setup = false;
    enEditStatus _status;

    // public bool Focusable { set { _tb.Focusable = value; } get { return _tb.Focusable; } }

    public void setup(string label, int index, int widthLbl = 80, bool edit = false, dlgSelItemChanged? dlgValChange = null, dlgClickLabel? dlgClick = null)
    {
      _setup = true;
      _lbl.Text = label;
      _lbl.Focusable = false;
      _dlgValChange = dlgValChange;
      _index = index;
      _tb.Focusable = edit;
      _tb.IsEnabled = edit;
      _tb.ContextMenu = new ContextMenu();
      _lbl.Width = widthLbl;
      _dlgClickLabel = dlgClick;
      _status = enEditStatus.VALID;
      _setup = false;
    }

    public CtlCallItem()
    {
      InitializeComponent();
    }

    public void setValue(string val)
    {
      _setup = true;
      _valString = val;
      _tb.Text = val;
      _setup = false;
    }

    public void setItems(string[] items)
    {
      _items = items;
    }


    public string getValue()
    {
      return _valString;
    }

    public void setBgColor(SolidColorBrush color)
    {
      _tb.Background = color;
   //   _lbl.Background = color;
    }

    public void setFontBold(bool bold)
    {
      if (bold)
      {
        _tb.FontWeight = FontWeights.Bold;
        _lbl.FontWeight = FontWeights.Bold;
      }
      else
      {
        _tb.FontWeight = FontWeights.Normal;
        _lbl.FontWeight = FontWeights.Normal;
      }
    }

    private void _tb_TextChanged(object sender, TextChangedEventArgs e)
    {
      if(_setup) 
        return;
      _setup = true;
      _tb.CaretIndex = _tb.Text.Length;
      if (((_status == enEditStatus.VALID) || (_status == enEditStatus.ERROR)) && (_tb.Text.Length > 0))
        _tb.Text = _tb.Text.Substring(_tb.Text.Length - 1);
      _status = enEditStatus.EDIT;

      string[] finds = Array.FindAll(_items, s => s.StartsWith(_tb.Text, StringComparison.OrdinalIgnoreCase));

      switch (finds.Length)
      {
        case 0:
          _status = enEditStatus.ERROR;
          _tb.Background = System.Windows.Media.Brushes.Red;
          break;
        case 1:
          if (finds[0] == "Social")
            finds[0] = "Social";
          _status = enEditStatus.VALID;
          _valString = finds[0];
          _tb.Text = _valString;
          _dlgValChange?.Invoke(_index, _valString);
          break;
        default:
          _status = enEditStatus.EDIT;
          _tb.Background = System.Windows.Media.Brushes.Yellow;
          break;
      }
      _tb.CaretIndex = _tb.Text.Length;
      _setup = false;
    }

    private void _lbl_MouseDown(object sender, MouseButtonEventArgs e)
    {
      try
      {
        _dlgClickLabel?.Invoke(_index);
      }
      catch (Exception ex)
      {
        DebugLog.log($"_lbl_MouseDown: {ex}", enLogType.ERROR);
      }
    }

    private void _tb_MouseDown(object sender, MouseButtonEventArgs e)
    {
      if (sender is System.Windows.Controls.TextBox textBox && !textBox.IsKeyboardFocusWithin)
      {
        textBox.Focus();
        e.Handled = true; // Verhindert das Standard-Klickverhalten von WPF
        textBox.CaretIndex = textBox.Text.Length;
      }
    }

    void clickContext(object sender, EventArgs e)
    {
      _setup = true;
      MenuItem? it = (MenuItem) sender;
      _tb.Text = it?.Header.ToString();
      _valString = _tb.Text ?? "";
      _status = enEditStatus.VALID;
      Keyboard.ClearFocus();
      _dlgValChange?.Invoke(_index, _valString);
      _setup = false;
    }

    private void _tb_ContextMenuOpening(object sender, ContextMenuEventArgs e)
    {
      System.Windows.Controls.ContextMenu menu = new ContextMenu();
      menu.Width = 90;
      foreach(string s in _items)
      {
        MenuItem it = new MenuItem
        {
          Header = s,
          Padding = new Thickness(-30, 0, 10, 0),
          Height = 15,
          Style = GetStableMenuItemStyle()
        };
        it.Click += clickContext;
        menu.Items.Add(it);
      }
      _tb.ContextMenu = menu;
    }

    private Style GetStableMenuItemStyle()
    {
      string xaml = @"
        <Style xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
               xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
             TargetType='MenuItem'>
            <Setter Property='Padding' Value='8,4'/>
            <Setter Property='BorderThickness' Value='1'/>
            <Setter Property='BorderBrush' Value='Transparent'/>
            <Setter Property='Template'>
                <Setter.Value>
                    <ControlTemplate TargetType='MenuItem'>
                        <Border x:Name='Bd' Background='{TemplateBinding Background}' BorderBrush='{TemplateBinding BorderBrush}' BorderThickness='{TemplateBinding BorderThickness}' Padding='{TemplateBinding Padding}'>
                            <ContentPresenter ContentSource='Header' HorizontalAlignment='Left' VerticalAlignment='Center'/>
                        </Border>
                        <ControlTemplate.Triggers>
                            <Trigger Property='IsHighlighted' Value='True'>
                                <Setter TargetName='Bd' Property='Background' Value='#E5E5E5'/>
                                <Setter TargetName='Bd' Property='BorderBrush' Value='#CCCCCC'/>
                            </Trigger>
                        </ControlTemplate.Triggers>
                    </ControlTemplate>
                </Setter.Value>
            </Setter>
        </Style>";

      return (Style)System.Windows.Markup.XamlReader.Parse(xaml);
    }
  }
}
