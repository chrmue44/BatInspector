/*********************************************************
 *         project: ProcessMon                           *
 *                                                       *
 *              (C) 2010 Christian Mueller               *
 *                       Odenwaldstrasse 134g            *
 *                       D-64372 Ober-Ramstadt           *
 *                                                       *
 *********************************************************/

#include "clCondParser.h"
#include <string>
#include <iostream>
#include <stdio.h>


#include "clError.h"
#include "clVarList.h"
#include "clMethods.h"
#include "clMthdResult.h"

//////////////////////////////////////////////////////////////////////
// Construction/Destruction
//////////////////////////////////////////////////////////////////////

clCondParser::clCondParser(clVarList* pVarList, clMethods* pMethods)
:
m_pVarList(pVarList),
m_pMthds(pMethods)
{
}

clCondParser::~clCondParser()
{
}

bool clCondParser::ishex(tPM_Char Ch)
{
  if( 
      ((Ch >= '0') && (Ch <= '9')) ||
      ((Ch >= 'A') && (Ch <= 'F')) ||
      ((Ch >= 'a') && (Ch <= 'f'))
     )
    return true;
  else
    return false;
}

clCondParser::tToken clCondParser::getToken()
{
  tToken RetVal;
  int32_t Result;
  char ch;
  do
  {
  	if(getNextChar(&ch) != 0)
		{
			m_CurrTok = END;
      return END;
		}
		if(ch <= 0)
		{
			m_CurrTok = END;
      return END;
		}
  } while(isspace(ch));
  switch(ch)
  {
    case ',':
         RetVal = KOMMA;
         break;

    case '+':
         RetVal = PLUS;
         break;
         
    case '*':
         RetVal = MUL;
         break;

    case '/':
         Result = getNextChar(&ch);
         if((Result == 0) && (ch == '/'))
         {
           while(Result == 0)
           {
             Result = getNextChar(&ch);
             if(Result == 0)
               m_NameString += ch;
           }
           RetVal = COMMENT;
         }
         else
         {
           putBack();
           RetVal = DIV;
         }
         break;

    case '-':
         RetVal = MINUS;
         break;

    case '(':
         RetVal = BRACE_OPEN;
         break;

    case ')':
         RetVal = BRACE_CLOSE;
         break;

    case '[':
         RetVal = SQR_BRACK_OPEN;
         break;

    case ']':
         RetVal = SQR_BRACK_CLOSE;
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
         Result = getNextChar(&ch);
         if((Result == 0) && (ch == '='))
           RetVal = LESSER_EQUAL;
         else
         {
           putBack();
           RetVal = LESSER;
         }
         break;

    case '>':
         Result = getNextChar(&ch);
         if((Result == 0) && (ch == '='))
           RetVal = GREATER_EQUAL;
         else
         {
           putBack();
           RetVal = GREATER;
         }
         break;

    case '!':
         Result = getNextChar(&ch);
         if((Result == 0) && (ch == '='))
           RetVal = UNEQUAL;
         else
         {
           putBack();
           RetVal = NOT;
         }
         break;

    case '=':
         Result = getNextChar(&ch);
         if((Result == 0) && (ch == '='))
           RetVal = EQUAL;
         else
         {
           putBack();
           RetVal = ASSIGN;
         }
         break;

    case '&':
         Result = getNextChar(&ch);
         if((Result == 0) && (ch == '&'))
           RetVal = AND;
         else
         {
           putBack();
           RetVal = BAD_TOKEN;
         }
         break;

    case '|':
         Result = getNextChar(&ch);
         if((Result == 0) && (ch == '|'))
           RetVal = OR;
         else
         {
           putBack();
           RetVal = BAD_TOKEN;
         }
         break;

    case ':':
         RetVal = LABEL;
         break;

    case '0': case '1': case '2': case '3': case '4':
    case '5': case '6': case '7': case '8': case '9':
    case '.':
         {
           uint32_t NumPos = 0;
           tType Type = RT_UINT64;
           double DoubleVal;
           uint64_t Uint64Val;

           if(ch == '0')
           {
             NumPos++;
             m_NumString[0] = '0';
             Result = getNextChar(&ch);
             if(Result == 0)
             {
               // ist es eine HEX-Zahl?
               if((ch == 'x') || (ch == 'X')) 
               {
                 m_NumString[1] = 'x';
                 NumPos = 2;
                 Type = RT_HEXVAL;
                 Result = getNextChar(&ch);
                 if(Result == 0)
                 {
                   while (ishex(ch))
                   {
                     m_NumString[NumPos] = ch;
                     NumPos++;
                     Result = getNextChar(&ch);
                     if(Result != 0)
                       break;
                   }
                 }
               }
               else
               {
                 Type = RT_UINT64;
               }
             }
           }
           if(Type != RT_HEXVAL)
           {
             Type = RT_UINT64;
             while (isdigit(ch) || (ch == '.'))
             {
               m_NumString[NumPos] = ch;
               if(ch == '.')
                 Type = RT_FLOAT;
               NumPos++;
               Result = getNextChar(&ch);
               if(Result != 0)
                 break;
             }
           }
           if(ch == 'e') {
             m_NumString[NumPos++] = ch;
             Type = RT_FLOAT;
             Result = getNextChar(&ch);
             if(Result != 0)
               break;
             if(ch == '-') {
               m_NumString[NumPos++] = ch;
               Result = getNextChar(&ch);
               if(Result != 0)
                 break;
             }
             while (isdigit(ch))
             {
               m_NumString[NumPos] = ch;
               NumPos++;
               Result = getNextChar(&ch);
               if(Result != 0)
                 break;
             }
           }
           if(ch == 'i')
             Type = RT_COMPLEX;
           else {
             if(Result == 0)
               putBack();
           }
           m_NumString[NumPos] = 0;

           if(NumPos != 0)
           {
             switch(Type)
             {
               case RT_COMPLEX:
                    sscanf(m_NumString,DOUBLE_TOSTR_FMT,&DoubleVal);
                    m_NumValue.setType(RT_COMPLEX);
                    m_NumValue.setComplex(0, DoubleVal);
                    break;

               case RT_FLOAT:
                    sscanf(m_NumString,DOUBLE_TOSTR_FMT,&DoubleVal);
                    m_NumValue = DoubleVal;
                    break;
               case RT_UINT64:
                    sscanf(m_NumString,UINT64_TOSTR_FMT,&Uint64Val);
                    m_NumValue = Uint64Val;
                    break;
               case RT_HEXVAL:
                    sscanf(m_NumString,UINT64HEX_TOSTR_FMT,&Uint64Val);
                    m_NumValue = Uint64Val;
                    m_NumValue.changeType(RT_HEXVAL);
                    break;
//                case RT_INT32:
//                case RT_UINT32:
                case RT_INT64:
                case RT_BOOL:
                case RT_STR:
                case RT_COMMENT:
                case RT_FORMULA:
                    break;
             }
             RetVal = NUMBER;
           }
           else
             RetVal = BAD_TOKEN;
         }
         break;
    
    case '"':
        {
          m_NameString ="";
          for(;;)
          {
            int32_t Result;
            Result = getNextChar(&ch);
            
            if (Result != 0)
            {
              RetVal = BAD_TOKEN;
              break;
            }
            if(ch == '\\')
            {
              Result = getNextChar(&ch);
              if (Result != 0)
              {
                RetVal = BAD_TOKEN;
                break;
              }
              m_NameString += ch;
              continue;
            }
            if (ch =='"')
              break;
            m_NameString += ch;
          }
          RetVal = STRING;
        }
        break;

    default:
        if(isalpha(ch))
        {
          m_NameString = ch;
          for(;;)
          {
            int32_t Result;
            Result = getNextChar(&ch);
            if (Result != 0)
              break;
            if (!isalnum(ch))
            {
              putBack();
              break;
            }
            m_NameString += ch;
          }
          RetVal = NAME;
        }
        else
          RetVal = BAD_TOKEN;
  }
  m_CurrTok = RetVal;
  return RetVal;
}


