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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Collections.ObjectModel;
using libParser;

namespace libScripter
{
  public enum enToken
  {
    NAME,
    UNKNOWN,
    COMMENT,
    LABEL,
    EOL,
    FORMULA
  }

  public delegate void delegateUpdateProgress(int perCent);

  public struct stLabelItem
  {
    public string Name;
    public int LineNr;

    public stLabelItem(string name, int line)
    {
      Name = name;
      LineNr = line;
    }
  }


  /// <summary>
  /// parses scripts and executes them
  /// </summary>
  public class Parser : BaseCommands
  {
    string _actLine = "";
    int _actPos = 0;
    string _actName = "";
    enToken _lastToken;
    Variables _vars;
    string _lastErr = "";
    string[] _lines;
    int _actLineNr = 0;
    List<stLabelItem> _labels;
    public const string ERROR_LEVEL = "ERROR_LEVEL";
    public const string RET_VALUE = "RET_VALUE";
    string _scriptName;
    bool _busy = false;
    Thread _oThread;
    string _logLine;
    ProcessRunner _proc;
    BaseCommands[] _commands;
    string _wrkDir;
    string _codeSection;
    CodeBlock _currBlock;
    List<CodeBlock> _blockStack;

    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="proc">process executer</param>
    /// <param name="delUpd">delegate to update progress bar</param>
    /// <param name="vars">Storage for variables</param>
    public Parser(ref ProcessRunner proc, BaseCommands[] commands, string wrkDir, 
          delegateUpdateProgress delUpd, Variables vars = null) : base(delUpd)
    {
      _parser = this;
      _proc = proc;
      _commands = commands;
      _wrkDir = wrkDir;
      _updateProgress = delUpd;
      _labels = new List<stLabelItem>();
      _currBlock = null;
      _blockStack = new List<CodeBlock>();
      if (vars == null)
        _vars = new Variables();
      else
        _vars = vars;

      _features = new ReadOnlyCollection<OptItem>(new[]
        {
        new OptItem("SET", "<varName> <Value> set variable", 2, fctSetVar),
        new OptItem("CALL", "<scriptName> call script", 1, fctCallScript),
        new OptItem("ON_ERROR_GOTO", "<label> ON Error goto label", 1, fctOnErrorGoto),
        new OptItem("GOTO", "<label> goto label", 1, fctGoto),
        new OptItem("SKIP_IF_EQUAL","<arg1> <arg2> skip next line if arguments are equal", 2, fctSkipIfEqual),
        new OptItem("SKIP_IF_NE","<arg1> <arg2> skip next line if arguments are not equal", 2, fctSkipIfNotEqual),
        new OptItem("RET_VALUE","<value> set return value of script", 1, fctRetValue),
        new OptItem("PAUSE", "wait for entry on console", 0, fctPause),
        new OptItem("FOR", "<option> <arg1> <arg2> ... <argn> execute code block", 0, fctFor),
        new OptItem("IF", "<condition>  execute code block if condition = TRUE", 1, fctIf),
        new OptItem("ELSE", "elsefor preceeding if", 0, fctElse),
        new OptItem("END", "end of code block", 0, fctEnd),
      });

      _options = new Options(_features, false);
    }

    public Variables VarTable { get { return _vars; } }
    public string LastError { get { return _lastErr; } }
    public bool Busy { get { return _busy; } }
    public int CurrentLineNr { get { return _actLineNr; } }
    public string CurrentLine { get { return _actLine; } }
    public static int ParsedLines { get; set; }


    public void startParsingSection(string code)
    {
      _codeSection = code;
      _busy = true;
      /* Create the thread object, passing in the abc.Read() method
      via a ThreadStart delegate. This does not start the thread. */
      _oThread = new Thread(new ThreadStart(this.ParseSection));
      _oThread.Start();
    }

    public void StartParsing(string name)
    {
      _busy = true;
      _scriptName = name;
      /* Create the thread object, passing in the abc.Read() method
      via a ThreadStart delegate. This does not start the thread. */
      _oThread = new Thread(new ThreadStart(this.ParseScript));
      _oThread.Start();
    }

