/*********************************************************
 *         project: ProcessMon                           *
 *                                                       *
 *              (C) 2010 Christian Mueller               *
 *                       Odenwaldstrasse 134g            *
 *                       D-64372 Ober-Ramstadt           *
 *                                                       *
 *********************************************************/

#include "clAnyType.h"
#include "clError.h"
#include "clCondParser.h"

#include <stdio.h>
#include <cstring>
#include <cmath>

//////////////////////////////////////////////////////////////////////
// Construction/Destruction
//////////////////////////////////////////////////////////////////////

clAnyType::clAnyType(clVarList* pVarList, clMethods* pMethods)
:
m_Type(RT_FLOAT),
m_String(""),
m_pVarList(pVarList),
m_pMethods(pMethods)
{
  m_Val.m_Double = 0.0;
}

clAnyType::clAnyType()
:
m_Type(RT_FLOAT),
m_String(""),
m_pVarList(NULL),
m_pMethods(NULL)
{
  m_Val.m_Double = 0.0;
}

clAnyType::~clAnyType()
{

}

void clAnyType::operator=(int64_t Val)
{
  m_Type = RT_INT64;
  m_Val.m_Int64 = Val;
}

void clAnyType::operator=(uint64_t Val)
{
  m_Type = RT_UINT64;
  m_Val.m_Uint64 = Val;
}

void clAnyType::operator=(double Val)
{
  m_Type = RT_FLOAT;
  m_Val.m_Double = Val;
}


void clAnyType::operator=(const char* pValStr)
{
  m_Type = RT_STR;
  if(pValStr != NULL)
  {
    if(*pValStr == '=')
      m_Type = RT_FORMULA;
    m_String = pValStr;
  }
  else
    m_String = "";
}

void clAnyType::setString(const char *pData, uint32_t Len)
{
  m_Type  = RT_STR;
  m_String = "";
  if(pData != NULL)
    m_String.insert(0,pData,Len);
    
}

void clAnyType::setComplex(double re, double im) {
  m_Val.m_complex.re = re;
  m_Val.m_complex.im = im;
}

void clAnyType::setComplexPolar(double abs, double arg) {
  m_Val.m_complex.re = abs * std::cos(arg);
  m_Val.m_complex.im = abs * std::sin(arg);
}


void clAnyType::operator=(bool Val)
{
  m_Type = RT_BOOL;
  m_Val.m_Bool = Val;
}

void clAnyType::adaptType(clAnyType& s) {
  if(m_Type != s.getType()) {
    if(static_cast<int32_t>(m_Type) > static_cast<int32_t>(s.getType()))
      s.changeType(m_Type);
    else
      changeType(s.getType());
  }
}

void clAnyType::assignFormulaResult() {
  clAnyType res = getFormulaResult();
  switch(res.getType()) {
    case RT_INT64:
      m_Val.m_Int64 = res.getInt64();
      break;
    case RT_HEXVAL:
    case RT_UINT64:
      m_Val.m_Uint64 = res.getUint64();
      break;
    case RT_FLOAT:
      m_Val.m_Double = res.getFloat();
      break;
    case RT_BOOL:
      m_Val.m_Bool = res.getBool();
      break;
    case RT_COMPLEX:
      m_Val.m_complex.re = res.getComplexRe();
      m_Val.m_complex.im = res.getComplexIm();
      break;
    case RT_STR:
    case RT_COMMENT:
      m_String = res.getString();
      break;
    case RT_FORMULA:
      break;
    }
  m_Type = res.getType();

}


