/*********************************************************
 *         project: ProcessMon                           *
 *                                                       *
 *              (C) 2010 Christian Mueller               *
 *                       Odenwaldstrasse 134g            *
 *                       D-64372 Ober-Ramstadt           *
 *                                                       *
 *********************************************************/

#if !defined(AFX_CLMETHODS_H__D92245CF_1B9D_4EA0_A394_AAFC26189B3D__INCLUDED_)
#define AFX_CLMETHODS_H__D92245CF_1B9D_4EA0_A394_AAFC26189B3D__INCLUDED_

#include "clAnyType.h"
#include "os/objhndlglib.h"

class clMthdList;
class clMthdResult;

typedef tPM_vector<clMthdList*> tMthdListList;
typedef tMthdListList::iterator tMthdListListIter;

// haelt eine Liste aus Mehtodenlisten
// beim Aufruf von executeFunction() werden alle eingetragenen Listen
// durchsucht, bis die Funktion gefunden wurde

class clMethods  
{
public:
	explicit clMethods(clMthdResult* pResult);
	virtual ~clMethods();
  // Funktion ausfuehren
  int32_t executeFunction(
    // Name der Funktion
    tPM_string& FuncName, 
    // Anzahl der Parameter
    uint32_t ParCnt,
    // Liste der Parameter
    clAnyType** argv,
    clAnyType& result);

  // fuegt eine Liste mit Methoden hinzu
  void addMethodList(
    // Liste von Methoden
    clMthdList* pMthdList);

private:
  // Liste aus Zeigern auf Methodenlisten
  tMthdListList m_List;

  // Hilfe-Funktion
  void help(
    // Anzahl der Parameter
    uint32_t ParCnt,
    // Liste der Parameter
    clAnyType** argv);

  // Zeiger auf Antwort
  clMthdResult* m_pResult;
};

#endif // !defined(AFX_CLMETHODS_H__D92245CF_1B9D_4EA0_A394_AAFC26189B3D__INCLUDED_)
