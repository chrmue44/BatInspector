using BatInspector.Controls;
using libParser;
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
  /// Interaktionslogik für ctlDataItem.xaml
  /// </summary>
  public partial class ctlDataItem : UserControl
  {
    enDataType _type = enDataType.STRING;
    int _decimals = 3;
    double _valDouble;
    int _valInt;
    uint _valUInt;
    string _valString;
    dlgValueChanged _dlgValChange = null;
    

   // public bool Focusable { set { _tb.Focusable = value; } get { return _tb.Focusable; } }

    public void setup(string label, enDataType type, int decimals = 2, int widthLbl = 80, int widthTb = 80, bool edit = false, dlgValueChanged dlgValChange = null)
    {
      _lbl.Content= label;
      _lbl.Focusable = false;
      _type = type;
      _decimals = decimals;
      _dlgValChange = dlgValChange;
      _tb.Focusable = edit;
      _tb.Width = widthTb;
      _lbl.Width = widthLbl;

    }

    public void setValue(uint val)
    {
      if (_type == enDataType.UINT)
      {
        _valUInt = val;
        _tb.Text = val.ToString();
      }
      else
        DebugLog.log("wring data type for ctlDataItem: " + _lbl.Content, enLogType.ERROR);
    }

    public void setValue (int val)
    {
      if (_type == enDataType.INT)
      {
        _valInt = val;
        _tb.Text = val.ToString();
      }
      else
        DebugLog.log("wring data type for ctlDataItem: " + _lbl.Content, enLogType.ERROR);
    }
    public void setValue(double val)
    {
      string format = "0.";
      for (int i = 0; i < _decimals; i++)
        format += "#";
      if (_type == enDataType.DOUBLE)
      {
        _valDouble = val;
        _tb.Text = val.ToString(format);
      }
      else
        DebugLog.log("wrong data type for ctlDataItem: " + _lbl.Content, enLogType.ERROR);

    }
    public void setValue(string val)
    {
      if (_type == enDataType.STRING)
      {
        _tb.Text = val;
        _valString = val;
      }
      else
        DebugLog.log("wrong data type for ctlDataItem: " + _lbl.Content, enLogType.ERROR);

    }

    public int getIntValue()
    {
      return _valInt;
    }

    public uint getUIntValue()
    {
      return _valUInt;
    }

    public string getValue()
    {
      switch(_type)
      {
        case enDataType.STRING:
          return _valString;
        case enDataType.DOUBLE:
          return _valDouble.ToString();
        case enDataType.INT:
          return _valInt.ToString();
        case enDataType.UINT:
          return _valUInt.ToString();
      }
      return "";
    }

    public ctlDataItem()
    {
      InitializeComponent();
    }

    private void _tb_TextChanged(object sender, TextChangedEventArgs e)
    {
      switch(_type)
      {
        case enDataType.INT:
          int.TryParse(_tb.Text, out _valInt);
          if (_dlgValChange != null)
            _dlgValChange(enDataType.INT, _valInt);
          break;

        case enDataType.UINT:
          int.TryParse(_tb.Text, out _valInt);
          if (_dlgValChange != null)
            _dlgValChange(enDataType.UINT, _valInt);
          break;

        case enDataType.DOUBLE:
          double.TryParse(_tb.Text, out _valDouble);
          if (_dlgValChange != null)
            _dlgValChange(enDataType.DOUBLE, _valDouble);
          break;

        case enDataType.STRING:
          _valString = _tb.Text;
          if (_dlgValChange != null)
            _dlgValChange(enDataType.STRING, _valString);
          break;
      }
    }
  }

  public delegate void dlgValueChanged(enDataType type , object val);

}
