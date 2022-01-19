using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using libParser;

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
