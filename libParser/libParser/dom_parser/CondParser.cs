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


    public CondParser(VarList pVarList, Methods pMethods)
    {
      m_pVarList = pVarList;
      m_pMthds = pMethods;
    }


    // parst den Inhalt des Strings
    public AnyType parse(string Str)
    {
      Error.init();
      m_Len = Str.Length;
      m_Str = Str;
      m_Pos = 0;
      m_Errors = 0;
      getToken();
      AnyType retVal = expr();

      if (Error.get() != 0)
      {
        m_Errors++;
        m_LastError = Error.get();
        retVal.assign("ERROR " + m_LastError.ToString());
      }
      return retVal;
    }


    // parst den Inhalt eines Ergebnis-Strings
    public tParseError parseResultStr(string Str, out MthdResult Result)
    {
      Result = new MthdResult();
      m_CharStr = null;
      m_Len = Str.Length;
      m_Str = Str;
      return parseResultStr(ref Result);
    }


    // parst den Inhalt eines Ergebnis-Strings
    public tParseError parseResultStr(string Str, int BufLen, out MthdResult Result)
    {
      Result = new MthdResult();
      m_Len = BufLen;
      m_Str = null;
      m_CharStr = Str;
      return parseResultStr(ref Result);
    }


    // gibt die Anzahl der Fehler beim Parsen aus
    public UInt32 getParseErrors()
    {
      return m_Errors;
    }


    // letzten Fehler fragen
    public tParseError getLastError()
    {
      return m_LastError;
    }


    // prueft ob die Zeile ein Label enthaelt
    bool checkLabel(string Line, out string Label)
    {
      bool RetVal = false;
      Label = "";
      m_Len = Line.Length;
      m_Str = Line;
      m_Pos = 0;
      m_Errors = 0;
      getToken();
      if (m_CurrTok == tToken.NAME)
      {
        Label = m_NameString;
        getToken();
        if (m_CurrTok == tToken.LABEL)
          RetVal = true;
      }
      return RetVal;
    }
 

  tParseError parseResultStr(ref MthdResult Result)
    {
      bool Finished = false;
      m_Pos = 0;
      m_Errors = 0;
      m_LastError = 0;
      bool ErrorCode = true;
      //	Result.flush();
      while (!Finished)
      {
        getToken();
        switch (m_CurrTok)
        {
          case tToken.NUMBER:
            if (ErrorCode)
            {
              m_NumValue.changeType(AnyType.tType.RT_INT64);
              reportError((tParseError)m_NumValue.getInt64());
            }
            Result.addValue(m_NumValue);
            break;
          case tToken.STRING:
            Result.addValue(m_NameString);
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
          if (m_CurrTok != tToken.KOMMA)
          {
            Finished = true;
            if (m_CurrTok != tToken.END)
              reportError(tParseError.RESULTSTRING);
          }
        }
      }
      return m_LastError;
    }


    // Verarbeitung von Ausdruecken
    AnyType expr()
    {
      AnyType left = new AnyType( term());
      for (; ; )
      {
        switch (m_CurrTok)
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
        switch (m_CurrTok)
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
      AnyType RetVal = new AnyType(m_pVarList, m_pMthds);

      switch (m_CurrTok)
      {
        case tToken.NUMBER:
          getToken();
          RetVal.assign(m_NumValue);
          return RetVal;

        case tToken.STRING:
          getToken();
          RetVal.assign(m_NameString);
          return RetVal;

        case tToken.NAME:
          getToken();
          switch (m_CurrTok)
          {
            case tToken.ASSIGN:
              {
                Int32 Err;
                VarName n = m_pVarList.insert(m_NameString, false, m_pMthds);
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
              RetVal = function(m_NameString);
              if (m_CurrTok != tToken.BRACE_CLOSE)
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
                LabName.assign(m_NameString);
                List<AnyType> pLabName = new List<AnyType>();
                pLabName.Add(LabName);
                tParseError err = m_pMthds.executeFunction(Fname, 1, pLabName, ref RetVal);
                if (err != 0)
                  Error.report(err);
              }
              break;

            case tToken.SQR_BRACK_OPEN:
              {
                string Name = m_NameString;
                getToken();
                RetVal.assign(expr());
                RetVal.changeType(AnyType.tType.RT_INT64);
                // Index fuer Array
                int Index = (int)RetVal.getInt64();
                if (m_CurrTok != tToken.SQR_BRACK_CLOSE)
                {
                  reportError(tParseError.SQR_BRACK_CLOSE);
                  break;
                }

                getToken();
                if (m_CurrTok == tToken.ASSIGN)
                {
                  VarName n = m_pVarList.insert(Name, false, m_pMthds);
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
                  VarName n = m_pVarList.look(Name);
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
                VarName n = m_pVarList.look(m_NameString);
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
          if (m_CurrTok != tToken.BRACE_CLOSE)
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
          RetVal.assign(m_NameString);
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
        if (m_CurrTok == tToken.BRACE_CLOSE)
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

        if ((m_CurrTok != tToken.KOMMA) && (m_CurrTok != tToken.BRACE_CLOSE))
        {
          reportError(tParseError.KOMMA);
          ErrorFlag = true;
          RetVal.assign(1.0);
          break;
        }
        if (m_CurrTok == tToken.BRACE_CLOSE)
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

        tParseError err = m_pMthds.executeFunction(FuncName, ParCnt, argv, ref RetVal);
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
          m_CurrTok = tToken.END;
          return tToken.END;
        }
        if (ch <= 0)
        {
          m_CurrTok = tToken.END;
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
                m_NameString += ch;
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
            m_NumString = "";

            if (ch == '0')
            {
              m_NumString = "0";
              Result = getNextChar(out ch);
              if (Result == 0)
              {
                // ist es eine HEX-Zahl?
                if ((ch == 'x') || (ch == 'X'))
                {
                  m_NumString += 'x';
                  Type = AnyType.tType.RT_HEXVAL;
                  Result = getNextChar(out ch);
                  if (Result == 0)
                  {
                    while (ishex(ch))
                    {
                      m_NumString += ch;
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
                m_NumString += ch;
                if (ch == '.')
                  Type = AnyType.tType.RT_FLOAT;
                Result = getNextChar(out ch);
                if (Result != 0)
                  break;
              }
            }
            if ((ch == 'e') || (ch == 'E'))
            {
              m_NumString += ch;
              Type = AnyType.tType.RT_FLOAT;
              Result = getNextChar(out ch);
              if (Result != 0)
                break;
              if (ch == '-')
              {
                m_NumString += ch;
                Result = getNextChar(out ch);
                if (Result != 0)
                  break;
              }
              while (isdigit(ch))
              {
                m_NumString += ch;
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

            if (m_NumString.Length != 0)
            {
              switch (Type)
              {
                case AnyType.tType.RT_COMPLEX:

                  double.TryParse(m_NumString, NumberStyles.Any, CultureInfo.InvariantCulture, out DoubleVal);
                  m_NumValue.setType(AnyType.tType.RT_COMPLEX);
                  m_NumValue.setComplex(0, DoubleVal);
                  break;

                case AnyType.tType.RT_FLOAT:
                  double.TryParse(m_NumString, NumberStyles.Any, CultureInfo.InvariantCulture, out DoubleVal);
                  m_NumValue.assign(DoubleVal);
                  break;
                case AnyType.tType.RT_UINT64:
                  UInt64.TryParse(m_NumString, out Uint64Val);
                  m_NumValue.assign(Uint64Val);
                  break;
                case AnyType.tType.RT_HEXVAL:
                  UInt64.TryParse(m_NumString.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out Uint64Val);
                  m_NumValue.assign(Uint64Val);
                  m_NumValue.changeType(AnyType.tType.RT_HEXVAL);
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
            m_NameString = "";
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
                m_NameString += ch;
                continue;
              }
              if (ch == '"')
                break;
              m_NameString += ch;
            }
            RetVal = tToken.STRING;
          }
          break;

        default:
          if (isalpha(ch))
          {
            m_NameString = "";
            m_NameString += ch;
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
              m_NameString += ch;
            }
            RetVal = tToken.NAME;
          }
          else
            RetVal = tToken.BAD_TOKEN;
          break;
      }
      m_CurrTok = RetVal;
      if (RetVal == tToken.BAD_TOKEN)
        reportError(tParseError.BAD_TOKEN);
      return RetVal;

    }


    // holt das naechste Zeichen aus dem Puffer
    tParseError getNextChar(out char ch)
    {
      if (m_Pos < m_Len)
      {
        if (m_Str != null)
          ch = m_Str[m_Pos];
        /*  else if ((m_CharStr != null) && (m_Pos < m_Len))
            ch = m_CharStr[m_Pos];*/
        else
        {
          ch = '\0';
          return tParseError.PARSESTRING;
        }
        m_Pos++;
        return tParseError.SUCCESS;
      }
      else
      {
        ch = '\0';
        return tParseError.PARSESTRING_END;
      }
    }


    // prueft, ob der uebergebene Character ein erlaubter HEX-Character ist
    bool ishex(char Ch)
    {
      if (
    ((Ch >= '0') && (Ch <= '9')) ||
    ((Ch >= 'A') && (Ch <= 'F')) ||
    ((Ch >= 'a') && (Ch <= 'f'))
   )
        return true;
      else
        return false;
    }


    // Schreibt Zeichen zurueck
    void putBack()
    {
      if (m_Pos > 0)
        m_Pos--;
    }

    // Fehler melden
    Int32 reportError(tParseError Err)
    {
      m_Errors++;
      m_LastError = Err;
      Error.report(Err);
      return 1;

    }
    // zu parsender String (als String)
    string m_Str = null;
    // zu parsender String als char Buffer
    string m_CharStr = null;
    int m_Len = 0;             ///< Laenge des Strings, der zu parsen ist
    int m_Pos = 0;           ///< Aktuelle Position im String
    AnyType m_NumValue = new AnyType();     ///< num. Wert
    tToken m_CurrTok = new tToken();         ///< aktuell zu bearbeitendes Token
    string m_NameString = "";  ///< Variablenname
    string m_NumString = "";     ///< num. String
    // zaehlt die Fehler eines Parsevorganges
    UInt32 m_Errors;
    // letzter aufgetretener Fehler
    tParseError m_LastError;
    // Zeiger auf die Variablenliste, die der Parser verwenden soll
    VarList m_pVarList;
    // Zeiger auf Liste externer Funktionen
    Methods m_pMthds;
  }
}