void clAnyType::operator+=(clAnyType s)
{
  clAnyType Zero;
  Zero = (uint64_t)0;
  if((s.getType() != RT_COMPLEX) && (s < Zero))
  {
    if(m_Type == RT_UINT64)
      changeType(RT_INT64);
  }

  if(m_Type == RT_FORMULA)
    assignFormulaResult();
  if(s.getType() == RT_FORMULA)
    s.assignFormulaResult();

  adaptType(s);
  switch(m_Type)
  {
    case RT_INT64:
         m_Val.m_Int64 += s.getInt64();
         break;

    case RT_UINT64:
         m_Val.m_Int64 += s.getUint64();
         break;

    case RT_HEXVAL:
         m_Val.m_Int64 += s.getUint64();
         break;

    case RT_BOOL:
         m_Val.m_Bool |= s.getBool();
         break;

    case RT_STR:
         m_String += s.getString();
         break;

    case RT_FLOAT:
         m_Val.m_Double += s.getFloat();
         break;

    case RT_FORMULA:
    case RT_COMMENT:
         clError::report(ERR_PLUS_NOT_SUPPORTED);
         break;
    case RT_COMPLEX:
         m_Val.m_complex.re += s.getComplexRe();
         m_Val.m_complex.im += s.getComplexIm();
         break;

  }
}

void clAnyType::operator+=(int64_t s)
{
  if(s < 0)
  {
    if(m_Type == RT_UINT64)
      changeType(RT_INT64);
 //   else if(m_Type == RT_UINT32)
 //     changeType(RT_INT32);
  }
  if(m_Type == RT_FORMULA)
    assignFormulaResult();

  switch(m_Type)
  {
    case RT_INT64:
         m_Val.m_Int64 += s;
         break;
    case RT_UINT64:
         m_Val.m_Uint64 += s;
         break;
    case RT_HEXVAL:
         m_Val.m_Uint64 += s;
         break;
    case RT_FLOAT:
         m_Val.m_Double += s;
         break;
    case RT_COMPLEX:
         m_Val.m_complex.re += s;
         break;
    case RT_FORMULA:
    case RT_BOOL:
    case RT_STR:
    case RT_COMMENT:
         break;
  }
}



void clAnyType::operator-=(clAnyType s)
{
  if((m_Type == RT_UINT64) || (m_Type == RT_HEXVAL))
    changeType(RT_INT64);
  if(m_Type == RT_FORMULA)
    assignFormulaResult();
  if(s.getType() == RT_FORMULA)
    s.assignFormulaResult();

  adaptType(s);
  switch(m_Type)
  {
    case RT_INT64:
         m_Val.m_Int64 -= s.getInt64();
         break;

    case RT_UINT64:
         changeType(RT_INT64);
         s.changeType(RT_INT64);
         m_Val.m_Uint64 -= s.getInt64();
         break;

    case RT_HEXVAL:
         m_Val.m_Uint64 -= s.getUint64();
         break;

    case RT_BOOL:
         clError::report(ERR_BOOL_SUB);
         break;
         
    case RT_STR:
         clError::report(ERR_STRING_SUB);
         break;

    case RT_FLOAT:
         m_Val.m_Double -= s.getFloat();
         break;
    case RT_COMPLEX:
         m_Val.m_complex.re -= s.getComplexRe();
         m_Val.m_complex.im -= s.getComplexIm();
         break;
    case RT_FORMULA:
    case RT_COMMENT:
         clError::report(ERR_MINUS_NOT_SUPPORTED);
         break;

  }
}


void clAnyType::operator*=(clAnyType m)
{
  // wenn vorzeichenbehaftete Zahl multipliziert wird, Typ in INT wandeln
  if(
      ((m_Type == RT_UINT64) || (m_Type == RT_HEXVAL)) &&
      (m.getType() != RT_UINT64)
    )
    changeType(RT_INT64);
  if(m_Type == RT_FORMULA)
    assignFormulaResult();
  if(m.getType() == RT_FORMULA)
    m.assignFormulaResult();

  adaptType(m);
  switch(m_Type)
  {
    case RT_INT64:
         m_Val.m_Int64 *= m.getInt64();
         break;

    case RT_UINT64:
    case RT_HEXVAL:
         m_Val.m_Uint64 *= m.getUint64();
         break;

    case RT_BOOL:
         m_Val.m_Bool &= m.getBool();
         break;

    case RT_STR:
      clError::report(ERR_STRING_MUL);
         break;

    case RT_FLOAT:
         m_Val.m_Double *= m.getFloat();
         break;

    case RT_COMPLEX: {
         double re = m_Val.m_complex.re * m.getComplexRe() - m_Val.m_complex.im * m.getComplexIm();
         m_Val.m_complex.im = m_Val.m_complex.im * m.getComplexRe() + m_Val.m_complex.re * m.getComplexIm();
         m_Val.m_complex.re = re;
         }
         break;

    case RT_COMMENT:
    case RT_FORMULA:
         clError::report(ERR_MUL_NOT_SUPPORTED);
         break;
  }
}

