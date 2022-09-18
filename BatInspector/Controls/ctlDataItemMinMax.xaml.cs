
using BatInspector.Forms;
using libParser;
using System.Globalization;
using System.Windows.Controls;

namespace BatInspector.Controls
{

  /// <summary>
  /// Interaktionslogik für ctlDataItem.xaml
  /// </summary>
  public partial class ctlDataItemMinMax : UserControl
  {
    enDataType _type = enDataType.STRING;
    int _decimals = 3;
    double _minValDouble;
    double _maxValDouble;
    int _minValInt;
    int _maxValInt;
    string _minValString;
    string _maxValString;
    dlgValueChanged _dlgValChange = null;
    
    public double MaxDouble { get { return _maxValDouble; } }
    public double MinDouble { get { return _minValDouble; } }

    // public bool Focusable { set { _tb.Focusable = value; } get { return _tb.Focusable; } }

    public void setup(string label, enDataType type, int decimals = 2, int widthLbl = 80, int widthTb = 80, int height = 22, dlgValueChanged dlgValChange = null)
    {
      _lbl.Text= label;
      _lbl.Focusable = false;
      _type = type;
      _decimals = decimals;
      _dlgValChange = dlgValChange;
      _tbMin.Width = widthTb;
      _tbMax.Width = widthTb;
      _lbl.Width = widthLbl;
      _lbl.Height = height;
      _tbMin.Height = Height;
      _tbMax.Height = height;
      _lblmin.Height = Height;
      _lblmax.Height = Height;
    }

    public void setMinValue(int val)
    {
      setValue(val, ref _minValInt, _tbMin);
    }

    public void setMaxValue(int val)
    {
      setValue(val, ref _maxValInt, _tbMax);
    }

    public void setMinValue(double val)
    {
      setValue(val, ref _minValDouble, _tbMin);     
    }

    public void setMaxValue(double val)
    {
      setValue(val, ref _maxValDouble, _tbMax);
    }

    private void setValue(int newVal, ref int val, TextBox tb)
    {
      if (_type == enDataType.INT)
      {
        val = newVal;
        tb.Text = newVal.ToString();
      }
      else
        DebugLog.log("wrong data type for ctlDataItem: " + _lbl.Text, enLogType.ERROR);
    }


    private void setValue(double newVal, ref double val, TextBox tb)
    {
      string format = "0.";
      for (int i = 0; i < _decimals; i++)
        format += "#";
      if (_type == enDataType.DOUBLE)
      {
        val = newVal;
        tb.Text = newVal.ToString(format, CultureInfo.InvariantCulture);
      }
      else
        DebugLog.log("wrong data type for ctlDataItemMinMax: " + _lbl.Text, enLogType.ERROR);
    }

    private void setValue(string newVal, ref string val, TextBox tb)
    {
      if (_type == enDataType.STRING)
      {
        tb.Text = newVal;
        val = newVal;
      }
      else
        DebugLog.log("wrong data type for ctlDataItem: " + _lbl.Text, enLogType.ERROR);
    }


    public string getMinValue()
    {
      switch(_type)
      {
        case enDataType.STRING:
          return _minValString;
        case enDataType.DOUBLE:
          return _minValDouble.ToString();
        case enDataType.INT:
          return _minValInt.ToString();
      }
      return "";
    }

    public string getMaxValue()
    {
      switch (_type)
      {
        case enDataType.STRING:
          return _maxValString;
        case enDataType.DOUBLE:
          return _maxValDouble.ToString();
        case enDataType.INT:
          return _maxValInt.ToString();
      }
      return "";
    }

    public ctlDataItemMinMax()
    {
      InitializeComponent();
    }

    private void _tb_TextChangedMin(object sender, TextChangedEventArgs e)
    {
      switch(_type)
      {
        case enDataType.INT:
          int.TryParse(_tbMin.Text, out _minValInt);
          if (_dlgValChange != null)
            _dlgValChange(enDataType.INT, _minValInt);
          break;

        case enDataType.DOUBLE:
          double.TryParse(_tbMin.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out _minValDouble);
          if (_dlgValChange != null)
            _dlgValChange(enDataType.DOUBLE, _minValDouble);
          break;

        case enDataType.STRING:
          _minValString = _tbMin.Text;
          if (_dlgValChange != null)
            _dlgValChange(enDataType.STRING, _minValString);
          break;
      }
    }
    private void _tb_TextChangedMax(object sender, TextChangedEventArgs e)
    {
      switch (_type)
      {
        case enDataType.INT:
          int.TryParse(_tbMax.Text, out _maxValInt);
          if (_dlgValChange != null)
            _dlgValChange(enDataType.INT, _maxValInt);
          break;

        case enDataType.DOUBLE:
          double.TryParse(_tbMax.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out _maxValDouble);
          if (_dlgValChange != null)
            _dlgValChange(enDataType.DOUBLE, _maxValDouble);
          break;

        case enDataType.STRING:
          _maxValString = _tbMax.Text;
          if (_dlgValChange != null)
            _dlgValChange(enDataType.STRING, _maxValString);
          break;
      }
    }
  }
}
