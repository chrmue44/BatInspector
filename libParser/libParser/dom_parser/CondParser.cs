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
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libParser
{
  public class CondParser
  {
    public enum tToken
    {
      NAME,
      NUMBER,
      KOMMA,
      PLUS,
      MUL,
      DIV,
      END,
      AND,
      OR,
      NOT,
      GREATER,
      GREATER_EQUAL,
      LESSER,
      LESSER_EQUAL,
      EQUAL,
      UNEQUAL,
      BRACE_OPEN,
      BRACE_CLOSE,
      SQR_BRACK_OPEN,
      SQR_BRACK_CLOSE,
      //  CRL_BRACE_OPEN,
      //  CRL_BRACE_CLOSE,
      MINUS,
      ASSIGN,
      STRING,
      BAD_TOKEN,
      COMMENT,
      LABEL
    };


    public CondParser(VarList varList, Methods methods)
    {
      _varList = varList;
      _methods = methods;
    }


    // parst den Inhalt des Strings
    public AnyType parse(string Str)
    {
      Error.init();
      _len = Str.Length;
      _str = Str;
      _pos = 0;
      _errors = 0;
      getToken();
      AnyType retVal = expr();

      if (Error.get() != 0)
      {
        _errors++;
        _lastError = Error.get();
        retVal.assign("ERROR " + _lastError.ToString());
      }
      return retVal;
    }


    // parst den Inhalt eines Ergebnis-Strings
    public tParseError parseResultStr(string Str, out MthdResult Result)
    {
      Result = new MthdResult();
      m_CharStr = null;
      _len = Str.Length;
      _str = Str;
      return parseResultStr(ref Result);
    }


    // parst den Inhalt eines Ergebnis-Strings
    public tParseError parseResultStr(string Str, int BufLen, out MthdResult Result)
    {
      Result = new MthdResult();
      _len = BufLen;
      _str = null;
      m_CharStr = Str;
      return parseResultStr(ref Result);
    }


    // gibt die Anzahl der Fehler beim Parsen aus
    public UInt32 getParseErrors()
    {
      return _errors;
    }


    // letzten Fehler fragen
    public tParseError getLastError()
    {
      return _lastError;
    }


    // prueft ob die Zeile ein Label enthaelt
    bool checkLabel(string Line, out string Label)
    {
      bool RetVal = false;
      Label = "";
      _len = Line.Length;
      _str = Line;
      _pos = 0;
      _errors = 0;
      getToken();
      if (_currTok == tToken.NAME)
      {
        Label = _nameString;
        getToken();
        if (_currTok == tToken.LABEL)
          RetVal = true;
      }
      return RetVal;
    }
 

  tParseError parseResultStr(ref MthdResult Result)
    {
      bool Finished = false;
      _pos = 0;
      _errors = 0;
      _lastError = 0;
      bool ErrorCode = true;
      //	Result.flush();
      while (!Finished)
      {
        getToken();
        switch (_currTok)
        {
          case tToken.NUMBER:
            if (ErrorCode)
            {
              _numValue.changeType(AnyType.tType.RT_INT64);
              reportError((tParseError)_numValue.getInt64());
            }
            Result.addValue(_numValue);
            break;
          case tToken.STRING:
            Result.addValue(_nameString);
            break;
          case tToken.END:
            Finished = true;
            break;
          default:
            reportError(tParseError.RESULTSTRING);
            break;
        }
        ErrorCode = false;
        if (!Finished)
        {
          getToken();
          if (_currTok != tToken.KOMMA)
          {
            Finished = true;
            if (_currTok != tToken.END)
              reportError(tParseError.RESULTSTRING);
          }
        }
      }
      return _lastError;
    }


    // Verarbeitung von Ausdruecken
    AnyType expr()
    {
      AnyType left = new AnyType( term());
      for (; ; )
      {
        switch (_currTok)
        {
          case tToken.PLUS:
            getToken();
            left.assign(left + term());
            break;

          case tToken.MINUS:
            getToken();
            left.assign(left - term());
            break;

          case tToken.OR:
            getToken();
            left.assign(left | term());
            break;

          default:
            return left;
        }
      }
    }


    // verarbeitung von Termen
    AnyType term()
    {
      AnyType left = new AnyType();
      left.assign(prim());
      for (; ; )
      {
        switch (_currTok)
        {
          case tToken.MUL:
            getToken();
            left.assign(left * prim());
            break;

          case tToken.DIV:
            getToken();
            left.assign(left / prim());
            break;

          case tToken.AND:
            getToken();
            left.assign(left & prim());
            break;

          case tToken.LESSER:
            getToken();
            left.assignBool(left < prim());
            break;

          case tToken.LESSER_EQUAL:
            getToken();
            left.assignBool(left <= prim());
            break;

            case tToken.GREATER:
            getToken();
            left.assignBool(left > prim());
            break;

          case tToken.GREATER_EQUAL:
            getToken();
            left.assignBool(left >= prim());
            break;

          case tToken.EQUAL:
            getToken();
            left.assignBool(left == prim());
            break;

          case tToken.UNEQUAL:
            getToken();
            left.assignBool(left != prim());
            break;

          default:
            return left;
        }
      }
    }


    // Verarbeitung von Primaries
    AnyType prim()
    {
      AnyType RetVal = new AnyType(_varList, _methods);

      switch (_currTok)
      {
        case tToken.NUMBER:
          getToken();
          RetVal.assign(_numValue);
          return RetVal;

        case tToken.STRING:
          getToken();
          RetVal.assign(_nameString);
          return RetVal;

        case tToken.NAME:
          getToken();
          switch (_currTok)
          {
            case tToken.ASSIGN:
              {
                Int32 Err;
                _varList.set(_nameString, 0, _methods);
                VarName n = _varList.get(_nameString, _methods);
                getToken();
                if (!n.isConst())
                  n.setValue(0, expr());
                else
                  Error.report(tParseError.ASSIGN_CONST);
                Err = n.getValue(0, ref RetVal);
                Debug.Assert(Err == 0, "clCondParser:Error reading variable");
                break;
              }

            case tToken.BRACE_OPEN:
              RetVal = function(_nameString);
              if (_currTok != tToken.BRACE_CLOSE)
              {
                reportError(tParseError.BRACECLOSE);
                RetVal.assign(1.0);
              }
              getToken();
              break;

            case tToken.LABEL:
              {
                string Fname = "addLabel";
                AnyType LabName = new AnyType();
                LabName.assign(_nameString);
                List<AnyType> pLabName = new List<AnyType>();
                pLabName.Add(LabName);
                tParseError err = _methods.executeFunction(Fname, 1, pLabName, ref RetVal);
                if (err != 0)
                  Error.report(err);
              }
              break;

            case tToken.SQR_BRACK_OPEN:
              {
                string Name = _nameString;
                getToken();
                RetVal.assign(expr());
                RetVal.changeType(AnyType.tType.RT_INT64);
                // Index fuer Array
                int Index = (int)RetVal.getInt64();
                if (_currTok != tToken.SQR_BRACK_CLOSE)
                {
                  reportError(tParseError.SQR_BRACK_CLOSE);
                  break;
                }

                getToken();
                if (_currTok == tToken.ASSIGN)
                {
                  _varList.set(Name, 0, _methods);
                  VarName n = _varList.get(Name, _methods);
                  Int32 Err;
                  getToken();
                  RetVal.assign(expr());
                  Err = n.setValue(Index, RetVal);
                  if (Err != 0)
                    reportError(tParseError.ARRAY_INDEX);
                  getToken();
                }
                else
                {
                  VarName n = _varList.get(Name);
                  if (n != null)
                  {
                    Int32 Err;
                    Err = n.getValue(Index, ref RetVal);
                    if (Err != 0)
                      reportError(tParseError.ARRAY_INDEX);
                  }
                  else
                  {
                    RetVal.assign(1.0);
                    reportError(tParseError.VARIABLE);
                  }
                }
              }
              break;

            default:
              {
                VarName n = _varList.get(_nameString);
                if (n != null)
                {
                  Int32 Err;
                  Err = n.getValue(0, ref RetVal);
                  Debug.Assert(Err == 0, "clCondParser:Error reading variable");
                }
                else
                {
                  RetVal.assign(1.0);
                  reportError(tParseError.VARIABLE);
                }
              }
              break;
          }
          break;

        case tToken.NOT:
          getToken();
          RetVal.assign(prim());
          RetVal.assign(!RetVal);
          break;

        case tToken.MINUS:
          getToken();
          RetVal.assign(prim());
          RetVal.negate(); 
          break;

        case tToken.BRACE_OPEN:
          getToken();
          RetVal.assign(expr());
          if (_currTok != tToken.BRACE_CLOSE)
          {
            RetVal.assign(1.0);
            reportError(tParseError.BRACECLOSE);
          }
          getToken();
          break;


        case tToken.END:
        case tToken.BRACE_CLOSE:  //fuer funktionen mit keinen Parametern!, koennte evtl. an anderer Stelle Aerger machen(?)
          RetVal.assign(1.0);
          break;

        case tToken.COMMENT:
          RetVal.assign(_nameString);
          RetVal.changeType(AnyType.tType.RT_COMMENT);
          break;
        /*
            case CRL_BRACE_OPEN: {
                 RetVal.changeType(RT_COMPLEX);
                 getToken();
                 clAnyType re = expr();
                 if(m_CurrTok != KOMMA)
                   clError::report(COMPLEX_COMMA);
                 getToken();
                 clAnyType im = expr();
                 if(m_CurrTok != CRL_BRACE_CLOSE)
                   clError::report(CRL_BRACE_CLOSE);
                 getToken();
                 re.changeType(RT_FLOAT);
                 RetVal.setComplex(re.getFloat(),im.getComplexIm());
                 }
                 break;
        */
        default:
          RetVal.assign(1.0);
          reportError(tParseError.PRIMARY);
          break;

      }
      return RetVal;
    }


    class ParListItem
    {
      public AnyType Par;
      public ParListItem Next;
    };


    // Verarbeitung eines Funktionsasudrucks
    AnyType function(string Name)
    {
      string FuncName = Name;
      AnyType RetVal = new AnyType();
      bool ErrorFlag = false;
      int ParCnt = 0;
      ParListItem pPar;
      ParListItem pLast = null;
      // Zeiger auf den Anfang der Parametertabelle
      ParListItem pFirst = null;

      getToken();
      for (; ; )
      {
        // keine Parameter
        if (_currTok == tToken.BRACE_CLOSE)
          break;
        // neuen Parameter erzeugen
        pPar = new ParListItem();
        pPar.Par = expr();
        pPar.Next = null;
        ParCnt++;

        if (pFirst == null)
          pFirst = pPar;
        if (pLast != null)
          pLast.Next = pPar;

        pLast = pPar;

        if ((_currTok != tToken.KOMMA) && (_currTok != tToken.BRACE_CLOSE))
        {
          reportError(tParseError.KOMMA);
          ErrorFlag = true;
          RetVal.assign(1.0);
          break;
        }
        if (_currTok == tToken.BRACE_CLOSE)
          break;
        getToken();
      }

      // Parametertabelle erzeugen
      if (!ErrorFlag)
      {
        ParListItem Iter = pFirst;
        List<AnyType> argv = null;
        if (ParCnt > 0)
        {
          argv = new List<AnyType>();

          while (Iter != null)
          {
            argv.Add(Iter.Par);
            Iter = Iter.Next;
          }
        }

        tParseError err = _methods.executeFunction(FuncName, ParCnt, argv, ref RetVal);
        if (err != 0)
          Error.report(err);
      }


      return RetVal;
    }

    bool isalpha(char ch)
    {
      string specChars = "_";
      return Char.IsLetter(ch) || (specChars.IndexOf(ch) >= 0);
    }

    bool isspace(char ch)
    {
      return (ch == ' ') || (ch == '\t');
    }

    bool isdigit(char ch)
    {
      return Char.IsNumber(ch);
    }

    bool isalnum(char ch)
    {
      return isalpha(ch) || isdigit(ch);
    }

    // holt das naechste Token
    tToken getToken()
    {
      tToken RetVal = tToken.BAD_TOKEN;
      tParseError Result = 0;
      char ch;
      do
      {
        if (getNextChar(out ch) != 0)
        {
          _currTok = tToken.END;
          return tToken.END;
        }
        if (ch <= 0)
        {
          _currTok = tToken.END;
          return tToken.END;
        }
      } while (isspace(ch));
      switch (ch)
      {
        case ',':
          RetVal = tToken.KOMMA;
          break;

        case '+':
          RetVal = tToken.PLUS;
          break;

        case '*':
          RetVal = tToken.MUL;
          break;

        case '/':
          Result = getNextChar(out ch);
          if ((Result == tParseError.SUCCESS) && (ch == '/'))
          {
            while (Result == 0)
            {
              Result = getNextChar(out ch);
              if (Result == 0)
                _nameString += ch;
            }
            RetVal = tToken.COMMENT;
          }
          else
          {
            putBack();
            RetVal = tToken.DIV;
          }
          break;

        case '-':
          RetVal = tToken.MINUS;
          break;

        case '(':
          RetVal = tToken.BRACE_OPEN;
          break;

        case ')':
          RetVal = tToken.BRACE_CLOSE;
          break;

        case '[':
          RetVal = tToken.SQR_BRACK_OPEN;
          break;

        case ']':
          RetVal = tToken.SQR_BRACK_CLOSE;
          break;
        /*
            case '{':
                 RetVal = CRL_BRACE_OPEN;
                 break;

            case '}':
                 RetVal = CRL_BRACE_CLOSE;
                 break;
        */
        case '<':
          Result = getNextChar(out ch);
          if ((Result == 0) && (ch == '='))
            RetVal = tToken.LESSER_EQUAL;
          else
          {
            putBack();
            RetVal = tToken.LESSER;
          }
          break;

        case '>':
          Result = getNextChar(out ch);
          if ((Result == 0) && (ch == '='))
            RetVal = tToken.GREATER_EQUAL;
          else
          {
            putBack();
            RetVal = tToken.GREATER;
          }
          break;

        case '!':
          Result = getNextChar(out ch);
          if ((Result == 0) && (ch == '='))
            RetVal = tToken.UNEQUAL;
          else
          {
            putBack();
            RetVal = tToken.NOT;
          }
          break;

        case '=':
          Result = getNextChar(out ch);
          if ((Result == 0) && (ch == '='))
            RetVal = tToken.EQUAL;
          else
          {
            putBack();
            RetVal = tToken.ASSIGN;
          }
          break;

        case '&':
          Result = getNextChar(out ch);
          if ((Result == 0) && (ch == '&'))
            RetVal = tToken.AND;
          else
          {
            putBack();
            RetVal = tToken.BAD_TOKEN;
          }
          break;

        case '|':
          Result = getNextChar(out ch);
          if ((Result == 0) && (ch == '|'))
            RetVal = tToken.OR;
          else
          {
            putBack();
            RetVal = tToken.BAD_TOKEN;
          }
          break;

        case ':':
          RetVal = tToken.LABEL;
          break;

        case '0':
        case '1':
        case '2':
        case '3':
        case '4':
        case '5':
        case '6':
        case '7':
        case '8':
        case '9':
        case '.':
          {
            AnyType.tType Type = AnyType.tType.RT_UINT64;
            double DoubleVal;
            UInt64 Uint64Val;
            _numString = "";

            if (ch == '0')
            {
              _numString = "0";
              Result = getNextChar(out ch);
              if (Result == 0)
              {
                // ist es eine HEX-Zahl?
                if ((ch == 'x') || (ch == 'X'))
                {
                  _numString += 'x';
                  Type = AnyType.tType.RT_HEXVAL;
                  Result = getNextChar(out ch);
                  if (Result == 0)
                  {
                    while (Utils.ishex(ch))
                    {
                      _numString += ch;
                      Result = getNextChar(out ch);
                      if (Result != 0)
                        break;
                    }
                  }
                }
                else
                {
                  Type = AnyType.tType.RT_UINT64;
                }
              }
            }
            if (Type != AnyType.tType.RT_HEXVAL)
            {
              Type = AnyType.tType.RT_UINT64;
              while (isdigit(ch) || (ch == '.'))
              {
                _numString += ch;
                if (ch == '.')
                  Type = AnyType.tType.RT_FLOAT;
                Result = getNextChar(out ch);
                if (Result != 0)
                  break;
              }
            }
            if ((ch == 'e') || (ch == 'E'))
            {
              _numString += ch;
              Type = AnyType.tType.RT_FLOAT;
              Result = getNextChar(out ch);
              if (Result != 0)
                break;
              if (ch == '-')
              {
                _numString += ch;
                Result = getNextChar(out ch);
                if (Result != 0)
                  break;
              }
              while (isdigit(ch))
              {
                _numString += ch;
                Result = getNextChar(out ch);
                if (Result != 0)
                  break;
              }
            }
            if (ch == 'i')
              Type = AnyType.tType.RT_COMPLEX;
            else
            {
              if (Result == 0)
                putBack();
            }

            if (_numString.Length != 0)
            {
              switch (Type)
              {
                case AnyType.tType.RT_COMPLEX:

                  double.TryParse(_numString, NumberStyles.Any, CultureInfo.InvariantCulture, out DoubleVal);
                  _numValue.setType(AnyType.tType.RT_COMPLEX);
                  _numValue.setComplex(0, DoubleVal);
                  break;

                case AnyType.tType.RT_FLOAT:
                  double.TryParse(_numString, NumberStyles.Any, CultureInfo.InvariantCulture, out DoubleVal);
                  _numValue.assign(DoubleVal);
                  break;
                case AnyType.tType.RT_UINT64:
                  UInt64.TryParse(_numString, out Uint64Val);
                  _numValue.assign(Uint64Val);
                  break;
                case AnyType.tType.RT_HEXVAL:
                  UInt64.TryParse(_numString.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out Uint64Val);
                  _numValue.assign(Uint64Val);
                  _numValue.changeType(AnyType.tType.RT_HEXVAL);
                  break;

                //                case RT_INT32:
                //                case RT_UINT32:
                case AnyType.tType.RT_INT64:
                case AnyType.tType.RT_BOOL:
                case AnyType.tType.RT_STR:
                case AnyType.tType.RT_COMMENT:
                case AnyType.tType.RT_FORMULA:
                  break;
              }
              RetVal = tToken.NUMBER;
            }
            else
              RetVal = tToken.BAD_TOKEN;
          }
          break;

        case '"':
          {
            _nameString = "";
            for (; ; )
            {
              Result = getNextChar(out ch);

              if (Result != 0)
              {
                RetVal = tToken.BAD_TOKEN;
                break;
              }
              if (ch == '\\')
              {
                Result = getNextChar(out ch);
                if (Result != 0)
                {
                  RetVal = tToken.BAD_TOKEN;
                  break;
                }
                _nameString += ch;
                continue;
              }
              if (ch == '"')
                break;
              _nameString += ch;
            }
            RetVal = tToken.STRING;
          }
          break;

        default:
          if (isalpha(ch))
          {
            _nameString = "";
            _nameString += ch;
            for (; ; )
            {
              Result = getNextChar(out ch);
              if (Result != 0)
                break;
              if (!isalnum(ch))
              {
                putBack();
                break;
              }
              _nameString += ch;
            }
            RetVal = tToken.NAME;
          }
          else
            RetVal = tToken.BAD_TOKEN;
          break;
      }
      _currTok = RetVal;
      if (RetVal == tToken.BAD_TOKEN)
        reportError(tParseError.BAD_TOKEN);
      return RetVal;

    }


    // holt das naechste Zeichen aus dem Puffer
    tParseError getNextChar(out char ch)
    {
      if (_pos < _len)
      {
        if (_str != null)
          ch = _str[_pos];
        /*  else if ((m_CharStr != null) && (m_Pos < m_Len))
            ch = m_CharStr[m_Pos];*/
        else
        {
          ch = '\0';
          return tParseError.PARSESTRING;
        }
        _pos++;
        return tParseError.SUCCESS;
      }
      else
      {
        ch = '\0';
        return tParseError.PARSESTRING_END;
      }
    }



    // Schreibt Zeichen zurueck
    void putBack()
    {
      if (_pos > 0)
        _pos--;
    }

    // Fehler melden
    Int32 reportError(tParseError Err)
    {
      _errors++;
      _lastError = Err;
      Error.report(Err);
      return 1;

    }
    // zu parsender String (als String)
    string _str = null;
    // zu parsender String als char Buffer
    string m_CharStr = null;
    int _len = 0;             ///< Laenge des Strings, der zu parsen ist
    int _pos = 0;           ///< Aktuelle Position im String
    AnyType _numValue = new AnyType();     ///< num. Wert
    tToken _currTok = new tToken();         ///< aktuell zu bearbeitendes Token
    string _nameString = "";  ///< Variablenname
    string _numString = "";     ///< num. String
    // zaehlt die Fehler eines Parsevorganges
    UInt32 _errors;
    // letzter aufgetretener Fehler
    tParseError _lastError;
    // Zeiger auf die Variablenliste, die der Parser verwenden soll
    VarList _varList;
    // Zeiger auf Liste externer Funktionen
    Methods _methods;
  }
}