int32_t clCondParser::getNextChar(char* ch)
{
  if (m_Pos < m_Len)
  {
		if(m_Str != NULL)
      *ch = (m_Str->c_str())[m_Pos];
		else if((m_CharStr != NULL) && (m_Pos < m_Len))
      *ch = m_CharStr[m_Pos];
		else
			return -1;
    m_Pos++;
    return ERR_SUCCESS;
  }
  else
    return -1;
}

void clCondParser::putBack()
{
  if (m_Pos > 0)
    m_Pos--;
}


clAnyType clCondParser::parse(tPM_string* Str)
{
  clAnyType RetVal;

  clError::init();
  m_Len = Str->size();
  m_Str = Str;
  m_Pos = 0;
  m_Errors = 0;
  getToken();
  RetVal = expr();
	if (clError::get() != 0)
	{
		m_Errors++;
	  m_LastError = clError::get();
	}
  return RetVal;
}

int32_t clCondParser::parseResultStr(char* Str, uint32_t BufLen, clMthdResult& Result)
{
  m_Len = BufLen;
  m_Str = NULL;
	m_CharStr = Str;
	return parseResultStr(Result);
}

int32_t clCondParser::parseResultStr(tPM_string* Str, clMthdResult& Result)
{
	m_CharStr = NULL;
  m_Len = Str->size();
  m_Str = Str;
	return parseResultStr(Result);
}

