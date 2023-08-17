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
  /// <summary>
  /// Interaction logic for ctlExpressionEditor.xaml
  /// </summary>
  public partial class ctlExpressionEditor : UserControl
  {

    ExpressionGenerator _gen;
    List<ExpressionItem> _left;
    List<ExpressionItem> _op;
    List<ExpressionItem> _right;

    public string Expression { get{ return _tbExpression.getValue() ; } }
    public ctlExpressionEditor()
    {
      InitializeComponent();
      _tbExpression.setup(BatInspector.Properties.MyResources.FrmFilterFilterExpression, enDataType.STRING, 0, 100, true);
    }

    public void setup(ExpressionGenerator gen)
    {
      _gen = gen;

      _left = _gen.getAvailableOptions(enField.LEFT);
      foreach (ExpressionItem item in _left)
        _cbLeft.Items.Add(item.Text);
      _op = _gen.getAvailableOptions(enField.OPERATOR);
      foreach (ExpressionItem item in _op)
        _cbOperator.Items.Add(item.Text + " [" + item.HelpText + "]");
      _right = _gen.getAvailableOptions(enField.RIGHT);
      foreach (ExpressionItem item in _right)
        _cbRight.Items.Add(item.Text);
    }

    private void _cbLeft_SelectionChanged(object sender, SelectionChangedEventArgs e)
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
    }

    private void generateFormula()
    {
        string f = "(";
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
      generateFormula();
    }

    private void _cbRight_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
    {
      if ((_cbLeft.SelectedIndex >= 0) && (_cbRight.SelectedIndex >= 0) && (_cbOperator.SelectedIndex >= 0))
        generateFormula();
    }
  }
}
