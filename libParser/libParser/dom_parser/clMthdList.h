/*********************************************************
 *         project: ProcessMon                           *
 *                                                       *
 *              (C) 2010 Christian Mueller               *
 *                       Odenwaldstrasse 134g            *
 *                       D-64372 Ober-Ramstadt           *
 *                                                       *
 *********************************************************/

#if !defined(IMTHDLIST_H)
#define IMTHDLIST_H


#include "os/typedef.h"
#include "clAnyType.h"

class clVarList;
class iModule;
class clMthdResult;

typedef   int32_t (*tpFunc)(int32_t argc, clAnyType** argv, clAnyType& result);

#define PAR_HELP_CNT    20
#define RET_HELP_CNT    10

struct stHelpTabItem
{
  char* pMthdName;
  char* pMthdHelp;
  char* pParHelp[PAR_HELP_CNT];
  char* pRetHelp[RET_HELP_CNT];
};


struct tFuncTabItem
{
  const char* pName;
  tpFunc pFunc;
};

typedef tPM_vector<tFuncTabItem> tFuncTab;
typedef tFuncTab::iterator tFuncTabIter;


#define ADD_MTHD(txt,pf) do {tFuncTabItem It; It.pName = txt;It.pFunc = pf; addMethod(It);} while(0)

class clMthdList  
{
public:
	clMthdList();
	virtual ~clMthdList();

  // sucht einen Methodennamen in der Liste und gibt einen Iterator
  // auf die Methode zurueck. Wird die Methode nicht gefunden, wird false
  // zurueckgegeben
  bool findMethod(
    // Rückgabe des Iterators, falls einer gefunden wurde
    tFuncTabIter& RetIter,
    // Name der Methode
    tPM_string& MthdName);

  // Initialisiert die Funktionstabelle
  virtual void initMthdTab() = 0;

  // Ueberschrift fuer Hilfe zur Methodenliste
  virtual tPM_Char* getMthdListHelp() = 0;

  // liefert die Tabelle mit detaillierter Hilfe zu den Methoden
  virtual const stHelpTabItem* getHelpTab() = 0;
  // Zeiger auf Result-Objekt setzen
  inline void setpResult(clMthdResult* pResult)
  {
    m_pResult = pResult;
  }

  // Zeiger auf VariablenListe setzen
  void setVarList(clVarList* VarList) {
    m_pVarList = VarList;
  }

  inline tFuncTab* getMthdList()
  {
    return &m_Mthds;
  };

 
protected:
  void addMethod(tFuncTabItem Item)
  {
    m_Mthds.push_back(Item);
  }

  // Liste der Methoden
  tFuncTab m_Mthds;
  // Zeiger auf Ergbenis
  clMthdResult* m_pResult;

  clVarList* m_pVarList;
};

#endif // !defined(AFX_CLCOMMAND_H__E93CFC11_5620_4A11_8A41_ADC5A7D61337__INCLUDED_)