int32_t clCondParser::parseResultStr(clMthdResult& Result)
{
	bool Finished = false;
  m_Pos = 0;
  m_Errors = 0;
	m_LastError = 0;
	bool ErrorCode = true;
//	Result.flush();
	while(!Finished)
	{
    getToken();
		switch(m_CurrTok)
		{
		  case NUMBER:
				if(ErrorCode)
				{
		    m_NumValue.changeType(RT_INT64);
					reportError(m_NumValue.getInt64());
				}
				Result.addValue(m_NumValue);
				break;
			case STRING:
				Result.addValue(m_NameString.c_str());
				break;
			case END:
				Finished = true;
				break;
			default:
				reportError(ERR_RESULTSTRING);
				break;
		}
		ErrorCode = false;
		if(!Finished)
		{
		  getToken();
		  if(m_CurrTok != KOMMA)
			{
				Finished = true;
				if(m_CurrTok != END)
				  reportError(ERR_RESULTSTRING);
			}
		}
	}
  return m_LastError;
}

bool clCondParser::checkLabel(tPM_string* Str, tPM_string& Label)
{
  bool RetVal = false;
  m_Len = Str->size();
  m_Str = Str;
  m_Pos = 0;
  m_Errors = 0;
  getToken();
  if(m_CurrTok == NAME)
  {
    Label = m_NameString;
    getToken();
    if(m_CurrTok == LABEL)
      RetVal = true;
  }
  return RetVal;
}

clAnyType clCondParser::expr()
{
  clAnyType left = term();
  for(;;)
  {
    switch(m_CurrTok)
    {
      case PLUS:
           getToken();
           left += term();
           break;

      case MINUS:
           getToken();
           left -= term();
           break;

      case OR:
           getToken();
           left |= (term());
           break;

      default:
           return left;
    }
  }
}


clAnyType clCondParser::term()
{
  clAnyType left = prim();
  for(;;)
  {
    switch(m_CurrTok)
    {
      case MUL:
           getToken();
           left *= prim();
           break;

      case DIV:
           getToken();
           left /= prim();
           break;

      case AND:
           getToken();
           left &= prim();
           break;

      case LESSER:
           getToken();
           left = (left < prim());
           break;

      case LESSER_EQUAL:
           getToken();
           left = (left <= prim());
           break;

      case GREATER:
           getToken();
           left = (left > prim());
           break;

      case GREATER_EQUAL:
           getToken();
           left = (left >= prim());
           break;

      case EQUAL:
           getToken();
           left = (left == prim());
           break;

      case UNEQUAL:
           getToken();
           left = (left != prim());
           break;
           
      default:
           return left;
    }
  }
}


