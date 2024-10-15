using System.Reflection;
using System.Windows;
using System;
using System.Windows.Controls;
using System.Windows.Input;
using System.Runtime.InteropServices;

//https://stackoverflow.com/questions/841293/where-is-the-wpf-numeric-updown-control
//Answer Sonorx

namespace BatInspector.Controls
{
  /// <summary>
  /// Interaction logic for ctlNumericUpDown.xaml
  /// </summary>
  public partial class ctlNumericUpDown : UserControl
  {
    int minvalue = -100,
            maxvalue = 100,
            startvalue = 10;
    dlgValueChanged _dlgValChange = null;
    int _number = 0;

    public ctlNumericUpDown()
    {
      InitializeComponent();
      NUDTextBox.Text = startvalue.ToString();
    }


    public int Value { get { return _number; } set {  _number = value; } }

    public void setup(string label,  int widthLbl = 80, int widthTb = 40, dlgValueChanged dlgValChange = null)
    {
      _lbl.Text = label;
      _lbl.Focusable = false;
      // _lbl.Opacity = edit ? 1 : 0.5;
      _grd.ColumnDefinitions[0].Width = new GridLength(widthLbl);
      _grd.ColumnDefinitions[1].Width = new GridLength(widthTb);
      _dlgValChange = dlgValChange;
    }


    private void NUDButtonUP_Click(object sender, RoutedEventArgs e)
    {
      if(_number < maxvalue)
       _number++;
      NUDTextBox.Text = Convert.ToString(_number);
    }

    private void NUDButtonDown_Click(object sender, RoutedEventArgs e)
    {
      if (_number > minvalue)
        _number--;
      NUDTextBox.Text = Convert.ToString(_number);
    }

    private void NUDTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {

      if (e.Key == Key.Up)
      {
        NUDButtonUP.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        typeof(Button).GetMethod("set_IsPressed", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(NUDButtonUP, new object[] { true });
      }


      if (e.Key == Key.Down)
      {
        NUDButtonDown.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        typeof(Button).GetMethod("set_IsPressed", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(NUDButtonDown, new object[] { true });
      }
    }

    private void NUDTextBox_PreviewKeyUp(object sender, KeyEventArgs e)
    {
      if (e.Key == Key.Up)
        typeof(Button).GetMethod("set_IsPressed", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(NUDButtonUP, new object[] { false });

      if (e.Key == Key.Down)
        typeof(Button).GetMethod("set_IsPressed", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(NUDButtonDown, new object[] { false });
    }

    private void NUDTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
      if (_number > maxvalue) NUDTextBox.Text = maxvalue.ToString();
      if (_number < minvalue) NUDTextBox.Text = minvalue.ToString();
      NUDTextBox.SelectionStart = NUDTextBox.Text.Length;
      if (_dlgValChange != null)
        _dlgValChange(enDataType.INT, null);
    }

  }
}