void clAnyType::operator*=(double m)
{
  clAnyType s;

  // wenn vorzeichenbehaftete Zahl multipliziert wird, Typ in INT wandeln
  if((m_Type == RT_UINT64) || (m_Type == RT_HEXVAL))
    changeType(RT_INT64);
  s = m;
  if(m_Type == RT_FORMULA)
    assignFormulaResult();
  adaptType(s);
  *this *= s;
}


void clAnyType::operator/=(clAnyType d)
{
  // wenn vorzeichenbehaftete Zahl dividiert wird, Typ in INT wandeln
  if(
      ((m_Type == RT_UINT64) || (m_Type == RT_HEXVAL)) &&
      (d.getType() != RT_UINT64)
    )
    changeType(RT_INT64);
  if(m_Type == RT_FORMULA)
    assignFormulaResult();
  if(d.getType() == RT_FORMULA)
    d.assignFormulaResult();
  adaptType(d);
  switch(m_Type)
  {
    case RT_INT64:
         if(d.getInt64() != 0)
           m_Val.m_Int64 /= d.getInt64();
         else
           clError::report(ERR_DIV);
         break;

    case RT_UINT64:
    case RT_HEXVAL:
         if(d.getUint64() != 0)
           m_Val.m_Uint64 /= d.getUint64();
         else
           clError::report(ERR_DIV);
         break;

    case RT_BOOL:
         if (!d.getBool())
           clError::report(ERR_DIV);
         break;

    case RT_STR:
         clError::report(ERR_STRING_DIV);
         break;

    case RT_COMMENT:
    case RT_FORMULA:
         clError::report(ERR_MUL_NOT_SUPPORTED);
         break;

    case RT_FLOAT:
         if(d.getFloat() != 0.0)
           m_Val.m_Double /= d.getFloat();
         else
           clError::report(ERR_DIV);
         break;

    case RT_COMPLEX: {
           double a = m_Val.m_complex.re;
           double b = m_Val.m_complex.im;
           double c = d.getComplexRe();
           double e = d.getComplexIm();
           if((c != 0) || (e != 0)) {
             double re = (a * c + b * e) / (c*c + e*e);
             double im = (b * c - a * e) / (c*c + e*e);
             m_Val.m_complex.re = re;
             m_Val.m_complex.im = im;
           }
           else
             clError::report(ERR_DIV);
         }
         break;
  }
}


void clAnyType::operator|=(clAnyType m)
{
  if(m_Type == RT_FORMULA)
    assignFormulaResult();
  adaptType(m);
  switch(m_Type)
  {
    case RT_INT64:
         m_Val.m_Int64 |= m.getInt64();
         break;

    case RT_UINT64:
    case RT_HEXVAL:
         m_Val.m_Uint64 |= m.getUint64();
         break;

    case RT_BOOL:
         m_Val.m_Bool |= m.getBool();
         break;

    case RT_STR:
         clError::report(ERR_STRING_OR);
         break;

    case RT_COMMENT:
    case RT_FORMULA:
    case RT_COMPLEX:
         clError::report(ERR_OR_NOT_SUPPORTED);
         break;

    case RT_FLOAT:
         clError::report(ERR_FLOAT_OR);
  }
}