clAnyType clCondParser::prim()
{
  clAnyType RetVal(m_pVarList, m_pMthds);

  switch(m_CurrTok)
  {
    case NUMBER:
         getToken();
         RetVal = m_NumValue;
         return RetVal;

    case STRING:
         getToken();
         RetVal = m_NameString.c_str();
         return RetVal;

    case NAME:
         getToken();
         switch(m_CurrTok)
         {
           case ASSIGN:
               {
                 int32_t Err;
                 clName* n = m_pVarList->insert(m_NameString.c_str(), false, m_pMthds);
                 getToken();
                 if(!n->isConst())
                   n->setValue(0, expr());
                 else
                   clError::report(ERR_ASSIGN_CONST);
                 Err = n->getValue(0,RetVal);
                 PM_ASSERT(Err == 0, "clCondParser:Error reading variable");
                 break;
               }

           case BRACE_OPEN:
                RetVal = function(m_NameString.c_str());
                if(m_CurrTok != BRACE_CLOSE)
                {
                  reportError(ERR_BRACECLOSE);
                  RetVal = (float)1.0;
                }
                getToken();
                break;
         
           case LABEL:
               {
                 tPM_string Fname = "addLabel";
                 clAnyType LabName;
                 LabName = m_NameString.c_str();
                 clAnyType* pLabName = &LabName;
                 clAnyType RetVal;
                 int32_t err = m_pMthds->executeFunction(Fname, 1, &pLabName, RetVal);
                 if(err != 0)
                   clError::report(err);
               }
               break;

           case SQR_BRACK_OPEN:
               {
                 tPM_string Name = m_NameString;
                 getToken();
                 RetVal = expr();
                 RetVal.changeType(RT_INT64);
                 // Index fuer Array
                 uint32_t Index = RetVal.getInt64();
                 if(m_CurrTok != SQR_BRACK_CLOSE)
                 {
                   reportError(ERR_SQR_BRACK_CLOSE);
                   break;
                 }

                 getToken();
                 if(m_CurrTok == ASSIGN)
                 {
                   clName* n = m_pVarList->insert(Name.c_str(), m_pMthds);
                   int32_t Err;
                   getToken();
                   RetVal = expr();
                   Err = n->setValue(Index,RetVal);
                   if(Err != 0)
                     reportError(ERR_ARRAY_INDEX);
                   getToken();
                 }
                 else
                 {
                   clName* n = m_pVarList->look(Name.c_str());
                   if(n != NULL)
                   {
                     int32_t Err;
                     Err = n->getValue(Index,RetVal);
                     if(Err != 0)
                       reportError(ERR_ARRAY_INDEX);
                   }
                   else
                   {
                     RetVal = static_cast<float>(1.0);
                     reportError(ERR_VARIABLE);
                   }
                 }
               }
               break;

           default:
               {
                 clName* n = m_pVarList->look(m_NameString.c_str());
                 if(n != NULL)
                 {
                   int32_t Err;
                   Err = n->getValue(0,RetVal);
                   PM_ASSERT(Err == 0, "clCondParser:Error reading variable");
                 }
                 else
                 {
                   RetVal = static_cast<float>(1.0);
                   reportError(ERR_VARIABLE);
                 }
               }
         }
         break;

    case NOT:
         getToken();
         RetVal = prim();
         RetVal = !RetVal;
         break;

    case MINUS:
         getToken();
         RetVal = prim();
         RetVal *= -1.0;
         break;

    case BRACE_OPEN:
         getToken();
         RetVal = expr();
         if(m_CurrTok != BRACE_CLOSE)
         {
           RetVal = static_cast<float>(1.0);
           reportError(ERR_BRACECLOSE);
         }
         getToken();
         break;
    

    case END:
    case BRACE_CLOSE:  //fuer funktionen mit keinen Parametern!, koennte evtl. an anderer Stelle Aerger machen(?)
         RetVal = static_cast<float>(1.0);
         break;

    case COMMENT:
         RetVal = m_NameString.c_str();
         RetVal.changeType(RT_COMMENT);
         break;         
/*
    case CRL_BRACE_OPEN: {
         RetVal.changeType(RT_COMPLEX);
         getToken();
         clAnyType re = expr();
         if(m_CurrTok != KOMMA)
           clError::report(ERR_COMPLEX_COMMA);
         getToken();
         clAnyType im = expr();
         if(m_CurrTok != CRL_BRACE_CLOSE)
           clError::report(ERR_CRL_BRACE_CLOSE);
         getToken();
         re.changeType(RT_FLOAT);
         RetVal.setComplex(re.getFloat(),im.getComplexIm());
         }
         break;
*/
    default:
         RetVal = static_cast<float>(1.0);
         reportError(ERR_PRIMARY);
         break;

  }
  return RetVal;
}