    public void StartParsing(string wrkDir, string name)
    {
      _busy = true;
      _wrkDir = wrkDir;
      _scriptName = wrkDir + "\\" + name;
      /* Create the thread object, passing in the abc.Read() method
      via a ThreadStart delegate. This does not start the thread. */
      _oThread = new Thread(new ThreadStart(this.ParseScript));
      _oThread.Start();
    }

    public void StopParsing()
    {
      if (_oThread != null)
        _oThread.Abort();
      _busy = false;
    }

    public void log(string msg, enLogType type)
    {
      if (EnableLog)
        DebugLog.log(msg, type);
    }

    public void ParseSection()
    {
      log("start SCRIPT BLOCK", enLogType.INFO);
      ParseTextSection(_codeSection);
    }

    public int ParseScript(string name)
    {
      _scriptName = _wrkDir + "\\" + name;
      ParseScript();
      log("exit SCRIPT: " + name, enLogType.INFO);
      int retVal;
      int.TryParse(_parser.LastError.Substring(0,1), out retVal);
      return retVal;
    }

    public int ParseScript(string wrkDir, string name)
    {
      log("start SCRIPT: " + name, enLogType.INFO);
      _scriptName = wrkDir + "\\" + name;
      ParseScript();
      int retVal;
      int.TryParse(_parser.LastError.Substring(0, 1), out retVal);
      return retVal;
    }

    /// <summary>
    /// parse complete script
    /// </summary>
    /// <param name="name">file name of the script</param>
    /// <returns></returns>
    public void ParseScript()
    {
      log("start SCRIPT: " + _scriptName, enLogType.INFO);
      _busy = true;
      _lastErr = "0";
      try
      {
        _lines = File.ReadAllLines(_scriptName);
      }
      catch (Exception ex)
      {
        _lastErr = "1 error reading file " + _scriptName + ": " + ex.ToString();
        log(_lastErr, enLogType.ERROR);
        _busy = false;
        return;
      }
      ParseLines();
    }

    /// <summary>
    /// parse a block of text
    /// </summary>
    /// <param name="text"></param>
    public void ParseTextSection(string text)
    {
      if ((text != null) && (text.Length > 1))
      {
        _lines = text.Split('\n');
        ParseLines();
      }
    }

    public void ParseLines(string[] lines)
    {
      _lines = lines;
      ParseLines();
    }


      private void ParseLines()
    {
      _vars.SetValue(ERROR_LEVEL, "0");
      findLabels();
      _actLineNr = 0;
      while (_actLineNr < _lines.Length)
      {
        string result = "0";
        if ( (_currBlock == null) ||
             (_currBlock.Type != enBlockType.IF) || 
            ((_currBlock.Type == enBlockType.IF) && _currBlock.Execute))
          result = ParseLine(_lines[_actLineNr]);
        else
          result = mangeBlockLevel(_lines[_actLineNr]);
        _vars.SetValue(ERROR_LEVEL, result);
        _actLineNr++;
        if (_updateProgress != null)
          _updateProgress(_actLineNr * 100 / _lines.Length);
      }
      _busy = false;
    }

    public string getTextBlock(int startLine, int endLine)
    {
      string retVal = "";
      for (int i = startLine; i < _lines.Length; i++)
      {
        if (i <= endLine)
          retVal += _lines[i] + "\n";
        else
          break;
      }
      return retVal;
    }
    /// <summary>
    /// find all labels in scripts without executing scripts (for 1st pass)
    /// </summary>
    void findLabels()
    {
      int lineNr = 0;
      _labels.Clear();
      while (lineNr < _lines.Length)
      {
        _actLine = _lines[lineNr];
        _actPos = 0;
        GetToken();
        if (_lastToken == enToken.LABEL)
        {
          stLabelItem item = new stLabelItem(_actName, lineNr);
          _labels.Add(item);
        }
        lineNr++;
      }
    }