void clAnyType::operator&=(clAnyType m)
{
  if(m_Type == RT_FORMULA)
    assignFormulaResult();
  adaptType(m);
  switch(m_Type)
  {
    case RT_INT64:
         m_Val.m_Int64 &= m.getInt64();
         break;

    case RT_UINT64:
    case RT_HEXVAL:
         m_Val.m_Uint64 &= m.getUint64();
         break;

    case RT_BOOL:
         m_Val.m_Bool &= m.getBool();
         break;

    case RT_STR:
         clError::report(ERR_STRING_AND);
         break;

    case RT_FLOAT:
         clError::report(ERR_FLOAT_AND);
         break;

    case RT_COMMENT:
    case RT_FORMULA:
    case RT_COMPLEX:
         clError::report(ERR_AND_NOT_SUPPORTED);
         break;
  }
}


clAnyType clAnyType::operator!()
{
  clAnyType r;
  switch(m_Type)
  {
    case RT_INT64:
         r = ~m_Val.m_Int64;
         break;

    case RT_UINT64:
    case RT_HEXVAL:
         r = ~m_Val.m_Uint64;
         break;

    case RT_BOOL:
         r = !m_Val.m_Bool;
         break;

    case RT_STR:
         clError::report(ERR_STRING_NOT);
         break;

    case RT_COMMENT:
    case RT_FORMULA:
    case RT_COMPLEX:
         clError::report(ERR_NOT_NOT_SUPPORTED);
         break;

    case RT_FLOAT:
         clError::report(ERR_FLOAT_NOT);
  }
  return r;
}

bool clAnyType::operator==(clAnyType m)
{
  bool Result;

  if(m_Type == RT_FORMULA)
    assignFormulaResult();

  adaptType(m);
  switch(m_Type)
  {
    case RT_INT64:
         Result = (m_Val.m_Int64 == m.getInt64());
         break;

    case RT_UINT64:
    case RT_HEXVAL:
         Result = (m_Val.m_Uint64 == m.getUint64());
         break;

    case RT_BOOL:
         Result = (m_Val.m_Bool == m.getBool());
         break;

    case RT_STR:
    case RT_COMMENT:
         Result = (m_String == m.getString());
         break;

    case RT_FLOAT:
         Result = (m_Val.m_Double == m.getFloat());
         break;

    case RT_FORMULA: {
           clAnyType res = getFormulaResult();
           switch(res.getType()) {
             case RT_FLOAT:
               Result = (res.getFloat() == m.getFloat());
               break;
             case RT_COMPLEX:
               Result = ((res.getComplexIm() == m.getComplexIm()) && (res.getComplexRe() == m.getComplexRe()));
               break;
             default:
               clError::report(ERR_NOT_IMPLEMENTED);
               break;

           }
         }
         break;

     case RT_COMPLEX:
         Result = ((m_Val.m_complex.re == m.getComplexRe()) && (m_Val.m_complex.im == m.getComplexIm()));
         break;
   }
  return Result;
}


bool clAnyType::operator!=(clAnyType m)
{
  clAnyType s1;
  bool Result;

  s1.setType(m_Type);
  s1 = m;
  adaptType(s1);
  switch(m_Type)
  {
    case RT_INT64:
         Result = (m_Val.m_Int64 != s1.getInt64());
         break;

    case RT_UINT64:
    case RT_HEXVAL:
         Result = (m_Val.m_Uint64 != s1.getUint64());
         break;

    case RT_BOOL:
         Result = (m_Val.m_Bool != s1.getBool());
         break;

    case RT_STR:
    case RT_COMMENT:
         Result = (m_String != s1.getString());
         break;

    case RT_FLOAT:
         Result = (m_Val.m_Double != s1.getFloat());
         break;

    case RT_COMPLEX:
        Result = ((m_Val.m_complex.re != m.getComplexRe()) || (m_Val.m_complex.im != m.getComplexIm()));
        break;

    case RT_FORMULA:
        clError::report(ERR_UNEQ_NOT_SUPPORTED);
        break;

  }
  return Result;
}


