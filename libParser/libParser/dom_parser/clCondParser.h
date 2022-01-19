/*********************************************************
 *         project: ProcessMon                           *
 *                                                       *
 *              (C) 2010 Christian Mueller               *
 *                       Odenwaldstrasse 134g            *
 *                       D-64372 Ober-Ramstadt           *
 *                                                       *
 *********************************************************/

#if !defined(AFX_CLCONDPARSER_H__988C437B_8797_4531_B910_5E2A01E094C5__INCLUDED_)
#define AFX_CLCONDPARSER_H__988C437B_8797_4531_B910_5E2A01E094C5__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

#include "os/typedef.h"
#include "os/objhndlglib.h"
#include "clAnyType.h"
#include "clError.h"
//#include "clMthdList.h"


class clVarList;
class clMethods;
class clMthdResult;

class clCondParser  
{
public:
enum tToken
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

	explicit clCondParser(clVarList* pVarList, clMethods* pMethods);
	virtual ~clCondParser();
  // parst den Inhalt des Strings
  clAnyType parse(tPM_string* Str);
	// parst den Inhalt eines Ergebnis-Strings
  int32_t parseResultStr(tPM_string* Str, clMthdResult& Result);
	// parst den Inhalt eines Ergebnis-Strings
  int32_t parseResultStr(char* Str, uint32_t BufLen, clMthdResult& Result);
  // gibt die Anzahl der Fehler beim Parsen aus
  uint32_t getParseErrors();
  // letzten Fehler fragen
  int32_t getLastError();
  // prueft ob die Zeile ein Label enthaelt
  bool checkLabel(tPM_string* Line, tPM_string& Label);

private:
  int32_t parseResultStr(clMthdResult& Result);
	// Verarbeitung von Ausdruecken
  clAnyType expr();
  // verarbeitung von Termen
  clAnyType term();
  // Verarbeitung von Primaries
  clAnyType prim();
  // Verarbeitung eines Funktionsasudrucks
  clAnyType function(const char* Name);
  // holt das naechste Token
  tToken getToken();
  // holt das naechste Zeichen aus dem Puffer
  int32_t getNextChar(char* ch);
  // prueft, ob der uebergebene Character ein erlaubter HEX-Character ist
  bool ishex(tPM_Char Ch);
  // Schreibt Zeichen zurueck
  void putBack();
  // Fehler melden
  int32_t reportError(int32_t Err);
  // zu parsender String (als String)
  tPM_string* m_Str;
  // zu parsender String als char Buffer
  char* m_CharStr;
  size_t m_Len;             ///< Laenge des Strings, der zu parsen ist
  uint32_t m_Pos;           ///< Aktuelle Position im String
  clAnyType m_NumValue;     ///< num. Wert
  tToken m_CurrTok;         ///< aktuell zu bearbeitendes Token
  tPM_string m_NameString;  ///< Variablenname
  char m_NumString[80];     ///< num. String
  // zaehlt die Fehler eines Parsevorganges
  uint32_t m_Errors;
  // letzter aufgetretener Fehler
  int32_t m_LastError;
  // Zeiger auf die Variablenliste, die der Parser verwenden soll
  clVarList* m_pVarList;
  // Zeiger auf Liste externer Funktionen
  clMethods* m_pMthds;
};

#endif // !defined(AFX_CLCONDPARSER_H__988C437B_8797_4531_B910_5E2A01E094C5__INCLUDED_)