    /// <summary>
    /// get a single token of the script
    /// </summary>
    /// <returns>a token</returns>
    enToken GetToken()
    {
      char c;
      if (_actPos >= _actLine.Length)
      {
        _lastToken = enToken.EOL;
        return _lastToken;
      }

      for (; ; )
      {
        c = _actLine[_actPos];
        if (!Utils.isWhiteSpace(c))
          break;
        _actPos++;
        if (_actPos >= _actLine.Length)
        {
          _lastToken = enToken.EOL;
          return _lastToken;
        }
      }

      if (c == ':')
      {
        _actName = "";
        for (; ; )
        {
          _actPos++;
          if (_actPos >= _actLine.Length)
            break;
          if (Utils.isWhiteSpace(c))
            break;
          c = _actLine[_actPos];
          _actName += c;
        }
        _lastToken = enToken.LABEL;
      }

      else if (c == '#')
      {
        _actName = "";
        for (; ; )
        {
          _actPos++;
          if (_actPos >= _actLine.Length)
            break;
          c = _actLine[_actPos];
        }
        _lastToken = enToken.COMMENT;
      }

      else if (c == '\"')
      {
        _actName = "\"";
        for (; ; )
        {
          _actPos++;
          if (_actPos >= _actLine.Length)
            break;
          c = _actLine[_actPos];
          if (c == '\"')
          {
            _actName += '\"';
            _actPos++;
            if ((_actPos >= _actLine.Length) || (Utils.isWhiteSpace(_actLine[_actPos])))
              break;
            else
            {
              _lastToken = enToken.UNKNOWN;
              return _lastToken;
            }
          }
          _actName += c;
        }
        ReplaceVariables();
        _lastToken = enToken.NAME;
      }

      else if (Utils.isAlphaNum(c))
      {
        _actName = "" + c;
        for (; ; )
        {
          _actPos++;
          if (_actPos >= _actLine.Length)
            break;
          c = _actLine[_actPos];
          if (Utils.isWhiteSpace(c))
            break;
          _actName += c;
        }
        ReplaceVariables();
        _lastToken = enToken.NAME;
      }

      else if(c == '=')
      {
        _actPos++;
        c = _actLine[_actPos];
        if (c == '(')
        {
          _actName = "(";
          int countBrace = 1;
          while (_actPos < _actLine.Length - 1)
          {
            _actPos++;
            c = _actLine[_actPos];
            _actName += c;
            if (c == '(')
              countBrace++;
            else if (c == ')')
              countBrace--;
            if(countBrace == 0)
            {
              ReplaceVariables();
              _lastToken = enToken.FORMULA;
              _actPos++;
              break;
            }
          }
        }
        else
          _lastToken = enToken.UNKNOWN;
      }

      else
      {
        _lastToken = enToken.UNKNOWN;
      }
      return _lastToken;
    }

    string mangeBlockLevel(string line)
    {
      string retVal = "0";
      CodeBlock blk = checkForBlockStart(line);
      if (blk != null)
        _blockStack.Add(blk);

      if (_currBlock != null)
      {
        if (_currBlock == _blockStack.Last())
        {
            switch (_currBlock.Type)
            {
              case enBlockType.IF:
                bool ok = checkForElse(line) || checkForEnd(line);
                if (ok)
                  retVal = ParseLine(line);
                break;
            }
        }
        else
        {
          bool yes = checkForEnd(line);
          if (yes && _currBlock != _blockStack.Last())
            _blockStack.Remove(_blockStack.Last());
        }
      }
      return retVal;
    }

    CodeBlock checkForBlockStart(string line)
    {
      CodeBlock retVal = null;
      _actLine = line;
      _actPos = 0;
      if (GetToken() == enToken.NAME)
      {
        if (_actName == "FOR")
          retVal = new ForCsvCodeBlock(null, _actLineNr);
        if (_actName == "IF")
          retVal = new IfCodeBlock("",_actLineNr);
      }
      return retVal;
    }

    bool checkForEnd(string line)
    {
      bool retVal = false;
      _actLine = line;
      _actPos = 0;
      if (GetToken() == enToken.NAME)
      {
        if (_actName == "END")
          retVal = true;
      }
      return retVal;
    }

    bool checkForElse(string line)
    {
      bool retVal = false;
      _actLine = line;
      _actPos = 0;
      if (GetToken() == enToken.NAME)
      {
        if (_actName == "ELSE")
          retVal = true;
      }
      return retVal;
    }

