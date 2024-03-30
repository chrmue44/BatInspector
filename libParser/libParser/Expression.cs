/********************************************************************************
 *               Author: Christian Müller
 *     Date of creation: 2021-08-10                                       
 *   Copyright (C) 2023: Christian Müller chrmue44(at)gmail(dot).com
 *
 *              Licence:  CC BY-NC 4.0 
 ********************************************************************************/
 using System;


namespace libParser
{
  public class Expression
  {
    uint _err;
    public uint Errors {  get { return _err; } }
    public VarList Variables { get { return _varList; } }
    public Expression(VarList varlist)
    {
      _methList = new MthdListMath();
      _result = new MthdResult();
      if (varlist != null)
        _varList = varlist;
      else
        _varList = new VarList() ;           

      _methods = new Methods(_result);

      _methList.initMthdTab();
      _methList.setpResult(_result);
      _methList.setVarList(_varList);
      _methods.addMethodList(_methList);

      _varList.addConstant("PI", Math.PI);
      _varList.addConstant("e", Math.E);
      _varList.addConstant("TRUE", 1, AnyType.tType.RT_BOOL);
      _varList.addConstant("FALSE", 0, AnyType.tType.RT_BOOL);
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
      _varList.set(name, value, _methods, index);
    }

    public void setVariable(string name, double value, int index = 0)
    {
      _varList.set(name, value, _methods, index) ;
    }

    public void setVariable(string name, int value, int index = 0)
    {
      _varList.set(name, value, _methods, index);
    }


    MthdListMath _methList;
    MthdResult _result;
    VarList _varList;
    Methods _methods;
    CondParser _parser;
  }
}
