/********************************************************************************
 *               Author: Christian Müller
 *     Date of creation: 2021-08-10                                       
 *   Copyright (C) 2023: Christian Müller chrmue44(at)gmail(dot).com
 *
 *              Licence:  CC BY-NC 4.0 
 ********************************************************************************/
using System.Collections.Generic;
using libParser;


namespace libScripter
{

  public enum enBlockType
  {
    FOR,
    IF,
    WHILE
  }

  public abstract class CodeBlock
  {
    protected enBlockType _blockType;
    protected int _startLine;
    protected int _endLine;
    protected List<string> _args;
    protected string _errText;
    protected List<string> _lines;
    protected bool _execute;
    protected bool _inactive;
    public string ErrText { get { return _errText; } }
    public int StartLine { get { return _startLine; } }
    public enBlockType Type { get { return _blockType; } }
    public bool Execute { get { return _execute; } set { _execute = value; } }

    public CodeBlock(enBlockType type, List<string> args, int startLine)
    {
      _execute = true;
      _blockType = type;
      _args = args;
      _startLine = startLine;
      _lines = new List<string>();
    }

    public abstract bool loopEnd();
    public abstract void loopStart(string condition);
  }

  public struct ColName
  {
    public string Name;
    public int ColNr;

    public ColName(string name, int nr)
    {
      Name = name;
      ColNr = nr;
    }
  }

  public class ForItCodeBlock : CodeBlock
  {
    int _iterator;
    int _itStart;
    int _itEnd;
    string _itName;
    Variables _vars;

    public ForItCodeBlock(List<string> args, int startLine, Variables vars) 
                           : base(enBlockType.FOR, args, startLine)
    {     
      _vars = vars;
      if ((_args != null) && (_args.Count > 2))
      {
        _errText = "";
        _itName = _args[0];
        Expression exp = new Expression(vars.VarList);
        AnyType start = exp.parse(args[1]);
        start.changeType(AnyType.tType.RT_INT64);
        _itStart = (int)start.getInt64();
        AnyType end = exp.parse(args[2]);
        end.changeType(AnyType.tType.RT_INT64);
        _itEnd = (int)end.getInt64();

        _iterator = _itStart;
        loopStart("");
      }
      else
        _errText = "IF: missing argument";
    }


    public override void loopStart(string condition)
    {
      _vars.VarList.set(_itName, _iterator);
      _execute = (_iterator < _itEnd);
    }

    public override bool loopEnd()
    {
      _iterator++;
      _vars.VarList.set(_itName, _iterator);
      return _execute;
    }
  }

  public class WhileCodeBlock : CodeBlock
  {
    bool _condition;
    public bool Condition { get { return _condition; } }

    public WhileCodeBlock(string condition, int startLine)
                           : base(enBlockType.WHILE, null, startLine)
    {
      loopStart(condition);
      _errText = "";
    }

    public override void loopStart(string condition)
    {
      if ((condition == "true") || (condition == "TRUE") || (condition == "1"))
        _execute = true;
      else
        _execute = false;
      _condition = _execute;
    }

    public override bool loopEnd()
    {
      return _execute;
    }

  }

  public class IfCodeBlock : CodeBlock
  {
    bool _condition;

    public bool Condition { get { return _condition; } }

    public IfCodeBlock(string condition, int startLine)
                           : base(enBlockType.IF, null, startLine)
    {
      loopStart(condition);
      _errText = "";
    }


    public override void loopStart(string condition)
    {
      if ((condition == "true") || (condition == "TRUE") || (condition == "1"))
        _execute = true;
      else
        _execute = false;
      _condition = _execute;
    }

    public override bool loopEnd()
    {
      bool retVal = false;
      return retVal;
    }
  }
}