    /// <summary>
    /// parse one single line
    /// </summary>
    /// <param name="line">line</param>
    /// <returns></returns>
    string ParseLine(string line)
    {
      _actLine = line;
      _actPos = 0;
      _actName = "";

      ParsedLines++;
      string cmd;
      List<string> args = new List<string>();

      string retVal = "0";
      if (GetToken() == enToken.COMMENT)
        return "0";
      if (_lastToken == enToken.LABEL)
        return "0";
      if (_lastToken != enToken.NAME)
        return "1";
      cmd = _actName; //.Replace("-","");
      args.Add(cmd);
      _logLine = _actLine;

      do
      {
        GetToken();
        if (_lastToken == enToken.NAME)
          args.Add(_actName);
        else if(_lastToken == enToken.FORMULA)
        {
          Expression formula = new Expression();
        //  foreach (VariableItem v in _vars.VarList)
        //    formula.addVariable(v.Name, v.Value);
          string result = formula.parseToString(_actName);
          if (formula.Errors > 0)
            DebugLog.log("Error parsing formula: '" + _actName + "'", enLogType.ERROR);
          args.Add(result);          
        }
        else if(_lastToken == enToken.UNKNOWN)
        {
          retVal = "1 unknown token";
          break;
        }
      } while (_lastToken != enToken.EOL);

      if (retVal == "0")
      {
        string[] argArr = args.ToArray<string>();
        // try all registered commands
        for (int i = 0; i < _commands.Length; i++)
        {
          retVal = _commands[i].run(argArr, this);
          if (retVal.IndexOf("unknown option") < 0)
            break;
        }

        //try script control commands
        if (retVal.IndexOf("unknown option") >= 0)
        {
          retVal = run(argArr, this);
        }
      }
      if (retVal != "0")
        DebugLog.log(_actLine + ": " + retVal, enLogType.ERROR);
      return retVal;
    }

    int fctSkipIfEqual(List<string> pars, out string ErrText)
    {
      int retVal = 0;
      ErrText = "";
      if (pars[0] == pars[1])
        _actLineNr++;
      return retVal;
    }

    int fctSkipIfNotEqual(List<string> pars, out string ErrText) {
      int retVal = 0;
      ErrText = "";
      if (pars[0] != pars[1])
        _actLineNr++;
      return retVal;
    }

    int fctSetVar(List<string> pars, out string ErrText)
    {
      int retVal = 0;
      ErrText = "";
      _vars.SetValue(pars[0], pars[1]);
      return retVal;
    }

    int fctRetValue(List<string> pars, out string ErrText)
    {
      int retVal = 0;
      int.TryParse(pars[0], out retVal);
      ErrText = "";
      _vars.SetValue(RET_VALUE, retVal.ToString());
      return 0;
    }
    int fctPause(List<string> pars, out string ErrText)
    {
      int retVal = 0;
      Console.WriteLine("tye any key to continue");
      Console.ReadLine();
      ErrText = "";
      return retVal;
    }

      

    /// <summary>
    /// goto line number in script
    /// </summary>
    /// <param name="label"></param>
    /// <returns>error code</returns>
      int fctGoto(List<string> pars, out string ErrText)
    {
      int retVal = 0;
      ErrText = "";
      bool found = false;
      foreach (stLabelItem lab in _labels)
      {
        if (lab.Name == pars[0])
        {
          found = true;
          _actLineNr = lab.LineNr - 1;  //decrement by 1 because line nr will be incremented automatically after ParseLine()
          break;
        }
      }
      if (!found)
      {
        retVal = 1;
        ErrText = "1 Label '" + pars[0] + "' not found";
      }
      return retVal;
    }

    int fctOnErrorGoto(List<string> pars, out string ErrText)
    {
      int retVal = 0;
      ErrText = "";
      VariableItem err = _vars.Find(ERROR_LEVEL);
      if (err != null)
      {
        if (err.Value != "0")
          retVal = fctGoto(pars, out ErrText);
      }
      else
      {
        retVal = 1;
        ErrText = "1 variable '" + ERROR_LEVEL + "' not defined!";
      }
      return retVal;
    }

