using libParser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;


namespace BatInspector.Controls
{
  public delegate void dlgEnableOk(bool enable);
  /// <summary>
  /// Interaction logic for ctlExpressionEditor.xaml
  /// </summary>
  public partial class ctlExpressionEditor : UserControl
  {

    ExpressionGenerator _gen;
    List<ExpressionItem> _left;
    List<ExpressionItem> _op;
    List<ExpressionItem> _right;
    List<ExpressionItem> _append;
    dlgEnableOk  _dlgOk;
    string _prefix;

    public string Expression { get{ return _tbExpression.getValue() ; } }
    public ctlExpressionEditor()
    {
      InitializeComponent();
      _tbExpression.setup(BatInspector.Properties.MyResources.FrmFilterFilterExpression, enDataType.STRING, 0, 100, true);
    }

    public void setup(ExpressionGenerator gen, dlgEnableOk dlkOk)
    {
      _gen = gen;
      _prefix = "";
      _dlgOk = dlkOk;
      _left = _gen.getAvailableOptions(enField.LEFT);
      foreach (ExpressionItem item in _left)
        _cbLeft.Items.Add(item.Text);

      _op = _gen.getAvailableOptions(enField.OPERATOR);
      foreach (ExpressionItem item in _op)
        _cbOperator.Items.Add(item.Text + " [" + item.HelpText + "]");

      _right = _gen.getAvailableOptions(enField.RIGHT);
      foreach (ExpressionItem item in _right)
        _cbRight.Items.Add(item.Text);

      _append = new List<ExpressionItem>();
      _append.Add(new ExpressionItem(enExpType.OPERATOR, enOperator.OR, "||", "oder"));
      _append.Add(new ExpressionItem(enExpType.OPERATOR, enOperator.AND, "&&", "und"));
      _cbAppend.Items.Add("erweitern...");
      foreach (ExpressionItem item in _append)
        _cbAppend.Items.Add(item.Text + " [" + item.HelpText + "]");
      
       init();
    }

    private void _cbLeft_DropDownClosed(object sender, EventArgs e)
    {
      int iLeft = _cbLeft.SelectedIndex;
      _op = _gen.getAvailableOptions(enField.OPERATOR, _left[iLeft].DataType);
      _cbRight.Items.Clear();
      _lblHelpLeft.Content = _left[iLeft].HelpText;
      _right = _gen.getAvailableOptions(enField.RIGHT, _left[iLeft].DataType);
      foreach (ExpressionItem item in _right)
        _cbRight.Items.Add(item.Text);

      _cbOperator.Items.Clear();
      foreach (ExpressionItem item in _op)
        _cbOperator.Items.Add(item.Text + " [" + item.HelpText + "]");
      if (AnyType.isNum(_left[iLeft].DataType))
      {
        _lblTodo.Visibility = Visibility.Visible;
        _lblTodo.Content = BatInspector.Properties.MyResources.ctlExpressionEditorTodoNr;
        _tbFreeTxt.Visibility = Visibility.Visible;
        _cbRight.SelectedIndex = 0;
      }
      if (AnyType.isStr(_left[iLeft].DataType))
      {
        _lblTodo.Visibility = Visibility.Hidden;
        _tbFreeTxt.Visibility = Visibility.Hidden;
      }

      _cbOperator.IsEnabled = true;
      _cbRight.IsEnabled = true;
    }

    private void generateFormula()
    {
        string f = _prefix + "(";
        f += _left[_cbLeft.SelectedIndex].Text + " ";
        f += _op[_cbOperator.SelectedIndex].Text + " ";
        if (AnyType.isStr(_left[_cbLeft.SelectedIndex].DataType))
          f += "\"" + _right[_cbRight.SelectedIndex].Text + "\"";
        else
          f += (_tbFreeTxt.Text);
        f += ")";
        _tbExpression.setValue(f);
    }

    private void _tbFreeTxt_TextChanged(object sender, TextChangedEventArgs e)
    {
      if ((_tbFreeTxt.Text.Length > 0) && (_cbRight.SelectedIndex == 0))
      {
        generateFormula();
        _cbAppend.IsEnabled = true;
        enableOk(true);
      }
    }

    private void _cbRight_DropDownClosed(object sender, EventArgs e)
    {
      if ((_cbLeft.SelectedIndex >= 0) && (_cbRight.SelectedIndex >= 0) && (_cbOperator.SelectedIndex >= 0))
        generateFormula();
      _cbAppend.IsEnabled = true;
      enableOk(true);
    }

    private void _cbAppend_DropDownClosed(object sender, EventArgs e)
    {
      if ((_cbAppend.SelectedIndex > 0))
      {
        _prefix = _tbExpression.getValue() + " " + _append[_cbAppend.SelectedIndex - 1].Text + " ";
        init();
      }
    }

    private void init(bool fromCbLeft = false)
    {
      if(!fromCbLeft)
        _cbLeft.SelectedIndex = -1;
      _cbOperator.SelectedIndex = -1;
      _cbRight.SelectedIndex = -1;
      _cbAppend.SelectedIndex = 0;
      _tbFreeTxt.Text = "";
      _cbOperator.IsEnabled = false;
      _cbRight.IsEnabled = false;
      _cbAppend.IsEnabled = false;
      enableOk(false);
    }

    private void enableOk(bool en)
    {
      if (_dlgOk != null)
        _dlgOk(en);
    }

    private void _cbLeft_DropDownOpened(object sender, EventArgs e)
    {
      init(true); 
    }
  }
}
