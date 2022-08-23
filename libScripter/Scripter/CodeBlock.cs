using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libScripter
{

  public enum enBlockType
  {
    FOR_CSV_ROWS,
    FOR_IT,
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

  public class ForCsvCodeBlock : CodeBlock
  {
    int _actRow;
    int _endRow;
    Csv _csv;
    List<ColName> _cols;


    public ForCsvCodeBlock(List<string> args, int startLine) 
                  : base(enBlockType.FOR_CSV_ROWS, args, startLine)
    {
      if (args != null)
      {
        _errText = "";
        if (_args.Count > 0)
        {
          _csv = new Csv();
          _cols = new List<ColName>();
          int ret = _csv.read(_args[0]);
          if (ret == 0)
          {
            _actRow = 2;
            _endRow = _csv.RowCnt;
            initColNames();
          }
          else
            _errText = "FOR CSV_ROWS: error opening file: " + _args[0];
        }
        else
          _errText = "FOR: CSV_ROW : missing argument";
      }
    }

    public string getCell(string name)
    {
      string retVal = "";
      if (_csv != null)
      {
        foreach (ColName col in _cols)
        {
          if (name == col.Name)
          {
            retVal = _csv.getCell(_actRow, col.ColNr);
            break;
          }
        }
      }
      else
        _errText = "Error reading cell: " + name;
      return retVal;
    }

    public override void loopStart(string condition)
    {
    }


    public override bool loopEnd()
    {
      bool retVal = false;
      if (_actRow < _endRow)
      {
        _actRow++;
        retVal = true;
      }
      return retVal;
    }

    private void initColNames()
    {
      for(int i = 1; i <= _csv.ColCnt; i++)
      {
        ColName col = new ColName(_csv.getCell(1, i), i);
        _cols.Add(col);        
      }
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
                           : base(enBlockType.FOR_IT, args, startLine)
    {     
      _vars = vars;
      if (_args.Count > 2)
      {
        _errText = "";
        _itName = _args[0];
        bool ok = int.TryParse(args[1], out _itStart);
        ok |= int.TryParse(args[2], out _itEnd);
        _iterator = _itStart;
        _vars.VarList.set(_itName, _iterator);
      }
      else
        _errText = "IF: missing argument";
    }


    public override void loopStart(string condition)
    {
      _vars.VarList.set(_itName, _iterator);
    }

    public override bool loopEnd()
    {
      bool retVal = true;
      _iterator++;
      _vars.VarList.set(_itName, _iterator);
      if (_iterator > _itEnd)
        retVal = false;
      return retVal;
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
