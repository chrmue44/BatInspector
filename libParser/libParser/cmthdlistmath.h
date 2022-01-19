#ifndef CMTHDLISTMATH_H
#define CMTHDLISTMATH_H

#include "dom_parser/clMthdList.h"

#define FILENAME_VARS "vars.csv"

class CMthdListMath : public clMthdList
{
public:
  CMthdListMath();
  virtual ~CMthdListMath();

  // Initialisiert die Funktionstabelle
  virtual void initMthdTab();

  // Ueberschrift fuer Hilfe zur Methodenliste
  virtual tPM_Char* getMthdListHelp();

  // liefert die Tabelle mit detaillierter Hilfe zu den Methoden
  virtual const stHelpTabItem* getHelpTab();

  void setMethods(clMethods* pMethods) {
   m_pMethods = pMethods;
  }

 /* inline void setInst(CMthdListMath* pInst)
  {
    m_Inst = pInst;
  }*/

private:
  static int32_t sqrt(int32_t argcnt, clAnyType** argv, clAnyType& result);
  static int32_t sin(int32_t argcnt, clAnyType** argv, clAnyType& result);
  static int32_t asin(int32_t argcnt, clAnyType** argv, clAnyType& result);
  static int32_t sinh(int32_t argcnt, clAnyType** argv, clAnyType& result);
  static int32_t cos(int32_t argcnt, clAnyType** argv, clAnyType& result);
  static int32_t acos(int32_t argcnt, clAnyType** argv, clAnyType& result);
  static int32_t cosh(int32_t argcnt, clAnyType** argv, clAnyType& result);
  static int32_t tan(int32_t argcnt, clAnyType** argv, clAnyType& result);
  static int32_t atan(int32_t argcnt, clAnyType** argv, clAnyType& result);
  static int32_t tanh(int32_t argcnt, clAnyType** argv, clAnyType& result);
  static int32_t pow(int32_t argcnt, clAnyType** argv, clAnyType& result);
  static int32_t ln(int32_t argcnt, clAnyType** argv, clAnyType& result);
  static int32_t exp(int32_t argcnt, clAnyType** argv, clAnyType& result);
  static int32_t plot(int32_t argcnt, clAnyType** argv, clAnyType& result);
  static int32_t vars(int32_t argcnt, clAnyType** argv, clAnyType& result);
  static int32_t consts(int32_t argcnt, clAnyType **argv, clAnyType& result);
  static int32_t abs(int32_t argcnt, clAnyType** argv, clAnyType& result);
  static int32_t arg(int32_t argcnt, clAnyType** argv, clAnyType& result);
  static int32_t cast(int32_t argcnt, clAnyType** argv, clAnyType& result);
  static int32_t unittest(int32_t argcnt, clAnyType** argv, clAnyType& result);
  static int32_t savevars(int32_t argcnt, clAnyType** argv, clAnyType& result);
  static int32_t loadvars(int32_t argcnt, clAnyType** argv, clAnyType& result);

  clMethods* m_pMethods;
  static CMthdListMath* m_Inst;
};
#endif // CMTHDLISTMATH_H
