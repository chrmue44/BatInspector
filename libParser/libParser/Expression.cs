/********************************************************************************
 *               Author: Christian Müller
 *      Date of cration: 2021-08-10                                       
 *   Copyright (C) 2022: christian Müller christian(at)chrmue(dot).de
 *
 *              Licence:
 * 
 * THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND,
 * EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
 ********************************************************************************/
using System;


namespace libParser
{
  public class Expression
  {
    uint _err;
    public uint Errors {  get { return _err; } }
    public VarList Variables { get { return _varList; } }
    public Expression()
    {
      _methList = new MthdListMath();
      _result = new MthdResult();
      _varList = new VarList();
      _methods = new Methods(_result);

      _methList.initMthdTab();
      _methList.setpResult(_result);
      _methList.setVarList(_varList);
      _methods.addMethodList(_methList);

      _varList.addConstant("PI", Math.PI);
      _varList.addConstant("e", Math.E);
      _parser = new CondParser(_varList, _methods);
    }

    public void addMethodList(MethodList list)
    {
      list.initMthdTab();
      list.setpResult(_result);
      list.setVarList(_varList);
      _methods.addMethodList(list);
    }

    public AnyType parse(string str)
    {
      AnyType retVal = _parser.parse(str);
      _err = _parser.getParseErrors();
      return retVal;
    }

    public string parseToString(string str)
    {
      AnyType v = _parser.parse(str);
      _err = _parser.getParseErrors();        
      v.changeType(AnyType.tType.RT_STR);
      if(_parser.getParseErrors() != 0)
        v.assign(_parser.getLastError().ToString());
      return v.getString();
    }

    public void setVariable(string name, string value, int index = 0)
    {
      VarName n = _varList.look(name);
      if(n != null)
      {
        AnyType v = new AnyType();
        v.setType(AnyType.tType.RT_STR);
        v.assign(value);
        n.setValue(index, v);
      }
    }

    public void setVariable(string name, double value, int index = 0)
    {
      VarName n = _varList.look(name);
      if (n != null)
      {
        AnyType v = new AnyType();
        v.setType(AnyType.tType.RT_FLOAT);
        v.assign(value);
        n.setValue(index, v);
      }
    }

    public void setVariable(string name, int value, int index = 0)
    {
      VarName n = _varList.look(name);
      if (n != null)
      {
        AnyType v = new AnyType();
        v.setType(AnyType.tType.RT_INT64);
        v.assign(value);
        n.setValue(index, v);
      }
    }


    MthdListMath _methList;
    MthdResult _result;
    VarList _varList;
    Methods _methods;
    CondParser _parser;
  }
}
