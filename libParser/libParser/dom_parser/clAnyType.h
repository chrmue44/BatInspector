/*********************************************************
 *         project: ProcessMon                           *
 *                                                       *
 *              (C) 2010 Christian Mueller               *
 *                       Odenwaldstrasse 134g            *
 *                       D-64372 Ober-Ramstadt           *
 *                                                       *
 *********************************************************/

#if !defined(AFX_CLANYTYPE_H__2D189CBE_D276_4F13_9037_86F3A06ED330__INCLUDED_)
#define AFX_CLANYTYPE_H__2D189CBE_D276_4F13_9037_86F3A06ED330__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

#include "os/typedef.h"
#include "os/objhndlglib.h"

class clMethods;
class clVarList;

#define DOUBLE_EPS 1E-30

// Datentypen, die von clAnyType unterstuetzt werden
#define TYPEFLAG_BOOL    0x10
#define TYPEFLAG_INT     0x20
#define TYPEFLAG_FLOAT   0x40
#define TYPEFLAG_CPLX    0x80
#define TYPEFLAG_FORM   0x100
#define TYPEFLAG_STRING 0x800

enum tType
{
  RT_BOOL    = TYPEFLAG_BOOL   | 0x01,
//  RT_UINT32  = TYPEFLAG_INT    | 0x01,
//  RT_INT32   = TYPEFLAG_INT    | 0x02,
  RT_UINT64  = TYPEFLAG_INT    | 0x03,
  RT_INT64   = TYPEFLAG_INT    | 0x04,
  RT_HEXVAL  = TYPEFLAG_INT    | 0x05,
  RT_FLOAT   = TYPEFLAG_FLOAT  | 0x01,
  RT_COMPLEX = TYPEFLAG_CPLX   | 0x01,
  RT_COMMENT = TYPEFLAG_STRING | 0x01,
  RT_STR     = TYPEFLAG_STRING | 0x02,
  RT_FORMULA = TYPEFLAG_FORM   | 0x01,
};

struct stComplex {
  double re;
  double im;
};

typedef union
{
  stComplex m_complex; ///< Wert als komplexe Zahl
  double m_Double;     ///< Wert als Float
  int64_t m_Int64;     ///< Wert als int64
  uint64_t m_Uint64;   ///< Wert als uint64
  bool m_Bool;         ///< Wert als Bool
} tValue;


// Klasse zur Darstellung von Zahlen in verschiedenen Typen und zur 
// Konvertierung von einem Typ in den anderen. 
// Ermoeglicht dem Parser das Parsen von Ausdruecken unabhaengig vom
// Typ der beteiligten Variablen
class clAnyType  
{
public:
  clAnyType();
  clAnyType(clVarList* pVarList, clMethods* pMethods);
  virtual ~clAnyType();
  // Zuweisungsoperatoren
  // ACHTUNG: dieser Zuweisungsoperator funktioniert nur mit o-terminierten
  // Strings, ansonsten setString() verwenden!
  void operator=(const char* pValStr);
  void operator=(int64_t Val);
  void operator=(uint64_t Val);
  void operator=(double Val);
  void operator=(bool Val);
  
  // Vergleichsoperatoren
  bool operator<(clAnyType a);
  bool operator<=(clAnyType a);
  bool operator>(clAnyType a);
  bool operator>=(clAnyType a);
  bool operator==(clAnyType a);
  bool operator!=(clAnyType a);

  // Mathematik
  void operator+=(clAnyType s);
  void operator+=(int64_t s);
  void operator-=(clAnyType s);
  void operator*=(double m);
  void operator*=(clAnyType m);
  void operator/=(clAnyType d);

  // logische Verknuepfungen
  void operator&=(clAnyType a);
  void operator|=(clAnyType a);
  clAnyType operator!();

  // Typ setzen
  void setType(tType Type);
  // in anderen Typ konvertieren und den Wert beibehalten
  void changeType(tType Type, uint32_t decimals = 4);
  // String in Variable kopieren (Funktion wie operator=() mit dem Unterschied,
  // dass die Laenge explizit angegeben werden kann. Hilfreich bei Strings, die
  // nicht o-terminiert sind

  // typen adaptieren
  void adaptType(clAnyType& s);
  void assignFormulaResult();

  void doubleToString(std::string& string, double val, uint32_t decimals);
  void setString(const char *pData, uint32_t Len);
  void setComplex(double re, double im);
  void setComplexPolar(double abs, double arg);
  double getFloat();
  double getComplexIm() { return m_Val.m_complex.im;}
  double getComplexRe() { return m_Val.m_complex.re;}
  double getComplexAbs();
  double getComplexArg();
  int64_t getInt64();
  uint64_t getUint64();
  tType getType();
  const char* getTypeString();
  bool getBool();
  const char* getString();
  clAnyType getFormulaResult();
  uint32_t getStrSize();
  
  static tType stringToType(const char* typeString);

private:
  // Datentyp
  tType m_Type;
  tValue m_Val;
  // Wert als String
  tPM_string m_String;

  clVarList* m_pVarList;
  clMethods* m_pMethods;
};

#endif // !defined(AFX_CLANYTYPE_H__2D189CBE_D276_4F13_9037_86F3A06ED330__INCLUDED_)