bool clAnyType::operator<(clAnyType m)
{
  bool Result;

  if(m_Type == RT_UINT64) 
    changeType(RT_INT64);
  if(m_Type == RT_FORMULA)
    assignFormulaResult();

  adaptType(m);
  switch(m_Type)
  {
    case RT_INT64:
         Result = (m_Val.m_Int64 < m.getInt64());
         break;

    case RT_UINT64:
    case RT_HEXVAL:
         Result = (m_Val.m_Uint64 < m.getUint64());
         break;

    case RT_BOOL:
         Result = (m_Val.m_Bool < m.getBool());
         break;

    case RT_STR:
    case RT_COMMENT:
         Result = (m_String < m.getString());
         break;

    case RT_FLOAT:
         Result = (m_Val.m_Double < m.getFloat());
         break;
     case RT_COMPLEX:
     case RT_FORMULA:
        clError::report(ERR_LESS_NOT_SUPPORTED);
        break;
  }
  return Result;
}


bool clAnyType::operator<=(clAnyType m)
{
  bool Result;

  if(m_Type == RT_UINT64) 
    changeType(RT_INT64);
  if(m_Type == RT_FORMULA)
    assignFormulaResult();

  adaptType(m);
  switch(m_Type)
  {
    case RT_INT64:
         Result = (m_Val.m_Int64 <= m.getInt64());
         break;

    case RT_UINT64:
    case RT_HEXVAL:
         Result = (m_Val.m_Uint64 <= m.getUint64());
         break;

    case RT_BOOL:
         Result = (m_Val.m_Bool <= m.getBool());
         break;

    case RT_STR:
    case RT_COMMENT:
         Result = (m_String <= m.getString());
         break;

    case RT_FLOAT:
         Result = (m_Val.m_Double <= m.getFloat());
         break;

    case RT_COMPLEX:
    case RT_FORMULA:
       clError::report(ERR_LESSEQ_NOT_SUPPORTED);
       break;
  }
  return Result;
}


bool clAnyType::operator>(clAnyType m)
{
  bool Result;

  if(m_Type == RT_UINT64) 
    changeType(RT_INT64);
  if(m_Type == RT_FORMULA)
    assignFormulaResult();

  adaptType(m);
  switch(m_Type)
  {
    case RT_INT64:
         Result = (m_Val.m_Int64 > m.getInt64());
         break;

    case RT_UINT64:
    case RT_HEXVAL:
         Result = (m_Val.m_Uint64 > m.getUint64());
         break;

    case RT_BOOL:
         Result = (m_Val.m_Bool > m.getBool());
         break;

    case RT_STR:
    case RT_COMMENT:
         Result = (m_String > m.getString());
         break;

    case RT_FLOAT:
         Result = (m_Val.m_Double > m.getFloat());;
         break;

    case RT_COMPLEX:
    case RT_FORMULA:
         clError::report(ERR_GREATER_NOT_SUPPORTED);
         break;
  }
  return Result;
}


bool clAnyType::operator>=(clAnyType m)
{
  bool Result;

  if(m_Type == RT_UINT64) 
    changeType(RT_INT64);
  if(m_Type == RT_FORMULA)
    assignFormulaResult();

  adaptType(m);
  switch(m_Type)
  {
    case RT_INT64:
         Result = (m_Val.m_Int64 >= m.getInt64());
         break;

    case RT_UINT64:
    case RT_HEXVAL:
         Result = (m_Val.m_Uint64 >= m.getUint64());
         break;

    case RT_BOOL:
         Result = (m_Val.m_Bool >= m.getBool());
         break;

    case RT_STR:
         Result = (m_String >= m.getString());
         break;

    case RT_FLOAT:
         Result = (m_Val.m_Double >= m.getFloat());;
        break;

    case RT_COMMENT:
    case RT_COMPLEX:
    case RT_FORMULA:
        clError::report(ERR_GREATER_EQ_NOT_SUPPORTED);
        break;
  }
  return Result;
}


