/********************************************************************************
 *               Author: Christian Müller
 *     Date of creation: 2023-08-18                                       
 *   Copyright (C) 2023: Christian Müller chrmue44(at)gmail(dot).com
 *
 *              Licence:  CC BY-NC 4.0 
 ********************************************************************************/
using BatInspector.Forms;
using BatInspector.Properties;
using libParser;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
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
    bool _formulaOk;
    public bool FormulaOk { get { return _formulaOk; } }

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
      _cbAppend.Items.Add(MyResources.Expand + " ...");
      foreach (ExpressionItem item in _append)
        _cbAppend.Items.Add(item.Text + " [" + item.HelpText + "]");
      
       init();
    }

    private void _cbLeft_DropDownClosed(object sender, EventArgs e)
    {
      try
      {
        int iLeft = _cbLeft.SelectedIndex;
        if ((string)_cbLeft.SelectedItem == MyResources.ExpGenNested)
        {
          frmExpression frm = new frmExpression(_gen, false, false);
          bool? res = frm.ShowDialog();
          if (res == true)
          {
            _left[iLeft].Text = frm.FilterExpression;
            _left[iLeft].DataType = AnyType.tType.RT_BOOL;
            _left[iLeft].Type = enExpType.EXPRESSION;
            _cbLeft.Items[iLeft] = frm.FilterExpression;
            _cbLeft.SelectedIndex = iLeft;
          }
        }
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
      catch(Exception ex) 
      {
        DebugLog.log("error closing dropdown Expressioneditor: " + ex.ToString(), enLogType.ERROR);
      }
    }

    private void _cbOperator_DropDownClosed(object sender, EventArgs e)
    {
      try
      {
        if ((_op[_cbOperator.SelectedIndex].Operator == enOperator.CONTAINS) ||
        (_op[_cbOperator.SelectedIndex].Operator == enOperator.CONTAINS_NOT))
        {
          _cbRight.Items.Clear();
          _cbRight.Items.Add(BatInspector.Properties.MyResources.ExpGenUserString);
          _cbRight.SelectedIndex = 0;
          _lblTodo.Visibility = Visibility.Visible;
          _lblTodo.Content = BatInspector.Properties.MyResources.ctlExpressionEditorTodString;
          _tbFreeTxt.Visibility = Visibility.Visible;
          _tbFreeTxt.Text = "";
        }
      }
      catch(Exception ex)
      {
        DebugLog.log("error closing operator dropsown expression editor: " + ex.ToString(), enLogType.ERROR);
      }
    }

    private void _cbRight_DropDownClosed(object sender, EventArgs e)
    {
      try
      {
        int iRight = _cbRight.SelectedIndex;
        if ((string)_cbRight.SelectedItem == MyResources.ExpGenNested)
        {
          frmExpression frm = new frmExpression(_gen, false, false);
          bool? res = frm.ShowDialog();
          if (res == true)
          {
            _right[iRight].Text = frm.FilterExpression;
            _right[iRight].DataType = AnyType.tType.RT_BOOL;
            _right[iRight].Type = enExpType.EXPRESSION;
            _cbRight.Items[iRight] = frm.FilterExpression;
            _cbRight.SelectedIndex = iRight;
          }
        }
        else
        {
          if ((_cbRight.SelectedIndex == 0) && (_left[_cbLeft.SelectedIndex].DataType == AnyType.tType.RT_STR))
          {
            _lblTodo.Visibility = Visibility.Visible;
            _lblTodo.Content = "Bitte String eingeben";
            _tbFreeTxt.Visibility = Visibility.Visible;
          }
          else
          {
            _lblTodo.Visibility = Visibility.Hidden;
            _tbFreeTxt.Visibility = Visibility.Hidden;
          }
        }
        if ((_cbLeft.SelectedIndex >= 0) && (_cbRight.SelectedIndex >= 0) && (_cbOperator.SelectedIndex >= 0))
          generateFormula();
        _cbAppend.IsEnabled = true;
        enableOk(true);
      }
      catch(Exception ex) 
      {
        DebugLog.log("error closing dropdown right exception editor: " + ex.ToString(), enLogType.ERROR);
      }
    }

    private void generateFormula()
    {
        string f = _prefix + "(";
      if ((_op[_cbOperator.SelectedIndex].Operator == enOperator.CONTAINS) ||
         (_op[_cbOperator.SelectedIndex].Operator == enOperator.CONTAINS_NOT))
      {
        f += "indexOf(" + _left[_cbLeft.SelectedIndex].Text + ",\"";
        if (_right[_cbRight.SelectedIndex].Type == enExpType.EXPRESSION)
          f += " " + _right[_cbRight.SelectedIndex].Text;
        else
          f += _tbFreeTxt.Text + "\"";
        f += ")";
        if (_op[_cbOperator.SelectedIndex].Operator == enOperator.CONTAINS)
          f += ">=0";
        else
          f += "<0";
      }
      else
      {
        f += _left[_cbLeft.SelectedIndex].Text + " ";
        f += _op[_cbOperator.SelectedIndex].Text + " ";
        if (AnyType.isStr(_left[_cbLeft.SelectedIndex].DataType))
        {
          if ((string)_cbRight.SelectedItem == BatInspector.Properties.MyResources.ExpGenUserString) 
            f += "\"" + _tbFreeTxt.Text + "\"";
          else
            f += _right[_cbRight.SelectedIndex].Text;
        }
        else if (_right[_cbRight.SelectedIndex].Type == enExpType.EXPRESSION)
          f += " " + _right[_cbRight.SelectedIndex].Text;
        else
          f += (_tbFreeTxt.Text);
      }
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
      _formulaOk = en;
      if (_dlgOk != null)
        _dlgOk(en);
    }

    private void _cbLeft_DropDownOpened(object sender, EventArgs e)
    {
      init(true); 
    }
  }
}