    int fctCallScript(List<string> pars, out string ErrText)
    {
      int retVal = 0;
      ErrText = "";
      Parser p = new Parser(ref _proc, _commands, _wrkDir, _updateProgress, _vars);
      p.ParseScript(pars[0]);
      while (p.Busy)
        Thread.Sleep(10);
      ErrText = p.LastError;
      return retVal;
    }

    int fctFor(List<string> pars, out string ErrText)
    {
      int retVal = 0;
      ErrText = "";
      switch (pars[0])
      {
        case "CSV_ROWS":
          if ((_currBlock is ForCsvCodeBlock) && (_currBlock.StartLine == _actLineNr))
          {
            _currBlock.loopStart();
          }
          else
          {
            List<string> arg = new List<string>();
            for (int i = 1; i < pars.Count; i++)
              arg.Add(pars[i]);
            ForCsvCodeBlock blk = new ForCsvCodeBlock(arg, _actLineNr);
            ErrText = blk.ErrText;
            if (ErrText == "")
            {
              _currBlock = blk;
              _blockStack.Add(_currBlock);
            }
          }
          break;

        case "ITERATOR":
          if ((_currBlock is ForItCodeBlock) && (_currBlock.StartLine == _actLineNr))
          {
            _currBlock.loopStart();
          }
          else
          {
            List<string> arg = new List<string>();
            for (int i = 1; i < pars.Count; i++)
              arg.Add(pars[i]);
            ForItCodeBlock blk = new ForItCodeBlock(arg, _actLineNr, VarTable);
            ErrText = blk.ErrText;
            if (ErrText == "")
            {
              _currBlock = blk;
              _blockStack.Add(_currBlock);
            }
          }
          break;

      }
      return retVal;
    }

    int fctIf(List<string> pars, out string ErrText)
    {
      int retVal = 0;
      ErrText = "";
      IfCodeBlock blk = new IfCodeBlock(pars[0], _actLineNr);
      ErrText = blk.ErrText;
      if (ErrText == "")
      {
        _currBlock = blk;
        _blockStack.Add(_currBlock);
      }
      return retVal;
    }

    int fctEnd(List<string> pars, out string ErrText)
    {
      int retVal = 0;
      ErrText = "";
      if (_currBlock != null)
      {
        if (_currBlock.loopEnd())
        {
          _actLineNr = _currBlock.StartLine - 1;  //decrement by 1 because line nr will be incremented automatically after ParseLine()
        }
        else
        {
          _blockStack.Remove(_currBlock);
          if (_blockStack.Count > 0)
            _currBlock = _blockStack.Last();
          else
            _currBlock = null;
        }
      }
      else
      {
        ErrText = "END outside of code block";
        retVal = 1;
      }
      return retVal;
    }

    int fctElse(List<string> pars, out string ErrText)
    {
      int retVal = 0;
      ErrText = "";
      if ((_currBlock != null) &&(_currBlock.Type == enBlockType.IF))
      {
        IfCodeBlock blk = _currBlock as IfCodeBlock;
        blk.Execute = !blk.Execute;
      }
      else
      {
        ErrText = "ELSE not allowed outside of IF statement";
        retVal = 1;
      }
      return retVal;
    }
     
    /// <summary>
    /// replace variables in actual name
    /// </summary>
    /// <returns></returns>
    int ReplaceVariables()
    {
      int retVal = 0;
      do
      {
        int pos = 0;
        int posEnd = 0;

        pos = _actName.IndexOf('%');
        if ((pos >= _actName.Length) || (pos < 0))
          break;
        posEnd = _actName.IndexOf('%', pos + 1);
        if ((posEnd >= _actName.Length) || (posEnd < 0))
          break;
        string varName = _actName.Substring(pos + 1, posEnd - pos - 1);
        VariableItem var = _vars.Find(varName);
        if (var == null)
        {
          _lastErr = "Variable " + varName + " not found";
          retVal = 1;
          break;
        }
        string newName = "";
        if (pos > 0)
          newName = _actName.Substring(0, pos);
        newName += var.Value + _actName.Substring(posEnd + 1);
        _actName = newName;
      } while (true);
      return retVal;
    }
  }
}