clAnyType clAnyType::getFormulaResult() {
  clAnyType retVal(m_pVarList, m_pMethods);
  clCondParser p(m_pVarList, m_pMethods);
  if(m_String.size() >= 1) {
    std::string str = m_String.substr(1);
    retVal = p.parse(&str);
  }
  return retVal;
}

double clAnyType::getFloat()
{
  return m_Val.m_Double;
}

int64_t clAnyType::getInt64()
{
  return m_Val.m_Int64;
}

uint64_t clAnyType::getUint64()
{
  return m_Val.m_Uint64;
}

bool clAnyType::getBool()
{
  return m_Val.m_Bool;
}

const char* clAnyType::getString()
{
  return m_String.c_str();
}

uint32_t clAnyType::getStrSize()
{
  return static_cast<uint32_t>(m_String.size());
}

tType clAnyType::getType()
{
  return m_Type;
}

double clAnyType::getComplexAbs() {
  double abs = std::sqrt(m_Val.m_complex.re * m_Val.m_complex.re + m_Val.m_complex.im * m_Val.m_complex.im);
  return abs;
}

double clAnyType::getComplexArg() {
  double arg = 0.0;
  double abs = getComplexAbs();
  arg = std::acos(m_Val.m_complex.re / abs);
  if(m_Val.m_complex.im < 0)
    arg *= -1.0;
  if(arg < 0)
    arg += M_PI * 2;

  return arg;
}

const char* clAnyType::getTypeString() {
  switch(m_Type) {
    case RT_INT64:
      return "INT64";
    case RT_UINT64:
      return "UINT64";
    case RT_FLOAT:
      return "FLOAT";
    case RT_FORMULA:
      return "FORMULA";
    case RT_BOOL:
      return "BOOL";
    case RT_COMPLEX:
      return "COMPLEX";
    case RT_COMMENT:
      return "COMMENT";
    case RT_STR:
      return "STRING";
    case RT_HEXVAL:
      return "HEX";
  }
  return "";
}

tType clAnyType::stringToType(const char* typeString) {
  if(strcmp("INT64", typeString) == 0)
    return RT_INT64;
  else if (strcmp("UINT64", typeString) == 0)
    return RT_UINT64;
  else if (strcmp("FLOAT", typeString) == 0)
    return RT_FLOAT;
  else if (strcmp("FORMULA", typeString) == 0)
    return RT_FORMULA;
  else if (strcmp("BOOL", typeString) == 0)
    return RT_BOOL;
  else if (strcmp("COMPLEX", typeString) == 0)
    return RT_COMPLEX;
  else if (strcmp("COMMENT", typeString) == 0)
    return RT_COMMENT;
  else if (strcmp("STRING", typeString) == 0)
    return RT_STR;
  else if (strcmp("HEX", typeString) == 0)
    return RT_HEXVAL;

  return RT_FLOAT;
}

void stripFollowingZeroes(char* pBuf) {
  bool foundPoint = false;
  for(size_t i = 0; i < strlen(pBuf); i++) {
    if(pBuf[i] == '.') {
      foundPoint = true;
      break;
    }
  }
  if(foundPoint) {
    char* p = &pBuf[strlen(pBuf) - 1];
    while((*p == '0') && (p > pBuf)) {
      --p;
    }
    pBuf[p - pBuf + 1] = 0;
  }
}

