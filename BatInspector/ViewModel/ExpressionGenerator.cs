﻿/********************************************************************************
 *               Author: Christian Müller
 *     Date of creation: 2023-09-01                                       
 *   Copyright (C) 2023: Christian Müller chrmue44(at)gmail(dot).com
 *
 *              Licence:  CC BY-NC 4.0 
 ********************************************************************************/

using System.Collections.Generic;
using System.Windows.Media.Animation;
using libParser;
using BatInspector.Properties;

namespace BatInspector
{
  public enum enExpType
  {
    EXPRESSION,
    NEW_EXPRESSION,
    OPERATOR,
    CONST,
  }

  public enum enOperator
  {
    LESS,
    LESS_OR_EQUAL,
    EQUAL,
    GREATER,
    GREATER_OR_EQUAL,
    UNEQUAL,
    OR,
    AND,
    CONTAINS,
    CONTAINS_NOT,
    END_OF_EXPRESSION
  }

  public enum enField
  {
    LEFT,
    OPERATOR,
    RIGHT
  }

  public delegate List<FilterVarItem> dlgGetVarList();

  public class ExpressionItem
  {
    public enExpType Type { get; set; }
    public ExpressionItem Left { get; set; }
    public enOperator Operator { get; set; }
    public ExpressionItem Right { get; set; }
    public AnyType.tType DataType { get; set; }
    public string Text { get; set; }
    public string HelpText { get; set; }

    public ExpressionItem(enExpType type, enOperator op, string text, string helpText)
    {
      Type = type;
      Left = null;
      Operator = op;
      Right = null;
      Text = text;
      DataType = AnyType.tType.RT_STR;
      HelpText = helpText;
    }

    public ExpressionItem(enExpType type, string text, AnyType.tType dataType)
    {
      Type = type;
      Operator = enOperator.LESS;
      Left = null;
      Right = null;
      DataType = dataType;
      Text = text;
    }

    public ExpressionItem(enExpType type, string text, AnyType.tType dataType, string help)
    {
      Type = type;
      Operator = enOperator.LESS;
      Left = null;
      Right = null;
      DataType = dataType;
      Text = text;
      HelpText = help;
    }
  }

  public class ExpressionGenerator
  {
    enExpType _state = enExpType.EXPRESSION;
    dlgGetVarList _dlgGetVars;
    List<string> _species;
    public enExpType State { get { return _state; } }
    public List<string> Species { get { return _species; } }

    public ExpressionGenerator(dlgGetVarList dlgGetVars, List<string> species)
    {
      _dlgGetVars = dlgGetVars;
      _species = species;
    }

    public List<ExpressionItem> getAvailableOptions(enField field, AnyType.tType type = AnyType.tType.RT_HEXVAL)
    {
      List<ExpressionItem> retVal = new List<ExpressionItem>();
      switch(field) 
      {
        case enField.LEFT:
          retVal = getVariables();
          retVal.Add(new ExpressionItem(enExpType.NEW_EXPRESSION, MyResources.ExpGenNested, AnyType.tType.RT_FLOAT));
          break;

        case enField.RIGHT:
          if (AnyType.isNum(type))
            retVal.Add(new ExpressionItem(enExpType.CONST, BatInspector.Properties.MyResources.ExpGen_Nr, AnyType.tType.RT_FLOAT));
          else if (AnyType.isStr(type))
          {
            retVal.Add(new ExpressionItem(enExpType.CONST, BatInspector.Properties.MyResources.ExpGenUserString, AnyType.tType.RT_STR));
            foreach (string bat in _species)
              retVal.Add(new ExpressionItem(enExpType.CONST, bat, AnyType.tType.RT_STR));
          }
          else if (AnyType.isBool(type))
            retVal.Add(new ExpressionItem(enExpType.NEW_EXPRESSION, MyResources.ExpGenNested, AnyType.tType.RT_FLOAT));
          break;

        case enField.OPERATOR:
          retVal = getOperators(type);
          break;
      }
      return retVal;
    }

    private List<ExpressionItem> getVariables()
    {
      List<ExpressionItem> retVal = new List<ExpressionItem>();
      List<FilterVarItem> list = _dlgGetVars();
      foreach (FilterVarItem it in list)
      {
        retVal.Add(new ExpressionItem(enExpType.EXPRESSION, it.VarName, it.Type, it.Help));
      }
      return retVal;
    }

    private List<ExpressionItem> getOperators(AnyType.tType type)
    {
      List<ExpressionItem> retVal = new List<ExpressionItem>();
      if (AnyType.isNum(type))
      {
        retVal.Add(new ExpressionItem(enExpType.OPERATOR, enOperator.LESS, "<",BatInspector.Properties.MyResources.ExpGen_LT));
        retVal.Add(new ExpressionItem(enExpType.OPERATOR, enOperator.LESS_OR_EQUAL, "<=",BatInspector.Properties.MyResources.ExpGen_LTE));
      }
      if (AnyType.isNum(type) || AnyType.isStr(type) || AnyType.isBool(type))
      {
        retVal.Add(new ExpressionItem(enExpType.OPERATOR, enOperator.EQUAL, "==", BatInspector.Properties.MyResources.ExpGen_EQ));
        retVal.Add(new ExpressionItem(enExpType.OPERATOR, enOperator.UNEQUAL, "!=", BatInspector.Properties.MyResources.ExpGen_NE));
      }
      if (AnyType.isNum(type))
      {

        retVal.Add(new ExpressionItem(enExpType.OPERATOR, enOperator.GREATER, ">", BatInspector.Properties.MyResources.ExpGen_GT));
        retVal.Add(new ExpressionItem(enExpType.OPERATOR, enOperator.GREATER_OR_EQUAL, ">=",BatInspector.Properties.MyResources.ExpGen_GTE));
      }
      if (AnyType.isBool(type))
      {
        retVal.Add(new ExpressionItem(enExpType.OPERATOR, enOperator.OR, "||", BatInspector.Properties.MyResources.ExpGen_OR));
        retVal.Add(new ExpressionItem(enExpType.OPERATOR, enOperator.AND, "&&", BatInspector.Properties.MyResources.ExpGen_AND));
      }
      if(AnyType.isStr(type))
      {
        retVal.Add(new ExpressionItem(enExpType.OPERATOR, enOperator.CONTAINS, "", BatInspector.Properties.MyResources.ExpGen_CONTAINS));
        retVal.Add(new ExpressionItem(enExpType.OPERATOR, enOperator.CONTAINS_NOT, "", BatInspector.Properties.MyResources.ExpGen_CONTAINS_NOT));
      }
      return retVal;
    }
  }
}