struct tParListItem
{
  clAnyType Par;
  tParListItem* Next;
};

clAnyType clCondParser::function(const char* Name)
{
  tPM_string FuncName = Name;
  clAnyType RetVal;
  bool ErrorFlag = false;
  uint32_t ParCnt = 0;
  tParListItem* pPar;
  tParListItem* pLast = NULL;
  // Zeiger auf den Anfang der Parametertabelle
  tParListItem* pFirst = NULL;

  getToken();
  for(;;)
  {
    // keine Parameter
    if(m_CurrTok == BRACE_CLOSE)
      break;
    // neuen Parameter erzeugen
    pPar = CREATE_OBJECT_DEFCON(tParListItem,"tParListItem");
    pPar->Par = expr();
    pPar->Next = NULL;
    ParCnt++;

    if(pFirst == NULL)
      pFirst = pPar;
    if(pLast != NULL)
      pLast->Next = pPar;

    pLast = pPar;

    if((m_CurrTok != KOMMA) && (m_CurrTok != BRACE_CLOSE))
    {
      reportError(ERR_KOMMA);
      ErrorFlag = true;
      RetVal = (float)1.0;
      break;
    }
    if(m_CurrTok == BRACE_CLOSE)
      break;    
    getToken();
  }

  // Parametertabelle erzeugen
  if(!ErrorFlag)
  {
    tParListItem* Iter = pFirst;
    uint32_t i = 0;
    clAnyType** argv = NULL;
    if(ParCnt > 0)
    {
      argv = CREATE_OBJECT_ARR(clAnyType*,ParCnt,"clCondParser::function");

      while(Iter != NULL)
      {
        argv[i] = &Iter->Par;
        i++;
        Iter = Iter->Next;
      }
    }

    int32_t err = m_pMthds->executeFunction(FuncName, ParCnt, argv, RetVal);
    if(err != 0)
      clError::report(err);
    if(ParCnt > 0)
    {
      DELETE_OBJECT_ARR(argv,"clCondParser::function");
    }
    else
      pFirst = NULL;
  }

  // Parameter aufraeumen
  while(pFirst != NULL)
  {
    pPar = pFirst->Next;
    DELETE_TYPED_OBJECT(tParListItem,pFirst,"tParListItem");
    pFirst = pPar;
  }

  return RetVal;
}


int32_t clCondParser::reportError(int32_t Err)
{
  m_Errors++;
  m_LastError = Err;
  clError::report(Err);
  return 1;
}


uint32_t clCondParser::getParseErrors()
{
  return m_Errors;
}

int32_t clCondParser::getLastError()
{
  return m_LastError;
}