void clAnyType::doubleToString(std::string& string, double val, uint32_t decimals) {
  char ValStr[128];
  std::string exp;
  std::string strFormat = "%1.";
  char str[10];
  snprintf(str,sizeof(str),"%i", decimals);
  strFormat += str;
  if((std::fabs(val) < 1e-4) || (std::fabs(val) > 1e7)) {
    strFormat += 'e';
    snprintf(ValStr,sizeof(ValStr),strFormat.c_str(),val);
    for(size_t i = 0; i < strlen(ValStr); i++) {
      if((ValStr[i] == 'e') || (ValStr[i] == 'E')){
        exp = &ValStr[i];
        ValStr[i] = 0;
        break;
      }
    }
    stripFollowingZeroes(ValStr);
    string = ValStr;
    string += exp;
  }
  else {
    strFormat += 'f';
    snprintf(ValStr,sizeof(ValStr),strFormat.c_str(),val);
    stripFollowingZeroes(ValStr);
    string = ValStr;
  }
}

void clAnyType::setType(tType Type)
{
  m_Type = Type;
}

void clAnyType::changeType(tType NewType, uint32_t decimals)
{
  char ValStr[80];

  switch(m_Type)
  {
    case RT_COMMENT:
         clError::report(ERR_TYPE_CONVERSION);
         break;

    case RT_BOOL:         
         switch(NewType)
         {
           case RT_BOOL:         
                break;
           case RT_INT64:
                m_Val.m_Int64 = m_Val.m_Bool;
                break;
           case RT_UINT64:
           case RT_HEXVAL:
                m_Val.m_Uint64 = m_Val.m_Bool;
                break;
           case RT_STR:
                if(m_Val.m_Bool)
                  m_String = "TRUE";
                else
                  m_String = "FALSE";
                break;
           case RT_FLOAT:
                m_Val.m_Double = m_Val.m_Bool;
                break;
           case RT_FORMULA:
           case RT_COMPLEX:
           case RT_COMMENT:
                clError::report(ERR_TYPE_CONVERSION);
                break;
         }
         break;

    case RT_INT64:
         switch(NewType)
         {
           case RT_BOOL:         
                m_Val.m_Bool = (m_Val.m_Int64 != 0);
                break;
           case RT_INT64:
                m_Val.m_Int64 = static_cast<int64_t>(m_Val.m_Int64);
                break;
           case RT_UINT64:
           case RT_HEXVAL:
                m_Val.m_Uint64 = static_cast<uint64_t>(m_Val.m_Int64);
                break;
           case RT_STR:
                snprintf(ValStr,sizeof(ValStr),INT64_TOSTR_FMT,m_Val.m_Int64);
                m_String = ValStr;
                break;
           case RT_FLOAT:
                m_Val.m_Double = static_cast<double>(m_Val.m_Int64);
                break;
           case RT_COMPLEX:
                m_Val.m_complex.re = static_cast<double>(m_Val.m_Int64);
                m_Val.m_complex.im = 0.0;
                break;
           case RT_FORMULA:
           case RT_COMMENT:
                clError::report(ERR_TYPE_CONVERSION);
                break;

         }
         break;

    case RT_UINT64:
    case RT_HEXVAL:
         switch(NewType)
         {
           case RT_BOOL:         
                m_Val.m_Bool = (m_Val.m_Uint64 != 0);
                break;
           case RT_INT64:
                m_Val.m_Int64 = static_cast<INT64>(m_Val.m_Uint64); 
                break;
           case RT_UINT64:
           case RT_HEXVAL:
                break;
           case RT_STR:
                if(m_Type == RT_UINT64)
                  snprintf(ValStr,sizeof(ValStr),UINT64_TOSTR_FMT,m_Val.m_Uint64);
                else
                  snprintf(ValStr,sizeof(ValStr),UINT64HEX_TOSTR_FMT,m_Val.m_Uint64);
                m_String = ValStr;
                break;
           case RT_FLOAT:
                m_Val.m_Int64 = static_cast<INT64>(m_Val.m_Uint64);
                m_Val.m_Double = static_cast<double>(m_Val.m_Int64);
                break;

           case RT_COMPLEX:
                m_Val.m_Int64 = static_cast<INT64>(m_Val.m_Uint64);
                m_Val.m_complex.re = static_cast<double>(m_Val.m_Int64);
                m_Val.m_complex.im = 0.0;
                break;
           case RT_COMMENT:
           case RT_FORMULA:
                clError::report(ERR_TYPE_CONVERSION);
                break;

         }
         break;


    case RT_STR:
         switch(NewType)
         {
           case RT_BOOL:
                if(m_String == "TRUE")
                  m_Val.m_Bool = true;
                else
                  m_Val.m_Bool = false;
                break;

           case RT_INT64:
                sscanf(m_String.c_str(),INT64_TOSTR_FMT,&m_Val.m_Int64);
                break;
           case RT_UINT64:
                sscanf(m_String.c_str(),UINT64_TOSTR_FMT,&m_Val.m_Uint64);
                break;
           case RT_HEXVAL:
                sscanf(m_String.c_str(),UINT64HEX_TOSTR_FMT,&m_Val.m_Uint64);
                break;     
           case RT_STR:
                break;
           case RT_FLOAT:
                sscanf(m_String.c_str(),DOUBLE_TOSTR_FMT,&m_Val.m_Double);
               break;
           case RT_COMPLEX:
           case RT_FORMULA:
           case RT_COMMENT:
                clError::report(ERR_TYPE_CONVERSION);
                break;

         }
         break;

    case RT_FLOAT:
         switch(NewType)
         {
           case RT_BOOL:
                m_Val.m_Bool = ((m_Val.m_Double > 0.1) || (m_Val.m_Double < -0.1));
                break;

           case RT_INT64:
                m_Val.m_Int64 = static_cast<int64_t>(m_Val.m_Double);
                break;

           case RT_UINT64:
           case RT_HEXVAL:
                m_Val.m_Uint64 = static_cast<uint64_t>(m_Val.m_Double);
                break;

           case RT_STR:
                doubleToString(m_String, m_Val.m_Double, decimals);
                break;

           case RT_FLOAT:
                break;

           case RT_COMPLEX:
                m_Val.m_complex.re = m_Val.m_Double;
                m_Val.m_complex.im = 0.0;
                break;
           case RT_COMMENT:
           case RT_FORMULA:
                clError::report(ERR_TYPE_CONVERSION);
                break;

         }
         break;

    case RT_FORMULA: {
         clAnyType resu(m_pVarList, m_pMethods);
         resu = getFormulaResult();
         resu.changeType(NewType);
         switch(NewType) {
           case RT_BOOL:
              m_Val.m_Bool = resu.getBool();
              break;

           case RT_INT64:
              m_Val.m_Int64 = resu.getInt64();
              break;

           case RT_UINT64:
           case RT_HEXVAL:
                m_Val.m_Uint64 = resu.getUint64();
                break;
           case RT_FLOAT:
                m_Val.m_Double = resu.getFloat();
                break;
           case RT_STR:
                m_String = resu.getString();
               break;
           case RT_COMPLEX:
                m_Val.m_complex.re = resu.getComplexRe();
                m_Val.m_complex.im = resu.getComplexIm();
                break;

           case RT_COMMENT:
                clError::report(ERR_TYPE_CONVERSION);
                break;
           case RT_FORMULA:
                break;
           }
         }
         break;

    case RT_COMPLEX: {
        std::string val;
        doubleToString(val, m_Val.m_complex.re, decimals);
        m_String = val;
        if(m_Val.m_complex.im >= 0.0)
          m_String += "+";
        doubleToString(val, m_Val.m_complex.im, decimals);
        m_String += val;
        m_String += "i";
      }
      break;
    }

  m_Type = NewType;
  switch(m_Type)
  {
    case RT_STR:
    case RT_COMMENT:
         m_Val.m_Double = 0;
         m_Val.m_complex.im = 0;
         break;
    case RT_BOOL:
    case RT_INT64:
    case RT_UINT64:
    case RT_HEXVAL:
    case RT_FLOAT:
    case RT_COMPLEX:
    case RT_FORMULA:
         m_String = "";
         break;        
  }
}
